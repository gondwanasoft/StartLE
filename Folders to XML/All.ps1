$folderPath = "C:\Users\Peter\Start\Games\Launchers (Stores)"

# We use the Shell.Application object for deep link inspection
$shellApp = New-Object -ComObject Shell.Application
$folder = $shellApp.NameSpace($folderPath)

if ($null -eq $folder) { Write-Error "Path not found"; return }

$results = foreach ($item in $folder.Items()) {
    if ($item.IsLink) {
        $link = $item.GetLink

        # 1. Try standard path
        $target = $link.Path

        # 2. If it's a Store App, standard path is empty; we check the 'Target' property
        if ([string]::IsNullOrWhiteSpace($target)) {
            # This fetches the AUMID (e.g., Microsoft.WindowsCalculator_...)
            $target = $item.ExtendedProperty("System.Link.TargetParsingPath")
        }

        [PSCustomObject]@{
            ShortcutName = $item.Name
            Target       = $target
        }
    }
}

$results | Out-GridView  # Opens in a searchable window, or use Format-Table