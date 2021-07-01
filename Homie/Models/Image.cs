using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Homie.Models
{
    public class Image
    {
        public int Id { get; set; }
        public string NameImg { get; set; }
        public byte[] Avatar { get; set; }
        public Guid _uid { get; set; }
    }
}
