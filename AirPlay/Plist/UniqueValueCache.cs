//-----------------------------------------------------------------------
// <copyright file="UniqueValueCache.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace System.Runtime.Serialization.Plists
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides a cache of unique primitive values when writing a binary plist.
    /// </summary>
    internal sealed class UniqueValueCache
    {
        private Dictionary<bool, int> booleans = new Dictionary<bool, int>();
        private Dictionary<long, int> integers = new Dictionary<long, int>();
        private Dictionary<float, int> floats = new Dictionary<float, int>();
        private Dictionary<double, int> doubles = new Dictionary<double, int>();
        private Dictionary<DateTime, int> dates = new Dictionary<DateTime, int>();
        private Dictionary<string, int> strings = new Dictionary<string, int>();

        /// <summary>
        /// Gets a value indicating whether the cache contains the given value.
        /// </summary>
        /// <param name="value">The value to check for.</param>
        /// <returns>True if the cache contains the value, false otherwise.</returns>
        public bool Contains(bool value)
        {
            return this.booleans.ContainsKey(value);
        }

        /// <summary>
        /// Gets a value indicating whether the cache contains the given value.
        /// </summary>
        /// <param name="value">The value to check for.</param>
        /// <returns>True if the cache contains the value, false otherwise.</returns>
        public bool Contains(long value)
        {
            return this.integers.ContainsKey(value);
        }

        /// <summary>
        /// Gets a value indicating whether the cache contains the given value.
        /// </summary>
        /// <param name="value">The value to check for.</param>
        /// <returns>True if the cache contains the value, false otherwise.</returns>
        public bool Contains(float value)
        {
            return this.floats.ContainsKey(value);
        }

        /// <summary>
        /// Gets a value indicating whether the cache contains the given value.
        /// </summary>
        /// <param name="value">The value to check for.</param>
        /// <returns>True if the cache contains the value, false otherwise.</returns>
        public bool Contains(double value)
        {
            return this.doubles.ContainsKey(value);
        }

        /// <summary>
        /// Gets a value indicating whether the cache contains the given value.
        /// </summary>
        /// <param name="value">The value to check for.</param>
        /// <returns>True if the cache contains the value, false otherwise.</returns>
        public bool Contains(DateTime value)
        {
            return this.dates.ContainsKey(value);
        }

        /// <summary>
        /// Gets a value indicating whether the cache contains the given value.
        /// </summary>
        /// <param name="value">The value to check for.</param>
        /// <returns>True if the cache contains the value, false otherwise.</returns>
        public bool Contains(string value)
        {
            return this.strings.ContainsKey(value);
        }

        /// <summary>
        /// Gets the index in the object table for the given value, assuming it has already been added to the cache.
        /// </summary>
        /// <param name="value">The value to get the index of.</param>
        /// <returns>The index of the value.</returns>
        public int GetIndex(bool value)
        {
            return this.booleans[value];
        }

        /// <summary>
        /// Gets the index in the object table for the given value, assuming it has already been added to the cache.
        /// </summary>
        /// <param name="value">The value to get the index of.</param>
        /// <returns>The index of the value.</returns>
        public int GetIndex(long value)
        {
            return this.integers[value];
        }

        /// <summary>
        /// Gets the index in the object table for the given value, assuming it has already been added to the cache.
        /// </summary>
        /// <param name="value">The value to get the index of.</param>
        /// <returns>The index of the value.</returns>
        public int GetIndex(float value) 
        {
            return this.floats[value];
        }

        /// <summary>
        /// Gets the index in the object table for the given value, assuming it has already been added to the cache.
        /// </summary>
        /// <param name="value">The value to get the index of.</param>
        /// <returns>The index of the value.</returns>
        public int GetIndex(double value)
        {
            return this.doubles[value];
        }

        /// <summary>
        /// Gets the index in the object table for the given value, assuming it has already been added to the cache.
        /// </summary>
        /// <param name="value">The value to get the index of.</param>
        /// <returns>The index of the value.</returns>
        public int GetIndex(DateTime value)
        {
            return this.dates[value];
        }

        /// <summary>
        /// Gets the index in the object table for the given value, assuming it has already been added to the cache.
        /// </summary>
        /// <param name="value">The value to get the index of.</param>
        /// <returns>The index of the value.</returns>
        public int GetIndex(string value)
        {
            return this.strings[value];
        }

        /// <summary>
        /// Sets the index in the object table for the given value.
        /// </summary>
        /// <param name="value">The value to set the index for.</param>
        /// <param name="index">The index to set.</param>
        public void SetIndex(bool value, int index)
        {
            this.booleans[value] = index;
        }

        /// <summary>
        /// Sets the index in the object table for the given value.
        /// </summary>
        /// <param name="value">The value to set the index for.</param>
        /// <param name="index">The index to set.</param>
        public void SetIndex(long value, int index)
        {
            this.integers[value] = index;
        }

        /// <summary>
        /// Sets the index in the object table for the given value.
        /// </summary>
        /// <param name="value">The value to set the index for.</param>
        /// <param name="index">The index to set.</param>
        public void SetIndex(float value, int index)
        {
            this.floats[value] = index;
        }

        /// <summary>
        /// Sets the index in the object table for the given value.
        /// </summary>
        /// <param name="value">The value to set the index for.</param>
        /// <param name="index">The index to set.</param>
        public void SetIndex(double value, int index)
        {
            this.doubles[value] = index;
        }

        /// <summary>
        /// Sets the index in the object table for the given value.
        /// </summary>
        /// <param name="value">The value to set the index for.</param>
        /// <param name="index">The index to set.</param>
        public void SetIndex(string value, int index)
        {
            this.strings[value] = index;
        }

        /// <summary>
        /// Sets the index in the object table for the given value.
        /// </summary>
        /// <param name="value">The value to set the index for.</param>
        /// <param name="index">The index to set.</param>
        public void SetIndex(DateTime value, int index)
        {
            this.dates[value] = index;
        }
    }
}
