using System;

namespace GourmetClient.Core.Network;

public class GourmetHtmlNodeException : Exception
{
    public GourmetHtmlNodeException(string message)
        : base(message)
    {
    }
}