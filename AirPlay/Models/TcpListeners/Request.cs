using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AirPlay.Models.Enums;
using AirPlay.Utils;

namespace AirPlay.Models
{
    public class Request
    {
        public const string AIRTUNES_SERVER_VERSION = "AirTunes/220.68";

        private readonly string _hex;

        private bool _valid = true;
        private RequestType _type;
        private string _path;
        private ProtocolType _protocol;
        private byte[] _rawBody;
        private HeadersCollection _headers;

        public bool IsValid => _valid;
        public RequestType Type => _type;
        public string Path => _path;
        public ProtocolType Protocol => _protocol;
        public byte[] Body => _rawBody;
        public HeadersCollection Headers => _headers;

        public Request(string hex)
        {
            _hex = hex ?? throw new ArgumentNullException(nameof(hex));
            _headers = new HeadersCollection();

            Initialize();
        }

        public Task<byte[]> GetFullRawAsync()
        {
            return Task.FromResult(_hex.HexToBytes());
        }

        private void Initialize ()
        {
            // Split hex by '\r\n' (0D0A)
            var rows = _hex.Split("0D0A", StringSplitOptions.None).ToArray();

            if(rows?.Any() == false)
            {
                _valid = false;
                return;
            }

            var type = ResolveRequestType(rows[0]);
            if(!type.HasValue)
            {
                _valid = false;
                return;
            }

            var path = ResolvePath(rows[0]);
            if (string.IsNullOrWhiteSpace(path))
            {
                _valid = false;
                return;
            }

            var protocol = ResolveProtocol(rows[0]);
            if (!protocol.HasValue)
            {
                _valid = false;
                return;
            }

            _type = type.Value;
            _path = path;
            _protocol = protocol.Value;

            foreach (var header in rows.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(header))
                {
                    // End of headers
                    break;
                }

                var h = ResolveHeader(header);
                _headers.Add(h.Name, h);
            }

            if(_headers.ContainsKey("Content-Length"))
            {
                var contentLength = _headers.GetValue<int>("Content-Length");
                if(contentLength > 0)
                {
                    // Some request can have body w/ '\r\n' chars
                    // Use full hex request to extract body based on 'Content-Length'
                    var requestBytes = _hex.HexToBytes();
                    var bodyBytes = requestBytes.Skip(requestBytes.Length - contentLength).ToArray();

                    if (contentLength == bodyBytes.Length)
                    {
                        _rawBody = bodyBytes;
                    }
                    else
                    {
                        throw new Exception("wrong body length");
                    }
                }
                else
                {
                    _rawBody = new byte[0];
                }
            }
        }

        private RequestType? ResolveRequestType (string hex)
        {
            if (hex.StartsWith(RequestConst.GET, StringComparison.OrdinalIgnoreCase))
            {
                return RequestType.GET;
            }
            if (hex.StartsWith(RequestConst.POST, StringComparison.OrdinalIgnoreCase))
            {
                return RequestType.POST;
            }
            if (hex.StartsWith(RequestConst.SETUP, StringComparison.OrdinalIgnoreCase))
            {
                return RequestType.SETUP;
            }
            if (hex.StartsWith(RequestConst.RECORD, StringComparison.OrdinalIgnoreCase))
            {
                return RequestType.RECORD;
            }
            if (hex.StartsWith(RequestConst.GET_PARAMETER, StringComparison.OrdinalIgnoreCase))
            {
                return RequestType.GET_PARAMETER;
            }
            if (hex.StartsWith(RequestConst.SET_PARAMETER, StringComparison.OrdinalIgnoreCase))
            {
                return RequestType.SET_PARAMETER;
            }
            if (hex.StartsWith(RequestConst.OPTIONS, StringComparison.OrdinalIgnoreCase))
            {
                return RequestType.OPTIONS;
            }
            if (hex.StartsWith(RequestConst.ANNOUNCE, StringComparison.OrdinalIgnoreCase))
            {
                return RequestType.ANNOUNCE;
            }
            if (hex.StartsWith(RequestConst.FLUSH, StringComparison.OrdinalIgnoreCase))
            {
                return RequestType.FLUSH;
            }
            if (hex.StartsWith(RequestConst.TEARDOWN, StringComparison.OrdinalIgnoreCase))
            {
                return RequestType.TEARDOWN;
            }
            if (hex.StartsWith(RequestConst.PAUSE, StringComparison.OrdinalIgnoreCase))
            {
                return RequestType.PAUSE;
            }

            return null;
        }

        private string ResolvePath (string hex)
        {
            var r = new Regex("20(.*)20");
            var m = r.Match(hex);

            if (m.Success)
            {
                var pathHex = m.Groups[1].Value;
                var pathBytes = pathHex.HexToBytes();
                return Encoding.ASCII.GetString(pathBytes);
            }

            return null;
        }

        private ProtocolType? ResolveProtocol(string hex)
        {
            if (hex.EndsWith(ProtocolConst.HTTP10, StringComparison.OrdinalIgnoreCase))
            {
                return ProtocolType.HTTP10;
            }
            if (hex.EndsWith(ProtocolConst.HTTP11, StringComparison.OrdinalIgnoreCase))
            {
                return ProtocolType.HTTP11;
            }
            if (hex.EndsWith(ProtocolConst.RTSP10, StringComparison.OrdinalIgnoreCase))
            {
                return ProtocolType.RTSP10;
            }

            return null;
        }

        public Response GetBaseResponse()
        {
            var response = new Response(_protocol, StatusCode.OK);
            response.Headers.Add("Server", AIRTUNES_SERVER_VERSION);
            if(_headers.ContainsKey("CSeq"))
            {
                response.Headers.Add("CSeq", _headers["CSeq"]);
            }

            return response;
        }

        private Header ResolveHeader(string hex)
        {
            return new Header(hex);
        }
    }
}
