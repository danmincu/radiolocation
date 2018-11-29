using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Mapping.Mapping
{
    /// <summary>
    /// Adds convenience extension methods to <see cref="Object"/>.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Convenience shortcut to Assembly.GetManifestResourceStream(Type, String) where the
        /// <see cref="Type"/> is obtained from the specified <paramref name="instance"/> and the <see cref="Assembly"/>
        /// is derived from said <see cref="Type"/>.
        /// </summary>
        /// 
        /// <param name="instance">
        /// An instance of a class that is a sibling to the embedded resource.
        /// </param>
        /// 
        /// <param name="fileName">
        /// The name of the file that is embedded as a resource inside the assembly where <paramref name="instance"/> is
        /// found.
        /// </param>
        /// 
        /// <returns>
        /// A <see cref="Stream"/> to the embedded resource data.
        /// </returns>
        public static Stream GetManifestResourceStream(this Object instance, string fileName)
        {
            var type = instance.GetType();
            return GetManifestResourceStream(type, fileName);
        }

        /// <summary>
        /// Convenience shortcut to Assembly.GetManifestResourceStream(Type, String) where the
        /// <see cref="Type"/> is comes from <paramref name="type"/> and the <see cref="Assembly"/>
        /// is derived from said <see cref="Type"/>.
        /// </summary>
        /// 
        /// <param name="type">
        /// A type that is a sibling to the embedded resource.
        /// </param>
        /// 
        /// <param name="fileName">
        /// The name of the file that is embedded as a resource inside the assembly where <paramref name="type"/> is
        /// found.
        /// </param>
        /// 
        /// <returns>
        /// A <see cref="Stream"/> to the embedded resource data.
        /// </returns>
        public static Stream GetManifestResourceStream(this Type type, string fileName)
        {
            var us = type.GetTypeInfo().Assembly;
            var result = us.GetManifestResourceStream(fileName);
            return result;
        }

        /// <summary>
        /// Convenience method to facilitate the computation of a hash code in implementations of
        /// <see cref="Object.GetHashCode()"/>.
        /// </summary>
        /// 
        /// <param name="instance">
        /// The object from which this method is invoked.  It is not used in the computation of the hash code but is
        /// merely there to be able to invoke this method on any object.
        /// </param>
        /// 
        /// <param name="fields">
        /// The values of the object's fields to participate in the computation of the hash code.
        /// </param>
        /// 
        /// <returns>
        /// A hash code combining that of all the supplied <paramref name="fields"/>.
        /// </returns>
        /// 
        /// <example>
        /// <para>
        /// The example below illustrates the typical use of the <see cref="ComputeHashCode(Object, Object[])"/> method:
        /// </para>
        /// <code>
        /// <![CDATA[
        /// internal class ResultRange
        /// {
        ///     private readonly string rangeStart;
        ///     private readonly string rangeEnd;
        ///     private readonly Int64 rangeSize;
        ///     private readonly Int64 numberOfItemsSeen;
        ///     private readonly Int64 numberOfSkippedItems;
        ///     private readonly Int64 offsetInsideBucket;
        ///     private readonly int hashCode;
        ///     private readonly string toString;
        /// 
        ///     public ResultRange
        ///         (string rangeStart, string rangeEnd, long rangeSize, long numberOfItemsSeen, long offsetInsideBucket)
        ///     {
        ///         this.rangeStart = rangeStart;
        ///         this.rangeEnd = rangeEnd;
        ///         this.rangeSize = rangeSize;
        ///         this.numberOfItemsSeen = numberOfItemsSeen;
        ///         this.numberOfSkippedItems = numberOfItemsSeen - rangeSize;
        ///         this.offsetInsideBucket = offsetInsideBucket;
        ///         this.hashCode = this.ComputeHashCode(
        ///                             rangeStart,
        ///                             rangeEnd,
        ///                             rangeSize,
        ///                             numberOfItemsSeen,
        ///                             offsetInsideBucket);
        ///         this.toString =
        ///             "[{0} - {1}] size: {2}, seen: {3}, offset: {4}".FormatInvariantCulture
        ///             (rangeStart, rangeEnd, rangeSize, numberOfItemsSeen, offsetInsideBucket);
        ///     }
        /// 
        ///     public override string ToString()
        ///     {
        ///         return this.toString;
        ///     }
        /// 
        ///     public override bool Equals(object obj)
        ///     {
        ///         var that = obj as ResultRange;
        ///         if (null == that)
        ///         {
        ///             return false;
        ///         }
        ///         return this.rangeStart == that.rangeStart
        ///                && this.rangeEnd == that.rangeEnd
        ///                && this.rangeSize == that.rangeSize
        ///                && this.numberOfItemsSeen == that.numberOfItemsSeen
        ///                && this.offsetInsideBucket == that.offsetInsideBucket;
        ///     }
        /// 
        ///     public override int GetHashCode()
        ///     {
        ///         return this.hashCode;
        ///     }
        /// }]]>
        /// </code>
        /// </example>
        public static int ComputeHashCode(this Object instance, params Object[] fields)
        {
            return ComputeHashCode(instance, (IEnumerable)fields);
        }

#pragma warning disable CA1801 // Review unused parameters
        /// <summary>
        /// Convenience method to facilitate the computation of a hash code in implementations of
        /// <see cref="Object.GetHashCode()"/>.
        /// </summary>
        /// 
        /// <param name="instance">
        /// The object from which this method is invoked.  It is not used in the computation of the hash code but is
        /// merely there to be able to invoke this method on any object.
        /// </param>
        /// 
        /// <param name="fields">
        /// The values of the object's fields to participate in the computation of the hash code.
        /// </param>
        /// 
        /// <returns>
        /// A hash code combining that of all the supplied <paramref name="fields"/>.
        /// </returns>
        /// 
        /// <example>
        /// <para>
        /// The example below illustrates the typical use of the <see cref="ComputeHashCode(Object, Object[])"/> method:
        /// </para>
        /// <code>
        /// <![CDATA[
        /// internal class ResultRange
        /// {
        ///     private readonly string rangeStart;
        ///     private readonly string rangeEnd;
        ///     private readonly Int64 rangeSize;
        ///     private readonly Int64 numberOfItemsSeen;
        ///     private readonly Int64 numberOfSkippedItems;
        ///     private readonly Int64 offsetInsideBucket;
        ///     private readonly int hashCode;
        ///     private readonly string toString;
        /// 
        ///     public ResultRange
        ///         (string rangeStart, string rangeEnd, long rangeSize, long numberOfItemsSeen, long offsetInsideBucket)
        ///     {
        ///         this.rangeStart = rangeStart;
        ///         this.rangeEnd = rangeEnd;
        ///         this.rangeSize = rangeSize;
        ///         this.numberOfItemsSeen = numberOfItemsSeen;
        ///         this.numberOfSkippedItems = numberOfItemsSeen - rangeSize;
        ///         this.offsetInsideBucket = offsetInsideBucket;
        ///         this.hashCode = this.ComputeHashCode(
        ///                             rangeStart,
        ///                             rangeEnd,
        ///                             rangeSize,
        ///                             numberOfItemsSeen,
        ///                             offsetInsideBucket);
        ///         this.toString =
        ///             "[{0} - {1}] size: {2}, seen: {3}, offset: {4}".FormatInvariantCulture
        ///             (rangeStart, rangeEnd, rangeSize, numberOfItemsSeen, offsetInsideBucket);
        ///     }
        /// 
        ///     public override string ToString()
        ///     {
        ///         return this.toString;
        ///     }
        /// 
        ///     public override bool Equals(object obj)
        ///     {
        ///         var that = obj as ResultRange;
        ///         if (null == that)
        ///         {
        ///             return false;
        ///         }
        ///         return this.rangeStart == that.rangeStart
        ///                && this.rangeEnd == that.rangeEnd
        ///                && this.rangeSize == that.rangeSize
        ///                && this.numberOfItemsSeen == that.numberOfItemsSeen
        ///                && this.offsetInsideBucket == that.offsetInsideBucket;
        ///     }
        /// 
        ///     public override int GetHashCode()
        ///     {
        ///         return this.hashCode;
        ///     }
        /// }]]>
        /// </code>
        /// </example>
        public static int ComputeHashCode(this Object instance, IEnumerable fields)
        {
            var fieldsArray = fields?.Cast<object>().ToArray() ?? Enumerable.Empty<object>().ToArray();

            int result = (fieldsArray.Length == 0 || fieldsArray[0] == null) ? 0 : fieldsArray[0].GetHashCode();

            for (int i = 1; i < fieldsArray.Length; i++)
            {
                result = CombineHashCodes(result, (fieldsArray[i] == null) ? 0 : fieldsArray[i].GetHashCode());
            }

            return result;
        }
#pragma warning restore CA1801 // Review unused parameters

        /// <summary>
        /// A utility method for combining hash codes.  This was taken from <see cref="System.Tuple" /> since the
        /// method there is declare internal only and to avoid creating Tuples everywhere we want to use it.
        /// </summary>
        /// <param name="h1">The first hash code.</param>
        /// <param name="h2">The second hash code.</param>
        private static int CombineHashCodes(int h1, int h2)
        {
            return (((h1 << 5) + h1) ^ h2);
        }


        /// <summary>
        /// Converts this object to an enumerable sequence of type <typeparamref name="TSource"/>.  If the object is already
        /// of the desired type, it is returned directly.  If the object is a single value of type <typeparamref name="TSource"/>
        /// type, then a sequence with a single value is returned.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <param name="source">The source object to convert.</param>
        /// <returns>An enumerable sequence that is the source object, or a new sequence that contains only the source object.</returns>
        /// <exception cref="InvalidCastException">If the source object is either an IEnumerable of <typeparamref name="TSource"/>, or cannot be cast to <typeparamref name="TSource"/>.</exception>
        public static IEnumerable<TSource> ToEnumerable<TSource>(this object source)
        {
            ArgumentValidation.CheckArgumentForNull(source, nameof(source));

            var typedEnumerable = source as IEnumerable<TSource>;
            var enumerable = source as IEnumerable;

            // The object is already IEnumerable of the proper type.
            if (typedEnumerable != null)
                return typedEnumerable;

            // The object is a single value of the proper type, no casting required.
            if (source is TSource)
                return Enumerable.Repeat((TSource)source, 1);

            // We have an untype enumerable.  Try casting the sequence.
            if (enumerable != null)
                return enumerable.Cast<TSource>();

            // We have an object of a different type.  Try casting it.
            return Enumerable.Repeat((TSource)source, 1);
        }

        /// <summary>
        /// Uses reflection to try to get the value of a property
        /// </summary>
        /// <param name="subject">The object to get the property value from</param>
        /// <param name="propName">The name of the property to get the value from</param>
        /// <param name="value">The value of the property, if found</param>
        /// <returns>true, if it was able to get the value of the property, false otherwise</returns>
        public static bool TryGetPropertyValue(this object subject, string propName, out object value)
        {
            if (subject == null)
                throw new ArgumentNullException(nameof(subject));

            value = null;

            var pi = subject.GetType().GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (pi == null)
                return false;

            value = pi.GetValue(subject);

            return true;
        }
    }
}
