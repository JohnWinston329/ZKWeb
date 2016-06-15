﻿using System;
using System.IO;
using ZKWeb.Server;
using ZKWeb.Tests.Server;
using ZKWebStandard.Extensions;
using ZKWeb.Web.ActionResults;
using ZKWebStandard.Testing;
using ZKWebStandard.Web.Mock;

namespace ZKWeb.Tests.Web.ActionResults {
	[Tests]
	class FileResultTest {
		public void WriteResponse() {
			using (var layout = new TestDirectoryLayout()) {
				layout.WriteAppDataFile("static/__test.txt", "test contents");
				var pathManager = Application.Ioc.Resolve<PathManager>();
				var resourcePath = pathManager.GetResourceFullPath("static/__test.txt");
				var lastModified = File.GetLastWriteTimeUtc(resourcePath).Truncate();
				Assert.Equals(File.ReadAllText(resourcePath), "test contents");

				var ifModifiedSinces = new DateTime?[] { null, DateTime.UtcNow.AddDays(1), lastModified };
				foreach (var ifModifiedSince in ifModifiedSinces) {
					var result = new FileResult(resourcePath, ifModifiedSince);
					var contextMock = new HttpContextMock();
					result.WriteResponse(contextMock.response);
					contextMock.response.body.Seek(0, SeekOrigin.Begin);
					if (ifModifiedSince == lastModified) {
						Assert.Equals(contextMock.response.StatusCode, 304);
						Assert.Equals(new StreamReader(contextMock.response.body).ReadToEnd(), "");
					} else {
						Assert.Equals(contextMock.response.StatusCode, 200);
						Assert.Equals(contextMock.response.ContentType, "text/plain");
						Assert.Equals(new StreamReader(contextMock.response.body).ReadToEnd(), "test contents");
					}
					contextMock.request.headers["If-Modified-Since"] = (
						contextMock.response.headers["Last-Modified"][0]);
					Assert.Equals(contextMock.request.GetIfModifiedSince(), lastModified);
				}
			}
		}
	}
}
