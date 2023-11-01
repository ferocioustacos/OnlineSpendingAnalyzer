using System;
using System.Collections.Generic;
using SpendingInfo.Transactions.Transactions;

namespace SpendingInfo.Transactions.ReportDocuments
{
    public class AmazonTransactionModel
    {
        public IReadOnlyCollection<AmazonTransaction> Transactions { get; }
        public DateTime ModelCreateTime { get; }

        public AmazonTransactionModel(IReadOnlyCollection<AmazonTransaction> transactions)
        {
            Transactions = transactions;
            ModelCreateTime = DateTime.Now;
        }
    }
}
