﻿using System;
using System.IO;
using System.Linq;
using ZKWeb.Server;
using ZKWeb.Web;
using ZKWebStandard.Extensions;

namespace ZKWeb.Plugin {
	/// <summary>
	/// Automatic reload plugins and website configuration
	/// It will determine the following files are changed then reload the website after
	/// - {Plugin directory}/*.cs
	/// - {Plugin directory}/*.json
	/// - {Plugin directory}/*.dll
	/// - App_Data/*.json (No recursion)
	/// - App_Data/*.ddl (No recursion)
	/// </summary>
	internal static class PluginReloader {
		/// <summary>
		/// Start reloader
		/// </summary>
		internal static void Start() {
			// Function use to stop website
			Action stopWebsite = () => {
				var stoppers = Application.Ioc.ResolveMany<IWebsiteStopper>();
				stoppers.ForEach(s => s.StopWebsite());
			};
			// Function use to handle file changed
			Action<string> onFileChanged = (path) => {
				var ext = Path.GetExtension(path).ToLower();
				if (ext == ".cs" || ext == ".json" || ext == ".dll") {
					stopWebsite();
				}
			};
			// Function use to start file system watcher
			Action<FileSystemWatcher> startWatcher = (watcher) => {
				watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
				watcher.Changed += (sender, e) => onFileChanged(e.FullPath);
				watcher.Created += (sender, e) => onFileChanged(e.FullPath);
				watcher.Deleted += (sender, e) => onFileChanged(e.FullPath);
				watcher.Renamed += (sender, e) => { onFileChanged(e.FullPath); onFileChanged(e.OldFullPath); };
				watcher.EnableRaisingEvents = true;
			};
			// Monitor plugin directory
			var pathManager = Application.Ioc.Resolve<PathManager>();
			pathManager.GetPluginDirectories().Where(p => Directory.Exists(p)).ForEach(p => {
				var pluginFilesWatcher = new FileSystemWatcher();
				pluginFilesWatcher.Path = p;
				pluginFilesWatcher.IncludeSubdirectories = true;
				startWatcher(pluginFilesWatcher);
			});
			// Monitor App_Data directory
			var pathConfig = Application.Ioc.Resolve<PathConfig>();
			var websiteConfigWatcher = new FileSystemWatcher();
			websiteConfigWatcher.Path = pathConfig.AppDataDirectory;
			websiteConfigWatcher.Filter = "*.json";
			startWatcher(websiteConfigWatcher);
			// Monitor ddl script files, only trigger when deleted
			var ddlWatcher = new FileSystemWatcher();
			ddlWatcher.Path = pathConfig.AppDataDirectory;
			ddlWatcher.Filter = "*.ddl";
			ddlWatcher.Deleted += (sender, e) => stopWebsite();
			ddlWatcher.EnableRaisingEvents = true;
		}
	}
}
