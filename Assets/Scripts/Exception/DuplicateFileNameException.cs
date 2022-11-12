using System;
using System.Runtime.Serialization;

[Serializable]
public class DuplicateFileNameException : Exception
{
    public DuplicateFileNameException()
        : base()
    {
    }

    public DuplicateFileNameException(string message)
        : base(message)
    {
    }

    public DuplicateFileNameException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    protected DuplicateFileNameException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
