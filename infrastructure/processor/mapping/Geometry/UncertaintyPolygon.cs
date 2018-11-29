using Mapping.Mapping;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mapping.Geometry
{
    /// <summary>
    /// This uncertainty polygon is represented by a group of Coordinates
    /// </summary>
    [DataContract]
    public class UncertaintyPolygon : GeographicShape
    {
        [DataMember]
        private readonly List<Coordinate> points;

        public UncertaintyPolygon()
        {
            points = new List<Coordinate>();
        }

        public List<Coordinate> Points
        {
            get { return points; }
        }


        public void AddPoint(Coordinate p)
        {
            points.Add(p);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (!(obj is UncertaintyPolygon))
                return false;

            return Equals((UncertaintyPolygon)obj);
        }

        protected bool Equals(UncertaintyPolygon obj)
        {
            if (obj == null)
                return false;

            if (obj.Points == null || this.Points == null)
            {
                return obj.Points == null && this.Points == null;
            }

            if (obj.Points.Count != this.Points.Count)
            {
                return false;
            }

            for (int i = 0; i < this.Points.Count; i++)
            {
                if (!this.Points[i].Equals(obj.Points[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return this.ComputeHashCode(points?.ToArray());
        }
    }
}
