# FPS Method

Upload to TikTok without TikTok destroying your video.

FPS Method re-encodes your clip to H.265 at 60fps, then patches a metadata box in the MP4 container that causes TikTok's upload pipeline to skip its recompression step. The result is noticeably sharper video on the platform compared to a straight upload.

Everything runs on your machine. Nothing leaves your computer.

---

## Download

Grab a build from [Releases](../../releases) — single-file executables for Windows, macOS (Intel + Apple Silicon) and Linux, no installer needed.

---

## Usage

1. Drop your video onto the app, or click the drop zone to browse
2. Check the video info: resolution, fps, bitrate, file size
3. Pick a quality preset
4. Hit **Patch & Save** and choose where to save
5. Upload the output file to TikTok

The app will re-encode and patch in one pass. The whole thing usually takes a minute or two depending on your hardware and the clip length.

### Quality presets

| Preset | Max resolution | Bitrate |
|--------|---------------|---------|
| 1080p  | 1920px        | 20 Mbps |
| 720p   | 1280px        | 10 Mbps |
| 540p   | 960px         | 6 Mbps  |
| Custom | your choice   | your choice |

All fixed presets force **60fps**, which is part of what makes the trick work. Custom lets you set your own resolution, bitrate, and frame rate. Resolution is aspect-ratio locked to your source clip — type in a width and the height updates automatically when you tab out.

Encoding uses your GPU when possible — NVIDIA (NVENC), AMD (AMF), and Intel (QSV) are all detected automatically. Falls back to CPU (libx265) if none are found.

---

## Building from source

```bash
git clone https://github.com/your-username/fps-method
cd fps-method/FpsMethod
dotnet run
```

To produce release binaries for all platforms at once, run `build-all.bat` on Windows. Outputs land in `dist/`.

```
dist/
  win-x64/    FpsMethod-win-x64.exe
  linux-x64/  FpsMethod-linux-x64
  osx-x64/    FpsMethod-osx-x64
  osx-arm64/  FpsMethod-osx-arm64
```

### Requirements

- uhhh idk like 1GB of storage ig
