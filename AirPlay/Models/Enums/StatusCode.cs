using System;
namespace AirPlay.Models.Enums
{
    public enum StatusCode
    {
        OK = 200,
        NOCONTENT = 201,
        BADREQUEST = 400,
        UNAUTHORIZED = 401,
        FORBIDDEN = 403,
        INTERNALSERVERERROR = 500
    }
}
