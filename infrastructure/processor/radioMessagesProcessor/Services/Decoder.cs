using AutoMapper;
using LocationData.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace radioMessagesProcessor.Services
{
    public interface IDecoder
    {
        /// <summary>
        /// Creates the message from the raw event
        /// </summary>
        /// <param name="rawEvent"></param>
        /// <param name="isSuccessful">true when the creation succeeds with no errors.</param>
        /// <param name="error_message">error message encountered whenever the raw event cannot be parsed</param>
        /// <returns></returns>
        RadioLocationMessageDto FromRawEvent(string rawEvent, out bool isSuccessful, out string error_message);

        /// <summary>
        /// Attempts to decode the location of the neighbour towers and the approximate device location using triangulation
        /// </summary>
        /// <param name="radioLocation">the undecoded <see cref="RadioLocationMessageDto"/> message</param>
        /// <param name="error_message">errors encounters during decoding</param>
        /// <returns>true if succesfully decoded</returns>
        Task<DecodeResult> DecodeAsync(RadioLocationMessageDto radioLocation);
    }

    public class DecodeResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class Decoder : IDecoder
    {
        private IMapper mapper;
        private ICellsitesQueryService cellsitesQueryService;

        public Decoder(ICellsitesQueryService cellsitesQueryService, IMapper mapper)
        {
            this.mapper = mapper;
            this.cellsitesQueryService = cellsitesQueryService;
        }


        public async Task<DecodeResult> DecodeAsync(RadioLocationMessageDto radioLocation)
        {
            // populate the gps locations
            // main cell is either the registered one or the one with mcc/mnc/lac/cid populated
            var mainCell = radioLocation.Cells.FirstOrDefault(c => c.IsReg) ??
                radioLocation.Cells.FirstOrDefault(c => !(c.Mnc == "-1") &&
                    !(c.Mnc == "-1") &&
                    !(c.Lac == "-1") &&
                    !(c.Cid == "-1"));
            if (mainCell == null || mainCell.Mcc == "0" || mainCell.Mnc == "0")
            {
                return new DecodeResult
                {
                    Success = false,
                    ErrorMessage = "not enought information to decode the radio message."
                };
            }

            mainCell.IsMain = true;

            var solrCell = await this.cellsitesQueryService.GetCellSiteAsync(mainCell);
            if (solrCell != null)
            {
                Populate(solrCell, mainCell);
                mainCell.IsDecoded = true;


                //at least one cell has PscPci so we should get the geographical neighbours
                if (radioLocation.CellsExcept(mainCell).Any(c => !string.IsNullOrEmpty(c.PscPci)))
                {
                    //var neighbours = await this.cellsitesQueryService.GetNeighboursAsync(5, mainCell).ConfigureAwait(false);
                    var cellsWithPscPciExceptMainCell = radioLocation.CellsExcept(mainCell).Where(c => !string.IsNullOrEmpty(c.PscPci));
                    var neighbours = await this.cellsitesQueryService
                        .GetNeighboursForUnitsAsync(35, mainCell, cellsWithPscPciExceptMainCell.Select(c => c.PscPci))
                        .ConfigureAwait(false);

                    foreach (var cell in cellsWithPscPciExceptMainCell)
                    {
                        var identifyCell = this.cellsitesQueryService.ResolveCell(cell, mainCell, neighbours.ToList());
                        if (identifyCell != null)
                        {
                            Populate(identifyCell, cell);
                            cell.IsDecoded = true;
                        }
                    }
                }

                var radioIntersection = Mapping.Radio.RadioIntersection.GenerateRadioIntersection(
                    radioLocation
                    .Cells
                    .Where(c => c.IsDecoded)
                    .Select(c => new Mapping.Radio.RadioInfoGps(c.Radio, c.Rssi, c.Longitude, c.Latitude)).ToList());

                var center = Mapping.Radio.RadioIntersection.CenterOfMass(
                        radioIntersection.Intersection,
                        radioIntersection.Level,
                        radioIntersection.TranslationX,
                        radioIntersection.TranslationY);

                radioLocation.Rssi = mainCell.Rssi;
                radioLocation.DecodedLatitude = center.Latitude;
                radioLocation.DecodedLongitude = center.Longitude;
                radioLocation.DecodedDateUTC = DateTime.UtcNow;

                radioLocation.RadioShapes = radioIntersection;

                return new DecodeResult
                {
                    Success = true
                };
            }

            return new DecodeResult
            {
                Success = false,
                ErrorMessage = "cannot find the main cell in the database."
            };
        }

        private void Populate(CellSiteSolr from, CellInfoDto to)
        {
            if (to.Mnc == "-1")
                to.Mnc = from.net;
            if (to.Mcc == "-1")
                to.Mcc = from.mcc;
            if (to.Lac == "-1")
                to.Lac = from.area;
            if (to.Cid == "-1")
                to.Cid = from.cell;
            if (to.PscPci == "-1")
                to.PscPci = from.unit;

            to.Latitude = Double.Parse(from.lat);
            to.Longitude = Double.Parse(from.lon);
            to.Created = DateTimeOffset.FromUnixTimeSeconds(from.created.FirstOrDefault()).DateTime;
            to.Updated = DateTimeOffset.FromUnixTimeSeconds(from.updated).DateTime;
            to.Range = from.range;
            to.Samples = from.samples;
        }



        public RadioLocationMessageDto FromRawEvent(string rawEvent, out bool isSuccessful, out string error_message)
        {
            var lines = rawEvent.Split("\n");
            if (!lines[0].Equals("#collectionDateTime"))
            {
                isSuccessful = false;
                error_message = "missing <<#collectionDateTime>> line";
                return null;
            }

            var result = new RadioLocationMessageDto()
            {
                RawEventString = rawEvent,
                CollectionDateUTC = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(lines[1])).DateTime
            };

            if (!lines[2].Equals("#deviceId,deviceTime"))
            {
                isSuccessful = false;
                error_message = "missing <<#deviceId,deviceTime>> line";
                return null;
            }

            var deviceIdTime = lines[3].Trim().Split(",");
            if (deviceIdTime.Count() != 2)
            {
                isSuccessful = false;
                error_message = "wrong format for <<#deviceId,deviceTime>> line";
                return null;
            }

            result.Imei = deviceIdTime[0];
            result.DeviceDate = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(deviceIdTime[1])).DateTime;

            if (!lines[4].Equals("#latitude,longitude,age,accuracy,speed,bearing"))
            {
                isSuccessful = false;
                error_message = "missing <<#latitude,longitude,age,accuracy,speed,bearing>> line";
                return null;
            }

            var gpsLine = lines[5].Trim().Split(",");
            if (gpsLine.Count() != 6)
            {
                isSuccessful = false;
                error_message = "wrong format for <<#latitude,longitude,age,accuracy,speed,bearing>> line";
                return null;
            }

            //# latitude,longitude,age,accuracy,speed,bearing
            // 45.277281,-75.925078,1156,18.224,0.0,?
            if (double.TryParse(gpsLine[0], out double latitude))
            {
                result.GpsLatitude = latitude;
            }

            if (double.TryParse(gpsLine[1], out double longitude))
            {
                result.GpsLongitude = longitude;
            }

            if (long.TryParse(gpsLine[2], out long agems))
            {
                result.GpsAge = agems;
            }

            if (double.TryParse(gpsLine[3], out double accuracy))
            {
                result.GpsAccuracy = accuracy;
            }

            if (double.TryParse(gpsLine[4], out double speed))
            {
                result.GpsSpeed = speed;
            }

            if (double.TryParse(gpsLine[5], out double bearing))
            {
                result.GpsBearing = bearing;
            }

            if (!lines[6].Equals("#Radio,Mcc,Mnc,Cid,Lac,SignalS,Level,mAsu,mTa,PscPci,isReg"))
            {
                isSuccessful = false;
                error_message = "missing <<#Radio,Mcc,Mnc,Cid,Lac,SignalS,Level,mAsu,mTa,PscPci,isReg>> line";
                return null;
            }

            for (int i = 7; i < lines.Length; i++)
            {
                //# Radio,Mcc,Mnc,Cid,Lac,SignalS,Level,mAsu,mTa,PscPci,isReg
                // wcdma,302,490,1323033,20,-101,-1,6,-1,400,1
                // wcdma,-1,-1,-1,-1,-99,-1,7,-1,167,0
                var cellinfoLine = lines[i].Split(',');
                if (cellinfoLine.Length >= 10)
                {
                    RadioCellInfoDto radioCellInfoDto = RadioCellInfoDto.Parse(cellinfoLine);
                    result.AddCell(mapper.Map<CellInfoDto>(radioCellInfoDto));
                    // - todo when things don't go smooth
                }
                else
                {
                    // - todo when things don't go smooth
                }
            }

            isSuccessful = true;
            error_message = "";
            return result;
        }
    }
}
