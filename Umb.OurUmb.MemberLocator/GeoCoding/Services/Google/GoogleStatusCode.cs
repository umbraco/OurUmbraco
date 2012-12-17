namespace Umb.OurUmb.MemberLocator.GeoCoding.Services.Google
{
    using System;

    public enum GoogleStatusCode
    {
        BadKey = 610,
        MissingAddress = 0x259,
        ServerError = 500,
        Success = 200,
        UnavailableAddress = 0x25b,
        UnknownAddress = 0x25a
    }
}

