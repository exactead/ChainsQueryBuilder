using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainsQueryBuilder
{
    public partial class QueryBuilder
    {
 #pragma warning disable 8602,8604,8619
        private ICreateQuery? que { get; set; }
        private IEnumerable<IFieldValuePair>? pairValues { get; set; }
        private string tableName { get; set; } = string.Empty;
        private char parameterSymbol { get; } = ':';
        private QueryType queryType { get; set; }
        private IList<IFieldValuePair>? columnValues { get; set; }
        
        public QueryBuilder(char parameterSymbol = ':')
        {
            this.parameterSymbol = parameterSymbol;
            columnValues = new List<IFieldValuePair>();
        }

        public QueryBuilder(IEnumerable<IFieldValuePair> pairs, string tableName, QueryType queryType = QueryType.Select, char parameterSymbol = ':')
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentNullException(nameof(tableName));
            this.pairValues = pairs ?? throw new ArgumentNullException(nameof(pairs));
            this.tableName = tableName;
            this.parameterSymbol = parameterSymbol;
            this.queryType = queryType;
        }

        private void CreateQueryConstructor()
        {
            switch (queryType)
            {
                case QueryType.Select:
                    que = new CreateSelectQuery(pairValues, tableName, parameterSymbol);
                    break;
                case QueryType.Insert:
                    que = new CreateInsertQuery(pairValues, tableName, parameterSymbol);
                    break;
                case QueryType.Update:
                    que = new CreateUpdateQuery(pairValues, tableName, parameterSymbol);
                    break;
                case QueryType.Delete:
                    que = new CreateDeleteQuery(pairValues, tableName, parameterSymbol);
                    break;
                default:
                    que = new NullQuery();
                    break;
            }
        }
        
        public string? ToQueryString()
        {
            if (columnValues != null) pairValues = columnValues;
            CreateQueryConstructor();
            return que?.builder.ToString().TrimEnd();
        }
        
        public object GetParameters()
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (var value in que.valueParameter)
            {
                expando.Add(value);
            }
            return expando;
        }

        public override string ToString() => $"tableName:{this.tableName}, queryType:{this.queryType.ToString()}, paramsCount:{que.valueParameter.Count}";
    }
}
