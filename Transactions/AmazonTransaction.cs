using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Controls;

using static SpendingInfo.Transactions.ITransaction;


namespace SpendingInfo.Transactions
{
    public class AmazonTransaction : ITransaction
    {
        public String id { get; set; } = "";
        public String description{ get; set; } = "";
        public String fileSource { get; set; } = "";
        public TransactionType transactionType { get; set; } = TransactionType.UNKNOWN;
        public DateTime date { get; set; } = DateTime.MinValue;
        public float amount { get; set; } = 0.0f;
        public enum Category { MATERIALS, TOOLS, OTHER, UNCATEGORIZED };
        public String ASIN { get; }
        public Category category { get; set; }

        public AmazonTransaction(String id, DateTime date, float amount, String description, String ASIN)
        {
            this.transactionType = TransactionType.AMAZON;
            this.id = id;
            this.date = date;
            this.amount = amount;
            this.description= description;
            this.ASIN = ASIN;
            this.category = Category.UNCATEGORIZED;
        }

        public AmazonTransaction(String id, DateTime date, float amount, String description, String ASIN, String fileSource)
        {
            this.transactionType = TransactionType.AMAZON;
            this.id = id;
            this.date = date;
            this.amount = amount;
            this.description = description;
            this.ASIN = ASIN;
            this.fileSource = fileSource;
            this.category = Category.UNCATEGORIZED;
        }

        public DateTime GetDate() => this.date;
        public TransactionType GetTransactionType() => this.transactionType;
        public void SetTransactionType(TransactionType type) => this.transactionType = type;

        public string GetID() => this.id;
        public string GetASIN() => this.ASIN;
        public float GetAmount() => this.amount;
        public string GetDescription() => this.description;
        public void SetDescription(String description) => this.description = description;
        public override string ToString() => ITransaction.ToString(this);
    }
}
