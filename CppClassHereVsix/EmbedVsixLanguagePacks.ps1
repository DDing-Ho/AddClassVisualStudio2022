param(
    [Parameter(Mandatory = $true)]
    [string]$VsixPath,
    [Parameter(Mandatory = $true)]
    [string]$DefaultLangPackPath,
    [Parameter(Mandatory = $true)]
    [string]$KoreanLangPackPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.IO.Compression.FileSystem

$resolvedVsixPath = (Resolve-Path -LiteralPath $VsixPath).Path
$resolvedDefaultLangPackPath = (Resolve-Path -LiteralPath $DefaultLangPackPath).Path
$resolvedKoreanLangPackPath = (Resolve-Path -LiteralPath $KoreanLangPackPath).Path

$archive = [System.IO.Compression.ZipFile]::Open($resolvedVsixPath, [System.IO.Compression.ZipArchiveMode]::Update)
try {
    foreach ($entryName in @('extension.vsixlangpack', 'ko-KR/extension.vsixlangpack')) {
        $existingEntry = $archive.GetEntry($entryName)
        if ($null -ne $existingEntry) {
            $existingEntry.Delete()
        }
    }

    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $resolvedDefaultLangPackPath, 'extension.vsixlangpack') | Out-Null
    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $resolvedKoreanLangPackPath, 'ko-KR/extension.vsixlangpack') | Out-Null
}
finally {
    $archive.Dispose()
}
