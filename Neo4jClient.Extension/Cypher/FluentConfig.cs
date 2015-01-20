// ***********************************************************************
// Assembly         : Neo4jClient.Extension
// Author           : sanderson
// Created          : 01-19-2015
//
// Last Modified By : sanderson
// Last Modified On : 01-19-2015
// ***********************************************************************
// <copyright file="FluentConfig.cs" company="">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Neo4jClient.Extension.Cypher.Attributes;

namespace Neo4jClient.Extension.Cypher
{
	/// <summary>
	/// Class FluentConfig.
	/// </summary>
	public class FluentConfig
	{
		/// <summary>
		/// The _context
		/// </summary>
		private readonly CypherExtensionContext _context;

		/// <summary>
		/// Initializes a new instance of the <see cref="FluentConfig"/> class.
		/// </summary>
		/// <param name="context">The context.</param>
		private FluentConfig(CypherExtensionContext context)
		{
			_context = context;
		}

		/// <summary>
		/// Configurations the specified context.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns>FluentConfig.</returns>
		public static FluentConfig Config(CypherExtensionContext context = null)
		{
			return new FluentConfig(context??new CypherExtensionContext());
		}

		/// <summary>
		/// Withes the specified label.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="label">The label.</param>
		/// <returns>ConfigWith&lt;T&gt;.</returns>
		public ConfigWith<T> With<T>(string label = null)
		{
			return new ConfigWith<T>(_context, label);
		}  
	}

	/// <summary>
	/// Class ConfigWith.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ConfigWith<T>
	{
		/// <summary>
		/// The _properties
		/// </summary>
		private readonly ConcurrentBag<Tuple<Type, CypherProperty>> _properties = new ConcurrentBag<Tuple<Type, CypherProperty>>();
		/// <summary>
		/// The _context
		/// </summary>
		private readonly CypherExtensionContext _context;
		/// <summary>
		/// The _label
		/// </summary>
		private readonly string _label;

		/// <summary>
		/// Initializes a new instance of the <see cref="ConfigWith{T}"/> class.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="label">The label.</param>
		public ConfigWith(CypherExtensionContext context, string label = null)
		{
			_label = label;
			_context = context;
		}

		/// <summary>
		/// Configurations the specified property.
		/// </summary>
		/// <typeparam name="TP">The type of the tp.</typeparam>
		/// <param name="property">The property.</param>
		/// <param name="attribute">The attribute.</param>
		/// <returns>ConfigWith&lt;T&gt;.</returns>
		private ConfigWith<T> Config<TP>(Expression<Func<T, TP>> property, Type attribute)
		{
			var memberExpression = property.Body as MemberExpression ?? ((UnaryExpression) property.Body).Operand as MemberExpression;
			var name = memberExpression == null ? null : memberExpression.Member.Name; 
			_properties.Add(new Tuple<Type, CypherProperty>(attribute, new CypherProperty {TypeName = name, JsonName = name.ApplyCasing(_context)}));
			return this;
		}

		/// <summary>
		/// Matches the specified property.
		/// </summary>
		/// <typeparam name="TP">The type of the tp.</typeparam>
		/// <param name="property">The property.</param>
		/// <returns>ConfigWith&lt;T&gt;.</returns>
		public ConfigWith<T> Match<TP>(Expression<Func<T, TP>> property)
		{
			return Config(property, typeof(CypherMatchAttribute)); ;
		}

		/// <summary>
		/// Merges the specified property.
		/// </summary>
		/// <typeparam name="TP">The type of the tp.</typeparam>
		/// <param name="property">The property.</param>
		/// <returns>ConfigWith&lt;T&gt;.</returns>
		public ConfigWith<T> Merge<TP>(Expression<Func<T,TP>> property)
		{
			return Config(property, typeof (CypherMergeAttribute));
		}

		/// <summary>
		/// Merges the on create.
		/// </summary>
		/// <typeparam name="TP">The type of the tp.</typeparam>
		/// <param name="property">The property.</param>
		/// <returns>ConfigWith&lt;T&gt;.</returns>
		public ConfigWith<T> MergeOnCreate<TP>(Expression<Func<T, TP>> property)
		{
			return Config(property, typeof(CypherMergeOnCreateAttribute)); ;
		}
		/// <summary>
		/// Merges the on match.
		/// </summary>
		/// <typeparam name="TP">The type of the tp.</typeparam>
		/// <param name="property">The property.</param>
		/// <returns>ConfigWith&lt;T&gt;.</returns>
		public ConfigWith<T> MergeOnMatch<TP>(Expression<Func<T, TP>> property)
		{
			return Config(property, typeof(CypherMergeOnMatchAttribute)); ;
		}

		/// <summary>
		/// Sets this instance.
		/// </summary>
		/// <returns>List&lt;Tuple&lt;CypherTypeItem, List&lt;CypherProperty&gt;&gt;&gt;.</returns>
		public List<Tuple<CypherTypeItem, List<CypherProperty>>> Set()
		{
			//set the properties
			var returnValue = _properties.GroupBy(x => x.Item1)
				.Select(x => new Tuple<CypherTypeItem, List<CypherProperty>>(new CypherTypeItem()
				{
					AttributeType = x.Key,
					Type = typeof (T)
				}, x.Select(y => y.Item2).Distinct(new CypherPropertyComparer()).ToList())).ToList();
		
				returnValue.ForEach(x => CypherExtension.ConfigProperties(x.Item1, x.Item2));
			//set the label
			if (!string.IsNullOrWhiteSpace(_label))
			{
				CypherExtension.ConfigLabel(typeof (T), _label);
			}
			return returnValue;
		}

		/// <summary>
		/// Withes the specified label.
		/// </summary>
		/// <typeparam name="TN">The type of the tn.</typeparam>
		/// <param name="label">The label.</param>
		/// <returns>ConfigWith&lt;TN&gt;.</returns>
		public ConfigWith<TN> With<TN>(string label=null)
		{
			Set();
			return new ConfigWith<TN>(_context, label);
		}  
	}
}
