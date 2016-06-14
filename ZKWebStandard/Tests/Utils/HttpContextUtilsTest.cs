﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZKWebStandard.Collections;
using ZKWebStandard.Utils;
using ZKWebStandard.Testing;
/* TODO: FIXME
namespace ZKWebStandard.Tests.Functions {
	[Tests]
	class HttpContextUtilsTest {
		public void PutData() {
			var list = new string[] { "a", "b", "c" };
			HttpContextUtils.PutData("TestPutData", list);
			Assert.Equals(HttpContextUtils.GetData<string[]>("TestPutData"), list);
		}

		public void GetData() {
			var list = new string[] { "a", "b", "c" };
			HttpContextUtils.PutData("TestGetData", list);
			Assert.Equals(HttpContextUtils.GetData<string[]>("TestGetData"), list);
		}

		public void GetOrCreateData() {
			HttpContextUtils.PutData("TestGetData", "abc");
			Assert.Equals(HttpContextUtils.GetOrCreateData("TestGetData", () => "def"), "abc");
			Assert.Equals(HttpContextUtils.GetOrCreateData("TestCreateData", () => "def"), "def");
		}

		public void RemoveData() {
			HttpContextUtils.PutData("TestRemoveData", "abc");
			HttpContextUtils.RemoveData("TestRemoveData");
			Assert.Equals(HttpContextUtils.GetData<string>("TestRemoveData"), null);
		}

		public void GetCookie() {
			HttpContextUtils.PutCookie("TestGetCookie", "abc");
			Assert.Equals(HttpContextUtils.GetCookie("TestGetCookie"), "abc");
		}

		public void PutCookie() {
			HttpContextUtils.PutCookie("TestPutCookie", "abc");
			Assert.Equals(HttpContextUtils.GetCookie("TestPutCookie"), "abc");
		}

		public void RemoveCookie() {
			HttpContextUtils.PutCookie("TestRemoveCookie", "abc");
			HttpContextUtils.RemoveCookie("TestRemoveCookie");
			Assert.Equals(HttpContextUtils.GetCookie("TestRemoveCookie"), null);
		}

		public void GetClientIpAddress() {
			Assert.Equals(HttpContextUtils.GetClientIpAddress(), "::1");
			using (HttpManager.OverrideContext("", "GET")) {
				var request = (HttpRequestMock)HttpContextUtils.CurrentContext.Request;
				request.userHostAddress = "192.168.168.168";
				Assert.Equals(HttpContextUtils.GetClientIpAddress(), "192.168.168.168");
			}
		}

		public void GetRequestHostUrl() {
			Assert.Equals(HttpContextUtils.GetRequestHostUrl(), "http://localhost");
			using (HttpManager.OverrideContext(new Uri("http://www.abc.com/admin"), "GET")) {
				Assert.Equals(HttpContextUtils.GetRequestHostUrl(), "http://www.abc.com");
			}
		}

		public void UseContext() {
			Assert.Equals(HttpContextUtils.CurrentContext, null);
			using (HttpManager.OverrideContext("a", "GET")) {
				using (HttpManager.OverrideContext("b", "GET")) {
					Assert.Equals(HttpContextUtils.CurrentContext.Request.Url.AbsolutePath, "/b");
				}
				Assert.Equals(HttpContextUtils.CurrentContext.Request.Url.AbsolutePath, "/a");
			}
			Assert.Equals(HttpContextUtils.CurrentContext, null);
		}
	}
}
*/