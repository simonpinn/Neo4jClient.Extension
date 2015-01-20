// ***********************************************************************
// Assembly         : Neo4jClient.Extension
// Author           : Shawn Anderson
// Created          : 01-19-2015
//
// Last Modified By : Shawn Anderson
// Last Modified On : 01-19-2015
// ***********************************************************************
// <copyright file="CypherExtensionContext.cs" company="">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Neo4jClient.Cypher;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient.Extension.Cypher
{
	/// <summary>
	/// Interface ICypherExtensionContext
	/// </summary>
	public interface ICypherExtensionContext
	{
		/// <summary>
		/// Gets or sets the json contract resolver.
		/// </summary>
		/// <value>The json contract resolver.</value>
		IContractResolver JsonContractResolver { get; set; }
	}

	/// <summary>
	/// Class CypherExtensionContext.
	/// </summary>
	public class CypherExtensionContext : ICypherExtensionContext
	{
		/// <summary>
		/// Creates the specified query.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <returns>CypherExtensionContext.</returns>
		public static CypherExtensionContext Create(ICypherFluentQuery query)
		{
			return new CypherExtensionContext()
			{
				//TODO: Once other pull request is in for camel casing, pass whatever is set
				//JsonContractResolver = query.Query.JsonContractResolver
			};
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CypherExtensionContext"/> class.
		/// </summary>
		public CypherExtensionContext()
		{
			JsonContractResolver = new CamelCasePropertyNamesContractResolver();
		}

		/// <summary>
		/// Gets or sets the json contract resolver.
		/// </summary>
		/// <value>The json contract resolver.</value>
		public IContractResolver JsonContractResolver { get; set; }
	}
}
