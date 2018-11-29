using Mapping.Mapping;
using System.Collections.Generic;
using System.Linq;

namespace Mapping.Shapes
{
    public class OmaPolygon : OmaShape
    {
        private IEnumerable<Coordinate> coordinates;

        public static bool CanCast(shape shape)
        {
            return (shape != null && shape.Polygon != null
                            && shape.Polygon.outerBoundaryIs != null
                            && shape.Polygon.outerBoundaryIs.LinearRing != null && shape.Polygon.outerBoundaryIs.LinearRing.Any());
        }

        public OmaPolygon(OmaShape omaShape)
            : base(omaShape)
        {
            GetProperties();
        }

        public OmaPolygon(string xmlData)
            : base(xmlData)
        {
            GetProperties();
        }

        protected void GetProperties()
        {
            if (CanCast(this.Shape))
            {
                this.coordinates = this.Shape.Polygon.outerBoundaryIs.LinearRing.Select(crd => new Coordinate(crd.X.ParsedValue, crd.Y.ParsedValue));
            }
        }

        public override IEnumerable<Coordinate> PolygonCoordinates
        {
            get { return this.coordinates; }
        }

    }
}
