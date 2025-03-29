using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace NEXUS.Helpers;

public enum ApplicationType
{
    Fractal,
    Growth,
    GrowthSimulation
}

/// <summary>
/// NEXUS Releases updater
/// </summary>
/// <param name="appType"></param>
/// <param name="currentVersion"></param>
public class GitHubUpdater(ApplicationType appType, Version currentVersion)
{
    private const string RepoOwner = "Talwok";
    private const string RepoName = "NEXUS";
    
    private static readonly Dictionary<ApplicationType, string> Applications = new()
    {
        { ApplicationType.Fractal, "Fractal" },
        { ApplicationType.Growth, "Growth" },
        { ApplicationType.GrowthSimulation, "Growth.Simulation" }
    };
     

    public async Task<bool> CheckForUpdates()
    {
        try
        {
            var latestRelease = await GetLatestReleaseAsync();
            if (latestRelease == null) 
                return false;

            if (latestRelease.Tag != null)
            {
                var latestVersion = ParseVersion(latestRelease.Tag);
                return latestVersion > currentVersion;
            }
        }
        catch
        {
            return false;
        }
        
        return false;
    }

    public async Task<bool> UpdateApplication()
    {
        try
        {
            var latestRelease = await GetLatestReleaseAsync();
            if (latestRelease == null) return false;

            if (latestRelease.Assets != null)
            {
                var asset = latestRelease.Assets.FirstOrDefault(a => 
                    a.Name != null &&
                    a.Name.Contains($"NEXUS.{Applications[appType]}") && 
                    a.Name.EndsWith(".zip"));

                if (asset?.Name == null || asset.DownloadUrl == null) 
                    return false;

                var tempZip = Path.Combine(Path.GetTempPath(), asset.Name);
                var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                var appDir = Path.GetDirectoryName(exePath);
                if (appDir != null)
                {
                    var backupDir = Path.Combine(appDir, "Backup");

                    // Скачивание обновления
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Add("User-Agent", "NEXUS.Updater");
                        var response = await httpClient.GetAsync(asset.DownloadUrl);
                        await using (var fs = new FileStream(tempZip, FileMode.Create)) 
                            await response.Content.CopyToAsync(fs);
                    }

                    // Создание скрипта обновления
                    var updateScript = Path.Combine(Path.GetTempPath(), $"update_NEXUS.{Applications[appType]}.bat");
                    await File.WriteAllTextAsync(updateScript, 
                        $@"
                @echo off
                timeout /t 2 /nobreak >nul
                mkdir ""{backupDir}""
                robocopy ""{appDir}"" ""{backupDir}"" /mir /njh /njs /ndl /nc /ns /nfl
                tar -xf ""{tempZip}"" -C ""{appDir}""
                start """" ""{exePath}""
                del ""{updateScript}""
                del ""{tempZip}""
                ");

                    // Запуск обновления
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C start \"\" \"{updateScript}\"",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true
                    });
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Update failed: {ex.Message}");
            return false;
        }
    }

    private async Task<GitHubRelease?> GetLatestReleaseAsync()
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "NEXUS.Updater");
        var response = await httpClient.GetAsync(
            $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest");
        
        return await response.Content.ReadFromJsonAsync<GitHubRelease>();
    }

    private Version ParseVersion(string tag)
    {
        return new Version(tag.TrimStart('v').Split('-')[0]);
    }
}

public class GitHubRelease
{
    [JsonPropertyName("tag_name")]
    public string? Tag { get; set; }
    [JsonPropertyName("assets")]
    public GitHubAsset[]? Assets { get; set; }
}

public class GitHubAsset
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("browser_download_url")]
    public string? DownloadUrl { get; set; }
}