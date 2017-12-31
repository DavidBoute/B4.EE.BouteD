using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B4.EE.BouteD.Models
{
    public class SmsDTOWithClient
    {
        public SmsDTO SmsDTO { get; set; }
        public string Client { get; set; }
        public string Operation { get; set; }
    }
}
