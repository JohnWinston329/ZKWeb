﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ZKWebStandard.Collections {
	/// <summary>
	/// 缓存类，支持指定过期时间，线程安全
	/// </summary>
	/// <typeparam name="TKey">键类型</typeparam>
	/// <typeparam name="TValue">值类型</typeparam>
	public class MemoryCache<TKey, TValue> {
		/// <summary>
		/// 定期删除过期数据的间隔时间
		/// 默认180秒
		/// </summary>
		public TimeSpan RevokeExpiresInterval { get; set; }
		/// <summary>
		/// 缓存数据
		/// 结构 { 键, (对象, 过期时间) }
		/// 使用了线程锁所以这里只需要普通的Dictionary
		/// </summary>
		protected IDictionary<TKey, Pair<TValue, DateTime>> Cache { get; set; }
		/// <summary>
		/// 缓存数据的线程锁
		/// </summary>
		protected ReaderWriterLockSlim CacheLock { get; set; }
		/// <summary>
		/// 最后一次删除过期缓存的时间
		/// </summary>
		protected DateTime LastRevokeExpires { get; set; }

		/// <summary>
		/// 初始化
		/// </summary>
		public MemoryCache() {
			RevokeExpiresInterval = TimeSpan.FromSeconds(180);
			Cache = new Dictionary<TKey, Pair<TValue, DateTime>>();
			CacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
			LastRevokeExpires = DateTime.UtcNow;
		}

		/// <summary>
		/// 删除过期的缓存数据
		/// 固定每180秒一次
		/// </summary>
		protected void RevokeExpires() {
			var now = DateTime.UtcNow;
			if ((now - LastRevokeExpires) < RevokeExpiresInterval) {
				return;
			}
			CacheLock.EnterWriteLock();
			try {
				if ((now - LastRevokeExpires) < RevokeExpiresInterval) {
					return; // double check
				}
				LastRevokeExpires = now;
				var expireKeys = Cache.Where(c => c.Value.Second < now).Select(c => c.Key).ToList();
				foreach (var key in expireKeys) {
					Cache.Remove(key);
				}
			} finally {
				CacheLock.ExitWriteLock();
			}
		}

		/// <summary>
		/// 设置缓存数据
		/// </summary>
		/// <param name="key">缓存键</param>
		/// <param name="value">缓存值</param>
		/// <param name="keepTime">保留时间</param>
		public void Put(TKey key, TValue value, TimeSpan keepTime) {
			RevokeExpires();
			if (keepTime == TimeSpan.Zero) {
				return;
			}
			var now = DateTime.UtcNow;
			CacheLock.EnterWriteLock();
			try {
				Cache[key] = Pair.Create(value, now + keepTime);
			} finally {
				CacheLock.ExitWriteLock();
			}
		}

		/// <summary>
		/// 获取缓存数据
		/// 没有或已过期时返回false
		/// </summary>
		/// <param name="key">缓存键</param>
		/// <param name="value">缓存值</param>
		/// <returns></returns>
		public bool TryGetValue(TKey key, out TValue value) {
			RevokeExpires();
			var now = DateTime.UtcNow;
			CacheLock.EnterReadLock();
			try {
				Pair<TValue, DateTime> pair;
				if (Cache.TryGetValue(key, out pair) && pair.Second > now) {
					value = pair.First;
					return true;
				} else {
					value = default(TValue);
					return false;
				}
			} finally {
				CacheLock.ExitReadLock();
			}
		}

		/// <summary>
		/// 获取缓存数据
		/// 没有或已过期时返回默认值
		/// </summary>
		/// <param name="key">缓存键</param>
		/// <param name="defaultValue">默认值</param>
		/// <returns></returns>
		public TValue GetOrDefault(TKey key, TValue defaultValue = default(TValue)) {
			TValue value;
			if (TryGetValue(key, out value)) {
				return value;
			}
			return defaultValue;
		}

		/// <summary>
		/// 获取缓存数据
		/// 没有或已过期时创建并返回
		/// 注意获取和创建整体不是原子的
		/// </summary>
		/// <param name="key">缓存</param>
		/// <param name="creator">创建函数</param>
		/// <param name="keepTime">保留时间</param>
		/// <returns></returns>
		public TValue GetOrCreate(TKey key, Func<TValue> creator, TimeSpan keepTime) {
			TValue value;
			if (keepTime == TimeSpan.Zero || !TryGetValue(key, out value)) {
				value = creator();
				Put(key, value, keepTime);
			}
			return value;
		}

		/// <summary>
		/// 删除缓存数据
		/// </summary>
		/// <param name="key">缓存键</param>
		public void Remove(TKey key) {
			RevokeExpires();
			CacheLock.EnterWriteLock();
			try {
				Cache.Remove(key);
			} finally {
				CacheLock.ExitWriteLock();
			}
		}

		/// <summary>
		/// 返回在缓存中的对象数量
		/// </summary>
		/// <returns></returns>
		public int Count() {
			CacheLock.EnterReadLock();
			try {
				return Cache.Count;
			} finally {
				CacheLock.ExitReadLock();
			}
		}

		/// <summary>
		/// 清空缓存数据
		/// </summary>
		public void Clear() {
			CacheLock.EnterWriteLock();
			try {
				Cache.Clear();
			} finally {
				CacheLock.ExitWriteLock();
			}
		}
	}
}
