using System;

namespace RadioMessagesProcessor.Dtos
{
    public class CellInfoDto
    {
        public string Radio { get; set; }
        public string Mcc { get; set; }
        public string Mnc { get; set; }
        public string Lac { get; set; }
        public string Cid { get; set; }
        public string PscPci { get; set; }
        public int Rssi { get; set; }
        public bool IsReg { get; set; }
        public int Level { get; private set; }
        public string Asu { get; private set; }
        public string Ta { get; private set; }

        #region Extended Decoded Information
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public int Samples { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public int Range { get; set; }

        public bool IsDecoded { get; set; }
        public bool IsMain { get; set; }

        public string ToFriendlyName()
        {
            return $"{this.Rssi} [{this.Radio}:{this.Mnc}-{this.Mcc}-{this.Lac}-{this.Cid}]";
        }
        #endregion

    }

}