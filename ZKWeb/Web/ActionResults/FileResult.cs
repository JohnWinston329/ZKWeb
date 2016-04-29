﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using ZKWeb.Utils.Extensions;
using ZKWeb.Web.Interfaces;

namespace ZKWeb.Web.ActionResults {
	/// <summary>
	/// 文件结果
	/// </summary>
	public class FileResult : IActionResult {
		/// <summary>
		/// 文件路径
		/// </summary>
		public string FilePath { get; set; }
		/// <summary>
		/// 客户端传入的文件修改时间
		/// </summary>
		public DateTime? IfModifiedSince { get; set; }

		/// <summary>
		/// 初始化
		/// </summary>
		/// <param name="path">文件路径</param>
		/// <param name="ifModifiedSince">客户端传入的文件修改时间</param>
		public FileResult(string path, DateTime? ifModifiedSince = null) {
			FilePath = path;
			IfModifiedSince = ifModifiedSince;
		}

		/// <summary>
		/// 写入文件到http回应中
		/// </summary>
		/// <param name="response">http回应</param>
		public void WriteResponse(HttpResponseBase response) {
			// 设置文件的最后修改时间
			var lastModified = File.GetLastWriteTimeUtc(FilePath).Truncate();
			response.Cache.SetLastModified(lastModified);
			// 文件没有修改时返回304
			if (IfModifiedSince != null && IfModifiedSince == lastModified) {
				response.StatusCode = 304;
				response.SuppressContent = true;
				return;
			}
			// 写入文件到http回应中
			response.ContentType = MimeMapping.GetMimeMapping(FilePath);
			response.WriteFile(FilePath);
		}
	}
}
