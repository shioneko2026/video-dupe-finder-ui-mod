> For the user-facing README, see [README.md](README.md).

# README for Developers

Technical reference for contributors, future maintainers, and anyone picking this up after a gap.

---

## Tech Stack & Dependencies

| Layer | Technology |
|---|---|
| Runtime | .NET 9 |
| Web framework | ASP.NET Core + Blazor Server |
| Real-time transport | SignalR (via Blazor Server circuit) |
| Image processing | SixLabors.ImageSharp |
| Video/audio analysis | FFmpeg + FFprobe (external, auto-downloaded) |
| Scan engine | VDF.Core (upstream, unmodified) |

No npm, no JavaScript framework, no database. State lives in singleton C# services.

---

## Project Structure

```
video-dupe-finder-ui-mod/
├── VDF.Core/                        # Upstream scan engine — do not modify
│   └── ScanEngine.cs                # Perceptual hashing, FFmpeg pipeline, thumbnail retrieval
├── VDF.Web/
│   ├── Components/
│   │   ├── Layout/
│   │   │   └── MainLayout.razor     # Top nav, vdfBeep() JS function
│   │   └── Pages/
│   │       ├── Index.razor          # Scan page — folder config, FFmpeg banner, scan trigger
│   │       ├── Results.razor        # Main results UI — all card/group/delete logic lives here
│   │       └── Settings.razor       # Settings form — maps to VDF.Core Settings + WebSettingsService
│   ├── Services/
│   │   ├── ScanService.cs           # Singleton — owns ScanEngine, exposes scan lifecycle
│   │   ├── WebSettingsService.cs    # Persists settings to web-settings.json
│   │   └── FFmpegSetupService.cs    # Checks for / downloads FFmpeg on first launch
│   ├── wwwroot/
│   │   └── app.css                  # All custom styles — light mode, cards, toolbar, buttons
│   └── Program.cs                   # DI registration, SignalR config, timeout tuning
├── publish.bat                      # Builds self-contained win-x64 exe to C:\VDF_build\app\
├── Launch VDF.bat                   # Dev launcher — starts dotnet run, polls until ready, opens browser
└── HOW TO INSTALL.md                # Plain-English install guide for non-technical users
```

---

## How to Run from Source

**Requirements:** .NET 9 SDK. FFmpeg on PATH, or place `ffmpeg.exe` + `ffprobe.exe` in a `bin/` folder next to the exe (or in the working directory when running from source).

```powershell
git clone https://github.com/shioneko2026/video-dupe-finder-ui-mod.git
cd video-dupe-finder-ui-mod
dotnet run --project VDF.Web
```

Then open `http://localhost:5000`.

Or double-click `Launch VDF.bat` — starts the server minimized and opens the browser automatically once ready.

**NuGet source issue:** On a fresh clone, `dotnet run` may fail with `NU1100: Unable to resolve`. Fix:
```powershell
dotnet restore VDF.Web --source https://api.nuget.org/v3/index.json
```

---

## Build for Distribution

```powershell
dotnet publish VDF.Web/VDF.Web.csproj -c Release -r win-x64 --self-contained true -o "C:\VDF_build\app"
```

**Important:** The output path must not contain `[` or `]` characters — dotnet publish silently produces 0 files when the output path contains square brackets. Use a plain path like `C:\VDF_build\app`.

The `publish.bat` script handles this and also writes `Start VDF.bat` and `HOW TO USE.txt` into the output folder.

---

## Config Reference

Settings are persisted in `web-settings.json`, written next to the exe (published) or in the working directory (dev). It is created automatically on first save.

| Key | Default | Description |
|---|---|---|
| `IncludeList` | `[]` | Include folder paths |
| `BlackList` | `[]` | Exclude folder paths |
| `Threshhold` | `5` | Comparison threshold (lower = stricter) |
| `Percent` | `96.0` | Minimum similarity % to flag as duplicate |
| `PercentDurationDifference` | `20.0` | Max duration difference % to consider as duplicate |
| `MaxDegreeOfParallelism` | `1` | Threads for scan/thumbnail retrieval |
| `ThumbnailCount` | `1` | Thumbnails per video for comparison |
| `IncludeSubDirectories` | `true` | Recurse into subdirectories |
| `IncludeImages` | `true` | Include image files in scan |
| `IncludeNonExistingFiles` | `false` | Include database entries for files no longer on disk |
| `ScanAgainstEntireDatabase` | `false` | Compare against all previously scanned files, not just current include list |
| `UsePHashing` | `false` | Use perceptual hashing (slower, more accurate) |
| `HardwareAccelerationMode` | `None` | FFmpeg hardware decode mode |
| `CustomFFArguments` | `""` | Extra FFmpeg arguments |
| `BeepOnScanComplete` | `true` | Play beep and auto-redirect to Results when scan finishes |

`BeepOnScanComplete` is a UI-only preference stored in the same file but handled by `WebSettingsService` separately from `VDF.Core Settings`.

---

## Architecture Notes

### Service model
`ScanService` is a singleton that owns the `ScanEngine` instance. Blazor components inject it and subscribe to its `StateChanged` event. All scan state (progress, duplicates, thumbnails) flows through this single service. Components call `StateHasChanged()` inside `InvokeAsync()` when the event fires.

### Blazor Server / SignalR circuit
Each browser tab gets a persistent SignalR circuit. `Nav.NavigateTo()` tears down the circuit and creates a new one — this is why "rejoining server" appears in logs on every page navigation. This is normal Blazor Server behavior, not a bug.

SignalR timeouts are configured in `Program.cs`:
- `ClientTimeoutInterval`: 120s
- `HandshakeTimeout`: 30s
- `KeepAliveInterval`: 20s

### Progress throttling
`ScanService` throttles `StateHasChanged` calls to max 4/sec (250ms gate on `_lastProgressNotify`). Without this, large scans flood SignalR with hundreds of events per second and cause disconnects.

### Delete modes
`ScanService.DeleteItems()` takes a `removeFromResults` parameter (default `true`). When `false`, the file is deleted from disk but the `DuplicateItem` stays in `_engine.Duplicates` — the UI greys it out. Controlled by the `_greyOutOnDelete` toggle in `Results.razor`.

### FFmpeg setup
`FFmpegSetupService` is a singleton that runs `EnsureAsync()` on page load. It checks `FFToolsUtils.GetPath()` first. If not found and the process is the published exe (detected by checking if `ProcessPath` starts with `dotnet`), it downloads the correct BtbN build for the detected architecture, extracts the zip, and copies all `ff*.exe` binaries to `bin/` next to the exe. Dev mode shows a manual-install message instead.

### Thumbnail retrieval
`ScanEngine.RetrieveThumbnails()` is `async void` — it fires from the `ScanDone` event handler and runs a `Parallel.ForEachAsync` over all duplicates. VDF.Core's `IncludeNonExistingFiles` defaults to `true`, which causes `needsThumbnails = false` for any file not on disk. Both thumbnail methods now guard against null `timeStamps` with an early return rather than the original `Debug.Assert` (which called `FailFast` and terminated the process).

### Column ordering
`Results.razor` has a `FolderOrder(item)` method that checks which include folder in `OrderedIncludeList` the file's path starts with. The index determines column position. Main folder (index 0) is always leftmost.

---

## Known Issues & Tech Debt

| ID | Issue | Impact |
|---|---|---|
| F-001 | `OrderedIncludeList` is not persisted — rebuilt from an unordered `HashSet` on restart, so folder order (and Main folder designation) may change | Medium |
| F-002 | Context menu fields in `Results.razor` (`_menuVisible`, `_menuX`, `_menuY`, `_menuItem`, `_menuGroup`) are stubbed but never used — generates CS0169 warnings | Low |
| F-003 | Group sort uses the main folder's filename; groups with no main-folder file fall back to alphabetically-first filename across all files | Low |
| F-004 | No deep-link or jump-to-group — large result sets require scrolling from the top | Low |

---

## Upstream / Credits

Forked from [0x90d/videoduplicatefinder](https://github.com/0x90d/videoduplicatefinder) (AGPL-3.0).

**What's changed:** Only `VDF.Web/` has been modified. `VDF.Core/` is upstream except for two null-guard fixes in `ScanEngine.cs` (lines ~1347 and ~1416) that replaced `Debug.Assert(timeStamps != null)` with early returns.

**Pulling upstream changes:**
```powershell
git remote add upstream https://github.com/0x90d/videoduplicatefinder.git
git fetch upstream
git merge upstream/master
```

Merges should be clean — upstream changes to `VDF.Core` integrate without conflict. Conflicts only arise if upstream also modifies `VDF.Web`.
