﻿using System;
using System.Collections.Generic;

namespace ZKWebStandard.Extensions {
	/// <summary>
	/// IDictionary类型的扩展函数
	/// </summary>
	public static class IDictionaryExtensions {
		/// <summary>
		/// 尝试获取值，值不存在时返回默认值
		/// </summary>
		/// <typeparam name="TKey">键类型</typeparam>
		/// <typeparam name="TValue">值类型</typeparam>
		/// <param name="dict">词典</param>
		/// <param name="key">键名，等于null时直接返回默认值</param>
		/// <param name="defaultValue">默认值</param>
		/// <returns></returns>
		public static TValue GetOrDefault<TKey, TValue>(
			this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default(TValue)) {
			TValue value;
			if (key != null && dict.TryGetValue(key, out value)) {
				return value;
			}
			return defaultValue;
		}

		/// <summary>
		/// 尝试获取值，值不存在时创建并返回默认值
		/// </summary>
		/// <typeparam name="TKey">键类型</typeparam>
		/// <typeparam name="TValue">值类型</typeparam>
		/// <param name="dict">词典</param>
		/// <param name="key">键名</param>
		/// <param name="defaultValue">默认值</param>
		/// <returns></returns>
		public static TValue GetOrCreate<TKey, TValue>(
			this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> defaultValue) {
			TValue value;
			if (dict.TryGetValue(key, out value)) {
				return value;
			}
			dict[key] = (value = defaultValue());
			return value;
		}

		/// <summary>
		/// 尝试获取并转换值，值不存在或转换失败时返回默认值
		/// 转换使用object.ConvertOrDefault
		/// </summary>
		/// <typeparam name="T">x需要转换到的类型</typeparam>
		/// <param name="dict">词典</param>
		/// <param name="key">键名</param>
		/// <param name="defaultValue">默认值</param>
		/// <returns></returns>
		public static T GetOrDefault<T>(
			this IDictionary<string, object> dict, string key, T defaultValue = default(T)) {
			object result = dict.GetOrDefault(key);
			if (result == null) {
				return defaultValue;
			}
			return result.ConvertOrDefault(defaultValue);
		}
	}
}
