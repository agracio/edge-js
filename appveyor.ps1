
#-------------------------------
# Installation
#-------------------------------

Write-Host "Downloading latest .NET Core SDK..."

(New-Object System.Net.WebClient).DownloadFile('https://download.visualstudio.microsoft.com/download/pr/9e753d68-7701-4ddf-b358-79d64e776945/2a58564c6d0779a7b443a692c520782f/dotnet-sdk-8.0.203-win-x64.exe','dotnet-core-sdk.exe')

Write-Host "Installing .NET Core SDK..."

Invoke-Command -ScriptBlock { ./dotnet-core-sdk.exe /S /v/qn }

Write-Host "Installation succeeded." -ForegroundColor Green
