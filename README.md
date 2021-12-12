# Jellyfin 插件

## 介绍

用于刮削某些影片元数据的 Jellyfin 插件。

目前已经支持 `AVMOO` `AVSOX`，后续会支持 `JavBus`、`SodPrime` 等。

> **建议**：
> 
> 建议**视频文件**使用 `番号` 命名，可以提高刮削的准确度。

> **提醒**：
>
> 支持的 Jellyfin 版本：10.7.6 以上版本。

## 安装

### 通用方法

在 `控制台` -> `插件` -> `存储库` 中，添加 `https://gitee.com/bg5tue/jellyfin-plugin/raw/master/manifest.json` 库，在 `目录` 里安装。

### Windows 系统

下载 zip 包，解压到 `C:\ProgramData\Jellyfin\Server\plugins` 目录下，重启 Jellyfin 服务器。

### Linux 系统

待完善...

## 使用
库管理中启用元数据下载器 `AvMoo Movie Provider` 和 图片下载器 `AvMoo Image Provider` 后，然后更新元数据。

## 更新记录

A：请参考插件设备重新设置可用域名，或考虑科学上网。

### Q：自动刮削的信息与实际不对应

A：视频文件文件名改为 `识别号` 命名可以增加准确率。

### Q：更新后识别的数据或封面异常

A：检查插件设置中的正则规则，更新插件后可能无法自动更新已经生成的规则文件，请停止 jellyfin 服务（`控制台` - `关机`），然后根据 jellyfin 版本，打开以下目录：

* Windows 绿色版：
  `C:\Users\用户名\AppData\Local\jellyfin\plugins\configurations` 

* Windows 安装版：`C:\ProgramData\Jellyfin\Server\plugins\configurations`

手动删除（如有自定义规则，请先备份相关规则！）目录下的 `Jellyfin.Plugin.AvMoo.xml` 和 `Jellyfin.Plugin.AvSox.xml` 两个文件，重新开启 jellyfin，检查刮削的数据是否正常。