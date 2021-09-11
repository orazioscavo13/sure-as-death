using SureAsDeath.App.Common.Models.Enumerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace SureAsDeath.App.Common.Models.Requests.EventGrid
{
    public class EventMessage
    {
        public Guid Id { get; set; }
        public EventType EventType { get; set; }
        public EventSubject Subject { get; set; }
        public DateTime EventTime { get; set; }
    }
}
