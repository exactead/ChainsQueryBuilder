using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainsQueryBuilder
{
    internal class CreateInsertQuery : CreateQuery
    {
        public CreateInsertQuery(IEnumerable<IFieldValuePair> pairs, string tableName, char parameterSymbol) : base(pairs, tableName, parameterSymbol)
        {
            AppendPhrase(() => $"insert into {tableName} ");
            AppendPhrase(() => $"({string.Join(", ", pairs.OfType<ColumnFieldValuePair>().Select(x => x.ColumnName))}) ");
            AppendPhrase(() => $"values ({string.Join(", ", pairs.OfType<ColumnFieldValuePair>().Select(x => parameterSymbol + x.ColumnName))})");
        }

    }
}
