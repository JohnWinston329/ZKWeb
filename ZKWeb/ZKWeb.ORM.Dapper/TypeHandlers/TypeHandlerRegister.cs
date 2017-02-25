﻿using Dapper;
using System;

namespace ZKWeb.ORM.Dapper.TypeHandlers {
	/// <summary>
	/// Dapper type handler registrator
	/// </summary>
	internal static class TypeHandlerRegistrator {
		/// <summary>
		/// Register dapper type handler
		/// </summary>
		public static void Register(Type type, SqlMapper.ITypeHandler handler) {
			// Dapper will replace exists handler, and no need to clone typeHandlers
			SqlMapper.AddTypeHandlerImpl(type, handler, false);
		}

		/// <summary>
		/// Register dapper type handler for json serialized type
		/// </summary>
		/// <param name="type">Serialized type</param>
		public static void RegisterJsonSerializedType(Type type) {
			Register(type,
				(SqlMapper.ITypeHandler)Activator.CreateInstance(
				typeof(JsonSerializedTypeHandler<>).MakeGenericType(type)));
		}
	}
}
