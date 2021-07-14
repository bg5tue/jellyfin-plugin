# Jellyfin 插件

## 介绍

用于刮削某些影片元数据的 Jellyfin 插件。

## 安装

### 通用方法

在 `控制台` -> `插件` -> `存储库` 中，添加 `https://gitee.com/bg5tue/jellyfin-plugin/raw/master/manifest.json` 库，在 `目录` 里安装。

### Windows 系统

下载 zip 包，解压到 `C:\ProgramData\Jellyfin\Server\plugins` 目录下，重启 Jellyfin 服务器。

### Linux 系统

待完善...

## 使用
库管理中启用元数据下载器 `AvMoo Movie Provider` 和 图片下载器 `AvMoo Image Provider` 后，再更新元数据。

## 更新记录

### v1.2.4 2021.6.27

* 修复 ExternalId 中的 UrlFormatString 没有 https 导致无法打开源影片页面的问题。
* 修复上一版本号错误问题。

### v1.1.3 2021.6.27

* 新增图片下载功能，具体在影片菜单“修改图片”中使用。
* 修复配置默认值中缩略图单图正则不正确，导致获取不到截图的问题。

### v1.1.2 2021.6.25

已实现基础数据刮削功能，可以正确的刮削文本数据。

未实现功能：

1. 刮削图片；
2. 系列功能。
