param(
    [string]$ProcessName = "MutinyIRC.UI",
    [string]$WindowTitle,
    [string]$OutFile = "$PSScriptRoot\screenshot.png"
)

Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public class WinAPI {
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd, out RECT r);
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);
    [DllImport("user32.dll")] public static extern bool IsIconic(IntPtr hwnd);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hwnd, int cmd);
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left, Top, Right, Bottom; }
}
"@

# Match on the process, not the window title. Several unrelated windows carry
# "MutinyIRC" in their title - Rider, and any terminal showing the repo path - so a
# title search picks the wrong one. -WindowTitle stays available to disambiguate
# when the process owns more than one window.
$candidates = @(Get-Process -Name $ProcessName -ErrorAction SilentlyContinue |
                Where-Object { $_.MainWindowHandle -ne [IntPtr]::Zero })

if ($WindowTitle) {
    $candidates = @($candidates | Where-Object { $_.MainWindowTitle -like "*$WindowTitle*" })
}

if ($candidates.Count -eq 0) {
    $hint = if ($WindowTitle) { " with a title matching '$WindowTitle'" } else { "" }
    Write-Error "No window found for process '$ProcessName'$hint. Is the app running?"
    exit 1
}

$proc = $candidates[0]
$hwnd = $proc.MainWindowHandle

# PrintWindow reads a minimized window as blank, so restore one. Leave every other
# window state alone: restoring unconditionally would silently un-maximize the app
# and photograph the wrong layout.
if ([WinAPI]::IsIconic($hwnd)) {
    [WinAPI]::ShowWindow($hwnd, 9) | Out-Null   # SW_RESTORE
    Start-Sleep -Milliseconds 600
}

$rect = New-Object WinAPI+RECT
[WinAPI]::GetWindowRect($hwnd, [ref]$rect) | Out-Null

$width  = $rect.Right  - $rect.Left
$height = $rect.Bottom - $rect.Top

if ($width -le 0 -or $height -le 0) {
    Write-Error "Window found but has zero size. Restore the window first."
    exit 1
}

$bmp = New-Object System.Drawing.Bitmap($width, $height)
$g   = [System.Drawing.Graphics]::FromImage($bmp)
$hdc = $g.GetHdc()
# PW_RENDERFULLCONTENT (2) is required for Avalonia's GPU-composited windows.
# PrintWindow copies the window's own buffer, so an occluded or background window
# still captures correctly and no focus gets stolen from whatever you're doing.
$printed = [WinAPI]::PrintWindow($hwnd, $hdc, 2)
$g.ReleaseHdc($hdc)
$g.Dispose()

if (-not $printed) {
    $bmp.Dispose()
    Write-Error "PrintWindow failed for '$ProcessName' (handle $hwnd)."
    exit 1
}

# A driver that refuses PrintWindow hands back a single flat colour rather than an
# error. Sample a coarse grid and say so, so the blank image isn't mistaken for the
# real UI.
$colors = New-Object System.Collections.Generic.HashSet[int]
for ($x = 4; $x -lt $width;  $x += [Math]::Max(1, [int]($width  / 16))) {
    for ($y = 4; $y -lt $height; $y += [Math]::Max(1, [int]($height / 16))) {
        [void]$colors.Add($bmp.GetPixel($x, $y).ToArgb())
    }
}
if ($colors.Count -le 1) {
    Write-Warning "Capture is a single flat colour - the window likely did not render. Treat $OutFile as unreliable."
}

$bmp.Save($OutFile, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()

# Name what was captured. The old failure mode was saving a different application's
# window and reporting success.
Write-Host "Saved: $OutFile"
Write-Host "  from: $($proc.ProcessName) (PID $($proc.Id)) `"$($proc.MainWindowTitle)`"  ${width}x${height}"
