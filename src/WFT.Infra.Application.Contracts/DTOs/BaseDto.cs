namespace WFT.Infra.Application.Contracts.DTOs
{
    public record BaseDto
    {
        public long Id { get; set; }
        public long CreatedUserId { get; set; }
        public long? UpdatedUserId { get; set; }
    }
}
