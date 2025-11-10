namespace DO;

using System;

/// <summary>
/// נזרקת כאשר מנסים לגשת (קריאה/עדכון/מחיקה)
/// לאובייקט עם מזהה שלא קיים.
/// </summary>
[Serializable]
public class DalDoesNotExistException : Exception
{
    public DalDoesNotExistException(string? message) : base(message)
    {
    }

    public DalDoesNotExistException(string? message, Exception? inner) : base(message, inner)
    {
    }
}

/// <summary>
/// נזרקת כאשר מנסים להוסיף אובייקט עם מזהה שכבר קיים.
/// </summary>
[Serializable]
public class DalAlreadyExistsException : Exception
{
    public DalAlreadyExistsException(string? message) : base(message)
    {
    }

    public DalAlreadyExistsException(string? message, Exception? inner) : base(message, inner)
    {
    }
}

/// <summary>
/// נזרקת כאשר מופעלת פעולה על אובייקט DAL שלא אותחל (null).
/// </summary>
[Serializable]
public class DalNullException : Exception
{
    public DalNullException(string? message) : base(message)
    {
    }

    public DalNullException(string? message, Exception? inner) : base(message, inner)
    {
    }
}

/// <summary>
/// נזרקת כאשר יש שגיאה ביצירת או טעינת קובץ XML.
/// </summary>
[Serializable]
public class DalXMLFileLoadCreateException : Exception
{
    public DalXMLFileLoadCreateException(string? message) : base(message)
    {
    }

    public DalXMLFileLoadCreateException(string? message, Exception? inner) : base(message, inner)
    {
    }
}