using SureAsDeath.App.Common.Models.Enumerators;
using System;

namespace SureAsDeath.App.Common.Models.Requests.Base
{
    public class MessageBase
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public MessageType MessageType { get; set; }
        public MessageStatus Status { get; set; }
        public AdditionalInfo AdditionalInfo { get; set; }
    }
}
