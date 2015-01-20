// ***********************************************************************
// Assembly         : Neo4jClient.Extension
// Author           : Shawn Anderson
// Created          : 01-19-2015
//
// Last Modified By : Shawn Anderson
// Last Modified On : 01-19-2015
// ***********************************************************************
// <copyright file="CypherExtension.cs" company="">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Neo4jClient.Cypher;
using Neo4jClient.Extension.Cypher.Attributes;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient.Extension.Cypher
{
	/// <summary>
	/// Class CypherExtension.
	/// </summary>
	public static class CypherExtension
	{
		/// <summary>
		/// The cypher type item helper
		/// </summary>
		private static readonly CypherTypeItemHelper CypherTypeItemHelper = new CypherTypeItemHelper();
		/// <summary>
		/// The default extension context
		/// </summary>
		public static CypherExtensionContext DefaultExtensionContext = new CypherExtensionContext();
		/// <summary>
		/// The entity label cache
		/// </summary>
		private static readonly Dictionary<Type, string> EntityLabelCache = new Dictionary<Type, string>();

		/// <summary>
		/// Entities the label.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entity">The entity.</param>
		/// <returns>System.String.</returns>
		public static string EntityLabel<T>(this T entity)
		{
			var entityType = entity.GetType();
			if (!EntityLabelCache.ContainsKey(entityType))
			{
				var label = entityType.GetCustomAttributes(typeof (CypherLabelAttribute), true).FirstOrDefault() as CypherLabelAttribute;
				EntityLabelCache.Add(entityType, label == null ? entityType.Name : label.Name);
			}
			return EntityLabelCache[entityType];
		}

		/// <summary>
		/// Entities the parameter key.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entity">The entity.</param>
		/// <param name="paramKey">The parameter key.</param>
		/// <returns>System.String.</returns>
		public static string EntityParamKey<T>(this T entity, string paramKey = null)
		{
			return paramKey ?? entity.GetType().Name.ToLowerInvariant();
		}

		/// <summary>
		/// To the cypher string.
		/// </summary>
		/// <typeparam name="TEntity">The type of the t entity.</typeparam>
		/// <param name="entity">The entity.</param>
		/// <param name="context">The context.</param>
		/// <param name="useProperties">The use properties.</param>
		/// <param name="paramKey">The parameter key.</param>
		/// <returns>System.String.</returns>
		public static string ToCypherString<TEntity>(this TEntity entity, ICypherExtensionContext context, List<CypherProperty> useProperties, string paramKey)
			where TEntity : class
		{
			//with the list of properties construct the string
			var label = entity.EntityLabel();
			paramKey = entity.EntityParamKey(paramKey);
			//Iterate properties, insert paramKey
			var properties = new Func<string>(() => string.Format("{{{0}}}",
						string.Join(",",useProperties.Select(x => string.Format("{0}:{{{1}}}.{0}", x.JsonName, paramKey)))));

			return string.Format("{0}:{1} {2}", paramKey, label, properties());
		}

		/// <summary>
		/// To the cypher string.
		/// </summary>
		/// <typeparam name="TEntity">The type of the t entity.</typeparam>
		/// <typeparam name="TAttr">The type of the t attribute.</typeparam>
		/// <param name="entity">The entity.</param>
		/// <param name="context">The context.</param>
		/// <param name="paramKey">The parameter key.</param>
		/// <param name="useProperties">The use properties.</param>
		/// <returns>System.String.</returns>
		public static string ToCypherString<TEntity, TAttr>(this TEntity entity, ICypherExtensionContext context, string paramKey = null, List<CypherProperty> useProperties = null)
			where TAttr : CypherExtensionAttribute
			where TEntity : class
		{
			return entity.ToCypherString(context, useProperties?? CypherTypeItemHelper.PropertiesForPurpose<TEntity,TAttr>(entity), paramKey);
		}

		/// <summary>
		/// Creates the dynamic.
		/// </summary>
		/// <typeparam name="TEntity">The type of the t entity.</typeparam>
		/// <param name="entity">The entity.</param>
		/// <param name="properties">The properties.</param>
		/// <returns>Dictionary&lt;System.String, System.Object&gt;.</returns>
		public static Dictionary<string, object> CreateDynamic<TEntity>(this TEntity entity, List<CypherProperty> properties) where TEntity : class
		{
			var type = entity.GetType();
			return properties.Select(prop => new { Key = prop.JsonName, Value = type.GetProperty(prop.TypeName).GetValue(entity, null) }).ToDictionary(x => x.Key, x => x.Value);
		}

		/// <summary>
		/// Matches the entity.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query">The query.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="paramKey">The parameter key.</param>
		/// <param name="preCql">The pre CQL.</param>
		/// <param name="postCql">The post CQL.</param>
		/// <param name="propertyOverride">The property override.</param>
		/// <returns>ICypherFluentQuery.</returns>
		public static ICypherFluentQuery MatchEntity<T>(this ICypherFluentQuery query, T entity, string paramKey = null, string preCql = "", string postCql = "", List<CypherProperty> propertyOverride = null) where T : class
		{
			paramKey = entity.EntityParamKey(paramKey);
			var cql = string.Format("{0}({1}){2}", preCql, entity.ToCypherString<T, CypherMatchAttribute>(CypherExtensionContext.Create(query), paramKey, propertyOverride), postCql);
			//create a dynamic object for the type
			dynamic cutdown = entity.CreateDynamic(propertyOverride ?? CypherTypeItemHelper.PropertiesForPurpose<T, CypherMatchAttribute>(entity));
			return query.Match(cql).WithParam(paramKey, cutdown);
		}

		/// <summary>
		/// Merges the entity.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query">The query.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="paramKey">The parameter key.</param>
		/// <param name="mergeOverride">The merge override.</param>
		/// <param name="onMatchOverride">The on match override.</param>
		/// <param name="onCreateOverride">The on create override.</param>
		/// <param name="preCql">The pre CQL.</param>
		/// <param name="postCql">The post CQL.</param>
		/// <returns>ICypherFluentQuery.</returns>
		public static ICypherFluentQuery MergeEntity<T>(this ICypherFluentQuery query, T entity, string paramKey = null, List<CypherProperty> mergeOverride = null, List<CypherProperty> onMatchOverride = null, List<CypherProperty> onCreateOverride = null,string preCql = "", string postCql = "") where T : class
		{
			paramKey = entity.EntityParamKey(paramKey);
			var cql = string.Format("{0}({1}){2}", preCql, entity.ToCypherString<T, CypherMergeAttribute>(CypherExtensionContext.Create(query), paramKey,mergeOverride), postCql);
			return query.CommonMerge(entity, paramKey, cql, mergeOverride, onMatchOverride, onCreateOverride);
		}

		/// <summary>
		/// Merges the relationship.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query">The query.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="mergeOverride">The merge override.</param>
		/// <param name="onMatchOverride">The on match override.</param>
		/// <param name="onCreateOverride">The on create override.</param>
		/// <returns>ICypherFluentQuery.</returns>
		public static ICypherFluentQuery MergeRelationship<T>(this ICypherFluentQuery query, T entity, List<CypherProperty> mergeOverride = null, List<CypherProperty> onMatchOverride = null, List<CypherProperty> onCreateOverride = null) where T : BaseRelationship
		{
			//Eaxctly the same as a merge entity except the cql is different
			var cql = string.Format("({0})-[{1}]->({2})", entity.FromKey, entity.ToCypherString<T, CypherMergeAttribute>(CypherExtensionContext.Create(query), entity.Key, mergeOverride), entity.ToKey);
			return query.CommonMerge(entity, entity.Key, cql, mergeOverride, onMatchOverride, onCreateOverride);
		}

		/// <summary>
		/// Commons the merge.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="query">The query.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="key">The key.</param>
		/// <param name="cql">The CQL.</param>
		/// <param name="mergeOverride">The merge override.</param>
		/// <param name="onMatchOverride">The on match override.</param>
		/// <param name="onCreateOverride">The on create override.</param>
		/// <returns>ICypherFluentQuery.</returns>
		private static ICypherFluentQuery CommonMerge<T>(this ICypherFluentQuery query, T entity, string key, string cql, List<CypherProperty> mergeOverride = null, List<CypherProperty> onMatchOverride = null, List<CypherProperty> onCreateOverride = null) where T : class
		{
			//A merge requires the properties of both merge, create and match in the cutdown object
			var merge = mergeOverride ?? CypherTypeItemHelper.PropertiesForPurpose<T, CypherMergeAttribute>(entity);
			var create = onCreateOverride ?? CypherTypeItemHelper.PropertiesForPurpose<T, CypherMergeOnCreateAttribute>(entity);
			var match = onMatchOverride ?? CypherTypeItemHelper.PropertiesForPurpose<T, CypherMergeOnMatchAttribute>(entity);
			var compare = new CypherPropertyComparer();
			var propertyOverride = create.Union(match.Union(merge.Union(create, compare), compare), compare).ToList();

			dynamic cutdown = entity.CreateDynamic(propertyOverride);
			var setOnAction = new Action<List<CypherProperty>,ICypherFluentQuery,  Action<ICypherFluentQuery, string>>((list,q, action) => {
				var set = string.Join(",", list.Select(x => string.Format("{0}.{1}={{{0}}}.{1}", key, x.JsonName)));
				if (!string.IsNullOrEmpty(set))
				{
					action(q, set);
				}
			});

			query = query.Merge(cql);

			setOnAction(match, query, (q, s) => query = q.OnMatch().Set(s));
			setOnAction(create, query, (q, s) => query = q.OnCreate().Set(s));

			return query.WithParam(key, cutdown);
		}

		/// <summary>
		/// Uses the properties.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entity">The entity.</param>
		/// <param name="properties">The properties.</param>
		/// <returns>List&lt;CypherProperty&gt;.</returns>
		public static List<CypherProperty> UseProperties<T>(this T entity, params Expression<Func<T, object>>[] properties)
			where T : class
		{
			return entity.UseProperties(DefaultExtensionContext, properties);
		}

		/// <summary>
		/// Uses the properties.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entity">The entity.</param>
		/// <param name="context">The context.</param>
		/// <param name="properties">The properties.</param>
		/// <returns>List&lt;CypherProperty&gt;.</returns>
		public static List<CypherProperty> UseProperties<T>(this T entity, CypherExtensionContext context, params Expression<Func<T, object>>[] properties)
			where T : class
		{
			//Cache the T entity properties into a dictionary of strings
			if (properties != null)
			{
				return properties.ToList().Where(x => x != null).Select(x =>
				{
					var memberExpression = x.Body as MemberExpression ?? ((UnaryExpression) x.Body).Operand as MemberExpression;
					return memberExpression == null ? null : memberExpression.Member.Name;
				}).Select(x => new CypherProperty {TypeName = x, JsonName = x.ApplyCasing(context)}).ToList();
			}
			return new List<CypherProperty>();
		}

		/// <summary>
		/// Gets the formatted debug text.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <returns>System.String.</returns>
		public static string GetFormattedDebugText(this ICypherFluentQuery query)
		{
			var regex = new Regex("\\\"([^(\\\")\"]+)\\\":", RegexOptions.Multiline);
			return regex.Replace(query.Query.DebugQueryText, "$1:");
		}

		/// <summary>
		/// Applies the casing.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="context">The context.</param>
		/// <returns>System.String.</returns>
		public static string ApplyCasing(this string value, ICypherExtensionContext context)
		{
			var camelCase = (context.JsonContractResolver is CamelCasePropertyNamesContractResolver);
			return camelCase ? string.Format("{0}{1}", value.Substring(0, 1).ToLowerInvariant(), value.Length > 1 ? value.Substring(1, value.Length - 1) : string.Empty)
								: value;
		}

		/// <summary>
		/// Configurations the properties.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="properties">The properties.</param>
		public static void ConfigProperties(CypherTypeItem type, List<CypherProperty> properties)
		{
			CypherTypeItemHelper.AddPropertyUsage(type, properties);
		}
		/// <summary>
		/// Configurations the label.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="label">The label.</param>
		public static void ConfigLabel(Type type, string label)
		{
			if (EntityLabelCache.ContainsKey(type))
			{
				EntityLabelCache[type] = label;
			}
			else
			{
				EntityLabelCache.Add(type, label);
			}
		}
	}
}
