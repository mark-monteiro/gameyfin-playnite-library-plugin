<#
.SYNOPSIS
    Build the addon in release mode, then package the addon.
#>

[CmdletBinding()]
param (
  # Path to the Playnite Toolbox utility. It can be found in the same directory that Playnite is installed in.
  [ValidateNotNullOrEmpty()]
  [ValidatePattern(".*\.exe")]
  [ValidateScript({ Test-Path $_ -PathType leaf }, ErrorMessage = "'{0}' is not a valid file path")]
  [string]$ToolboxPath = (Join-Path $env:LOCALAPPDATA "Playnite" "Toolbox.exe"),

  [ValidateNotNullOrEmpty()]
  [ValidatePattern(".*\.exe")]
  [ValidateScript({ Test-Path $_ -PathType leaf }, ErrorMessage = "'{0}' is not a valid file path")]
  [string]$MsBuildPath = 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe',

  # The output directory for the packaged extension. Defaults to ./dist
  [ValidateNotNullOrEmpty()]
  [string]$OutputDir = (Join-Path $PWD "dist")
)

# Stop executing on errors
$ErrorActionPreference = "Stop"

# Determine some paths
$projectFolder = (Join-Path $PWD "GameyfinLibrary")
$buildOutputFolder = (Join-Path $projectFolder "bin" "Release")

# Build project in Release mode
& $MsBuildPath -p:Configuration=Release $projectFolder

if ($LASTEXITCODE -ne 0)
{
  throw "Project build failed"
}

# Pack the extension
& $ToolboxPath pack $buildOutputFolder $OutputDir

if ($LASTEXITCODE -ne 0)
{
  throw "Project packaging failed"
}
