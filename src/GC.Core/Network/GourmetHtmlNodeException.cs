using System;

namespace GC.Core.Network;

public class GourmetHtmlNodeException : Exception {
  public GourmetHtmlNodeException(string message)
    : base(message) { }
}