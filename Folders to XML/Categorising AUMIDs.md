Because `System.Link.TargetParsingPath` is a bit of a "catch-all" property, it can return several different types of strings depending on what the shortcut points to.

To distinguish an **AUMID** from a standard file path or a GUID-based virtual folder, you can use a combination of pattern matching and property checks.

### The Three Common Return Types

| Return Type | Example Format | Identifying Characteristics |
| --- | --- | --- |
| **AUMID** | `Microsoft.WindowsCalculator_8wekyb3d8bbwe!App` | Contains an **exclamation mark** (`!`) and usually an underscore. |
| **Standard Path** | `C:\Windows\System32\calc.exe` | Contains a **drive letter** (e.g., `C:\`) or starts with `\\`. |
| **CLSID / Shell ID** | `::{20D04FE0-3AEA-1069-A2D8-08002B30309D}` | Starts with **double colons** (`::`) and contains curly braces. |

---

### How to Validate via PowerShell

You can use a `switch` statement with **Regex** to categorize the target. This is the most robust way to decide if you should prefix it with `shell:AppsFolder\` or handle it as a file.

```powershell
$target = $item.ExtendedProperty("System.Link.TargetParsingPath")

$category = switch -regex ($target) {
    "^::" { "ShellNamespace" }                          # Virtual items like 'This PC'
    "^[a-zA-Z]:\\" { "FileSystemPath" }                 # Standard Local Path
    "^\\\\" { "NetworkPath" }                            # UNC/Network Path
    "!" { "AUMID" }                                      # Likely a Store App
    default { "Unknown" }
}

if ($category -eq "AUMID") {
    Write-Host "This is a Store App: $target" -ForegroundColor Cyan
}

```

### Deep Verification: Checking the Package Manager

If you want to be 100% certain that the string is a valid Store App AUMID and not just a weirdly named file, you can query the **Appx Package Manager**.

Note that the AUMID consists of the **Package Family Name** (before the `!`) and the **App ID** (after the `!`).

```powershell
# Split the AUMID at the '!'
$parts = $target -split '!'
$pfName = $parts[0]

# Check if a package with this Family Name actually exists on the system
if (Get-AppxPackage -PackageFamilyName $pfName -ErrorAction SilentlyContinue) {
    Write-Host "$target is a verified Windows Store App ID."
}

```

### Why "TargetParsingPath" is safer than "Path"

For Store apps, the standard `.Path` property often returns a "shorthand" GUID or nothing at all. `TargetParsingPath` is specifically designed for the Windows Shell to "reparse" the location.

* If it has a `!`, it's an **App**.
* If it has `::`, it's a **System Folder** (like Control Panel).
* If it has `\`, it's a **File/Folder**.

Would you like me to integrate this classification logic into the main recursive script so it labels the "Type" of each shortcut for you?