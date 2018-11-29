using Mapping.Mapping;
using System.Collections.Generic;

namespace Mapping.Shapes
{
    public class OmaCircularArcArea : OmaShape
    {
        private IEnumerable<Coordinate> coordinates;

        public static bool CanCast(shape shape)
        {
            return (shape != null && shape.CircularArcArea != null
                 && shape.CircularArcArea.coord != null);
        }

        public OmaCircularArcArea(OmaShape omaShape)
            : base(omaShape)
        {
            GetProperties();
        }

        public OmaCircularArcArea(string xmlData)
            : base(xmlData)
        {
            GetProperties();
        }

        protected void GetProperties()
        {
            if (CanCast(this.Shape))
            {
                this.Center = new Coordinate(this.Shape.CircularArcArea.coord.X.ParsedValue,
                                             this.Shape.CircularArcArea.coord.Y.ParsedValue);
                this.InRadius = this.Shape.CircularArcArea.inRadius;
                this.OutRadius = this.Shape.CircularArcArea.outRadius;
                this.StartAngle = this.Shape.CircularArcArea.startAngle;

                //We need to calculate the stop angle as the shape provides us with a start angle and a sweep angle
                this.CalculatedStopAngle = (short)((this.Shape.CircularArcArea.startAngle + this.Shape.CircularArcArea.sweepAngle) % 360);

                this.coordinates = OmaShape.SectorSliceToPolygon(
                    this.Center.Longitude,
                    this.Center.Latitude,
                    this.StartAngle,
                    (this.StartAngle == 0 && this.CalculatedStopAngle == 0) ? 360 : this.CalculatedStopAngle,
                    this.InRadius,
                    this.OutRadius);
            }
        }

        public int InRadius { set; get; }
        public int OutRadius { set; get; }
        public short StartAngle { set; get; }
        public short CalculatedStopAngle { set; get; }

        public Coordinate Center { set; get; }

        public override IEnumerable<Coordinate> PolygonCoordinates
        {
            get { return this.coordinates; }
        }
    }
}
