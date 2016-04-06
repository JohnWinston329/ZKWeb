﻿using DotLiquid;
using DotLiquid.Tags;
using DryIoc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using ZKWeb.Plugin.Interfaces;
using ZKWeb.Server;
using ZKWeb.Utils.Collections;
using ZKWeb.Utils.Extensions;

namespace ZKWeb.Templating.AreaSupport {
	/// <summary>
	/// 提供区域(Area)和模块(Widget)的管理
	/// </summary>
	public class TemplateAreaManager : ICacheCleaner {
		/// <summary>
		/// 模块信息的缓存时间
		/// 缓存用于减少硬盘查询次数，但时间不能超过1秒否则影响修改
		/// </summary>
		public TimeSpan WidgetInfoCacheTime { get; set; } = TimeSpan.FromSeconds(1);
		/// <summary>
		/// 自定义模块列表的缓存时间
		/// 缓存用于减少硬盘查询次数，但时间不能超过1秒否则影响修改
		/// </summary>
		public TimeSpan CustomWidgetsCacheTime { get; set; } = TimeSpan.FromSeconds(1);
		/// <summary>
		/// 已知的区域列表
		/// </summary>
		private Dictionary<string, TemplateArea> Areas =
			new Dictionary<string, TemplateArea>();
		/// <summary>
		/// 模块描画结果的缓存
		/// { 模块名称和参数: 描画结果, ... }
		/// </summary>
		private MemoryCache<string, string> WidgetRenderCache =
			new MemoryCache<string, string>();
		/// <summary>
		/// 模块信息的缓存
		/// { 模块名称: 模块信息, ... }
		/// </summary>
		private MemoryCache<string, TemplateWidgetInfo> WidgetInfoCache =
			new MemoryCache<string, TemplateWidgetInfo>();
		/// <summary>
		/// 自定义模块列表的缓存
		/// { 区域Id: 自定义模块列表, ... }
		/// </summary>
		private MemoryCache<string, List<TemplateWidget>> CustomWidgetsCache =
			new MemoryCache<string, List<TemplateWidget>>();

		/// <summary>
		/// 获取区域
		/// 没有时新建指定Id的区域并返回
		/// </summary>
		/// <param name="areaId">区域Id</param>
		/// <returns></returns>
		public virtual TemplateArea GetArea(string areaId) {
			return Areas.GetOrCreate(areaId, () => new TemplateArea(areaId));
		}

		/// <summary>
		/// 获取模块信息
		/// </summary>
		/// <param name="widgetPath">模块路径</param>
		/// <returns></returns>
		public virtual TemplateWidgetInfo GetWidgetInfo(string widgetPath) {
			var info = WidgetInfoCache.GetOrDefault(widgetPath);
			if (info == null) {
				info = TemplateWidgetInfo.FromPath(widgetPath);
				WidgetInfoCache.Put(widgetPath, info, WidgetInfoCacheTime);
			}
			return info;
		}

		/// <summary>
		/// 获取保存区域的自定义模块列表的Json绝对路径
		/// 路径 App_Data/areas/{areaId}.widgets
		/// </summary>
		/// <param name="areaId">区域Id</param>
		/// <returns></returns>
		protected virtual string GetCustomWidgetsJsonPath(string areaId) {
			var pathManager = Application.Ioc.Resolve<PathManager>();
			var path = pathManager.GetStorageFullPath("areas", string.Format("{0}.widgets", areaId));
			return path;
		}

		/// <summary>
		/// 获取区域的自定义模块列表
		/// 没有时返回null
		/// </summary>
		/// <param name="areaId">区域Id</param>
		/// <returns></returns>
		public virtual List<TemplateWidget> GetCustomWidgets(string areaId) {
			// 从缓存获取
			var widgets = CustomWidgetsCache.GetOrDefault(areaId);
			if (widgets != null) {
				return widgets;
			}
			// 从文件获取
			var path = GetCustomWidgetsJsonPath(areaId);
			if (File.Exists(path)) {
				// 获取成功时保存到缓存中
				widgets = JsonConvert.DeserializeObject<List<TemplateWidget>>(File.ReadAllText(path));
				CustomWidgetsCache.Put(areaId, widgets, CustomWidgetsCacheTime);
			}
			return widgets;
		}

		/// <summary>
		/// 设置区域的自定义模块列表
		/// 列表等于null时删除
		/// </summary>
		/// <param name="areaId">区域Id</param>
		/// <param name="widgets">自定义模块列表，等于null时删除</param>
		public virtual void SetCustomWidgets(string areaId, List<TemplateWidget> widgets) {
			// 删除缓存
			CustomWidgetsCache.Remove(areaId);
			// 保存到文件
			var path = GetCustomWidgetsJsonPath(areaId);
			if (widgets != null) {
				File.WriteAllText(path, JsonConvert.SerializeObject(widgets, Formatting.Indented));
			} else {
				File.Delete(path);
			}
		}

		/// <summary>
		/// 描画模块
		/// 返回描画结果
		/// </summary>
		/// <param name="context">模板上下文</param>
		/// <param name="widget">模块</param>
		/// <returns></returns>
		public virtual string RenderWidget(Context context, TemplateWidget widget) {
			// 从缓存获取
			var key = JsonConvert.SerializeObject(widget);
			var renderResult = WidgetRenderCache.GetOrDefault(key);
			if (renderResult != null) {
				return renderResult;
			}
			// 描画模块
			var writer = new StringWriter();
			writer.Write($"<div class='template_widget' data-widget='{key}'>");
			var scope = Hash.FromAnonymousObject(widget.Args);
			context.Stack(scope, () => {
				var includeTag = new Include();
				var htmlPath = widget.Path + TemplateWidgetInfo.HtmlExtension;
				includeTag.Initialize("include", htmlPath, null);
				includeTag.Render(context, writer);
			});
			writer.Write("</div>");
			renderResult = writer.ToString();
			// 保存描画结果到缓存中
			var info = GetWidgetInfo(widget.Path);
			if (info.CacheTime > 0) {
				WidgetRenderCache.Put(key, renderResult, TimeSpan.FromSeconds(info.CacheTime));
			}
			return renderResult;
		}

		/// <summary>
		/// 清理缓存
		/// </summary>
		public void ClearCache() {
			WidgetInfoCache.Clear();
			WidgetRenderCache.Clear();
			CustomWidgetsCache.Clear();
		}
	}
}
