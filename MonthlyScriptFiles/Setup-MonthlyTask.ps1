# PowerShell script to create a scheduled task for the SusEquip Mail Sender
# Run this script as Administrator

param(
    [Parameter(Mandatory=$false)]
    [string]$ExecutablePath = "C:\SusEquip\SusEquipMailSender.exe",  # Default path - change this to your actual path
    
    [Parameter(Mandatory=$false)]
    [string]$TaskName = "SusEquip Monthly Mail Sender",
    
    [Parameter(Mandatory=$false)]
    [string]$Description = "Sends monthly notifications for equipment expiring soon"
)

# Validate that the executable exists
if (-not (Test-Path $ExecutablePath)) {
    Write-Error "Executable not found at: $ExecutablePath"
    exit 1
}

try {
    # Use COM object for better compatibility with older PowerShell versions
    $Service = New-Object -ComObject("Schedule.Service")
    $Service.Connect()
    
    $RootFolder = $Service.GetFolder("\")
    
    # Create the task definition
    $TaskDefinition = $Service.NewTask(0)
    $TaskDefinition.RegistrationInfo.Description = $Description
    $TaskDefinition.Settings.Enabled = $true
    $TaskDefinition.Settings.AllowDemandStart = $true
    $TaskDefinition.Settings.AllowHardTerminate = $true
    $TaskDefinition.Settings.StartWhenAvailable = $true
    $TaskDefinition.Settings.RunOnlyIfNetworkAvailable = $true
    
    # Create the trigger (monthly on 1st day at 9:00 AM)
    $Triggers = $TaskDefinition.Triggers
    $Trigger = $Triggers.Create(3) # 3 = Monthly trigger
    $Trigger.StartBoundary = "2025-08-01T09:00:00" # Start from next month
    $Trigger.DaysOfMonth = 1
    $Trigger.MonthsOfYear = 0xFFF # All months (bitwise: 111111111111)
    $Trigger.Enabled = $true
    
    # Create the action
    $Actions = $TaskDefinition.Actions
    $Action = $Actions.Create(0) # 0 = Execute action
    $Action.Path = $ExecutablePath
    $Action.WorkingDirectory = Split-Path $ExecutablePath -Parent
    
    # Set the principal (run as SYSTEM)
    $Principal = $TaskDefinition.Principal
    $Principal.UserId = "SYSTEM"
    $Principal.LogonType = 5 # Service account
    $Principal.RunLevel = 1 # Highest privileges
    
    # Register the task
    $RootFolder.RegisterTaskDefinition($TaskName, $TaskDefinition, 6, $null, $null, 5) # 6 = Create or Update, 5 = Service logon

    Write-Host "✅ Scheduled task '$TaskName' created successfully!" -ForegroundColor Green
    Write-Host "   - Runs on the 1st of every month at 9:00 AM" -ForegroundColor Gray
    Write-Host "   - Executable: $ExecutablePath" -ForegroundColor Gray
    
    # Show the created task using schtasks command for compatibility
    schtasks /query /tn $TaskName /fo LIST
}
catch {
    Write-Error "Failed to create scheduled task: $_"
    exit 1
}
