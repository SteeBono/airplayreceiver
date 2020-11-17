//-----------------------------------------------------------------------
// <copyright file="DataContractBinaryPlistSerializer.cs" company="Tasty Codes">
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
    using System.Reflection;
    using System.Runtime.Serialization;

    /// <summary>
    /// Serializes data contracts to and from the binary plist format.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public sealed class DataContractBinaryPlistSerializer
    {
        private Type rootType;
        private bool isDictionary;
        private Dictionary<Type, TypeCacheItem> typeCache;

        /// <summary>
        /// Initializes a new instance of the DataContractBinaryPlistSerializer class.
        /// </summary>
        /// <param name="type">The type of the instances that are serialized or de-serialized.</param>
        public DataContractBinaryPlistSerializer(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type", "type cannot be null.");
            }

            this.isDictionary = typeof(IDictionary).IsAssignableFrom(type);

            if (!this.isDictionary && type.IsCollection())
            {
                throw new ArgumentException("root type cannot be a collection unless it is an IDictionary implementation.", "type");
            }

            if (type.IsPrimitiveOrEnum())
            {
                throw new ArgumentException("type must be an implementation of IDictionary or a complex object type.", "type");
            }

            this.rootType = type;
            this.typeCache = new Dictionary<Type, TypeCacheItem>();
        }

        /// <summary>
        /// Reads an object from the specified stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The de-serialized object.</returns>
        public object ReadObject(Stream stream)
        {
            return this.GetReadablePlistObject(this.rootType, new BinaryPlistReader().ReadObject(stream));
        }

        /// <summary>
        /// Writes the complete contents of the given object to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="graph">The object to write.</param>
        public void WriteObject(Stream stream, object graph)
        {
            this.WriteObject(stream, graph, true);
        }

        /// <summary>
        /// Writes the complete contents of the given object to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="graph">The object to write.</param>
        /// <param name="closeStream">A value indicating whether to close the stream after the write operation completes.</param>
        public void WriteObject(Stream stream, object graph, bool closeStream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream", "stream cannot be null.");
            }

            if (graph == null)
            {
                throw new ArgumentNullException("graph", "graph cannot be null.");
            }

            Type type = graph.GetType();

            if (!this.rootType.IsAssignableFrom(type))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The object specified is of type {0}, which is not assignable to this instance's root type, {1}.", type, this.rootType));
            }

            IDictionary dict = this.GetWritablePlistObject(type, graph) as IDictionary;

            if (dict == null)
            {
                throw new ArgumentException("The root object must be assignable to IDictionary, a complex type with an explicit or implied data contract, or assignable to IPlistSerializable.", "graph");
            }

            new BinaryPlistWriter().WriteObject(stream, dict, closeStream);
        }

        /// <summary>
        /// Gets the readable plist value of the given object identified by the specified type.
        /// </summary>
        /// <param name="type">The type the object is expected to have after being de-serialized.</param>
        /// <param name="obj">The raw plist object value.</param>
        /// <returns>A readable plist object value.</returns>
        private object GetReadablePlistObject(Type type, object obj)
        {
            object result = null;
            IDictionary plistDict = obj as IDictionary;

            if (obj != null)
            {
                if (typeof(IPlistSerializable).IsAssignableFrom(type))
                {
                    if (plistDict != null)
                    {
                        IPlistSerializable serResult = (IPlistSerializable)Activator.CreateInstance(type);
                        serResult.FromPlistDictionary(plistDict);
                    }
                }
                else if (typeof(IDictionary).IsAssignableFrom(type))
                {
                    if (plistDict != null)
                    {
                        Type keyType = typeof(object), valueType = typeof(object);

                        if (type.IsGenericType)
                        {
                            Type[] args = type.GetGenericArguments();
                            keyType = args[0];
                            valueType = args[1];
                        }

                        IDictionary dictResult = (IDictionary)(type.IsInterface ?
                                Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType)) :
                                Activator.CreateInstance(type));

                        foreach (object key in plistDict.Keys)
                        {
                            if (!type.IsGenericType)
                            {
                                keyType = key.GetType();
                                valueType = plistDict[key] != null ? plistDict[key].GetType() : typeof(object);
                            }

                            dictResult[this.GetReadablePlistObject(keyType, key)] = this.GetReadablePlistObject(valueType, plistDict[key]);
                        }

                        result = dictResult;
                    }
                }
                else if (type.IsCollection())
                {
                    IEnumerable plistColl = obj as IEnumerable;

                    if (plistColl != null)
                    {
                        Type valueType = typeof(object);
                        bool isArray = false;
                        IList listResult;
                        
                        if (type.IsGenericType)
                        {
                            valueType = type.GetGenericArguments()[0];
                        }
                        else if (typeof(Array).IsAssignableFrom(type))
                        {
                            valueType = type.GetElementType();
                            isArray = true;
                        }

                        if (isArray)
                        {
                            listResult = new ArrayList();
                        }
                        else
                        {
                            // TODO: The default DataContractSerializer uses an informal protocal requiring a method named "Add()"
                            // rather than requiring concrete collection types to implement IList.
                            listResult = (IList)(type.IsInterface ?
                                Activator.CreateInstance(typeof(List<>).MakeGenericType(valueType)) :
                                Activator.CreateInstance(type));
                        }

                        foreach (object value in plistColl)
                        {
                            listResult.Add(this.GetReadablePlistObject(valueType, value));
                        }

                        result = isArray ? ((ArrayList)listResult).ToArray() : listResult;
                    }
                }
                else if (type.IsPrimitiveOrEnum())
                {
                    result = obj;
                }
                else
                {
                    if (plistDict != null)
                    {
                        if (!this.typeCache.ContainsKey(type))
                        {
                            this.typeCache[type] = new TypeCacheItem(type);
                        }

                        TypeCacheItem cache = this.typeCache[type];
                        result = Activator.CreateInstance(type);

                        for (int i = 0; i < cache.Fields.Count; i++)
                        {
                            FieldInfo field = cache.Fields[i];
                            DataMemberAttribute member = cache.FieldMembers[i];

                            if (plistDict.Contains(member.Name))
                            {
                                field.SetValue(result, this.GetReadablePlistObject(field.FieldType, plistDict[member.Name]));
                            }
                        }

                        for (int i = 0; i < cache.Properties.Count; i++)
                        {
                            PropertyInfo property = cache.Properties[i];
                            DataMemberAttribute member = cache.PropertyMembers[i];

                            if (plistDict.Contains(member.Name))
                            {
                                property.SetValue(result, this.GetReadablePlistObject(property.PropertyType, plistDict[member.Name]), null);
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the writable plist value of the given object identified by the specified type.
        /// </summary>
        /// <param name="type">The of the object.</param>
        /// <param name="obj">The object to get the plist value of.</param>
        /// <returns>The plist value of the given object.</returns>
        private object GetWritablePlistObject(Type type, object obj)
        {
            object result = null;

            if (obj != null)
            {
                if (typeof(IPlistSerializable).IsAssignableFrom(type))
                {
                    result = ((IPlistSerializable)obj).ToPlistDictionary();
                }
                else if (typeof(IDictionary).IsAssignableFrom(type))
                {
                    IDictionary dict = obj as IDictionary;
                    Dictionary<object, object> resultDict = new Dictionary<object, object>();

                    foreach (object key in dict.Keys)
                    {
                        object value = dict[key];
                        resultDict[this.GetWritablePlistObject(key.GetType(), key)] = this.GetWritablePlistObject(value.GetType(), value);
                    }

                    result = resultDict;
                }
                else if (type.IsCollection())
                {
                    IEnumerable coll = obj as IEnumerable;
                    List<object> resultColl = new List<object>();

                    foreach (object value in coll)
                    {
                        resultColl.Add(this.GetWritablePlistObject(value.GetType(), value));
                    }

                    result = resultColl;
                }
                else if (type.IsPrimitiveOrEnum())
                {
                    result = obj;
                }
                else
                {
                    if (!this.typeCache.ContainsKey(type))
                    {
                        this.typeCache[type] = new TypeCacheItem(type);
                    }

                    TypeCacheItem cache = this.typeCache[type];
                    Dictionary<string, object> resultDict = new Dictionary<string, object>();

                    for (int i = 0; i < cache.Fields.Count; i++)
                    {
                        FieldInfo field = cache.Fields[i];
                        DataMemberAttribute member = cache.FieldMembers[i];
                        object fieldValue = field.GetValue(obj);

                        if (member.EmitDefaultValue || !field.FieldType.IsDefaultValue(fieldValue))
                        {
                            resultDict[member.Name] = this.GetWritablePlistObject(field.FieldType, fieldValue);
                        }
                    }

                    for (int i = 0; i < cache.Properties.Count; i++)
                    {
                        PropertyInfo property = cache.Properties[i];
                        DataMemberAttribute member = cache.PropertyMembers[i];
                        object propertyValue = property.GetValue(obj, null);

                        if (member.EmitDefaultValue || !property.PropertyType.IsDefaultValue(propertyValue))
                        {
                            resultDict[member.Name] = this.GetWritablePlistObject(property.PropertyType, propertyValue);
                        }
                    }

                    result = resultDict;
                }
            }

            return result;
        }
    }
}
