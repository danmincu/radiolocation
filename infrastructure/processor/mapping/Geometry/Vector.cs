using Mapping.Mapping;
using System;

namespace Mapping.Geometry
{
    public struct Vector : IEquatable<Vector>
    {
        private readonly double x;
        private readonly double y;

        public Vector(double x, double y) : this()
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Gets or sets the X component of this vector.
        /// </summary>
        /// <value>The X component of this vector. The default value is 0.</value>
        public double X
        {
            get { return this.x; }
        }

        /// <summary>
        /// Gets or sets the Y component of this vector.
        /// </summary>
        /// <value>The Y component of this vector. The default value is 0.</value>
        public double Y
        {
            get { return this.y; }
        }      

        /// <summary>
        /// Multiplies the specified vector by the specified scalar and returns the resulting <see cref="Vector" />. 
        /// </summary>
        /// <param name="vector">The vector to multiply.</param>
        /// <param name="scalar">The scalar to multiply.</param>
        /// <returns>The result of multiplying vector and scalar.</returns>
        public static Vector Multiply(Vector vector, double scalar)
        {
            return new Vector(vector.X * scalar, vector.Y * scalar);
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

            if (!(obj is Vector))
                return false;

            return this.Equals((Vector) obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this.ComputeHashCode(this.X, this.Y);
        }

        /// <summary>
        /// Determines if this vector and the target are equivalent.
        /// </summary>
        /// <param name="other">The other vector to compare.</param>
        /// <returns><c>True</c> if the two vectors are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Vector other)
        {
            return this.X == other.X && this.Y == other.Y;
        }

        #region Operators

        public static bool operator ==(Vector v1, Vector v2)
        {
            return v1.Equals(v2);
        }

        public static bool operator !=(Vector v1, Vector v2)
        {
            return !v1.Equals(v2);
        }

        #endregion
    }
}