namespace WFT.Infra.Application.Contracts.DTOs.UserManagment
{
    public record ABACEvaluationResponseDto
    {
        public bool HasAccess { get; set; }
        public long UserId { get; set; }
        public string Permission { get; set; } = string.Empty;
        public object? Resource { get; set; }
        public object? Context { get; set; }
        public List<PolicyEvaluationResultDto> PolicyResults { get; set; } = new List<PolicyEvaluationResultDto>();
        public List<AttributeValueDto> UserAttributes { get; set; } = new List<AttributeValueDto>();
        public List<AttributeValueDto> ResourceAttributes { get; set; } = new List<AttributeValueDto>();
        public string EvaluationReason { get; set; } = string.Empty;
        public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
    }

    public record PolicyEvaluationResultDto
    {
        public long PolicyRuleId { get; set; }
        public string PolicyName { get; set; } = string.Empty;
        public string Effect { get; set; } = string.Empty; // "Allow" or "Deny"
        public int Priority { get; set; }
        public bool IsApplicable { get; set; }
        public bool ConditionsMet { get; set; }
        public List<ConditionEvaluationResultDto> ConditionResults { get; set; } = new List<ConditionEvaluationResultDto>();
        public string Reason { get; set; } = string.Empty;
    }

    public record ConditionEvaluationResultDto
    {
        public long ConditionId { get; set; }
        public string AttributeName { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public string ExpectedValue { get; set; } = string.Empty;
        public string ActualValue { get; set; } = string.Empty;
        public bool IsMet { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
} 