using System;

namespace SpendingInfo.Transactions.Transactions
{
    public interface ITransaction
    {
        public enum TransactionType { BANK_DEBIT, BANK_CREDIT, AMAZON, EBAY, UNKNOWN, ERROR };

        public string GetID();
        public float GetAmount();

        // Returns a formatted string of amount as $x or -$x
        public string GetAmountString()
        {
            float amount = GetAmount();
            return amount > 0 ? $"${amount}" : $"-${Math.Abs(amount)}";
        }

        public string GetDescription();

        public DateTime GetDate();

        public TransactionType GetTransactionType();

        public bool IsBankTransaction() => GetTransactionType() == TransactionType.BANK_DEBIT || GetTransactionType() == TransactionType.BANK_CREDIT;
        public bool IsAmazonTransaction() => GetTransactionType() == TransactionType.AMAZON;
        public bool IsEbayTransaction() => GetTransactionType() == TransactionType.EBAY;
        public bool IsInvalidTransaction() => GetTransactionType() == TransactionType.UNKNOWN || GetTransactionType() == TransactionType.ERROR;
        public bool IsValidTransaction() => !IsInvalidTransaction();

        public string TypeToString()
        {
            TransactionType type = GetTransactionType();
            switch (type)
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

        public bool Equals(ITransaction other) => GetID().Equals(other.GetID());
        public int GetHashCode() => GetID().GetHashCode();
        public static string ToString(ITransaction transaction) => $"[{transaction.GetDate()}] {transaction.GetDescription()} | {transaction.GetAmountString()} | ";
    }
}
