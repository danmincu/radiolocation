using AutoMapper;
using radioMessagesProcessor.Helpers;
using RadioMessagesProcessor.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        bool Decode(RadioLocationMessageDto radioLocation, out string error_message);
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

        
        public bool Decode(RadioLocationMessageDto radioLocation, out string error_message)
        {
            throw new NotImplementedException();
        }

        
        public RadioLocationMessageDto FromRawEvent(string rawEvent, out bool isSuccessful, out string error_message)
        {
            var lines = rawEvent.Split("\n");
            if (!lines[0].Equals("#deviceId,deviceTime"))
            {
                isSuccessful = false;
                error_message = "missing <<#deviceId,deviceTime>> line";
                return null;
            }

            var result = new RadioLocationMessageDto();

            var line1split = lines[1].Trim().Split(",");
            if (line1split.Count() != 2)
            {
                isSuccessful = false;
                error_message = "wrong format for <<#deviceId,deviceTime>> line";
                return null;
            }

            result.Imei = line1split[0];
            result.DeviceDate = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(line1split[1])).DateTime;

            if (!lines[2].Equals("#latitude,longitude,age,accuracy,speed,bearing"))
            {
                isSuccessful = false;
                error_message = "missing <<#latitude,longitude,age,accuracy,speed,bearing>> line";
                return null;
            }

            var line3split = lines[3].Trim().Split(",");
            if (line3split.Count() != 6)
            {
                isSuccessful = false;
                error_message = "wrong format for <<#latitude,longitude,age,accuracy,speed,bearing>> line";
                return null;
            }
            
            //# latitude,longitude,age,accuracy,speed,bearing
            // 45.277281,-75.925078,1156,18.224,0.0,?
            result.CollectionDateUTC = DateTime.UtcNow;
            if (double.TryParse(line3split[0], out double latitude))
            {
                result.GpsLatitude = latitude;
            }

            if (double.TryParse(line3split[1], out double longitude))
            {
                result.GpsLongitude = longitude;
            }

            if (long.TryParse(line3split[2], out long agems))
            {
                result.GpsAge = agems;
            }

            if (double.TryParse(line3split[3], out double accuracy))
            {
                result.GpsAccuracy = accuracy;
            }

            if (double.TryParse(line3split[4], out double speed))
            {
                result.GpsSpeed = speed;
            }

            if (double.TryParse(line3split[5], out double bearing))
            {
                result.GpsBearing = bearing;
            }

            if (!lines[4].Equals("#Radio,Mcc,Mnc,Cid,Lac,SignalS,Level,mAsu,mTa,PscPci,isReg"))
            {
                isSuccessful = false;
                error_message = "missing <<#Radio,Mcc,Mnc,Cid,Lac,SignalS,Level,mAsu,mTa,PscPci,isReg>> line";
                return null;
            }

            for (int i = 5; i < lines.Length; i++)
            {
                //# Radio,Mcc,Mnc,Cid,Lac,SignalS,Level,mAsu,mTa,PscPci,isReg
                // wcdma,302,490,1323033,20,-101,-1,6,-1,400,1
                // wcdma,-1,-1,-1,-1,-99,-1,7,-1,167,0
                var cellinfoLine = lines[i].Split(',');
                if (cellinfoLine.Length == 11)
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
            result.RawEvent = ZipUnzip.Zip(rawEvent);
            return result;
        }
    }
}
