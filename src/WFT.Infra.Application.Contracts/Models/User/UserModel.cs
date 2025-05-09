namespace WFT.Infra.Application.Contracts.Models.User
{
    public partial record UserModel
    {
        public Int64 Id { get; set; }
        public string Name { get; set; }
        public string Family { get; set; }
        public string NationalCode { get; set; }
        public int Age { get; set; }
    }
}
