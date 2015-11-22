using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using ZKWeb.Core.Plugin;
using ZKWeb.Core.Web;

namespace ZKWeb {
	/// <summary>
	/// ��վ����ڵ�
	/// TODO:
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
		protected PluginManager pluginManager { get; set; }

		/// <summary>
		/// ��վ����ʱ�Ĵ���
		/// ��ʼ�����������
		/// </summary>
		public void Application_Start() {
			pluginManager = new PluginManager();
		}

		/// <summary>
		/// �յ�Http����ʱ�Ĵ���
		/// �ò������������
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Application_BeginRequest(object sender, EventArgs e) {
			pluginManager.Trigger<ControllerBase>(this);
		}

		/// <summary>
		/// ��������ʱ�Ĵ���
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Application_Error(object sender, EventArgs e) {
			Response.Write(Server.GetLastError()?.ToString());
		}
	}
}
