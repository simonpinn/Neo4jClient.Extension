﻿using System.Collections.Generic;

namespace Neo4jClient.Extension.Cypher
{
	public class CypherProperty
	{

		public string TypeName { get; set; }

		public string JsonName { get; set; }
	}

	/// <summary>
	/// Class CypherPropertyComparer.
	/// </summary>
	public class CypherPropertyComparer : IEqualityComparer<CypherProperty>
	{

		public bool Equals(CypherProperty x, CypherProperty y)
		{
			return x.JsonName == y.JsonName;
		}

		public int GetHashCode(CypherProperty obj)
		{
			return obj.JsonName.GetHashCode();
		}
	}
}
