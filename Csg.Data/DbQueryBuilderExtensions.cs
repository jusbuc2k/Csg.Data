﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Csg.Data.Sql;
using System.Data;
using System.Collections.ObjectModel;
using System.Data.Common;

namespace Csg.Data
{
    /// <summary>
    /// Provides extension methods for the <see cref="IDbQueryBuilder"/>.
    /// </summary>
    public static partial class DbQueryBuilderExtensions
    {
        /// <summary>
        /// Creates a <see cref="IDbQueryBuilder"/> associated with the given <see cref="IDbScope"/> which can be used to build and execute a SQL statement.
        /// </summary>
        /// <param name="scope">The connection with which the resulting <see cref="IDbQueryBuilder"/> will be associated with.</param>
        /// <param name="commandText">The table expression, table name, or object name to use as the target of the query.</param>
        /// <param name="provider">The SQL text provider to use for query generation. If not specified <see cref="Sql.SqlProviderFactory.GetProvider(Type)"/> will be used.</param>
        /// <returns></returns>
        //public static IDbQueryBuilder QueryBuilder(this Csg.Data.IDbScope scope, string commandText, Abstractions.ISqlProvider provider = null)
        //{
        //    return new DbQueryBuilder(commandText, scope.Connection, scope.Transaction, provider ?? Sql.SqlProviderFactory.GetProvider(scope.Connection));
        //}

        ///// <summary>
        ///// Creates a <see cref="IDbQueryBuilder"/> associated with the given <see cref="IDbConnection"/> which can be used to build and execute a SQL statement.
        ///// </summary>
        ///// <param name="connection">The connection with which the resulting <see cref="IDbCommand"/> will be associated with.</param>
        ///// <param name="commandText">The table expression, table name, or object name to use as the target of the query.</param>
        ///// <param name="provider">The SQL text provider to use for query generation. If not specified <see cref="Sql.SqlProviderFactory.GetProvider(Type)"/> will be used.</param>
        ///// <returns></returns>
        //public static IDbQueryBuilder QueryBuilder(this System.Data.IDbConnection connection, string commandText, Abstractions.ISqlProvider provider = null)
        //{
        //    return new DbQueryBuilder(commandText, connection, transaction: null, provider: provider ?? Sql.SqlProviderFactory.GetProvider(connection));
        //}

        ///// <summary>
        ///// Creates a <see cref="IDbQueryBuilder"/> associated with the given <see cref="IDbTransaction"/> which can be used to build and execute a SQL SELECT statement.
        ///// </summary>
        ///// <param name="transaction">The transaction with which the resulting <see cref="IDbCommand"/> will be associated with.</param>
        ///// <param name="commandText">The table expression, table name, or object name to use as the target of the query.</param>
        ///// <param name="provider">The SQL text provider to use for query generation. If not specified <see cref="Sql.SqlProviderFactory.GetProvider(Type)"/> will be used.</param>
        ///// <returns></returns>
        //public static IDbQueryBuilder QueryBuilder(this System.Data.IDbTransaction transaction, string commandText, Abstractions.ISqlProvider provider = null)
        //{
        //    return new DbQueryBuilder(commandText, transaction.Connection, transaction: transaction, provider: provider ?? Sql.SqlProviderFactory.GetProvider(transaction.Connection));
        //}

        /// <summary>
        /// Executes the query and returns an open data reader.
        /// </summary>
        /// <returns></returns>
        public static IDataReader ExecuteReader(this IDbQueryBuilder query)
        {
            return query.CreateCommand().ExecuteReader();
        }

        /// <summary>
        /// Executes the query and returns the first column from the first row in the result set.
        /// </summary>
        /// <param name="query">The query builder instance.</param>
        /// <returns></returns>
        public static object ExecuteScalar(this IDbQueryBuilder query)
        {
            return query.CreateCommand().ExecuteScalar();
        }

        /// <summary>
        /// Executes the query and returns the first column from the first row in the result set, casted to the given type.
        /// </summary>
        /// <param name="query">The query builder instance.</param>
        public static T ExecuteScalar<T>(this IDbQueryBuilder query) where T : struct
        {
            var value = query.ExecuteScalar();

            if (value is T)
            {
                return (T)value;
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// Executes a T-SQL SELECT statement in format only mode (SET FMTONLY ON) which validates the query and returns the result metadata.
        /// </summary>
        /// <param name="query">The query builder instance.</param>
        /// <param name="schema">Outputs the query metadata.</param>
        /// <param name="errors">Outputs any validation or execution errors encountered.</param>
        /// <returns></returns>
        public static bool GetSchemaTable(this IDbQueryBuilder query, out DataTable schema, out ICollection<Exception> errors)
        {
            var cmd = query.CreateCommand();

            errors = null;
            schema = null;
            
            try
            {
                using (var reader = cmd.ExecuteReader(CommandBehavior.SchemaOnly))
                {
                    if (reader.FieldCount <= 0)
                    {
                        return false;
                    }

                    schema = reader.GetSchemaTable();
                }

                return true;
            }
            catch (Exception ex)
            {
                errors = new List<Exception>();
                errors.Add(ex);
                return false;
            }
        }

        /// <summary>
        /// Executes a T-SQL SELECT statement in format only mode (SET FMTONLY ON) which validates the query and returns the result metadata.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="schema"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public static bool GetColumnSchema(this IDbQueryBuilder query, out ReadOnlyCollection<DbColumn> schema, out ICollection<Exception> errors)
        {
            var cmd = query.CreateCommand();

            errors = null;
            schema = null;

            try
            {
                using (var reader = cmd.ExecuteReader(CommandBehavior.SchemaOnly))
                {
                    if (reader.FieldCount <= 0)
                    {
                        return false;
                    }

                    if (reader is System.Data.Common.IDbColumnSchemaGenerator)
                    {
                        schema = ((System.Data.Common.IDbColumnSchemaGenerator)reader).GetColumnSchema();
                        return true;
                    }
                    else
                    {
                        throw new NotSupportedException("The interface System.Data.Common.IDbColumnSchemaGenerator is not supported by the underlying data provider.");
                    }
                }
            }
            catch (Exception ex)
            {
                errors = new List<Exception>();
                errors.Add(ex);
                return false;
            }
        }

        /// <summary>
        /// Populates the field select list for the resulting SELECT statement with the given field name(s).
        /// </summary>
        /// <param name="query">The query builder instance.</param>
        /// <param name="fields">A set of field names to select.</param>
        /// <returns></returns>
        public static IDbQueryBuilder Select(this IDbQueryBuilder query, params string[] fields)
        {             
            return query.Select((IEnumerable<string>)fields);
        }

        /// <summary>
        /// Populates the field select list for the resulting SELECT statement with the given field names.
        /// </summary>
        /// <param name="query">The query builder instance.</param>
        /// <param name="fields">A set of field names to select.</param>
        /// <returns></returns>
        public static IDbQueryBuilder Select(this IDbQueryBuilder query, IEnumerable<string> fields)
        {
            var fork = query.Fork();

            foreach (var field in fields)
            {
                fork.SelectColumns.Add(SqlColumn.Parse(fork.Root, field));
            }

            return fork;
        }

        /// <summary>
        /// Sets the resulting query to use SELECT DISTINCT.
        /// </summary>
        /// <param name="query">The query builder instance.</param>
        /// <returns></returns>
        public static IDbQueryBuilder Distinct(this IDbQueryBuilder query)
        {
            var fork = query.Fork();
            fork.SelectDistinct = true;
            return fork;
        }

        /// <summary>
        /// Adds a set of WHERE clause conditions defined by the given expression, grouped by logical AND.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static IDbQueryBuilder Where(this IDbQueryBuilder query, Action<IDbQueryWhereClause> expression)
        {
            query = query.Fork();
            var group = new DbQueryWhereClause(query.Root, SqlLogic.And);

            expression.Invoke(group);

            group.ApplyToQuery(query);

            return query;
        }

        /// <summary>
        /// Adds a set of WHERE clause conditions defined by the given expression, grouped by logical OR.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static IDbQueryBuilder WhereAny(this IDbQueryBuilder query, Action<IDbQueryWhereClause> expression)
        {
            query = query.Fork();
            var group = new DbQueryWhereClause(query.Root, SqlLogic.Or);

            expression.Invoke(group);

            query.AddFilter(group.Filters);

            return query;
        }

        /// <summary>
        /// Adds a set of WHERE clause conditions by looping over the given collection, and then joining them together with the given logic.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="collection"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static IDbQueryBuilder WhereAny<TItem>(this IDbQueryBuilder query, IEnumerable<TItem> collection, Action<IDbQueryWhereClause, TItem> expression)
        {
            query = query.Fork();
            var group = new DbQueryWhereClause(query.Root, SqlLogic.Or);

            foreach (var item in collection)
            {
                var innerGroup = new DbQueryWhereClause(query.Root, SqlLogic.And);
                expression.Invoke(innerGroup, item);
                group.AddFilter(innerGroup.Filters);
            }

            query.AddFilter(group.Filters);

            return query;
        }

        /// <summary>
        /// Adds a set of WHERE clause conditions by looping over the given collection, and then joining them together with the given logic.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="collection"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static IDbQueryBuilder WhereAny<TItem>(this IDbQueryBuilder query, IList<TItem> list, Action<IDbQueryWhereClause, TItem, int> expression)
        {
            query = query.Fork();
            var group = new DbQueryWhereClause(query.Root, SqlLogic.Or);

            for(var i = 0; i < list.Count; i++)
            {
                var innerGroup = new DbQueryWhereClause(query.Root, SqlLogic.And);
                expression.Invoke(innerGroup, list[i], i);
                group.AddFilter(innerGroup.Filters);
            }

            query.AddFilter(group.Filters);

            return query;
        }

        /// <summary>
        /// Adds ORDER BY fields to a query for ascending sort.
        /// </summary>
        /// <typeparam name="T">The type of the query builder.</typeparam>
        /// <param name="query">The query builder instance</param>
        /// <param name="fields">A set of field expressions to order the query by.</param>
        /// <returns></returns>
        public static IDbQueryBuilder OrderBy(this IDbQueryBuilder query, params string[] fields) 
        {
            var fork = query.Fork();

            foreach (var field in fields)
            {
                fork.OrderBy.Add(new Csg.Data.Sql.SqlOrderColumn() { ColumnName = field, SortDirection = Csg.Data.Sql.DbSortDirection.Ascending });
            }

            return fork;
        }

        /// <summary>
        /// Adds ORDER BY fields to a query for descending sort.
        /// </summary>
        /// <param name="query">The query builder instance</param>
        /// <param name="fields">A set of field expressions to order the query by.</param>
        /// <returns></returns>
        public static IDbQueryBuilder OrderByDescending(this IDbQueryBuilder query, params string[] fields)
        {
            var fork = query.Fork();
            foreach (var field in fields)
            {
                fork.OrderBy.Add(new Csg.Data.Sql.SqlOrderColumn() { ColumnName = field, SortDirection = Csg.Data.Sql.DbSortDirection.Descending });
            }
            return fork;
        }

        /// <summary>
        /// Sets the execution command timeout for the query.
        /// </summary>
        /// <param name="query">The query builder instance</param>
        /// <param name="timeout">The timeout value in seconds.</param>
        /// <returns></returns>
        public static IDbQueryBuilder Timeout(this IDbQueryBuilder query, int timeout) 
        {
            var fork = query.Fork();
            fork.CommandTimeout = timeout;
            return fork;
        }

        /// <summary>
        /// Adds an arbitrary parameter to the resulting query.
        /// </summary>
        /// <param name="query">The query builder instance</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="dbType">The data type of the parameter.</param>
        /// <param name="size">The size of the parameter.</param>
        /// <returns></returns>
        public static IDbQueryBuilder AddParameter(this IDbQueryBuilder query, string name, object value, DbType dbType, int? size = null)
        {
            var fork = query.Fork();

            fork.Parameters.Add(new DbParameterValue() { 
                ParameterName = name,
                DbType = dbType,                
                Value = value,
                Size = size
            });

            return fork;
        }

    }
}
