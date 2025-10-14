# 📝 基于Statiq框架的个人博客

![.NET Version](https://img.shields.io/badge/.NET-8.0+-blue)
![License](https://img.shields.io/badge/license-MIT-green)
![Build](https://img.shields.io/badge/build-Statiq-blueviolet)
![TailwindCSS](https://img.shields.io/badge/style-TailwindCSS-06B6D4)

## 📖 项目简介

**MyBlog** 是一个基于 **Statiq** 静态站点生成器与 **Razor 模板引擎** 构建的个人博客网站。  
通过在本地维护 Markdown 文件并自动生成 HTML 页面，可以轻松管理和展示自己的技术文章。  

前端部分采用 **TailwindCSS** 进行响应式布局设计，并支持 **浅色/深色主题切换**。  
项目主要面向开发者与独立创作者，专注于简洁、高效与自我掌控的内容发布体验。

---

## 🧩 技术栈

| 模块         | 使用技术                              |
| ------------ | ------------------------------------- |
| 静态站点生成 | [Statiq.Web](https://www.statiq.dev/) |
| 模板引擎     | Razor (.cshtml)                       |
| 样式系统     | TailwindCSS (通过 CDN 引入)           |
| 内容格式     | Markdown (.md)                        |
| 运行环境     | .NET 8.0+                             |
| 部署环境     | 任意支持静态站点的服务器              |

---

## 📁 项目结构

```
Myblog/
│
├── Helpers/				  # 工具类目录
├── input/
│   ├── assets/				  # 资源文件目录
│   ├── posts/                # Markdown 文章目录
│   │   ├── example-post.md
│   │   └── ...
│   ├── _Layout.cshtml        # 全局布局模板
│   ├── index.cshtml          # 首页模板（列出文章）
│
├── output/                   # 构建后的静态网站输出
│   ├── assets/				  # 资源文件目录
|	├── posts/                # 输出文章目录
│   │   ├── example-post.html
│   │   └── ...
│   ├── index.html          # 首页页面
├── Program.cs				  # 程序入口
└── README.md
```

> ⚙️ Statiq 会将 `input/posts/` 中的 Markdown 文件解析、渲染为 HTML 文件，并输出到 `output/post/` 目录下。

---

## 🚀 本地运行

### 1️⃣ 安装 .NET SDK

请确保系统安装了 [.NET 8.0 或更高版本](https://dotnet.microsoft.com/download)。

### 2️⃣ 安装依赖包

```bash
dotnet add package Statiq.Web
```

### 3️⃣ 进入项目目录并构建静态文件

```bash
npm run dev
```

### 4️⃣ 运行本地服务器

```bash
cd output
anywhere
```

---

## ✨ 特性

- 📝 **Markdown 自动渲染**：将 `.md` 文件转为完整网页  
- 🎨 **TailwindCSS 响应式布局**：适配桌面与移动端  
- 🌗 **日夜模式切换**：自动或手动切换主题  
- 🧠 **Razor 模板灵活扩展**：可添加组件或布局逻辑  
- 🚫 **无需数据库 / 后端**：纯静态内容，快速部署  
- 🛡️ **适合个人博客、知识文档、作品展示等场景**

---

## 🌐 部署方式

### 1. 生成静态站点

```bash
npm run build:css
dotnet run
```

构建完成后，生成的静态文件会保存在 `/output` 目录中。

### 2. 部署到服务器或平台

你可以选择任意静态站点托管方式，例如：

- GitHub Pages  
- Netlify  
- Vercel  

**GitHub Pages 示例配置：**

```yml
# ---- TailwindCss Build ----
- name: Setup Node.js
uses: actions/setup-node@v4
with:
  node-version: '20'
  cache: 'npm'

- name: Install npm packages
run: npm ci || npm install

- name: Ensure css output folder exists
run: mkdir -p input/assets/css

- name: Build tailwindcss
run: npm run build:css

# ---- Statiq Build ----
- name: Setup .NET 8
uses: actions/setup-dotnet@v4
with:
  dotnet-version: '8.0.x'

- name: Restore NuGet packages
run: dotnet restore

- name: Restore dotnet tools (if any)
run: dotnet tool restore || echo "no dotnet tools to restore"

- name: Build blog site with Statiq
run: dotnet run --configuration Release
env:
  SITE_LINKROOT: "/blog"
```

---

## 🧩 自定义与扩展

- 修改 `_Layout.cshtml` 以更改全局布局  
- 在 `input/posts/` 中新增 Markdown 文件即可发布新文章  
- 可通过 CSS 变量或自定义 Tailwind 配置进行主题个性化  

---

## 🧱 运行环境建议

| 项目阶段 | 环境要求                           |
| -------- | ---------------------------------- |
| 开发     | .NET 8.0+ SDK                      |
| 部署     | Linux (Ubuntu 22.04+ 推荐)         |
| 构建命令 | `npm run build:css` + `dotnet run` |
| 本地调试 | `npm run dev`                      |

---

## 📄 开源许可

本项目采用 **MIT License** 开源协议。  
你可以自由使用、修改、分发本项目代码，但请保留原作者署名。
