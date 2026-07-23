using System.ComponentModel.DataAnnotations;

namespace WFT.Infra.Core.Entities
{
    public partial class BaseEntity
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public long CreatedUserId { get; set; }
        public long? UpdatedUserId { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        protected BaseEntity()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}