namespace BO;

using System;

public class DalException : Exception
{
    public DalException() { }
    public DalException(string? message) : base(message) { }
    public DalException(string? message, Exception? inner) : base(message, inner) { }
}