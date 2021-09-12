using SureAsDeath.App.Common.Models.Requests.Base;
using SureAsDeath.Core.Models;

namespace SureAsDeath.App.Common.Models.Requests.Queue.Data
{
    public class RetrieveData : MessageBase
    {
        public Match Match { get; set; }
    }
}
