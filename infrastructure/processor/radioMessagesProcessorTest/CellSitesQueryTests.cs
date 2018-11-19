using radioMessagesProcessor.Services;
using System;
using System.Linq;
using Xunit;

namespace radioMessagesProcessorTest
{
    public class CellSitesQueryTests
    {
        [Fact]
        public void TestGetCell()
        {
            var subject = new CellsitesQueryService(new RadioMessagesProcessor.Helpers.AppSettings { SolrCellSitesServer = "192.168.1.8:8983",
                SolrCellSitesServerCore = "cellsites" });
            var cellSite = subject.GetCellSiteAsync(new RadioMessagesProcessor.Dtos.CellInfoDto() { Radio = "lte", Mcc="302", Mnc="490", Cid= "102715167", Lac="30020",
                PscPci ="441" }).Result;
            Assert.NotNull(cellSite);
        }

        [Fact]
        public void TestGetCellNeighbours()
        {
            var subject = new CellsitesQueryService(new RadioMessagesProcessor.Helpers.AppSettings
            {
                SolrCellSitesServer = "192.168.1.8:8983",
                SolrCellSitesServerCore = "cellsites"
            });

            var cellInfoDto = new RadioMessagesProcessor.Dtos.CellInfoDto()
            {
                Radio = "lte",
                Mcc = "302",
                Mnc = "490",
                Cid = "102715167",
                Lac = "30020",
                PscPci = "441"
            };

            var cellSite = subject.GetCellSiteAsync(cellInfoDto).Result;
            Assert.NotNull(cellSite);

            var neighbours = subject.GetNeighboursAsync(double.Parse(cellSite.lat), double.Parse(cellSite.lon), 1.44, cellInfoDto).Result;

            Assert.Equal(44, neighbours.Count());
        }
    }
}
