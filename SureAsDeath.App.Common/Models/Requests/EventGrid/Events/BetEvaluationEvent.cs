using SureAsDeath.App.Common.Models.Requests.EventGrid.Data;

namespace SureAsDeath.App.Common.Models.Requests.EventGrid.Events
{
    public class BetEvaluationEvent : EventMessage
    {
        public BetEvaluation Data { get; set; }
    }
}
