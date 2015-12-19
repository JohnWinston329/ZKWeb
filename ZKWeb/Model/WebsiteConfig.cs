﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ZKWeb.Model {
	/// <summary>
	/// 网站配置
	/// </summary>
	public class WebsiteConfig {
		/// <summary>
		/// 使用的数据库
		/// 可以指定这些值
		/// postgresql, sqlite, mysql, mssql
		/// </summary>
		public string Database { get; set; }
		/// <summary>
		/// 数据库的链接字符串
		/// </summary>
		public string ConnectionString { get; set; }
		/// <summary>
		/// 插件目录列表
		/// 必须是相对于网站程序的路径
		/// 如果没有指定则使用"App_Data/Plugins"
		/// </summary>
		public List<string> PluginDirectories { get; set; }
		/// <summary>
		/// 使用的插件列表
		/// </summary>
		public List<string> Plugins { get; set; }
		/// <summary>
		/// 其他附加配置
		/// </summary>
		public Dictionary<string, object> Extra { get; set; }

		/// <summary>
		/// 从文件读取网站配置
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static WebsiteConfig FromFile(string path) {
			var json = File.ReadAllText(path);
			var config = JsonConvert.DeserializeObject<WebsiteConfig>(json);
			config.PluginDirectories = config.PluginDirectories ?? new List<string>();
			if (!config.PluginDirectories.Any()) {
				config.PluginDirectories.Add("App_Data/Plugins");
			}
			config.Plugins = config.Plugins ?? new List<string>();
			config.Extra = config.Extra ?? new Dictionary<string, object>();
			return config;
		}
	}

	/// <summary>
	/// 数据库类型的字符串定义
	/// </summary>
	public static class DatabaseTypes {
		/// <summary>
		/// PostgreSQL
		/// </summary>
		public const string PostgreSQL = "postgresql";
		/// <summary>
		/// SQLite
		/// </summary>
		public const string SQLite = "sqlite";
		/// <summary>
		/// MySQL
		/// </summary>
		public const string MySQL = "mysql";
		/// <summary>
		/// MSSQL
		/// </summary>
		public const string MSSQL = "mssql";
	}
}
