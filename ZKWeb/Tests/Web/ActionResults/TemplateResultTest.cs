﻿using System.IO;
using ZKWeb.Templating;
using ZKWeb.Tests.Server;
using ZKWeb.Web.ActionResults;
using ZKWebStandard.Ioc;
using ZKWebStandard.Testing;
using ZKWebStandard.Web.Mock;

namespace ZKWeb.Tests.Web.ActionResults {
	[Tests]
	class TemplateResultTest {
		public void WriteResponse() {
			using (var layout = new TestDirectoryLayout()) {
				Application.Ioc.Unregister<TemplateManager>();
				Application.Ioc.RegisterMany<TemplateManager>(ReuseType.Singleton);
				layout.WritePluginFile("PluginA", "templates/__test_a.html", "test a {{ name }}");
				layout.WritePluginFile("PluginB", "templates/__test_b.html", "test b {{ name }}");

				var result = new TemplateResult("__test_a.html", new { name = "asd" });
				var contextMock = new HttpContextMock();
				result.WriteResponse(contextMock.response);
				Assert.Equals(contextMock.response.StatusCode, 200);
				Assert.Equals(contextMock.response.ContentType, "text/html");
				contextMock.response.body.Seek(0, SeekOrigin.Begin);
				Assert.Equals(new StreamReader(contextMock.response.body).ReadToEnd(), "test a asd");

				result = new TemplateResult("__test_b.html", new { name = "asd" });
				contextMock = new HttpContextMock();
				result.WriteResponse(contextMock.response);
				Assert.Equals(contextMock.response.StatusCode, 200);
				Assert.Equals(contextMock.response.ContentType, "text/html");
				contextMock.response.body.Seek(0, SeekOrigin.Begin);
				Assert.Equals(new StreamReader(contextMock.response.body).ReadToEnd(), "test b asd");
			}
		}
	}
}
