using System;

namespace Mapping
{
    public static class MathUtils
    {
        /// <summary>
        /// Helper function to expand a line within a set range so that the expanded line fits entirely with that range. If the line fits within the range and
        /// is equal to or greater than the minimum size, it is returned as-is.
        /// If the needs to be expanded:
        ///   1. If the expanded line fits within the limit, it will be returned.
        ///   2. If the expanded line hangs off of the lower end of the limit, it will be shifted to begin at the lower point.
        ///   3. If the expanded line hangs off of the upper end of the limit, it will be shifted to end at the upper point.
        /// </summary>
        /// <param name="line">Line that may have to be expanded, if it does not meet the minimum </param>
        /// <param name="limit">Both upper and lower limits of the line.</param>
        /// <param name="minimumLength">Minimum length of the line.</param>
        /// <returns>Line that meets the minimum length and which falls between the given range limits.</returns>
        public static Tuple<double, double> ExpandWithinRange(Tuple<double, double> line, Tuple<double, double> limit, double minimumLength)
        {
            if (limit.Item2 - limit.Item1 < minimumLength)
                throw new ArgumentException("Minimum length cannot be larger than the range in which the line must fit");

            var currentLength = line.Item2 - line.Item1;

            // Determine the amount by which to expand the line to reach the minium length
            var expansionDelta = (currentLength < minimumLength) ? (minimumLength - currentLength) / 2 : 0;

            // If no expansion is required, return the original values
            if (expansionDelta == 0)
                return line;

            double shiftedLower = line.Item1 - expansionDelta;
            double shiftedUpper = line.Item2 + expansionDelta;

            if (shiftedLower < limit.Item1)
            {
                // The line falls off of the lower end of the limiting line
                shiftedLower = limit.Item1;
                shiftedUpper = limit.Item1 + minimumLength;
            }
            else if (shiftedUpper > limit.Item2)
            {
                // The line falls off of the upper end of the limiting line
                shiftedLower = limit.Item2 - minimumLength;
                shiftedUpper = limit.Item2;
            }

            return new Tuple<double, double>(shiftedLower, shiftedUpper);
        }

        /// <summary>
        /// Helper method that will ensure that the given value 'x' is always within the given upper and lower bounds.
        /// </summary>
        /// <param name="x">The value to evaluate.</param>
        /// <param name="lowerBound">Lower bound on 'x'.</param>
        /// <param name="upperBound">Upper bound on 'x'.</param>
        /// <returns>The value 'x' clamped by lowerBound and upperBound.</returns>
        public static double Clamp(double x, double lowerBound, double upperBound)
        {
            return (x < lowerBound ? lowerBound : (x > upperBound) ? upperBound : x);
        }

        /// <summary>
        /// Verifies that the given value 'x1' is equal to a value 'x2' within a given percentage tolerancePercentage.
        /// </summary>
        /// <param name="x1">The value to compare.</param>
        /// <param name="x2">The value we are comparing against.</param>
        /// <param name="tolerancePercentage">The tolerancePercentage for the comparison. This value should be in the range [0,+inf).</param>
        /// <returns>True if the value of x1 falls within the tolerancePercentage distance from x2.</returns>
        public static bool EqualsWithPercentageTolerance(this float x1, float x2, float tolerancePercentage)
        {
            ArgumentValidation.CheckArgumentIsGreaterThanOrEqualToValue(tolerancePercentage, 0, "tolerancePercentage");
            var toleranceOffset = tolerancePercentage * x2;
            var lowerBound = x2 - toleranceOffset - float.Epsilon;
            var upperBound = x2 + toleranceOffset + float.Epsilon;
            return (x1 >= lowerBound && x1 <= upperBound);
        }

        /// <summary>
        /// Verifies that the given value 'x1' is equal to a value 'x2' within a given percentage tolerancePercentage.
        /// </summary>
        /// <param name="x1">The value to compare.</param>
        /// <param name="x2">The value we are comparing against.</param>
        /// <param name="tolerancePercentage">The tolerancePercentage for the comparison. This value should be in the range [0,+inf).</param>
        /// <returns>True if the value of x1 falls within the tolerancePercentage distance from x2.</returns>
        public static bool EqualsWithPercentageTolerance(this double x1, double x2, double tolerancePercentage)
        {
            ArgumentValidation.CheckArgumentIsGreaterThanOrEqualToValue(tolerancePercentage, 0, "tolerancePercentage");
            var toleranceOffset = tolerancePercentage * x2;
            var lowerBound = x2 - toleranceOffset - double.Epsilon;
            var upperBound = x2 + toleranceOffset + double.Epsilon;
            return (x1 >= lowerBound && x1 <= upperBound);
        }

        /// <summary>
        /// Verifies that the given value 'x1' is equal to a value 'x2' within a given percentage tolerancePercentage.
        /// </summary>
        /// <param name="x1">The value to compare.</param>
        /// <param name="x2">The value we are comparing against.</param>
        /// <param name="tolerancePercentage">The tolerancePercentage for the comparison. This value should be in the range [0,+inf).</param>
        /// <returns>True if the value of x1 falls within the tolerancePercentage distance from x2.</returns>
        public static bool EqualsWithPercentageTolerance(this int x1, int x2, double tolerancePercentage)
        {
            return EqualsWithPercentageTolerance(x1, (double)x2, tolerancePercentage);
        }

        /// <summary>
        /// Verifies that the given value 'x1' is equal to a value 'x2' within a given percentage tolerancePercentage.
        /// </summary>
        /// <param name="x1">The value to compare.</param>
        /// <param name="x2">The value we are comparing against.</param>
        /// <param name="tolerancePercentage">The tolerancePercentage for the comparison. This value should be in the range [0,+inf).</param>
        /// <returns>True if the value of x1 falls within the tolerancePercentage distance from x2.</returns>
        public static bool EqualsWithPercentageTolerance(this long x1, long x2, double tolerancePercentage)
        {
            return EqualsWithPercentageTolerance(x1, (double)x2, tolerancePercentage);
        }
    }
}