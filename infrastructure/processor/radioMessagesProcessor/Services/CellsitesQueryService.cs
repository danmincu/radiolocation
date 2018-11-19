using Newtonsoft.Json;
using RadioMessagesProcessor.Dtos;
using RadioMessagesProcessor.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace radioMessagesProcessor.Services
{
    public interface ICellsitesQueryService
    {
        Task<CellSiteSolr> GetCellSiteAsync(CellInfoDto ci);
        Task<IEnumerable<CellSiteSolr>> GetNeighboursAsync(double latitude, double longitude, double distance, CellInfoDto except);
    }

    public class CellsitesQueryService : ICellsitesQueryService
    {
        // http://192.168.1.8:8983/solr/cellsites/select?&q=*:*&fq={!geofilt%20sfield=location}&pt=45.2767978,-75.9120855&d=1
        // http://192.168.1.8:8983/solr/cellsites/select?fq=area:30020&fq=cell:102715167&fq=mcc:302&fq=net:490&fq=radio:LTE&q=*:*
        private HttpClient client;

        // "UMTS",8947749,
        //"LTE",8641236,
        //"GSM",3546674]},

        private AppSettings appSettings;
        public CellsitesQueryService(AppSettings appSettings)
        {
            this.appSettings = appSettings;
            this.client = new HttpClient();
            this.client.DefaultRequestHeaders.Accept.Clear();
            this.client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        }

        private string ToSolrRadio(string radio)
        {
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
            var querySolTask = this.client.GetStringAsync($"http://{this.appSettings.SolrCellSitesServer}/solr/{this.appSettings.SolrCellSitesServerCore}" +
                $"/select?fq=area:{ci.Lac}&fq=cell:{ci.Cid}&fq=mcc:{ci.Mcc}&fq=net:{ci.Mnc}&fq=radio:{this.ToSolrRadio(ci.Radio)}&q=*:*");
            var msg = await querySolTask.ConfigureAwait(false);
            var solrResponse = JsonConvert.DeserializeObject<CellSitesQuery>(msg);
            return solrResponse.response.docs.FirstOrDefault();
        }

        public async Task<IEnumerable<CellSiteSolr>> GetNeighboursAsync(double latitude, double longitude, double distance, CellInfoDto except)
        {
            var geofitRequest = $"http://{this.appSettings.SolrCellSitesServer}/solr/{this.appSettings.SolrCellSitesServerCore}" +
                $"/select??&q=*:*&fq={{!geofilt%20sfield=location}}&pt={latitude},{longitude}&d={distance}&rows=500";
            var querySolTask = this.client.GetStringAsync(geofitRequest);
            var msg = await querySolTask.ConfigureAwait(false);
            var solrResponse = JsonConvert.DeserializeObject<CellSitesQuery>(msg);
            return solrResponse.response.docs.Where(c => !(c.mcc.Equals(except.Mcc, StringComparison.OrdinalIgnoreCase) &&
                    c.net.Equals(except.Mnc, StringComparison.OrdinalIgnoreCase) &&
                    c.cell.Equals(except.Cid, StringComparison.OrdinalIgnoreCase) && 
                    c.area.Equals(except.Lac, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
