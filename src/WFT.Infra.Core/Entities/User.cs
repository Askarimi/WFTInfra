namespace WFT.Infra.Core.Entities
{
    public abstract class User
    {
        public string Name { get; set; }
        public string Family { get; set; }
        public string NationalCode { get; set; }
        public int Age { get; set; }
    }
}
