using System.Net.Http;

namespace GC.Core.Utils;

public record HttpClientResult<T>(HttpClient Client, T ResponseResult);