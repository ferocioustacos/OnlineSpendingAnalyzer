using SpendingInfo.Transactions.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Text.Json;

using static QuestPDF.Helpers.Colors;
using System.IO;

namespace SpendingInfo.Transactions
{
    public class TransactionTable<T> : ObservableCollection<T>, ICollection<T>, IEnumerable<T> where T : ITransaction
    {
        HashSet<String> filePaths = new HashSet<String>();
        Dictionary<String, ICollection<T>> fileAssociations = new Dictionary<string, ICollection<T>>();
        ICollection<T> allTransactions = new List<T>();
        
        public IReadOnlySet<String> LoadedFiles() { return filePaths; }
        public IReadOnlyDictionary<String, ICollection<T>> Associations() { return fileAssociations; }

        public void AddTransactions(String fileSource, ICollection<T> transactions)
        {
            if(filePaths.Contains(fileSource))
                fileAssociations[fileSource] = transactions;
            else
                filePaths.Add(fileSource);

            foreach (T t in transactions)
            {
                this.Add(t);
                allTransactions.Add(t);
            }
        }

        public ICollection<T> GetTransactionFromPath(String path)
        {
            if(filePaths.Contains(path)) return fileAssociations[path];
            return new Collection<T>();
        }

        public IEnumerable<T> EnumerateTransactions()
        {
            foreach(T transaction in this.allTransactions)
            {
                yield return transaction;
            }
        }

        public void RemoveTransaction(T t)
        {
            // 1.) Remove from fileAssociations list
            foreach (var collection in fileAssociations.Values)
                collection.Remove(t);
            allTransactions.Remove(t);


            // 2.) Remove from observable list
            this.Remove(t);
        }

        public void RemoveTransactions(String path)
        {
            if (!filePaths.Contains(path)) return;

            ICollection<T> transactions = fileAssociations[path];
            foreach (var t in transactions)
            {
                this.Remove(t);
                allTransactions.Remove(t);
            }

            fileAssociations.Remove(path);
        }

        public ICollection<T> GetAllTransactions()
        {
            return allTransactions;
        }

        public void SelectWithDates(DateTime start, DateTime end)
        {
            this.Clear();
            Func<T, bool> InRange = t => t.GetDate().Date >= start.Date && t.GetDate().Date <= end.Date;
            IEnumerable<T> sourceEnumerable = GetAllTransactions().Where(InRange);
            foreach (T t in sourceEnumerable) this.Add(t);
        }

        public void SearchByDescription(String query)
        {
            this.Clear();
            Func<T, bool> searchFunc = t => t.GetDescription().ToLower().Contains(query.ToLower());
            IEnumerable<T> sourceEnumerable = GetAllTransactions().Where(searchFunc);
            foreach (T t in sourceEnumerable) this.Add(t);
        }

        public void SelectWithinDatesAndSearch(DateTime start, DateTime end, String query)
        {
            this.Clear();
            Func<T, bool> searchFunc = t => t.GetDescription().ToLower().Contains(query.ToLower());
            Func<T, bool> InRange = t => t.GetDate().Date >= start.Date && t.GetDate().Date <= end.Date;
            IEnumerable<T> sourceEnumerable = GetAllTransactions().Where(InRange).Where(searchFunc);
            foreach (T t in sourceEnumerable) this.Add(t);
        }

        public ICollection<T> GetSelectedTransactions() => this;

        // TODO: need a way to signal that the collection elements have been updated
        //       so that it can properly update in the datagrid
        public void SignalCollectionChange(IList<T> changedItems)
        {
/*            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, changedItems);
            var eh = new NotifyCollectionChangedEventHandler(null, e);
            base.CollectionChanged += eh;
*/        }

        public String SerializeJson()
        {
            var options = new JsonSerializerOptions { };
            return this.SerializeJson(options);
        }

        public String SerializeJson(JsonSerializerOptions options)
        {
            var selectedTransactions = this.GetSelectedTransactions();
            return JsonSerializer.Serialize(selectedTransactions, options);
        }

        public void SaveToJson(String path)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            SaveToJson(path, options);
        }

        public async void SaveToJson(String path, JsonSerializerOptions options)
        {
            using FileStream fileStream = File.Create(path);
            var transactions = this.GetSelectedTransactions();
            await JsonSerializer.SerializeAsync(fileStream, transactions, options);
            await fileStream.DisposeAsync();
        }

        public static TransactionTable<T> LoadFromJson(String path)
        {
            var options = new JsonSerializerOptions { };
            return LoadFromJson(path, options);
        }

        public static TransactionTable<T> LoadFromJson(string path, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
     
    public class BankTransactionTable : TransactionTable<BankTransaction> { }

    public class AmazonTransactionTable : TransactionTable<AmazonTransaction>
    {
        public new void SearchByDescription(String query)
        {
            this.Clear();
            Func<AmazonTransaction, bool> searchFunc = t => t.GetDescription().ToLower().Contains(query.ToLower());
            IEnumerable<AmazonTransaction> sourceEnumerable = base.GetAllTransactions().Where(searchFunc);
            foreach (AmazonTransaction t in sourceEnumerable) this.Add(t);
        }

        public new void SelectWithinDatesAndSearch(DateTime start, DateTime end, String query)
        {
            query = query.Trim();
            AmazonTransaction.Category category = DetermineCategory(query);
            this.Clear();
            Func<AmazonTransaction, bool> InRange = t => t.GetDate().Date >= start.Date && t.GetDate().Date <= end.Date;
            Func<AmazonTransaction, bool> SearchFunc = t => t.GetDescription().ToLower().Contains(query.ToLower());
            Func<AmazonTransaction, bool> SameCategory = t => t.category == category;
            IEnumerable<AmazonTransaction> sourceEnumerable;
            if(category != AmazonTransaction.Category.UNCATEGORIZED)
            {
                Debug.WriteLine("SEARCHING FOR CATEGORY");
                sourceEnumerable = base.GetAllTransactions().Where(InRange).Where(SameCategory);
            } else
            {
                sourceEnumerable = base.GetAllTransactions().Where(InRange).Where(SearchFunc);
            }

            foreach (AmazonTransaction t in sourceEnumerable) this.Add(t);
        }

        public AmazonTransaction.Category DetermineCategory(String query)
        {
            String MATERIALS = "materials";
            String TOOLS = "tools";
            String OTHER = "other";
            String[] categories = new String[] {MATERIALS, TOOLS, OTHER};

            Debug.WriteLine($"query = '{query}'");
            for(int i = 0; i < categories.Length; i++)
            {
                String category = categories[i];
                Debug.WriteLine(category);
                query = query.ToLower();
                if (category.Equals(query.ToLower()))
                    return (AmazonTransaction.Category) i;
            }

            return AmazonTransaction.Category.UNCATEGORIZED;
        }
    }
}
