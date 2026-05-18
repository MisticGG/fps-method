using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace FpsMethod;

public partial class MainWindow : Window
{
    private string? _srcPath;
    private Mp4Info? _mp4Info;
    private Run? _statsRun;

    private static readonly IBrush Accent = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));
    private static readonly IBrush Accent2 = new SolidColorBrush(Color.FromRgb(0x99, 0x99, 0x99));
    private static readonly IBrush Red = new SolidColorBrush(Color.FromRgb(0xFF, 0x44, 0x44));
    private static readonly IBrush Mid = new SolidColorBrush(Color.FromRgb(0x68, 0x68, 0x68));
    private static readonly IBrush Muted = new SolidColorBrush(Color.FromRgb(0x38, 0x38, 0x38));
    private static readonly IBrush Border_ = new SolidColorBrush(Color.FromRgb(0x1A, 0x1A, 0x1A));

<<<<<<< HEAD
    private static string _ffmpegExe = "ffmpeg";
    private static string _ffprobeExe = "ffprobe";
    private static readonly string FfmpegDataDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "fps-method");

=======
>>>>>>> 80e5fa159319a2e549efd36f31334d8936243364
    private static readonly (string Name, string Sub, int MaxDim, double BitrateM)[] PresetDefs =
    {
        ("1080p",  "20 Mbps",  1920, 20.0),
        ("720p",   "12 Mbps",  1280, 12.0),
        ("540p",   "7 Mbps",   960,  7.0),
        ("CUSTOM", "manual",   0,    0.0),
    };

    private int _selectedPreset = 0;
    private Button[] _presetButtons = null!;
    private int _srcW = 1920;
    private int _srcH = 1080;

    public MainWindow()
    {
        InitializeComponent();

        DropZoneBorder.AddHandler(DragDrop.DropEvent, DropZone_Drop);
        DropZoneBorder.AddHandler(DragDrop.DragEnterEvent, DropZone_DragEnter);
        DropZoneBorder.AddHandler(DragDrop.DragLeaveEvent, DropZone_DragLeave);

        LogDim("ready; drop an mp4 or click the zone to begin.");

        _presetButtons = new[] { Preset0Btn, Preset1Btn, Preset2Btn, Preset3Btn };
        SelectPreset(0);

        _ = SetupFfmpegAsync();
    }

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    private void MinBtn_Click(object? sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void CloseBtn_Click(object? sender, RoutedEventArgs e) => Close();

    private void SelectPreset(int index)
    {
        _selectedPreset = index;

        for (int i = 0; i < _presetButtons.Length; i++)
        {
            bool sel = i == index;
            _presetButtons[i].BorderBrush = new SolidColorBrush(sel
                ? Color.FromRgb(0x44, 0x44, 0x44)
                : Color.FromRgb(0x1E, 0x1E, 0x1E));
            _presetButtons[i].Background = new SolidColorBrush(sel
                ? Color.FromRgb(0x16, 0x16, 0x16)
                : Color.FromRgb(0x0C, 0x0C, 0x0C));

            if (_presetButtons[i].Content is StackPanel sp)
            {
                if (sp.Children.Count >= 1 && sp.Children[0] is TextBlock top)
                    top.Foreground = new SolidColorBrush(sel
                        ? Color.FromRgb(0xB8, 0xB8, 0xB8)
                        : Color.FromRgb(0x68, 0x68, 0x68));
                if (sp.Children.Count >= 2 && sp.Children[1] is TextBlock sub)
                    sub.Foreground = new SolidColorBrush(sel
                        ? Color.FromRgb(0x68, 0x68, 0x68)
                        : Color.FromRgb(0x3A, 0x3A, 0x3A));
            }
        }

        CustomSettingsPanel.IsVisible = (index == 3);
    }

    private void PresetBtn_Click(object? sender, RoutedEventArgs e)
    {
        int idx = Array.IndexOf(_presetButtons, sender as Button);
        if (idx >= 0) SelectPreset(idx);
    }

<<<<<<< HEAD
    private async Task SetupFfmpegAsync()
    {
        var ffmpeg = await Task.Run(() => ProbeBinary("ffmpeg"));
        var ffprobe = await Task.Run(() => ProbeBinary("ffprobe"));

        if (ffmpeg != null) _ffmpegExe = ffmpeg;
        if (ffprobe != null) _ffprobeExe = ffprobe;

        if (ffmpeg != null) return;

        Dispatcher.UIThread.Invoke(() =>
        {
            FFmpegBorder.IsVisible = true;

            if (OperatingSystem.IsWindows())
            {
                FFmpegStatusText.Text =
                    "FFmpeg not found, needed for encoding. " +
                    "Click on download to install a portable copy (~100 MB)";
                FFmpegDownloadBtn.IsVisible = true;
            }
            else if (OperatingSystem.IsMacOS())
            {
                FFmpegStatusText.Text =
                    "FFmpeg not found — install it with:\u2002brew install ffmpeg";
            }
            else
            {
                FFmpegStatusText.Text =
                    "FFmpeg not found — install it with:\u2002sudo apt install ffmpeg  (or your distro’s equivalent)";
            }
        });
    }

    private static string? ProbeBinary(string name)
    {
        var local = Path.Combine(FfmpegDataDir,
            OperatingSystem.IsWindows() ? name + ".exe" : name);
        if (File.Exists(local) && TestBinary(local)) return local;

        if (TestBinary(name)) return name;

        return null;
    }

    private static bool TestBinary(string exe)
    {
        try
        {
            var psi = new ProcessStartInfo(exe, "-version")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            using var p = Process.Start(psi);
            p?.WaitForExit(4_000);
            return p?.ExitCode == 0;
        }
        catch { return false; }
    }

    private async void FFmpegDownloadBtn_Click(object? sender, RoutedEventArgs e)
    {
        FFmpegDownloadBtn.IsEnabled = false;
        await DownloadFfmpegAsync();
        if (FFmpegBorder.IsVisible)
            FFmpegDownloadBtn.IsEnabled = true;
    }

    private async Task DownloadFfmpegAsync()
    {
        const string url =
            "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip";

        var zipPath = Path.Combine(Path.GetTempPath(), "ffmpeg_fpsmethod.zip");

        LogAccent("\ndownloading ffmpeg\u2026");
        LogDim("   this may take a moment (~100 MB)");

        try
        {
            Directory.CreateDirectory(FfmpegDataDir);

            using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(15) };
            http.DefaultRequestHeaders.UserAgent.ParseAdd("FpsMethod/1.0");

            {
                using var response =
                    await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                long total = response.Content.Headers.ContentLength ?? -1L;
                await using var netStream = await response.Content.ReadAsStreamAsync();
                await using var fileStream = File.Create(zipPath);

                var buf = new byte[81_920];
                long got = 0, lastTick = 0;
                int read;

                while ((read = await netStream.ReadAsync(buf)) > 0)
                {
                    await fileStream.WriteAsync(buf.AsMemory(0, read));
                    got += read;
                    long now = Environment.TickCount64;
                    if (total > 0 && now - lastTick >= 2_000)
                    {
                        lastTick = now;
                        LogDim($"   {got * 100.0 / total:F0}%  —  {got / 1_048_576.0:F0} / {total / 1_048_576.0:F0} MB");
                    }
                }
            }

            LogInfo("extracting ffmpeg and ffprobe\u2026");

            await Task.Run(() =>
            {
                using var zip = ZipFile.OpenRead(zipPath);
                foreach (var entry in zip.Entries)
                {
                    var fname = Path.GetFileName(entry.FullName);
                    if (fname is not ("ffmpeg.exe" or "ffprobe.exe")) continue;
                    entry.ExtractToFile(Path.Combine(FfmpegDataDir, fname), overwrite: true);
                    LogInfo($"   {fname}  ({entry.Length / 1_048_576.0:F0} MB)");
                }
            });

            try { File.Delete(zipPath); } catch { }

            var ffmpegPath = Path.Combine(FfmpegDataDir, "ffmpeg.exe");
            var ffprobePath = Path.Combine(FfmpegDataDir, "ffprobe.exe");

            if (File.Exists(ffmpegPath) && TestBinary(ffmpegPath))
            {
                _ffmpegExe = ffmpegPath;
                _ffprobeExe = File.Exists(ffprobePath) ? ffprobePath : _ffprobeExe;
                LogAccent("ffmpeg ready ✓");
                Dispatcher.UIThread.Invoke(() => FFmpegBorder.IsVisible = false);
            }
            else
            {
                LogError("extraction problem — ffmpeg.exe not found after download");
            }
        }
        catch (Exception ex)
        {
            try { File.Delete(zipPath); } catch { }
            LogError($"download failed — {ex.Message}");
        }
    }

=======
>>>>>>> 80e5fa159319a2e549efd36f31334d8936243364
    private void CustomWidthBox_LostFocus(object? sender, RoutedEventArgs e)
        => ApplyAspectLock(widthChanged: true);

    private void CustomHeightBox_LostFocus(object? sender, RoutedEventArgs e)
        => ApplyAspectLock(widthChanged: false);

    private void CustomDim_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        ApplyAspectLock(widthChanged: sender == CustomWidthBox);
        e.Handled = true;
    }

    private void ApplyAspectLock(bool widthChanged)
    {
        if (_srcW <= 0 || _srcH <= 0) return;

        if (widthChanged)
        {
            if (!int.TryParse(CustomWidthBox.Text?.Trim(), out int w) || w < 2) return;
            w = Math.Max(2, (w / 2) * 2);
            CustomWidthBox.Text = w.ToString();
            int h = (int)Math.Round((double)w * _srcH / _srcW);
            h = Math.Max(2, (h / 2) * 2);
            CustomHeightBox.Text = h.ToString();
        }
        else
        {
            if (!int.TryParse(CustomHeightBox.Text?.Trim(), out int h) || h < 2) return;
            h = Math.Max(2, (h / 2) * 2);
            CustomHeightBox.Text = h.ToString();
            int w = (int)Math.Round((double)h * _srcW / _srcH);
            w = Math.Max(2, (w / 2) * 2);
            CustomWidthBox.Text = w.ToString();
        }
    }

    private async void DropZone_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

        var sp = TopLevel.GetTopLevel(this)!.StorageProvider;
        var files = await sp.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select a video file",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Video files")
                {
                    Patterns = new[] { "*.mp4", "*.mov", "*.mkv", "*.avi", "*.wmv",
                                       "*.flv", "*.webm", "*.ts", "*.m4v", "*.mts", "*.m2ts" }
                },
                new FilePickerFileType("All files") { Patterns = new[] { "*" } }
            }
        });

        if (files.Count > 0)
            LoadFile(files[0].Path.LocalPath);
    }

    private void DropZone_DragEnter(object? sender, DragEventArgs e)
    {
        if (!e.Data.Contains(DataFormats.Files)) return;
        DropZoneBorder.Background = new SolidColorBrush(Color.FromArgb(0xCC, 0x22, 0x22, 0x22));
        DropZoneBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
        e.DragEffects = DragDropEffects.Copy;
    }

    private void DropZone_DragLeave(object? sender, DragEventArgs e)
    {
        DropZoneBorder.ClearValue(Border.BackgroundProperty);
        DropZoneBorder.ClearValue(Border.BorderBrushProperty);
    }

    private void DropZone_Drop(object? sender, DragEventArgs e)
    {
        DropZone_DragLeave(sender, e);
        var files = e.Data.GetFiles()?.ToArray();
        if (files?.Length > 0)
            LoadFile(files[0].Path.LocalPath);
    }

    private async void LoadFile(string path)
    {
        _srcPath = path;
        _mp4Info = null;

        var name = Path.GetFileName(path);
        DropLabel.Text = name.Length > 32 ? name[..29] + "…" : name;
        DropLabel.Foreground = Accent2;
        DropSubLabel.Text = "file ready · click Patch to process";

        DropZoneBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(0x88, 0x50, 0x50, 0x50));
        DropZoneBorder.Background = new SolidColorBrush(Color.FromArgb(0x20, 0x30, 0x30, 0x30));

        ResetStats();
        WarnBorder.IsVisible = false;
        SetProgress(0);
        SetButtonState(ButtonState.Analysing);
        LogClear();
        LogAccent(name);
        LogDim("scanning…");

        var info = await Task.Run(() => ParseMp4(path));
        _mp4Info = info;

        StatRes.Text = info.Width > 0 ? $"{info.Width}×{info.Height}" : "?";
        StatFps.Text = info.Fps.HasValue ? $"{info.Fps.Value} fps" : "?";
        StatBr.Text = info.Bitrate > 0 ? $"{info.Bitrate:F1} Mbps" : "?";
        StatMb.Text = $"{info.SizeMb:F1} MB";

        foreach (var tb in new[] { StatRes, StatFps, StatBr, StatMb })
            tb.Foreground = new SolidColorBrush(Color.FromRgb(0x77, 0x77, 0x77));

        LogInfo($"res      {StatRes.Text}");
        LogInfo($"fps      {StatFps.Text}");
        LogInfo($"bitrate  {StatBr.Text}");
        LogInfo($"size     {StatMb.Text}");

        if (info.Width > 1920 || info.Height > 1920)
        {
            WarnText.Text = $"Resolution {info.Width}\u00d7{info.Height} exceeds 1080p \u2014 will be auto-downscaled on encode.";
            WarnBorder.IsVisible = true;
            LogInfo("note: exceeds 1080p, will downscale on encode");
        }

        if (info.Width > 0 && info.Height > 0)
        {
            _srcW = info.Width;
            _srcH = info.Height;
            CustomWidthBox.Text = _srcW.ToString();
            CustomHeightBox.Text = _srcH.ToString();
        }
        double defaultBr = info.Bitrate > 0 ? Math.Round(Math.Min(info.Bitrate, 50.0)) : 20.0;
        CustomBitrateBox.Text = defaultBr.ToString(CultureInfo.InvariantCulture);
        CustomFpsBox.Text = (info.Fps.HasValue ? info.Fps.Value : 60).ToString();

        SetButtonState(ButtonState.Ready);
    }

    private async void ActionBtn_Click(object? sender, RoutedEventArgs e)
    {
        if (_srcPath == null) return;

        int customW = 0;
        int customH = 0;
        double customBitrateM = 20.0;
        int customFps = 60;

        if (_selectedPreset == 3)
        {
            if (!int.TryParse(CustomWidthBox.Text?.Trim(), out customW) || customW < 2)
            { LogError("invalid custom width \u2014 enter a positive integer"); return; }
            if (!int.TryParse(CustomHeightBox.Text?.Trim(), out customH) || customH < 2)
            { LogError("invalid custom height \u2014 enter a positive integer"); return; }
            if (!double.TryParse(CustomBitrateBox.Text?.Trim(), NumberStyles.Float,
                                 CultureInfo.InvariantCulture, out customBitrateM) || customBitrateM <= 0)
            { LogError("invalid custom bitrate \u2014 enter a positive number"); return; }
            if (!int.TryParse(CustomFpsBox.Text?.Trim(), out customFps) || customFps < 1)
            { LogError("invalid custom fps \u2014 enter a positive integer"); return; }
            customW = Math.Max(2, (customW / 2) * 2);
            customH = Math.Max(2, (customH / 2) * 2);
        }

        var sp = TopLevel.GetTopLevel(this)!.StorageProvider;
        var saveFile = await sp.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save patched file",
            DefaultExtension = ".mp4",
            FileTypeChoices = new[] { new FilePickerFileType("MP4 files") { Patterns = new[] { "*.mp4" } } },
            SuggestedFileName = Path.GetFileNameWithoutExtension(_srcPath) + "_patched",
        });
        if (saveFile == null) return;
        string savePath = saveFile.Path.LocalPath;

        string workingPath = _srcPath;
        bool cleanupWorking = false;

        try
        {
            string? scaleFilter;
            double bitrateM;
            int forceFps;
            switch (_selectedPreset)
            {
                case 0:
                    scaleFilter = $"scale={PresetDefs[0].MaxDim}:{PresetDefs[0].MaxDim}:force_original_aspect_ratio=decrease";
                    bitrateM = PresetDefs[0].BitrateM;
                    forceFps = 60;
                    break;
                case 1:
                    scaleFilter = $"scale={PresetDefs[1].MaxDim}:{PresetDefs[1].MaxDim}:force_original_aspect_ratio=decrease";
                    bitrateM = PresetDefs[1].BitrateM;
                    forceFps = 60;
                    break;
                case 2:
                    scaleFilter = $"scale={PresetDefs[2].MaxDim}:{PresetDefs[2].MaxDim}:force_original_aspect_ratio=decrease";
                    bitrateM = PresetDefs[2].BitrateM;
                    forceFps = 60;
                    break;
                default:
                    scaleFilter = $"scale={customW}:{customH}";
                    bitrateM = customBitrateM;
                    forceFps = customFps;
                    break;
            }

            string reason = _selectedPreset == 3
                ? $"custom {customW}\u00d7{customH} @ {customBitrateM:F0} Mbps {customFps} fps"
                : $"preset {PresetDefs[_selectedPreset].Name} @ {PresetDefs[_selectedPreset].BitrateM:F0} Mbps 60 fps";

            SetButtonState(ButtonState.Encoding);
            SetProgress(5);
            LogAccent($"\nencoding h265 @ {bitrateM:F0} Mbps: {reason}\u2026");

            bool ffmpegOk = await Task.Run(() => IsFfmpegAvailable());
            if (!ffmpegOk)
                throw new Exception("ffmpeg not found, install ffmpeg and ensure it is in PATH");

            string encoder = await Task.Run(() => DetectGpuEncoder(LogInfo));
            string gpuLabel = encoder switch
            {
                "hevc_nvenc" => "nvidia (nvenc)",
                "hevc_amf" => "amd (amf)",
                "hevc_qsv" => "intel (qsv)",
                _ => "cpu (libx265)",
            };
            LogInfo($"encoder  {gpuLabel}");
            if (scaleFilter != null) LogInfo($"scaling  \u2192 {scaleFilter}");
            LogInfo($"fps      \u2192 {forceFps}");

            double totalSecs = _mp4Info?.Duration ?? 0;

            workingPath = await Task.Run(() =>
                CompressVideo(_srcPath, encoder, scaleFilter, bitrateM, forceFps, totalSecs,
                              pct => SetProgress(5 + pct * 80),
                              LogDim,
                              LogStats));
            _statsRun = null;

            cleanupWorking = true;
            double encMb = new FileInfo(workingPath).Length / 1_048_576.0;
            LogInfo($"encoded  {encMb:F1} MB");

            SetButtonState(ButtonState.Patching);
            SetProgress(85);
            LogAccent("\napplying edit list patch\u2026");

            var (patched, msg) = await Task.Run(() => PatchFile(workingPath, savePath));
            SetProgress(100);

            if (workingPath != _srcPath && File.Exists(workingPath))
                try { File.Delete(workingPath); } catch { }

            if (msg != null) LogInfo(msg);

            if (patched)
            {
                LogAccent($"saved  \u2192  {Path.GetFileName(savePath)}");
                LogDim("upload this file to tiktok.");
                SetButtonState(ButtonState.Success);
                RevealInFileManager(savePath);
            }
            else
            {
                LogError("elst box not found \u2014 patch skipped.");
                LogDim("   the moov/edts/elst atom chain is missing from this file.");
                LogDim("   this can happen with recordings from certain encoders.");
                LogDim("   try remuxing with ffmpeg (-c copy) before patching.");
                SetButtonState(ButtonState.Ready);
            }
        }
        catch (Exception ex)
        {
            if (cleanupWorking && workingPath != _srcPath && File.Exists(workingPath))
                try { File.Delete(workingPath); } catch { }

            LogError($"error  {ex.GetType().Name}");
            LogError($"   {ex.Message}");
            SetProgress(0);
            SetButtonState(ButtonState.Ready);
        }
    }

    private static bool IsFfmpegAvailable() => TestBinary(_ffmpegExe);

    private static string DetectGpuEncoder(Action<string> log)
    {
        string[] candidates = ["hevc_nvenc", "hevc_amf", "hevc_qsv"];

        foreach (var enc in candidates)
        {
            log($"probing  {enc}\u2026");
            try
            {
                var psi = new ProcessStartInfo(_ffmpegExe)
                {
                    Arguments = $"-f lavfi -i nullsrc=s=128x128:d=0.04 -frames:v 1 -c:v {enc} -f null -",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };
                using var p = Process.Start(psi)!;
                p.WaitForExit(10_000);
                if (p.ExitCode == 0) return enc;
                log($"         not available (exit {p.ExitCode})");
            }
            catch (Exception ex) { log($"         failed \u2014 {ex.Message}"); }
        }

        return "libx265";
    }

    private static string CompressVideo(
        string srcPath,
        string encoder,
        string? scaleFilter,
        double bitrateM,
        int forceFps,
        double totalSeconds,
        Action<double> onProgress,
        Action<string> onLog,
        Action<string> onStats)
    {
        var dir = Path.GetDirectoryName(srcPath)!;
        var stem = Path.GetFileNameWithoutExtension(srcPath);
        var tmpPath = Path.Combine(dir, stem + "_enc_tmp.mp4");

        var vfParts = new List<string>();
        if (scaleFilter != null) vfParts.Add(scaleFilter);
        vfParts.Add($"fps={forceFps}");
        string vfArg = $"-vf \"{string.Join(',', vfParts)}\"";

        double maxrateM = Math.Round(bitrateM * 1.2);
        double bufsizeM = Math.Round(bitrateM * 2.0);

        string encArgs = encoder switch
        {
            "hevc_nvenc" => $"-c:v hevc_nvenc -rc vbr -b:v {bitrateM:F0}M -maxrate {maxrateM:F0}M -bufsize {bufsizeM:F0}M -preset p7 -spatial_aq 1 -temporal_aq 1",
            "hevc_amf" => $"-c:v hevc_amf -b:v {bitrateM:F0}M -maxrate {maxrateM:F0}M -bufsize {bufsizeM:F0}M -quality quality",
            "hevc_qsv" => $"-c:v hevc_qsv -b:v {bitrateM:F0}M -maxrate {maxrateM:F0}M -bufsize {bufsizeM:F0}M -look_ahead 1",
            _ => $"-c:v libx265 -b:v {bitrateM:F0}M -bufsize {bufsizeM:F0}M -preset slow",
        };

        onLog($"cmd      ffmpeg -i <src> {vfArg} {encArgs} -c:a copy -tag:v hvc1 <out>");

        string args = $"-y -i \"{srcPath}\" {vfArg} {encArgs} -c:a copy -tag:v hvc1 \"{tmpPath}\"";

        var psi = new ProcessStartInfo(_ffmpegExe)
        {
            Arguments = args,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        Process proc;
        try { proc = Process.Start(psi)!; }
        catch (Win32Exception)
        {
            throw new Exception("ffmpeg could not be started \u2014 ensure it is installed and in PATH");
        }

        var timeRegex = new Regex(@"time=(\d+):(\d+):([\d.]+)", RegexOptions.Compiled);
        var allStderr = new List<string>();
        var lastStatTick = new long[] { 0L };

        proc.ErrorDataReceived += (_, ev) =>
        {
            var line = ev.Data?.Trim() ?? "";
            if (line.Length == 0) return;

            allStderr.Add(line);

            if (line.StartsWith("Error", StringComparison.OrdinalIgnoreCase)
             || line.StartsWith("Invalid", StringComparison.OrdinalIgnoreCase)
             || line.StartsWith("Unknown encoder", StringComparison.OrdinalIgnoreCase)
             || line.StartsWith("Conversion failed", StringComparison.OrdinalIgnoreCase)
             || line.Contains("[error]", StringComparison.OrdinalIgnoreCase)
             || line.Contains("No such file", StringComparison.OrdinalIgnoreCase))
            {
                onLog($"ffmpeg   {line}");
            }

            if (line.StartsWith("frame="))
            {
                long now = Environment.TickCount64;
                if (now - lastStatTick[0] >= 1_000)
                {
                    lastStatTick[0] = now;
                    onStats(line);
                }
            }

            if (totalSeconds > 0)
            {
                var m = timeRegex.Match(line);
                if (m.Success)
                {
                    double secs = int.Parse(m.Groups[1].Value) * 3_600
                                + int.Parse(m.Groups[2].Value) * 60
                                + double.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture);
                    onProgress(Math.Min(secs / totalSeconds, 1.0));
                }
            }
        };

        proc.BeginErrorReadLine();
        proc.WaitForExit();

        if (proc.ExitCode != 0)
        {
            onLog($"ffmpeg exited with code {proc.ExitCode} \u2014 last output:");
            foreach (var l in allStderr.TakeLast(10))
                onLog($"   {l}");
            throw new Exception($"ffmpeg exited with code {proc.ExitCode}");
        }

        return tmpPath;
    }

    private static (bool patched, string? logMsg) PatchFile(string srcPath, string outputPath)
    {
        byte[] data = File.ReadAllBytes(srcPath);
        int ei = FindBox(data, 0x65, 0x6C, 0x73, 0x74);
        if (ei == -1) return (false, "elst not found");

        data[ei + 8] = 0x10;
        data[ei + 9] = 0x00;
        data[ei + 10] = 0x00;
        data[ei + 11] = 0x01;

        File.WriteAllBytes(outputPath, data);
        return (true, "elst+8 \u2190 0x10000001");
    }

    private record Mp4Info(int Width, int Height, int? Fps, double Bitrate, double SizeMb, double Duration);

    private static Mp4Info ParseMp4(string path)
    {
        var fi = new FileInfo(path);
        double szMb = fi.Length / 1_048_576.0;

        try { return ParseWithFfprobe(path, szMb); }
        catch { /* fall through to binary parser */ }

        return ParseBinary(fi, szMb);
    }

    private static Mp4Info ParseWithFfprobe(string path, double szMb)
    {
        var psi = new ProcessStartInfo(_ffprobeExe)
        {
            Arguments = $"-v quiet -print_format json -show_streams -show_format \"{path}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        using var proc = Process.Start(psi) ?? throw new InvalidOperationException("ffprobe not found");
        string json = proc.StandardOutput.ReadToEnd();
        proc.WaitForExit();
        if (proc.ExitCode != 0) throw new InvalidOperationException("ffprobe exited with error");

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        int w = 0, h = 0;
        int? fps = null;
        double duration = 0, bitrate = 0;

        if (root.TryGetProperty("streams", out var streams))
        {
            foreach (var stream in streams.EnumerateArray())
            {
                if (!stream.TryGetProperty("codec_type", out var ct) ||
                    ct.GetString() != "video") continue;

                if (stream.TryGetProperty("width", out var wEl)) w = wEl.GetInt32();
                if (stream.TryGetProperty("height", out var hEl)) h = hEl.GetInt32();

                if (stream.TryGetProperty("r_frame_rate", out var fpsEl))
                {
                    var parts = fpsEl.GetString()?.Split('/');
                    if (parts?.Length == 2
                        && int.TryParse(parts[0], out int num)
                        && int.TryParse(parts[1], out int den)
                        && den > 0 && num > 0)
                        fps = (int)Math.Round((double)num / den);
                }
                break;
            }
        }

        if (root.TryGetProperty("format", out var fmt))
        {
            if (fmt.TryGetProperty("duration", out var durEl)
                && double.TryParse(durEl.GetString(), NumberStyles.Float,
                                   CultureInfo.InvariantCulture, out double d))
                duration = d;

            if (fmt.TryGetProperty("bit_rate", out var brEl)
                && double.TryParse(brEl.GetString(), NumberStyles.Float,
                                   CultureInfo.InvariantCulture, out double br))
                bitrate = br / 1_000_000.0;
        }

        return new Mp4Info(w, h, fps, bitrate, szMb, duration);
    }

    private static Mp4Info ParseBinary(FileInfo fi, double szMb)
    {
        int w = 0, h = 0;
        int? fps = null;
        double duration = 0, bitrate = 0;

        try
        {
            int readLen = (int)Math.Min(fi.Length, 20_000_000);
            byte[] buf = new byte[readLen];
            using var fs = File.OpenRead(fi.FullName);
            int read = fs.Read(buf, 0, readLen);

            int ti = FindBox(buf, 0x74, 0x6B, 0x68, 0x64);
            if (ti != -1)
            {
                byte ver = buf[ti + 4];
                int wOff = ver == 1 ? ti + 92 : ti + 80;
                if (wOff + 8 <= read)
                {
                    w = ReadBE(buf, wOff) >> 16;
                    h = ReadBE(buf, wOff + 4) >> 16;
                }
            }

            int mi = FindBox(buf, 0x6D, 0x76, 0x68, 0x64);
            if (mi != -1 && mi + 24 <= read)
            {
                byte ver = buf[mi + 4];
                long dur; uint ts;
                if (ver == 1 && mi + 36 <= read)
                {
                    ts = (uint)ReadBE(buf, mi + 24);
                    dur = (long)ReadBE(buf, mi + 28) << 32 | (uint)ReadBE(buf, mi + 32);
                }
                else
                {
                    ts = (uint)ReadBE(buf, mi + 16);
                    dur = (uint)ReadBE(buf, mi + 20);
                }
                duration = ts > 0 ? (double)dur / ts : 0;
            }

            if (duration > 0) bitrate = fi.Length * 8.0 / duration / 1_000_000.0;

            int vi = FindBox(buf, 0x76, 0x69, 0x64, 0x65);
            if (vi != -1)
            {
                int si = FindBox(buf, 0x73, 0x74, 0x73, 0x7A, vi);
                if (si != -1 && si + 16 <= read)
                {
                    int frames = ReadBE(buf, si + 12);
                    if (duration > 0 && frames > 0)
                        fps = (int)Math.Round(frames / duration);
                }
            }
        }
        catch { }

        return new Mp4Info(w, h, fps, bitrate, szMb, duration);
    }

    private static int FindBox(byte[] buf, byte a, byte b, byte c, byte d, int start = 0)
    {
        int lim = buf.Length - 4;
        for (int i = start; i < lim; i++)
            if (buf[i] == a && buf[i + 1] == b && buf[i + 2] == c && buf[i + 3] == d) return i;
        return -1;
    }

    private static int ReadBE(byte[] buf, int o) =>
        (buf[o] << 24) | (buf[o + 1] << 16) | (buf[o + 2] << 8) | buf[o + 3];

    private enum ButtonState { Analysing, Ready, Encoding, Patching, Success }

    private void SetButtonState(ButtonState state)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            switch (state)
            {
                case ButtonState.Analysing:
                    ActionBtnText.Text = "ANALYSING…";
                    ActionBtnText.Foreground = Muted;
                    ActionBtn.IsEnabled = false;
                    ActionBtn.BorderBrush = Border_;
                    break;

                case ButtonState.Ready:
                    ActionBtnText.Text = "PATCH  &  SAVE";
                    ActionBtnText.Foreground = Accent;
                    ActionBtn.IsEnabled = true;
                    ActionBtn.BorderBrush = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44));
                    break;

                case ButtonState.Encoding:
                    ActionBtnText.Text = "ENCODING…";
                    ActionBtnText.Foreground = Muted;
                    ActionBtn.IsEnabled = false;
                    ActionBtn.BorderBrush = Border_;
                    break;

                case ButtonState.Patching:
                    ActionBtnText.Text = "PATCHING…";
                    ActionBtnText.Foreground = Muted;
                    ActionBtn.IsEnabled = false;
                    ActionBtn.BorderBrush = Border_;
                    break;

                case ButtonState.Success:
                    ActionBtnText.Text = "\u2713  PATCHED, UPLOAD TO TIKTOK";
                    ActionBtnText.Foreground = Accent2;
                    ActionBtn.IsEnabled = false;
                    ActionBtn.BorderBrush = new SolidColorBrush(Color.FromRgb(0x38, 0x38, 0x38));
                    break;
            }
        });
    }

    private void SetProgress(double pct)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            if (ProgressBar.Parent is Grid parent)
                ProgressBar.Width = parent.Bounds.Width * pct / 100.0;
            ProgressPct.Text = pct > 0 ? $"{(int)pct}%" : "—";
        });
    }

    private void ResetStats()
    {
        var dim = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44));
        foreach (var tb in new[] { StatRes, StatFps, StatBr, StatMb })
        {
            tb.Text = "—";
            tb.Foreground = dim;
        }
    }

    private void LogClear()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            LogBlock.Inlines!.Clear();
            _statsRun = null;
        });
    }

    private void LogAccent(string msg) => AppendLog("\u203a  " + msg, Accent);
    private void LogInfo(string msg) => AppendLog("   " + msg, Mid);
    private void LogError(string msg) => AppendLog("\u203a  " + msg, Red);
    private void LogDim(string msg) => AppendLog("\u203a  " + msg, Muted);

    private void LogStats(string msg)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            if (_statsRun is null)
            {
                _statsRun = new Run { Text = "   " + msg + "\n", Foreground = Muted };
                LogBlock.Inlines!.Add(_statsRun);
            }
            else
            {
                _statsRun.Text = "   " + msg + "\n";
            }
            LogScroll.SetCurrentValue(ScrollViewer.OffsetProperty,
                new Vector(LogScroll.Offset.X, double.MaxValue));
        });
    }

    private void AppendLog(string msg, IBrush colour)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            LogBlock.Inlines!.Add(new Run { Text = msg + "\n", Foreground = colour });
            LogScroll.SetCurrentValue(ScrollViewer.OffsetProperty,
                new Vector(LogScroll.Offset.X, double.MaxValue));
        });
    }

    private static void RevealInFileManager(string filePath)
    {
        if (OperatingSystem.IsWindows())
            Process.Start("explorer.exe", $"/select,\"{filePath}\"");
        else if (OperatingSystem.IsMacOS())
            Process.Start("open", $"-R \"{filePath}\"");
        else
            Process.Start(new ProcessStartInfo
            {
                FileName = "xdg-open",
                Arguments = $"\"{Path.GetDirectoryName(filePath)}\"",
                UseShellExecute = true,
            });
    }
}
