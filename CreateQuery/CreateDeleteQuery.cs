using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainsQueryBuilder
{
    internal class CreateDeleteQuery : CreateQuery
    {
#pragma warning disable 8619
        public CreateDeleteQuery(IEnumerable<IFieldValuePair> pairs, string tableName, char parameterSymbol) : base(pairs, tableName, parameterSymbol)
        {
            var manager = new ClauseManager(tableName, parameterSymbol, valueParameter);
            AppendPhrase($"delete from {tableName} ");

            var wherePairs = pairs.OfType<WhereColumnFieldValuePair>();
            var whereQueries = pairs.OfType<WhereQueryField>();
            if (!wherePairs.Any() && !whereQueries.Any()) return;
            AppendPhrase("where ");
            if (wherePairs.Any()) AppendPhrase(() => manager.CreateWhereClause(pairs.OfType<WhereColumnFieldValuePair>()));

            if (whereQueries.Any()) AppendPhrase(() => whereQueries.Select(x => x.Value.ToString()), " ");
        }
    }
}
