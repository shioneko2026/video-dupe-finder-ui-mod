using System.IO.Compression;
using System.Runtime.InteropServices;
using VDF.Core.FFTools;
using VDF.Core.Utils;

namespace VDF.Web.Services;

public enum FfmpegSetupStatus { Idle, Checking, Ready, Downloading, Failed }

/// <summary>
/// Checks for FFmpeg on startup and downloads it automatically if missing.
/// Only auto-downloads when running as a published self-contained exe.
/// In dev mode (dotnet run), reports failure if FFmpeg is not on PATH.
/// </summary>
public class FFmpegSetupService {
    public FfmpegSetupStatus Status { get; private set; } = FfmpegSetupStatus.Idle;
    public bool IsReady => Status == FfmpegSetupStatus.Ready;
    public int ProgressPercent { get; private set; }
    public string ProgressMessage { get; private set; } = string.Empty;
    public string ErrorMessage { get; private set; } = string.Empty;

    public event Action? StateChanged;

    private bool _started;
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// Idempotent — safe to call from multiple components. Only runs once.
    /// </summary>
    public async Task EnsureAsync() {
        await _lock.WaitAsync();
        try {
            if (_started) return;
            _started = true;
        }
        finally {
            _lock.Release();
        }
        await Task.Run(RunSetupAsync);
    }

    private void Notify(FfmpegSetupStatus status, int percent = 0, string message = "", string error = "") {
        Status = status;
        ProgressPercent = percent;
        ProgressMessage = message;
        ErrorMessage = error;
        StateChanged?.Invoke();
    }

    private async Task RunSetupAsync() {
        Notify(FfmpegSetupStatus.Checking, 0, "Checking for FFmpeg...");

        // If FFmpeg is already accessible anywhere, we're done.
        if (FFToolsUtils.GetPath(FFToolsUtils.FFTool.FFmpeg) != null) {
            Notify(FfmpegSetupStatus.Ready, 100, "FFmpeg ready.");
            return;
        }

        // Detect dev mode: running under dotnet.exe rather than the published VDF.Web.exe
        string processName = Path.GetFileName(Environment.ProcessPath ?? string.Empty);
        bool isDev = processName.StartsWith("dotnet", StringComparison.OrdinalIgnoreCase);
        if (isDev) {
            Notify(FfmpegSetupStatus.Failed, 0, string.Empty,
                "FFmpeg not found. In development mode, add ffmpeg.exe and ffprobe.exe to your PATH " +
                "or place them in VDF.Web/bin/.");
            return;
        }

        // Auto-download for published exe
        const string versionTag = "8.0";
        string archSegment = RuntimeInformation.ProcessArchitecture switch {
            Architecture.X64   => "win64",
            Architecture.X86   => "win32",
            Architecture.Arm64 => "winarm64",
            _                  => "win64"
        };
        string fileName    = $"ffmpeg-n{versionTag}-latest-{archSegment}-gpl-shared-{versionTag}.zip";
        string downloadUrl = $"https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/{fileName}";

        string tempRoot        = Path.Combine(Path.GetTempPath(), "VDF.FFmpegDownload");
        string downloadPath    = Path.Combine(tempRoot, fileName);
        string extractedFolder = Path.Combine(tempRoot, "extracted");

        try {
            Directory.CreateDirectory(tempRoot);
            if (Directory.Exists(extractedFolder))
                Directory.Delete(extractedFolder, true);
            Directory.CreateDirectory(extractedFolder);

            await DownloadFileAsync(new Uri(downloadUrl), downloadPath);

            Notify(FfmpegSetupStatus.Downloading, 90, "Extracting FFmpeg...");
            await Task.Run(() => ZipFile.ExtractToDirectory(downloadPath, extractedFolder, overwriteFiles: true));

            Notify(FfmpegSetupStatus.Downloading, 95, "Installing FFmpeg...");
            string targetFolder = Path.Combine(CoreUtils.CurrentFolder, "bin");
            Directory.CreateDirectory(targetFolder);
            await Task.Run(() => CopyFfmpegFiles(extractedFolder, targetFolder));

            Notify(FfmpegSetupStatus.Ready, 100, "FFmpeg ready.");
        }
        catch (Exception ex) {
            Notify(FfmpegSetupStatus.Failed, 0, string.Empty,
                $"Could not download FFmpeg automatically: {ex.Message}\n\n" +
                "To fix this manually: download FFmpeg from https://ffmpeg.org/download.html " +
                "and place ffmpeg.exe and ffprobe.exe in the bin\\ folder next to VDF.Web.exe.");
        }
        finally {
            try { if (File.Exists(downloadPath)) File.Delete(downloadPath); } catch { /* ignore cleanup errors */ }
        }
    }

    private async Task DownloadFileAsync(Uri url, string destinationPath) {
        using var client   = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"HTTP {(int)response.StatusCode} {response.ReasonPhrase}");

        long? totalBytes = response.Content.Headers.ContentLength;
        await using var source = await response.Content.ReadAsStreamAsync();
        await using var dest   = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);

        var buffer    = new byte[81920];
        long totalRead = 0;
        int  read;
        while ((read = await source.ReadAsync(buffer.AsMemory())) > 0) {
            await dest.WriteAsync(buffer.AsMemory(0, read));
            totalRead += read;
            if (totalBytes > 0) {
                int pct    = (int)(totalRead * 80L / totalBytes.Value); // reserve 80% for download phase
                string msg = $"Downloading FFmpeg... {totalRead / 1_048_576.0:0.#} / {totalBytes.Value / 1_048_576.0:0.#} MB";
                Notify(FfmpegSetupStatus.Downloading, pct, msg);
            }
        }
    }

    private static void CopyFfmpegFiles(string sourceRoot, string targetFolder) {
        string? ffmpegPath  = Directory.EnumerateFiles(sourceRoot, "ffmpeg.exe",  SearchOption.AllDirectories).FirstOrDefault();
        string? ffprobePath = Directory.EnumerateFiles(sourceRoot, "ffprobe.exe", SearchOption.AllDirectories).FirstOrDefault();

        if (ffmpegPath == null || ffprobePath == null)
            throw new FileNotFoundException("ffmpeg.exe / ffprobe.exe not found in the downloaded archive.");

        string? binFolder = Path.GetDirectoryName(ffmpegPath);
        if (string.IsNullOrEmpty(binFolder))
            throw new DirectoryNotFoundException("Could not determine ffmpeg folder inside the archive.");

        // Copy everything in the same folder as ffmpeg.exe (includes exes + core dlls)
        foreach (var file in Directory.EnumerateFiles(binFolder)) {
            string dest = FFToolsUtils.LongPathFix(Path.Combine(targetFolder, Path.GetFileName(file)));
            File.Copy(file, dest, overwrite: true);
        }

        // Also copy av* / sw* shared libs from anywhere else in the archive tree
        string[] libPrefixes = { "avcodec", "avformat", "avutil", "swresample", "swscale" };
        foreach (var file in Directory.EnumerateFiles(sourceRoot, "*.dll", SearchOption.AllDirectories)) {
            string name = Path.GetFileName(file);
            if (libPrefixes.Any(p => name.StartsWith(p, StringComparison.OrdinalIgnoreCase))) {
                string dest = FFToolsUtils.LongPathFix(Path.Combine(targetFolder, name));
                File.Copy(file, dest, overwrite: true);
            }
        }
    }
}
