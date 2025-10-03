# Enable ASP.NET Core Logging for Troubleshooting
# Run this script to enable detailed logging on the server

$targetPath = "\\sus-pequip01\inetpub\wwwroot\SusEquip"
$webConfigPath = "$targetPath\web.config"

Write-Host "Enabling detailed logging for SusEquip troubleshooting..." -ForegroundColor Yellow

# Create logs directory if it doesn't exist
$logsPath = "$targetPath\logs"
if (-not (Test-Path $logsPath)) {
    New-Item -Path $logsPath -ItemType Directory -Force
    Write-Host "Created logs directory: $logsPath" -ForegroundColor Green
}

# Update web.config to enable stdout logging
$webConfigContent = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\SusEquip.dll" 
                  stdoutLogEnabled="true" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
          <environmentVariable name="ASPNETCORE_DETAILEDERRORS" value="true" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
"@

# Backup current web.config
Copy-Item -Path $webConfigPath -Destination "$webConfigPath.backup" -Force
Write-Host "Backed up current web.config" -ForegroundColor Green

# Write new web.config
$webConfigContent | Out-File -FilePath $webConfigPath -Encoding UTF8 -Force
Write-Host "Updated web.config with detailed logging enabled" -ForegroundColor Green

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Try accessing the application again" -ForegroundColor White
Write-Host "2. Check the logs in: $logsPath" -ForegroundColor White
Write-Host "3. Look for stdout log files for detailed error information" -ForegroundColor White
Write-Host ""
Write-Host "Common server-side issues to check:" -ForegroundColor Yellow
Write-Host "- .NET 8.0 Runtime installed on sus-pequip01" -ForegroundColor White
Write-Host "- IIS ASP.NET Core Module v2 installed" -ForegroundColor White
Write-Host "- Application pool permissions" -ForegroundColor White
Write-Host "- Database connectivity from server" -ForegroundColor White