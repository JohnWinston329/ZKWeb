﻿namespace ZKWeb.Cache {
	/// <summary>
	/// 缓存清理器的接口
	/// 这个回调用于清理网站的页面和数据缓存
	/// </summary>
	public interface ICacheCleaner {
		/// <summary>
		/// 清理缓存
		/// </summary>
		void ClearCache();
	}
}
