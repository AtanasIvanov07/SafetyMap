using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafetyMap.Core.DTOs.Email
{
    public class EmailPayload
    {
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string HtmlMessage { get; set; } = string.Empty;
    }
}
