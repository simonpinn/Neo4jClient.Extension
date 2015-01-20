// ***********************************************************************
// Assembly         : Neo4jClient.Extension
// Author           : Shawn Anderson
// Created          : 01-19-2015
//
// Last Modified By : Shawn Anderson
// Last Modified On : 01-19-2015
// ***********************************************************************
// <copyright file="CypherTypeItem.cs" company="">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;

namespace Neo4jClient.Extension.Cypher
{
	/// <summary>
	/// Class CypherTypeItem.
	/// </summary>
	public class CypherTypeItem : IEquatable<CypherTypeItem>
	{
		/// <summary>
		/// Gets or sets the type.
		/// </summary>
		/// <value>The type.</value>
		public Type Type { get; set; }
		/// <summary>
		/// Gets or sets the type of the attribute.
		/// </summary>
		/// <value>The type of the attribute.</value>
		public Type AttributeType { get; set; }

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
		public bool Equals(CypherTypeItem other)
		{
			return other.Type == Type && other.AttributeType == AttributeType;
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
		bool IEquatable<CypherTypeItem>.Equals(CypherTypeItem other)
		{
			return Equals(other);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
		public override int GetHashCode()
		{
			return Type.GetHashCode() ^ AttributeType.GetHashCode();
		}
	}
}
