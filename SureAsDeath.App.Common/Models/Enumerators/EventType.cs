using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace SureAsDeath.App.Common.Models.Enumerators
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EventType
    {
        NewBet
    }
}