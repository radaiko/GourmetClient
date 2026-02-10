using System;

namespace GourmetClient.Maui.Core.Network;

public class GourmetHtmlNodeException : Exception
{
    public GourmetHtmlNodeException(string message)
        : base(message)
    {
    }
}