# video-dupe-finder-ui-mod

A fork of [videoduplicatefinder](https://github.com/0x90d/videoduplicatefinder) with a fully redesigned light-mode Web UI, built for personal use on Windows.

The core engine (VDF.Core) is unchanged — all scanning, hashing, and perceptual comparison logic is inherited from upstream. Only the web frontend has been replaced.

---

## What's Different

| Original | This Fork |
|---|---|
| Dark mode flat list | Light mode card-per-group layout |
| Single scrollable table | Horizontal file columns (280px each) |
| No folder ordering | Configurable main folder (leftmost column) |
| No color pairing | Color-coded matching metadata values |
| Generic auto-select | Smart Select per group + global with clear-before-apply |

---

## Features

- **Card-per-group layout** — each duplicate group is a horizontal row of file columns
- **280px fixed columns** — files side by side, buttons and metadata always level regardless of filename length
- **Thumbnail preview** — lazy-loaded frame grabs for each file
- **Color-coded metadata** — matching values highlighted in blue/purple/amber; unique (differing) values in red
- **Smart Select** — global and per-group dropdown to flag lowest resolution, smallest file, oldest, newest, shortest, longest
  - Clears previous selection before applying new one
  - Unselect All button globally and per group
- **Folder browser** — click Browse to navigate drives and pick folders visually instead of typing paths
- **Main folder** — first include folder is starred (Main); its files appear as the leftmost column in every group card
- **Column order** — files in each group sort by include folder order (main = col 1, second = col 2, etc.)
- **Alphabetical group sort** — groups ordered A to Z by the main folder's filename
- **Filter by path** — live search to narrow results while scrolling
- **Safety mode** — deletion requires confirmation by default; Raw Mode skips prompts
- **Recycle Bin only** — all deletions go to Recycle Bin, no permanent delete
- **Deleted column greyed out** — deleted files stay visible in the card with an overlay so you know what was removed
- **Complete and Clear** — removes a group from results without deleting files
- **Status bar** — live group count, remaining files, reclaimed space

---

## How to Run

### Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- FFmpeg and FFprobe on PATH (or in the `bin/` subfolder)

### From Source

```powershell
git clone https://github.com/shioneko2026/video-dupe-finder-ui-mod.git
cd video-dupe-finder-ui-mod
dotnet run --project VDF.Web
```

Then open `http://localhost:5000` in your browser.

### With the Launcher (Windows)

A `Launch VDF.bat` file is included one level above the repo. Double-click it to:

1. Start the server in a minimized terminal
2. Wait until the server is ready (polls every 2 seconds)
3. Open `http://localhost:5000` in your default browser automatically

To stop the app, close the minimized VDF Server terminal window.

---

## Workflow

1. **Scan tab** — add include folders (Browse or type). First folder added = Main folder. Start scan.
2. **Results tab** — review duplicate groups. Main folder's file is always leftmost.
3. Use **Flag** or click a column to mark files for deletion.
4. Use **Smart Select** (per group or global) to auto-flag by quality, size, or date.
5. **Delete Marked** or **Delete Flagged** (per group) sends files to Recycle Bin.
6. **Complete and Clear** removes a resolved group from the list.

---

## Upstream

This fork tracks [0x90d/videoduplicatefinder](https://github.com/0x90d/videoduplicatefinder). To pull upstream changes:

```powershell
git fetch upstream
git merge upstream/master
```

Only `VDF.Web/` has been modified — merges should be clean unless upstream changes the web layer.

---

## License

AGPL-3.0 — same as upstream.
