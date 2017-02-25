﻿using Newtonsoft.Json;
using System.IO;
using System.Linq;
using ZKWeb.Toolkits.WebsitePublisher.Model;
using ZKWeb.Toolkits.WebsitePublisher.Utils;

namespace ZKWeb.Toolkits.WebsitePublisher {
	/// <summary>
	/// Website publisher
	/// </summary>
	public class WebsitePublisher {
		/// <summary>
		/// Publish website parameters
		/// </summary>
		public PublishWebsiteParameters Parameters { get; protected set; }

		/// <summary>
		/// Initialize
		/// </summary>
		/// <param name="parameters">Publish website parameters</param>
		public WebsitePublisher(PublishWebsiteParameters parameters) {
			parameters.Check();
			Parameters = parameters;
		}

		/// <summary>
		/// Get directory of website root
		/// </summary>
		/// <returns></returns>
		protected virtual string GetWebRoot() {
			return Path.GetFullPath(Parameters.WebRoot);
		}

		/// <summary>
		/// Get path of `Web.config`
		/// </summary>
		/// <returns></returns>
		protected virtual string GetWebConfigPath() {
			var webRoot = GetWebRoot();
			var webConfigPath = Path.Combine(webRoot, "Web.config");
			if (!File.Exists(webConfigPath)) {
				Path.Combine(Parameters.WebRoot, "web.config"); // 照顾到大小写区分的文件系统
			}
			if (!File.Exists(webConfigPath)) {
				throw new FileNotFoundException("web.config not found");
			}
			return webConfigPath;
		}

		/// <summary>
		/// Get directory of bin
		/// Asp.Net:
		/// - Use WebRoot\bin
		/// Asp.Net Core:
		/// - Find directory contains "ZKWeb.dll", "release", "net461", but not contains "publish"
		/// - Publish with .Net Core is not support yet
		/// </summary>
		/// <param name="isCore">Is Asp.Net Core</param>
		/// <returns></returns>
		protected virtual string GetBinDirectory(out bool isCore) {
			var webRoot = GetWebRoot();
			var binDir = Path.Combine(webRoot, "bin");
			if (!File.Exists(Path.Combine(binDir, "ZKWeb.dll"))) {
				isCore = true;
				var dllPath = Directory.EnumerateFiles(binDir, "ZKWeb.dll", SearchOption.AllDirectories)
					.Where(p => {
						var relPath = p.Substring(webRoot.Length).ToLower();
						// TODO: support publish other configuration and framework
						return (relPath.Contains("release") &&
							relPath.Contains("net461") && !relPath.Contains("publish"));
					}).FirstOrDefault();
				if (dllPath == null) {
					throw new DirectoryNotFoundException(
						"bin directory not found, please compile the project " +
						"with release configuration first");
				}
				binDir = Path.GetDirectoryName(dllPath);
			} else {
				isCore = false;
			}
			return binDir;
		}

		/// <summary>
		/// Get path of `config.json`
		/// </summary>
		/// <returns></returns>
		protected virtual string GetConfigJsonPath() {
			var webRoot = GetWebRoot();
			return Path.Combine(webRoot, "App_Data", "config.json");
		}

		/// <summary>
		/// Get Asp.Net Core launcher path
		/// </summary>
		/// <param name="binDir">bin directory</param>
		/// <returns></returns>
		protected virtual string GetAspNetCoreLauncherPath(string binDir) {
			var exeName = Directory.EnumerateFiles(
				binDir, "*.exe", SearchOption.TopDirectoryOnly)
				.Select(path => Path.GetFileName(path))
				.Where(name => !name.Contains(".vshost.")).FirstOrDefault();
			if (string.IsNullOrEmpty(exeName)) {
				throw new FileNotFoundException("Asp.Net Core Launcher exe not found");
			}
			return "." + Path.DirectorySeparatorChar + exeName;
		}

		/// <summary>
		/// Find plugin path
		/// </summary>
		/// <param name="config">Website configuration</param>
		/// <param name="pluginName">Plugin name</param>
		/// <returns></returns>
		protected virtual string FindPluginDirectory(WebsiteConfig config, string pluginName) {
			var pluginDirectories = config.PluginDirectories
				.Select(d => Path.GetFullPath(Path.Combine(GetWebRoot(), d))).ToList();
			foreach (var dir in pluginDirectories) {
				var pluginDir = Path.Combine(dir, pluginName);
				if (Directory.Exists(pluginDir)) {
					return pluginDir;
				}
			}
			throw new DirectoryNotFoundException($"Plugin directory for {pluginName} not found");
		}

		/// <summary>
		/// Publish website
		/// </summary>
		public virtual void PublishWebsite() {
			// Get paths
			var webRoot = GetWebRoot();
			var webConfigPath = GetWebConfigPath();
			var isCore = false;
			var binDir = GetBinDirectory(out isCore);
			var configJsonPath = GetConfigJsonPath();
			var outputDir = Path.Combine(Parameters.OutputDirectory, Parameters.OutputName);
			// Copy website binaries
			if (!isCore) {
				// Asp.Net: copy files to output\bin, and copy Global.asax
				DirectoryUtils.CopyDirectory(
					binDir, Path.Combine(outputDir, "bin"), Parameters.IgnorePattern);
				File.Copy(webConfigPath, Path.Combine(outputDir, "web.config"), true);
				File.Copy(Path.Combine(webRoot, "Global.asax"),
					Path.Combine(outputDir, "Global.asax"), true);
			} else {
				// Asp.Net Core: copy files to output\, and replace launcher path in web.config
				DirectoryUtils.CopyDirectory(binDir, outputDir, Parameters.IgnorePattern);
				var webConfig = File.ReadAllText(webConfigPath);
				webConfig = webConfig.Replace("%LAUNCHER_PATH%", GetAspNetCoreLauncherPath(binDir));
				webConfig = webConfig.Replace("%LAUNCHER_ARGS%", "");
				File.WriteAllText(Path.Combine(outputDir, "web.config"), webConfig);
			}
			// Merge website configuration
			var outputConfigJsonPath = Path.Combine(outputDir, "App_Data", "config.json");
			var config = WebsiteConfig.Merge(configJsonPath, outputConfigJsonPath);
			config.PluginDirectories = new[] { "App_Data/Plugins" };
			Directory.CreateDirectory(Path.GetDirectoryName(outputConfigJsonPath));
			File.WriteAllText(outputConfigJsonPath,
				JsonConvert.SerializeObject(config, Formatting.Indented));
			// Copy plugins
			var originalConfig = WebsiteConfig.FromFile(configJsonPath);
			var outputPluginRoot = Path.Combine(outputDir, config.PluginDirectories[0]);
			foreach (var pluginName in config.Plugins) {
				var pluginDir = FindPluginDirectory(originalConfig, pluginName);
				var outputPluginDir = Path.Combine(outputPluginRoot, pluginName);
				DirectoryUtils.CopyDirectory(pluginDir, outputPluginDir, Parameters.IgnorePattern);
			}
			// Remove src directory under plugins
			foreach (var dir in Directory.EnumerateDirectories(
				outputPluginRoot, "src", SearchOption.AllDirectories)) {
				Directory.Delete(dir, true);
			}
		}
	}
}
