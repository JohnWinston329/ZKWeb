using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using ZKWeb.Core;
using ZKWeb.Core.Model;

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
	/// 
	/// namespace problem (keep App_Code?)
	/// plugin name (as folder name)
	/// plugin priority (use static list or?)
	/// 
	/// </summary>
	public class Application : HttpApplication {
		/// <summary>
		/// ���������
		/// </summary>
		public PluginManager PluginManager { get; protected set; }
		/// <summary>
		/// ���ù�����
		/// </summary>
		public ConfigManager ConfigManager { get; protected set; }

		/// <summary>
		/// ��վ����ʱ�Ĵ���
		/// </summary>
		public void Application_Start() {
			PluginManager = new PluginManager();
			ConfigManager = new ConfigManager();
			Reloader.Start(this);
		}

		/// <summary>
		/// �յ�Http����ʱ�Ĵ���
		/// </summary>
		protected void Application_BeginRequest(object sender, EventArgs e) {
			PluginManager.TriggerReversed<ControllerBase>(this);
		}

		/// <summary>
		/// ��������ʱ�Ĵ���
		/// </summary>
		protected void Application_Error(object sender, EventArgs e) {
			Response.Write(Server.GetLastError()?.ToString());
		}
	}
}
