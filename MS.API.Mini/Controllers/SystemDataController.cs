using Asp.Versioning;
using MS.API.Mini.Contracts;
using MS.API.Mini.Data;
using MS.API.Mini.Data.Models;
using MS.API.Mini.Extensions;

namespace MS.API.Mini.Controllers
{
    public class SystemDataController(
        IDBContract dbContractor,
        ILogger<SystemDataController> logger) : ControllerBaseExtension
    {
        [MapToApiVersion(1)]
        [HttpGet]
        public async Task<ActionResult<APIResponse<SystemMetric>>> SystemData([FromQuery(Name = "agent")] string AgentID,
            [FromQuery] string Entity, [FromQuery] long startPeriod, [FromQuery] long endPeriod)
        {
            logger.LogDebug("Start period: {StartPeriod}, end period: {EndPeriod}", startPeriod, endPeriod);
            var sysMetrics = await dbContractor.GetSystemMetricsAsync(AgentID, Entity, startPeriod, endPeriod);

            return Success(sysMetrics, "FBM", null);
        }

        [HttpGet("disks")]
        public async Task<ActionResult<APIResponse<DiskData>>> SystemDisks([FromQuery(Name = "agent")] string AgentID)
        {
            var diskDataResponse =
                await dbContractor.GetSystemDiskData(AgentID);
            
            return Ok(diskDataResponse);
        }

        // [HttpGet("disks/{AgentID}")]
        // [Authorize]
        // public async Task<ActionResult<APIResponse<DiskData>>> SystemDiskData(string agentID)
        // {
        //     return Success(await .GetMetricsListAsync<DiskData>($"{agentID}.disk"));
        // }

        [HttpPost("rdp")]
        public ActionResult GenerateRDPFile(string IPAddress, string Username, string Host)
        {
            // $"Invalid URL or unable to resolve the IP address. IP: {domain}";

            var rdpContent = string.Format("""
                                           
                                                       screen mode id:i:2
                                                       use multimon:i:0
                                                       desktopwidth:i:1920
                                                       desktopheight:i:1080
                                                       session bpp:i:32
                                                       winposstr:s:0,3,0,0,800,600
                                                       compression:i:1
                                                       keyboardhook:i:2
                                                       audiocapturemode:i:0
                                                       videoplaybackmode:i:1
                                                       connection type:i:2
                                                       networkautodetect:i:1
                                                       bandwidthautodetect:i:1
                                                       displayconnectionbar:i:1
                                                       enableworkspacereconnect:i:0
                                                       disable wallpaper:i:0
                                                       allow font smoothing:i:0
                                                       allow desktop composition:i:0
                                                       disable full window drag:i:1
                                                       disable menu anims:i:1
                                                       disable themes:i:0
                                                       disable cursor setting:i:0
                                                       bitmapcachepersistenable:i:1
                                                       full address:s:{0}
                                                       server port:i:3389
                                                       username:s:{1}
                                                       domain:s:{2}
                                                       audiomode:i:0
                                                       redirectprinters:i:1
                                                       redirectcomports:i:0
                                                       redirectsmartcards:i:1
                                                       redirectclipboard:i:1
                                                       redirectposdevices:i:0
                                                       autoreconnection enabled:i:1
                                                       authentication level:i:2
                                                       prompt for credentials:i:0
                                                       negotiate security layer:i:1
                                                       remoteapplicationmode:i:0
                                                       alternate shell:s:
                                                       shell working directory:s:
                                                       gatewayhostname:s:
                                                       gatewayusagemethod:i:4
                                                       gatewaycredentialssource:i:4
                                                       gatewayprofileusagemethod:i:0
                                                       promptcredentialonce:i:0
                                                       use redirection server name:i:0
                                                       rdgiskdcproxy:i:0
                                                       kdcproxyname:s:
                                                       ``
                                           """, IPAddress, Username, Host);

            var fileContents = Encoding.UTF8.GetBytes(rdpContent);
            const string contentType = "application/x-rdp";
            //"application/octet-stream"

            var uuid = Guid.NewGuid();
            var filePath = $"RemoteConnection-{IPAddress}, PSM Address-{uuid}.rdp";

            Console.WriteLine($"RDP file created at: {Path.GetFullPath(filePath)}");

            return File(fileContents, contentType, filePath);
        }
    }
}