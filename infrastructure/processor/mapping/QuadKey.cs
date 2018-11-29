using Mapping.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mapping
{
    /// <summary>
    /// QuadKey locations laid out in a 3 X 3 matrix
    /// </summary>
    public enum QuadKeyLocation
    {
        NW,
        N,
        NE,
        W,
        Central,
        E,
        SW,
        S,
        SE
    }

    /// <summary>
    /// A key within a QuadTree for mapping
    /// </summary>
    public struct QuadKey : IRange<Coordinate>
    {
        #region Fields
        
        private static readonly Regex KeyPattern = new Regex(@"^[0123]+$", RegexOptions.Compiled);
        
        private Dictionary<QuadKeyLocation, QuadKey?> neighbours;
        private readonly string key;

        #endregion

        #region Properties

        public Dictionary<QuadKeyLocation, QuadKey?> Neighbours
        {
            get
            {
                if (neighbours == null)
                    neighbours = TileSystem.GetQuadKeyNeighbours(this);
                return this.neighbours;
            }
        }

        public string Key
        {
            get
            {
                return this.key;
            }
        }

        public int Level
        {
            get
            {
                return this.Key.Length;
            }
        }

        #endregion

        #region Constructor
        
        public QuadKey(string key)
        {
            ArgumentValidation.CheckArgumentForNullOrEmpty(key, "key");

            if (!KeyPattern.Match(key).Success)
            {
                throw new ArgumentException("Invalid quad key specified.", "key");
            }

            this.key = key;
            this.neighbours = null;
        }

        #endregion
        
        #region Public Methods
        
        public bool OverlapsWith(IRange range)
        {
            // QuadKeys always abut if they're at the same level.  Keys are
            // only considered to overlap if one contains the other.
            return this.Contains(range) || range.Contains(this);
        }

        public bool Contains(IRange innerRange)
        {
            if (!(innerRange is QuadKey)) return false;

            // If the inner key starts with the same characters as this key, it's is contained by it
            var innerQuadKey = (QuadKey)innerRange;
            return innerQuadKey.Key.StartsWith(this.Key, StringComparison.Ordinal);
        }

        public bool Contains(Coordinate point)
        {
            ArgumentValidation.CheckArgumentForNull(point, "point");

            QuadKey qk = QuadKey.ConvertCoordinateToNumericQuadKey(point, this.Level);
            return (qk.Key == this.Key);
        }      

        public bool Equals(QuadKey other)
        {
            return other.Key == this.Key;
        }

        public string ParentQuadKeyId()
        {
            return (this.Key.Length > 1) ? this.Key.Substring(0, this.Key.Length - 1) : string.Empty;
        }

        /// <summary>
        /// Check if quadkey is child quadkey
        /// </summary>
        public bool DescendantOf(QuadKey q)
        {
            if (string.IsNullOrEmpty(q.key) || string.IsNullOrEmpty(this.key))
                return false;

            if (q.Key.Length == this.Key.Length) return false;

            return this.ToString().StartsWith(q.ToString(), StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Check if quadkey is child quadkey or equal
        /// </summary>
        public bool DescendantOfOrEqual(QuadKey q)
        {
            return this.Equals(q) || this.DescendantOf(q);
        }

        /// <summary>
        /// Check if this quadkey is parent of the specified quadkey.
        /// </summary>
        public bool ParentOf(QuadKey q)
        {
            return q.DescendantOf(this);
        }

        /// <summary>
        /// Check if this quadkey is parent of or equal to this 
        /// </summary>
        public bool ParentOfOrEqual(QuadKey q)
        {
            return q.DescendantOfOrEqual(this);
        }

        /// <summary>
        /// Return the children of this quadkey down to a specific level of the hierarchy.
        /// </summary>
        public IEnumerable<QuadKey> Children(int depth)
        {         
            if (depth == 0)
            {
                yield return this;
            }
            else
                // If this is the bottom of the hierarchy, there are not more children to return
                if (this.Level == TileSystem.MaxDetailLevel)
                {
                    yield break;
                }

                // If we want this node's immediate children, return them
                if (depth == 1)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        yield return new QuadKey(this.key + i);
                    }
                }
                // If we want deeper, then recurse into the hierarchy
                else
                {
                    foreach (var child in this.Children(depth - 1))
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            yield return new QuadKey(child.key + i);
                        }
                    }
                }
        }

        public IEnumerable<QuadKey> SumOfAllNeighbours(int depth)
        {
            return this.Children(depth).SelectMany(q => q.Neighbours.Values).Where(q => (object)q != null).Select(q => ((QuadKey)q)).Distinct();
        }

        #endregion

        #region Override Methods

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (!(obj is QuadKey)) return false;
            return Equals((QuadKey)obj);
        }

        public override string ToString()
        {
            return this.Key;
        }

        public override int GetHashCode()
        {
            return this.Key.GetHashCode();
        }

        #endregion

        #region Static Methods

        public static bool operator ==(QuadKey qk1, QuadKey qk2)
        {
            return qk1.Equals(qk2);
        }

        public static bool operator !=(QuadKey qk1, QuadKey qk2)
        {
            return !(qk1 == qk2);
        }

        /// <summary>
        /// Converts the coordinate to a quad key using the numeric conversion scheme.
        /// </summary>
        /// <remarks>
        /// Phase this method out.  Please do not use it for new code!!!!
        /// </remarks>
        public static QuadKey ConvertCoordinateToNumericQuadKey(Coordinate coordinate, int level)
        {
            return ConvertCoordinateToQuadKey(coordinate, level, false);
        }

        /// <summary>
        /// Converts the coordinate to a quad key using the linear conversion scheme.
        /// </summary>
        public static QuadKey ConvertCoordinateToLinearQuadKey(Coordinate coordinate, int level)
        {
            return ConvertCoordinateToQuadKey(coordinate, level, true);
        }

        private static QuadKey ConvertCoordinateToQuadKey(Coordinate coordinate, int level, bool useLinearQuadkeys)
        {
            ArgumentValidation.CheckArgumentForNull(coordinate, "coordinate");

            return TileSystem.CoordinateToQuadKey(coordinate, level, useLinearQuadkeys);
        }

        /// <summary>
        /// Converts the quad key to a quadrangle.
        /// </summary>
        public static Quadrangle ConvertQuadKeyToQuadrangle(QuadKey quadKey)
        {
            return TileSystem.QuadKeyToQuadrangle(quadKey);
        }
        
        /// <summary>
        /// Determine the subset of quadkeys within the viewport that are completely contained by it.
        /// 
        /// Larger granularity quadkeys are favoured over smaller granularity keys.
        /// </summary>
        public static List<QuadKey> CalculateBestFitQuadKeys(Quadrangle viewport, int maximumLookahead, int maximumQuadkeyDepth)
        {
            var results = new List<QuadKey>();

            // initial candidates for the recursive helper algorithm.
            var candidates = TileSystem.GetQuadkeysFromQuadrangle(viewport).ToList();
            CalculateBestFitQuadKeys(viewport, candidates, results, maximumLookahead, maximumQuadkeyDepth);

            return results;
        }

        /// <summary>
        /// Assess if any of the <paramref name="candidates"/> fit completely within the <paramref name="viewport"/>.  If they
        /// don't, break them down and see if any of their children fit.
        /// </summary>
        /// <returns>
        /// True if all <paramref name="candidates"/> fit in the <paramref name="viewport"/>, or false otherwise. 
        /// </returns>
        public static bool CalculateBestFitQuadKeys(Quadrangle viewport, IEnumerable<QuadKey> candidates, List<QuadKey> results, int levelsToDrilldown, int maximumQuadkeyDepth)
        {
            bool allCandidatesAdded = true;
            foreach (var candidate in candidates)
            {
                var candidateQuad = QuadKey.ConvertQuadKeyToQuadrangle(candidate);
                if (viewport.Contains(candidateQuad))
                {
                    // If it fits, add it.
                    results.Add(candidate);
                }
                else if (viewport.OverlapsWith(candidateQuad))
                {
                    if (levelsToDrilldown > 0 && candidate.Level + 1 <= maximumQuadkeyDepth)
                    {
                        // If it doesn't fit, try to add a subset of the children
                        var children = candidate.Children(1).ToList();
                        bool allChildrenAdded = CalculateBestFitQuadKeys(viewport, children, results, levelsToDrilldown - 1, maximumQuadkeyDepth);

                        // If all of the children were added, then we really should have just added this quad, so
                        // remove the children and add the candidate.
                        if (allChildrenAdded)
                        {
                            children.ForEach(child => results.Remove(child));
                            results.Add(candidate);
                        }

                        allCandidatesAdded &= allChildrenAdded;
                    }
                    else
                    {
                        // If it doesn't fit but we can't drilldown further, add it.
                        results.Add(candidate);
                    }
                }
                else
                {
                    allCandidatesAdded = false;
                }
            }
            return allCandidatesAdded;
        }

        /// <summary>
        /// Given a level, row, and column, returns the equivalent QuadKey.
        /// </summary>
        public static QuadKey Create(int level, int row, int column)
        {
            if (!TileSystem.IsValidLevelOfDetail(level))
                throw new ArgumentOutOfRangeException("level");
            ArgumentValidation.CheckArgumentIsGreaterThanOrEqualToValue(row, 0, "row");
            ArgumentValidation.CheckArgumentIsGreaterThanOrEqualToValue(column, 0, "column");

            var quadKey = String.Empty;
            for (var currentLevel = 1; currentLevel <= level; currentLevel++)
                quadKey += (((row >> level - currentLevel) & 1) << 1) | ((column >> level - currentLevel) & 1);

            return new QuadKey(quadKey);
        }

        #endregion

    }
}
