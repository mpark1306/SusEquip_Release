using Microsoft.Extensions.Logging;
using SusEquip.Data.Interfaces.Services;
using SusEquip.Data.Services.Validation;
using SusEquip.Data.Models;
using System.Text.RegularExpressions;

namespace SusEquip.Data.Services
{
    /// <summary>
    /// Implementation of validation service that orchestrates validation strategies
    /// </summary>
    public class ValidationService : IValidationService
    {
        private readonly IEnumerable<IValidationStrategy> _validationStrategies;
        private readonly IEquipmentService _equipmentService;
        private readonly ILogger<ValidationService> _logger;

        public ValidationService(
            IEnumerable<IValidationStrategy> validationStrategies,
            IEquipmentService equipmentService,
            ILogger<ValidationService> logger)
        {
            _validationStrategies = validationStrategies;
            _equipmentService = equipmentService;
            _logger = logger;
        }

        public async Task<List<ValidationIssue>> ValidateAllEquipmentAsync()
        {
            try
            {
                _logger.LogInformation("Starting validation of all equipment");
                
                var allEquipment = await _equipmentService.GetEquipmentAsync();
                var allIssues = new List<ValidationIssue>();

                foreach (var equipment in allEquipment)
                {
                    var issues = await ValidateEquipmentInternal(equipment);
                    allIssues.AddRange(issues);
                }

                _logger.LogInformation("Completed validation of all equipment: {TotalIssues} issues found across {EquipmentCount} items",
                    allIssues.Count, allEquipment.Count());

                return allIssues;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during validation of all equipment");
                return new List<ValidationIssue>();
            }
        }

        public async Task<List<ValidationIssue>> ValidateEquipmentAsync(EquipmentData equipment)
        {
            try
            {
                var issues = await ValidateEquipmentInternal(equipment);
                return issues.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating equipment {PCName}", equipment.PC_Name);
                return new List<ValidationIssue>();
            }
        }

        public async Task<bool> IsValidEquipmentAsync(EquipmentData equipment)
        {
            try
            {
                var issues = await ValidateEquipmentInternal(equipment);
                return !issues.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking validity of equipment {PCName}", equipment.PC_Name);
                return false;
            }
        }

        // Issue management
        public async Task IgnoreIssueAsync(int issueId, string reason, string ignoredBy)
        {
            try
            {
                _logger.LogInformation("Marking issue {IssueId} as ignored by {IgnoredBy} with reason: {Reason}", 
                    issueId, ignoredBy, reason);
                
                // TODO: Implement database persistence for ignored issues
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ignoring issue {IssueId}", issueId);
                throw;
            }
        }

        public async Task UnignoreIssueAsync(int issueId)
        {
            try
            {
                _logger.LogInformation("Unmarking issue {IssueId} as ignored", issueId);
                
                // TODO: Implement database persistence for unignored issues
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unignoring issue {IssueId}", issueId);
                throw;
            }
        }

        public async Task MarkIssueSolvedAsync(int issueId, string solution, string solvedBy)
        {
            try
            {
                _logger.LogInformation("Marking issue {IssueId} as solved by {SolvedBy} with solution: {Solution}", 
                    issueId, solvedBy, solution);
                
                // TODO: Implement database persistence for solved issues
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking issue {IssueId} as solved", issueId);
                throw;
            }
        }

        // Issue retrieval
        public async Task<List<int>> GetIgnoredIssueIdsAsync()
        {
            try
            {
                // TODO: Implement database query for ignored issue IDs
                await Task.CompletedTask;
                return new List<int>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ignored issue IDs");
                return new List<int>();
            }
        }

        public async Task<List<int>> GetSolvedIssueIdsAsync()
        {
            try
            {
                // TODO: Implement database query for solved issue IDs
                await Task.CompletedTask;
                return new List<int>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving solved issue IDs");
                return new List<int>();
            }
        }

        // Validation rules
        public async Task<bool> IsValidMacAddressAsync(string macAddress)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(macAddress))
                    return false;

                // MAC address formats: XX:XX:XX:XX:XX:XX, XX-XX-XX-XX-XX-XX, XXXXXXXXXXXX
                var patterns = new[]
                {
                    @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$", // XX:XX:XX:XX:XX:XX or XX-XX-XX-XX-XX-XX
                    @"^[0-9A-Fa-f]{12}$" // XXXXXXXXXXXX
                };

                return await Task.FromResult(patterns.Any(pattern => 
                    Regex.IsMatch(macAddress, pattern)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating MAC address {MacAddress}", macAddress);
                return false;
            }
        }

        public async Task<bool> IsValidUuidAsync(string uuid)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(uuid))
                    return false;

                // UUID format: XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX
                var pattern = @"^[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}$";
                
                return await Task.FromResult(Regex.IsMatch(uuid, pattern));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating UUID {UUID}", uuid);
                return false;
            }
        }

        public async Task<bool> IsValidInstNoAsync(int instNo)
        {
            try
            {
                // Check if inst no is positive and within reasonable range
                if (instNo <= 0 || instNo > 999999) // Max 6 digits
                    return false;

                // Check if inst no is already taken using search methods available
                var allEquipment = await _equipmentService.GetEquipmentAsync();
                return !allEquipment.Any(e => e.Inst_No == instNo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating inst no {InstNo}", instNo);
                return false;
            }
        }

        public async Task<bool> IsValidSerialNoAsync(string serialNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(serialNo))
                    return false;

                // Check format (8-12 alphanumeric characters)
                if (!Regex.IsMatch(serialNo, @"^[A-Z0-9]{8,12}$", RegexOptions.IgnoreCase))
                    return false;

                // Check if serial no is already taken
                var allEquipment = await _equipmentService.GetEquipmentAsync();
                return !allEquipment.Any(e => string.Equals(e.Serial_No, serialNo, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating serial no {SerialNo}", serialNo);
                return false;
            }
        }

        // Internal helper method for validation
        private async Task<IEnumerable<ValidationIssue>> ValidateEquipmentInternal(BaseEquipmentData equipment)
        {
            var allIssues = new List<ValidationIssue>();

            try
            {
                // Get applicable strategies and sort by priority
                var applicableStrategies = _validationStrategies
                    .Where(s => s.CanValidate(equipment))
                    .OrderByDescending(s => s.Priority);

                _logger.LogDebug("Validating equipment {PCName} with {StrategyCount} strategies",
                    equipment.PC_Name, applicableStrategies.Count());

                // Run all applicable validation strategies
                foreach (var strategy in applicableStrategies)
                {
                    try
                    {
                        var issues = await strategy.ValidateAsync(equipment);
                        allIssues.AddRange(issues);

                        _logger.LogDebug("Strategy {StrategyName} found {IssueCount} issues for {PCName}",
                            strategy.StrategyName, issues.Count(), equipment.PC_Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in validation strategy {StrategyName} for equipment {PCName}",
                            strategy.StrategyName, equipment.PC_Name);
                    }
                }

                _logger.LogDebug("Validation completed for {PCName}: {TotalIssues} issues found",
                    equipment.PC_Name, allIssues.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during equipment validation for {PCName}", equipment.PC_Name);
            }

            return allIssues;
        }
    }
}