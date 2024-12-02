param(
    [string]$version = ""
)



# if string is empty then set at null
if ($version -eq "") {
    $version = $null
}


Write-Host ($null -eq $version)

# if version is null get last version from manifest.json
if ("" -eq $version) {
    Write-Host "########################################"
    Write-Host "Incrementing version number"
    Write-Host "########################################"

    $manifestPath = "./modpackage/manifest.json"

    $manifestContent = Get-Content -Path $manifestPath -Encoding UTF8

    $version = $manifestContent | ConvertFrom-Json | Select-Object -ExpandProperty version_number

    Write-Host "Version from manifest.json: $version"

    # Increment the version number

    $versionParts = $version -split "\."

    $lastPart = $versionParts[-1]

    $lastPart = [int]$lastPart + 1

    $versionParts[-1] = $lastPart

    $version = $versionParts -join "."
    Write-Host "Incremented version: $version"
}

# Check if changelog contains the version

$changelogPath = "./modpackage/CHANGELOG.md"

$changelogContent = Get-Content -Path $changelogPath -Encoding UTF8

if (-not $changelogContent -match "$version") {
    Write-Host "Changelog does not contain the version $version" -ForegroundColor Red
    exit
}


if ("" -ne $version) {
    # Change version in ./Dimensional Storage/Dimensional Storage.csproj

    $csprojPath = "./Dimensional Storage/Dimensional Storage.csproj"

    $csprojContent = Get-Content -Path $csprojPath -Encoding UTF8

    $csprojContent = $csprojContent -replace "<Version>.*</Version>", "<Version>$version</Version>"

    Set-Content -Path $csprojPath -Value $csprojContent -Encoding UTF8

    Write-Host "Version changed to $version in $csprojPath"

    # Change version in ./modpackage/manifest.json

    $manifestPath = "./modpackage/manifest.json"

    $manifestContent = Get-Content -Path $manifestPath -Encoding UTF8

    $manifestContent = $manifestContent -replace "`"version_number`": `".*`"", "`"version_number`": `"$version`""

    Set-Content -Path $manifestPath -Value $manifestContent -Encoding UTF8

    Write-Host "Version changed to $version in $manifestPath"

    # Change version in ./Dimensional Storage/Dimensional Storage.cs
    # [BepInPlugin("com.jiceedev.DimensionalStorage", "Dimensional Storage", "1.0.0")]

    $csPath = "./Dimensional Storage\DimensionalStorageMod.cs"

    $csContent = Get-Content -Path $csPath -Encoding UTF8

    $csContent = $csContent -replace "\[BepInPlugin\(`"com.jiceedev.DimensionalStorage`", `"Dimensional Storage`", `".*`"\]", "[BepInPlugin(`"com.jiceedev.DimensionalStorage`", `"Dimensional Storage`", `"$version`")]"

    Set-Content -Path $csPath -Value $csContent -Encoding UTF8

    Write-Host "Version changed to $version in $csPath"


}






Write-Host "########################################"
Write-Host "Package Folder preparation"
Write-Host "########################################"



# Create ./deploy folder is not exists
if (-not (Test-Path -Path "./deploy")) {
    Write-Host "Creating ./deploy folder"
    New-Item -ItemType Directory -Path "./deploy"
}
else {
    # Clear the ./deploy folder
    Write-Host "Clearing ./deploy folder"
    Remove-Item -Path "./deploy" -Recurse -Force
    New-Item -ItemType Directory -Path "./deploy"
}

# Copy all files from ./modpackage to ./deploy recursively

Write-Host "Copying all files from ./modpackage to ./deploy"
Copy-Item -Path "./modpackage/*" -Destination "./deploy" -Recurse

# dotnet build
Write-Host "########################################"
Write-Host "Building the project"
Write-Host "########################################"

dotnet build -c Release

$filesToTake = @(
    "./Dimensional Storage/bin/Release/net48/Com.JiceeDev.DimensionalStorage.dll",
    "./Libraries/FullSerializer.dll"
)

Write-Host "########################################"
Write-Host "Copying dll files to ./deploy"

foreach ($file in $filesToTake) {
    Write-Host "Copying $file to ./deploy"
    Copy-Item -Path $file -Destination "./deploy" -Force
}


# Zip the deploy folder's content to a zip
Write-Host "Zipping the deploy folder"

$zipFileName = "DimensionalStorage.zip"
$zipTempPath = "./$zipFileName"
$zipFilePath = "./deploy/$zipFileName"

# if (Test-Path -Path $zipFilePath) {
#     Write-Host "Removing existing zip file"
#     Remove-Item -Path $zipFilePath -Force
# }

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory("./deploy", $zipTempPath)

# Clean up the deploy folder

Write-Host "Cleaning up the deploy folder"

Remove-Item -Path "./deploy/*" -Recurse -Force

Write-Host "Moving the zip file to ./deploy"

Move-Item -Path $zipTempPath -Destination $zipFilePath -Force


Write-Host "Done."




