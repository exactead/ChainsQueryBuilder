using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainsQueryBuilder
{
#pragma warning disable 8618,8625
    public interface IFieldValuePair
    {
        string TableName { get; }
        string ColumnName { get; }
        object? Value { get; }
    }

    public interface IConditionalFieldValuePair : IFieldValuePair
    {
        ConditionalOperators Operator { get; }
        ConditionalLogicalOperators ConditionalType { get; }
    }

    public abstract class SelectColumnFieldValuePair : IFieldValuePair
    {
        public string TableName { get; }
        public string ColumnName { get; }
        public object? Value { get; set; }

        public SelectColumnFieldValuePair(string tableName, string columnName, object? value = null)
        {
            this.TableName = tableName;
            this.ColumnName = columnName;
        }
    }

    public sealed class ColumnFieldValuePair : SelectColumnFieldValuePair
    {
        public ColumnFieldValuePair(string tableName, string columnName, object value = null) : base(tableName, columnName, value)
        {
            this.Value = value;
        }
    }

    public sealed class AggregateColumnFieldValuePair : SelectColumnFieldValuePair
    {
        public AggregateColumnFieldValuePair(string tableName, string columnName, object value) : base(tableName, columnName, value)
        {
            if(value is null) throw new ArgumentNullException(nameof(value));
            this.Value = (AggregateContents)value;
        }
    }

    public sealed class WhereColumnFieldValuePair : IConditionalFieldValuePair
    {
        public string TableName { get; }
        public string ColumnName { get; }
        public object? Value { get; }
        public ConditionalOperators Operator { get; }
        public ConditionalLogicalOperators ConditionalType { get; }

        public WhereColumnFieldValuePair
            ( string tableName, string columnName,ConditionalOperators conditionalOperators = ConditionalOperators.Equal,
            ConditionalLogicalOperators conditionalType = ConditionalLogicalOperators.None, object? value = null)
        {
            this.ColumnName = columnName;
            this.TableName = tableName;
            this.Value = value;
            this.Operator = conditionalOperators;
            this.ConditionalType = conditionalType;
        }
    }

    public sealed class HavingColumnFieldValuePair : IConditionalFieldValuePair
    {
        public string ColumnName { get; }
        public string TableName { get; }
        public object? Value { get; }
        public ConditionalOperators Operator { get; }
        public ConditionalLogicalOperators ConditionalType { get; }

        public HavingColumnFieldValuePair
            (string tableName, string columnName, ConditionalOperators conditionalOperators = ConditionalOperators.Equal,
            ConditionalLogicalOperators conditionalType = ConditionalLogicalOperators.None, object value = null)
        {
            this.ColumnName = columnName;
            this.TableName = tableName;
            this.Value = value;
            this.Operator = conditionalOperators;
            this.ConditionalType = conditionalType;
        }
    }

    public sealed class OrderColumnFieldValuePair : IFieldValuePair
    {
        public string TableName { get; }
        public string ColumnName { get; }
        public object? Value => string.Empty; // Dummy
        public OrderByDirection Direction { get; }

        public OrderColumnFieldValuePair(string tableName, string columnName, OrderByDirection direction = OrderByDirection.Ascending)
        {
            this.TableName = tableName;
            this.ColumnName = columnName;
            this.Direction = direction;
        }
    }

    public class JoinFieldBase : IFieldValuePair
    {
        public string ColumnName => string.Empty; // Dummy
        public object Value { get; }
        public JoinType JoinType { get; }
        public string TableName { get; }

        public JoinFieldBase(string tableName, JoinType joinType, IEnumerable<(string joinBaseTargetTableColumn, string tableColumn)> columnsInfo)
        {
            this.TableName = tableName;
            this.Value = columnsInfo;
            this.JoinType = joinType;
        }

        public JoinFieldBase(string tableName, JoinType joinType, IEnumerable<(ColumnFieldPair joinBaseColumn, string columnName)> columnsInfo)
        {
            this.TableName = tableName;
            this.Value = columnsInfo;
            this.JoinType = joinType;
        }
    }

    public sealed class JoinFieldValuePair : JoinFieldBase
    {
        public string TargetTable { get; }

        public JoinFieldValuePair(string tableName, JoinType joinType, string targetTable, IEnumerable<(string joinBaseTargetTableColumn, string tableColumn)> columnsInfo) : base(tableName, joinType, columnsInfo)
        {
            this.TargetTable = targetTable;
        }
    }

    public sealed class JoinFieldSubQueryPair : JoinFieldBase
    {
        public string TargetTable { get; }
        public string SubQuery { get; }

        public JoinFieldSubQueryPair(string tableName, JoinType joinType, string targetTable, string query, IEnumerable<(string joinBaseTargetTableColumn, string tableColumn)> columnsInfo) : base(tableName, joinType, columnsInfo)
        {
            this.SubQuery = query;
        }
    }

    public sealed class JoinFieldColumnPair : JoinFieldBase
    {
        public JoinFieldColumnPair(string tableName, JoinType joinType, IEnumerable<(ColumnFieldPair joinBaseColumn, string columnName)> columnsInfo) : base(tableName, joinType, columnsInfo)
        {
        }
    }

    public sealed class ColumnFieldPair : IFieldValuePair
    {
        public string TableName { get; }
        public string ColumnName { get; }
        public object Value { get; } = null;

        public ColumnFieldPair(string tableName, string columnName)
        {
            this.TableName = tableName;
            this.ColumnName = columnName;
        }
    }

    public abstract class QueryField : IFieldValuePair
    {
        public string TableName => string.Empty;
        public string ColumnName => string.Empty;
        public object Value { get; }

        public QueryField(string query)
        {
            this.Value = query;
        }
    }

    public sealed class SelectQueryField : QueryField
    {
        public SelectQueryField(string query) : base(query) { }
    }

    public sealed class WhereQueryField : QueryField
    {
        public WhereQueryField(string query) : base(query) { }
    }

    public sealed class HavingQueryField : QueryField
    {
        public HavingQueryField(string query) : base(query) { }
    }
}
