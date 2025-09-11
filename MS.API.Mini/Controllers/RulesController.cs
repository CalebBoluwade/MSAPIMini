using MS.API.Mini.Data;
using MS.API.Mini.Data.Models;
using MS.API.Mini.Extensions;
using MS.API.Mini.Services;

namespace MS.API.Mini.Controllers;

public class RulesController(IRuleService ruleService) : ControllerBaseExtension
{
    [MapToApiVersion(1)]
    [HttpPost]
    public async Task<ActionResult<APIResponse<MonitoringRule>>> CreateRule([FromBody] CreateRuleRequest request)
    {
        var result = await ruleService.CreateRuleAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [MapToApiVersion(1)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<APIResponse<MonitoringRule>>> UpdateRule(Guid id, [FromBody] UpdateRuleRequest request)
    {
        var result = await ruleService.UpdateRuleAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [MapToApiVersion(1)]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<APIResponse<bool>>> DeleteRule(Guid id)
    {
        var result = await ruleService.DeleteRuleAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [MapToApiVersion(1)]
    [HttpGet]
    public async Task<ActionResult<APIResponse<PagedResult<MonitoringRule>>>> GetRules([FromQuery] RuleQueryParameters parameters)
    {
        var result = await ruleService.GetRulesAsync(parameters);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [MapToApiVersion(1)]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<APIResponse<MonitoringRule>>> GetRule(Guid id)
    {
        var result = await ruleService.GetRuleByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}