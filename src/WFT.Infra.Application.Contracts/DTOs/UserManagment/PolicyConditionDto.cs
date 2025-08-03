namespace WFT.Infra.Application.Contracts.DTOs.UserManagment
{
    public record PolicyConditionDto : BaseDto
    {
        public long PolicyRuleId { get; set; }
        public long AttributeDefinitionId { get; set; }
        public string AttributeName { get; set; } = string.Empty;
        public long ConditionOperatorId { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string LogicalOperator { get; set; } = "AND";
        public int Order { get; set; }
    }
} 