﻿using System;
using System.IO;
using ZKWebStandard.Extensions;
using ZKWebStandard.Web;
using ZKWebStandard.Utils;

namespace ZKWeb.Web.ActionResults {
	/// <summary>
	/// File result
	/// </summary>
	public class FileResult : IActionResult {
		/// <summary>
		/// File path
		/// </summary>
		public string FilePath { get; set; }
		/// <summary>
		/// Cached modify time received from client
		/// </summary>
		public DateTime? IfModifiedSince { get; set; }

		/// <summary>
		/// Initialize
		/// </summary>
		/// <param name="path">File path</param>
		/// <param name="ifModifiedSince">Cached modify time received from client</param>
		public FileResult(string path, DateTime? ifModifiedSince = null) {
			FilePath = path;
			IfModifiedSince = ifModifiedSince;
		}

		/// <summary>
		/// Write file to http response
		/// If file not modified, return 304
		/// </summary>
		/// <param name="response">Http Reponse</param>
		public void WriteResponse(IHttpResponse response) {
			// Set last modified time
			var lastModified = File.GetLastWriteTimeUtc(FilePath).Truncate();
			response.SetLastModified(lastModified);
			// Set mime
			response.ContentType = MimeUtils.GetMimeType(FilePath);
			// If file not modified, return 304
			if (IfModifiedSince != null && IfModifiedSince == lastModified) {
				response.StatusCode = 304;
				return;
			}
			// Write file to http response
			// TODO: support range request
			// http://dotnetslackers.com/articles/aspnet/Range-Specific-Requests-in-ASP-NET.aspx
			// http://www.freesoft.org/CIE/RFC/2068/178.htm
			response.StatusCode = 200;
			response.WriteFile(FilePath);
		}
	}
}
