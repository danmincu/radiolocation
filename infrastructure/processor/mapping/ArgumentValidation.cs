using Mapping.Mapping;
using System;
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace Mapping
{
    /// <summary>
    /// Helper methods for performing argument validation
    /// </summary>
    public static class ArgumentValidation
    {
        /// <summary>
        /// Checks if an argument value is null.
        /// </summary>
        /// <typeparam name="T">The type of argument.</typeparam>
        /// <param name="value">The value of the argument to be checked.</param>
        /// <param name="paramName">The name of the parameter to throw the exception for.</param>
        /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
        public static void CheckArgumentForNull<T>(T value, string paramName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Checks if a string argument is null or empty.
        /// </summary>
        /// <param name="value">The value of the argument to be checked.</param>
        /// <param name="paramName">The name of the parameter to throw the exception for.</param>
        /// <exception cref="ArgumentNullException">Thrown if value is null or empty.</exception>
        public static void CheckArgumentForNullOrEmpty(IEnumerable value, string paramName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (!value.GetEnumerator().MoveNext())
            {
                throw new ArgumentException("Enumeration sequence contains no elements", paramName);
            }
        }

        /// <summary>
        /// Checks if a string argument is null or empty.
        /// </summary>
        /// <param name="value">The value of the argument to be checked.</param>
        /// <param name="paramName">The name of the parameter to throw the exception for.</param>
        /// <exception cref="ArgumentNullException">Thrown if value is null or empty.</exception>
        public static void CheckArgumentForNullOrEmpty(string value, string paramName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Checks if a GUID argument is empty.
        /// </summary>
        /// <param name="value">The value of the argument to be checked.</param>
        /// <param name="paramName">The name of the parameter to throw the exception for.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is empty.</exception>
        public static void CheckArgumentForEmpty(Guid value, string paramName)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Checks the array and array elements for null.
        /// </summary>
        /// <typeparam name="T">The type of array being passed.</typeparam>
        /// <param name="array">The array to be checked for null.</param>
        /// <param name="paramName">Name of the parameter to be placed in the exception.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if an element of the array is null or the array itself is null.
        /// </exception>
        public static void CheckArrayForNull<T>(T[] array, string paramName) where T : class
        {
            CheckArgumentForNull(array, paramName);

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == null)
                {
                    throw new ArgumentNullException(string.Format(CultureInfo.CurrentCulture, "{0}[{1}]", paramName, i));
                }
            }
        }

        /// <summary>
        /// Checks the array is not null and then validates the length of an array argument is correct.
        /// </summary>
        /// <typeparam name="T">The type of array</typeparam>
        /// <param name="array">The array to be checked.</param>
        /// <param name="requiredLength">The length the array should be.</param>
        /// <param name="paramName">The name of the parameter to throw the exception for.</param>
        /// <exception cref="ArgumentNullException">Thrown if array is null.</exception>
        /// <exception cref="ArgumentException">Thrown if array is an incorrect length</exception>
        public static void CheckArrayLength<T>(T[] array, int requiredLength, string paramName)
        {
            CheckArgumentForNull(array, paramName);

            if (array.Length != requiredLength)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resource.Argument_IncorrectArrayLength, array.Length, requiredLength));
            }
        }

        /// <summary>
        /// Checks the array is not null and then ensures the length of the array is between low and high.
        /// </summary>
        /// <typeparam name="T">The type of the array being checked</typeparam>
        /// <param name="array">The array to be checked.</param>
        /// <param name="low">The minimum acceptable length of the array.</param>
        /// <param name="high">The maximum acceptable length of the array.</param>
        /// <param name="paramName">Name of the parameter being checked.</param>
        /// <exception cref="ArgumentNullException">Thrown if array is null.</exception>
        /// <exception cref="ArgumentException">Thrown if array is an incorrect length</exception>
        public static void CheckArrayLength<T>(T[] array, int low, int high, string paramName)
        {
            CheckArgumentForNull(array, paramName);

            if (array.Length < low || array.Length > high)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resource.Argument_IncorrectArrayLengthRange, low, high, array.Length));
            }
        }

        /// <summary>
        /// Checks the array is not null and then ensures the length of the array is at least minimumLength.
        /// </summary>
        /// <typeparam name="T">The type of the array being checked</typeparam>
        /// <param name="array">The array to be checked.</param>
        /// <param name="minimumLength">The minimum acceptable length of the array.</param>
        /// <param name="paramName">Name of the parameter being checked.</param>
        /// <exception cref="ArgumentNullException">Thrown if array is null.</exception>
        /// <exception cref="ArgumentException">Thrown if array is an incorrect length</exception>
        public static void CheckMinimumArrayLength<T>(T[] array, int minimumLength, string paramName)
        {
            CheckArgumentForNull(array, paramName);

            if (array.Length < minimumLength)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resource.Argument_MinimumArrayLength, minimumLength, array.Length));
            }
        }

        /// <summary>
        /// Checks the array is not null and then ensures the length of the array is at least maximumLength.
        /// </summary>
        /// <typeparam name="T">The type of the array being checked</typeparam>
        /// <param name="array">The array to be checked.</param>
        /// <param name="maximumLength">The maximum acceptable length of the array.</param>
        /// <param name="paramName">Name of the parameter being checked.</param>
        /// <exception cref="ArgumentNullException">Thrown if array is null.</exception>
        /// <exception cref="ArgumentException">Thrown if array is an incorrect length</exception>
        public static void CheckMaximumArrayLength<T>(T[] array, int maximumLength, string paramName)
        {
            CheckArgumentForNull(array, paramName);

            if (array.Length > maximumLength)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resource.Argument_MaximumArrayLength, maximumLength, array.Length), paramName);
            }
        }

        /// <summary>
        /// Checks the argument is not null then verifies the type of an argument is correct.
        /// </summary>
        /// <remarks>
        /// true if the current Type is in the inheritance hierarchy of the object 
        /// represented by o, or if the current Type is an interface that o supports. 
        /// false if neither of these conditions is the case
        /// or if the current Type is an open generic type (that is, 
        /// ContainsGenericParameters returns true).
        /// </remarks>
        /// <param name="param">The object that the type is to be checked for.</param>
        /// <param name="desiredType">The type the object should be.</param>
        /// <param name="paramName">Name of the parameter being checked.</param>
        /// <exception cref="ArgumentNullException">Thrown if object is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the object is the incorrect type.</exception>
        public static void CheckArgumentIsCorrectType(object param, Type desiredType, string paramName)
        {
            CheckArgumentForNull(param, paramName);

            if (!desiredType.GetTypeInfo().IsInstanceOfType(param))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resource.Argument_IncorrectType, param.GetType(), desiredType));
            }
        }

        /// <summary>
        /// Checks if the type of a nullable argument is correct.
        /// </summary>
        /// <remarks>
        /// true if the current Type is in the inheritance hierarchy of the object 
        /// represented by o, or if the current Type is an interface that o supports. 
        /// false if neither of these conditions is the case
        /// or if the current Type is an open generic type (that is, 
        /// ContainsGenericParameters returns true).
        /// </remarks>
        /// <param name="param">The object that the type is to be checked for.</param>
        /// <param name="desiredType">The type the object should be.</param>
        /// <exception cref="ArgumentException">Thrown if the object is the incorrect type.</exception>
        public static void CheckNullableArgumentIsCorrectType(object param, Type desiredType)
        {
            if (param == null)
                return;

            if (!desiredType.IsInstanceOfType(param))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resource.Argument_IncorrectType, param.GetType(), desiredType));
            }
        }

        /// <summary>
        /// Checks the argument is greater than or equal to minValue.
        /// </summary>
        /// <param name="arg">The arg to be checked.</param>
        /// <param name="minValue">The minimum allowable value.</param>
        /// <param name="argumentName">Name of the argument being checked.</param>
        /// <exception cref="ArgumentOutOfRangeException">thrown if <paramref name="arg"/> &lt; <paramref name="minValue"/>.</exception>
        public static void CheckArgumentIsGreaterThanOrEqualToValue<T>(T arg, T minValue, string argumentName) where T : IComparable
        {
            if (arg.CompareTo(minValue) < 0)
            {
                throw new ArgumentOutOfRangeException(argumentName, arg, Resource.Argument_MustBeGreaterThanOrEqual);
            }
        }

        /// <summary>
        /// Checks the argument is greater than minValue.
        /// </summary>
        /// <param name="arg">The arg to be checked.</param>
        /// <param name="minValue">The minimum allowable value.</param>
        /// <param name="argumentName">Name of the argument being checked.</param>
        /// <exception cref="ArgumentOutOfRangeException">thrown if <paramref name="arg"/> &lt; <paramref name="minValue"/>.</exception>
        public static void CheckArgumentIsGreaterThanValue<T>(T arg, T minValue, string argumentName) where T : IComparable
        {
            if (arg.CompareTo(minValue) <= 0)
            {
                throw new ArgumentOutOfRangeException(argumentName, arg, Resource.Argument_MustBeGreaterThan);
            }
        }

        /// <summary>
        /// Checks the argument is strictly less than the maximum value provided.
        /// </summary>
        /// <param name="arg">The arg to be checked.</param>
        /// <param name="maxValue">The value that must be strictly greater than <paramref name="arg"/></param>
        /// <param name="argumentName">Name of the argument being checked.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="maxValue"/> is less than or equal to <paramref name="arg"/></exception>
        public static void CheckArgumentIsStrictlyLessThan<T>(T arg, T maxValue, string argumentName) where T : IComparable
        {
            if (maxValue.CompareTo(arg) <= 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        /// <summary>
        /// Checks that the argument value is greater than or equal to the minValue and 
        /// less than or equal to maxValue.
        /// </summary>
        /// <param name="arg">The argument value to check</param>
        /// <param name="minValue">The minimum allowable value</param>
        /// <param name="maxValue">The maximum allowable value</param>
        /// <param name="argumentName">The name of the argument being checked</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if arg &lt; minValue or arg &gt; maxValue</exception>
        public static void CheckArgumentIsInRange(int arg, int minValue, int maxValue, string argumentName)
        {
            if (arg < minValue || arg > maxValue)
                throw new ArgumentOutOfRangeException(argumentName);
        }

        /// <summary>
        /// Checks that the argument value is greater than or equal to the minValue and 
        /// less than or equal to maxValue.
        /// </summary>
        /// <param name="arg">The argument value to check</param>
        /// <param name="minValue">The minimum allowable value</param>
        /// <param name="maxValue">The maximum allowable value</param>
        /// <param name="argumentName">The name of the argument being checked</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if arg &lt; minValue or arg &gt; maxValue</exception>
        public static void CheckArgumentIsInRange(long arg, long minValue, long maxValue, string argumentName)
        {
            if (arg < minValue || arg > maxValue)
                throw new ArgumentOutOfRangeException(argumentName);
        }

        /// <summary>
        /// Checks that the argument value is greater than or equal to the minValue and 
        /// less than or equal to maxValue.
        /// </summary>
        /// <param name="arg">The argument value to check</param>
        /// <param name="minValue">The minimum allowable value</param>
        /// <param name="maxValue">The maximum allowable value</param>
        /// <param name="argumentName">The name of the argument being checked</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if arg &lt; minValue or arg &gt; maxValue</exception>
        public static void CheckArgumentIsInRange(float arg, float minValue, float maxValue, string argumentName)
        {
            if (arg < minValue || arg > maxValue)
                throw new ArgumentOutOfRangeException(argumentName);
        }

        /// <summary>
        /// Checks that the argument value is greater than or equal to the minValue and 
        /// less than or equal to maxValue.
        /// </summary>
        /// <param name="arg">The argument value to check</param>
        /// <param name="minValue">The minimum allowable value</param>
        /// <param name="maxValue">The maximum allowable value</param>
        /// <param name="argumentName">The name of the argument being checked</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if arg &lt; minValue or arg &gt; maxValue, or <paramref name="arg"/> is <c>null</c> (for nullable types only).</exception>
        public static void CheckArgumentIsInRange<T>(T arg, T minValue, T maxValue, string argumentName) where T : IComparable
        {
            if (arg == null || arg.CompareTo(minValue) < 0 || arg.CompareTo(maxValue) > 0)
                throw new ArgumentOutOfRangeException(argumentName);
        }

        /// <summary>
        /// Checks if the argument is a Zero Intptr, raising an <see cref="ArgumentOutOfRangeException"/>
        /// if it is
        /// </summary>
        /// <param name="arg">The argument to test</param>
        /// <param name="argumentName">The name of the argument</param>
        public static void CheckArgumentNotIntPtrZero(IntPtr arg, string argumentName)
        {
            if (IntPtr.Zero == arg)
                throw new ArgumentOutOfRangeException(argumentName);
        }

        /// <summary>
        /// Checks that the value is defined for the given enumeration.  If not a <see cref="ArgumentOutOfRangeException"/> is thrown.
        /// </summary>
        /// <typeparam name="T">The type of the Enum being checked.  This must be a value type.</typeparam>
        /// <param name="value">The value being checked.</param>
        /// <param name="argumentName">The name of the argument to the method.</param>
        public static void CheckEnumValueIsDefined<T>(object value, string argumentName) where T : struct
        {
            var enumType = typeof(T);

            if (!Enum.IsDefined(enumType, value))
                throw new ArgumentOutOfRangeException(
                    argumentName, Resource.Argument_EnumValueNotDefined.FormatInvariantCulture(value, enumType));
        }
    }
}
