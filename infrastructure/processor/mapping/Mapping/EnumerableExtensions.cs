using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mapping.Mapping
{
    /// <summary>
    /// Extensions methods for <see cref="IEnumerable{T}"/>
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Finds the index in the collection where the predicate evaluates to true.
        /// 
        /// Returns -1 if no matching item found
        /// </summary>
        /// <typeparam name="TSource">Type of collection</typeparam>
        /// <param name="source">Source collection</param>
        /// <param name="predicate">Function to evaluate</param>
        /// <returns>Index where predicate is true, or -1 if not found.</returns>
        public static int FindIndex<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            var enumerator = source.GetEnumerator();
            var index = 0;
            while (enumerator.MoveNext())
            {
                var obj = enumerator.Current;
                if (predicate(obj))
                    return index;
                index++;
            }
            return -1;
        }

        /// <summary>
        /// Finds the index in the collection where the predicate evaluates to true.
        /// 
        /// Returns -1 if no matching item found
        /// </summary>
        /// <typeparam name="TSource">Type of collection</typeparam>
        /// <param name="source">Source collection</param>
        /// <param name="predicate">Function to evaluate</param>
        /// <returns>Index where predicate is true, or -1 if not found.</returns>
        public static int FindIndex<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            var enumerator = source.GetEnumerator();
            var index = 0;
            while (enumerator.MoveNext())
            {
                var obj = enumerator.Current;
                if (predicate(obj, index))
                    return index;
                index++;
            }
            return -1;
        }

        /// <summary>
        /// Returns the first element of a sequence, or a default value if the sequence contains no elements. If source collection is null it returns default value
        /// </summary>
        /// <typeparam name="TSource">Type of collection</typeparam>
        /// <param name="source">Source collection</param>
        /// <returns>the first element of a sequence, or a default value if the sequence contains no elements</returns>
        public static TSource SafeFirstOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                return default(TSource);
            return source.FirstOrDefault();
        }

        /// <summary>
        /// Returns the first element of a sequence, or a default value if the sequence contains no elements. If source collection is null it returns default value
        /// </summary>
        /// <typeparam name="TSource">Type of collection</typeparam>
        /// <param name="source">Source collection</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>the first element of a sequence, or a default value if the sequence contains no elements</returns>
        public static TSource SafeFirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
                return default(TSource);

            return source.FirstOrDefault(predicate);
        }

        /// <summary>
        /// Returns the first element of a sequence. If source collection is null it throws null parameter exception
        /// </summary>
        /// <typeparam name="TSource">Type of collection</typeparam>
        /// <param name="source">Source collection</param>
        /// <returns>the first element of a sequence</returns>
        public static TSource SafeFirst<TSource>(this IEnumerable<TSource> source)
        {
            ArgumentValidation.CheckArgumentForNull(source, nameof(source));

            return source.First();
        }

        /// <summary>
        /// Returns whether or not the provided sequence contains any elements. This is safe to call if  the sequence is <c>null</c>,
        /// in which case <c>false</c> will be returned.
        /// </summary>
        /// <typeparam name="TSource">Type of collection</typeparam>
        /// <param name="source">Source collection</param>
        /// <returns><c>true</c> if the enumerable contains any elements, <c>false</c> otherwise</returns>
        public static bool SafeAny<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                return false;

            return source.Any();
        }

        /// <summary>
        /// Returns whether or not the provided sequence contains any elements. This is safe to call if  the sequence is <c>null</c>,
        /// in which case <c>false</c> will be returned.
        /// </summary>
        /// <typeparam name="TSource">Type of collection</typeparam>
        /// <param name="source">Source collection</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns><c>true</c> if the sequence contains any elements which satisfy the given predicate, <c>false</c> otherwise</returns>
        public static bool SafeAny<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
                return false;

            return source.Any(predicate);
        }

        /// <summary>
        /// Returns an empty enumerable sequence if the current sequence is <c>null</c>.
        /// This is useful to chain Linq functions together and ensure to prevent <see cref="ArgumentNullException"/>
        /// being thrown.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>The source, or an empty enumerable if the source is <c>cnull</c>.</returns>
        public static IEnumerable<TSource> EmptyIfNull<TSource>(this IEnumerable<TSource> source)
        {
            return source ?? System.Linq.Enumerable.Empty<TSource>();
        }

        /// <summary>
        /// Iterates the elements in <paramref name="enumerable" /> and calls <paramref name="action" /> for each element.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in <paramref name="enumerable" />.
        /// </typeparam>
        /// <param name="enumerable">
        /// The elements to perform <paramref name="action" /> on.
        /// </param>
        /// <param name="action">
        /// The <see cref="System.Action&lt;TSource&gt;"/> to perform on the elements of <paramref name="enumerable" />.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="enumerable" /> and/or <paramref name="action" /> is null.
        /// </exception>
        public static void ForEach<TSource>(this IEnumerable<TSource> enumerable, Action<TSource> action)
        {
            ArgumentValidation.CheckArgumentForNull(enumerable, nameof(enumerable));
            ArgumentValidation.CheckArgumentForNull(action, nameof(action));

            foreach (TSource t in enumerable)
            {
                action(t);
            }
        }

        /// <summary>
        /// Determines if <paramref name="enumerable"/> is empty.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in <paramref name="enumerable" />.
        /// </typeparam>
        /// <param name="enumerable">
        /// The <see cref="System.Collections.Generic.IEnumerable&lt;TSource&gt;"/> to check for elements
        /// </param>
        /// <returns>
        /// <c>true</c>, if there are any elements in <paramref name="enumerable"/>; <c>false</c>, otherwise.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="enumerable" /> is null
        /// </exception>
        public static bool Empty<TSource>(this IEnumerable<TSource> enumerable)
        {
            ArgumentValidation.CheckArgumentForNull(enumerable, nameof(enumerable));

            return !enumerable.Any();
        }


        /// <summary>
        /// Creates an <see cref="System.Collections.Generic.IEnumerable&lt;T&gt;"/> from a tree of
        /// <see cref="System.Collections.IEnumerable"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of elements to be returned in the <see cref="System.Collections.Generic.IEnumerable&lt;T&gt;"/>.
        /// </typeparam>
        /// <param name="enumerable">
        /// The <see cref="System.Collections.IEnumerable"/> to check for elements of type <typeparamref name="T"/>.
        /// </param>
        /// <returns>
        /// <see cref="System.Collections.Generic.IEnumerable&lt;T&gt;"/> containing elements found in the 
        /// <see cref="System.Collections.IEnumerable"/> tree.
        /// </returns>
        public static IEnumerable<T> Flatten<T>(this IEnumerable enumerable)
        {
            ArgumentValidation.CheckArgumentForNull(enumerable, nameof(enumerable));

            foreach (object item in enumerable)
            {
                if (item is T)
                {
                    yield return (T)item;
                }
                else if (item is IEnumerable)
                {
                    foreach (T t in Flatten<T>((IEnumerable)item))
                    {
                        yield return t;
                    }
                }
            }
        }

        /// <summary>
        /// Flattens a tree into a list.
        /// </summary>
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> rootNodes, Func<T, IEnumerable<T>> getChildrenFunc)
        {
            return rootNodes.SelectMany(
                node =>
                {
                    var childNodes = getChildrenFunc(node);

                    // If there are any children to visit, recursively call this function
                    return childNodes == null ? System.Linq.Enumerable.Empty<T>() : childNodes.Flatten(getChildrenFunc);
                }).Concat(rootNodes);
        }

        /// <summary>
        /// Returns a <see cref="Tuple"/> representing the minimum and maximum values in a sequence of 
        /// <see cref="Int64"/> <paramref name="values"/>.
        /// </summary>
        /// 
        /// <param name="values">
        /// A sequence of <see cref="Int64"/> values to determine the minimum and maximum values of.
        /// </param>
        /// 
        /// <returns>
        /// The minimum and maximum values in the sequence.
        /// </returns>
        /// 
        /// <exception cref="ArgumentNullException">
        /// <paramref name="values"/> is <see langword="null"/>.
        /// </exception>
        /// 
        /// <exception cref="InvalidOperationException">
        /// <paramref name="values"/> contains no elements.
        /// </exception>
        public static Tuple<long, long> MinMax(this IEnumerable<long> values)
        {
            ArgumentValidation.CheckArgumentForNull(values, "values");

            var e = values.GetEnumerator();
            if (!e.MoveNext())
            {
                throw new InvalidOperationException("Input sequence contained no values");
            }

            long value = e.Current;
            long min = value;
            long max = value;

            while (e.MoveNext())
            {
                value = e.Current;
                min = Math.Min(min, value);
                max = Math.Max(max, value);
            }

            var result = new Tuple<long, long>(min, max);
            return result;
        }

        /// <summary>
        /// returns a set of values as an enumerable
        /// </summary>
        /// <typeparam name="T">The type of the objects</typeparam>
        /// <param name="values">The values to return as a sequence</param>
        /// <returns>an enumerable containing all of the values passed in</returns>
        public static IEnumerable<T> Return<T>(params T[] values)
        {
            return values;
        }
    }
}
