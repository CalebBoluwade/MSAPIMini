using System.Text.Json;
using MS.API.Mini.Data;
using MS.API.Mini.Data.Models;

namespace MS.API.Mini.Services;

public interface IRuleService
{
    Task<APIResponse<MonitoringRule>> CreateRuleAsync(CreateRuleRequest request);
    Task<APIResponse<MonitoringRule>> UpdateRuleAsync(Guid id, UpdateRuleRequest request);
    Task<APIResponse<bool>> DeleteRuleAsync(Guid id);
    Task<APIResponse<PagedResult<MonitoringRule>>> GetRulesAsync(RuleQueryParameters parameters);
    Task<APIResponse<MonitoringRule>> GetRuleByIdAsync(Guid id);
    Task<APIResponse<List<RuleConflict>>> CheckConflictsAsync(CreateRuleRequest request, Guid? existingRuleId = null);
}

public class RuleService(
    MonitorDBContext dbCtx,
    IRuleValidationService validationService,
    IActiveDirectoryService adService,
    ILogger<RuleService> logger)
    : IRuleService
{
    public async Task<APIResponse<MonitoringRule>> CreateRuleAsync(CreateRuleRequest request)
    {
       await using var transaction = await dbCtx.Database.BeginTransactionAsync();
        
        try
        {
            // Validate rule using FluentValidation
            var validationResult = await validationService.ValidateRuleAsync(request);
            if (!validationResult.IsValid)
            {
                return APIResponse<MonitoringRule>.ErrorResult(
                   ResponseMessage.FailedValidation, 
                    validationResult.Errors
                );
            }

            var serviceId = Guid.TryParse(request.ServiceId, out var parsedServiceId) ? parsedServiceId : Guid.Empty;

            var rule = new MonitoringRule
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                ServiceId = serviceId,
                Description = request.Description,
                MetricName = request.MetricName,
                Conditions = JsonSerializer.SerializeToDocument(request.Conditions),
                AlertChannels = request.AlertChannels,
                Constraints = request.Constraints != null
                    ? JsonSerializer.SerializeToDocument(request.Constraints)
                    : null,
                Priority = request.Priority,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = Guid.NewGuid() // TODO: Get actual user ID from claims
            };

            dbCtx.MonitoringRules.Add(rule);
            await dbCtx.SaveChangesAsync();

            // Check for conflicts
            var conflicts = await validationService.DetectConflictsAsync(rule);
            if (conflicts.Count > 0)
            {
                await dbCtx.RuleConflicts.AddRangeAsync(conflicts);
                await dbCtx.SaveChangesAsync();
                
                logger.LogWarning(
                    "Rule {RuleId} created with {ConflictCount} conflicts", 
                    rule.Id, conflicts.Count
                );
            }

            await transaction.CommitAsync();

            return APIResponse<MonitoringRule>.SuccessResult(rule, 
                conflicts.Count > 0 ? "Rule created with warnings" : "Rule created successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error creating rule: {RuleName}", request.Name);
            return APIResponse<MonitoringRule>.ErrorResult(ResponseMessage.Error, null!);
        }
    }

    public async Task<APIResponse<MonitoringRule>> UpdateRuleAsync(Guid id, UpdateRuleRequest request)
    {
        await using var transaction = await dbCtx.Database.BeginTransactionAsync();
        
        try
        {
            var rule = await dbCtx.MonitoringRules.FindAsync(id);
            if (rule == null)
            {
                return APIResponse<MonitoringRule>.ErrorResult("Rule not found");
            }

            // Validate update request
            var validationResult = await validationService.ValidateRuleAsync(request, id);
            if (!validationResult.IsValid)
            {
                return APIResponse<MonitoringRule>.ErrorResult(
                    "Validation failed", 
                    validationResult.Errors
                );
            }

            // Update rule properties
            rule.Name = request.Name;
            rule.Description = request.Description;
            rule.Conditions = JsonSerializer.SerializeToDocument(request.Conditions);
            rule.AlertChannels = request.AlertChannels;
            rule.Priority = request.Priority;
            rule.UpdatedAt = DateTime.UtcNow;

            // Clear existing conflicts for this rule
            var existingConflicts = await dbCtx.RuleConflicts
                .Where(c => c.RuleId1 == id || c.RuleId2 == id)
                .ToListAsync();
            
            dbCtx.RuleConflicts.RemoveRange(existingConflicts);

            // Check for new conflicts
            var newConflicts = await validationService.DetectConflictsAsync(rule);
            if (newConflicts.Count > 0)
            {
                await dbCtx.RuleConflicts.AddRangeAsync(newConflicts);
            }

            await dbCtx.SaveChangesAsync();
            await transaction.CommitAsync();

            return APIResponse<MonitoringRule>.SuccessResult(rule, 
                newConflicts.Count > 0 ? "Rule updated with warnings" : "Rule updated successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error updating rule: {RuleId}", id);
            return APIResponse<MonitoringRule>.ErrorResult("Internal server error");
        }
    }

 public async Task<APIResponse<bool>> DeleteRuleAsync(Guid id)
    {
        await using var transaction = await dbCtx.Database.BeginTransactionAsync();
        
        try
        {
            var rule = await dbCtx.MonitoringRules.FindAsync(id);
            if (rule == null)
            {
                return APIResponse<bool>.ErrorResult("Rule not found");
            }

            // Remove related conflicts
            var conflicts = await dbCtx.RuleConflicts
                .Where(c => c.RuleId1 == id || c.RuleId2 == id)
                .ToListAsync();
            
            dbCtx.RuleConflicts.RemoveRange(conflicts);
            dbCtx.MonitoringRules.Remove(rule);
            
            await dbCtx.SaveChangesAsync();
            await transaction.CommitAsync();

            return APIResponse<bool>.SuccessResult(true, "Rule deleted successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error deleting rule: {RuleId}", id);
            return APIResponse<bool>.ErrorResult(ResponseMessage.Error);
        }
    }

    public async Task<APIResponse<PagedResult<MonitoringRule>>> GetRulesAsync(RuleQueryParameters parameters)
    {
        try
        {
            // Set default pagination values if not provided
            var pageNumber = parameters.PageNumber <= 0 ? 1 : parameters.PageNumber;
            var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

            var query = dbCtx.MonitoringRules.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(parameters.MetricName))
            {
                query = query.Where(r => r.MetricName == parameters.MetricName);
            }

            query = parameters.IsActive.HasValue ? query.Where(r => r.IsActive == parameters.IsActive.Value) : query.Where(r => r.IsActive);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var rules = await query
                .OrderBy(r => r.Priority)
                .ThenBy(r => r.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Fetch recipients for each rule
            
            foreach (var rule in rules.Where(rule => rule.AlertChannels.Count > 0))
            {
                rule.Recipients = await adService.GetUsersByRuleIdAsync(rule.Id);
            }

            var totalPages = totalCount > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 0;

            var pagedResult = new PagedResult<MonitoringRule>
            {
                Data = rules,
                TotalCount = totalCount,
                Page = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return APIResponse<PagedResult<MonitoringRule>>.SuccessResult(pagedResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving rules");
            return APIResponse<PagedResult<MonitoringRule>>.ErrorResult("Internal server error");
        }
    }

    public async Task<APIResponse<MonitoringRule>> GetRuleByIdAsync(Guid id)
    {
        try
        {
            var rule = await dbCtx.MonitoringRules
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rule == null)
            {
                return APIResponse<MonitoringRule>.ErrorResult("Rule not found", null!);
            }

            return APIResponse<MonitoringRule>.SuccessResult(rule);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving rule: {RuleId}", id);
            return APIResponse<MonitoringRule>.ErrorResult("Internal server error");
        }
    }

    public async Task<APIResponse<List<RuleConflict>>> CheckConflictsAsync(CreateRuleRequest request, Guid? existingRuleId = null)
    {
        try
        {
            var tempRule = new MonitoringRule
            {
                Id = existingRuleId ?? Guid.NewGuid(),
                MetricName = request.MetricName,
                Conditions = JsonSerializer.SerializeToDocument(request.Conditions)
            };

            var conflicts = await validationService.DetectConflictsAsync(tempRule);
            return APIResponse<List<RuleConflict>>.SuccessResult(conflicts);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking conflicts for rule");
            return APIResponse<List<RuleConflict>>.ErrorResult("Internal server error");
        }
    }
}