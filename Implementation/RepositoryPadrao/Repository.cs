using Dapper;
using Domain.Models;
using Microsoft.Data.SqlClient;
using System.ComponentModel.Design.Serialization;
using System.Linq.Expressions;

namespace RepositoryPadrao;

public class Repository
{
    private readonly string connection;

    public Repository(string connection)
    {
        this.connection = connection;
    }

    public async Task Insert(PessoaModel pessoa)
    {
        using var connection = new SqlConnection(this.connection);
        await connection.QueryFirstOrDefaultAsync<PessoaModel>("""
            INSERT INTO Pessoas
            (Id, Nome, Sobrenome, CPF, Nascimento, Sexo, EmAtividade)
            OUTPUT INSERTED.*
            VALUES (@Id, @Nome, @Sobrenome, @CPF, @Nascimento, @Sexo, @EmAtividade);
            """, pessoa);


    }
    public async Task<PessoaModel?> GetAsync(Expression<Func<PessoaModel, bool>> expression)
    {
        
        (string WhereClause, Dictionary<string, object> Parameters) = DapperQueryBuilder<PessoaModel>.Build(expression);

        string ss = $"select * from Pessoas {WhereClause}";

        return new PessoaModel();
    }
}



//010.140.863-86
//EA228EFB-9487-4024-8220-00003FDE8683