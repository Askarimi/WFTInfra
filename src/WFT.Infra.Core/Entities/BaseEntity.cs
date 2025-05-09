namespace WFT.Infra.Core.Entities
{
    public abstract class BaseEntity
    {
        public long Id { get; protected set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        protected BaseEntity()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}