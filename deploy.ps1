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




