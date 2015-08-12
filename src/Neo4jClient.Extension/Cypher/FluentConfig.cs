using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Neo4jClient.Extension.Cypher.Attributes;

namespace Neo4jClient.Extension.Cypher
{
    public class FluentConfig
    {
        private readonly CypherExtensionContext _context;

        private FluentConfig(CypherExtensionContext context)
        {
            _context = context;
        }

        public static FluentConfig Config(CypherExtensionContext context = null)
        {
            return new FluentConfig(context??new CypherExtensionContext());
        }

        public ConfigWith<T> With<T>(string label = null)
        {
            return new ConfigWith<T>(_context, label);
        }  
    }

    public class ConfigWith<T>
    {
        private readonly ConcurrentBag<Tuple<Type, CypherProperty>> _properties = new ConcurrentBag<Tuple<Type, CypherProperty>>();
        private readonly CypherExtensionContext _context;
        private readonly string _label;

        public ConfigWith(CypherExtensionContext context, string label = null)
        {
            _label = label;
            _context = context;
        }

        private ConfigWith<T> Config<TP>(Expression<Func<T, TP>> property, Type attribute)
        {
            var memberExpression = property.Body as MemberExpression ?? ((UnaryExpression) property.Body).Operand as MemberExpression;
            var name = memberExpression == null ? null : memberExpression.Member.Name; 
            _properties.Add(new Tuple<Type, CypherProperty>(attribute, new CypherProperty {TypeName = name, JsonName = name.ApplyCasing(_context)}));
            return this;
        }

        public ConfigWith<T> Match<TP>(Expression<Func<T, TP>> property)
        {
            return Config(property, typeof(CypherMatchAttribute));
        }

        public ConfigWith<T> Merge<TP>(Expression<Func<T,TP>> property)
        {
            return Config(property, typeof (CypherMergeAttribute));
        }
        
        public ConfigWith<T> MergeOnCreate<TP>(Expression<Func<T, TP>> property)
        {
            return Config(property, typeof(CypherMergeOnCreateAttribute));
        }

        public ConfigWith<T> MergeOnMatch<TP>(Expression<Func<T, TP>> property)
        {
            return Config(property, typeof(CypherMergeOnMatchAttribute));
        }

        public ConfigWith<T> MergeOnMatchOrCreate<TP>(Expression<Func<T, TP>> property)
        {
            return MergeOnMatch(property).MergeOnCreate(property);
        }

        public List<Tuple<CypherTypeItem, List<CypherProperty>>> Set()
        {
            //set the properties
            var returnValue = _properties.GroupBy(x => x.Item1)
                .Select(x => new Tuple<CypherTypeItem, List<CypherProperty>>(new CypherTypeItem()
                {
                    AttributeType = x.Key,
                    Type = typeof (T)
                }, x.Select(y => y.Item2).Distinct(new CypherPropertyComparer()).ToList())).ToList();
        
                returnValue.ForEach(x => CypherExtension.AddConfigProperties(x.Item1, x.Item2));
            //set the label
            if (!string.IsNullOrWhiteSpace(_label))
            {
                CypherExtension.SetConfigLabel(typeof (T), _label);
            }
            return returnValue;
        }

        public ConfigWith<TN> With<TN>(string label=null)
        {
            Set();
            return new ConfigWith<TN>(_context, label);
        }  
    }
}
