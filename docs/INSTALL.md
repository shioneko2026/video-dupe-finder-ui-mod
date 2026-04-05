# How to Install for Normal Users

A plain-English guide for someone who just wants to find duplicate videos. No technical knowledge required.

---

## What this app does

It scans folders on your computer, finds videos that are duplicates of each other, and lets you delete the ones you don't want — safely, to the Recycle Bin.

---

## What you need

- A Windows computer (Windows 10 or 11)
- An internet connection for the first launch (it downloads one thing automatically — see below)
- That's it. Nothing else to install.

---

## Step 1 — Get the app

The developer will share a zip file with you (e.g. `VideoDupeFinder.zip`).

1. Download it and save it somewhere you can find it (e.g. your Desktop)
2. **Right-click the zip file → Extract All**
3. Choose where to extract it (Desktop is fine), then click Extract

> **Important:** Do not try to run it from inside the zip file. Extract it first, then open the extracted folder.

---

## Step 2 — Launch it

Inside the extracted folder, find the file called **`Start VDF.bat`**.

Double-click it.

### If you see a "Windows protected your PC" warning

This is normal. Windows shows this warning for any app that wasn't downloaded from the Microsoft Store. The app is safe.

1. Click **More info**
2. Click **Run anyway**

### What happens next

A small black terminal window will appear in your taskbar — this is the server running in the background. **Do not close it.** It's supposed to be there.

The launcher will wait until the server is ready, then open your browser automatically.

---

## Step 3 — First-time setup (automatic)

The very first time you launch, the app needs to download **FFmpeg** — the tool it uses to read video files. This happens automatically, no action needed from you.

You'll see a blue progress bar on the Scan page while it downloads. It's about 60–80 MB and takes 1–2 minutes on a normal connection.

**This only happens once.** After the first launch it starts instantly.

---

## Step 4 — Using the app

Once the browser opens and the progress bar is gone, you're ready.

### Scanning for duplicates

1. Go to the **Scan** tab
2. Click **Browse** and navigate to the folder you want to scan
3. The first folder you add is the "Main" folder — its files will always appear on the left when comparing
4. Add more folders if you want to compare across multiple locations
5. Click **Start Scan** and wait

### Reviewing results

1. Go to the **Results** tab after the scan completes
2. Each group of duplicates appears as a card with the files side by side
3. Matching values are colored blue/purple — different values are red, so you can spot differences at a glance
4. Click a file column to mark it for deletion (it turns green)
5. Use **Smart Select** to automatically flag the lower-quality copy in each group

### Deleting duplicates

- Click **Delete Flagged** in a group to send that group's marked files to the Recycle Bin
- Or click **Delete Marked** in the toolbar to delete everything marked across all groups
- Files go to the **Recycle Bin only** — nothing is permanently deleted
- If you change your mind, open the Recycle Bin and restore the file

### Stopping the app

Close the minimised server window in your taskbar. That's it.

---

## Troubleshooting

**"Windows protected your PC" — won't let me click Run anyway**
Right-click the `Start VDF.bat` file → Properties → at the bottom, check "Unblock" → OK. Then try launching again.

**Browser opens but nothing loads / page says "connection refused"**
The server might still be starting. Wait 10–15 seconds and refresh the page.

**Browser never opens at all**
Check if the server window shows a red error message. If it says "address already in use", another app is using port 5000 — close other apps and try again.

**The FFmpeg download failed**
The app will show a red message explaining what happened. You can download FFmpeg manually:
1. Go to https://ffmpeg.org/download.html
2. Download the Windows build
3. Copy `ffmpeg.exe` and `ffprobe.exe` into the `bin\` folder next to `VDF.Web.exe`
4. Restart the app

**The scan finished but no results showed up**
Either no duplicates were found, or the similarity threshold is set too high. Go to Settings and lower the minimum similarity percentage.

---

## Notes for the person sharing this app

Before sending the zip to someone:

1. Run `publish.bat` from the repo root — this builds the release into `release\app\`
2. Zip the entire `release\app\` folder
3. Share the zip

The recipient follows this guide from Step 1. No installs, no setup required on their end.
