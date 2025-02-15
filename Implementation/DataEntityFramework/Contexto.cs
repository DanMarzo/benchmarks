using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataEntityFramework;

public class Contexto : DbContext
{

    public Contexto(DbContextOptions<Contexto> options) : base(options) { }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }


    public DbSet<PessoaModel> Pessoas { get; set; }

}
