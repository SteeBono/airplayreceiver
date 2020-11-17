using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace AirPlay.Models
{
    public class HeadersCollection : IEnumerable<Header>
    {
        private Dictionary<string, Header> _headers;

        public string this[string key]
        {
            get
            {
                return string.Join(",", _headers[key].Values);
            }
            set
            {
                _headers.Add(key, new Header(key, value));
            }
        }

        public ICollection<string> Keys => _headers.Keys;

        public ICollection<string> Values => _headers.Values.Select(v => string.Join(",", v.Values)).ToList();

        public int Count => _headers.Count;

        public bool IsReadOnly => throw new NotImplementedException();


        public HeadersCollection()
        {
            _headers = new Dictionary<string, Header>(StringComparer.OrdinalIgnoreCase);
        }

        public T GetValue<T>(string key)
        {
            var value = this[key];

            var typeConverter = TypeDescriptor.GetConverter(typeof(T));
            return (T)typeConverter.ConvertFromString(value);
        }

        public IEnumerable<T> GetValues<T>(string key)
        {
            var typeConverter = TypeDescriptor.GetConverter(typeof(T));
            var values = _headers[key].Values;

            var vals = new List<T>();
            foreach (var value in values)
            {
                vals.Add((T)typeConverter.ConvertFromString(value));
            }

            return vals;
        }

        public void Add(string key, string value)
        {
            _headers.Add(key, new Header(key, value));
        }

        public void Add(string key, Header value)
        {
            _headers.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _headers.ContainsKey(key);
        }

        public void Clear()
        {
            _headers.Clear();
        }

        public IEnumerator<Header> GetEnumerator()
        {
            return _headers.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
