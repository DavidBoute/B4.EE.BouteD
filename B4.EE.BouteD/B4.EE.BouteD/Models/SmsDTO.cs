using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace B4.EE.BouteD.Models
{
    public class SmsDTO :INotifyPropertyChanged
    {
       // Properties
        #region Properties

        private string _id;
        public string Id
        {
            get { return _id; }
            set
            {
                if (value != _id)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                if(value != _message)
                {
                    _message = value;
                    OnPropertyChanged();
                }   
            }
        }

        private string _timeStamp;
        public string TimeStamp
        {
            get { return _timeStamp; }
            set
            {
                if (value != _timeStamp)
                {
                    _timeStamp = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _statusId;
        public int StatusId
        {
            get { return _statusId; }
            set
            {
                if (value != _statusId)
                {
                    _statusId = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _statusName;
        public string StatusName
        {
            get { return _statusName; }
            set
            {
                if (value != _statusName)
                {
                    _statusName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _contactId;
        public string ContactId
        {
            get { return _contactId; }
            set
            {
                if (value != _contactId)
                {
                    _contactId = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _contactFirstName;
        public string ContactFirstName
        {
            get { return _contactFirstName; }
            set
            {
                if (value != _contactFirstName)
                {
                    _contactFirstName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _contactLastName;
        public string ContactLastName
        {
            get { return _contactLastName; }
            set
            {
                if (value != _contactLastName)
                {
                    _contactLastName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _contactNumber;
        public string ContactNumber
        {
            get { return _contactNumber; }
            set
            {
                if (value != _contactNumber)
                {
                    _contactNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ContactFullName { get { return ContactFirstName + " " + ContactLastName; } }

        #endregion

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

        public void CopyFrom(SmsDTO b)
        {
            this.ContactId = b.ContactId;
            this.ContactFirstName = b.ContactFirstName;
            this.ContactLastName = b.ContactLastName;
            this.ContactNumber = b.ContactNumber;
            this.Message = b.Message;
            this.StatusId = b.StatusId;
            this.StatusName = b.StatusName;
            this.TimeStamp = b.TimeStamp;
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        internal void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged == null)
                return;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
