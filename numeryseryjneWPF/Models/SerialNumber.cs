using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace numeryseryjneWPF.Models
{
    internal class SerialNumber
    {
        public int id { get; set; }
        public string name { get; set; }
        public string number {  get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
