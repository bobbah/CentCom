using System;
using System.Collections.Generic;
using System.Text;

namespace CentCom.Common.Models
{
    public class FlatBansVersion
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public uint Version { get; set; }
        public DateTime PerformedAt { get; set; }
    }
}
