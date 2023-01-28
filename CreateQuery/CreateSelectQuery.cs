using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainsQueryBuilder
{
#pragma warning disable 8619
    internal class CreateSelectQuery : CreateQuery
    {
        ClauseManager manager;

        /// <summary>
        /// Select構文を生成します
        /// </summary>
        /// <param name="pairs">カラム情報</param>
        /// <param name="tableName">取得テーブル名称</param>
        /// <param name="parameterSymbol">バインドパラメータ</param>
        public CreateSelectQuery(IEnumerable<IFieldValuePair> pairs, string tableName, char parameterSymbol)
            : base(pairs, tableName, parameterSymbol)
        {
            manager = new ClauseManager(tableName, parameterSymbol, valueParameter);

            // TypeSeparete
            var selectColumns = pairs.OfType<ColumnFieldValuePair>();
            var aggregatetColumns = pairs.OfType<AggregateColumnFieldValuePair>();
            var whereConditions = pairs.OfType<WhereColumnFieldValuePair>();
            var havingConditions = pairs.OfType<HavingColumnFieldValuePair>();

            var selectQueries = pairs.OfType<SelectQueryField>();
            var whereQueries = pairs.OfType<WhereQueryField>();
            var havingQueries = pairs.OfType<HavingQueryField>();
            var joinColumns = pairs.OfType<JoinFieldBase>();
            var orderColumns = pairs.OfType<OrderColumnFieldValuePair>();

            Select(selectColumns, aggregatetColumns, selectQueries);

            From();

            Join(joinColumns);

            Where(whereConditions, whereQueries);

            GroupBy(selectColumns, aggregatetColumns);

            Having(havingConditions, havingQueries, aggregatetColumns);

            OrderBy(orderColumns);
        }

        private void Select
            (IEnumerable<ColumnFieldValuePair> selectColumns, IEnumerable<AggregateColumnFieldValuePair> aggregatetColumns,
            IEnumerable<SelectQueryField> selectQueries)
        {
            // Column
            AppendPhrase($"select ");
            if (selectColumns.Any())
            {
                AppendPhrase(() => manager.CreateSelectColumnClause(selectColumns), ", ");
            }
            if (aggregatetColumns.Any())
            {
                if (selectColumns.Any()) AppendPhrase(", ");
                AppendPhrase(() => manager.CreateSelectAggregateColumnClause(aggregatetColumns), ", ");
            }
            if (selectQueries.Any())
            {
                if (selectColumns.Any() || aggregatetColumns.Any()) AppendPhrase(", ");
                AppendPhrase(() => manager.CreateSelectSubQueryColumnClause(selectQueries), ", ");
            }

            AppendPhrase(() => $" ");
        }

        private void From()
        {
            // From
            AppendPhrase(() => $"from {tableName} ");
        }

        private void Join(IEnumerable<JoinFieldBase> joinColumns)
        {
            // Join
            if (joinColumns.Any())
            {
                AppendPhrase(() => manager.CreateJoinClause(joinColumns));
            }
        }

        private void Where(IEnumerable<WhereColumnFieldValuePair> whereConditions, IEnumerable<WhereQueryField> whereQueries)
        {
            // Where
            if (!whereConditions.Any() && !whereQueries.Any()) return;

            AppendPhrase("where ");
            if (whereConditions.Any())
            {
                AppendPhrase(() => manager.CreateWhereClause(whereConditions));
            }
            if (whereQueries.Any())
            {
                AppendPhrase(() => whereQueries.Select(x => x.Value.ToString()));
            }
            AppendPhrase(" ");
        }

        private void GroupBy
            (IEnumerable<ColumnFieldValuePair> selectColumns, IEnumerable<AggregateColumnFieldValuePair> aggregatetColumns)
        {
            // GroupBy
            if (!selectColumns.Any() || !aggregatetColumns.Any()) return;

            AppendPhrase("group by ");
            AppendPhrase(() => manager.CreateGroupByClause(selectColumns), ", ");
            AppendPhrase(" ");
        }

        private void Having
            (IEnumerable<HavingColumnFieldValuePair> havingConditions,
            IEnumerable<HavingQueryField> havingQueries,
            IEnumerable<AggregateColumnFieldValuePair> aggregatetColumns)
        {
            // Having
            if (!havingConditions.Any() && !havingQueries.Any()) return;

            AppendPhrase("having ");
            if (havingConditions.Any())
            {
                AppendPhrase(() => manager.CreateHavingClause(havingConditions, aggregatetColumns));
            }
            if (havingQueries.Any())
            {
                AppendPhrase(() => havingQueries.Select(x => x.Value.ToString()));
            }
        }

        private void OrderBy(IEnumerable<OrderColumnFieldValuePair> orderColumns)
        {
            // Orderby
            if (orderColumns.Any())
            {
                AppendPhrase("order by ");
                AppendPhrase(() => manager.CreateOrderByClause(orderColumns), ", ");
            }
        }
    }
}
