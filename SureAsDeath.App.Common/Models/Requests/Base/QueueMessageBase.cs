using System;
using System.Collections.Generic;
using System.Text;

namespace SureAsDeath.App.Common.Models.Requests.Base
{
    public class QueueMessageBase
    {
        public string MessageId { get; set; }
        public string PopReceipt { get; set; }
        public long DequeueCount { get; set; }
    }
}
