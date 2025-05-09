namespace RPK.Infra.Core.Entities
{
    public class SampleEntity : BaseEntity
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public bool IsActive { get; private set; }
    }
}