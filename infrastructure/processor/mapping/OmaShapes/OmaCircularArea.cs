using Mapping.Mapping;
using System.Collections.Generic;

namespace Mapping.Shapes
{
    public class OmaCircularArea : OmaShape
    {
        public static bool CanCast(shape shape)
        {
            return (shape != null && shape.CircularArea != null && shape.CircularArea.coord != null);
        }

        public OmaCircularArea(OmaShape omaShape)
            : base(omaShape)
        {
            GetProperties();
        }

        public OmaCircularArea(string xmlData)
            : base(xmlData)
        {
            GetProperties();
        }

        protected void GetProperties()
        {
            if (CanCast(this.Shape))
            {
                this.Center = new Coordinate(this.Shape.CircularArea.coord.X.ParsedValue,
                                             this.Shape.CircularArea.coord.Y.ParsedValue);
                this.Radius = this.Shape.CircularArea.radius;
            }
        }

        public Coordinate Center { set; get; }
        public int Radius { set; get; }

        public override IEnumerable<Coordinate> PolygonCoordinates
        {
            get { return OmaShape.SectorSliceToPolygon(this.Center.Longitude, this.Center.Latitude, 0, 360, 0, Radius); }
        }
    }
}
