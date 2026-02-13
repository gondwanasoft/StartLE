In PowerShell, determining if a shortcut (LNK file) is set to "Run as Administrator" requires digging into the **ShellLinkDataList** flags. This isn't visible via standard file properties, so we have to use a COM object to parse the shortcut's metadata.

The specific flag we are looking for is `0x2000` (or `RunAsUser`) within the `SLDF_FLAGS`.

### The PowerShell Script

You can use the following snippet to check a specific `.lnk` file. Just replace the path in the first line:

```powershell
$shortcutPath = "C:\Path\To\Your\Shortcut.lnk"

# Create a Shell COM object
$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)

# Use a Shell.Application object to get the specific Link object
$shellApp = New-Object -ComObject Shell.Application
$directory = Split-Path $shortcutPath
$filename = Split-Path $shortcutPath -Leaf
$folder = $shellApp.NameSpace($directory)
$item = $folder.ParseName($filename)
$link = $item.GetLink

# Check the Flags (0x2000 is the 'Run as Admin' bit)
$isAdmin = ($link.Flags -band 0x2000) -eq 0x2000

# Output the result
[PSCustomObject]@{
    Name    = $filename
    IsAdmin = $isAdmin
    Target  = $shortcut.TargetPath
}

```

---

### How it Works

1. **COM Objects:** PowerShell uses the `WScript.Shell` and `Shell.Application` interfaces to "look inside" the shortcut file.
2. **Bitwise Comparison:** The `.Flags` property contains a series of binary toggles.
3. **The Magic Number:** The value `0x2000` represents the `SLDF_RUNAS_USER` flag. Using the `-band` (bitwise AND) operator allows us to see if that specific bit is flipped "on."

### Bulk Checking a Directory

If you want to audit an entire folder (like your Desktop or Start Menu) for admin-level shortcuts, you can wrap the logic in a loop:

```powershell
$path = "$env:USERPROFILE\Desktop"
Get-ChildItem -Path $path -Filter *.lnk | ForEach-Object {
    $shellApp = New-Object -ComObject Shell.Application
    $folder = $shellApp.NameSpace($_.DirectoryName)
    $item = $folder.ParseName($_.Name)
    $link = $item.GetLink

    if ($link.Flags -band 0x2000) {
        Write-Host "[!] Admin Shortcut: $($_.Name)" -ForegroundColor Red
    } else {
        Write-Host "[✓] Standard Shortcut: $($_.Name)" -ForegroundColor Green
    }
}

```

**Note:** This only detects if the *shortcut* is forced to run as admin. It won't tell you if the target executable itself requires admin privileges via its own internal manifest.

Would you like me to help you turn this into a reusable function you can add to your PowerShell profile?