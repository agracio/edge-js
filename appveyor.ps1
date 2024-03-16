
#-------------------------------
# Installation
#-------------------------------

function Get-TimeStamp {
    return "[{0:yyyy/MM/dd} {0:HH:mm:ss}]" -f (Get-Date)
}

Write-Host "$(Get-TimeStamp) Downloading latest .NET Core SDK..."

(New-Object System.Net.WebClient).DownloadFile('https://download.visualstudio.microsoft.com/download/pr/9e753d68-7701-4ddf-b358-79d64e776945/2a58564c6d0779a7b443a692c520782f/dotnet-sdk-8.0.203-win-x64.exe','C:\projects\edge-js\dotnet-core-sdk.exe')

Write-Host "$(Get-TimeStamp) Installing .NET Core SDK..."

Start-Process -Wait 'C:\projects\edge-js\dotnet-core-sdk.exe' -ArgumentList '/install /quiet /norestart';

# Invoke-Command -ScriptBlock { dotnet-core-sdk.exe /install /quiet /norestart }

Write-Host "$(Get-TimeStamp) Installation succeeded." -ForegroundColor Green
