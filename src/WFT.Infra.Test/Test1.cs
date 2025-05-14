using WFT.Infra.Application.Helper;

namespace WFT.Infra.Test
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void TestMethod1()
        {
            PasswordHasher hasher = new PasswordHasher();

            var hashPass =   hasher.HashPassword("RPK12345");

            var checkPass = hasher.VerifyPassword("RPK12345", hashPass);

        }
    }
}
