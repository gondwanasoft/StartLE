function Get-LinkInfo {
    param([string]$Path)

    $shell = New-Object -ComObject Shell.Application
    $folder = $shell.Namespace((Split-Path $Path))
    $item   = $folder.ParseName((Split-Path $Path -Leaf))

    [PSCustomObject]@{
        Path               = $Path
        TargetParsingPath  = $item.ExtendedProperty("System.Link.TargetParsingPath")
        Arguments          = $item.ExtendedProperty("System.Link.Arguments")
        IsShellGuidTarget  = ($item.ExtendedProperty("System.Link.TargetParsingPath") -match '^::\{[0-9A-Fa-f-]+\}$')
        IsMissingTarget    = [string]::IsNullOrWhiteSpace($item.ExtendedProperty("System.Link.TargetParsingPath"))
        IsSpecialShellLink = (
            [string]::IsNullOrWhiteSpace($item.ExtendedProperty("System.Link.TargetParsingPath")) -or
            ($item.ExtendedProperty("System.Link.TargetParsingPath") -match '^::\{[0-9A-Fa-f-]+\}$') -or
            ($item.ExtendedProperty("System.Link.Arguments") -match '::\{[0-9A-Fa-f-]+\}')
        )
    }
}
