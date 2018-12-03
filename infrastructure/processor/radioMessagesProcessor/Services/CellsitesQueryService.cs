using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RadioMessagesProcessor.Dtos;
using RadioMessagesProcessor.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace radioMessagesProcessor.Services
{
    public interface ICellsitesQueryService
    {
        Task<CellSiteSolr> GetCellSiteAsync(CellInfoDto ci);

        Task<IEnumerable<CellSiteSolr>> GetNeighboursAsync(double distance, CellInfoDto except);

        Task<IEnumerable<CellSiteSolr>> GetNeighboursForUnitsAsync(double distance, CellInfoDto except, IEnumerable<string> units);

        Task<IEnumerable<CellSiteSolr>> GetNeighboursAsync(double latitude, double longitude, double distance, CellInfoDto except);

        CellSiteSolr ResolveCell(CellInfoDto cellInfoDto, CellInfoDto mainCell, IEnumerable<CellSiteSolr> neighbours);

    }

    public class CellsitesQueryService : ICellsitesQueryService
    {
        // http://192.168.1.8:8983/solr/cellsites/select?&q=*:*&fq={!geofilt%20sfield=location}&pt=45.2767978,-75.9120855&d=1
        // http://192.168.1.8:8983/solr/cellsites/select?fq=area:30020&fq=cell:102715167&fq=mcc:302&fq=net:490&fq=radio:LTE&q=*:*
        private HttpClient client;
        private ILogger logger;
        // "UMTS",8947749,
        //"LTE",8641236,
        //"GSM",3546674]},

        private AppSettings appSettings;
        public CellsitesQueryService(IServiceProvider serviceProvider, IOptions<AppSettings> appSettingsProvider) :
            this(serviceProvider, appSettingsProvider.Value)
        { }

        public CellsitesQueryService(IServiceProvider serviceProvider, AppSettings appSettings)
        {
            this.appSettings = appSettings;
            this.client = new HttpClient();
            this.client.DefaultRequestHeaders.Accept.Clear();
            this.client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            this.logger = serviceProvider?.GetService<ILoggerFactory>().CreateLogger<CellsitesQueryService>();
        }


        private string ToSolrRadio(string radio)
        {
            if (string.IsNullOrEmpty(radio))
            {
                return "";
            }

            if (radio.Equals("lte", StringComparison.OrdinalIgnoreCase))
            {
                return "LTE";
            }
            else
            {
                if (radio.Equals("gsm", StringComparison.OrdinalIgnoreCase))
                {
                    return "GSM";
                }
                else
                    return "UMTS";
            }
        }

        public async Task<CellSiteSolr> GetCellSiteAsync(CellInfoDto ci)
        {
            var queryText = $"http://{this.appSettings.SolrCellSitesServer}/solr/{this.appSettings.SolrCellSitesServerCore}" +
                $"/select?fq=area:{ci.Lac}&fq=cell:{ci.Cid}&fq=mcc:{ci.Mcc}&fq=net:{ci.Mnc}&fq=radio:{this.ToSolrRadio(ci.Radio)}&q=*:*";
            if (string.IsNullOrEmpty(ci.Radio))
            {
                queryText = $"http://{this.appSettings.SolrCellSitesServer}/solr/{this.appSettings.SolrCellSitesServerCore}" +
                    $"/select?fq=area:{ci.Lac}&fq=cell:{ci.Cid}&fq=mcc:{ci.Mcc}&fq=net:{ci.Mnc}&q=*:*";
            }

            var querySolTask = this.client.GetStringAsync(queryText);

            try
            {
                var msg = await querySolTask.ConfigureAwait(false);
                var solrResponse = JsonConvert.DeserializeObject<CellSitesQuery>(msg);
                return solrResponse.response.docs.FirstOrDefault();
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Error getting cell site from Solr database. Query:{queryText}/nException:{ex}");
                throw ex;
            }
        }

        public async Task<IEnumerable<CellSiteSolr>> GetNeighboursAsync(double distance, CellInfoDto except)
        {
            var geofitRequest = $"http://{this.appSettings.SolrCellSitesServer}/solr/{this.appSettings.SolrCellSitesServerCore}" +
            $"/select??&q=*:*&fq={{!geofilt%20sfield=location}}&pt={except.Latitude},{except.Longitude}&d={distance}&rows=800";
            try
            {
                var querySolTask = this.client.GetStringAsync(geofitRequest);
                var msg = await querySolTask.ConfigureAwait(false);
                var solrResponse = JsonConvert.DeserializeObject<CellSitesQuery>(msg);
                return solrResponse.response.docs.Where(c => !(c.mcc.Equals(except.Mcc, StringComparison.OrdinalIgnoreCase) &&
                        c.net.Equals(except.Mnc, StringComparison.OrdinalIgnoreCase) &&
                        c.cell.Equals(except.Cid, StringComparison.OrdinalIgnoreCase) &&
                        c.area.Equals(except.Lac, StringComparison.OrdinalIgnoreCase)) &&
                        (c.unit == null || except.PscPci == null ||
                        c.unit.Equals(except.PscPci, StringComparison.OrdinalIgnoreCase)));
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Error getting cell site neighbours from Solr database. Query:{geofitRequest}/nException:{ex}");
                throw ex;
            }

        }

        public async Task<IEnumerable<CellSiteSolr>> GetNeighboursForUnitsAsync(double distance, CellInfoDto except, IEnumerable<string> units)
        {
            if (!units.Any())
            {
                return Enumerable.Empty<CellSiteSolr>();
            }

            var unitsQuery = string.Join("%20OR%20", units.Select(u => $"unit:{u}"));
            var geofitRequest = $"http://{this.appSettings.SolrCellSitesServer}/solr/{this.appSettings.SolrCellSitesServerCore}" +
            $"/select??&q=*:*&fq={unitsQuery}&fq={{!geofilt%20sfield=location}}&pt={except.Latitude},{except.Longitude}&d={distance}&rows=800";

            try
            {
                var querySolTask = this.client.GetStringAsync(geofitRequest);
                var msg = await querySolTask.ConfigureAwait(false);
                var solrResponse = JsonConvert.DeserializeObject<CellSitesQuery>(msg);
                return solrResponse.response.docs.Where(c => !(c.mcc.Equals(except.Mcc, StringComparison.OrdinalIgnoreCase) &&
                        c.net.Equals(except.Mnc, StringComparison.OrdinalIgnoreCase) &&
                        c.cell.Equals(except.Cid, StringComparison.OrdinalIgnoreCase) &&
                        c.area.Equals(except.Lac, StringComparison.OrdinalIgnoreCase) &&
                        (c.unit == null || except.PscPci == null ||
                        c.unit.Equals(except.PscPci, StringComparison.OrdinalIgnoreCase))));
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Error getting cell site neighbours for units from Solr database. Query:{geofitRequest}/nException:{ex}");
                throw ex;
            }

        }

        public async Task<IEnumerable<CellSiteSolr>> GetNeighboursAsync(double latitude, double longitude, double distance, CellInfoDto except)
        {
            var geofitRequest = $"http://{this.appSettings.SolrCellSitesServer}/solr/{this.appSettings.SolrCellSitesServerCore}" +
                $"/select??&q=*:*&fq={{!geofilt%20sfield=location}}&pt={latitude},{longitude}&d={distance}&rows=500";
            try
            {
                var querySolTask = this.client.GetStringAsync(geofitRequest);
                var msg = await querySolTask.ConfigureAwait(false);
                var solrResponse = JsonConvert.DeserializeObject<CellSitesQuery>(msg);
                return solrResponse.response.docs.Where(c => !(c.mcc.Equals(except.Mcc, StringComparison.OrdinalIgnoreCase) &&
                        c.net.Equals(except.Mnc, StringComparison.OrdinalIgnoreCase) &&
                        c.cell.Equals(except.Cid, StringComparison.OrdinalIgnoreCase) &&
                        c.area.Equals(except.Lac, StringComparison.OrdinalIgnoreCase)));
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Error getting cell site neighbours for units from Solr database. Query:{geofitRequest}/nException:{ex}");
                throw ex;
            }
        }

        public CellSiteSolr ResolveCell(CellInfoDto cell, CellInfoDto mainCell, IEnumerable<CellSiteSolr> neighbours)
        {
            var matchingPscCells = neighbours
                .Where(c => c.radio == this.ToSolrRadio(cell.Radio))
                .Where(c => double.TryParse(c.lat, out double lat) && double.TryParse(c.lon, out double lon))
                .Where(c => c.unit?.Equals(cell.PscPci, StringComparison.OrdinalIgnoreCase) == true);

            if (!matchingPscCells.Any())
            {
                this.logger?.LogWarning($"PscPci {cell.PscPci} radio {cell.Radio} in {mainCell.Latitude}-{mainCell.Longitude} cannot be resolved.");
                return null;
            }

            /* not relevant
            var count = matchingPscCells?.Count();
            if (count > 1)
            {
                this.logger?.LogWarning($"PscPci {cell.PscPci} radio {cell.Radio} in {mainCell.Latitude}-{mainCell.Longitude} resolved to {count} matching neighbours");
            }*/

            // same network as main cell takes priority
            var matchingPscCellsWithPriority = matchingPscCells.Where(c => c.mcc == mainCell.Mcc && c.net == mainCell.Mnc);
            matchingPscCells = matchingPscCellsWithPriority.Any() ? matchingPscCellsWithPriority : matchingPscCells;

            var stepDistanceInMeters = 300;

            // order by distance (nearest km) and samples
            var orderedMatchingPscCells = matchingPscCells
                .OrderBy(c => DistanceSteps(c, mainCell, stepDistanceInMeters))
                .OrderByDescending(c => c.samples);

            // to do - this is tricky how to choose the right one, double check what I am doing here
            if (orderedMatchingPscCells.Count() > 1)
            {
                // create steps rouded to few  hundreed meters
                var steps = orderedMatchingPscCells.Select(mc => new Tuple<int, CellSiteSolr>(DistanceSteps(mc, mainCell, 1), mc));
                var rssi = Mapping.Radio.RssiRanges.GetNearesRssi(cell.Radio, cell.Rssi);
                var lowerDistanceRange = rssi[1] - rssi[2];
                // return the first tower to be able to have such rssi on the lower end
                return steps.FirstOrDefault(s => s.Item1 < lowerDistanceRange)?.Item2 ?? steps.First().Item2;
            }
            else
                return orderedMatchingPscCells.FirstOrDefault();
        }

        private int DistanceSteps(CellSiteSolr solrCell, CellInfoDto cell2, int distanceStepMeters = 300)
        {
            return (int)Math.Truncate((Mapping
                .CoordinateTransformations
                .Distance(new Mapping.Mapping.Coordinate(double.Parse(solrCell.lat), double.Parse(solrCell.lon)),
                  new Mapping.Mapping.Coordinate(cell2.Latitude, cell2.Longitude)) * 1000) / distanceStepMeters);
        }
    }
}
