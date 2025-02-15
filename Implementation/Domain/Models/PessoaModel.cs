using DataApp.Annotations;

namespace Domain.Models;

[TableDB(Nome = "Pessoas")]
public class PessoaModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nome { get; set; } = string.Empty;
    public string Sobrenome { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public DateTime Nascimento { get; set; }
    public int? Sexo { get; set; }
    public bool EmAtividade { get; set; } = true;
}
