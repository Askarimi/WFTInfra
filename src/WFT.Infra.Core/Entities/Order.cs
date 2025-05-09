namespace RPK.Infra.Core.Entities
{
    public class Order : BaseEntity
    {
        public string OrderNumber { get; private set; }
        public string CustomerName { get; private set; }
        public decimal TotalAmount { get; private set; }
        public DateTime OrderDate { get; private set; }
        public string Status { get; private set; }
    }
}