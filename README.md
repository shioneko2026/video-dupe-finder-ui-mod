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

## Screenshot

![Results page](docs/screenshot.jpg)

---

## Features

### Side-by-side comparison that actually works

The original VDF results page is a flat list — you scroll down through rows, mentally piecing together which files belong together. This fork throws that out entirely.

Every duplicate group gets its own **card**. Inside each card, the files sit side-by-side as fixed-width columns — thumbnails, metadata, and action buttons all lined up at the same height. You see every file in a group at once, in one glance, without scrolling or cross-referencing rows.

### Color coding tells you what matters before you even read it

Every metadata value is colored based on whether it matches across the group:

- **Blue / Purple / Amber** — values that are shared (two or more files have the same value)
- **Red** — values unique to that file (this is where the files differ)

Duration, file size, resolution, format, FPS, bitrate, audio — all color-coded independently. When you open a card, the red values immediately draw your eye to the differences. No mental comparison needed.

### Consistent left-right layout across every group

When you're scanning through dozens of groups, predictability matters. This fork lets you designate a **Main folder** — the first folder you add gets a ★ badge. Its file always appears as the leftmost column in every card. Your second folder is always second, and so on.

Combined with **alphabetical sort by the main folder's filename**, the results page reads like a sorted list you already know — Episode 01, 02, 03... in order, with each episode's files always in the same left-right position. No hunting around to figure out which column is which.

The original VDF has no concept of folder ordering or consistent column position.

### Everything else

- **Folder browser** — pick folders by clicking through drives and directories instead of pasting paths
- **Smart Select** — auto-flag lowest resolution, smallest file, oldest, newest, shortest, or longest, per group or globally; always clears the previous selection first
- **Filter by path** — live search to narrow down results
- **Safety mode** — bulk deletions require confirmation by default; Raw Mode skips the prompts
- **Recycle Bin only** — nothing is permanently deleted; restore from Windows Recycle Bin if you change your mind
- **Deleted columns stay visible** — greyed out with an overlay so you can see what was removed before moving on
- **Complete and Clear** — dismiss a resolved group without deleting anything

---

## For Normal Users

No technical knowledge required. You need a Windows PC and an internet connection for the first launch — that's it.

1. **Download** the zip from the [Releases page](../../releases) and save it somewhere you can find it
2. **Extract** — right-click the zip → Extract All → Extract (do not run from inside the zip)
3. **Launch** — open the extracted folder and double-click **`Start VDF.bat`**

If Windows shows a **"Windows protected your PC"** warning: click **More info → Run anyway**. This is normal for apps not published on the Microsoft Store.

A small server window will appear in your taskbar — leave it open. Your browser will open automatically once the server is ready.

**First launch only:** the app downloads FFmpeg (the tool it uses to read video files) automatically — about 60–80 MB, takes 1–2 minutes. You'll see a progress bar on the Scan page. It only happens once.

→ **[Full install guide with troubleshooting](docs/INSTALL.md)**

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
