﻿using MongoDB.Driver;
using System;
using ZKWeb.Database;

namespace ZKWeb.ORM.MongoDB {
	/// <summary>
	/// MongoDB database context factory<br/>
	/// MongoDB doesn't need to migrate database scheme<br/>
	/// <br/>
	/// <br/>
	/// </summary>
	internal class MongoDBDatabaseContextFactory : IDatabaseContextFactory {
		/// <summary>
		/// Connection url<br/>
		/// <br/>
		/// </summary>
		private MongoUrl ConnectionUrl { get; set; }
		/// <summary>
		/// MongoDB entity mappings<br/>
		/// <br/>
		/// </summary>
		private MongoDBEntityMappings Mappings { get; set; }

		/// <summary>
		/// Initialize<br/>
		/// <br/>
		/// </summary>
		/// <param name="database">Database type</param>
		/// <param name="connectionString">Connection string</param>
		public MongoDBDatabaseContextFactory(string database, string connectionString) {
			if (string.Compare(database, "MongoDB", true) != 0) {
				throw new ArgumentException($"Database type should be MongoDB");
			}
			ConnectionUrl = new MongoUrl(connectionString);
			if (string.IsNullOrEmpty(ConnectionUrl.DatabaseName)) {
				throw new ArgumentException("Please set the database name in connection string");
			}
			Mappings = new MongoDBEntityMappings(ConnectionUrl);
		}

		/// <summary>
		/// Create database context<br/>
		/// <br/>
		/// </summary>
		/// <returns></returns>
		public IDatabaseContext CreateContext() {
			return new MongoDBDatabaseContext(ConnectionUrl, Mappings);
		}
	}
}
