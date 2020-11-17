//-----------------------------------------------------------------------
// <copyright file="EndianConverter.cs" company="Tasty Codes">
//     Modifications copyright (c) 2011 Chad Burggraf.
//     Original copyright (c) 2009 Cor Schols
//     Original source: http://social.msdn.microsoft.com/forums/en-US/csharpgeneral/thread/c878e72e-d42e-417d-b4f6-1935ad96d8ae/
// </copyright>
//-----------------------------------------------------------------------

namespace System.Runtime.Serialization.Plists
{
    using System;

    /// <summary>
    /// Converts the endian-ness of primitive number types.
    /// </summary>
    [CLSCompliant(false)]
    public static class EndianConverter
    {
        /// <summary>
        /// Swaps the endian-ness of the given value.
        /// </summary>
        /// <param name="value">The value to swap the endian-ness of.</param>
        /// <returns>The resulting value.</returns>
        public static short SwapEndian(this short value)
        {
            return SwapInt16(value);
        }

        /// <summary>
        /// Swaps the endian-ness of the given value.
        /// </summary>
        /// <param name="value">The value to swap the endian-ness of.</param>
        /// <returns>The resulting value.</returns>
        public static ushort SwapEndian(this ushort value)
        {
            return SwapUInt16(value);
        }

        /// <summary>
        /// Swaps the endian-ness of the given value.
        /// </summary>
        /// <param name="value">The value to swap the endian-ness of.</param>
        /// <returns>The resulting value.</returns>
        public static int SwapEndian(this int value)
        {
            return SwapInt32(value);
        }

        /// <summary>
        /// Swaps the endian-ness of the given value.
        /// </summary>
        /// <param name="value">The value to swap the endian-ness of.</param>
        /// <returns>The resulting value.</returns>
        public static uint SwapEndian(this uint value)
        {
            return SwapUInt32(value);
        }

        /// <summary>
        /// Swaps the endian-ness of the given value.
        /// </summary>
        /// <param name="value">The value to swap the endian-ness of.</param>
        /// <returns>The resulting value.</returns>
        public static long SwapEndian(this long value)
        {
            return SwapInt64(value);
        }

        /// <summary>
        /// Swaps the endian-ness of the given value.
        /// </summary>
        /// <param name="value">The value to swap the endian-ness of.</param>
        /// <returns>The resulting value.</returns>
        public static ulong SwapEndian(this ulong value)
        {
            return SwapUInt64(value);
        }

        /// <summary>
        /// Swaps the endian-ness of the given value.
        /// </summary>
        /// <param name="value">The value to swap the endian-ness of.</param>
        /// <returns>The resulting value.</returns>
        public static short SwapInt16(short value)
        {
            return (short)(((value & 0xff) << 8) | ((value >> 8) & 0xff));
        }

        /// <summary>
        /// Swaps the endian-ness of the given value.
        /// </summary>
        /// <param name="value">The value to swap the endian-ness of.</param>
        /// <returns>The resulting value.</returns>
        public static int SwapInt32(int value)
        {
            return (int)(((SwapInt16((short)value) & 0xffff) << 0x10) | (SwapInt16((short)(value >> 0x10)) & 0xffff));
        }

        /// <summary>
        /// Swaps the endian-ness of the given value.
        /// </summary>
        /// <param name="value">The value to swap the endian-ness of.</param>
        /// <returns>The resulting value.</returns>
        public static long SwapInt64(long value)
        {
            return (long)(((SwapInt32((int)value) & 0xffffffffL) << 0x20) | (SwapInt32((int)(value >> 0x20)) & 0xffffffffL));
        }

        /// <summary>
        /// Swaps the endian-ness of the given value.
        /// </summary>
        /// <param name="value">The value to swap the endian-ness of.</param>
        /// <returns>The resulting value.</returns>
        public static uint SwapUInt32(uint value)
        {
            return (uint)(((SwapUInt16((ushort)value) & 0xffff) << 0x10) | (SwapUInt16((ushort)(value >> 0x10)) & 0xffff));
        }

        /// <summary>
        /// Swaps the endian-ness of the given value.
        /// </summary>
        /// <param name="value">The value to swap the endian-ness of.</param>
        /// <returns>The resulting value.</returns>
        public static ushort SwapUInt16(ushort value)
        {
            return (ushort)(((value & 0xff) << 8) | ((value >> 8) & 0xff));
        }

        /// <summary>
        /// Swaps the endian-ness of the given value.
        /// </summary>
        /// <param name="value">The value to swap the endian-ness of.</param>
        /// <returns>The resulting value.</returns>
        public static ulong SwapUInt64(ulong value)
        {
            return (ulong)(((SwapUInt32((uint)value) & 0xffffffffL) << 0x20) | (SwapUInt32((uint)(value >> 0x20)) & 0xffffffffL));
        }

        /// <summary>
        /// Gets the big-endian value of the given value if the current system is little-endian.
        /// If the current system is big-endian, returns the value as-is.
        /// </summary>
        /// <param name="value">The value to swap if necessary.</param>
        /// <returns>The resulting value.</returns>
        public static ushort ToBigEndianConditional(this ushort value)
        {
            if (BitConverter.IsLittleEndian)
            {
                return value.SwapEndian();
            }

            return value;
        }

        /// <summary>
        /// Gets the big-endian value of the given value if the current system is little-endian.
        /// If the current system is big-endian, returns the value as-is.
        /// </summary>
        /// <param name="value">The value to swap if necessary.</param>
        /// <returns>The resulting value.</returns>
        public static short ToBigEndianConditional(this short value)
        {
            if (BitConverter.IsLittleEndian)
            {
                return value.SwapEndian();
            }

            return value;
        }

        /// <summary>
        /// Gets the big-endian value of the given value if the current system is little-endian.
        /// If the current system is big-endian, returns the value as-is.
        /// </summary>
        /// <param name="value">The value to swap if necessary.</param>
        /// <returns>The resulting value.</returns>
        public static uint ToBigEndianConditional(this uint value)
        {
            if (BitConverter.IsLittleEndian)
            {
                return value.SwapEndian();
            }

            return value;
        }

        /// <summary>
        /// Gets the big-endian value of the given value if the current system is little-endian.
        /// If the current system is big-endian, returns the value as-is.
        /// </summary>
        /// <param name="value">The value to swap if necessary.</param>
        /// <returns>The resulting value.</returns>
        public static int ToBigEndianConditional(this int value)
        {
            if (BitConverter.IsLittleEndian)
            {
                return value.SwapEndian();
            }

            return value;
        }

        /// <summary>
        /// Gets the big-endian value of the given value if the current system is little-endian.
        /// If the current system is big-endian, returns the value as-is.
        /// </summary>
        /// <param name="value">The value to swap if necessary.</param>
        /// <returns>The resulting value.</returns>
        public static ulong ToBigEndianConditional(this ulong value)
        {
            if (BitConverter.IsLittleEndian)
            {
                return value.SwapEndian();
            }

            return value;
        }

        /// <summary>
        /// Gets the big-endian value of the given value if the current system is little-endian.
        /// If the current system is big-endian, returns the value as-is.
        /// </summary>
        /// <param name="value">The value to swap if necessary.</param>
        /// <returns>The resulting value.</returns>
        public static long ToBigEndianConditional(this long value)
        {
            if (BitConverter.IsLittleEndian)
            {
                return value.SwapEndian();
            }

            return value;
        }

        /// <summary>
        /// Gets the little-endian value of the given value if the current system is big-endian.
        /// If the current system is little-endian, returns the value as-is.
        /// </summary>
        /// <param name="value">The value to swap if necessary.</param>
        /// <returns>The resulting value.</returns>
        public static ushort ToLittleEndianConditional(this ushort value)
        {
            if (BitConverter.IsLittleEndian)
            {
                return value;
            }

            return value.SwapEndian();
        }

        /// <summary>
        /// Gets the little-endian value of the given value if the current system is big-endian.
        /// If the current system is little-endian, returns the value as-is.
        /// </summary>
        /// <param name="value">The value to swap if necessary.</param>
        /// <returns>The resulting value.</returns>
        public static short ToLittleEndianConditional(this short value)
        {
            if (BitConverter.IsLittleEndian)
            {
                return value;
            }

            return value.SwapEndian();
        }

        /// <summary>
        /// Gets the little-endian value of the given value if the current system is big-endian.
        /// If the current system is little-endian, returns the value as-is.
        /// </summary>
        /// <param name="value">The value to swap if necessary.</param>
        /// <returns>The resulting value.</returns>
        public static uint ToLittleEndianConditional(this uint value)
        {
            if (BitConverter.IsLittleEndian)
            {
                return value;
            }

            return value.SwapEndian();
        }

        /// <summary>
        /// Gets the little-endian value of the given value if the current system is big-endian.
        /// If the current system is little-endian, returns the value as-is.
        /// </summary>
        /// <param name="value">The value to swap if necessary.</param>
        /// <returns>The resulting value.</returns>
        public static int ToLittleEndianConditional(this int value)
        {
            if (BitConverter.IsLittleEndian)
            {
                return value;
            }

            return value.SwapEndian();
        }

        /// <summary>
        /// Gets the little-endian value of the given value if the current system is big-endian.
        /// If the current system is little-endian, returns the value as-is.
        /// </summary>
        /// <param name="value">The value to swap if necessary.</param>
        /// <returns>The resulting value.</returns>
        public static ulong ToLittleEndianConditional(this ulong value)
        {
            if (BitConverter.IsLittleEndian)
            {
                return value;
            }

            return value.SwapEndian();
        }

        /// <summary>
        /// Gets the little-endian value of the given value if the current system is big-endian.
        /// If the current system is little-endian, returns the value as-is.
        /// </summary>
        /// <param name="value">The value to swap if necessary.</param>
        /// <returns>The resulting value.</returns>
        public static long ToLittleEndianConditional(this long value)
        {
            if (BitConverter.IsLittleEndian)
            {
                return value;
            }

            return value.SwapEndian();
        }
    }
}