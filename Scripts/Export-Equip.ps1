Install-Module -Name SqlServer -Scope CurrentUser
Install-Module -Name ImportExcel -Scope CurrentUser


# Define the SQL Server instance, database, and query
$SqlServer = "SUS-EL-MPARK1\MSSQLSERVER01"
$Database = "MarkParkingDB"
$Query = "SELECT * FROM Equip"
$timestamp = Get-Date -Format "dd-MM-yyyy  HH:mm"

# Define the output file path
$OutputFilePath = "C:\Users\Public\Documents\Equip-Exports\EquipData_$timestamp.xlsx"

$Results = Invoke-Sqlcmd -ServerInstance $SqlServer -Database $Database -Query $Query -Username "sa" -Password "HejMedDig1234" -TrustServerCertificate

# Convert the results to a custom object
$CustomResults = $Results | ForEach-Object {
    New-Object PSObject -Property ([ordered]@{
        Entry_Id = $_.Entry_Id
        Entry_Date = $_.Entry_Date
        inst_no = $_.inst_no
        creator_initials = $_.creator_initials
        app_owner = $_.app_owner
        status = $_.status
        serial_no = $_.serial_no
        MAC_Address1 = $_.MAC_Address1
        MAC_Address2 = $_.MAC_Address2
        UUID = $_.UUID
        Product_no = $_.Product_no
        model_name_and_no = $_.model_name_and_no
        Pc_Name = $_.Pc_Name
        Service_Start = $_.Service_Start
        Service_Ends = $_.Service_Ends
        Note = $_.Note
        Department = $_.Department
    })
}



$CustomResults | Export-Excel -Path $OutputFilePath -WorksheetName "EquipData" -AutoSize -FreezeTopRow -BoldTopRow

