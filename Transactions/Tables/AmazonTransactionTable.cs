using CsvHelper;
using SpendingInfo.Transactions.Transactions;
using SpendingInfo.Transactions.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

using SpendingInfo.Transactions.Util.FileLoader;
using System.ComponentModel;

namespace SpendingInfo.Transactions.Tables
{
    public class AmazonTransactionTable : TransactionTable<AmazonTransaction>, INotifyPropertyChanged
    {

        public AmazonTransactionTable(IEnumerable<AmazonTransaction> enumerable)
        {
            foreach (var transaction in enumerable)
            {
                Add(transaction);
            }
        }

        public AmazonTransactionTable() : base()
        {
        }

        public override void SearchByDescription(string query)
        {
            Clear();
            Func<AmazonTransaction, bool> searchFunc = t => t.GetDescription().ToLower().Contains(query.ToLower());
            IEnumerable<AmazonTransaction> sourceEnumerable = GetAllTransactions().Where(searchFunc);
            foreach (AmazonTransaction t in sourceEnumerable) Add(t);
        }

        public override void SelectWithinDatesAndSearch(DateTime start, DateTime end, string query)
        {
            query = query.Trim().ToLower();
            int category = DetermineCategory(query);
            this.Clear();
            Func<AmazonTransaction, bool> InRange = t => t.GetDate().Date >= start.Date && t.GetDate().Date <= end.Date;
            Func<AmazonTransaction, bool> SearchFunc = t => t.SearchableDescription.Contains(query);
            Func<AmazonTransaction, bool> SameCategory = t => t.Category == category;
            IEnumerable<AmazonTransaction> sourceEnumerable;
            if (category >= 0)
            {
                Debug.WriteLine("SEARCHING FOR CATEGORY");
                sourceEnumerable = GetAllTransactions().Where(InRange).Where(SameCategory);
            }
            else
            {
                sourceEnumerable = GetAllTransactions().Where(InRange).Where(SearchFunc);
            }

            foreach (AmazonTransaction t in sourceEnumerable) Add(t);
        }

        public void SetTransaction(string transactionId, AmazonTransaction transaction)
        {
            int transactionIdx = -1;
            IList<AmazonTransaction> transactions = GetSelectedTransactions().ToList();
            for(int i = 0; i < GetSelectedTransactions().Count; i++)
            {
                transactions[i].ID.Equals(transactionId);
            }

            if(transactionIdx != -1)
            {
                base.SetItem(transactionIdx, transaction);
            }
        }

        public void SetCategory(string transactionID, int categoryIdx)
        {
            int transactionIdx = 0;
            AmazonTransaction? transaction = null;
            foreach (AmazonTransaction t in GetSelectedTransactions())
            {
                if (t.ID.Equals(transactionID))
                {
                    transaction = t;
                }
                transactionIdx++;
            }
            if(transaction == null) { return; }

            transaction.Category = categoryIdx;
            base.SetItem(transactionIdx, transaction);
        }

        public void SetCategory(int transactionIdx, int categoryIdx)
        {
            AmazonTransaction transaction = base[transactionIdx];
            transaction.Category = categoryIdx;
            base.SetItem(transactionIdx, transaction);
        }

        public int DetermineCategory(string query)
        {
            IList<string> categories = AmazonTransaction.Categories;
            for (int i = 0; i < categories.Count; i++)
            {
                string category = categories[i];
                if (category.Equals(query))
                    return i;
            }

            return -1;
        }

        public override void ExportSelectedAsCSV(CsvWriter csv)
        {
            var records = new List<dynamic>();
            foreach(AmazonTransaction transaction in GetSelectedTransactions())
            {
                records.Add(transaction.ToCsvRecord());
            }
            csv.WriteRecords(records);
        }

        public static AmazonTransactionTable FromZIP(string filePath) 
        {
            return new AmazonTransactionTable(
                AmazonLoader.LoadFromZIP(filePath, reload: true, ignoreReturned: true)
            );
        }

        public static AmazonTransactionTable FromCSV(string filePath)
        {
            using (Stream stream = File.OpenRead(filePath))
            {
                return FromCSV(stream);
            }
        }

        public static AmazonTransactionTable FromCSV(Stream stream)
        {
            var transactions = new List<AmazonTransaction>();
            using (StreamReader writer = new StreamReader(stream))
            using (CsvReader csv = new CsvReader(writer, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();
                while(csv.Read())
                {
                    string id = csv.GetField<string>("ID");
                    DateTime date = csv.GetField<DateTime>("Date");
                    float amount = csv.GetField<float>("Amount");
                    string description = csv.GetField<string>("Description");
                    string asin = csv.GetField<string>("ASIN");
                    int category = csv.GetField<int>("Category");

                    var transaction = new AmazonTransaction(id, date, amount, description, asin, category);
                    transactions.Add(transaction);
                }
            }

            return new AmazonTransactionTable(transactions);
        }
    }
}
