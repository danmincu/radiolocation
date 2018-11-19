using System;

namespace RadioMessagesProcessor.Dtos
{
    public class RadioCellInfoDto
    {
        public static RadioCellInfoDto Parse(string[] input)
        {
            return new RadioCellInfoDto
            {
                // #Radio,Mcc,Mnc,Cid,Lac,SignalS,Level,mAsu,mTa,PscPci,isReg
                // wcdma,302,490,1323033,20,-101,-1,6,-1,400,1
                // wcdma,-1,-1,-1,-1,-99,-1,7,-1,167,0
                Radio = input[0],
                Mcc = input[1],
                Mnc = input[2],
                Cid = input[3],
                Lac = input[4],
                Rssi = int.Parse(input[5]),
                Level = int.Parse(input[6]),
                Asu = input[7],
                Ta = input[8],
                PscPci = input[9],
                IsReg = input[10].Equals("1", System.StringComparison.OrdinalIgnoreCase)
            };
        }

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
    }

}