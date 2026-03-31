param(
    [Parameter(Mandatory = $true)]
    [string]$VsixPath,
    [Parameter(Mandatory = $true)]
    [string]$SourceManifestPath,
    [Parameter(Mandatory = $true)]
    [string]$OutputManifestPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$resolvedVsixPath = (Resolve-Path -LiteralPath $VsixPath).Path
$resolvedSourceManifestPath = (Resolve-Path -LiteralPath $SourceManifestPath).Path
$resolvedOutputManifestPath = [System.IO.Path]::GetFullPath($OutputManifestPath)

Copy-Item -LiteralPath $resolvedSourceManifestPath -Destination $resolvedOutputManifestPath -Force

$archive = [System.IO.Compression.ZipFile]::Open($resolvedVsixPath, [System.IO.Compression.ZipArchiveMode]::Update)
try {
    $existingEntry = $archive.GetEntry('extension.vsixmanifest')
    if ($null -ne $existingEntry) {
        $existingEntry.Delete()
    }

    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $resolvedSourceManifestPath, 'extension.vsixmanifest') | Out-Null
}
finally {
    $archive.Dispose()
}
