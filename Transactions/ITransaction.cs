using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace SpendingInfo.Transactions
{
    public interface ITransaction
    {
        public enum TransactionType { BANK_DEBIT, BANK_CREDIT, AMAZON, EBAY, UNKNOWN, ERROR };

        public String GetID();
        public float GetAmount();

        // Returns a formatted string of amount as $x or -$x
        public String GetAmountString()
        {
            float amount = GetAmount();
            return amount > 0 ? $"${amount}" : $"-${Math.Abs(amount)}";
        }

        public String GetDescription();
        public void SetDescription(String description);

        public DateTime GetDate();

        public TransactionType GetTransactionType();
        public void SetTransactionType(TransactionType type);

        public bool IsBankTransaction() => GetTransactionType() == TransactionType.BANK_DEBIT || GetTransactionType() == TransactionType.BANK_CREDIT;
        public bool IsAmazonTransaction() => GetTransactionType() == TransactionType.AMAZON;
        public bool IsEbayTransaction() => GetTransactionType() == TransactionType.EBAY;
        public bool IsInvalidTransaction() => GetTransactionType() == TransactionType.UNKNOWN || GetTransactionType() == TransactionType.ERROR;
        public bool IsValidTransaction() => !IsInvalidTransaction();

        public String TypeToString()
        {
            TransactionType type = GetTransactionType();
            switch(type)
            {
                // bank transaction
                case TransactionType.BANK_DEBIT:
                    return "Deposit";
                case TransactionType.BANK_CREDIT:
                    return "Withdrawl";

                // online retailers
                case TransactionType.AMAZON:
                    return "Amazon";
                case TransactionType.EBAY:
                    return "Ebay";

                // errors and defaults
                case TransactionType.UNKNOWN:
                    return "Unknown";
                case TransactionType.ERROR:
                default:
                    return "Error";
            }
        }

        public bool Equals(ITransaction other) => this.GetID().Equals(other.GetID());
        public int GetHashCode() => GetID().GetHashCode();

        public string SerializeJSON() => JsonSerializer.Serialize(this);

        public String ToString() { return $"[{GetDate()}] {GetDescription()} | {GetAmountString()}"; }
        public static String ToString(ITransaction transaction) => transaction.ToString();
    }
}
