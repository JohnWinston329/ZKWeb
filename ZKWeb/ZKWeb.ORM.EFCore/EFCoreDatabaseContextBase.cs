﻿using Microsoft.EntityFrameworkCore;
using System;
using ZKWeb.Server;
using ZKWeb.Storage;

namespace ZKWeb.ORM.EFCore {
	/// <summary>
	/// A base database context only contains migration history table<br/>
	/// <br/>
	/// </summary>
	public class EFCoreDatabaseContextBase : DbContext {
		/// <summary>
		/// Database type<br/>
		/// <br/>
		/// </summary>
		private string DatabaseName { get; set; }
		/// <summary>
		/// Connection string<br/>
		/// <br/>
		/// </summary>
		private string ConnectionString { get; set; }

		/// <summary>
		/// Initialize<br/>
		/// <br/>
		/// </summary>
		/// <param name="database">Database type</param>
		/// <param name="connectionString">Connection string</param>
		public EFCoreDatabaseContextBase(string database, string connectionString) {
			DatabaseName = database;
			ConnectionString = connectionString;
		}

		/// <summary>
		/// Configure context options<br/>
		/// <br/>
		/// </summary>
		/// <param name="optionsBuilder">Options builder</param>
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
			var pathConfig = Application.Ioc.Resolve<LocalPathConfig>();
			if (string.Compare(DatabaseName, "MSSQL", true) == 0) {
				optionsBuilder.UseSqlServer(
					ConnectionString, option => option.UseRowNumberForPaging());
			} else if (string.Compare(DatabaseName, "SQLite", true) == 0) {
				optionsBuilder.UseSqlite(
					ConnectionString.Replace("{{App_Data}}", pathConfig.AppDataDirectory));
			} else if (string.Compare(DatabaseName, "MySQL", true) == 0) {
				optionsBuilder.UseMySql(ConnectionString);
			} else if (string.Compare(DatabaseName, "PostgreSQL", true) == 0) {
				optionsBuilder.UseNpgsql(ConnectionString);
			} else if (string.Compare(DatabaseName, "InMemory", true) == 0) {
				optionsBuilder.UseInMemoryDatabase();
			} else {
				throw new ArgumentException($"unsupported database type {Database}");
			}
		}

		/// <summary>
		/// Configure entity model<br/>
		/// <br/>
		/// </summary>
		/// <param name="modelBuilder">Model builder</param>
		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			new EFCoreMigrationHistory().Configure(modelBuilder);
		}
	}
}
