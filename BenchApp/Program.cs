using BenchApp;
using Bogus;
using Domain.Models;
using RepositoryPadrao;

Repository repository = new Repository(ConstsSQL.ConnectionString);
var faker = new Faker("pt_BR");
PessoaModel pee = new PessoaModel()
{
    CPF = "###.###.###-##",
    Id = Guid.NewGuid()
};

await repository.GetAsync(x => x.CPF == pee.CPF && !x.EmAtividade && x.Id == pee.Id);

//await repository.GetAsync(x => x.EmAtividade);
