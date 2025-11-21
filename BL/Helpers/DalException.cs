namespace BO;

using System;

public class DalException : Exception
{
    public DalException()
    {
    }

    public DalException(string? message) : base(message)
    {
    }

    public DalException(string? message, Exception? inner) : base(message, inner)
    {
    }
}

public class InvalidRequestException : Exception
{
    public InvalidRequestException()
    {
    }

    public InvalidRequestException(string? message) : base(message)
    {
    }

    public InvalidRequestException(string? message, Exception? inner) : base(message, inner)
    {
    }
}