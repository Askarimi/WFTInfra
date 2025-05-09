namespace WFT.Infra.Application.Contracts.DTOs
{
    public record BaseDto
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        protected BaseDto()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
