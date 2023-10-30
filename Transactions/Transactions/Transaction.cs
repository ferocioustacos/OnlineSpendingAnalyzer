using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static SpendingInfo.Transactions.Transactions.ITransaction;

namespace SpendingInfo.Transactions.Transactions
{
    public class TransactionBase : ITransaction, INotifyPropertyChanged
    {
        // INotifyPropertyChanged impl
        public event PropertyChangedEventHandler? PropertyChanged;
        private protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Transaction ID 
        private protected string id;
        public string ID
        {
            get { return id; }
            set
            {
                if (id == value) return;
                id = value;
                NotifyPropertyChanged();
            }
        }
        public string GetID() => id;

        // Transaction Date
        private protected DateTime date;
        public DateTime Date
        {
            get
            {
                return date;
            }

            set
            {
                date = value;
                NotifyPropertyChanged();
            }
        }
        public DateTime GetDate() => date;

        // Transaction Amount
        private protected float amount;
        public float Amount
        {
            get { return amount; }
            set
            {
                amount = value;
                NotifyPropertyChanged();
            }
        }
        public float GetAmount() => amount;

        public String GetAmountString()
        {
            float amount = GetAmount();
            return amount > 0 ? $"${amount}" : $"-${Math.Abs(amount)}";
        }

        public string AmountStr
        {
            get { return GetAmountString(); }
        }

        // Transaction Description
        private protected string description;
        private protected string searchableDescription; // stores a version of description that as processing already done
        public string Description
        {
            get
            {
                return description;
            }

            set
            {
                description = value;
                searchableDescription = value.Trim().ToLower();
                NotifyPropertyChanged();
            }
        }
        public string SearchableDescription { get { return searchableDescription; } }
        public string GetDescription() => description;


        // Transaction Type
        private protected TransactionType type;
        public TransactionType Type
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
                NotifyPropertyChanged();
            }
        }
        public TransactionType GetTransactionType() => type;

        // Constructor
        private protected TransactionBase(string id, DateTime date, float amount, string description, TransactionType type)
        {
            ID = id;
            Date = date;
            Amount = amount;
            Description = description;
            Type = type;
        }
    }
}
