﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ZKWebStandard.Ioc;

namespace ZKWebStandard.Extensions {
	/// <summary>
	/// IContainer extension methods<br/>
	/// <br/>
	/// </summary>
	public static class IContainerExtensions {
		/// <summary>
		/// Build factory from original factory and reuse type<br/>
		/// <br/>
		/// </summary>
		/// <param name="container">IoC container</param>
		/// <param name="originalFactory">Original factory</param>
		/// <param name="reuseType">Reuse type</param>
		/// <returns></returns>
		public static Func<object> BuildFactory(
			this IContainer container, Func<object> originalFactory, ReuseType reuseType) {
			if (reuseType == ReuseType.Transient) {
				// Transient
				return originalFactory;
			} else if (reuseType == ReuseType.Singleton) {
				// Singleton
				object value = null;
				object valueLock = new object();
				return () => {
					if (value != null) {
						return value;
					}
					lock (valueLock) {
						if (value != null) {
							return value; // double check
						}
						value = originalFactory();
						return value;
					}
				};
			} else {
				throw new NotSupportedException(string.Format("unsupported reuse type {0}", reuseType));
			}
		}

		/// <summary>
		/// Cache for type and it's original factory<br/>
		/// <br/>
		/// </summary>
		private readonly static ConcurrentDictionary<Type, Func<object>> TypeFactorysCache =
			new ConcurrentDictionary<Type, Func<object>>();

		/// <summary>
		/// Build factory from type and reuse type<br/>
		/// <br/>
		/// </summary>
		/// <param name="container">IoC container</param>
		/// <param name="type">The type</param>
		/// <param name="reuseType">Reuse type</param>
		/// <returns></returns>
		public static Func<object> BuildFactory(
			this IContainer container, Type type, ReuseType reuseType) {
			var typeFactor = TypeFactorysCache.GetOrAdd(type, t => {
				// Support constructor dependency injection
				// First detect Inject attribute, then use the constructor that have most parameters	
				var argumentExpressions = new List<Expression>();
				var constructors = t.GetConstructors();
				var constructor = constructors.FirstOrDefault(
					c => c.GetAttribute<InjectAttribute>() != null);
				if (constructor == null) {
					constructor = constructors
						.Where(c => c.IsPublic)
						.OrderByDescending(c => c.GetParameters().Length)
						.FirstOrDefault();
				}
				if (constructor == null) {
					throw new ArgumentException(
						$"Type {type} should have atleast one public constructor");
				}
				foreach (var parameter in constructor.GetParameters()) {
					var parameterType = parameter.ParameterType;
					var parameterTypeInfo = parameterType.GetTypeInfo();
					if (parameterTypeInfo.IsGenericType &&
						parameterTypeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
						argumentExpressions.Add(Expression.Call(
							Expression.Constant(container), nameof(IContainer.ResolveMany),
							parameterTypeInfo.GetGenericArguments(),
							Expression.Constant(null)));
					} else {
						argumentExpressions.Add(Expression.Call(
							Expression.Constant(container), nameof(IContainer.Resolve),
							new[] { parameterType },
							Expression.Constant(IfUnresolved.ReturnDefault),
							Expression.Constant(null)));
					}
				}
				var newExpression = Expression.New(constructor, argumentExpressions);
				return Expression.Lambda<Func<object>>(newExpression).Compile();
			});
			return container.BuildFactory(typeFactor, reuseType);
		}
	}
}
