﻿namespace ZKWeb.Console {
	using DotLiquid;
	using DotLiquid.FileSystems;
	using DryIoc;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using NHibernate.Linq;
	using Newtonsoft.Json;
	using DryIocAttributes;
	using DryIoc.MefAttributedModel;
	using Utils.Functions;
	class Program {
		static void Main(string[] args) {
			new Application().Application_Start();
			TestUtils.RunAllTests();
			Console.WriteLine("done");
			Console.ReadLine();
		}
	}
}
