using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SpendingInfo.Transactions.ITransaction;

namespace SpendingInfo.Transactions
{
    // deprecated, going to remove it in later update
    public class BankTransaction : ITransaction
    {
        // inherited fields

        public String transactionId { get; set; }
        public String transactionDescription { get; set; }
        public TransactionType transactionType { get; set; }
        public DateTime transactionDate { get; set; }
        public float transactionAmount { get; set; }
        public String fileSource { get; set; }


        // additional fields
        public float balance { get; set; }

        // optional fields
        public String? refCheck { get; set; }
        public String? transactionCategory { get; set; }
        public String? transactionMemo { get; set; }

        public BankTransaction(DateTime date, float amount, float balance, String description, String fileSource)
        {
            this.transactionType = DetermineType(amount);
            this.transactionDate = date;
            this.transactionAmount = amount;
            this.balance = balance;
            this.transactionDescription = description;
            this.transactionId = date.GetHashCode().ToString();
            this.fileSource = fileSource;
        }

        public BankTransaction(TransactionType type, DateTime date, float amount, float balance, String description)
        {
            this.transactionType = VerifyType(type, amount);
            this.transactionDate = date;
            this.transactionAmount = amount;
            this.balance = balance;
            this.transactionDescription = description;
            this.transactionId = date.GetHashCode().ToString();
            this.fileSource = "";
        }

        public bool IsDebit() { return this.transactionType == TransactionType.BANK_DEBIT; }
        public bool IsCredit() { return this.transactionType == TransactionType.BANK_CREDIT; }

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

        public override String ToString()
        {
            String output = $"{transactionDate.ToString()}";

            if (refCheck != null) output += $" | {refCheck}";

            output += $" | {transactionDescription} | {transactionAmount}";

            if(transactionCategory != null) 
                output += $" | {transactionCategory}";

            if(transactionMemo != null) 
                output += $" | {transactionMemo}";

            return output;
        }

        public String GetRefCheck()
        {
            if(this.refCheck == null)
            {
                return "";
            }

            return this.refCheck.ToString();
        }

        public string GetID() => this.transactionId;

        public float GetAmount() => this.transactionAmount;

        public string GetDescription() => this.transactionDescription;

        public DateTime GetDate() => this.transactionDate;

        public void SetDescription(string description) => this.transactionDescription = description;
        public TransactionType GetTransactionType() => this.transactionType;
        public void SetTransactionType(TransactionType type) => this.transactionType = type;
    }
}
