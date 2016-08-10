﻿namespace ZKWeb.Database {
	/// <summary>
	/// Empty base interface
	/// </summary>
	public interface IEntity { }

	/// <summary>
	/// Interface for entity with a primary key
	/// </summary>
	/// <typeparam name="TPrimaryKey">Primary key type</typeparam>
	public interface IEntity<TPrimaryKey> : IEntity {
		/// <summary>
		/// Primary key
		/// </summary>
		TPrimaryKey Id { get; set; }
	}
}
