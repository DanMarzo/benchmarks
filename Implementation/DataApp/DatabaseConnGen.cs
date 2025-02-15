using Dapper;
using DataApp.Contracts;
using DataApp.ExpressionsConfig;
using DataApp.Generators;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;

namespace DataApp;

public class DatabaseConnGen : IDatabaseConnGen
{
    public DatabaseConnGen(DbConnection connection)
    {
        this.connection = connection;
    }

    private DbConnection? connection = null;
    private DbTransaction? transaction = null;
    private async Task ConnectionOpeningAsync()
    {
        if (this.connection is null)
            throw new DataException("Not exists connection");

        if (this.connection.State == ConnectionState.Closed)
            await this.connection.OpenAsync();
    }
    public async Task<int> ExecuteAsync(string query, object param)
    {
        await ConnectionOpeningAsync();
        var rows = await this.connection!.ExecuteAsync(query, param, this.transaction);
        return rows;
    }

    public async Task<T?> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> where) where T : class
    {
        await ConnectionOpeningAsync();
        var expressions = new ExpressionOverride(where);
        var parameters = expressions.GenerateWhere();
        var select = string.Join(" ", typeof(T).SelectSQL(), parameters.Where);
        DynamicParameters dynamicParameters = new DynamicParameters();
        foreach (var paremeter in parameters.Parameters)
            dynamicParameters.Add(paremeter.Key, paremeter.Value);
        T? result = await this.connection!.QueryFirstOrDefaultAsync<T>(select, dynamicParameters, this.transaction);
        return result;
    }

    public async Task<T?> FirstOrDefaultAsync<T>(string query)
    {
        await ConnectionOpeningAsync();
        T? result = await this.connection!.QueryFirstOrDefaultAsync<T>(query, null, this.transaction);
        return result;
    }

    public async Task<T?> FirstOrDefaultAsync<T>(string query, object param)
    {
        await ConnectionOpeningAsync();
        T? result = await this.connection!.QueryFirstOrDefaultAsync<T>(query, param, this.transaction);
        return result;
    }

    public async Task<IEnumerable<T>> ManyOrEmptyAsync<T>(Expression<Func<T, bool>> where) where T : class
    {
        await this.ConnectionOpeningAsync();
        var expression = new ExpressionOverride(where);
        var parameters = expression.GenerateWhere();
        IEnumerable<T> result = await this.connection!.QueryAsync<T>(string.Join(" ", typeof(T).SelectSQL(), parameters.Where), parameters.Parameters, this.transaction);
        return result;
    }

    public async Task<IEnumerable<T>> ManyOrEmptyAsync<T>(string query, object param)
    {
        IEnumerable<T> result = await this.connection!.QueryAsync<T>(query, param, this.transaction);
        return result;
    }
    public async Task<IEnumerable<T>> ManyOrEmptyAsync<T>(string query)
    {
        IEnumerable<T> result = await this.connection!.QueryAsync<T>(query, null, this.transaction);
        return result;
    }

    #region  Transacao

    public async Task BeginTransaction()
    {
        if (this.connection is null)
            throw new DataException("Conn not exists");

        if (this.InTransaction())
            return;

        if (this.connection.State == ConnectionState.Closed)
            await this.connection.OpenAsync();
        this.transaction = await this.connection.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (!this.InTransaction())
            throw new DataException("Não existe transação ativa.");
        await this.transaction.CommitAsync();
    }
    public async Task Rollback()
    {
        if (!this.InTransaction())
            throw new DataException("Não existe transação aberta");
        await this.transaction.RollbackAsync();
    }

    private bool InTransaction()
    {
        return this.transaction is not null && this.transaction.Connection is not null;
    }

    #endregion


    public async ValueTask DisposeAsync()
    {

        if (this.connection is not null)
        {
            await this.connection.DisposeAsync();
            this.connection = null;
        }

        if (this.InTransaction())
        {
            await this.transaction.DisposeAsync();
            this.transaction = null;
        }
    }

    public async Task<T?> Insert<T>(T value) where T : class
        => await this.connection!.QueryFirstOrDefaultAsync<T>(value.InsertSQL(), value, transaction: this.transaction);
}
