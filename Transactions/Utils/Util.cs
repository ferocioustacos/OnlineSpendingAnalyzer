using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xaml;

namespace SpendingInfo.Transactions.Utils
{
    internal static class Util
    {
        public static bool CheckValidValue<T>(T? value, T zero)
        {
            return value != null && !value.Equals(zero);
        }

        public static void ExtendCollection<T>(ref IEnumerable<T> source, ref ICollection<T> dest)
        {
            foreach (T x in source) { dest.Add(x); }
        }


        // maybe use check twice?
        // returns true when reload is detected
        public static bool CheckReload(String fileName, HashSet<String> loadedFiles, bool promptReload = true)
        {
            if (loadedFiles.Contains(fileName))
            {
                MessageBoxResult confirmation = MessageBoxResult.No;
                if (promptReload) confirmation = MessageBox.Show("File has been previously loaded. Reload this file? (Replaces previously selected data)", "Reload Confirmation", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                return confirmation == MessageBoxResult.Yes; // if yes, then already loaded file and wanted to reload
            }

            return true; // haven't loaded file yet
        }

/*        internal static void ExtendCollection<T>(ref TransactionTable<T> transactionTable, ref ObservableCollection<T> observableCollection) where T : Transaction
        {
            foreach(T t in transactionTable)
                if(!observableCollection.Contains(t))
                    observableCollection.Add(t);
        }

        internal static void ExtendCollection<T>(ref ICollection<T> currentTransactions, ref ObservableCollection<T> observableCollection) where T : Transaction
        {
            foreach(T transaction in currentTransactions)
                observableCollection.Add(transaction);
        }

        internal static void ExtendCollection<T>(ref IEnumerable<T> sourceEnumerable, ref ObservableCollection<T> dest) where T : Transaction
        {
            foreach (T transaction in sourceEnumerable)
                dest.Add(transaction);
        }

        public static void SelectWithinDates<T>(DateTime start, DateTime end, ref ICollection<T> source, ref ObservableCollection<T> dest) where T : Transaction
        {
            dest.Clear();
            Func<T, bool> InRange = t => t.transactionDate.Date >= start.Date && t.transactionDate.Date <= end.Date;
            IEnumerable<T> sourceEnumerable = source.Where(InRange);
            ExtendCollection(ref sourceEnumerable, ref dest);
        }

        internal static void SelectWithinDates<T>(DateTime start, DateTime end, ref TransactionTable<T> source, ref ObservableCollection<T> dest) where T : Transaction
        {
            dest.Clear();
            Func<T, bool> InRange = t => t.transactionDate.Date >= start.Date && t.transactionDate.Date <= end.Date;
            IEnumerable<T> sourceEnumerable = source.Where(InRange);
            ExtendCollection(ref sourceEnumerable, ref dest);
        }*/
    }

    class BankCSVFormatException : Exception
    {
        public BankCSVFormatException() { }
        public BankCSVFormatException(string message) : base(message) { }
        public BankCSVFormatException(string message, Exception innerException) : base(message, innerException) { }
    }

    class ZipFormatException : Exception
    {
        public ZipFormatException() { }
        public ZipFormatException(string message) : base(message) { }
        public ZipFormatException(string message, Exception innerException) : base(message, innerException) { }
    }


}
