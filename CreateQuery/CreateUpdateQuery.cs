using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainsQueryBuilder
{
    internal class CreateUpdateQuery : CreateQuery
    {
        public CreateUpdateQuery(IEnumerable<IFieldValuePair> pairs, string tableName, char parameterSymbol) 
            : base(pairs, tableName, parameterSymbol)
        {
            var manager = new ClauseManager(tableName, parameterSymbol, valueParameter);
            AppendPhrase($"update {tableName} set ");

            SetSetPhrase();

            var wherePairs = pairs.OfType<WhereColumnFieldValuePair>();
            var whereQueries = pairs.OfType<WhereQueryField>();
            if (!wherePairs.Any() && !whereQueries.Any()) return;
            AppendPhrase("where ");
            if (wherePairs.Any()) AppendPhrase(() => manager.CreateWhereClause(wherePairs), " ");

            if (whereQueries.Any()) AppendPhrase(() => whereQueries.Select(x => x.Value.ToString()), " ");
        }

        internal void SetSetPhrase()
        {
            // setの中身を作成、Valueはまだ使わないだし
            AppendPhrase(() => string.Join(", ",
                pairs
                .OfType<ColumnFieldValuePair>()
                .Select(x => $"{(string.IsNullOrEmpty(x.TableName) ? string.Empty : $"{x.TableName}.")}{x.ColumnName} = {parameterSymbol}{x.ColumnName} ")));
        }
    }
}
