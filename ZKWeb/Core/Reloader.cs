﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using ZKWeb.Model;
using ZKWeb.Utils.Functions;

namespace ZKWeb.Core {
	/// <summary>
	/// 自动重新载入插件和网站配置
	/// 检测网站目录的以下文件是否有改变，有改变时卸载当前程序域来让下次打开网站时重新载入
	///		App_Plugins/*.cs
	///		App_Plugins/*.json
	///		App_Data/*.json (仅根目录)
	/// </summary>
	public static class Reloader {
		/// <summary>
		/// 启用重载器
		/// </summary>
		public static void Start() {
			// 指定文件改变时卸载程序域
			Action<string> onFileChanged = (path) => {
				var ext = Path.GetExtension(path).ToLower();
				if (ext == ".cs" || ext == ".json") {
					HttpRuntime.UnloadAppDomain();
				}
			};
			// 绑定文件监视器的事件处理函数并启用监视器
			Action<FileSystemWatcher> startWatcher = (watcher) => {
				watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
				watcher.Changed += (sender, e) => onFileChanged(e.FullPath);
				watcher.Created += (sender, e) => onFileChanged(e.FullPath);
				watcher.Deleted += (sender, e) => onFileChanged(e.FullPath);
				watcher.Renamed += (sender, e) => { onFileChanged(e.FullPath); onFileChanged(e.OldFullPath); };
				watcher.EnableRaisingEvents = true;
			};
			// 监视插件目录
			var pluginFilesWatcher = new FileSystemWatcher();
			pluginFilesWatcher.Path = PathConfig.PluginsRootDirectory;
			pluginFilesWatcher.IncludeSubdirectories = true;
			startWatcher(pluginFilesWatcher);
			// 监视网站配置文件
			var websiteConfigWatcher = new FileSystemWatcher();
			websiteConfigWatcher.Path = PathConfig.AppDataDirectory;
			websiteConfigWatcher.Filter = "*.json";
			startWatcher(websiteConfigWatcher);
		}
	}
}
