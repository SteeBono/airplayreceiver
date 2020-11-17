using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AirPlay.Utils;

namespace AirPlay.Models
{
    public class Header
    {
        private readonly string _hex;

        private bool _valid = true;
        private string _name;
        private List<string> _values;

        public bool IsValid => _valid;
        public string Name => _name;
        public IEnumerable<string> Values => _values;

        public Header(string hex)
        {
            _hex = hex ?? throw new ArgumentNullException(nameof(hex));

            Initialize();
        }

        public Header(string name, string value)
        {
            _name = name;
            _values = value.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()).ToList();
        }

        public Header(string name, params string[] values)
        {
            _name = name;
            _values = values.ToList();
        }

        private void Initialize ()
        {
            // Split hex by ':' (3A)
            var data = _hex.Split("3A", StringSplitOptions.RemoveEmptyEntries).Select(h => h.Trim()).ToArray();
            if (data?.Any() == false)
            {
                _valid = false;
                return;
            }

            _name = ResolveName(data[0]);
            var hValue = data[1];

            _values = ResolveValues(hValue);
        }

        private string ResolveName(string hex)
        {
            var bytes = hex.HexToBytes();
            return Encoding.ASCII.GetString(bytes);
        }

        private List<string> ResolveValues(string hex)
        {
            // Split hex by ',' (2C)
            var vals = hex.Split("2C", StringSplitOptions.RemoveEmptyEntries).ToArray();

            var _values = new List<string>();

            foreach (var val in vals)
            {
                var bytes = hex.HexToBytes();
                var hVal = Encoding.ASCII.GetString(bytes).Trim();
                _values.Add(hVal);
            }

            return _values;
        }
    }
}
