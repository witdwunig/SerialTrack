using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;


namespace numeryseryjneWPF.Models
{
    internal class SerialNumber
    {
        public int id { get; set; }
        public string name { get; set; }
        public string number {  get; set; }
        [JsonPropertyName("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }
    }
}
