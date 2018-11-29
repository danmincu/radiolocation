using Mapping.Mapping;
using System;

namespace Mapping.Geometry
{
    /// <summary>
    /// This class represents a viewable area of the map.
    /// </summary>
    public struct Extent : IEquatable<Extent>
    {
        public Extent(Point topLeft, Point bottomRight) : this()
        {
            if (topLeft.X > bottomRight.X)
                throw new ArgumentOutOfRangeException("bottomRight", "Point must have a higher or equal X coordinate than top left.");

            if(topLeft.Y < bottomRight.Y)
                throw new ArgumentOutOfRangeException("bottomRight", "Point must have a lower or equal  coordinate than top left.");

            TopLeft = topLeft;
            BottomRight = bottomRight;
        }

        #region Properties 

        /// <summary>
        /// Gets or the top left point of this extent.
        /// </summary>
        /// <value>The top left.</value>
        public Point TopLeft
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or the bottom right point of the this extent.
        /// </summary>
        /// <value>The bottom right.</value>
        public Point BottomRight
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the coordinate of the left edge of the extent.
        /// </summary>
        /// <value>The left coordinate of the extent.</value>
        public double Left
        {
            get { return TopLeft.X; }
        }

        /// <summary>
        /// Gets the coordinate of the right edge of the extent.
        /// </summary>
        /// <value>The right coordinate of the extent.</value>
        public double Right
        {
            get { return BottomRight.X; }
        }

        /// <summary>
        /// Gets the coordinate of the top edge the extent.
        /// </summary>
        /// <value>The top coordinate of the extent.</value>
        public double Top
        {
            get { return TopLeft.Y; }
        }

        /// <summary>
        /// Gets the coordinate of the bottom edge of the extent.
        /// </summary>
        /// <value>The bottom coordinate of the extent.</value>
        public double Bottom
        {
            get { return BottomRight.Y; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty, that is the area this extent encompasses is equivalent to zero.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty
        {
            get { return (TopLeft.X == BottomRight.X) || (TopLeft.Y == BottomRight.Y); }
        }

        #endregion

        /// <summary>
        /// Determines if the area covered by this extent overlaps any piece of the area covered by the other extent.
        /// </summary>
        /// <param name="extent">The extent to check against.</param>
        /// <returns><c>True</c> if the area covered by this extent overlaps that of the target extent.</returns>
        public bool IntersectsWith(Extent extent)
        {
            if (this.IsEmpty || extent.IsEmpty)
                return false;

            return (extent.Left <= Right) &&
                   (extent.Right >= Left) &&
                   (extent.Top >= Bottom) &&
                   (extent.Bottom <= Top);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is Extent))
                return false;

            return this.Equals((Extent) obj);
        }

        /// <summary>
        /// Determines if this extent and the target are equivalent.
        /// </summary>
        /// <param name="other">The other extent.</param>
        /// <returns><c>True</c> if this extent is the same as the target; <c>false</c> otherwise.</returns>
        public bool Equals(Extent other)
        {
            return this.TopLeft.Equals(other.TopLeft) && this.BottomRight.Equals(other.BottomRight);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this.ComputeHashCode(this.TopLeft, this.BottomRight);
        }

        #region Operators

        public static bool operator ==(Extent extent1, Extent extent2)
        {
            return extent1.Equals(extent2);
        }

        public static bool operator !=(Extent extent1, Extent extent2)
        {
            return !extent1.Equals(extent2);
        }

        #endregion
    }
}