﻿using System;
using System.Threading;
using ZKWebStandard.Collections;
using ZKWebStandard.Web.Mock;

namespace ZKWebStandard.Web {
	/// <summary>
	/// Http manager
	/// </summary>
	public static class HttpManager {
		/// <summary>
		/// Get using http context
		/// Throw exception if not exist
		/// </summary>
		public static IHttpContext CurrentContext {
			get {
				var context = currentContext.Value;
				if (context == null) {
					throw new NullReferenceException("Context does not exists");
				}
				return context;
			}
		}
		private static ThreadLocal<IHttpContext> currentContext = new ThreadLocal<IHttpContext>();
		/// <summary>
		/// Determines if there a http context is using
		/// </summary>
		public static bool CurrentContextExists { get { return currentContext.Value != null; } }

		/// <summary>
		/// Override using http context
		/// Restore to previous context after disposed
		/// </summary>
		/// <param name="context">Http context</param>
		/// <returns></returns>
		public static IDisposable OverrideContext(IHttpContext context) {
			var original = currentContext.Value;
			currentContext.Value = context;
			return new SimpleDisposable(() => {
				// check again to avoid gc dispose
				if (currentContext.Value == context) {
					currentContext.Value = original;
				}
			});
		}

		/// <summary>
		/// Override using http context
		/// Restore to previous context after disposed
		/// </summary>
		/// <param name="pathAndQuery">Path and query, "/" will be automatic added to front if needed</param>
		/// <param name="method">Method, GET or POST</param>
		/// <returns></returns>
		public static IDisposable OverrideContext(string pathAndQuery, string method) {
			return OverrideContext(new HttpContextMock(pathAndQuery, method));
		}
	}
}
