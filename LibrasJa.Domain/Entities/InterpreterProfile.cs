namespace LibrasJa.Domain.Entities;

public class InterpreterProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Especialidades { get; set; }
    public string? DescricaoCurta { get; set; }
    public string? Disponivel { get; set; }
    public User? User { get; set; }
}
