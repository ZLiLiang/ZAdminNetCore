﻿using Infrastructure;
using NetTaste;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using UAParser;
using Z.Model.System;

namespace Z.Admin.WebApi.Extensions
{
    /// <summary>
    /// HttpContext扩展类
    /// </summary>
    public static partial class HttpContextExtension
    {
        /// <summary>
        /// 是否是ajax请求
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return request.Headers["X-Requested-With"] == "XMLHttpRequest" || request.Headers != null && request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        /// <summary>
        /// 获取客户端IP
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetClientUserIp(this HttpContext context)
        {
            if (context == null) return "";
            var result = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(result))
            {
                result = context.Connection.RemoteIpAddress?.ToString();
            }
            if (string.IsNullOrEmpty(result))
            {
                throw new Exception("获取IP失败");
            }

            if (result.Contains("::1")) result = "127.0.0.1";

            result = result.Replace("::ffff:", "127.0.0.1");
            result = result.Split(';')?.FirstOrDefault() ?? "127.0.0.1";
            result = IsIP(result) ? result : "127.0.0.1";
            return result;
        }

        /// <summary>
        /// 判断是否IP
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsIP(string ip)
        {
            return Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }

        /// <summary>
        /// 获取登录用户id
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static long GetUId(this HttpContext context)
        {
            var uid = context.User.FindFirstValue(ClaimTypes.PrimarySid);
            return !string.IsNullOrEmpty(uid) ? long.Parse(uid) : 0;
        }

        /// <summary>
        /// 获取登录用户名
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetName(this HttpContext context)
        {
            var uid = context.User?.Identity?.Name;
            return uid;
        }

        /// <summary>
        /// 判断是否是管理员
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool IsAdmin(this HttpContext context)
        {
            var userName = context.GetName();
            return userName == GlobalConstant.AdminPerm;
        }

        /// <summary>
        /// ClaimsIdentity
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IEnumerable<ClaimsIdentity>? GetClaims(this HttpContext context)
        {
            return context.User?.Identities;
        }

        public static string GetUserAgent(this HttpContext context)
        {
            return context.Request.Headers["User-Agent"];
        }

        /// <summary>
        /// 获取请求令牌
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetToken(this HttpContext context)
        {
            return context.Request.Headers["Authorization"];
        }

        /// <summary>
        /// 获取浏览器信息
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ClientInfo GetClientInfo(this HttpContext context)
        {
            var str = context.GetUserAgent();
            var uaParser = UAParser.Parser.GetDefault();
            ClientInfo c = uaParser.Parse(str);

            return c;
        }

        /// <summary>
        /// 获取请求Url
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string? GetRequestUrl(this HttpContext context)
        {
            return context != null ? context.Request.Path.Value : "";
        }

        /// <summary>
        /// 获取请求参数
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetQueryString(this HttpContext context)
        {
            return context != null ? context.Request.QueryString.Value : "";
        }

        /// <summary>
        /// 获取body请求参数
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetBody(this HttpContext context)
        {
            context.Request.EnableBuffering();

            string body = string.Empty;
            var buffer = new MemoryStream();
            context.Request.Body.Seek(0, SeekOrigin.Begin);
            context.Request.Body.CopyToAsync(buffer);
            buffer.Position = 0;
            try
            {
                using StreamReader streamReader = new(buffer, Encoding.UTF8);
                body = streamReader.ReadToEndAsync().Result;
            }
            finally
            {
                buffer?.Dispose();
            }
            return body;
        }

        /// <summary>
        /// 设置请求参数
        /// </summary>
        /// <param name="operLog"></param>
        /// <param name="context"></param>
        public static void GetRequestValue(this HttpContext context,SysOperLog operLog)
        {
            string reqMethod = operLog.RequestMethod;
            string param = string.Empty;

            if (HttpMethods.IsPost(reqMethod) || HttpMethods.IsPut(reqMethod) || HttpMethods.IsDelete(reqMethod))
            {
                param = context.GetBody();
                param = PwdRep().Replace(param, "***");
            }
            operLog.OperParam = param;
        }

        [GeneratedRegex("(?<=\"password\":\")[^\",]*")]
        private static partial Regex PwdRep();
    }
}
