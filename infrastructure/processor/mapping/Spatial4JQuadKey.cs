using Mapping.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mapping
{
    /// <summary>
    /// A key within a QuadTree for mapping
    /// </summary>
    public struct Spatial4JQuadKey : IRange<Coordinate>
    {
        private static readonly Regex KeyPattern = new Regex(@"^[ABCD]+$", RegexOptions.Compiled);

        private Dictionary<QuadKeyLocation, Spatial4JQuadKey?> neighbours;
        private readonly string key;


        public Dictionary<QuadKeyLocation, Spatial4JQuadKey?> Neighbours
        {
            get
            {
                if (neighbours == null)
                    neighbours = Spatial4JTileSystem.GetQuadKeyNeighbours(this);
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


        public Spatial4JQuadKey(string key)
        {
            ArgumentValidation.CheckArgumentForNullOrEmpty(key, "key");

            if (!KeyPattern.Match(key).Success)
            {
                throw new ArgumentException("Invalid quad key specified.", "key");
            }

            this.key = key;
            this.neighbours = null;
        }


        public bool OverlapsWith(IRange range)
        {
            // QuadKeys always abut if they're at the same level.  Keys are
            // only considered to overlap if one contains the other.
            return this.Contains(range) || range.Contains(this);
        }

        public bool Contains(IRange innerRange)
        {
            if (!(innerRange is Spatial4JQuadKey)) return false;

            // If the inner key starts with the same characters as this key, it's is contained by it
            var innerQuadKey = (Spatial4JQuadKey)innerRange;
            return innerQuadKey.Key.StartsWith(this.Key, StringComparison.Ordinal);
        }

        public bool Contains(Coordinate point)
        {
            ArgumentValidation.CheckArgumentForNull(point, "point");

            Spatial4JQuadKey qk = ConvertCoordinateToQuadKey(point, this.Level);
            return (qk.Key == this.Key);
        }


        public bool Equals(Spatial4JQuadKey other)
        {
            return other.Key == this.Key;
        }

        public string ParentQuadKeyId()
        {
            return (this.Key.Length > 1) ? this.Key.Substring(0, this.Key.Length - 1) : String.Empty;
        }

        /// <summary>
        /// Check if quadkey is child quadkey
        /// </summary>
        public bool DescendantOf(Spatial4JQuadKey q)
        {
            if (String.IsNullOrEmpty(q.key) || String.IsNullOrEmpty(this.key))
                return false;

            if (q.Key.Length == this.Key.Length) return false;

            return this.ToString().StartsWith(q.ToString(), StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Check if quadkey is child quadkey or equal
        /// </summary>
        public bool DescendantOfOrEqual(Spatial4JQuadKey q)
        {
            return this.Equals(q) || this.DescendantOf(q);
        }

        /// <summary>
        /// Check if this quadkey is parent of the specified quadkey.
        /// </summary>
        public bool ParentOf(Spatial4JQuadKey q)
        {
            return q.DescendantOf(this);
        }

        /// <summary>
        /// Check if this quadkey is parent of or equal to this 
        /// </summary>
        public bool ParentOfOrEqual(Spatial4JQuadKey q)
        {
            return q.DescendantOfOrEqual(this);
        }

        /// <summary>
        /// Return the children of this quadkey down to a specific level of the hierarchy.
        /// </summary>
        public IEnumerable<Spatial4JQuadKey> Children(int depth)
        {
            var keys = new[] { 'A', 'B', 'C', 'D' };
            if (depth == 0)
            {
                yield return this;
            }
            else
                // If this is the bottom of the hierarchy, there are not more children to return
                if (this.Level == Spatial4JTileSystem.MaxDetailLevel)
                {
                    yield break;
                }

            // If we want this node's immediate children, return them
            if (depth == 1)
            {
                for (var i = 0; i < 4; i++)
                {
                    yield return new Spatial4JQuadKey(this.key + keys[i]);
                }
            }
            // If we want deeper, then recurse into the hierarchy
            else
            {
                foreach (var child in this.Children(depth - 1))
                {
                    for (var i = 0; i < 4; i++)
                    {
                        yield return new Spatial4JQuadKey(child.key + keys[i]);
                    }
                }
            }
        }

        public IEnumerable<Spatial4JQuadKey> SumOfAllNeighbours(int depth)
        {
            ArgumentValidation.CheckArgumentIsInRange(depth, 1, Spatial4JTileSystem.MaxDetailLevel, "depth");
            return this.Children(depth).SelectMany(q => q.Neighbours.Values).Where(q => (object)q != null).Select(q => ((Spatial4JQuadKey)q)).Distinct();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (!(obj is Spatial4JQuadKey)) return false;
            return Equals((Spatial4JQuadKey)obj);
        }

        public override string ToString()
        {
            return this.Key;
        }

        public override int GetHashCode()
        {
            return this.Key.GetHashCode();
        }

        public static bool operator ==(Spatial4JQuadKey qk1, Spatial4JQuadKey qk2)
        {
            return qk1.Equals(qk2);
        }

        public static bool operator !=(Spatial4JQuadKey qk1, Spatial4JQuadKey qk2)
        {
            return !(qk1 == qk2);
        }

        /// <summary>
        /// Converts the coordinate to a quad key using the linear conversion scheme.
        /// </summary>
        public static Spatial4JQuadKey ConvertCoordinateToQuadKey(Coordinate coordinate, int level)
        {
            ArgumentValidation.CheckArgumentForNull(coordinate, "coordinate");
            return Spatial4JTileSystem.CoordinateToQuadKey(coordinate, level);
        }

        /// <summary>
        /// Converts the quad key to a quadrangle.
        /// </summary>
        public static Quadrangle ConvertQuadKeyToQuadrangle(Spatial4JQuadKey quadKey)
        {
            return Spatial4JTileSystem.QuadKeyToQuadrangle(quadKey);
        }

        /// <summary>
        /// Determine the subset of quadkeys within the viewport that are completely contained by it.
        /// 
        /// Larger granularity quadkeys are favoured over smaller granularity keys.
        /// </summary>
        public static List<Spatial4JQuadKey> CalculateBestFitQuadKeys(Quadrangle viewport, int maximumLookahead, int maximumQuadkeyDepth)
        {
            var results = new List<Spatial4JQuadKey>();

            // initial candidates for the recursive helper algorithm.
            var candidates = Spatial4JTileSystem.GetQuadkeysFromQuadrangle(viewport).ToList();
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
        public static bool CalculateBestFitQuadKeys(Quadrangle viewport, IEnumerable<Spatial4JQuadKey> candidates, List<Spatial4JQuadKey> results, int levelsToDrilldown, int maximumQuadkeyDepth)
        {
            bool allCandidatesAdded = true;
            foreach (var candidate in candidates)
            {
                var candidateQuad = ConvertQuadKeyToQuadrangle(candidate);
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
        public static Spatial4JQuadKey Create(int level, int row, int column)
        {
            if (!Spatial4JTileSystem.IsValidLevelOfDetail(level))
                throw new ArgumentOutOfRangeException("level");
            ArgumentValidation.CheckArgumentIsGreaterThanOrEqualToValue(row, 0, "row");
            ArgumentValidation.CheckArgumentIsGreaterThanOrEqualToValue(column, 0, "column");

            var quadKey = String.Empty;
            for (var currentLevel = 1; currentLevel <= level; currentLevel++)
                quadKey += (((row >> level - currentLevel) & 1) << 1) | ((column >> level - currentLevel) & 1);

            return new Spatial4JQuadKey(quadKey.Replace('0', 'A').Replace('1', 'B').Replace('2', 'C').Replace('3', 'D'));
        }

        // Found through experimentation
        // If you try to search on a region smaller than this, Solr throws errors.
        private const double minRegionWidth = 0.00000538;
        private const double minRegionHeight = 0.0000027;

        /// <summary>
        /// Gets the WKT string that represents the polygon that surrounds a quadkey.
        /// </summary>
        public static string QuadkeyToPolygon(string quadkey)
        {
            var ret = new List<Coordinate>();
            var upperLeft = Spatial4JTileSystem.QuadKeyToCoordinate(quadkey, 0);
            var lowerRight = Spatial4JTileSystem.QuadKeyToCoordinate(quadkey, 1);

            // Ensure the calculated the area is within the required range
            var longitudes = MathUtils.ExpandWithinRange(new Tuple<double, double>(lowerRight.Longitude, upperLeft.Longitude), new Tuple<double, double>(-180, 180), minRegionWidth);
            var latitudes = MathUtils.ExpandWithinRange(new Tuple<double, double>(lowerRight.Latitude, upperLeft.Latitude), new Tuple<double, double>(-90, 90), minRegionHeight);

            ret.Add(new Coordinate(latitudes.Item2, longitudes.Item1));
            ret.Add(new Coordinate(latitudes.Item1, longitudes.Item1));
            ret.Add(new Coordinate(latitudes.Item1, longitudes.Item2));
            ret.Add(new Coordinate(latitudes.Item2, longitudes.Item2));
            ret.Add(ret[0]);

            var poly = new Polygon(ret);

            return Polygon.ConvertToWKT(poly);
        }
    }
}