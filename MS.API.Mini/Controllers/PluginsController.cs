using Asp.Versioning;
using MS.API.Mini.Data;
using MS.API.Mini.Extensions;

namespace MS.API.Mini.Controllers;

public class PluginsController(MonitorDBContext _dbCtx): ControllerBaseExtension
{
    [MapToApiVersion(1)]
    [HttpGet]
    public async Task<IActionResult> GetPlugins()
    {
       var plugins = await _dbCtx.MonitorPlugins.ToListAsync();
       return Ok(plugins);
    }
}