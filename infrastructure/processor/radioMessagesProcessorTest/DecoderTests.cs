using AutoMapper;
using radioMessagesProcessor.Services;
using RadioMessagesProcessor.Helpers;
using System;
using System.Linq;
using Xunit;

namespace radioMessagesProcessorTest
{
    public class DecoderTests
    {
        [Fact]
        public void DecodeRawEventTest()
        {
            string rawEvent = @"#deviceId,deviceTime
358511080402476,1542594634630
#latitude,longitude,age,accuracy,speed,bearing
45.277281,-75.925078,1156,18.224,0.0,?
#Radio,Mcc,Mnc,Cid,Lac,SignalS,Level,mAsu,mTa,PscPci,isReg
wcdma,302,490,1323033,20,-101,-1,6,-1,400,1
wcdma,-1,-1,-1,-1,-99,-1,7,-1,167,0
wcdma,-1,-1,-1,-1,-101,-1,6,-1,370,0
wcdma,-1,-1,-1,-1,-111,-1,1,-1,154,0
wcdma,-1,-1,-1,-1,-111,-1,1,-1,284,0".Replace("\r", "");

            var subject = new Decoder(null, new MapperConfiguration(
                c =>
                {
                    c.AddProfile<AutoMapperProfile>();
                }).CreateMapper());

            var locationEvent = subject.FromRawEvent(rawEvent, out bool isSuccessful, out string error_message);

            Assert.True(isSuccessful);
            Assert.Empty(error_message);
            Assert.Equal("358511080402476", locationEvent.Imei);
            Assert.Equal(DateTime.Parse("2018-11-19 2:30:34.630 AM"), locationEvent.DeviceDate);

            Assert.Equal(45.277281, locationEvent.GpsLatitude);
            Assert.Equal(-75.925078, locationEvent.GpsLongitude);
            Assert.Equal(1156, locationEvent.GpsAge);
            Assert.Equal(18.224, locationEvent.GpsAccuracy);
            Assert.Equal(0.0, locationEvent.GpsSpeed);
            Assert.Null(locationEvent.GpsBearing);

            Assert.True(locationEvent.RawEvent.Length > 10);

            Assert.Equal(5, locationEvent.Cells.Count());
            var firstCell = locationEvent.Cells.FirstOrDefault();

            //# Radio,Mcc,Mnc,Cid,Lac,SignalS,Level,mAsu,mTa,PscPci,isReg
            // wcdma,302,490,1323033,20,-101,-1,6,-1,400,1
            Assert.NotNull(firstCell);
            Assert.Equal("wcdma", firstCell.Radio);
            Assert.Equal("302", firstCell.Mcc);
            Assert.Equal("490", firstCell.Mnc);
            Assert.Equal("1323033", firstCell.Cid);
            Assert.Equal("20", firstCell.Lac);
            Assert.Equal(-101, firstCell.Rssi);
            Assert.Equal(-1, firstCell.Level);
            Assert.Equal("6", firstCell.Asu);
            Assert.Equal("-1", firstCell.Ta);
            Assert.Equal("400", firstCell.PscPci);
            Assert.True(firstCell.IsReg);

            //wcdma,-1,-1,-1,-1,-111,-1,1,-1,284,0
            var lastCell = locationEvent.Cells.LastOrDefault();
            Assert.NotNull(lastCell);
            Assert.Equal("wcdma", lastCell.Radio);
            Assert.Equal("-1", lastCell.Mcc);
            Assert.Equal("-1", lastCell.Mnc);
            Assert.Equal("-1", lastCell.Cid);
            Assert.Equal("-1", lastCell.Lac);
            Assert.Equal(-111, lastCell.Rssi);
            Assert.Equal(-1, lastCell.Level);
            Assert.Equal("1", lastCell.Asu);
            Assert.Equal("-1", lastCell.Ta);
            Assert.Equal("284", lastCell.PscPci);
            Assert.False(lastCell.IsReg);
        }

    }
}
