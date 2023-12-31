# ZAdminNetCore
学习模仿项目

# **[原作者地址](https://gitee.com/izory/ZrAdminNetCore)**

# [部署教程](部署.md)

## 🍁 前端技术

Vue 版前端技术栈 ：基于 vue2.x/vue3.x/uniapp、vuex、vue-router 、vue-cli 、axios、 element-ui、echats、i18n 国际化等，前端采用 vscode 工具开发

## 🍀 后端技术

- 核心框架：.Net5.0/.Net7.0 + Web API + sqlsugar + swagger + signalR + IpRateLimit + Quartz.net + Redis
- 定时计划任务：Quartz.Net 组件，支持执行程序集或者 http 网络请求
- 安全支持：过滤器(数据权限过滤)、Sql 注入、请求伪造
- 日志管理：NLog、登录日志、操作日志、定时任务日志
- 工具类：验证码、丰富公共功能
- 接口限流：支持接口限流，避免恶意请求导致服务层压力过大
- 代码生成：高效率开发，代码生成器可以一键生成所有前后端代码
- 数据字典：支持数据字典，可以方便对一些状态进行管理
- 分库分表：使用 orm sqlsugar 可以很轻松的实现分库分库性能优越
- 多 租 户：支持多租户功能
- 缓存数据：内置内存缓存和 Redis

## 🍖 内置功能

1. 用户管理：用户是系统操作者，该功能主要完成系统用户配置。
2. 部门管理：配置系统组织机构（公司、部门、小组），树结构展现。
3. 岗位管理：配置系统用户所属担任职务。
4. 菜单管理：配置系统菜单，操作权限，按钮权限标识等。
5. 角色管理：角色菜单权限分配。
6. 字典管理：对系统中经常使用的一些较为固定的数据进行维护。
7. 操作日志：系统正常操作日志记录和查询；系统异常信息日志记录和查询。
8. 登录日志：系统登录日志记录查询包含登录异常。
9. 系统接口：使用 swagger 生成相关 api 接口文档。
10. 服务监控：监视当前系统 CPU、内存、磁盘、堆栈等相关信息。
11. 在线构建器：拖动表单元素生成相应的 VUE 代码(仅支持 vue2)。
12. 任务系统：基于 Quartz.NET，可以在线（添加、修改、删除、手动执行)任务调度包含执行结果日志。
13. 文章管理：可以写文章记录。
14. 代码生成：可以一键生成前后端代码(.cs、.vue、.js、.sql 等)支持下载，自定义配置前端展示控件、让开发更快捷高效（史上最强）。
15. 参数管理：对系统动态配置常用参数。
16. 发送邮件：可以对多个用户进行发送邮件。
17. 文件管理：可以进行上传文件管理，目前支持上传到本地、阿里云。
18. 通知管理：系统通知公告信息发布维护，使用 signalr 实现对用户实时通知。
19. 账号注册：可以注册账号登录系统。
20. 多语言管理：支持静态、后端动态配置国际化。目前只支持中、英、繁体(仅支持 vue3)

## 🍻 项目结构

```
├─ZR.Service             		->[服务层类库]：提供WebApi接口调用；
├─ZR.Repository                     ->[仓库层类库]：方便提供有执行存储过程的操作；
├─ZR.Model                		->[实体层类库]：提供项目中的数据库表、数据传输对象；
├─ZR.Admin.WebApi               	->[webapi接口]：为Vue版或其他三方系统提供接口服务。
├─ZR.Tasks               		->[定时任务类库]：提供项目定时任务实现功能；
├─ZR.CodeGenerator               	->[代码生成功能]：包含代码生成的模板、方法、代码生成的下载。
├─ZR.Vue               			->[前端UI]：vue2.0版本UI层。
├─document               		->[文档]：数据库脚本
```
