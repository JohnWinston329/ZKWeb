﻿using System;
using System.Collections.Generic;
using ZKWebStandard.Extensions;
using ZKWebStandard.Utils;
using ZKWebStandard.Web;

namespace ZKWebStandard.Collections {
	/// <summary>
	/// Key-value cache based on http context
	/// All cached value will be destroy after context end
	/// </summary>
	/// <typeparam name="TKey">Key type</typeparam>
	/// <typeparam name="TValue">Value type</typeparam>
	public class HttpContextCache<TKey, TValue> : IKeyValueCache<TKey, TValue> {
		/// <summary>
		/// Http context key
		/// Default is "HttpContextCache_" + random string
		/// </summary>
		public string ContextKey { get; set; }

		/// <summary>
		/// Initialize
		/// </summary>
		public HttpContextCache() {
			ContextKey = "HttpContextCache_" + RandomUtils.RandomString(12);
		}

		/// <summary>
		/// Get cache storage from http context
		/// Use normal dictionary because thread race is impossible
		/// </summary>
		/// <returns></returns>
		protected IDictionary<TKey, TValue> GetStorage() {
			return (IDictionary<TKey, TValue>)HttpManager.CurrentContext.Items
				.GetOrCreate(ContextKey, () => new Dictionary<TKey, TValue>());
		}

		/// <summary>
		/// Put value to cache
		/// </summary>
		/// <param name="key">Cache key</param>
		/// <param name="value">Cache value</param>
		/// <param name="keepTime">Keep time, not used</param>
		public void Put(TKey key, TValue value, TimeSpan keepTime) {
			if (!HttpManager.CurrentContextExists) {
				return;
			}
			var storage = GetStorage();
			storage[key] = value;
		}

		/// <summary>
		/// Try to get cached value
		/// Return false if no exist value or exist value expired
		/// </summary>
		/// <param name="key">Cache key</param>
		/// <param name="value">Cache value</param>
		/// <returns></returns>
		public bool TryGetValue(TKey key, out TValue value) {
			if (!HttpManager.CurrentContextExists) {
				value = default(TValue);
				return false;
			}
			var storage = GetStorage();
			return storage.TryGetValue(key, out value);
		}

		/// <summary>
		/// Remove cached value
		/// </summary>
		/// <param name="key">Cache key</param>
		public void Remove(TKey key) {
			if (!HttpManager.CurrentContextExists) {
				return;
			}
			var storage = GetStorage();
			storage.Remove(key);
		}

		/// <summary>
		/// Count all cached values
		/// </summary>
		/// <returns></returns>
		public int Count() {
			if (!HttpManager.CurrentContextExists) {
				return 0;
			}
			var storage = GetStorage();
			return storage.Count;
		}

		/// <summary>
		/// Clear all cached values
		/// </summary>
		public void Clear() {
			if (!HttpManager.CurrentContextExists) {
				return;
			}
			var storage = GetStorage();
			storage.Clear();
		}
	}
}
