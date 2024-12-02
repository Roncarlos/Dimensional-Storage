# Param init
param(
    [string]$GamePath = ""
)


if ($GamePath -eq "") {
    # Search steam app folder
    $steamPath = "C:\Program Files (x86)\Steam\steamapps\common\Dyson Sphere Program"
    # Search for dyson sphere program

    if (Test-Path $steamPath) {
        $GamePath = $steamPath
    }
    else {
        Write-Host "Game path not found"
        exit
    }

}   

$GamePath = $GamePath + "\DSPGAME_Data\Managed"

# Copy libraries except "System.*" and "netstandard.dll" and mscorlib.dll
# To ./Libraries

$LibrariesPath = ".\Libraries"

if (-not (Test-Path $LibrariesPath)) {
    New-Item -ItemType Directory -Path $LibrariesPath
}

$files = Get-ChildItem $GamePath

foreach ($file in $files) {
    if ($file.Name -match "System.*" -or $file.Name -eq "netstandard.dll" -or $file.Name -eq "mscorlib.dll") {
        continue
    }

    Write-Host "Copying $file"
    Copy-Item $file.FullName $LibrariesPath
}

Write-Host "Done"




