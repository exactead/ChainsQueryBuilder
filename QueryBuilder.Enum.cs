using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainsQueryBuilder
{
    public enum NotSetParameter
    {
        None
    }

    public enum ConditionalOperators
    {
        Equal,  // ==
        NotEqual,   // !=
        LessThanOrEqual,    // <=
        GreaterThanOrEqual, // >=
        LessThan,    // <
        GreaterThan, // >
        Like,
        In,
    }

    public enum ConditionalLogicalOperators
    {
        None = 0,
        And,
        Or,
        AndNot,
        OrNot
    }

    public enum OrderByDirection
    {
        Ascending = 0,
        Descending
    }

    /// <summary>
    /// クエリの種類
    /// </summary>
    public enum QueryType
    {
        Select,
        Insert,
        Update,
        Delete
    }

    public enum JoinType
    {
        InnerJoin,
        LeftOuterJoin,
        RightOuterJoin,
        FullOuterJoin,
        CrossJoin
    }

    public enum AggregateContents
    {
        Count,
        Max,
        Min,
        Avg,
        Sum
    }
}
