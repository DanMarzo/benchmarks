using Bogus;
using DataEntityFramework;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var opt = new DbContextOptionsBuilder<Contexto>();
opt.UseSqlServer();

using var contexto = new Contexto(opt.Options);

var quantidade = 1000000;
var faker = new Faker("pt_BR");

for (int i = 0; i < quantidade; i++)
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
    Console.WriteLine(JsonSerializer.Serialize(pessoa));
    await contexto.Pessoas.AddAsync(pessoa);
}

var linhas = await contexto.SaveChangesAsync();
Console.WriteLine($"Linhas incluidas {linhas}");

