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
    private const int ReleasesToCheck = 10; // Количество проверяемых релизов
    
    private static readonly Dictionary<ApplicationType, string> Applications = new()
    {
        { ApplicationType.Fractal, "Fractal" },
        { ApplicationType.Growth, "Growth" },
        { ApplicationType.GrowthSimulation, "Growth.Simulation" }
    };

    public GitHubAsset? Asset { get; private set; }
    public Version? LatestVersion { get; private set; }

    public async Task<bool> CheckForUpdates()
    {
        try
        {
            var releases = await GetAllReleasesAsync();
            if (releases == null || releases.Length == 0)
                return false;

            foreach (var release in releases)
            {
                if (release.Tag == null)
                    continue;

                var releaseVersion = ParseVersion(release.Tag);
                if (releaseVersion <= currentVersion)
                    continue; // Пропускаем старые версии

                var asset = FindMatchingAsset(release);
                if (asset == null)
                    continue; // Нет подходящего ассета для этого приложения

                // Нашли подходящий релиз
                LatestVersion = releaseVersion;
                Asset = asset;

                // Проверяем хеш, если файл уже скачан
                var tempZip = Path.Combine(Path.GetTempPath(), Asset.Name);
                if (File.Exists(tempZip))
                {
                    var isValid = await VerifyFileHash(tempZip, release);
                    if (isValid)
                        return true;
                }

                // Скачиваем обновление
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "NEXUS.Updater");
                
                var appResponse = await httpClient.GetAsync(Asset.DownloadUrl);
                await using var fs = new FileStream(tempZip, FileMode.Create);
                await appResponse.Content.CopyToAsync(fs);
                
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Update check failed: {ex}");
        }

        return false;
    }

    private GitHubAsset? FindMatchingAsset(GitHubRelease release)
    {
        return release.Assets?.FirstOrDefault(a =>
            a.Name != null &&
            a.Name.Contains($"NEXUS.{Applications[appType]}") &&
            a.Name.EndsWith(".zip"));
    }

    private async Task<bool> VerifyFileHash(string filePath, GitHubRelease release)
    {
        try
        {
            var hashesAsset = release.Assets?.FirstOrDefault(a => 
                a.Name != null && 
                a.Name.Equals("hashes.md5", StringComparison.OrdinalIgnoreCase));
            
            if (hashesAsset == null)
                return false;

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "NEXUS.Updater");

            var hashesResponse = await httpClient.GetAsync(hashesAsset.DownloadUrl);
            var stream = await hashesResponse.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);
            
            var localHash = await CalculateMd5Async(filePath);
            
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (line == null)
                    continue;
                
                if (line.Contains($"NEXUS.{Applications[appType]}"))
                {
                    var remoteHash = line.Split('-').Last().Trim().ToLower();
                    return remoteHash == localHash;
                }
            }
        }
        catch
        {
            // В случае ошибки считаем, что проверка не прошла
        }

        return false;
    }

    private async Task<GitHubRelease[]?> GetAllReleasesAsync()
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "NEXUS.Updater");
            httpClient.Timeout = TimeSpan.FromSeconds(15);
            
            var response = await httpClient.GetAsync(
                $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases?per_page={ReleasesToCheck}");
            
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<GitHubRelease[]>();
        }
        catch
        {
            return null;
        }
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

    private Version ParseVersion(string tag)
    {
        return new Version(tag.TrimStart('v').Split('-')[0]);
    }

    private async Task<string> CalculateMd5Async(string filePath)
    {
        using var md5 = MD5.Create();
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        var hashBytes = await md5.ComputeHashAsync(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
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