# Test script to validate the model hierarchy refactoring
Write-Host "Testing Model Hierarchy Refactoring..." -ForegroundColor Green

# Test 1: Verify the application compiles successfully
Write-Host "`n1. Testing Compilation..." -ForegroundColor Yellow
try {
    $buildResult = dotnet build --verbosity quiet --nologo 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Compilation successful" -ForegroundColor Green
    } else {
        Write-Host "❌ Compilation failed" -ForegroundColor Red
        Write-Host $buildResult
        exit 1
    }
} catch {
    Write-Host "❌ Build error: $($_)" -ForegroundColor Red
    exit 1
}

# Test 2: Check if key files exist
Write-Host "`n2. Testing File Structure..." -ForegroundColor Yellow
$requiredFiles = @(
    "Data\Models\BaseEquipmentData.cs",
    "Data\Models\EquipmentData.cs", 
    "Data\Models\OLDEquipmentData.cs",
    "Data\Models\MachineData.cs"
)

foreach ($file in $requiredFiles) {
    if (Test-Path $file) {
        Write-Host "✅ $file exists" -ForegroundColor Green
    } else {
        Write-Host "❌ $file missing" -ForegroundColor Red
        exit 1
    }
}

# Test 3: Verify inheritance relationships in the code
Write-Host "`n3. Testing Inheritance Relationships..." -ForegroundColor Yellow

# Check EquipmentData inherits from BaseEquipmentData
$equipmentContent = Get-Content "Data\Models\EquipmentData.cs" -Raw
if ($equipmentContent -match "class EquipmentData : BaseEquipmentData") {
    Write-Host "✅ EquipmentData inherits from BaseEquipmentData" -ForegroundColor Green
} else {
    Write-Host "❌ EquipmentData inheritance not found" -ForegroundColor Red
    exit 1
}

# Check OLDEquipmentData inherits from BaseEquipmentData
$oldEquipmentContent = Get-Content "Data\Models\OLDEquipmentData.cs" -Raw
if ($oldEquipmentContent -match "class OLDEquipmentData : BaseEquipmentData") {
    Write-Host "✅ OLDEquipmentData inherits from BaseEquipmentData" -ForegroundColor Green
} else {
    Write-Host "❌ OLDEquipmentData inheritance not found" -ForegroundColor Red
    exit 1
}

# Check MachineData inherits from EquipmentData
$machineContent = Get-Content "Data\Models\MachineData.cs" -Raw
if ($machineContent -match "class MachineData : EquipmentData") {
    Write-Host "✅ MachineData inherits from EquipmentData" -ForegroundColor Green
} else {
    Write-Host "❌ MachineData inheritance not found" -ForegroundColor Red
    exit 1
}

# Test 4: Verify BaseEquipmentData is abstract
Write-Host "`n4. Testing Abstract Base Class..." -ForegroundColor Yellow
$baseContent = Get-Content "Data\Models\BaseEquipmentData.cs" -Raw
if ($baseContent -match "abstract class BaseEquipmentData") {
    Write-Host "✅ BaseEquipmentData is abstract" -ForegroundColor Green
} else {
    Write-Host "❌ BaseEquipmentData not abstract" -ForegroundColor Red
    exit 1
}

# Test 5: Check for duplicate property removal (should not find PC_Name in child classes)
Write-Host "`n5. Testing Duplicate Property Removal..." -ForegroundColor Yellow

# Count PC_Name occurrences in EquipmentData (should be 0 after refactoring)
$equipmentPCNameCount = ($equipmentContent | Select-String "public.*PC_Name" -AllMatches).Matches.Count
if ($equipmentPCNameCount -eq 0) {
    Write-Host "✅ PC_Name removed from EquipmentData (inherits from base)" -ForegroundColor Green
} else {
    Write-Host "❌ PC_Name still exists in EquipmentData ($equipmentPCNameCount occurrences)" -ForegroundColor Red
    exit 1
}

# Count PC_Name occurrences in OLDEquipmentData (should be 0 after refactoring) 
$oldPCNameCount = ($oldEquipmentContent | Select-String "public.*PC_Name" -AllMatches).Matches.Count
if ($oldPCNameCount -eq 0) {
    Write-Host "✅ PC_Name removed from OLDEquipmentData (inherits from base)" -ForegroundColor Green
} else {
    Write-Host "❌ PC_Name still exists in OLDEquipmentData ($oldPCNameCount occurrences)" -ForegroundColor Red
    exit 1
}

Write-Host "`n🎉 All Model Hierarchy Tests Passed! 🎉" -ForegroundColor Green
Write-Host "`nRefactoring Summary:" -ForegroundColor Cyan
Write-Host "- ✅ BaseEquipmentData abstract class created"
Write-Host "- ✅ EquipmentData refactored to inherit from base"  
Write-Host "- ✅ OLDEquipmentData refactored to inherit from base"
Write-Host "- ✅ MachineData enhanced with inheritance chain"
Write-Host "- ✅ Duplicate properties eliminated"
Write-Host "- ✅ Application compiles successfully"