using System.Linq.Expressions;

namespace DataApp.Contracts;

public interface IDatabaseConnGen : IAsyncDisposable
{

    #region Many results
    // Equivale a SELECT
    Task<IEnumerable<T>> ManyOrEmptyAsync<T>(Expression<Func<T, bool>> where) where T : class;
    
    // SELECT || OU INSERT COM OUTPUT OU RETURNS, ISSO DEPENDE DO SGBD
    Task<IEnumerable<T>> ManyOrEmptyAsync<T>(string query);
    Task<IEnumerable<T>> ManyOrEmptyAsync<T>(string query, object param);
    #endregion

    #region Unique result
    // Equivale a SELECT
    Task<T?> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> where) where T : class;

    // SELECT || OU INSERT COM OUTPUT OU RETURNS, ISSO DEPENDE DO SGBD
    Task<T?> FirstOrDefaultAsync<T>(string query);
    Task<T?> FirstOrDefaultAsync<T>(string query, object param);
    #endregion

    Task<T?> Insert<T>(T value) where T : class;


    Task<int> ExecuteAsync(string query, object param);

    #region Transacões
    Task BeginTransaction();
    Task CommitAsync();
    Task Rollback();
    #endregion
}
