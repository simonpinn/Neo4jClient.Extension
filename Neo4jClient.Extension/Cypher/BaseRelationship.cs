// ***********************************************************************
// Assembly         : Neo4jClient.Extension
// Author           : Shawn Anderson
// Created          : 01-19-2015
//
// Last Modified By : Shawn Anderson
// Last Modified On : 01-19-2015
// ***********************************************************************
// <copyright file="BaseRelationship.cs" company="">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace Neo4jClient.Extension.Cypher
{
	/// <summary>
	/// Class BaseRelationship.
	/// </summary>
	public abstract class BaseRelationship
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BaseRelationship"/> class.
		/// </summary>
		/// <param name="key">The key.</param>
		protected BaseRelationship(string key):this(key,null,null){}
		/// <summary>
		/// Initializes a new instance of the <see cref="BaseRelationship"/> class.
		/// </summary>
		/// <param name="fromKey">From key.</param>
		/// <param name="toKey">To key.</param>
		protected BaseRelationship(string fromKey, string toKey):this(fromKey+toKey, fromKey,toKey){}
		/// <summary>
		/// Initializes a new instance of the <see cref="BaseRelationship"/> class.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="fromKey">From key.</param>
		/// <param name="toKey">To key.</param>
		protected BaseRelationship(string key, string fromKey, string toKey)
		{
			FromKey = fromKey;
			ToKey = toKey;
			Key = key;
		}

		/// <summary>
		/// Gets or sets from key.
		/// </summary>
		/// <value>From key.</value>
		public string FromKey { get; set; }
		/// <summary>
		/// Gets or sets the key.
		/// </summary>
		/// <value>The key.</value>
		public string Key { get; set; }
		/// <summary>
		/// Gets or sets to key.
		/// </summary>
		/// <value>To key.</value>
		public string ToKey { get; set; }
	}
}
