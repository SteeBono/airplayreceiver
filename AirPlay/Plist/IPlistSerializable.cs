//-----------------------------------------------------------------------
// <copyright file="IPlistSerializable.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace System.Runtime.Serialization.Plists
{
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Defines the interface for proxy serialization with <see cref="BinaryPlistReader"/> and <see cref="BinaryPlistWriter"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public interface IPlistSerializable
    {
        /// <summary>
        /// Populates this instance from the given plist <see cref="IDictionary"/> representation.
        /// Note that nested <see cref="IPlistSerializable"/> objects found in the graph during
        /// <see cref="ToPlistDictionary()"/> are represented as nested <see cref="IDictionary"/> instances here.
        /// </summary>
        /// <param name="plist">The plist <see cref="IDictionary"/> representation of this instance.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        void FromPlistDictionary(IDictionary plist);

        /// <summary>
        /// Gets a plist friendly <see cref="IDictionary"/> representation of this instance.
        /// The returned dictionary may contain nested implementations of <see cref="IPlistSerializable"/>.
        /// </summary>
        /// <returns>A plist friendly <see cref="IDictionary"/> representation of this instance.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        IDictionary ToPlistDictionary();
    }
}
