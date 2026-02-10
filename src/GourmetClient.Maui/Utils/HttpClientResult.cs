using System.Net.Http;

namespace GourmetClient.Maui.Utils;

public record HttpClientResult<T>(HttpClient Client, T ResponseResult);