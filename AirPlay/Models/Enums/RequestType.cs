namespace AirPlay.Models.Enums
{
    public enum RequestType : ushort
    {
        GET = 0,
        POST = 1,
        SETUP = 2,
        GET_PARAMETER = 3,
        RECORD = 4,
        SET_PARAMETER = 5,
        ANNOUNCE = 6,
        FLUSH = 7,
        TEARDOWN = 8,
        OPTIONS = 9,
        PAUSE = 10
    }
}