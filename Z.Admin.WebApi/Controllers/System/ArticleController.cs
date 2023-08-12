using Infrastructure.Attribute;
using Infrastructure.CustomException;
using Infrastructure.Enums;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Z.Admin.WebApi.Extensions;
using Z.Admin.WebApi.Filters;
using Z.Model.System;
using Z.Model.System.Dto;
using Z.Service.System.IService;

namespace Z.Admin.WebApi.Controllers.System
{
    /// <summary>
    /// 文章管理
    /// </summary>
    [Verify]
    [Route("article")]
    public class ArticleController : BaseController
    {
        /// <summary>
        /// 文章接口
        /// </summary>
        private readonly IArticleService _articleService;
        private readonly IArticleCategoryService _articleCategoryService;

        public ArticleController(IArticleService articleService, IArticleCategoryService articleCategoryService)
        {
            _articleService = articleService;
            _articleCategoryService = articleCategoryService;
        }

        /// <summary>
        /// 查询文章列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "system:article:list")]
        public IActionResult Query([FromQuery] ArticleQueryDto parm)
        {
            var response = _articleService.GetList(parm);

            return SUCCESS(response);
        }

        /// <summary>
        /// 查询我的文章列表
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("mylist")]
        public IActionResult QueryMyList([FromQuery] ArticleQueryDto parm)
        {
            parm.UserId = HttpContext.GetUId();
            var response = _articleService.GetMyList(parm);

            return SUCCESS(response);
        }

        /// <summary>
        /// 查询最新文章列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("newlist")]
        public IActionResult QueryNew()
        {
            var predicate = Expressionable.Create<Article>()
                .And(m => m.Status == "1")
                .And(m => m.IsPublic == 1);

            var response = _articleService.Queryable()
                .Where(predicate.ToExpression())
                .Includes(x => x.ArticleCategoryNav)//填充子对象
                .Take(10)
                .OrderBy(f => f.UpdateTime, OrderByType.Desc)
                .ToList();

            return SUCCESS(response);
        }

        /// <summary>
        /// 查询文章详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult Get(int id)
        {
            long userId = HttpContext.GetUId();
            var response = _articleService.GetId(id);
            var model = response.Adapt<ArticleDto>();
            if (model.IsPublic == 0 && userId != model.UserId)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, "访问失败");
            }
            if (model != null)
            {
                model.ArticleCategoryNav = _articleCategoryService.GetById(model.CategoryId);
                model.TagList = model.Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries);
            }
            return SUCCESS(model);
        }

        /// <summary>
        /// 添加文章
        /// </summary>
        /// <returns></returns>
        [HttpPost("add")]
        [ActionPermissionFilter(Permission = "system:article:add")]
        [Log(Title = "发布文章", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] ArticleDto parm)
        {
            var addModel = parm.Adapt<Article>().ToCreate(context: HttpContext);
            addModel.AuthorName = HttpContext.GetName();

            return SUCCESS(_articleService.InsertReturnIdentity(addModel));
        }

        /// <summary>
        /// 更新文章
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpPut("edit")]
        [ActionPermissionFilter(Permission = "system:article:update")]
        [Log(Title = "文章修改", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] ArticleDto parm)
        {
            parm.AuthorName = HttpContext.GetName();
            var modal = parm.Adapt<Article>().ToUpdate(HttpContext);
            var response = _articleService.UpdateArticle(modal);

            return SUCCESS(response);
        }

        /// <summary>
        /// 删除文章
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ActionPermissionFilter(Permission = "system:article:delete")]
        [Log(Title = "文章删除", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(int id = 0)
        {
            var response = _articleService.Delete(id);
            return SUCCESS(response);
        }
    }
}
