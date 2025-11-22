namespace BO;

using System;

/// <summary>
/// Thrown when a DAL layer exception occurs.
/// </summary>
[Serializable]
public class BlDalException : Exception
{

    public BlDalException(string? message) : base(message) { }

    public BlDalException(string? message, Exception? inner) : base(message, inner) { }
}

/// <summary>
/// Thrown when a request is invalid or the user is unauthorized.
/// </summary>
[Serializable]
public class BlInvalidRequestException : Exception
{

    public BlInvalidRequestException(string? message) : base(message) { }

    public BlInvalidRequestException(string? message, Exception? inner) : base(message, inner) { }
}

/// <summary>
/// Thrown when attempting to access an entity that does not exist.
/// </summary>
[Serializable]
public class BlDoesNotExistException : Exception
{

    public BlDoesNotExistException(string? message) : base(message) { }

    public BlDoesNotExistException(string? message, Exception? inner) : base(message, inner) { }
}

/// <summary>
/// Thrown when attempting to create an entity that already exists.
/// </summary>
[Serializable]
public class BlAlreadyExistsException : Exception
{

    public BlAlreadyExistsException(string? message) : base(message) { }

    public BlAlreadyExistsException(string? message, Exception? inner) : base(message, inner) { }
}

/// <summary>
/// Thrown when an operation cannot be performed due to internal state, e.g., deleting an active entity.
/// </summary>
[Serializable]
public class BlInvalidOperationException : Exception
{

    public BlInvalidOperationException(string? message) : base(message) { }

    public BlInvalidOperationException(string? message, Exception? inner) : base(message, inner) { }
}

/// <summary>
/// Thrown when a user is not authorized to perform an action.
/// </summary>
[Serializable]
public class BlUnauthorizedAccessException : Exception
{

    public BlUnauthorizedAccessException(string? message) : base(message) { }

    public BlUnauthorizedAccessException(string? message, Exception? inner) : base(message, inner) { }
}

/// <summary>
/// General exception for unexpected errors in the BL layer.
/// </summary>
[Serializable]
public class BlException : Exception
{

    public BlException(string? message) : base(message) { }

    public BlException(string? message, Exception? inner) : base(message, inner) { }
}

/// <summary>
/// Unexpected exception in the business layer.
/// </summary>
[Serializable]
public class BlUnexpectedException : Exception
{
    public BlUnexpectedException(string message) : base(message) { }
    public BlUnexpectedException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception related to DAL file operations.
/// </summary>
[Serializable]
public class BlDalFileException : Exception
{
    public BlDalFileException(string message) : base(message) { }
    public BlDalFileException(string message, Exception innerException) : base(message, innerException) { }
}