namespace Mapping
{
    /// <summary>
    /// Dictates the operations that are required to implement a class
    /// that expresses the boundaries of series of points in an arbitrary
    /// scope.  i.e. time range, geometrical range, etc.
    /// </summary>
    public interface IRange
    {
        /// <summary>
        /// Determines whether or not this range and the supplied range
        /// contain at least one shared point within their scopes.
        /// </summary>
        bool OverlapsWith(IRange range);

        /// <summary>
        /// Determines whether or not <paramref name="innerRange"/> is completely contained by this range.
        /// </summary>
        bool Contains(IRange innerRange);
    }

    /// <summary>
    /// Dictates the operations that are required to implement a class
    /// that expresses the boundaries of series of points in an arbitrary
    /// scope.  i.e. time range, geometrical range, etc.
    /// </summary>
    /// <typeparam name="TPoint">
    /// The type of objects that make up the points within the range.
    /// </typeparam>
    public interface IRange<in TPoint> : IRange
    {
        /// <summary>
        /// Determines whether or not <paramref name="point"/> is contained by this range.
        /// </summary>
        bool Contains(TPoint point);
    }
}
