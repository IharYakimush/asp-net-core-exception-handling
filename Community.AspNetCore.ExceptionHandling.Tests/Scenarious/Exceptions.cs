using System;
using System.Runtime.Serialization;

namespace Community.AspNetCore.ExceptionHandling.Tests.Scenarious
{
    [Serializable]
    public class BaseException : Exception
    {

    }

    [Serializable]
    public class NotBaseException : Exception
    {

    }

    [Serializable]
    public class RethrowException : BaseException
    {
        
    }

    [Serializable]
    public class NextEmptyException : BaseException
    {

    }

    [Serializable]
    public class EmptyChainException : BaseException
    {

    }

    [Serializable]
    public class HandledWithoutResponseException : BaseException
    {

    }

    [Serializable]
    public class CommonResponseException : BaseException
    {

    }

    [Serializable]
    public class CustomResponseException : BaseException
    {

    }

    [Serializable]
    public class CustomObjectResponseException : BaseException
    {

    }

    [Serializable]
    public class CustomJsonResponseException : BaseException
    {

    }

    [Serializable]
    public class RetryException : BaseException
    {

    }
}