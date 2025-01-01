# TModWatcher

<div>
   <a href="https://github.com/ForOne-Club/TModWatcher"><img alt="Github repo" src="https://img.shields.io/website?url=https%3A%2F%2Fgithub.com%2FPJ-568%2Fqmole-source-site&up_color=007bff&up_message=TModWatcher&logo=github&label=Github" loading="lazy"></a>
	<img src="https://img.shields.io/github/languages/top/TrifingZW/TModWatcher?color=green" alt="languages-top" />
    <img alt="license" src="https://img.shields.io/github/license/TrifingZW/TModWatcher"/>
</div>


欢迎使用 **TModWatcher**！这款工具由 TrifingZW 开发，旨在帮助你自动生成资源的 C# 引用并编译着色器文件，让你的 Terraria
模组开发更加轻松！

## 工具简介

**TModWatcher** 是一个为 Terraria 模组开发者设计的自动化工具。它通过监控 TModLoader 项目解决方案，自动生成 C#
资源引用，并且在项目更改时自动编译着色器文件。此工具适用于 Windows 操作系统，不需要安装任何额外工具即可使用，极大简化了模组开发流程。

![演示图片](DemoImages/InterfaceScreenshot.png)

## 功能特点

- **自动生成 C# 资源引用**：根据项目中的资源文件，自动生成相应的 C# 代码引用，避免手动操作的繁琐。
- **自动编译着色器文件**：在资源发生变化时，自动编译 HLSL 着色器文件，确保项目始终使用最新的着色器。
- **实时监控项目文件夹**：实时监控指定的解决方案路径，一旦检测到资源文件变化，立即触发相应的操作。
- **自定义命名方式**：支持蛇形命名法（snake_case），并允许为生成的字段名称添加后缀。
- **C# 静态类可选生成方式**：支持普通模式和嵌套模式。

## 安装与使用

**TModWatcher** 无需安装，无需.NET运行时，只需下载并运行可执行文件即可使用。

### 基本使用方法

1. **前往[Releases](https://github.com/ForOne-Club/TModWatcher/releases)下载并解压 TModWatcher**
2. **编辑配置文件**
    - 在解压后的文件夹中找到 `TModWatcher.exe` 文件，先运行一遍后会生成配置文件 `WatcherSettings.json`
    - `WorkPath`：指向 tModLoader 模组项目的解决方案路径
    - `ShaderCompile`：指向 FX 着色器编译器的路径，默认为 `ShaderCompile/ShaderCompile.exe`
    - `SnakeCase`：是否启用蛇形命名法，默认为 `true`
    - `GenerateExtension`：是否为生成的字段名称添加后缀，默认为 `true`
    - `ResourcePath`：生成的C#资源引用静态类的相对路径，默认为 `Empty`
    - `ResourceName`：生成的C#资源引用静态类的类名称，默认为 `R.cs`
    - `FileTypes`：需要监控的文件类型，默认为 `[".png", ".jpg", ".webp", ".bmp", ".gif", ".mp3", ".wav", ".ogg", ".flac", ".xnb"]`
    - `IgnorePaths`：需要忽略的文件夹路径，默认为 `[".git", ".idea", ".vs", "bin", "obj", "Properties", "Localization", "Resource"]`
3. **运行命令或者直接运行 `.exe` 文件**：
   ```shell
   TModWatcher SettingsPath=WatcherSettings.json
   ```
    - `SettingsPath=path`：指向配置文件，默认为相对路径 `WatcherSettings.json`

### 示例

1. **打开配置文件（第一次需要先运行一遍 `TModWatcher.exe` 生成配置文件）**
   ![演示图片](DemoImages/DirectoryScreenshot.png)
2. **编辑配置文件**
   ![演示图片](DemoImages/ConfigurationScreenshot.png)
3. **双击运行`.exe`文件**
   ![演示图片](DemoImages/RunScreenshot.png)
4. **根据配置查看生成的 C# 代码**
   演示的配置是ResourcePath为空，ResourceName为R.cs，所以生成的C#资源引用静态类在项目根目录下。
   ![演示图片](DemoImages/ResourceScreenshot.png)
   ![演示图片](DemoImages/CSScreenshot.png)
5. **在项目中使用**

- 在代码里引用`R.cs文件`，然后使用`R.xxx`来访问资源。
   ```csharp
  using MoreVoodooDolls.R;
  ```
  ```csharp
  Texture2D  texture2D = ModContent.Request<Texture2D>(R.Icon_Png).Value;
  ```
- 或者使用`using static`，然后直接通过字段名访问资源。
  ```csharp
   using static MoreVoodooDolls.R;
  ```
  ```csharp
  Texture2D  texture2D = ModContent.Request<Texture2D>(Icon_Png).Value;
  ```
- 你也可以使用`global using static`,这样你就没有必要为每个类都引入`R.cs`文件。
  ```csharp
  global using static MoreVoodooDolls.R;
  ```
  ```csharp
  Texture2D  texture2D = ModContent.Request<Texture2D>(Icon_Png).Value;
  ```

## 项目地址与支持

**TModWatcher** 隶属于 [万物一心](https://github.com/ForOne-Club) 团队，这是一个致力于 Terraria 模组开发的团队。

<div align="center">
   <img src="https://avatars.githubusercontent.com/u/179108262?s=200&v=4" alt="万物一心">
</div>

如果你有任何问题，欢迎加入我们的 **QQ群**：574904188 或者联系开发者 **QQ**：3077446541。

你也可以访问开发者的 GitHub 主页：https://github.com/TrifingZW 获取更多信息。

## 开源协议

**TModWatcher** 基于 MIT 开源协议发布，请自觉遵守协议规则。

---

感谢您使用 **TModWatcher**，祝您在 Terraria 模组开发中一切顺利！
