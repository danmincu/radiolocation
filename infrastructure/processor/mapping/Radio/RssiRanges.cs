using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Mapping.Radio
{
    public static class RssiRanges
    {
        private static int[][] lte = new int[][] {
                       new[] { -60, 150, 400 },
                       new[] { -70, 300, 400 },
                       new[] { -80, 600, 400 },
                       new[] { -90, 1200, 800 },
                       new[] { -100, 2400, 1200 },
                       new[] { -110, 4800, 2000 },
                       new[] { -120, 9600, 3000 } };

        private static int[][] cdma = new int[][]  {
                        new[] { -60, 200, 200 },
                        new[] { -70, 600, 300 },
                        new[] { -80, 800, 400 },
                        new[] { -90, 1000, 400 },
                        new[] { -100, 1300, 500 },
                        new[] { -106, 1430, 500 },
                        new[] { -107, 1800, 500 }, // << 
                        new[] { -108, 2450, 600 },
                        new[] { -109, 3000, 600 }, // <<
                        new[] { -110, 3500, 700 },
                        new[] { -111, 4000, 800 }, // <<
                        new[] { -112, 4850, 1000 },
                        new[] { -113, 6000, 1500 },
                        new[] { -114, 10000, 4000 },
                        new[] { -120, 25600, 5000 }
                     };

        public static int[][] Ranges(string radio)
        {
            return radio.Equals("lte", StringComparison.OrdinalIgnoreCase) ? lte : cdma;
        }

        public static int[] GetNearesRssi(string radio, int rssiRange)
        {
            return Ranges(radio).OrderBy(r => Math.Abs(r[0] - rssiRange)).FirstOrDefault();
        }
 

    }
}
