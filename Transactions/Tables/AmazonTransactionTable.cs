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
using System.Collections.Specialized;

namespace SpendingInfo.Transactions.Tables
{
    public class AmazonTransactionTable : TransactionTable<AmazonTransaction>
    {

        public AmazonTransactionTable(AmazonTransactionTable table)
        {
            foreach(var transaction in table.GetAllTransactions())
            {
                AddTransaction(transaction);
            }

            foreach(var transaction in table.GetSelectedTransactions())
            {
                AddObservableTransaction(transaction);
            }
        }

        public AmazonTransactionTable(IEnumerable<AmazonTransaction> enumerable)
        {
            foreach (var transaction in enumerable)
            {
                AddObservableTransaction(transaction);
            }
        }

        public AmazonTransactionTable() : base()
        {
        }

        public override void SearchByDescription(string query)
        {
            ClearSelected();
            Func<AmazonTransaction, bool> searchFunc = t => t.GetDescription().ToLower().Contains(query.ToLower());
            IEnumerable<AmazonTransaction> sourceEnumerable = GetAllTransactions().Where(searchFunc);
            foreach (AmazonTransaction t in sourceEnumerable) AddSelectedTransaction(t);
            RaiseCollectionChanged();
        }

        public override void SelectWithinDatesAndSearch(DateTime start, DateTime end, string query)
        {
            query = query.Trim().ToLower();
            // See TODO note at `DetermineCategory` (tldr; replace with better version)
            //int category = DetermineCategory(query);
            int category = -1;
            ClearSelected();
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
            Debug.WriteLine($"sourceEnumerable contains {sourceEnumerable.Count()} elements.");
            foreach (AmazonTransaction t in sourceEnumerable)
            {
                AddSelectedTransaction(t);
            }

            RaiseCollectionChanged();
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
            SetTransaction(transactionIdx, transaction);
        }

        public void SetCategory(int transactionIdx, int categoryIdx)
        {
            AmazonTransaction transaction = new AmazonTransaction(selectedTransactions[transactionIdx]);
            transaction.Category = categoryIdx;
            SetTransaction(transactionIdx, transaction);
        }

        // TODO: replace with a version that requires a prefix, e.g. "category=Category" or "category={Category1, Category2, ...}"
        //       should also return a pair containing a list of categories and a 'sanitized' query (without search params)
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
                    try
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
                    catch { }
                }
            }

            return new AmazonTransactionTable(transactions);
        }
    }
}
