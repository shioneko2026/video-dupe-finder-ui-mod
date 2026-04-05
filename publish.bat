@echo off
title VDF Publisher
echo ============================================================
echo  Video Dupe Finder -- Build Self-Contained Release
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

rem -- Write Start VDF.bat line by line to avoid block-parsing issues
set L=release\app\Start VDF.bat
echo @echo off > "%L%"
echo title Video Dupe Finder >> "%L%"
echo cd /d "%%~dp0" >> "%L%"
echo echo Starting Video Dupe Finder... >> "%L%"
echo echo A small server window will appear in the background. Do not close it. >> "%L%"
echo echo. >> "%L%"
echo start "VDF Server" /min cmd /k "VDF.Web.exe" >> "%L%"
echo. >> "%L%"
echo set /a _attempts=0 >> "%L%"
echo :wait >> "%L%"
echo set /a _attempts=_attempts+1 >> "%L%"
echo if %%_attempts%% GTR 30 goto timeout_err >> "%L%"
echo echo Waiting for server to start... ^(%%_attempts%%/30^) >> "%L%"
echo timeout /t 2 /nobreak ^>nul >> "%L%"
echo powershell -Command "try { Invoke-WebRequest http://localhost:5000 -TimeoutSec 1 -UseBasicParsing | Out-Null; exit 0 } catch { exit 1 }" >> "%L%"
echo if %%ERRORLEVEL%% NEQ 0 goto wait >> "%L%"
echo echo. >> "%L%"
echo echo Server ready! Opening browser... >> "%L%"
echo start "" http://localhost:5000 >> "%L%"
echo exit >> "%L%"
echo. >> "%L%"
echo :timeout_err >> "%L%"
echo echo. >> "%L%"
echo echo ERROR: Server did not respond after 60 seconds. >> "%L%"
echo echo Check the minimised server window for error messages. >> "%L%"
echo pause >> "%L%"
echo exit /b 1 >> "%L%"

rem -- Write HOW TO USE.txt
set T=release\app\HOW TO USE.txt
echo Video Dupe Finder -- How To Use > "%T%"
echo ================================= >> "%T%"
echo. >> "%T%"
echo 1. Double-click "Start VDF.bat" to launch the app. >> "%T%"
echo    A small server window will appear minimised. Do not close it. >> "%T%"
echo    Your browser will open automatically when the server is ready. >> "%T%"
echo. >> "%T%"
echo 2. The very first time you start the app, it will download FFmpeg >> "%T%"
echo    (the tool it uses to read video files). This is automatic and >> "%T%"
echo    takes 1-2 minutes depending on your internet speed. It only >> "%T%"
echo    happens once -- after that it starts instantly. >> "%T%"
echo. >> "%T%"
echo 3. In your browser, go to the Scan tab. >> "%T%"
echo    Click Browse to pick a folder, then click Start Scan. >> "%T%"
echo. >> "%T%"
echo 4. After scanning, go to the Results tab. >> "%T%"
echo    Click a file column to mark it for deletion. >> "%T%"
echo    Click Delete Flagged to send it to the Recycle Bin. >> "%T%"
echo    Nothing is permanently deleted -- restore from Recycle Bin if needed. >> "%T%"
echo. >> "%T%"
echo 5. To stop the app, close the minimised server window. >> "%T%"
echo. >> "%T%"
echo ---- Troubleshooting ---- >> "%T%"
echo. >> "%T%"
echo "Windows protected your PC" warning: >> "%T%"
echo   Click "More info" then "Run anyway". This is normal for apps >> "%T%"
echo   that aren't published on the Microsoft Store. >> "%T%"
echo. >> "%T%"
echo App won't start / browser never opens: >> "%T%"
echo   Check the server window for a red error message. >> "%T%"
echo   Make sure you extracted the full zip before running -- >> "%T%"
echo   running directly from inside a zip file will fail. >> "%T%"
echo. >> "%T%"
echo FFmpeg download failed: >> "%T%"
echo   Download ffmpeg.exe and ffprobe.exe from https://ffmpeg.org/download.html >> "%T%"
echo   and place both files in the bin\ folder next to VDF.Web.exe. >> "%T%"
echo. >> "%T%"
echo Port already in use: >> "%T%"
echo   Another app may be using port 5000. Close other apps and try again. >> "%T%"

echo.
echo ============================================================
echo  Done! Release files are in: release\app\
echo.
echo  To share: zip the release\app\ folder and send it.
echo  Recipients just extract and double-click "Start VDF.bat".
echo ============================================================
echo.
pause
