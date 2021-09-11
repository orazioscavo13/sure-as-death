using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SureAsDeath.App.Common.Models.Enumerators
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MessageStatus
    {
        DATA_COLLECTION
    }
}
