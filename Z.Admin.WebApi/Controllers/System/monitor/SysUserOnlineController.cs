using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Z.Admin.WebApi.Filters;
using Z.Admin.WebApi.Hubs;
using Z.Model;

namespace Z.Admin.WebApi.Controllers.System.monitor
{
    [Verify]
    [Route("monitor/online")]
    public class SysUserOnlineController : BaseController
    {
        private IHubContext<Hub> HubContext;

        public SysUserOnlineController(IHubContext<Hub> hubContext)
        {
            HubContext = hubContext;
        }

        /// <summary>
        /// 获取在线用户列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        public IActionResult Index([FromQuery] PagerInfo pager)
        {
            var result = MessageHub.clientUsers
                .OrderByDescending(f => f.LoginTime)
                .Skip(pager.PageNum - 1)
                .Take(pager.PageSize);

            return SUCCESS(new
            {
                result,
                totalNum = MessageHub.clientUsers.Count
            });
        }
    }
}
