namespace WFT.Infra.Application.Contracts.Interfaces
{
    public interface IPasswordHasher
    {
        public string HashPassword(string password);
        public bool VerifyPassword(string password, string hashedPassword);
        public string GenerateSalt();


    }
}
