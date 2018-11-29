using System;
using System.Globalization;
using System.Linq;

namespace Mapping.Shapes
{
    public partial class OmaCoordinate
    {
        private double GetDecimalRepresentation()
        {
            var strList = this.Value.Split(' ');
            if (strList.Count() != 3)
            {
                //try the new OptusLBA format Bug: 56479
                const int secondsFixedLength = 6;
                const int minutesFixedLength = 2;
                const int optusFormatDotPositionFromTail = 5;
                if (this.Value.IndexOf('.') == this.Value.Length - optusFormatDotPositionFromTail)
                {
                    try
                    {
                        var orientation = this.Value.Last().ToString().ToUpper(CultureInfo.InvariantCulture);
                        var seconds = double.Parse(this.Value.Substring(this.Value.Length - secondsFixedLength - 1, secondsFixedLength), CultureInfo.InvariantCulture);
                        var minutes = double.Parse(this.Value.Substring(this.Value.Length - secondsFixedLength - 1 - minutesFixedLength, minutesFixedLength), CultureInfo.InvariantCulture);
                        var degrees = double.Parse(this.Value.Substring(0, this.Value.Length - secondsFixedLength - 1 - minutesFixedLength), CultureInfo.InvariantCulture);
                        double result = degrees + (minutes / 60.0) + (seconds / 3600.0);
                        if (orientation.Equals("S", StringComparison.OrdinalIgnoreCase) || orientation.Equals("W", StringComparison.OrdinalIgnoreCase))
                            result = -result;
                        return result;
                    }
                    catch (Exception)
                    {
                        return 0;
                    }                    
                }                
                return 0;
            }
            try
            {
                var degrees = double.Parse(strList[0], CultureInfo.InvariantCulture);
                var minutes = double.Parse(strList[1], CultureInfo.InvariantCulture);
                var seconds = double.Parse(strList[2].Remove(strList[2].Count() - 1), CultureInfo.InvariantCulture);
                double result = degrees + (minutes / 60.0) + (seconds / 3600.0);
                if (strList[2].Last().ToString().ToUpper(CultureInfo.InvariantCulture) == "S" || strList[2].Last().ToString().ToUpper(CultureInfo.InvariantCulture) == "W")
                    result = -result;
                return result;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public double ParsedValue
        {
            get
            {
                return GetDecimalRepresentation();
            }
        }
    }
}