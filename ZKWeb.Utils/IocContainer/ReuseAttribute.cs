﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZKWeb.Utils.IocContainer {
	/// <summary>
	/// 设置重用策略的属性
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
	public class ReuseAttribute : Attribute {
		/// <summary>
		/// 重用策略
		/// </summary>
		public ReuseType ReuseType { get; set; }

		/// <summary>
		/// 初始化
		/// </summary>
		/// <param name="reuseType">重用策略</param>
		public ReuseAttribute(ReuseType reuseType) {
			ReuseType = reuseType;
		}
	}
}
