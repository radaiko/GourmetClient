using System;

namespace GourmetClient.Core.Update;

public class GourmetUpdateException : Exception
{
    public GourmetUpdateException(string message)
        : base(message)
    {
    }

    public GourmetUpdateException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}