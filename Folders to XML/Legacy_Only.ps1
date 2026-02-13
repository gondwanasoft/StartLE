# Define the folder path (Change this to your target folder)
$folderPath = "C:\Users\Peter\Start\Games\Launchers (Stores)"

# Create the COM object to parse shortcuts
$shell = New-Object -ComObject WScript.Shell

# Get all .lnk files in the folder
$shortcuts = Get-ChildItem -Path $folderPath -Filter *.lnk

if ($shortcuts) {
    foreach ($file in $shortcuts) {
        # Load the shortcut file
        $targetPath = $shell.CreateShortcut($file.FullName).TargetPath

        # Output the results
        [PSCustomObject]@{
            ShortcutName = $file.Name
            Target       = $targetPath
        }
    }
} else {
    Write-Host "No shortcut (.lnk) files found in $folderPath" -ForegroundColor Yellow
}