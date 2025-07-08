using WFT.Infra.Application.Contracts.Interfaces;

namespace WFT.Infra.Application.Helper
{
    public partial class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        public string GenerateSalt()
        {
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[32]; // Salt طول 32 بایت
                rng.GetBytes(salt);
                return Convert.ToBase64String(salt);
            }
        }
    }
}
