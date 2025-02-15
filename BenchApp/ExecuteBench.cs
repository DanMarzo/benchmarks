using BenchmarkDotNet.Attributes;
using Bogus;
using DataApp;
using DataApp.Contracts;
using DataApp.Generators;
using DataEntityFramework;
using Domain.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RepositoryPadrao;

namespace BenchApp;



[MemoryDiagnoser]
public class ExecuteBench
{
    private const int LINHAS = 1;
    //private const int LINHAS = 10;
    [Benchmark]
    public async Task GetPessoasWithEntityFramework()
    {
        try
        {
            var faker = new Faker("pt_BR");
            var opt = new DbContextOptionsBuilder<Contexto>();
            opt.UseSqlServer(ConstsSQL.ConnectionString);
            using var contexto = new Contexto(opt.Options);

            for (int i = 0; i < LINHAS; i++)
            {
                await contexto.Pessoas.AddAsync(new PessoaModel
                {
                    Id = Guid.NewGuid(),
                    Nome = faker.Name.FirstName(),
                    Sobrenome = faker.Name.LastName(),
                    CPF = faker.Random.Replace("###.###.###-##"), // CPF fictício
                    Nascimento = faker.Date.Between(new DateTime(1950, 1, 1), new DateTime(2005, 12, 31)),
                    Sexo = faker.Random.Int(0, 1), // 0 = Masculino, 1 = Feminino
                    EmAtividade = faker.Random.Bool()
                });
            }
            await contexto.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    [Benchmark]
    public async Task GetPessoasWithDapper()
    {
        try
        {
            var faker = new Faker("pt_BR");

            var repository = new Repository(ConstsSQL.ConnectionString);
            for (int i = 0; i < LINHAS; i++)
            {
                var pessoa = new PessoaModel
                {
                    Id = Guid.NewGuid(),
                    Nome = faker.Name.FirstName(),
                    Sobrenome = faker.Name.LastName(),
                    CPF = faker.Random.Replace("###.###.###-##"), // CPF fictício
                    Nascimento = faker.Date.Between(new DateTime(1950, 1, 1), new DateTime(2005, 12, 31)),
                    Sexo = faker.Random.Int(0, 1), // 0 = Masculino, 1 = Feminino
                    EmAtividade = faker.Random.Bool()
                };
                await repository.Insert(pessoa);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    [Benchmark]
    public async Task GetPessoasWithCustomDapper()
    {
        try
        {
            var faker = new Faker("pt_BR");
            await using IDatabaseConnGen database = new DatabaseConnGen(new SqlConnection(ConstsSQL.ConnectionString));
            for (int i = 0; i < LINHAS; i++)
            {
                var pessoa = new PessoaModel
                {
                    Id = Guid.NewGuid(),
                    Nome = faker.Name.FirstName(),
                    Sobrenome = faker.Name.LastName(),
                    CPF = faker.Random.Replace("###.###.###-##"), // CPF fictício
                    Nascimento = faker.Date.Between(new DateTime(1950, 1, 1), new DateTime(2005, 12, 31)),
                    Sexo = faker.Random.Int(0, 1), // 0 = Masculino, 1 = Feminino
                    EmAtividade = faker.Random.Bool()
                };
                await database.Insert<PessoaModel>(pessoa);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    [Benchmark]
    public async Task GetPessoasWithCustomDapperTransaction()
    {
        try
        {
            var faker = new Faker("pt_BR");
            await using IDatabaseConnGen database = new DatabaseConnGen(new SqlConnection(ConstsSQL.ConnectionString));

            await database.BeginTransaction();
            for (int i = 0; i < LINHAS; i++)
            {
                var pessoa = new PessoaModel
                {
                    Id = Guid.NewGuid(),
                    Nome = faker.Name.FirstName(),
                    Sobrenome = faker.Name.LastName(),
                    CPF = faker.Random.Replace("###.###.###-##"), // CPF fictício
                    Nascimento = faker.Date.Between(new DateTime(1950, 1, 1), new DateTime(2005, 12, 31)),
                    Sexo = faker.Random.Int(0, 1), // 0 = Masculino, 1 = Feminino
                    EmAtividade = faker.Random.Bool()
                };
                await database.Insert<PessoaModel>(pessoa);
            }
            await database.CommitAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }
}
