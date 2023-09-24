using CsvHelper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Globalization;

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
            if (filePaths.Contains(fileSource))
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
            if (filePaths.Contains(path)) return fileAssociations[path];
            return new Collection<T>();
        }

        public IEnumerable<T> EnumerateTransactions()
        {
            foreach (T transaction in this.allTransactions)
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

        public virtual void SelectWithDates(DateTime start, DateTime end)
        {
            this.Clear();
            Func<T, bool> InRange = t => t.GetDate().Date >= start.Date && t.GetDate().Date <= end.Date;
            IEnumerable<T> sourceEnumerable = GetAllTransactions().Where(InRange);
            foreach (T t in sourceEnumerable) this.Add(t);
        }

        public virtual void SearchByDescription(String query)
        {
            this.Clear();
            Func<T, bool> searchFunc = t => t.GetDescription().ToLower().Contains(query.ToLower());
            IEnumerable<T> sourceEnumerable = GetAllTransactions().Where(searchFunc);
            foreach (T t in sourceEnumerable) this.Add(t);
        }

        public virtual void SelectWithinDatesAndSearch(DateTime start, DateTime end, String query)
        {
            this.Clear();
            Func<T, bool> searchFunc = t => t.GetDescription().ToLower().Contains(query.ToLower());
            Func<T, bool> InRange = t => t.GetDate().Date >= start.Date && t.GetDate().Date <= end.Date;
            IEnumerable<T> sourceEnumerable = GetAllTransactions().Where(InRange).Where(searchFunc);
            foreach (T t in sourceEnumerable) this.Add(t);
        }

        public ICollection<T> GetSelectedTransactions() => this;

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

        public virtual void ExportSelectedAsCSV(String filePath)
        {
            using (var fileStream = File.Create(filePath))
            {
                this.ExportSelectedAsCSV(fileStream);
            }
        }

        public virtual void ExportSelectedAsCSV(Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                this.ExportSelectedAsCSV(csv);
            }
        }

        public virtual void ExportSelectedAsCSV(CsvWriter csv)
        {
            throw new NotImplementedException("Each implementing class of TransactionTable must implement this function");
        }

    }
}
