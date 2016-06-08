﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using ZKWeb.Utils.Collections;
using ZKWeb.Utils.Extensions;
using ZKWeb.Utils.Functions;
using ZKWeb.Utils.UnitTest;
using ZKWeb.Web;
using ZKWeb.Web.ActionResults;
using ZKWeb.Web.Interfaces;

namespace ZKWeb.Tests.Web {
	[UnitTest]
	class ControllerManagerTest {
		public void OnRequest() {
			using (Application.OverrideIoc()) {
				Application.Ioc.Unregister<ControllerManager>();
				Application.Ioc.RegisterMany<ControllerManager>();
				var controllerManager = Application.Ioc.Resolve<ControllerManager>();
				controllerManager.RegisterController<TestController>();

				using (HttpContextUtils.OverrideContext("__test_action_a", HttpMethods.GET)) {
					var response = (HttpResponseMock)HttpContextUtils.CurrentContext.Response;
					response.outputStream = new MemoryStream();
					controllerManager.OnRequest();
					response.outputStream.Seek(0, SeekOrigin.Begin);
					Assert.Equals(response.ContentType, "text/plain");
					Assert.Equals(new StreamReader(response.outputStream).ReadToEnd(), "test action a");
				}

				using (HttpContextUtils.OverrideContext("__test_action_b", HttpMethods.POST)) {
					var response = (HttpResponseMock)HttpContextUtils.CurrentContext.Response;
					response.outputStream = new MemoryStream();
					controllerManager.OnRequest();
					response.outputStream.Seek(0, SeekOrigin.Begin);
					Assert.Equals(response.ContentType, "application/json");
					Assert.Equals(
						new StreamReader(response.outputStream).ReadToEnd(),
						JsonConvert.SerializeObject(new { a = 1 }));
				}
			}
		}

		public void RegisterController() {
			using (Application.OverrideIoc()) {
				Application.Ioc.Unregister<ControllerManager>();
				Application.Ioc.RegisterMany<ControllerManager>();
				var controllerManager = Application.Ioc.Resolve<ControllerManager>();
				controllerManager.RegisterController<TestController>();
				Assert.Equals(
					((PlainResult)controllerManager.GetAction("__test_action_a", HttpMethods.GET)()).Text,
					"test action a");
				Assert.Equals(
					((PlainResult)controllerManager.GetAction("__test_action_a", HttpMethods.POST)()).Text,
					"test action a");
				Assert.Equals(controllerManager.GetAction("__test_action_b", HttpMethods.GET), null);
				var obj = ((JsonResult)controllerManager
					.GetAction("__test_action_b", HttpMethods.POST)()).Object;
				Assert.Equals(JsonConvert.SerializeObject(obj), JsonConvert.SerializeObject(new { a = 1 }));
			}
		}

		public void NormalizePath() {
			var controllerManager = Application.Ioc.Resolve<ControllerManager>();
			Assert.Equals(controllerManager.NormalizePath("abc"), "/abc");
			Assert.Equals(controllerManager.NormalizePath("/abc/"), "/abc");
			Assert.Equals(controllerManager.NormalizePath("/"), "/");
		}

		public void RegisterAction() {
			using (Application.OverrideIoc()) {
				Application.Ioc.Unregister<ControllerManager>();
				Application.Ioc.RegisterMany<ControllerManager>();
				var controllerManager = Application.Ioc.Resolve<ControllerManager>();
				var actionGet = new Func<IActionResult>(() => { return new PlainResult("get"); });
				var actionPost = new Func<IActionResult>(() => { return new PlainResult("post"); });
				var actionOther = new Func<IActionResult>(() => { return new PlainResult("other"); });

				controllerManager.RegisterAction("__test_action", HttpMethods.GET, actionGet);
				controllerManager.RegisterAction("__test_action", HttpMethods.POST, actionPost);
				Assert.Throws<ArgumentException>(() =>
					controllerManager.RegisterAction("__test_action", HttpMethods.GET, actionOther));

				Assert.Equals(controllerManager.GetAction("__test_action", HttpMethods.GET), actionGet);
				Assert.Equals(controllerManager.GetAction("__test_action", HttpMethods.POST), actionPost);

				controllerManager.RegisterAction("__test_action", HttpMethods.GET, actionOther, true);
				Assert.Equals(controllerManager.GetAction("__test_action", HttpMethods.GET), actionOther);
				Assert.Equals(controllerManager.GetAction("__test_action", HttpMethods.POST), actionPost);
			}
		}

		public void UnregisterAction() {
			using (Application.OverrideIoc()) {
				Application.Ioc.Unregister<ControllerManager>();
				Application.Ioc.RegisterMany<ControllerManager>();
				var controllerManager = Application.Ioc.Resolve<ControllerManager>();
				var actionGet = new Func<IActionResult>(() => { return new PlainResult("get"); });

				controllerManager.RegisterAction("__test_action", HttpMethods.GET, actionGet);
				Assert.Equals(controllerManager.GetAction("__test_action", HttpMethods.GET), actionGet);
				Assert.IsTrue(controllerManager.UnregisterAction("__test_action", HttpMethods.GET));
				Assert.IsTrue(!controllerManager.UnregisterAction("__test_action", HttpMethods.GET));
				Assert.Equals(controllerManager.GetAction("__test_action", HttpMethods.GET), null);
			}
		}

		public void GetAction() {
			using (Application.OverrideIoc()) {
				Application.Ioc.Unregister<ControllerManager>();
				Application.Ioc.RegisterMany<ControllerManager>();
				var controllerManager = Application.Ioc.Resolve<ControllerManager>();
				var actionGet = new Func<IActionResult>(() => { return new PlainResult("get"); });
				var actionPost = new Func<IActionResult>(() => { return new PlainResult("post"); });

				controllerManager.RegisterAction("__test_action", HttpMethods.GET, actionGet);
				controllerManager.RegisterAction("__test_action", HttpMethods.POST, actionPost);

				Assert.Equals(controllerManager.GetAction("__test_action", HttpMethods.GET), actionGet);
				Assert.Equals(controllerManager.GetAction("__test_action", HttpMethods.POST), actionPost);
				Assert.Equals(controllerManager.GetAction("__not_exist", HttpMethods.GET), null);
			}
		}

		public class TestController {
			[Action("__test_action_a")]
			[Action("__test_action_a", HttpMethods.POST)]
			public string TestActionA() {
				return "test action a";
			}

			[Action("__test_action_b", HttpMethods.POST)]
			public IActionResult TestActionB() {
				return new JsonResult(new { a = 1 });
			}
		}
	}
}
