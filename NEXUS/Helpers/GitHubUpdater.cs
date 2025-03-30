using System.Diagnostics;
using System.Net.Http.Json;
using System.Security.Cryptography;
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

    public GitHubAsset? Asset { get; private set; }

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
                if (latestVersion > currentVersion)
                {
                    Asset = latestRelease.Assets.FirstOrDefault(a =>
                        a.Name.Contains($"NEXUS.{Applications[appType]}") &&
                        a.Name.EndsWith(".zip"));

                    if (Asset == null) return false;

                    // Получаем пути
                    var tempZip = Path.Combine(Path.GetTempPath(), Asset.Name);

                    // 1. Скачивание обновления
                    using var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "NEXUS.Updater");
                    var response = await httpClient.GetAsync(Asset.DownloadUrl);
                    await using var fs = new FileStream(tempZip, FileMode.Create);
                    await response.Content.CopyToAsync(fs);
                    return true;
                }
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
            if (Asset?.Name == null ) 
                return false;

            var appDir = AppContext.BaseDirectory.TrimEnd('\\', '/');
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var tempZip = Path.Combine(Path.GetTempPath(), Asset.Name);
            var backupDir = Path.Combine(appDir, "Backup");
            var updateScript = Path.Combine(Path.GetTempPath(), $"update_{Guid.NewGuid()}.bat");

            // 2. Создание скрипта обновления
            var scriptContent = $@"
@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

:: Параметры
set APP_DIR={appDir}
set BACKUP_DIR={backupDir}
set TEMP_ZIP={tempZip}
set EXE_NAME=NEXUS.{Applications[appType]}.exe
set TEMP_DIR={tempDir}

:: Создаем резервную копию
if not exist ""!BACKUP_DIR!"" mkdir ""!BACKUP_DIR!""
robocopy ""!APP_DIR!"" ""!BACKUP_DIR!"" /mir /njh /njs /ndl /nc /ns /nfl

:: Распаковываем во временную директорию
mkdir ""!TEMP_DIR!"" 2>nul
""{Path.Combine(Environment.SystemDirectory, "tar.exe")}"" -xf ""!TEMP_ZIP!"" -C ""!TEMP_DIR!""

:: Копируем файлы с заменой
robocopy ""!TEMP_DIR!"" ""!APP_DIR!"" /e /njh /njs /ndl /nc /ns /nfl

:: Запускаем приложение
start """" /D ""!APP_DIR!"" ""!EXE_NAME!""

:: Очистка
rmdir /s /q ""!TEMP_DIR!"" >nul 2>&1
del ""{updateScript}"" >nul 2>&1
del ""!TEMP_ZIP!"" >nul 2>&1
";
            await File.WriteAllTextAsync(updateScript, scriptContent);

            // 3. Запуск обновления
            var processInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C \"\"{updateScript}\"\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = true
            };

            Process.Start(processInfo);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Update failed: {ex}");
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
    
    public async Task<string> CalculateMD5Async(string filePath)
    {
        using var md5 = MD5.Create();
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        var hashBytes = await md5.ComputeHashAsync(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

}

public class GitHubRelease
{
    [JsonPropertyName("tag_name")] public string? Tag { get; set; }
    [JsonPropertyName("assets")] public GitHubAsset[]? Assets { get; set; }
}

public class GitHubAsset
{
    [JsonPropertyName("name")] public string? Name { get; set; }

    [JsonPropertyName("browser_download_url")]
    public string? DownloadUrl { get; set; }
}