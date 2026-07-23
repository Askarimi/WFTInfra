namespace WFT.Infra.Application.Contracts.DTOs
{
    public record JwtResultDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
