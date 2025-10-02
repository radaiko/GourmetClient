using System.Net.Http;

namespace GourmetClient.Core.Utils;

public record HttpClientResult<T>(HttpClient Client, T ResponseResult);