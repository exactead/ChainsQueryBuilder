using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainsQueryBuilder
{
    internal abstract class CreateQuery : ICreateQuery
    {
        public StringBuilder builder { get; } = new StringBuilder();
        public string tableName { get; }
        public char parameterSymbol { get; }
        public IDictionary<string,object> valueParameter { get; internal set; }
        internal IEnumerable<IFieldValuePair> pairs { get; }
        
        protected CreateQuery(IEnumerable<IFieldValuePair> pairs, string tableName, char parameterSymbol)
        {
            this.tableName = tableName;
            this.pairs = pairs ?? new List<IFieldValuePair>();
            this.parameterSymbol = parameterSymbol;
            SetParameter();
        }

        protected void SetParameter()
        {
            valueParameter = pairs
                .Where(x => x.Value != null)
                .Where(x => x.GetType() == typeof(WhereColumnFieldValuePair)
                || x.GetType() == typeof(HavingColumnFieldValuePair)
                || (x.GetType() == typeof(ColumnFieldValuePair) && x.Value.GetType() != typeof(NotSetParameter)))
                .Select((x, idx) => new { Indexer = idx, x.ColumnName, x.Value, Type = x.GetType(), Pair = x })
                .ToDictionary(x => x.Type == typeof(WhereColumnFieldValuePair) ? $"wp_{x.ColumnName}{x.Indexer}"
                : x.Type == typeof(HavingColumnFieldValuePair) ? $"hp_{x.ColumnName}{x.Indexer}"
                : x.ColumnName,
                x => (x.Pair is WhereColumnFieldValuePair
                || x.Pair is HavingColumnFieldValuePair) && (x.Pair as IConditionalFieldValuePair).Operator == ConditionalOperators.Like ?
                $"%{x.Value}%" : x.Value);
        }

        protected void AppendPhrase(string phrase)
        {
            this.AppendPhrase(() => phrase);
        }

        protected void AppendPhrase(Func<string> phrase)
        {
            builder.Append(phrase());
        }

        protected void AppendPhrase(Func<IEnumerable<string>> phrases)
        {
            foreach (var phrase in phrases())
            {
                builder.Append(phrase);
            }
        }

        protected void AppendPhrase(Func<IEnumerable<string>> phrases, string separetor)
        {
            if (phrases == null) throw new ArgumentNullException(nameof(phrases));
            // 遅延評価にするとNullになるためToArray()で評価
            var array = phrases().ToArray();
            if (!array.Any()) return;
            builder.Append(array[0]);
            foreach (var phrase in array.Skip(1))
            {
                builder.Append(separetor);
                builder.Append(phrase);
            }
        }
    }
}
