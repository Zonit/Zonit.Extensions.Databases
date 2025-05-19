param(
    [string]$RootDirectory = "."
)

function Get-LatestMajorNuGetVersion {
    param (
        [string]$PackageId,
        [string]$MajorVersion
    )
    $LowerBound = "$MajorVersion."
    $url = "https://api.nuget.org/v3-flatcontainer/$($PackageId.ToLower())/index.json"
    try {
        $response = Invoke-RestMethod -UseBasicParsing -Uri $url -ErrorAction Stop
        if ($response.versions) {
            $matching = $response.versions | Where-Object { $_ -like "$LowerBound*" }
            if ($matching) {
                return $matching[-1]
            }
        }
    } catch {
        Write-Warning "  Problem downloading info for $PackageId ($url)"
    }
    return $null
}

function Update-Packages-InFile {
    param(
        [string]$FilePath
    )

    Write-Host "`nChecking $FilePath" -ForegroundColor Cyan
    # Wczytaj XML
    try {
        [xml]$xml = Get-Content $FilePath
    } catch {
        Write-Warning "$FilePath is not a valid XML file, skipping."
        return
    }

    $modified = $false

    # Jeœli Directory.Packages.props (zak³adamy: tylko ItemGroup)
    if ($FilePath -like "*Directory.Packages.props") {
        foreach ($itemGroup in $xml.Project.ItemGroup) {
            foreach ($pkg in $itemGroup.PackageVersion) {
                $pkgId   = $pkg.Include
                $currVer = $pkg.Version
                # Wyci¹gnij MAJOR wersji tej paczki
                if ($currVer -match '^(\d+)\.') {
                    $major = $matches[1]
                } else {
                    $major = 8 # fallback
                }
                $latest = Get-LatestMajorNuGetVersion -PackageId $pkgId -MajorVersion $major
                if ($latest -and $latest -ne $currVer) {
                    Write-Host ("  Updating {0} ({1} -> {2})" -f $pkgId,$currVer,$latest) -ForegroundColor Yellow
                    $pkg.SetAttribute("Version", "$latest")
                    $modified = $true
                } else {
                    Write-Host ("  {0} already up to date ({1})" -f $pkgId,$currVer) -ForegroundColor Gray
                }
            }
        }
    }
    elseif ($FilePath -like "*.csproj") {
        # Wyszukujemy ItemGroup z warunkiem TargetFramework
        $itemGroups = $xml.Project.ItemGroup | Where-Object { $_.Condition -match "TargetFramework" }
        foreach ($group in $itemGroups) {
            # Osobny warunek frameworka
            if ($group.Condition -match "'\$\((TargetFramework)\)'\s*==\s*'(?<tf>[a-zA-Z0-9\.]+)'") {
                $tf = $matches['tf']
                if ($tf -match "^net(?<major>\d+)\.0$") {
                    $major = $matches['major']
                } else {
                    continue
                }
                $packageItems = $group.PackageVersion
                foreach ($pkg in $packageItems) {
                    $pkgId   = $pkg.Include
                    $currVer = $pkg.Version
                    if (-not $pkgId -or -not $currVer) { continue }
                    $latest = Get-LatestMajorNuGetVersion -PackageId $pkgId -MajorVersion $major
                    if ($latest -and $latest -ne $currVer) {
                        Write-Host ("  Updating {0} ({1} -> {2})" -f $pkgId,$currVer,$latest) -ForegroundColor Yellow
                        $pkg.SetAttribute("Version", "$latest")
                        $modified = $true
                    } else {
                        Write-Host ("  {0} already up to date ({1})" -f $pkgId,$currVer) -ForegroundColor Gray
                    }
                }
            }
        }
    }
    else {
        # Inne typy plików pomijamy
        Write-Host "  Skipped - not relevant type" -ForegroundColor DarkGray
        return
    }

    if ($modified) {
        $xml.Save($FilePath)
        Write-Host "  Saved $FilePath" -ForegroundColor Green
    } else {
        Write-Host "  No changes in $FilePath"
    }
}

# Przeszukaj wszystkie Directory.Packages.props i .csproj
$files = Get-ChildItem -Path $RootDirectory -Recurse -Include "Directory.Packages.props","*.csproj" -File
foreach ($file in $files) {
    Update-Packages-InFile -FilePath $file.FullName
}

Write-Host "`nCompleted full scan!" -ForegroundColor Magenta