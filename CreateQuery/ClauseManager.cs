using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainsQueryBuilder
{
    internal class ClauseManager
    {
        private readonly char parameterSymbol;

        private readonly string tableName;

        private IDictionary<string, object> bindStatements;

        public ClauseManager(string tableName, char parameterSymbol, IDictionary<string, object> valueParameter)
        {
            this.parameterSymbol = parameterSymbol;
            this.tableName = tableName;
            this.bindStatements = valueParameter;
        }

        #region SelectClause
        public IEnumerable<string> CreateSelectColumnClause(IEnumerable<ColumnFieldValuePair> pairs)
        {
            foreach (var x in pairs)
            {
                yield return $"{x.TableName}.{x.ColumnName}";
            }
        }

        public IEnumerable<string> CreateSelectAggregateColumnClause(IEnumerable<AggregateColumnFieldValuePair> pairs)
        {
            foreach (var x in pairs)
            {
                if(x.Value is AggregateContents)
                {
                    yield return $"{GetAggregation(x.TableName, x.ColumnName, (AggregateContents)x.Value)}";
                }
            }
        }

        public IEnumerable<string> CreateSelectSubQueryColumnClause(IEnumerable<SelectQueryField> pairs)
        {
            foreach (var x in pairs)
            {
                yield return $"{(x.Value)}";
            }
        }
        #endregion

        #region JoinClause
        public IEnumerable<string> CreateJoinClause(IEnumerable<JoinFieldBase> pairs)
        {
            foreach (var value in pairs)
            {
                string GetJoin(JoinType joinType)
                {
                    switch (joinType)
                    {
                        case JoinType.InnerJoin: return "Inner Join";
                        case JoinType.LeftOuterJoin: return "Left Outer Join";
                        case JoinType.RightOuterJoin: return "Right Outer Join";
                        case JoinType.FullOuterJoin: return "Full Outer Join";
                        case JoinType.CrossJoin: return "Cross Join";
                        default: return string.Empty;
                    }
                };

                if (value is JoinFieldValuePair joinFieldValuePair)
                {
                    var onColumns = ((IEnumerable<(string joinBaseTargetTableColumn, string tableColumn)>)joinFieldValuePair.Value)
                        .Select(x => $"{joinFieldValuePair.TargetTable}.{x.joinBaseTargetTableColumn} = {joinFieldValuePair.TableName}.{x.tableColumn}");
                    yield return joinFieldValuePair.JoinType == JoinType.CrossJoin ?
                        $"{GetJoin(joinFieldValuePair.JoinType)} {joinFieldValuePair.TableName} "
                        : $"{GetJoin(joinFieldValuePair.JoinType)} {joinFieldValuePair.TableName} on {string.Join(" and ", onColumns)} ";
                }
                else if (value is JoinFieldSubQueryPair joinFieldSubQueryPair)
                {
                    var onColumns = ((IEnumerable<(string joinBaseTargetTableColumn, string tableColumn)>)joinFieldSubQueryPair.Value)
                        .Select(x => $"{joinFieldSubQueryPair.TargetTable}.{x.joinBaseTargetTableColumn} = {joinFieldSubQueryPair.TableName}.{x.tableColumn}");
                    yield return $"{GetJoin(joinFieldSubQueryPair.JoinType)} ({joinFieldSubQueryPair.SubQuery}) on {string.Join(" and ", onColumns)} ";
                }
                else if (value is JoinFieldColumnPair joinFieldColumnPair)
                {
                    var onColumns = ((IEnumerable<(ColumnFieldPair joinBaseColumn, string columnName)>)joinFieldColumnPair.Value)
                        .Select(x => $"{x.joinBaseColumn.TableName}.{x.joinBaseColumn.ColumnName} = {joinFieldColumnPair.TableName}.{x.columnName}");
                    yield return joinFieldColumnPair.JoinType == JoinType.CrossJoin ?
                        $"{GetJoin(joinFieldColumnPair.JoinType)} {joinFieldColumnPair.TableName} "
                        : $"{GetJoin(joinFieldColumnPair.JoinType)} {joinFieldColumnPair.TableName} on {string.Join(" and ", onColumns)} ";
                }
            }
        }
        #endregion

        #region WhereClause
        public IEnumerable<string> CreateWhereClause(IEnumerable<WhereColumnFieldValuePair> conditions)
        {
            // Where句をつくる
            var array = conditions.ToArray();

            int i = 0;
            while (i < array.Length)
            {
                // ローカル関数として宣言
                string WhereCondition(WhereColumnFieldValuePair pair, string bind)
                {
                    string whereTableName = string.IsNullOrWhiteSpace(array[i].TableName) ? tableName : array[i].TableName;
                    string phrase = $"{GetConditionalOperator(pair.ConditionalType, i == 0)}{whereTableName}.{pair.ColumnName} {GetOperator(pair)} ";
                    phrase += pair.Value == null ? string.Empty
                        : pair.Value is QueryBuilder ? $"({pair.Value}) "
                        : pair.Value is Array && pair.Operator == ConditionalOperators.In ? $"('{string.Join("','", pair.Value)}')" // ''なしでもバインドできるか
                        : pair.Operator == ConditionalOperators.In ? $"({pair.Value})" 
                        : $"{parameterSymbol}{bind} ";
                    return phrase;
                }
                yield return WhereCondition(array[i], GetBindStatement(array[i]));
                i++;
            }
        }
        #endregion
        
        #region HavingClause
        public IEnumerable<string> CreateHavingClause
            (IEnumerable<HavingColumnFieldValuePair> conditions, IEnumerable<AggregateColumnFieldValuePair> aggregates)
        {
            // Having句をつくる
            var array = conditions.ToArray();
            int i = 0;
            while (i < array.Length)
            {
                string HavingCondition(HavingColumnFieldValuePair pair, AggregateColumnFieldValuePair targetpair, string bind)
                {
                    string phrase = $"{GetConditionalOperator(pair.ConditionalType, i == 0)}" +
                        $"{GetAggregation(pair.TableName, pair.ColumnName, (AggregateContents)targetpair.Value)} {GetOperator(array[i])} ";
                    phrase += pair.Value == null ? string.Empty : $"{parameterSymbol}{bind} ";
                    return phrase;
                }

                AggregateColumnFieldValuePair GetTargetPairData() =>
                    aggregates.Where(x => x.TableName == array[i].TableName)
                        .FirstOrDefault(x => x.ColumnName == array[i].ColumnName);

                yield return HavingCondition(array[i], GetTargetPairData(), GetBindStatement(array[i]));
                i++;
            }
        }
        #endregion
        
        #region GroupByClause
        public IEnumerable<string> CreateGroupByClause(IEnumerable<ColumnFieldValuePair> pairs)
        {
            // 要確認：集計関数の外出も対象か
            foreach (var value in pairs)
            {
                yield return $"{value.TableName}.{value.ColumnName}";
            }
        }
        #endregion
        
        #region OrderByClause
        public IEnumerable<string> CreateOrderByClause(IEnumerable<OrderColumnFieldValuePair> pairs)
        {
            string GetDirection(OrderByDirection direction) =>
                direction == OrderByDirection.Ascending ? "asc" : "desc";

            // OrderBy句をつくる
            foreach (var pairValue in pairs)
            {
                yield return $"{pairValue.TableName}.{pairValue.ColumnName} {GetDirection(pairValue.Direction)}";
            }
        }
        #endregion


        private string GetAggregation(string tableName, string columnName, AggregateContents aggregate)
        {
            switch (aggregate)
            {
                case AggregateContents.Count when !string.IsNullOrWhiteSpace(tableName):
                    return $"Count({tableName}.{columnName})";
                case AggregateContents.Count:
                    return $"Count({columnName})";
                case AggregateContents.Max when !string.IsNullOrWhiteSpace(tableName):
                    return $"Max({tableName}.{columnName})";
                case AggregateContents.Max:
                    return $"Max({columnName})";
                case AggregateContents.Min when !string.IsNullOrWhiteSpace(tableName):
                    return $"Min({tableName}.{columnName})";
                case AggregateContents.Min:
                    return $"Min({columnName})";
                case AggregateContents.Avg when !string.IsNullOrWhiteSpace(tableName):
                    return $"Avg({tableName}.{columnName})";
                case AggregateContents.Avg:
                    return $"Avg({columnName})";
                case AggregateContents.Sum when !string.IsNullOrWhiteSpace(tableName):
                    return $"Sum({tableName}.{columnName})";
                case AggregateContents.Sum:
                    return $"Sum({columnName})";
                default: return string.Empty;
            }
        }

        protected string GetBindStatement(IConditionalFieldValuePair pairValue)
        {
            var prefix = pairValue is WhereColumnFieldValuePair ? "wp_" : "hp_";
            return bindStatements
                    .Where(x => x.Key.Contains($"{prefix}{pairValue.ColumnName}"))
                    .FirstOrDefault(x => x.Value == pairValue.Value)
                    .Key;
        }

        protected string GetOperator(IConditionalFieldValuePair value)
        {
            switch (value.Operator)
            {
                case ConditionalOperators.Equal when value.Value == null: return "is null";
                case ConditionalOperators.Equal: return "=";
                case ConditionalOperators.NotEqual when value.Value == null: return "is not null";
                case ConditionalOperators.NotEqual: return "!=";
                case ConditionalOperators.LessThanOrEqual: return "<=";
                case ConditionalOperators.GreaterThanOrEqual: return ">=";
                case ConditionalOperators.LessThan: return "<";
                case ConditionalOperators.GreaterThan: return ">";
                case ConditionalOperators.Like: return "like";
                case ConditionalOperators.In: return "in";
                default: return string.Empty;
            }
        }

        protected string GetFirstConditionalOperator(ConditionalLogicalOperators value)
        {
            switch (value)
            {
                case ConditionalLogicalOperators.AndNot:
                case ConditionalLogicalOperators.OrNot:
                    return "not ";
                case ConditionalLogicalOperators.And:
                case ConditionalLogicalOperators.Or:
                default:
                    return string.Empty;
            }
        }

        protected string GetConditionalOperator(ConditionalLogicalOperators value, bool IsFirstOperator =false)
        {
            switch (value)
            {
                case ConditionalLogicalOperators.And when IsFirstOperator:
                case ConditionalLogicalOperators.Or when IsFirstOperator: return string.Empty;
                case ConditionalLogicalOperators.And: return "and ";
                case ConditionalLogicalOperators.Or: return "or ";
                case ConditionalLogicalOperators.AndNot when IsFirstOperator:
                case ConditionalLogicalOperators.OrNot when IsFirstOperator: return "not ";
                case ConditionalLogicalOperators.AndNot: return "and not ";
                case ConditionalLogicalOperators.OrNot: return "or not ";
                default: return string.Empty;
            }
        }


    }
}
