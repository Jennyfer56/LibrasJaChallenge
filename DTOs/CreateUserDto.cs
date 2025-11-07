namespace LibrasJaChallenge.DTOs
{
    public class CreateUserDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Tipo { get; set; }   // se você usa isso no User
    }
}
