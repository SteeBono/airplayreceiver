//-----------------------------------------------------------------------
// <copyright file="BinaryPlistArray.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
//     Inspired by BinaryPListParser.java, copyright (c) 2005 Werner Randelshofer
//          http://www.java2s.com/Open-Source/Java-Document/Swing-Library/jide-common/com/jidesoft/plaf/aqua/BinaryPListParser.java.htm
// </copyright>
//-----------------------------------------------------------------------

namespace System.Runtime.Serialization.Plists
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    /// <summary>
    /// Represents an array value in a binary plist.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    internal class BinaryPlistArray
    {
        /// <summary>
        /// Initializes a new instance of the BinaryPlistArray class.
        /// </summary>
        /// <param name="objectTable">A reference to the binary plist's object table.</param>
        public BinaryPlistArray(IList<BinaryPlistItem> objectTable)
            : this(objectTable, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the BinaryPlistArray class.
        /// </summary>
        /// <param name="objectTable">A reference to the binary plist's object table.</param>
        /// <param name="size">The size of the array.</param>
        public BinaryPlistArray(IList<BinaryPlistItem> objectTable, int size)
        {
            this.ObjectReference = new List<int>(size);
            this.ObjectTable = objectTable;
        }

        /// <summary>
        /// Gets the array's object reference collection.
        /// </summary>
        public IList<int> ObjectReference { get; private set; }

        /// <summary>
        /// Gets a reference to the binary plist's object table.
        /// </summary>
        public IList<BinaryPlistItem> ObjectTable { get; private set; }

        /// <summary>
        /// Converts this instance into an <see cref="T:object[]"/> array.
        /// </summary>
        /// <returns>The <see cref="T:object[]"/> array representation of this instance.</returns>
        public object[] ToArray()
        {
            object[] array = new object[this.ObjectReference.Count];
            int objectRef;
            object objectValue;
            BinaryPlistArray innerArray;
            BinaryPlistDictionary innerDict;
            
            for (int i = 0; i < array.Length; i++)
            {
                objectRef = this.ObjectReference[i];

                if (objectRef >= 0 && objectRef < this.ObjectTable.Count && (this.ObjectTable[objectRef] == null || this.ObjectTable[objectRef].Value != this))
                {
                    objectValue = this.ObjectTable[objectRef] == null ? null : this.ObjectTable[objectRef].Value;
                    innerDict = objectValue as BinaryPlistDictionary;

                    if (innerDict != null)
                    {
                        objectValue = innerDict.ToDictionary();
                    }
                    else
                    {
                        innerArray = objectValue as BinaryPlistArray;

                        if (innerArray != null)
                        {
                            objectValue = innerArray.ToArray();
                        }
                    }

                    array[i] = objectValue;
                }
            }

            return array;
        }

        /// <summary>
        /// Returns the string representation of this instance.
        /// </summary>
        /// <returns>This instance's string representation.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("[");
            int objectRef;

            for (int i = 0; i < this.ObjectReference.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(",");
                }

                objectRef = this.ObjectReference[i];

                if (this.ObjectTable.Count > objectRef && (this.ObjectTable[objectRef] == null || this.ObjectTable[objectRef].Value != this))
                {
                    sb.Append(this.ObjectReference[objectRef]);
                }
                else
                {
                    sb.Append("*" + objectRef);
                }
            }

            return sb.ToString() + "]";
        }
    }
}
