using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B4.EE.BouteD.Models
{
    public class SmsDTO
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public string TimeStamp { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public string ContactId { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
        public string ContactNumber { get; set; }

        public string ContactFullName { get { return ContactFirstName + " " + ContactLastName; } }

        public bool IsEqual(SmsDTO b)
        {
            return (this.ContactId == b.ContactId
                    && this.ContactFirstName == b.ContactFirstName
                    && this.ContactLastName == b.ContactLastName
                    && this.ContactNumber == b.ContactNumber
                    && this.Message == b.Message
                    && this.StatusId == b.StatusId
                    && this.StatusName == b.StatusName
                    && this.TimeStamp == b.TimeStamp
                );
        }

    }
}
