﻿using System.Collections.Generic;

namespace ZKWeb.Plugin.CompilerServices {
	/// <summary>
	/// 编译选项
	/// </summary>
	public class CompilationOptions {
		/// <summary>
		/// 是否以除错模式编译
		/// 性能会降低但更容易调试，默认是false
		/// </summary>
		public bool Debug { get; set; }
		/// <summary>
		/// 是否生成pdb文件
		/// 不生成时将无法调试，默认是true
		/// </summary>
		public bool GeneratePdbFile { get; set; }
		/// <summary>
		/// 引用的程序集路径列表
		/// 默认会引用当前程序引用的程序集，这里是额外引用的列表
		/// </summary>
		public IList<string> ReferenceAssemblyPaths { get; set; }

		/// <summary>
		/// 初始化
		/// </summary>
		public CompilationOptions() {
			Debug = false;
			GeneratePdbFile = true;
			ReferenceAssemblyPaths = new List<string>();
		}
	}
}
