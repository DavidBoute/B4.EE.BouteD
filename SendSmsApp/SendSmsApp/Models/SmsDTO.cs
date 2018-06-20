using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SmsSenderApp.Models
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

        public string ContactFullName { get { return $"{ContactFirstName} {ContactLastName} ({ ContactNumber})"; } }

        private bool _isSending;
        public bool IsSending
        {
            get { return _isSending; }
            set
            {
                if (value != _isSending)
                {
                    _isSending = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        /// <summary>
        /// Vergelijkt of 2 SmsDTO's de zelfde properties hebben
        /// </summary>
        /// <param name="that">de andere SmsDTO</param>
        /// <returns></returns>
        public bool IsEqual(SmsDTO that)
        {
            return (this.ContactId == that.ContactId
                    && this.ContactFirstName == that.ContactFirstName
                    && this.ContactLastName == that.ContactLastName
                    && this.ContactNumber == that.ContactNumber
                    && this.Message == that.Message
                    && this.StatusId == that.StatusId
                    && this.StatusName == that.StatusName
                    && this.TimeStamp == that.TimeStamp
                );
        }

        /// <summary>
        /// Kopieert de properties van een andere SmsDTO naar de huidige
        /// </summary>
        /// <param name="that">de andere SmsDTO</param>
        public void CopyFrom(SmsDTO that)
        {
            this.ContactId = that.ContactId;
            this.ContactFirstName = that.ContactFirstName;
            this.ContactLastName = that.ContactLastName;
            this.ContactNumber = that.ContactNumber;
            this.Message = that.Message;
            this.StatusId = that.StatusId;
            this.StatusName = that.StatusName;
            this.TimeStamp = that.TimeStamp;
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
