using System;
namespace AirPlay.Models.Enums
{
    public enum ProtocolType
    {
        HTTP10 = 0,
        HTTP11 = 1,
        RTSP10 = 2,
    }

    public class ProtocolConst
    {
        public const string HTTP10 = "485454502F312E30";
        public const string HTTP11 = "485454502F312E31";
        public const string RTSP10 = "525453502F312E30";
    }
}