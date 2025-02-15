using Dapper;
using Domain.Models;
using Microsoft.Data.SqlClient;
using System.ComponentModel.Design.Serialization;

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
}
