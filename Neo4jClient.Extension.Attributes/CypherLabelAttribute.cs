// ***********************************************************************
// Assembly         : Neo4jClient.Extension.Cypher.Attributes
// Author           : Shawn Anderson
// Created          : 01-19-2015
//
// Last Modified By : Shawn Anderson
// Last Modified On : 01-19-2015
// ***********************************************************************
// <copyright file="CypherLabelAttribute.cs" company="">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace Neo4jClient.Extension.Cypher.Attributes
{
	/// <summary>
	/// Class CypherLabelAttribute.
	/// </summary>
	public class CypherLabelAttribute : CypherExtensionAttribute
	{
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }
	}
}
