﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZKWeb.Utils.UnitTest {
	/// <summary>
	/// 抛出此例外时
	/// 会把当前测试作为通过处理
	/// </summary>
	public class AssertPassedException : Exception {
	}
}
