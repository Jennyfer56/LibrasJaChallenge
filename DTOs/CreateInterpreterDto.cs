namespace LibrasJaChallenge.DTOs
{
    public class CreateInterpreterDto
    {
        public int UserId { get; set; }
        public string Especialidades { get; set; } = string.Empty;
        public string? DescricaoCurta { get; set; }
        public string? Disponivel { get; set; }
    }
}
