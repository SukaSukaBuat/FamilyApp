using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyApp.Common.Dtos
{
    public class ResponseProblemDto
    {
        public string? Type { get; set; }
        public string? Title { get; set; }
        public int Status { get; set; }
        public string? Error { get; set; }
        public string? ErrorStackTraces { get; set; }
        public string? TraceId { get; set; }
    }
    public record SuccessLoginDto(string Token, DateTimeOffset ValidTo);
}