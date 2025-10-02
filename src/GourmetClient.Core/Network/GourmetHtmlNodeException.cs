using System;

namespace GourmetClient.Network;

public class GourmetHtmlNodeException : Exception
{
    public GourmetHtmlNodeException(string message)
        : base(message)
    {
    }
}