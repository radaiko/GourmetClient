namespace GC.Core.Update.GitHubApi;

internal class ReleaseAsset {
  public required string Name { get; set; }
  public required string DownloadUrl { get; set; }
  public required long Size { get; set; }
}