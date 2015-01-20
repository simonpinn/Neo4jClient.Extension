// ***********************************************************************
// Assembly         : Neo4jClient.Extension
// Author           : Shawn Anderson
// Created          : 01-19-2015
//
// Last Modified By : Shawn Anderson
// Last Modified On : 01-19-2015
// ***********************************************************************
// <copyright file="CypherTypeItemHelper.cs" company="">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Neo4jClient.Extension.Cypher.Attributes;

namespace Neo4jClient.Extension.Cypher
{
	/// <summary>
	/// Class CypherTypeItemHelper.
	/// </summary>
	public class CypherTypeItemHelper
	{
		/// <summary>
		/// The _type properties
		/// </summary>
		private readonly ConcurrentDictionary<CypherTypeItem, List<CypherProperty>> _typeProperties = new ConcurrentDictionary<CypherTypeItem, List<CypherProperty>>();

		/// <summary>
		/// Adds the key attribute.
		/// </summary>
		/// <typeparam name="TEntity">The type of the t entity.</typeparam>
		/// <typeparam name="TAttr">The type of the t attribute.</typeparam>
		/// <param name="context">The context.</param>
		/// <param name="entity">The entity.</param>
		/// <returns>CypherTypeItem.</returns>
		public CypherTypeItem AddKeyAttribute<TEntity, TAttr>(ICypherExtensionContext context, TEntity entity)
			where TAttr : CypherExtensionAttribute
			where TEntity : class
		{
			var type = entity.GetType();
			var key = new CypherTypeItem { Type = type, AttributeType = typeof(TAttr) };
			//check cache
			if (!_typeProperties.ContainsKey(key))
			{
				//strip off properties create map for usage
				_typeProperties.AddOrUpdate(key, type.GetProperties().Where(x => x.GetCustomAttributes(typeof(TAttr),true).Any())
					.Select(x => new CypherProperty {TypeName = x.Name, JsonName = x.Name.ApplyCasing(context)})
					.ToList(), (k, e) => e);
			}
			return key;
		}

		/// <summary>
		/// Propertieses for purpose.
		/// </summary>
		/// <typeparam name="TEntity">The type of the t entity.</typeparam>
		/// <typeparam name="TAttr">The type of the t attribute.</typeparam>
		/// <param name="context">The context.</param>
		/// <param name="entity">The entity.</param>
		/// <returns>List&lt;CypherProperty&gt;.</returns>
		public List<CypherProperty> PropertiesForPurpose<TEntity, TAttr>(ICypherExtensionContext context, TEntity entity)
			where TEntity : class
			where TAttr : CypherExtensionAttribute
		{
			var key = AddKeyAttribute<TEntity, TAttr>(context, entity);
			return _typeProperties[key];
		}

		/// <summary>
		/// Propertieses for purpose.
		/// </summary>
		/// <typeparam name="TEntity">The type of the t entity.</typeparam>
		/// <typeparam name="TAttr">The type of the t attribute.</typeparam>
		/// <param name="entity">The entity.</param>
		/// <returns>List&lt;CypherProperty&gt;.</returns>
		public List<CypherProperty> PropertiesForPurpose<TEntity, TAttr>(TEntity entity)
			where TEntity : class
			where TAttr : CypherExtensionAttribute
		{
			return PropertiesForPurpose<TEntity, TAttr>(CypherExtension.DefaultExtensionContext,entity);
		}

		/// <summary>
		/// Adds the property usage.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="properties">The properties.</param>
		public void AddPropertyUsage(CypherTypeItem type, List<CypherProperty> properties)
		{
			_typeProperties.AddOrUpdate(type, properties, (item, list) => properties);
		}
	}
}
