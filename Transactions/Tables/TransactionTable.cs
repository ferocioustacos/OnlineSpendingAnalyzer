using CsvHelper;
using SpendingInfo.Transactions.Transactions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Navigation;

namespace SpendingInfo.Transactions.Tables
{
    public class TransactionTable<T> : INotifyCollectionChanged where T : ITransaction
    {
        protected IList<T> allTransactions = new List<T>(); // stores all transactions (is what is searched)
        protected HashSet<string> transactionIDs = new HashSet<string>(); // keeps track of all the transactions

        protected IList<T> selectedTransactions = new List<T>(); // what the 'user' can see
        protected HashSet<string> selectedIDs = new HashSet<string>();

        public IReadOnlyList<T> AllTransactions
        {
            get => (IReadOnlyList<T>) allTransactions;
        }

        public IReadOnlyList<T> SelectedTransactions
        {
            get => (IReadOnlyList<T>) selectedTransactions;
        }

        // INotifyCollectionChanged impl
        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        public void RaiseCollectionChanged(NotifyCollectionChangedAction action = NotifyCollectionChangedAction.Reset, object? item = null, int? index = null)
        {
            if (item != null)
            {
                if (index != null)
                {
                    var args = new NotifyCollectionChangedEventArgs(action, item, index);
                    CollectionChanged?.Invoke(this, args);
                }
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, item));
            } else
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action));
            }
        }

        public void RaiseCollectionReplaceChanged(object oldItem, object newItem)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, oldItem, newItem));
        }

        // IEnumerable impl
        public IEnumerator<T> GetSelectedEnumerator()
        {
            return selectedTransactions.GetEnumerator();
        }

        public void AddTransaction(T transaction)
        {
            if(transactionIDs.Contains(transaction.GetID()))
            {
                return;
            }

            transactionIDs.Add(transaction.GetID());
            allTransactions.Add(transaction);
        }

        public void AddObservableTransaction(T transaction)
        {
            if(!transactionIDs.Contains(transaction.GetID()))
            {
                allTransactions.Add(transaction);
                transactionIDs.Add(transaction.GetID());
                selectedIDs.Add(transaction.GetID());
                selectedTransactions.Add(transaction);
                RaiseCollectionChanged(NotifyCollectionChangedAction.Add, transaction);
            }
        }

        public void AddSelectedTransaction(T transaction)
        {
            if(selectedIDs.Contains(transaction.GetID())) 
            { 
                return; 
            }

            selectedIDs.Add(transaction.GetID());
            selectedTransactions.Add(transaction);
        }

        public void ClearSelected()
        {
            selectedIDs.Clear();
            selectedTransactions.Clear();
        }

        public void AddTransactions(IEnumerable<T> transactions)
        {
            Func<T, bool> idNotExists = t => !transactionIDs.Contains(t.GetID());
            foreach (T t in transactions.Where(idNotExists))
            {
                AddObservableTransaction(t);
            }
        }

        public IEnumerable<T> EnumerateTransactions()
        {
            foreach (T transaction in allTransactions)
            {
                yield return transaction;
            }
        }

        public void RemoveTransaction(T t)
        {
            selectedTransactions.Remove(t);
            transactionIDs.Remove(t.GetID());
            allTransactions.Remove(t);
            RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, t);
        }

        public void RemoveSelectedAt(int idx)
        {
            var transaction = selectedTransactions[idx];
            selectedTransactions.RemoveAt(idx);
            transactionIDs.Remove(transaction.GetID());
            allTransactions.Remove(transaction);
            RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, transaction);
        }

        public ICollection<T> GetAllTransactions()
        {
            return allTransactions;
        }

        public virtual void SelectWithDates(DateTime start, DateTime end)
        {
            selectedTransactions.Clear();
            Func<T, bool> InRange = t => t.GetDate().Date >= start.Date && t.GetDate().Date <= end.Date;
            IEnumerable<T> sourceEnumerable = GetAllTransactions().Where(InRange);
            foreach (T t in sourceEnumerable) selectedTransactions.Add(t);
        }

        public virtual void SearchByDescription(string query)
        {
            selectedTransactions.Clear();
            Func<T, bool> searchFunc = t => t.GetDescription().ToLower().Contains(query.ToLower());
            IEnumerable<T> sourceEnumerable = GetAllTransactions().Where(searchFunc);
            foreach (T t in sourceEnumerable) selectedTransactions.Add(t);
        }

        public virtual void SelectWithinDatesAndSearch(DateTime start, DateTime end, string query)
        {
            selectedTransactions.Clear();
            Func<T, bool> searchFunc = t => t.GetDescription().ToLower().Contains(query.ToLower());
            Func<T, bool> InRange = t => t.GetDate().Date >= start.Date && t.GetDate().Date <= end.Date;
            IEnumerable<T> sourceEnumerable = from t in GetAllTransactions() where searchFunc(t) && InRange(t) select t;
            foreach (T t in sourceEnumerable) selectedTransactions.Add(t);
        }

        public IReadOnlyCollection<T> GetSelectedTransactions() => (IReadOnlyCollection<T>) selectedTransactions;

        public void SetTransaction(string transactionId, T transaction)
        {
            int transactionIdx = -1;
            IList<T> transactions = GetSelectedTransactions().ToList();
            for (int i = 0; i < GetSelectedTransactions().Count; i++)
            {
                transactions[i].GetID().Equals(transactionId);
            }

            SetTransaction(transactionIdx, transaction);
        }

        public void SetTransaction(int transactionIdx, T transaction)
        {
            if (transactionIdx != -1)
            {
                T old = (T) selectedTransactions[transactionIdx];
                selectedTransactions[transactionIdx] = transaction;
                RaiseCollectionReplaceChanged(old, transaction);
            }
        }

        public void SetTransactionInAll(int transactionIdx, T transaction)
        {
            if (transactionIdx != -1)
            {
                allTransactions[transactionIdx] = transaction;
            }
        }

        public void ClearAll()
        {
            selectedTransactions.Clear();
            transactionIDs.Clear();
            allTransactions.Clear();
            RaiseCollectionChanged(NotifyCollectionChangedAction.Reset);
        }

        public virtual void ExportSelectedAsCSV(string filePath)
        {
            using (var fileStream = File.Create(filePath))
            {
                ExportSelectedAsCSV(fileStream);
            }
        }

        public virtual void ExportSelectedAsCSV(Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                ExportSelectedAsCSV(csv);
            }
        }

        public virtual void ExportSelectedAsCSV(CsvWriter csv)
        {
            throw new NotImplementedException("Each implementing class of TransactionTable must implement this function");
        }
    }
}
