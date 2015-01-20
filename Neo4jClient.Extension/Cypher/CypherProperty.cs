// ***********************************************************************
// Assembly         : Neo4jClient.Extension
// Author           : Shawn Anderson
// Created          : 01-19-2015
//
// Last Modified By : Shawn Anderson
// Last Modified On : 01-19-2015
// ***********************************************************************
// <copyright file="CypherProperty.cs" company="">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;

namespace Neo4jClient.Extension.Cypher
{
	/// <summary>
	/// Class CypherProperty.
	/// </summary>
	public class CypherProperty
	{
		/// <summary>
		/// Gets or sets the name of the type.
		/// </summary>
		/// <value>The name of the type.</value>
		public string TypeName { get; set; }
		/// <summary>
		/// Gets or sets the name of the json.
		/// </summary>
		/// <value>The name of the json.</value>
		public string JsonName { get; set; }
	}

	/// <summary>
	/// Class CypherPropertyComparer.
	/// </summary>
	public class CypherPropertyComparer : IEqualityComparer<CypherProperty>
	{
		/// <summary>
		/// Determines whether the specified objects are equal.
		/// </summary>
		/// <param name="x">The first object of type <paramref name="Neo4jClient.Extension.Cypher.CypherProperty" /> to compare.</param>
		/// <param name="y">The second object of type <paramref name="Neo4jClient.Extension.Cypher.CypherProperty" /> to compare.</param>
		/// <returns>true if the specified objects are equal; otherwise, false.</returns>
		public bool Equals(CypherProperty x, CypherProperty y)
		{
			return x.JsonName == y.JsonName;
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object" /> for which a hash code is to be returned.</param>
		/// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
		public int GetHashCode(CypherProperty obj)
		{
			return obj.JsonName.GetHashCode();
		}
	}
}
