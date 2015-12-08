using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using ZKWeb.Core;
using ZKWeb.Core.Model;
using DryIoc;
using ZKWeb.Manager;
using ZKWeb.Model;
using ZKWeb.Utils.Extensions;
using ZKWeb.Properties;

namespace ZKWeb {
	/// <summary>
	/// ��վ����
	/// ���ڳ�ʼ����վ�ͱ���ȫ������
	/// 
	/// TODO:
	/// log monitor
	/// database migrate
	/// plugin break point test
	/// template engine
	/// session, cookies, csrf manager
	/// </summary>
	public class Application : HttpApplication {
		/// <summary>
		/// Ioc����
		/// </summary>
		public static Container Ioc { get; set; } = new Container();
		
		/// <summary>
		/// ��վ����ʱ�Ĵ���
		/// </summary>
		public void Application_Start() {
			Ioc.RegisterMany<ConfigManager>(Reuse.Singleton);
			Ioc.RegisterMany<LogManager>(Reuse.Singleton);
			Ioc.RegisterMany<PluginManager>(Reuse.Singleton);
			Ioc.Resolve<PluginManager>();
			Reloader.Start();
		}

		/// <summary>
		/// �յ�Http����ʱ�Ĵ���
		/// </summary>
		protected void Application_BeginRequest(object sender, EventArgs e) {
			Ioc.ResolveMany<IApplicationRequestHandler>().ForEach(h => h.OnRequest());
			throw new HttpException(404, "404 Not Found");
		}

		/// <summary>
		/// ��������ʱ�Ĵ���
		/// ע���������ִ��ʱʹ�õ�Application���ܺͳ�ʼ���Ĳ�һ��
		/// ��ȡIoc��ԱʱӦ��ͨ��Current.Ioc
		/// </summary>
		protected void Application_Error(object sender, EventArgs e) {
			// ��ȡ����������׳�������
			// ����Application_Start�׳����������Responseʱ�������ʱ��Ҫ��¼��������־�в��ȴ�����
			var ex = Server.GetLastError();
			if (ex is HttpUnhandledException && ex.InnerException != null) {
				ex = ex.InnerException;
			}
			Server.ClearError();
			try {
				Response.Clear();
			} catch {
				new LogManager().LogError(ex.ToString());
				HttpRuntime.UnloadAppDomain();
				return;
			}
			// ��¼����־
			// ����¼404���Ҳ�������403��Ȩ�޲��㣩����
			var logManager = Ioc.Resolve<LogManager>();
			var httpException = ex as HttpException;
			if (!(httpException?.GetHttpCode() == 404 ||
				httpException?.GetHttpCode() == 403)) {
				logManager.LogError(ex.ToString());
			}
			// ���ûص����������Ϣ
			// ��ص����ض�����������Ĵ������׳�ThreadAbortException
			var handlers = Ioc.ResolveMany<IApplicationErrorHandler>();
			handlers.ForEach(h => h.OnError(ex));
			// �ص�û�н��������ǣ���ʾĬ�ϵ���Ϣ
			// �����ǳ��������������Դ�Ǳ���ʱ��ʾ�������Ϣ
			// ��IE��ʾ�Զ��������Ҫ���㹻�ĳ��ȣ�����ֻ���ں������հ�����
			bool isAjaxRequest = Request.IsAjaxRequest();
			if (httpException?.GetHttpCode() == 404) {
				Response.StatusCode = 404;
				Response.ContentType = "text/html";
				Response.Write(Resources._404NotFound);
				if (!isAjaxRequest) {
					Response.Write(Resources.HistoryBackScript);
				}
				Response.Write(string.Concat(Enumerable.Repeat("<div></div>", 100)));
			} else if (httpException?.GetHttpCode() == 403) {
				Response.StatusCode = 403;
				Response.ContentType = "text/html";
				Response.Write(httpException.Message);
				if (!isAjaxRequest) {
					Response.Write(Resources.HistoryBackScript);
				}
				Response.Write(string.Concat(Enumerable.Repeat("<div></div>", 100)));
			} else {
				Response.StatusCode = 500;
				Response.ContentType = "text/plain";
				Response.Write(Resources._500ServerInternalError);
				if (Request.IsLocal) {
					Response.Write($"\r\n{Resources.DisplayApplicationErrorDetails}\r\n\r\n");
					Response.Write(ex.ToString());
				} else {
					Response.Write(string.Concat(Enumerable.Repeat("\r\n", 100)));
				}
			}
			Response.End();
		}
	}
}
