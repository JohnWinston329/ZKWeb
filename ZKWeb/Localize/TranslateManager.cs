﻿using DryIoc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using ZKWeb.Cache.Interfaces;
using ZKWeb.Localize.Interfaces;
using ZKWeb.Server;
using ZKWeb.Utils.Collections;
using ZKWeb.Utils.Extensions;

namespace ZKWeb.Localize {
	/// <summary>
	/// 翻译管理器
	/// </summary>
	public class TranslateManager : ICacheCleaner {
		/// <summary>
		/// 翻译的缓存时间
		/// 默认是3秒，可通过网站配置指定
		/// </summary>
		public TimeSpan TranslateCacheTime { get; set; }
		/// <summary>
		/// 翻译缓存
		/// { 语言: { 文本: 翻译, ... } }
		/// </summary>
		protected ConcurrentDictionary<string, MemoryCache<string, string>> TranslateCache { get; set; }
		/// <summary>
		/// 翻译提供器的缓存
		/// { 语言: 提供器列表, ... }
		/// </summary>
		protected MemoryCache<string, List<ITranslateProvider>> TranslateProvidersCache { get; set; }

		/// <summary>
		/// 初始化
		/// </summary>
		public TranslateManager() {
			var configManager = Application.Ioc.Resolve<ConfigManager>();
			TranslateCacheTime = TimeSpan.FromSeconds(
				configManager.WebsiteConfig.Extra.GetOrDefault(ExtraConfigKeys.TranslateCacheTime, 3));
			TranslateCache = new ConcurrentDictionary<string, MemoryCache<string, string>>();
			TranslateProvidersCache = new MemoryCache<string, List<ITranslateProvider>>();
		}

		/// <summary>
		/// 按当前语言翻译文本
		/// </summary>
		/// <param name="text">文本</param>
		/// <returns></returns>
		public virtual string Translate(string text) {
			var cluture = Thread.CurrentThread.CurrentCulture;
			return Translate(text, cluture.Name);
		}

		/// <summary>
		/// 按指定的语言翻译文本
		/// </summary>
		/// <param name="text">文本</param>
		/// <param name="code">语言代码，格式是{语言}-{地区}</param>
		/// <returns></returns>
		public virtual string Translate(string text, string code) {
			// 文本是空白时不需要翻译
			if (string.IsNullOrEmpty(text)) {
				return "";
			}
			// 从缓存获取
			var localizeCache = TranslateCache.GetOrAdd(code, _ => new MemoryCache<string, string>());
			var translated = localizeCache.GetOrDefault(text);
			if (translated != null) {
				return translated;
			}
			// 获取翻译提供器列表
			var providers = GetTranslateProviders(code);
			// 翻译文本并保存到缓存
			// 没有找到翻译时使用原文本
			foreach (var provider in providers) {
				translated = provider.Translate(text);
				if (translated != null) {
					break;
				}
			}
			translated = translated ?? text;
			localizeCache.Put(text, translated, TranslateCacheTime);
			return translated;
		}

		/// <summary>
		/// 获取指定语言的翻译提供器列表
		/// </summary>
		/// <param name="code">格式是{语言}-{地区}</param>
		/// <returns></returns>
		public virtual List<ITranslateProvider> GetTranslateProviders(string code) {
			// 从缓存获取
			var provides = TranslateProvidersCache.GetOrDefault(code);
			if (provides != null) {
				return provides;
			}
			// 从容器获取并保存到缓存
			provides = Application.Ioc.ResolveMany<ITranslateProvider>()
				.Where(p => p.CanTranslate(code))
				.Reverse().ToList();
			TranslateProvidersCache.Put(code, provides, TranslateCacheTime);
			return provides;
		}

		/// <summary>
		/// 清理缓存
		/// </summary>
		public virtual void ClearCache() {
			TranslateCache.Clear();
		}
	}
}
