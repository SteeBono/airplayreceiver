//-----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace System.Runtime.Serialization.Plists
{
    using System;
    using System.Collections;

    /// <summary>
    /// Extensions and helpers for plist serialization.
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Gets the specified type's concrete type of it is an instance of <see cref="Nullable{T}"/>.
        /// If the type is not null-able, it is returned as-is.
        /// </summary>
        /// <param name="type">The type to get the concrete type of.</param>
        /// <returns>The type's concrete type.</returns>
        public static Type GetConcreteTypeIfNullable(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type", "type cannot be null.");
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return type.GetGenericArguments()[0];
            }

            return type;
        }

        /// <summary>
        /// Gets a value indicating whether the given string is all ASCII.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <returns>True if the string contains only ASCII characters, false otherwise.</returns>
        public static bool IsAscii(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                foreach (char c in value)
                {
                    if (c > 127)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Gets a value indicating whether the specified type is a collection type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is a collection type, false otherwise.</returns>
        public static bool IsCollection(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type", "type cannot be null.");
            }

            return (typeof(Array).IsAssignableFrom(type)
                || typeof(IEnumerable).IsAssignableFrom(type))
                && !typeof(string).IsAssignableFrom(type)
                && !typeof(byte[]).IsAssignableFrom(type);
        }

        /// <summary>
        /// Gets a value indicating whether the given value is the default value for the specified type.
        /// </summary>
        /// <param name="type">The type to check the value against.</param>
        /// <param name="value">The value to check.</param>
        /// <returns>True if the value is the default value, false otherwise.</returns>
        public static bool IsDefaultValue(this Type type, object value)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type", "type cannot be null.");
            }

            TypeCode typeCode = Type.GetTypeCode(type);

            if (typeCode != TypeCode.Empty && typeCode != TypeCode.Object && value == null)
            {
                throw new ArgumentException("Cannot pass a null value when the specified type is non-nullable.", "value");
            }

            if (!type.IsAssignableFrom(value.GetType()))
            {
                throw new ArgumentException("The specified object value is not assignable to the specified type.", "value");
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return (bool)value == false;
                case TypeCode.Byte:
                case TypeCode.Char:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return (long)value == 0;
                case TypeCode.DateTime:
                    return (DateTime)value == DateTime.MinValue;
                case TypeCode.DBNull:
                    return true;
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return (double)value == 0;
                default:
                    return value == null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the specified type is an enum or primitive or semi-primitive (e.g., string) type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is an enum or primitive type, false otherwise.</returns>
        public static bool IsPrimitiveOrEnum(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type", "type cannot be null.");
            }

            bool result = true;

            if (!type.IsEnum
                && Type.GetTypeCode(type) == TypeCode.Object
                && !typeof(Guid).IsAssignableFrom(type)
                && !typeof(TimeSpan).IsAssignableFrom(type)
                && !typeof(byte[]).IsAssignableFrom(type)
                && !typeof(Uri).IsAssignableFrom(type))
            {
                Type concrete = type.GetConcreteTypeIfNullable();

                if (concrete != type)
                {
                    result = IsPrimitiveOrEnum(concrete);
                }
                else
                {
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// Converts the given value into its binary representation as a string.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The value's binary representation as a string.</returns>
        public static string ToBinaryString(this byte value)
        {
            return Convert.ToString(value, 2);
        }

        /// <summary>
        /// Converts the given value into its binary representation as a string.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The value's binary representation as a string.</returns>
        public static string ToBinaryString(this int value)
        {
            return Convert.ToString(value, 2);
        }
    }
}
