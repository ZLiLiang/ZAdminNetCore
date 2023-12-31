﻿using Infrastructure.Attribute;
using Infrastructure.CustomException;
using Infrastructure.Enums;
using Infrastructure.Extensions;
using Infrastructure.Model;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Z.Admin.WebApi.Filters;
using Z.Common;
using Z.Model;
using Z.Model.System.Dto;
using Z.Service.System;
using Z.Service.System.IService;

namespace Z.Admin.WebApi.Controllers.System
{
    /// <summary>
    /// 多语言配置Controller
    /// </summary>
    [Verify]
    [Route("system/CommonLang")]
    public class CommonLangController : BaseController
    {
        /// <summary>
        /// 多语言配置接口
        /// </summary>
        private readonly ICommonLangService _commonLangService;

        public CommonLangController(ICommonLangService commonLangService)
        {
            _commonLangService = commonLangService;
        }

        /// <summary>
        /// 查询多语言配置列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "system:lang:list")]
        public IActionResult QueryCommonLang([FromQuery] CommonLangQueryDto parm)
        {
            if (parm.ShowMode == 2)
            {
                PagedInfo<dynamic> pagedInfo = new()
                {
                    Result = _commonLangService.GetListToPivot(parm)
                };

                return SUCCESS(pagedInfo);
            }

            return SUCCESS(_commonLangService.GetList(parm));
        }

        /// <summary>
        /// 查询多语言配置列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("list/{lang}")]
        [AllowAnonymous]
        public IActionResult QueryCommonLangs(string lang)
        {
            var msgList = _commonLangService.GetLangList(new CommonLangQueryDto
            {
                LangCode = lang
            });

            return SUCCESS(_commonLangService.SetLang(msgList));
        }

        /// <summary>
        /// 查询多语言配置详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [ActionPermissionFilter(Permission = "system:lang:query")]
        public IActionResult GetCommonLang(long Id)
        {
            var response = _commonLangService.GetFirst(x => x.Id == Id);

            var list = _commonLangService.GetList(x => x.LangKey == response.LangKey);
            var vo = list.Adapt<List<CommonLangDto>>();
            var modal = new CommonLangDto()
            {
                LangKey = response.LangKey,
                LangList = vo
            };

            return SUCCESS(modal);
        }

        /// <summary>
        /// 查询多语言配置详情
        /// </summary>
        /// <param name="langKey"></param>
        /// <returns></returns>
        [HttpGet("key/{langkey}")]
        [ActionPermissionFilter(Permission = "system:lang:query")]
        public IActionResult GetCommonLangByKey(string langKey)
        {
            var list = _commonLangService.GetList(x => x.LangKey == langKey);
            var vo = list.Adapt<List<CommonLangDto>>();
            var modal = new CommonLangDto()
            {
                LangKey = langKey,
                LangList = vo
            };

            return SUCCESS(modal);
        }

        /// <summary>
        /// 更新多语言配置
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "system:lang:edit")]
        [Log(Title = "多语言配置", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateCommonLang([FromBody] CommonLangDto parm)
        {
            if (parm == null || parm.LangKey.IsEmpty())
            {
                throw new CustomException("请求实体不能为空");
            }

            _commonLangService.StorageCommonLang(parm);

            return ToResponse(1);
        }

        /// <summary>
        /// 删除多语言配置
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "system:lang:delete")]
        [Log(Title = "多语言配置", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteCommonLang(string ids)
        {
            long[] idsArr = Tools.SpitLongArrary(ids);
            if (idsArr.Length <= 0) { return ToResponse(ApiResult.Error($"删除失败Id 不能为空")); }

            var response = _commonLangService.Delete(idsArr);

            return ToResponse(response);
        }

        /// <summary>
        /// 导出多语言配置
        /// </summary>
        /// <returns></returns>
        [Log(Title = "多语言配置", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "system:lang:export")]
        public IActionResult Export([FromQuery] CommonLangQueryDto parm)
        {
            parm.PageSize = 10000;
            var list = _commonLangService.GetList(parm).Result;

            string sFileName = ExportExcel(list, "CommonLang", "多语言配置");
            return SUCCESS(new { path = "/export/" + sFileName, fileName = sFileName });
        }
    }
}
