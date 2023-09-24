using CsvHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpendingInfo.Transactions
{
    public class AmazonTransactionTable : TransactionTable<AmazonTransaction>
    {
        public AmazonTransactionTable(IEnumerable<AmazonTransaction> enumerable)
        {
            foreach (var transaction in enumerable)
            {
                this.Add(transaction);
            }
        }

        public AmazonTransactionTable()
        {
        }

        public override void SearchByDescription(String query)
        {
            this.Clear();
            Func<AmazonTransaction, bool> searchFunc = t => t.GetDescription().ToLower().Contains(query.ToLower());
            IEnumerable<AmazonTransaction> sourceEnumerable = base.GetAllTransactions().Where(searchFunc);
            foreach (AmazonTransaction t in sourceEnumerable) this.Add(t);
        }

        public override void SelectWithinDatesAndSearch(DateTime start, DateTime end, String query)
        {
            query = query.Trim();
            AmazonTransaction.Category category = DetermineCategory(query);
            this.Clear();
            Func<AmazonTransaction, bool> InRange = t => t.GetDate().Date >= start.Date && t.GetDate().Date <= end.Date;
            Func<AmazonTransaction, bool> SearchFunc = t => t.GetDescription().ToLower().Contains(query.ToLower());
            Func<AmazonTransaction, bool> SameCategory = t => t.category == category;
            IEnumerable<AmazonTransaction> sourceEnumerable;
            if (category != AmazonTransaction.Category.UNCATEGORIZED)
            {
                Debug.WriteLine("SEARCHING FOR CATEGORY");
                sourceEnumerable = base.GetAllTransactions().Where(InRange).Where(SameCategory);
            }
            else
            {
                sourceEnumerable = base.GetAllTransactions().Where(InRange).Where(SearchFunc);
            }

            foreach (AmazonTransaction t in sourceEnumerable) this.Add(t);
        }

        public AmazonTransaction.Category DetermineCategory(String query)
        {
            String[] categories = Enum.GetNames(typeof(AmazonTransaction.Category));
            for (int i = 0; i < categories.Length; i++)
            {
                string category = categories[i];
                if (category.Equals(query.ToLower()))
                    return (AmazonTransaction.Category)i;
            }

            return AmazonTransaction.Category.UNCATEGORIZED;
        }

        public override void ExportSelectedAsCSV(CsvWriter csv)
        {
            csv.WriteHeader<AmazonTransaction>();
            csv.NextRecord();
            csv.WriteRecords(this.GetSelectedTransactions());
            csv.Flush();
        }

        public static AmazonTransactionTable FromCSV(string filePath)
        {
            using (Stream stream = File.OpenRead(filePath))
            {
                return AmazonTransactionTable.FromCSV(stream);
            }
        }
        public static AmazonTransactionTable FromCSV(Stream stream)
        {
            using (StreamReader writer =  new StreamReader(stream))
            using (CsvReader csv = new CsvReader(writer, CultureInfo.InvariantCulture))
            {
                return new AmazonTransactionTable(csv.GetRecords<AmazonTransaction>().ToList());
            }
        }
    }
}
