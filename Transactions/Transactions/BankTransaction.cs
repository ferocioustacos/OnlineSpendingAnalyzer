using System;
using static SpendingInfo.Transactions.Transactions.ITransaction;

namespace SpendingInfo.Transactions.Transactions
{
    // deprecated, going to remove it in later update
    public class BankTransaction : ITransaction
    {
        // inherited fields

        public string transactionId { get; set; }
        public string transactionDescription { get; set; }
        public TransactionType transactionType { get; set; }
        public DateTime transactionDate { get; set; }
        public float transactionAmount { get; set; }
        public string fileSource { get; set; }


        // additional fields
        public float balance { get; set; }

        // optional fields
        public string? refCheck { get; set; }
        public string? transactionCategory { get; set; }
        public string? transactionMemo { get; set; }

        public BankTransaction(DateTime date, float amount, float balance, string description, string fileSource)
        {
            transactionType = DetermineType(amount);
            transactionDate = date;
            transactionAmount = amount;
            this.balance = balance;
            transactionDescription = description;
            transactionId = date.GetHashCode().ToString();
            this.fileSource = fileSource;
        }

        public BankTransaction(TransactionType type, DateTime date, float amount, float balance, string description)
        {
            transactionType = VerifyType(type, amount);
            transactionDate = date;
            transactionAmount = amount;
            this.balance = balance;
            transactionDescription = description;
            transactionId = date.GetHashCode().ToString();
            fileSource = "";
        }

        public bool IsDebit() { return transactionType == TransactionType.BANK_DEBIT; }
        public bool IsCredit() { return transactionType == TransactionType.BANK_CREDIT; }

        public static TransactionType DetermineType(float amount)
        {
            if (amount >= 0) return TransactionType.BANK_DEBIT;
            if (amount <= 0) return TransactionType.BANK_CREDIT;

            return TransactionType.ERROR; // should never trigger
        }

        public static TransactionType VerifyType(TransactionType type, float amount)
        {
            if (type == TransactionType.BANK_DEBIT && amount >= 0) return type;
            if (type == TransactionType.BANK_CREDIT && amount <= 0) return type;

            return TransactionType.ERROR;
        }

        public override string ToString()
        {
            string output = $"{transactionDate.ToString()}";

            if (refCheck != null) output += $" | {refCheck}";

            output += $" | {transactionDescription} | {transactionAmount}";

            if (transactionCategory != null)
                output += $" | {transactionCategory}";

            if (transactionMemo != null)
                output += $" | {transactionMemo}";

            return output;
        }

        public string GetRefCheck()
        {
            if (refCheck == null)
            {
                return "";
            }

            return refCheck.ToString();
        }

        public string GetID() => transactionId;

        public float GetAmount() => transactionAmount;

        public string GetDescription() => transactionDescription;

        public DateTime GetDate() => transactionDate;

        public void SetDescription(string description) => transactionDescription = description;
        public TransactionType GetTransactionType() => transactionType;
        public void SetTransactionType(TransactionType type) => transactionType = type;
    }
}
