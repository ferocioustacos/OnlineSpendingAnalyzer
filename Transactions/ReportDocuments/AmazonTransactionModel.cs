using System;
using System.Collections.Generic;
using SpendingInfo.Transactions.Transactions;

namespace SpendingInfo.Transactions.ReportDocuments
{
    public class AmazonTransactionModel
    {
        public ICollection<AmazonTransaction> Transactions { get; }
        public DateTime ModelCreateTime { get; }

        public AmazonTransactionModel(ICollection<AmazonTransaction> transactions)
        {
            Transactions = transactions;
            ModelCreateTime = DateTime.Now;
        }
    }
}
