//-----------------------------------------------------------------------
// <copyright file="BinaryPlistDictionary.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
//     Inspired by BinaryPListParser.java, copyright (c) 2005 Werner Randelshofer
//          http://www.java2s.com/Open-Source/Java-Document/Swing-Library/jide-common/com/jidesoft/plaf/aqua/BinaryPListParser.java.htm
// </copyright>
//-----------------------------------------------------------------------

namespace System.Runtime.Serialization.Plists
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    /// <summary>
    /// Represents a dictionary in a binary plist.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    internal class BinaryPlistDictionary
    {
        /// <summary>
        /// Initializes a new instance of the BinaryPlistDictionary class.
        /// </summary>
        /// <param name="objectTable">A reference to the binary plist's object table.</param>
        /// <param name="size">The size of the dictionary.</param>
        public BinaryPlistDictionary(IList<BinaryPlistItem> objectTable, int size)
        {
            this.KeyReference = new List<int>(size);
            this.ObjectReference = new List<int>(size);
            this.ObjectTable = objectTable;
        }

        /// <summary>
        /// Gets the dictionary's key reference collection.
        /// </summary>
        public IList<int> KeyReference { get; private set; }

        /// <summary>
        /// Gets the dictionary's object reference collection.
        /// </summary>
        public IList<int> ObjectReference { get; private set; }

        /// <summary>
        /// Gets a reference to the binary plist's object table.
        /// </summary>
        public IList<BinaryPlistItem> ObjectTable { get; private set; }

        /// <summary>
        /// Converts this instance into a <see cref="Dictionary{Object, Object}"/>.
        /// </summary>
        /// <returns>A <see cref="Dictionary{Object, Object}"/> representation this instance.</returns>
        public Dictionary<object, object> ToDictionary()
        {
            Dictionary<object, object> dictionary = new Dictionary<object, object>();
            int keyRef, objectRef;
            object keyValue, objectValue;
            BinaryPlistArray innerArray;
            BinaryPlistDictionary innerDict;

            for (int i = 0; i < this.KeyReference.Count; i++)
            {
                keyRef = this.KeyReference[i];
                objectRef = this.ObjectReference[i];

                if (keyRef >= 0 && keyRef < this.ObjectTable.Count && (this.ObjectTable[keyRef] == null || this.ObjectTable[keyRef].Value != this) &&
                    objectRef >= 0 && objectRef < this.ObjectTable.Count && (this.ObjectTable[objectRef] == null || this.ObjectTable[objectRef].Value != this))
                {
                    keyValue = this.ObjectTable[keyRef] == null ? null : this.ObjectTable[keyRef].Value;
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

                    dictionary[keyValue] = objectValue;
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Returns the string representation of this instance.
        /// </summary>
        /// <returns>This instance's string representation.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("{");
            int keyRef, objectRef;

            for (int i = 0; i < this.KeyReference.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(",");
                }

                keyRef = this.KeyReference[i];
                objectRef = this.ObjectReference[i];

                if (keyRef < 0 || keyRef >= this.ObjectTable.Count)
                {
                    sb.Append("#" + keyRef);
                }
                else if (this.ObjectTable[keyRef] != null && this.ObjectTable[keyRef].Value == this)
                {
                    sb.Append("*" + keyRef);
                }
                else
                {
                    sb.Append(this.ObjectTable[keyRef]);
                }

                sb.Append(":");

                if (objectRef < 0 || objectRef >= this.ObjectTable.Count)
                {
                    sb.Append("#" + objectRef);
                }
                else if (this.ObjectTable[objectRef] != null && this.ObjectTable[objectRef].Value == this)
                {
                    sb.Append("*" + objectRef);
                }
                else
                {
                    sb.Append(this.ObjectTable[objectRef]);
                }
            }

            return sb.ToString() + "}";
        }
    }
}
