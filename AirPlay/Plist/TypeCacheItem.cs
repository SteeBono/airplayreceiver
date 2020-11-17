//-----------------------------------------------------------------------
// <copyright file="TypeCacheItem.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace System.Runtime.Serialization.Plists
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Represents a cached type used during serialization by a <see cref="DataContractBinaryPlistSerializer"/>.
    /// </summary>
    internal sealed class TypeCacheItem
    {
        private Type type;
        private bool hasCustomContract;

        /// <summary>
        /// Initializes a new instance of the TypeCacheItem class.
        /// </summary>
        /// <param name="type">The type to cache.</param>
        public TypeCacheItem(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type", "type cannot be null.");
            }

            this.type = type;
            this.hasCustomContract = type.GetCustomAttributes(typeof(DataContractAttribute), false).Length > 0;
            this.InitializeFields();
            this.InitializeProperties();
        }

        /// <summary>
        /// Gets the collection of concrete or simulated <see cref="DataMemberAttribute"/>s for the type's fields.
        /// </summary>
        public IList<DataMemberAttribute> FieldMembers { get; private set; }
        
        /// <summary>
        /// Gets a collection of the type's fields.
        /// </summary>
        public IList<FieldInfo> Fields { get; private set; }

        /// <summary>
        /// Gets a collection of the type's properties.
        /// </summary>
        public IList<PropertyInfo> Properties { get; private set; }

        /// <summary>
        /// Gets a collection of concrete or simulated <see cref="DataMemberAttribute"/>s for the type's properties.
        /// </summary>
        public IList<DataMemberAttribute> PropertyMembers { get; private set; }

        /// <summary>
        /// Initializes this instance's field-related properties.
        /// </summary>
        private void InitializeFields()
        {
            this.FieldMembers = new List<DataMemberAttribute>();
            this.Fields = new List<FieldInfo>();

            var fields = this.hasCustomContract ?
                this.type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) :
                this.type.GetFields(BindingFlags.Instance | BindingFlags.Public);

            var tuples = from f in fields
                         let attr = f.GetCustomAttributes(false)
                         let member = attr.OfType<DataMemberAttribute>().FirstOrDefault()
                         where !f.IsLiteral && attr.OfType<IgnoreDataMemberAttribute>().Count() == 0
                         select new
                         {
                             Info = f,
                             Member = member
                         };

            foreach (var tuple in tuples.Where(t => !this.hasCustomContract || t.Member != null))
            {
                DataMemberAttribute member = tuple.Member != null ?
                    tuple.Member :
                    new DataMemberAttribute()
                    {
                        EmitDefaultValue = true,
                        IsRequired = false
                    };

                member.Name = !string.IsNullOrEmpty(member.Name) ? member.Name : tuple.Info.Name;

                this.FieldMembers.Add(member);
                this.Fields.Add(tuple.Info);
            }
        }

        /// <summary>
        /// Initializes this instance's property-related properties.
        /// </summary>
        private void InitializeProperties()
        {
            this.Properties = new List<PropertyInfo>();
            this.PropertyMembers = new List<DataMemberAttribute>();

            var properties = this.hasCustomContract ?
                this.type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) :
                this.type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var tuples = from p in properties
                         let attr = p.GetCustomAttributes(false)
                         let member = attr.OfType<DataMemberAttribute>().FirstOrDefault()
                         where p.CanRead && p.CanWrite && attr.OfType<IgnoreDataMemberAttribute>().Count() == 0
                         select new
                         {
                             Info = p,
                             Member = member
                         };

            foreach (var tuple in tuples.Where(t => !this.hasCustomContract || t.Member != null))
            {
                DataMemberAttribute member = tuple.Member != null ?
                    tuple.Member :
                    new DataMemberAttribute()
                    {
                        EmitDefaultValue = true,
                        IsRequired = false
                    };

                member.Name = !string.IsNullOrEmpty(member.Name) ? member.Name : tuple.Info.Name;

                this.PropertyMembers.Add(member);
                this.Properties.Add(tuple.Info);
            }
        }
    }
}
