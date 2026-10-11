<#
.SYNOPSIS
  Generates Files.wxs (WiX v5 fragment) from a published app folder.

.DESCRIPTION
  Scans every file under the publish output (e.g. publish\daypilot) and emits
  a WiX fragment with one Component per file (deterministic GUIDs derived from
  the relative path, so upgrades stay stable) plus a ComponentGroup "AppFiles"
  that the authored <app>.wxs references. Run on the Windows build runner;
  never edit the output by hand.

.PARAMETER AppDir
  Publish folder to harvest, e.g. "publish\daypilot".

.PARAMETER OutWxs
  Where to write the fragment, e.g. "daypilot\Installer\Files.wxs".
#>
param(
  [Parameter(Mandatory = $true)][string]$AppDir,
  [Parameter(Mandatory = $true)][string]$OutWxs
)

$ErrorActionPreference = "Stop"
$pubRoot = (Resolve-Path $AppDir).Path
$files = Get-ChildItem $pubRoot -Recurse -File | Sort-Object FullName
if ($files.Count -eq 0) { throw "No files found under $AppDir" }

$md5 = [System.Security.Cryptography.MD5]::Create()
function DGuid([string]$s) {
  $b = $md5.ComputeHash([Text.Encoding]::UTF8.GetBytes($s.ToLowerInvariant()))
  return ([Guid]::new($b)).ToString().ToUpper()
}
function DId([string]$prefix, [string]$s) {
  $h = [BitConverter]::ToString($md5.ComputeHash([Text.Encoding]::UTF8.GetBytes($s.ToLowerInvariant()))).Replace("-", "")
  return ($prefix + $h.Substring(0, 12)).ToLower()
}
function XEsc([string]$s) { return [System.Security.SecurityElement]::Escape($s) }

# Build a directory tree: each node = @{ Name=; Subdirs=@{}; Files=@() }
$root = @{ Name = ""; Subdirs = @{}; Files = @() }
foreach ($f in $files) {
  $rel = $f.FullName.Substring($pubRoot.Length + 1)
  $parts = $rel -split "[\\/]"
  $node = $root
  for ($i = 0; $i -lt $parts.Count - 1; $i++) {
    $seg = $parts[$i]
    if (-not $node.Subdirs.ContainsKey($seg)) {
      $node.Subdirs[$seg] = @{ Name = $seg; Subdirs = @{}; Files = @() }
    }
    $node = $node.Subdirs[$seg]
  }
  # Source path relative to the repo root (wix build runs from there)
  $node.Files += @{ Name = $parts[-1]; Rel = $rel }
}

$componentIds = @()
$xml = New-Object System.Collections.Generic.List[string]
$xml.Add('<?xml version="1.0" encoding="UTF-8"?>')
$xml.Add('<!-- Generated at build time by tools/Build-MsiFiles.ps1 — do not edit. -->')
$xml.Add('<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs">')
$xml.Add('  <Fragment>')
$xml.Add('    <DirectoryRef Id="INSTALLFOLDER">')

# The publish folder itself is INSTALLFOLDER; source paths are
# relative to the repo root (wix build runs from there).
function Emit-Node($node, [string]$relDir, [int]$depth, [string]$appLeaf) {
  $pad = "      " + ("  " * $depth)
  foreach ($file in ($node.Files | Sort-Object { $_.Name })) {
    $key = "$relDir/$($file.Name)"
    $cmpId = DId "cmp_" $key
    $filId = DId "fil_" $key
    $guid = DGuid ("component:" + $key)
    $script:componentIds += $cmpId
    $srcRel = "publish\$appLeaf\" + (($key -replace "/", "\"))
    $xml.Add("$pad<Component Id=`"$cmpId`" Guid=`"$guid`" Bitness=`"always64`">")
    $xml.Add("$pad  <File Id=`"$filId`" Source=`"$(XEsc $srcRel)`" KeyPath=`"yes`" />")
    $xml.Add("$pad</Component>")
  }
  foreach ($seg in ($node.Subdirs.Keys | Sort-Object)) {
    $child = $node.Subdirs[$seg]
    $childRel = if ($relDir -eq "") { $seg } else { "$relDir/$seg" }
    $dirId = DId "d_" $childRel
    $xml.Add("$pad<Directory Id=`"$dirId`" Name=`"$(XEsc $seg)`">")
    Emit-Node $child $childRel ($depth + 1) $appLeaf
    $xml.Add("$pad</Directory>")
  }
}

$appLeaf = Split-Path $pubRoot -Leaf
Emit-Node $root "" 0 $appLeaf

$xml.Add('    </DirectoryRef>')
$xml.Add('    <ComponentGroup Id="AppFiles">')
foreach ($cid in ($componentIds | Sort-Object -Unique)) {
  $xml.Add("      <ComponentRef Id=`"$cid`" />")
}
$xml.Add('    </ComponentGroup>')
$xml.Add('  </Fragment>')
$xml.Add('</Wix>')

$outDir = Split-Path $OutWxs -Parent
if ($outDir -ne "" -and -not (Test-Path $outDir)) { New-Item -ItemType Directory $outDir | Out-Null }
[IO.File]::WriteAllLines($OutWxs, $xml, [Text.Encoding]::UTF8)
Write-Host "Files.wxs written: $($files.Count) files, $($componentIds.Count) components -> $OutWxs"
