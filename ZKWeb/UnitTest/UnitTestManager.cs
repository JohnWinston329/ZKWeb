﻿using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using ZKWeb.Database;
using ZKWeb.Plugin;
using ZKWeb.Utils.Collections;
using ZKWeb.Utils.UnitTest;
using ZKWeb.Utils.UnitTest.Event;

namespace ZKWeb.UnitTest {
	/// <summary>
	/// 单元测试管理器
	/// </summary>
	public class UnitTestManager {
		/// <summary>
		/// 获取单元测试使用的程序集列表
		/// </summary>
		/// <returns></returns>
		public virtual IList<Assembly> GetAssembliesForTest() {
			var result = new List<Assembly>();
			result.Add(typeof(UnitTestManager).Assembly); // ZKWeb
			result.Add(typeof(UnitTestRunner).Assembly); // ZKWeb.Utils
			var pluginManager = Application.Ioc.Resolve<PluginManager>();
			result.AddRange(pluginManager.PluginAssemblies); // 插件程序集列表}
			return result;
		}

		/// <summary>
		/// 运行指定程序集的测试
		/// 会阻塞到运行结束
		/// </summary>
		/// <param name="assembly">程序集</param>
		/// <param name="eventHandler">附加的事件处理器，可以等于null</param>
		public virtual void RunAssemblyTest(
			Assembly assembly, IUnitTestEventHandler eventHandler = null) {
			// 获取注册的事件处理器列表
			var eventHandlers = Application.Ioc.ResolveMany<IUnitTestEventHandler>().ToList();
			if (eventHandler != null) {
				eventHandlers.Add(eventHandler);
			}
			// 创建运行器并执行测试
			var runner = new UnitTestRunner(assembly, eventHandlers);
			runner.Run();
		}

		/// <summary>
		/// 运行所有程序集的测试
		/// 会阻塞到运行结束
		/// </summary>
		/// <param name="eventHandler">附加的事件处理器，可以等于null</param>
		public virtual void RunAllAssemblyTest(IUnitTestEventHandler eventHandler = null) {
			foreach (var assembly in GetAssembliesForTest()) {
				RunAssemblyTest(assembly, eventHandler);
			}
		}

		/// <summary>
		/// 在指定的范围内启用临时数据库
		/// </summary>
		/// <param name="dbPath">数据库文件的路径，不指定时使用临时文件</param>
		/// <returns></returns>
		public virtual IDisposable UseTemporaryDatabase(string dbPath = null) {
			// 创建数据库会话生成器
			dbPath = dbPath ?? Path.GetTempFileName();
			var connectionString = string.Format("Data Source={0};Version=3;", dbPath);
			var sessionFactory = DatabaseManager.BuildSessionFactory("sqlite", connectionString, null);
			// 重载当前的数据库管理器，使用刚才创建的数据库会话生成器
			var overrideIoc = Application.OverrideIoc();
			var databaseManagerMock = new Mock<DatabaseManager>() { CallBase = true };
			databaseManagerMock.Setup(d => d.SessionFactory).Returns(sessionFactory);
			Application.Ioc.Unregister<DatabaseManager>();
			Application.Ioc.RegisterInstance(databaseManagerMock.Object);
			// 区域结束后结束对容器的重载和删除数据库文件
			return new SimpleDisposable(() => { overrideIoc.Dispose(); File.Delete(dbPath); });
		}
	}
}
