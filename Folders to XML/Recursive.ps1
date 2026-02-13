# Define the root folder to start the search
$rootPath = "C:\Users\Peter\Start"
#$rootPath = "D:\Users\Peter\Documents\Development\StartLE\Folders to XML\StartLE"
#$rootPath = "C:\ProgramData\Microsoft\Windows\Start Menu\Programs"
#$rootPath = "C:\Users\Peter\AppData\Roaming\Microsoft\Windows\Start Menu\Programs"

function Get-LinkInfo {
    param (
      [Parameter(Mandatory, ValueFromPipeline)] [System.__ComObject]$FolderItem
    )
    [PSCustomObject]@{ 
        Path = $Path
        Name = $item.Name
        TargetParsingPath = $item.ExtendedProperty("System.Link.TargetParsingPath") 
        TargetPath = $item.ExtendedProperty("System.Link.TargetPath") 
        Arguments = $item.ExtendedProperty("System.Link.Arguments") 
        TargetIDList = $item.ExtendedProperty("System.Link.TargetIDList")
        WorkingDirectory = $item.ExtendedProperty("System.Link.WorkingDirectory") 
        ShowCmd = $item.ExtendedProperty("System.Link.ShowCmd") 
        HotKey = $item.ExtendedProperty("System.Link.HotKey") 
        Description = $item.ExtendedProperty("System.Link.Comment") 
        IconLocation = $item.ExtendedProperty("System.Link.IconLocation") 
        RelativePath = $item.ExtendedProperty("System.Link.RelativePath") 
    }
}

function Get-ShortcutTargets {
    param (
        [string]$folderPath
    )

    $shellApp = New-Object -ComObject Shell.Application
    $folder = $shellApp.NameSpace($folderPath)

    if ($null -eq $folder) { return }

    $folderName = Split-Path -Path $folderPath -Leaf
    Write-Host "<menu text=`"$folderName`">"

    # Process each item in the current folder
    foreach ($item in $folder.Items()) {

        # If it's a folder, go deeper (Recursive call)
        if ($item.IsFolder) {
            Get-ShortcutTargets -folderPath $item.Path
        }
        # If it's a shortcut (.lnk), extract the target
        elseif ($item.IsLink) {
            $link = $item.GetLink
            $target = $link.Path

            # Fallback for Windows Store Apps / Special items
            if ([string]::IsNullOrWhiteSpace($target)) {
                $target = $item.ExtendedProperty("System.Link.TargetParsingPath")
                #Write-Host "TargetPath=" $item.ExtendedProperty("System.Link.TargetPath")
                #Write-Host "Args=" $item.ExtendedProperty("System.Link.Arguments")

                # These seem to work:
                #Write-Host "TargetParsingPath=" $item.ExtendedProperty("System.Link.TargetParsingPath")
                #Write-Host "Args=" $item.ExtendedProperty("System.Link.Arguments")
                #Write-Host "Folder=" $item.ExtendedProperty("System.Link.WorkingFolderPath")
                #Write-Host "ShowCmd=" $item.ExtendedProperty("System.Link.ShowCmd")

                $category = switch -regex ($target) {
                    "^::" { "ShellNamespace" }                          # Virtual items like 'This PC'
                    "^[a-zA-Z]:\\" { "FileSystemPath" }                 # Standard Local Path
                    "^\\\\" { "NetworkPath" }                            # UNC/Network Path
                    "!" { "AUMID" }                                      # Likely a Store App
                    default { "Unknown" }
                }
                switch($category) {
                    "ShellNamespace" {}
                    "FileSystemPath" {}
                    "NetworkPath" {}
                    "AUMID" {$target = "shell:AppsFolder\" + $target}
                    "Unknown" {
                        $info = Get-LinkInfo($item)
                        Write-Warning "Unknown target category: $($item.Name) = $target"
                        #Write-Warning "$info"
                        #Write-Warning "------------------------"
                    }
                }
            }

            [PSCustomObject]@{
                ParentFolder = $folderPath
                ShortcutName = $item.Name
                Target       = $target
            }
        }
        else {
            Write-Warning "Unknown item type: $($item.Name)"
        }
    }
}

# Run the function and display results
$results = Get-ShortcutTargets -folderPath $rootPath
#$results | Out-GridView