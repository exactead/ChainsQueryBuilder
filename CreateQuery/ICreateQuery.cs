using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainsQueryBuilder
{
    interface ICreateQuery
    {
        StringBuilder builder { get; }
        string tableName { get; }
        char parameterSymbol { get; }
        IDictionary<string,object> valueParameter { get; }
    }

    internal class NullQuery : ICreateQuery
    {
        public StringBuilder builder { get; } = new StringBuilder();
        public string tableName => string.Empty;
        public char parameterSymbol => '@';
        public IDictionary<string, object> valueParameter { get; }
    }
}
