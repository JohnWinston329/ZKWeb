﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ZKWeb.Model {
	/// <summary>
	/// 插件信息
	/// </summary>
	public class PluginInfo {
		/// <summary>
		/// 插件所在目录
		/// </summary>
		public string Directory { get; set; }
		/// <summary>
		/// 插件名称
		/// </summary>
		public Dictionary<string, string> Name { get; set; }
		/// <summary>
		/// 插件描述
		/// </summary>
		public Dictionary<string, string> Description { get; set; }
		/// <summary>
		/// 依赖的其他插件
		/// </summary>
		public List<string> Dependencies { get; set; }

		/// <summary>
		/// 从插件目录生成插件信息
		/// </summary>
		/// <param name="dir"></param>
		/// <returns></returns>
		public static PluginInfo FromDirectory(string dir) {
			var json = File.ReadAllText(Path.Combine(dir, "plugin.json"));
			var info = JsonConvert.DeserializeObject<PluginInfo>(json);
			info.Directory = dir;
			info.Name = info.Name ?? new Dictionary<string, string>();
			info.Description = info.Description ?? new Dictionary<string, string>();
			info.Dependencies = info.Dependencies ?? new List<string>();
			return info;
		}
	}

	/// <summary>
	/// 插件信息的扩展函数
	/// </summary>
	public static class PluginInfoExtensions {
		
	}
}
