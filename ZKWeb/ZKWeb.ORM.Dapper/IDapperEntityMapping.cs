﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace ZKWeb.ORM.Dapper {
	/// <summary>
	/// Defines the mapping for an entity
	/// </summary>
	internal interface IDapperEntityMapping {
		/// <summary>
		/// The entity type
		/// </summary>
		Type EntityType { get; }
		/// <summary>
		/// Id member
		/// </summary>
		MemberInfo IdMember { get; set; }
		/// <summary>
		/// Ordinary members, not releated to other entities
		/// </summary>
		IList<MemberInfo> OrdinaryMembers { get; set; }
	}
}
