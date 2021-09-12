using System;
using System.Collections.Generic;
using System.Text;

namespace SureAsDeath.App.Common.Helpers
{
    public static class Utils
    {
        public static string DecodeFromBase64String(string base64String)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64String));
        }
        public static string EncodeToBase64String(string plainText)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
        }
    }
}
