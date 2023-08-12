using System.Collections.Generic;
using Z.Model;
using Z.Model.System;
using Z.Model.System.Dto;
using Z.Service;

namespace Z.Service.System.IService
{
    /// <summary>
    /// 通知公告表service接口
    ///
    /// @author zr
    /// @date 2021-12-15
    /// </summary>
    public interface ISysNoticeService : IBaseService<SysNotice>
    {
        List<SysNotice> GetSysNotices();

        PagedInfo<SysNotice> GetPageList(SysNoticeQueryDto parm);
    }
}
