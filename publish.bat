@echo off
title VDF Publisher
echo ============================================================
echo  Video Dupe Finder — Build Self-Contained Release
echo ============================================================
echo.

cd /d "%~dp0"

echo [1/3] Cleaning previous release...
if exist release rmdir /s /q release
mkdir release\app

echo [2/3] Building self-contained Windows exe...
dotnet publish VDF.Web -c Release -r win-x64 --self-contained -o release\app
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Build failed. See output above.
    pause
    exit /b 1
)

echo [3/3] Creating launcher and instructions...

(
echo @echo off
echo title VDF Launcher
echo cd /d "%%~dp0"
echo start "VDF Server" /min cmd /k "VDF.Web.exe"
echo echo Waiting for server to start...
echo :wait
echo timeout /t 2 /nobreak ^>nul
echo powershell -Command "try { Invoke-WebRequest http://localhost:5000 -TimeoutSec 1 -UseBasicParsing ^| Out-Null; exit 0 } catch { exit 1 }"
echo if %%ERRORLEVEL%% NEQ 0 goto wait
echo start "" http://localhost:5000
echo exit
) > release\app\Start VDF.bat

(
echo Video Dupe Finder — How To Use
echo ================================
echo.
echo 1. Double-click "Start VDF.bat" to launch the app.
echo    A small terminal window will appear — this is the server. Do not close it.
echo    Your browser will open automatically when the server is ready.
echo.
echo 2. The first time it starts, it will automatically download FFmpeg
echo    ^(the tool that reads video files^). This takes a minute or two
echo    depending on your internet speed. It only happens once.
echo.
echo 3. In the browser, go to the Scan tab.
echo    Click Browse to choose a folder, then click Start Scan.
echo.
echo 4. After scanning, go to the Results tab to review duplicates.
echo    Click a file column to flag it. Click Delete Flagged to send it to the Recycle Bin.
echo    Nothing is permanently deleted — you can restore from the Recycle Bin if needed.
echo.
echo 5. To stop the app, close the server terminal window.
echo.
echo ---- Troubleshooting ----
echo.
echo App won't start: make sure you extracted the full zip folder before running.
echo FFmpeg download fails: download ffmpeg.exe and ffprobe.exe manually from
echo   https://ffmpeg.org/download.html and place them in the bin\ folder.
echo Port already in use: another app may be using port 5000.
echo   Close other apps and try again.
) > "release\app\HOW TO USE.txt"

echo.
echo ============================================================
echo  Done! Release files are in: release\app\
echo  Share the entire release\app\ folder with the end user.
echo ============================================================
echo.
pause
