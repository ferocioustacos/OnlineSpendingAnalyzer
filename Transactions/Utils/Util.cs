using System;
using System.Collections.Generic;
using System.Windows;

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
