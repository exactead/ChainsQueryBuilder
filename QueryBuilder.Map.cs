using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainsQueryBuilder
{
#pragma warning disable 8602
    public partial class QueryBuilder
    {
        internal QueryBuilder Select
        {
            get
            {
                queryType = QueryType.Select;
                return this;
            }
        }

        internal QueryBuilder Insert
        {
            get
            {
                queryType = QueryType.Insert;
                return this;
            }
        }

        internal QueryBuilder Update
        {
            get
            {
                queryType = QueryType.Update;
                return this;
            }
        }

        internal QueryBuilder Delete
        {
            get
            {
                queryType = QueryType.Delete;
                return this;
            }
        }

        public QueryBuilder AddColumn(SelectColumnFieldValuePair column)
        {
            columnValues.Add(column);
            return this;
        }

        public QueryBuilder AddColumn(string tableName, string columnName, object? value = null)
        {
            SelectColumnFieldValuePair column;
            if (value is AggregateContents)
            {
                column = new AggregateColumnFieldValuePair(InquiryTableName(tableName), columnName, (AggregateContents)value);
            }
            else if (value is null)
            {
                object content;
                if (queryType == QueryType.Select || queryType == QueryType.Delete)
                {
                    content = NotSetParameter.None;
                }
                else
                {
                    content = DBNull.Value;
                }
                column = new ColumnFieldValuePair(InquiryTableName(tableName), columnName, content);
            }
            else
            {
                string table = !string.IsNullOrEmpty(tableName) ? tableName : !string.IsNullOrEmpty(this.tableName) ? this.tableName: string.Empty;
                column = new ColumnFieldValuePair(InquiryTableName(table), columnName, value);
            }

            return AddColumn(column);
        }

        /// <summary>
        /// Adding a Column of 'From Table'
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public QueryBuilder AddColumnWithDefaultTableName(string columnName, object? value = null) => AddColumn(string.Empty, columnName, value);
        
        internal QueryBuilder From(string tableName)
        {
            if (!string.IsNullOrEmpty(this.tableName)) throw new ArgumentException($"From:{nameof(tableName)}");
            this.tableName = tableName;
            return this;
        }

        public QueryBuilder Join(JoinType joinType, string tableName, string targetTable, IEnumerable<(string, string)> targetColumns)
        {
            if (targetTable is null) throw new ArgumentNullException(nameof(targetTable));
            if (targetColumns is null) throw new ArgumentNullException(nameof(targetColumns));

            columnValues.Add(new JoinFieldValuePair(InquiryTableName(tableName), joinType, targetTable, targetColumns));
            return this;
        }

        public QueryBuilder JoinWithSubQuery(JoinType joinType, string tableName, string targetTable, string query, IEnumerable<(string, string)> targetColumns)
        {
            if (targetTable is null) throw new ArgumentNullException(nameof(targetTable));
            if (targetColumns is null) throw new ArgumentNullException(nameof(targetColumns));
            if (query is null) throw new ArgumentNullException(nameof(query));

            columnValues.Add(new JoinFieldSubQueryPair(InquiryTableName(tableName), joinType, targetTable, query, targetColumns));
            return this;
        }

        public QueryBuilder JoinMultiTable(JoinType joinType, string tableName, IEnumerable<(ColumnFieldPair targetTableColumn, string thisTableColumn)> targetColumns)
        {
            if (targetColumns is null) throw new ArgumentNullException(nameof(targetColumns));

            columnValues.Add(new JoinFieldColumnPair(InquiryTableName(tableName), joinType, targetColumns));
            return this;
        }

        public QueryBuilder Where(string predicate)
        {
            columnValues.Add(new WhereQueryField(predicate));
            return this;
        }

        public QueryBuilder Where(string columnName, ConditionalOperators @operator, ConditionalLogicalOperators conjunction, object value)
            => Where(string.Empty, columnName, @operator, conjunction, value);

        public QueryBuilder Where(string tableName, string columnName, ConditionalOperators @operator, ConditionalLogicalOperators conjunction, object value)
        {
            columnValues.Add(new WhereColumnFieldValuePair(InquiryTableName(tableName), columnName, @operator, conjunction, value));
            return this;
        }

        public QueryBuilder OrderBy(string tableName, string columnName, OrderByDirection direction = OrderByDirection.Ascending)
        {
            columnValues.Add(new OrderColumnFieldValuePair(InquiryTableName(tableName), columnName, direction));
            return this;
        }

        public QueryBuilder Having(string tableName, string columnName, ConditionalOperators @operator, ConditionalLogicalOperators conjunction, object value)
        {
            columnValues.Add(new HavingColumnFieldValuePair(InquiryTableName(tableName), columnName, @operator, conjunction, value));
            return this;
        }

        public QueryBuilder Having(string query)
        {
            columnValues.Add(new HavingQueryField(query));
            return this;
        }

        private string InquiryTableName(string tableName) => string.IsNullOrWhiteSpace(tableName) ? this.tableName : tableName;
    }

    public class QueryMap
    {
        public static QueryBuilder SelectFrom(string tableName) => new QueryBuilder().From(tableName).Select;

        public static QueryBuilder InsertFrom(string tableName) => new QueryBuilder().From(tableName).Insert;

        public static QueryBuilder UpdateFrom(string tableName) => new QueryBuilder().From(tableName).Update;

        public static QueryBuilder DeleteFrom(string tableName) => new QueryBuilder().From(tableName).Delete;
    }
}
