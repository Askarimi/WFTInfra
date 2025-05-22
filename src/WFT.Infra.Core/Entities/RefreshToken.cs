namespace WFT.Infra.Core.Entities
{
    public partial class RefreshToken : BaseEntity
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;
    }
}
