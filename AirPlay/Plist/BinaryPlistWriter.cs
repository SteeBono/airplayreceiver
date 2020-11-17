//-----------------------------------------------------------------------
// <copyright file="BinaryPlistWriter.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace System.Runtime.Serialization.Plists
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;

    /// <summary>
    /// Performs serialization of objects into binary plist format.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public sealed class BinaryPlistWriter
    {
        #region Internal Fields

        /// <summary>
        /// Gets the magic number value used in a binary plist header.
        /// </summary>
        internal const uint HeaderMagicNumber = 0x62706c69;

        /// <summary>
        /// Gets the version number value used in a binary plist header.
        /// </summary>
        internal const uint HeaderVersionNumber = 0x73743030;

        /// <summary>
        /// Gets Apple's reference date value.
        /// </summary>
        internal static readonly DateTime ReferenceDate = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        #endregion

        #region Private Fields

        private List<BinaryPlistItem> objectTable;
        private List<long> offsetTable;
        private UniqueValueCache uniques;
        private int objectTableSize, objectRefCount, objectRefSize, topLevelObjectOffset;
        private long maxObjectRefValue;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the BinaryPlistWriter class.
        /// </summary>
        public BinaryPlistWriter()
        {
        }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Writes the specified <see cref="IPlistSerializable"/> object to the given file path as a binary plist.
        /// </summary>
        /// <param name="path">The file path to write to.</param>
        /// <param name="obj">The <see cref="IPlistSerializable"/> object to write.</param>
        public void WriteObject(string path, IPlistSerializable obj)
        {
            using (FileStream stream = File.Create(path))
            {
                this.WriteObject(stream, obj);
            }
        }

        /// <summary>
        /// Writes the specified <see cref="IPlistSerializable"/> object to the given stream as a binary plist.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="obj">The <see cref="IPlistSerializable"/> object to write.</param>
        public void WriteObject(Stream stream, IPlistSerializable obj)
        {
            this.WriteObject(stream, obj, true);
        }

        /// <summary>
        /// Writes the specified <see cref="IPlistSerializable"/> object to the given stream as a binary plist.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="obj">The <see cref="IPlistSerializable"/> object to write.</param>
        /// <param name="closeStream">A value indicating whether to close the stream after the write operation completes.</param>
        public void WriteObject(Stream stream, IPlistSerializable obj, bool closeStream)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj", "obj cannot be null.");
            }

            this.WriteObject(stream, obj.ToPlistDictionary(), closeStream);
        }

        /// <summary>
        /// Writes the specified <see cref="IDictionary"/> object to the given file path as a binary plist.
        /// </summary>
        /// <param name="path">The file path to write to.</param>
        /// <param name="dictionary">The <see cref="IDictionary"/> object to write.</param>
        public void WriteObject(string path, IDictionary dictionary)
        {
            using (FileStream stream = File.Create(path))
            {
                this.WriteObject(stream, dictionary);
            }
        }

        /// <summary>
        /// Writes the specified <see cref="IDictionary"/> object to the given stream as a binary plist.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="dictionary">The <see cref="IDictionary"/> object to write.</param>
        public void WriteObject(Stream stream, IDictionary dictionary)
        {
            this.WriteObject(stream, dictionary, true);
        }

        /// <summary>
        /// Writes the specified <see cref="IDictionary"/> object to the given stream as a binary plist.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="dictionary">The <see cref="IDictionary"/> object to write.</param>
        /// <param name="closeStream">A value indicating whether to close the stream after the write operation completes.</param>
        public void WriteObject(Stream stream, IDictionary dictionary, bool closeStream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream", "stream cannot be null.");
            }

            if (!stream.CanWrite)
            {
                throw new ArgumentException("The stream must be writable.", "stream");
            }

            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary", "dictionary cannot be null.");
            }

            // Reset the state and then build the object table.
            this.Reset();
            this.AddDictionary(dictionary);

            this.topLevelObjectOffset = 8;
            this.CalculateObjectRefSize();

            BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII);

            try
            {
                // Write the header.
                writer.Write(HeaderMagicNumber.ToBigEndianConditional());
                writer.Write(HeaderVersionNumber.ToBigEndianConditional());

                // Write the object table.
                long offsetTableOffset = this.topLevelObjectOffset + this.WriteObjectTable(writer);

                // Write the offset table.
                foreach (int offset in this.offsetTable)
                {
                    WriteReferenceInteger(writer, offset, this.objectRefSize);
                }

                // Write the trailer.
                writer.Write(new byte[6], 0, 6);
                writer.Write((byte)this.objectRefSize);
                writer.Write((byte)this.objectRefSize);
                writer.Write(((long)this.objectTable.Count).ToBigEndianConditional());
                writer.Write((long)0);
                writer.Write(offsetTableOffset.ToBigEndianConditional());
            }
            finally
            {
                writer.Flush();

                if (closeStream)
                {
                    writer.Close();
                }
            }
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Adds an integer count to the given buffer.
        /// </summary>
        /// <param name="buffer">The buffer to add the integer count to.</param>
        /// <param name="count">A count value to write.</param>
        private static void AddIntegerCount(IList<byte> buffer, int count)
        {
            byte[] countBuffer = GetIntegerBytes(count);

            // According to my inspection of the output of Property List Editor's .plist files,
            // it is marking the most significant bit for some unknown reason. So we're marking it too.
            buffer.Add((byte)((byte)Math.Log(countBuffer.Length, 2) | (byte)0x10));

            foreach (byte countByte in countBuffer)
            {
                buffer.Add(countByte);
            }
        }

        /// <summary>
        /// Gets a big-endian byte array that corresponds to the given integer value.
        /// </summary>
        /// <param name="value">The integer value to get bytes for.</param>
        /// <returns>A big-endian byte array.</returns>
        private static byte[] GetIntegerBytes(long value)
        {
            // See AddIntegerCount() for why this is restricting use
            // of the most significant bit.
            if (value >= 0 && value < 128)
            {
                return new byte[] { (byte)value };
            }
            else if (value >= short.MinValue && value <= short.MaxValue)
            {
                return BitConverter.GetBytes(((short)value).ToBigEndianConditional());
            }
            else if (value >= int.MinValue && value <= int.MaxValue)
            {
                return BitConverter.GetBytes(((int)value).ToBigEndianConditional());
            }
            else
            {
                return BitConverter.GetBytes(value.ToBigEndianConditional());
            }
        }

        /// <summary>
        /// Writes the given value using the number of bytes indicated by the specified size
        /// to the given <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="size">The size of the integer to write.</param>
        /// <returns>The number of bytes written.</returns>
        private static int WriteReferenceInteger(BinaryWriter writer, long value, int size)
        {
            byte[] buffer;

            switch (size)
            {
                case 1:
                    buffer = new byte[] { (byte)value };
                    break;
                case 2:
                    buffer = BitConverter.GetBytes(((short)value).ToBigEndianConditional());
                    break;
                case 4:
                    buffer = BitConverter.GetBytes(((int)value).ToBigEndianConditional());
                    break;
                case 8:
                    buffer = BitConverter.GetBytes(value.ToBigEndianConditional());
                    break;
                default:
                    throw new ArgumentException("The reference size must be one of 1, 2, 4 or 8. The specified reference size was: " + size, "size");
            }

            writer.Write(buffer, 0, buffer.Length);
            return buffer.Length;
        }

        #endregion

        #region Private Instance Methods

        /// <summary>
        /// Adds an array to the internal object table.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>The index of the added value.</returns>
        private int AddArray(IEnumerable value)
        {
            int index = this.objectTable.Count;

            BinaryPlistArray array = new BinaryPlistArray(this.objectTable);
            BinaryPlistItem item = new BinaryPlistItem(array);
            item.IsArray = true;
            this.objectTable.Add(item);

            foreach (object obj in value)
            {
                array.ObjectReference.Add(this.AddObject(obj));
                this.objectRefCount++;
            }

            if (array.ObjectReference.Count < 15)
            {
                item.Marker.Add((byte)((byte)0xA0 | (byte)array.ObjectReference.Count));
            }
            else
            {
                item.Marker.Add((byte)0xAF);
                AddIntegerCount(item.Marker, array.ObjectReference.Count);
            }

            this.objectTableSize += item.Size;
            return index;
        }

        /// <summary>
        /// Adds arbitrary data to the internal object table.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>The index of the added value.</returns>
        private int AddData(object value)
        {
            int index = this.objectTable.Count, count = 0, bufferIndex = 0;
            byte[] buffer = value as byte[];

            if (buffer == null)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, value);

                    stream.Position = 0;
                    buffer = new byte[stream.Length];

                    while (0 < (count = stream.Read(buffer, 0, buffer.Length - bufferIndex)))
                    {
                        bufferIndex += count;
                    }
                }
            }

            BinaryPlistItem item = new BinaryPlistItem(value);
            item.SetByteValue(buffer);

            if (buffer.Length < 15)
            {
                item.Marker.Add((byte)((byte)0x40 | (byte)buffer.Length));
            }
            else
            {
                item.Marker.Add(0x4F);
                AddIntegerCount(item.Marker, buffer.Length);
            }

            this.objectTable.Add(item);
            this.objectTableSize += item.Size;

            return index;
        }

        /// <summary>
        /// Adds a date to the internal object table.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>The index of the added value.</returns>
        private int AddDate(DateTime value)
        {
            if (!this.uniques.Contains(value))
            {
                int index = this.objectTable.Count;
                byte[] buffer = BitConverter.GetBytes(value.ToUniversalTime().Subtract(ReferenceDate).TotalSeconds);

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(buffer);
                }

                BinaryPlistItem item = new BinaryPlistItem(value);
                item.Marker.Add((byte)0x33);
                item.SetByteValue(buffer);

                this.objectTable.Add(item);
                this.objectTableSize += item.Size;

                this.uniques.SetIndex(value, index);
                return index;
            }

            return this.uniques.GetIndex(value);
        }

        /// <summary>
        /// Adds a dictionary to the internal object table.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>The index of the added value.</returns>
        private int AddDictionary(IDictionary value)
        {
            int index = this.objectTable.Count;

            BinaryPlistDictionary dict = new BinaryPlistDictionary(this.objectTable, value.Count);
            BinaryPlistItem item = new BinaryPlistItem(dict);
            item.IsDictionary = true;
            this.objectTable.Add(item);

            foreach (object key in value.Keys)
            {
                dict.KeyReference.Add(this.AddObject(key));
                dict.ObjectReference.Add(this.AddObject(value[key]));

                this.objectRefCount += 2;
            }

            if (dict.KeyReference.Count < 15)
            {
                item.Marker.Add((byte)((byte)0xD0 | (byte)dict.KeyReference.Count));
            }
            else
            {
                item.Marker.Add((byte)0xDF);
                AddIntegerCount(item.Marker, dict.KeyReference.Count);
            }

            this.objectTableSize += item.Size;
            return index;
        }

        /// <summary>
        /// Adds a double to the internal object table.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>The index of the added value.</returns>
        private int AddDouble(double value)
        {
            if (!this.uniques.Contains(value))
            {
                int index = this.objectTable.Count;
                byte[] buffer = BitConverter.GetBytes(value);

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(buffer);
                }

                BinaryPlistItem item = new BinaryPlistItem(value);
                item.Marker.Add((byte)((byte)0x20 | (byte)Math.Log(buffer.Length, 2)));
                item.SetByteValue(buffer);

                this.objectTable.Add(item);
                this.objectTableSize += item.Size;

                this.uniques.SetIndex(value, index);
                return index;
            }

            return this.uniques.GetIndex(value);
        }

        /// <summary>
        /// Adds a float to the internal object table.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>The index of the added value.</returns>
        private int AddFloat(float value)
        {
            if (!this.uniques.Contains(value))
            {
                int index = this.objectTable.Count;
                byte[] buffer = BitConverter.GetBytes(value);

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(buffer);
                }

                BinaryPlistItem item = new BinaryPlistItem(value);
                item.Marker.Add((byte)((byte)0x20 | (byte)Math.Log(buffer.Length, 2)));
                item.SetByteValue(buffer);

                this.objectTable.Add(item);
                this.objectTableSize += item.Size;

                this.uniques.SetIndex(value, index);
                return index;
            }

            return this.uniques.GetIndex(value);
        }

        /// <summary>
        /// Adds an integer to the internal object table.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>The index of the added value.</returns>
        private int AddInteger(long value)
        {
            if (!this.uniques.Contains(value))
            {
                int index = this.objectTable.Count;

                BinaryPlistItem item = new BinaryPlistItem(value);
                item.SetByteValue(GetIntegerBytes(value));
                item.Marker.Add((byte)((byte)0x10 | (byte)Math.Log(item.ByteValue.Count, 2)));

                this.objectTable.Add(item);
                this.objectTableSize += item.Size;

                this.uniques.SetIndex(value, index);
                return index;
            }

            return this.uniques.GetIndex(value);
        }

        /// <summary>
        /// Adds an object to the internal object table.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>The index of the added value.</returns>
        private int AddObject(object value)
        {
            int index = this.objectTable.Count;
            Type type = null;
            TypeCode typeCode = TypeCode.Empty;

            if (value != null)
            {
                type = value.GetType().GetConcreteTypeIfNullable();
                typeCode = Type.GetTypeCode(type);
            }
            
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    index = this.AddPrimitive((bool)value);
                    break;
                case TypeCode.Byte:
                    index = this.AddInteger((long)(byte)value);
                    break;
                case TypeCode.Char:
                    index = this.AddInteger((long)(char)value);
                    break;
                case TypeCode.DateTime:
                    index = this.AddDate((DateTime)value);
                    break;
                case TypeCode.DBNull:
                    index = this.AddPrimitive(null);
                    break;
                case TypeCode.Decimal:
                    index = this.AddDouble((double)(decimal)value);
                    break;
                case TypeCode.Double:
                    index = this.AddDouble((double)value);
                    break;
                case TypeCode.Empty:
                    index = this.AddPrimitive(null);
                    break;
                case TypeCode.Int16:
                    index = this.AddInteger((long)(short)value);
                    break;
                case TypeCode.Int32:
                    index = this.AddInteger((long)(int)value);
                    break;
                case TypeCode.Int64:
                    index = this.AddInteger((long)value);
                    break;
                case TypeCode.SByte:
                    index = this.AddInteger((long)(sbyte)value);
                    break;
                case TypeCode.Single:
                    index = this.AddFloat((float)value);
                    break;
                case TypeCode.String:
                    index = this.AddString((string)value);
                    break;
                case TypeCode.UInt16:
                    index = this.AddInteger((long)(ushort)value);
                    break;
                case TypeCode.UInt32:
                    index = this.AddInteger((long)(uint)value);
                    break;
                case TypeCode.UInt64:
                    throw new InvalidOperationException("UInt64 cannot be written to a binary plist. Please use Int64 instead. If your value cannot fit into an Int64, consider separating it into two UInt32 values.");
                default:
                    if (type.IsEnum)
                    {
                        index = this.AddInteger((int)value);
                    }
                    else if (typeof(IPlistSerializable).IsAssignableFrom(type))
                    {
                        index = this.AddDictionary(((IPlistSerializable)value).ToPlistDictionary());
                    }
                    else if (typeof(IDictionary).IsAssignableFrom(type))
                    {
                        index = this.AddDictionary(value as IDictionary);
                    }
                    else if ((typeof(Array).IsAssignableFrom(type)
                        || typeof(IEnumerable).IsAssignableFrom(type))
                        && !typeof(string).IsAssignableFrom(type)
                        && !typeof(byte[]).IsAssignableFrom(type))
                    {
                        index = this.AddArray(value as IEnumerable);
                    }
                    else if (typeof(byte[]).IsAssignableFrom(type) 
                        || typeof(ISerializable).IsAssignableFrom(type) 
                        || type.IsSerializable)
                    {
                        index = this.AddData(value);
                    }
                    else
                    {
                        throw new InvalidOperationException("A type was found in the object table that is not serializable. Types that are natively serializable to a binary plist include: null, booleans, integers, floats, dates, strings, arrays and dictionaries. Any other types must be marked with a SerializableAttribute or implement ISerializable. The type that caused this exception to be thrown is: " + type.FullName);
                    }

                    break;
            }

            return index;
        }

        /// <summary>
        /// Adds a primitive to the internal object table.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>The index of the added value.</returns>
        private int AddPrimitive(bool? value)
        {
            if (!value.HasValue || !this.uniques.Contains(value.Value))
            {
                int index = this.objectTable.Count;

                BinaryPlistItem item = new BinaryPlistItem(value);
                item.Marker.Add(value.HasValue ? (value.Value ? (byte)0x9 : (byte)0x8) : (byte)0);

                this.objectTable.Add(item);
                this.objectTableSize += item.Size;

                if (value.HasValue)
                {
                    this.uniques.SetIndex(value.Value, index);
                }

                return index;
            }

            return this.uniques.GetIndex(value.Value);
        }

        /// <summary>
        /// Adds a string to the internal object table.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>The index of the added value.</returns>
        private int AddString(string value)
        {
            if (!this.uniques.Contains(value))
            {
                int index = this.objectTable.Count;
                bool ascii = value.IsAscii();
                byte[] buffer;

                BinaryPlistItem item = new BinaryPlistItem(value);

                if (value.Length < 15)
                {
                    item.Marker.Add((byte)((byte)(ascii ? 0x50 : 0x60) | (byte)value.Length));
                }
                else
                {
                    item.Marker.Add((byte)(ascii ? 0x5F : 0x6F));
                    AddIntegerCount(item.Marker, value.Length);
                }

                if (ascii)
                {
                    buffer = Encoding.ASCII.GetBytes(value);
                }
                else
                {
                    buffer = Encoding.Unicode.GetBytes(value);

                    if (BitConverter.IsLittleEndian)
                    {
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            byte l = buffer[i];
                            buffer[i] = buffer[++i];
                            buffer[i] = l;
                        }
                    }
                }

                item.SetByteValue(buffer);

                this.objectTable.Add(item);
                this.objectTableSize += item.Size;

                this.uniques.SetIndex(value, index);
                return index;
            }

            return this.uniques.GetIndex(value);
        }

        /// <summary>
        /// Calculates the object ref size to use for this instance's current state.
        /// </summary>
        private void CalculateObjectRefSize()
        {
            while (this.objectTableSize + this.topLevelObjectOffset + (this.objectRefCount * this.objectRefSize) > this.maxObjectRefValue)
            {
                switch (this.objectRefSize)
                {
                    case 1:
                        this.objectRefSize = 2;
                        this.maxObjectRefValue = short.MaxValue;
                        break;
                    case 2:
                        this.objectRefSize = 4;
                        this.maxObjectRefValue = int.MaxValue;
                        break;
                    case 4:
                        this.objectRefSize = 8;
                        this.maxObjectRefValue = long.MaxValue;
                        break;
                    case 8:
                        break;
                    default:
                        throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Failed to calculate the required object reference size with an object table size of {0} and an object reference count of {1}.", this.objectTableSize, this.objectRefCount));
                }
            }
        }

        /// <summary>
        /// Resets this instance's state.
        /// </summary>
        private void Reset()
        {
            this.objectTableSize =
            this.objectRefCount =
            this.objectRefSize =
            this.topLevelObjectOffset = 0;

            this.objectRefSize = 1;
            this.maxObjectRefValue = 255;

            this.objectTable = new List<BinaryPlistItem>();
            this.offsetTable = new List<long>();
            this.uniques = new UniqueValueCache();
        }

        /// <summary>
        /// Writes an array item to the given <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
        /// <param name="value">The array item to write.</param>
        /// <returns>The number of bytes written.</returns>
        private int WriteArray(BinaryWriter writer, BinaryPlistItem value)
        {
            int size = value.Marker.Count;
            BinaryPlistArray array = (BinaryPlistArray)value.Value;

            writer.Write(value.Marker.ToArray());

            foreach (int objectRef in array.ObjectReference)
            {
                size += WriteReferenceInteger(writer, objectRef, this.objectRefSize);
            }

            return size;
        }

        /// <summary>
        /// Writes a dictionary item to the given <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
        /// <param name="value">The dictionary item to write.</param>
        /// <returns>The number of bytes written.</returns>
        private int WriteDictionary(BinaryWriter writer, BinaryPlistItem value)
        {
            int size = value.Marker.Count;
            BinaryPlistDictionary dict = (BinaryPlistDictionary)value.Value;

            writer.Write(value.Marker.ToArray());

            foreach (int keyRef in dict.KeyReference)
            {
                size += WriteReferenceInteger(writer, keyRef, this.objectRefSize);
            }

            foreach (int objectRef in dict.ObjectReference)
            {
                size += WriteReferenceInteger(writer, objectRef, this.objectRefSize);
            }

            return size;
        }

        /// <summary>
        /// Write the object table to the given <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
        /// <returns>The number of bytes written.</returns>
        private int WriteObjectTable(BinaryWriter writer)
        {
            int offset = this.topLevelObjectOffset;

            foreach (BinaryPlistItem item in this.objectTable)
            {
                this.offsetTable.Add(offset);

                if (item.IsArray)
                {
                    offset += this.WriteArray(writer, item);
                }
                else if (item.IsDictionary)
                {
                    offset += this.WriteDictionary(writer, item);
                }
                else
                {
                    writer.Write(item.Marker.ToArray());

                    if (item.ByteValue.Count > 0)
                    {
                        writer.Write(item.ByteValue.ToArray());
                    }

                    offset += item.Size;
                }
            }

            return offset - this.topLevelObjectOffset;
        }

        #endregion
    }
}