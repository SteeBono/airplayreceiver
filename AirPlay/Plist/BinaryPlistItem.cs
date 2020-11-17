//-----------------------------------------------------------------------
// <copyright file="BinaryPlistItem.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace System.Runtime.Serialization.Plists
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents an item in a binary plist's object table.
    /// </summary>
    internal class BinaryPlistItem
    {
        private List<byte> byteValue, marker;

        /// <summary>
        /// Initializes a new instance of the BinaryPlistItem class.
        /// </summary>
        public BinaryPlistItem()
        {
            this.byteValue = new List<byte>();
            this.marker = new List<byte>();
        }

        /// <summary>
        /// Initializes a new instance of the BinaryPlistItem class.
        /// </summary>
        /// <param name="value">The value of the object the item represents.</param>
        public BinaryPlistItem(object value)
            : this()
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets the item's byte value collection.
        /// </summary>
        public IList<byte> ByteValue
        {
            get { return this.byteValue; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this item represents an array.
        /// </summary>
        public bool IsArray { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this item represents a dictionary.
        /// </summary>
        public bool IsDictionary { get; set; }

        /// <summary>
        /// Gets the item's marker value collection.
        /// </summary>
        public IList<byte> Marker
        {
            get { return this.marker; }
        }

        /// <summary>
        /// Gets the item's size, which is a sum of the <see cref="Marker"/> and <see cref="ByteValue"/> lengths.
        /// </summary>
        public int Size
        {
            get { return this.Marker.Count + this.ByteValue.Count; }
        }

        /// <summary>
        /// Gets or sets the object value this item represents.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Sets the <see cref="ByteValue"/> to the given collection.
        /// </summary>
        /// <param name="buffer">The collection to set.</param>
        public void SetByteValue(IEnumerable<byte> buffer)
        {
            this.byteValue.Clear();

            if (buffer != null)
            {
                this.byteValue.AddRange(buffer);
            }
        }

        /// <summary>
        /// Gets the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString()
        {
            return this.Value != null ? this.Value.ToString() : "null";
        }
    }
}