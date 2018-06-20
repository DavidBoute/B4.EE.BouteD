using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsSenderApp.Models
{
    public class SmsDTOWithOperation
    {
        public SmsDTO SmsDTO { get; set; }
        public string Operation { get; set; }
    }
}
