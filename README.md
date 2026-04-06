# video-dupe-finder-ui-mod

A fork of [videoduplicatefinder](https://github.com/0x90d/videoduplicatefinder) with a fully redesigned light-mode Web UI, built for personal use on Windows.

The original does the hard work well — scanning, hashing, perceptual comparison. What this fork changes is the *review* experience. The upstream results page is a flat scrollable list that makes you mentally piece together which files belong together. This fork replaces it with a card-per-group layout: files sit side by side, metadata lines up at the same height, and color coding immediately shows you where the differences are — before you've read a single number.

The core engine (VDF.Core) is unchanged — all scanning, hashing, and perceptual comparison logic is inherited from upstream. Only the web frontend has been replaced.

---

## What's Different

| Original | This Fork |
|---|---|
| Dark mode flat list | Light mode card-per-group layout |
| Single scrollable table | Horizontal file columns (280px each) |
| No folder ordering | Configurable main folder (leftmost column) |
| No color pairing | Color-coded matching metadata values |
| Generic auto-select | Smart Select per group + global |

---

## Screenshot

![Results page](docs/screenshot.jpg)

---

## Features

### Side-by-side comparison that actually works

Every duplicate group gets its own **card**. Inside each card, the files sit side-by-side as fixed-width columns — thumbnails, metadata, and action buttons all lined up at the same height. You see every file in a group at once, in one glance, without scrolling or cross-referencing rows.

### Color coding tells you what matters before you even read it

Every metadata value is colored based on whether it matches across the group:

- **Blue / Purple / Amber** — values shared by two or more files
- **Red** — values unique to that file (where the files differ)

Duration, file size, resolution, format, FPS, bitrate, audio — all color-coded independently. Red values draw your eye to the differences immediately.

### Consistent left-right layout across every group

Designate a **Main folder** — the first folder you add gets a ★ badge. Its file always appears as the leftmost column in every card, every time. Combined with alphabetical sort by the main folder's filename, the results page reads like a sorted list you already know.

### Smart Select

Auto-flag files by criteria, per group or globally:

- **Global:** Lowest quality, smallest file, oldest, newest, 100% equal groups, No audio track, Invert
- **Per group:** Lowest/highest resolution, smallest/largest, shortest/longest, no audio track

All Smart Select modes clear the previous selection before applying — no accidental stacking.

### Deletion controls

- **On Delete: Grey Out** — deleted columns stay visible at 50% opacity with a ✕ overlay so you can see what was removed before moving on
- **On Delete: Remove** — columns disappear immediately
- All deletions go to the **Recycle Bin** — nothing permanently deleted; restore from Windows Recycle Bin if needed
- **Safety mode** — bulk deletions require confirmation by default; toggle off for Raw Mode

### Workflow tools

- **Rescan** button — re-run the scan with the same folders without going back to the Scan tab
- **Clear Resolved** — removes groups where all but one file has been deleted (no more clutter)
- **Complete and Clear** — dismiss a resolved group without deleting anything
- **Skip** — same as Complete and Clear
- **Beep + auto-redirect** — plays a tone and navigates to Results automatically when a scan finishes (toggle in Settings)

### Everything else

- **Folder browser** — pick folders by clicking through drives and directories instead of pasting paths
- **Filter by path** — live search to narrow results to folders you care about
- **Deleted columns stay visible** — greyed out with overlay so context isn't lost
- **FFmpeg auto-download** — downloads FFmpeg automatically on first launch; no manual setup required
- **Status bar** — group count, remaining files, reclaimed space, flagged count

---

## For Normal Users

No technical knowledge required. You need a Windows PC and an internet connection for the first launch.

1. **Download** the zip from the [Releases page](../../releases) and save it somewhere you can find it
2. **Extract** — right-click the zip → Extract All → Extract (do not run from inside the zip)
3. **Launch** — open the extracted folder and double-click **`Start VDF.bat`**

If Windows shows a **"Windows protected your PC"** warning: click **More info → Run anyway**. This is normal for apps not published on the Microsoft Store.

A small server window will appear in your taskbar — leave it open. Your browser will open automatically once the server is ready.

**First launch only:** the app downloads FFmpeg automatically — about 60–80 MB, takes 1–2 minutes. You'll see a progress bar on the Scan page. It only happens once.

→ **[Full install guide with troubleshooting](HOW%20TO%20INSTALL.md)**

---

## How to Run (from source)

### Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- FFmpeg and FFprobe on PATH (or in the `bin/` subfolder next to the exe)

### From Source

```powershell
git clone https://github.com/shioneko2026/video-dupe-finder-ui-mod.git
cd video-dupe-finder-ui-mod
dotnet run --project VDF.Web
```

Then open `http://localhost:5000` in your browser.

---

## Workflow

1. **Scan tab** — add include folders (Browse or type). First folder added = Main folder. Start scan.
2. **Results tab** — review duplicate groups. Main folder's file is always leftmost.
3. Click a column or use **Flag** to mark files for deletion.
4. Use **Smart Select** (per group or global) to auto-flag by quality, size, date, or audio.
5. **Delete Marked** or **Delete Flagged** (per group) sends files to Recycle Bin.
6. **Complete and Clear** or **Clear Resolved** to clean up finished groups.

---

## Upstream

This fork tracks [0x90d/videoduplicatefinder](https://github.com/0x90d/videoduplicatefinder). Only `VDF.Web/` has been modified — merges should be clean unless upstream changes the web layer.

---

## License

AGPL-3.0 — same as upstream.
