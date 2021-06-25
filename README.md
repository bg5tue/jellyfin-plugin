# Jellyfin 插件

## 介绍

用于刮削某些影片元数据的 Jellyfin 插件。

## 更新记录

### 2021.6.25 v1.0

已实现基础数据刮削功能，可以正确的刮削文本数据。

未实现功能：

1. 刮削图片；
2. 系列功能。

## 使用方法

### 通用方法

在 `控制台` -> `插件` -> `存储库` 中，添加 `https://gitee.com/bg5tue/jellyfin-plugin/raw/master/manifest.json` 库，在 `目录` 里安装。

### Windows 系统

下载 zip 包，解压到 `C:\ProgramData\Jellyfin\Server\plugins` 目录下，重启 Jellyfin 服务器。

### Linux 系统

待完善...
