# Define the root folder to start the search
$rootPath = "C:\Users\Peter\Start"
#$rootPath = "D:\Users\Peter\Documents\Development\StartLE\Folders to XML\StartLE"
$rootPath = "D:\Users\Peter\Documents\Development\StartLE\Folders to XML\Test"
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
        [string]$folderPath,
        [string]$tabs = ""
    )

    $shellApp = New-Object -ComObject Shell.Application
    $folder = $shellApp.NameSpace($folderPath)

    if ($null -eq $folder) { return }
      
    if ($tabs -eq "") {  
        Write-Output "<menu>" 
    } else {
        $folderName = Split-Path -Path $folderPath -Leaf
        Write-Output "$tabs<menu text=`"$folderName`">" 
    }

    # Process each item in the current folder
    foreach ($item in $folder.Items()) {

        if ($item.IsFolder) {        # If it's a folder, go deeper (Recursive call)
            Get-ShortcutTargets -folderPath $item.Path -tabs $tabs"  "
        } elseif ($item.IsLink) {   # If it's a shortcut (.lnk), extract the target
            $link = $item.GetLink
            $target = $link.Path

            $attribs = $item.ExtendedProperty("System.FileAttributes")
            if ($attribs.Contains("R")) {
                Write-Warning "Read-only item, some properties inaccessible: $($item.Name)"
            }

            $unknownString = ""

            # These seem to work - or do they need $link?
            #Write-Output "TargetParsingPath=" $item.ExtendedProperty("System.Link.TargetParsingPath")
            #Write-Output "Args=" $item.ExtendedProperty("System.Link.Arguments")
            #Write-Output "Folder=" $item.ExtendedProperty("System.Link.WorkingFolderPath")
            #Write-Output "ShowCmd=" $item.ExtendedProperty("System.Link.ShowCmd")

            # Fallback for Windows Store Apps / Special items
            if ([string]::IsNullOrWhiteSpace($target)) {
                $target = $item.ExtendedProperty("System.Link.TargetParsingPath")
                #Write-Output "TargetPath=" $item.ExtendedProperty("System.Link.TargetPath")
                #$args = $item.ExtendedProperty("System.Link.Arguments")

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
                        $unknownString = "? "
                    }
                }
            }

            #[PSCustomObject]@{
            #    ParentFolder = $folderPath
            #    ShortcutName = $item.Name
            #    Target       = $target
            #}
            
            #Write-Output "$tabs  <item text=`"$unknownString$($item.Name)`" filename=`"$target`"" -NoNewline
            $xml = "$tabs  <item text=`"$unknownString$($item.Name)`" filename=`"$target`""

            $folder = $link.WorkingDirectory
            if ($folder -ne "") {
                #Write-Output " folder=`"$folder`"" -NoNewline
                $xml = $xml + " folder=`"$folder`""
            }

            $args = $link.Arguments
            if ($args -ne "") {
                $args = [System.Security.SecurityElement]::Escape($args)
                #Write-Output " args=`"$args`"" -NoNewline
                $xml = $xml + " args=`"$args`""
            }

            $style = $link.ShowCommand
            if ($style -eq 3) {
                #Write-Output " style=`"maximized`"" -NoNewline
                $xml = $xml + " style=`"maximized`""
            } elseif ($style -eq 7) {
                #Write-Output " style=`"minimized`"" -NoNewline
                $xml = $xml + " style=`"minimized`""
            }

            # TODO check verb=runas

            Write-Output "$xml/>"

        }
        else {
            Write-Warning "Unknown item type: $($item.Name)"
        }
    }
    Write-Output "$tabs</menu>"
}

# Run the function and display results
$xml = Get-ShortcutTargets -folderPath $rootPath
Write-Output $xml
#$results | Out-GridView
#TODO can't extract folder and args from read-only .lnk (unless run as admin??)