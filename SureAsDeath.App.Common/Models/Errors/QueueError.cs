using System;

namespace SureAsDeath.App.Common.Models.Errors
{
    public class QueueError
    {
        public DateTime OccuredAt { get; set; }
        public string FunctionName { get; set; }
        public Exception Exception { get; set; }
        public object InputData { get; set; }
        public object OutputData { get; set; }
    }
}
