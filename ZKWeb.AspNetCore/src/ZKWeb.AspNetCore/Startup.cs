﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.IO;
using System.Threading.Tasks;
using ZKWeb.AspNetCore.Hosting;
using ZKWebStandard.Ioc;

namespace ZKWeb.AspNetCore {
	/// <summary>
	/// Asp.Net Core配置类
	/// </summary>
	public class Startup {
		/// <summary>
		/// 获取网站根目录
		/// </summary>
		/// <returns></returns>
		public static string GetWebsiteRootDirectory() {
			var path = PlatformServices.Default.Application.ApplicationBasePath;
			while (!File.Exists(Path.Combine(path, "Web.config"))) {
				path = Path.GetDirectoryName(path);
				if (string.IsNullOrEmpty(path)) {
					throw new DirectoryNotFoundException("Website root directory not found");
				}
			}
			return path;
		}

		public virtual void Configure(IApplicationBuilder app, IApplicationLifetime lifetime) {
			// 初始化程序
			Application.Ioc.RegisterMany<CoreWebsiteStopper>(ReuseType.Singleton);
			Application.Initialize(GetWebsiteRootDirectory());
			Application.Ioc.RegisterInstance(lifetime);
			// 设置处理请求的函数
			// 处理会在线程池中运行
			app.Run(coreContext => Task.Run(() => {
				var context = new CoreHttpContextWrapper(coreContext);
				try {
					// 处理请求
					Application.OnRequest(context);
				} catch (CoreHttpResponseEndException) {
					// 正常处理完毕
				} catch (Exception ex) {
					// 处理错误
					try {
						Application.OnError(context, ex);
					} catch (CoreHttpResponseEndException) {
						// 错误处理完毕
					} catch (Exception) {
						// 错误处理失败
					}
				}
			}));
		}
	}
}
