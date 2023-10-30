using Microsoft.Win32;
using QuestPDF.Fluent;
using SpendingInfo.Tabs.Amazon;
using SpendingInfo.Transactions.ReportDocuments;
using SpendingInfo.Transactions.Tables;
using SpendingInfo.Transactions.Transactions;
using SpendingInfo.Transactions.Util.FileLoader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpendingInfo.Tabs
{
    /// <summary>
    /// Interaction logic for AmazonTab.xaml
    /// </summary>
    public partial class AmazonTabContent : UserControl
    {
        AmazonTransactionTable amazonTransactions;

        DateTime amazonStartDate = DateTime.UnixEpoch;
        DateTime amazonEndDate = DateTime.Now;

        public AmazonTabContent()
        {
            InitializeComponent();
            amazonTransactions = new AmazonTransactionTable();
            amazonTable.ItemsSource = amazonTransactions;

            #if DEBUG
                AmazonTransaction.Categories = new String[]{ "Tech", "Clothes", "Other", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v"};
                string filePath = "testing-data/amazon-transactions.csv";
                IEnumerable<AmazonTransaction> currentTransactions = AmazonTransactionTable.FromCSV(filePath);
                amazonTransactions.AddTransactions(currentTransactions);
            #endif
        }

        private void AmazonLoadZip_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.ShowReadOnly = true;

            if (fileDialog.ShowDialog() == true)
            {
                String fileName = fileDialog.FileName;
                IEnumerable<AmazonTransaction> currentTransactions = AmazonLoader.LoadFromZIP(fileName);
                amazonTransactions.AddTransactions(currentTransactions);
                Debug.WriteLine($"Got {currentTransactions.Count()} amazon items");
            }
        }

        private void amazonStartDateSelector_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            amazonStartDate = amazonStartDateSelector.SelectedDate ?? DateTime.UnixEpoch;
            amazonTransactions.SelectWithinDatesAndSearch(amazonStartDate, amazonEndDate, amazonSearchBar.Text);
        }

        private void amazonEndDateSelector_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            amazonEndDate = amazonEndDateSelector.SelectedDate ?? DateTime.Now;
            amazonTransactions.SelectWithinDatesAndSearch(amazonStartDate, amazonEndDate, amazonSearchBar.Text);
        }

        private void amazonSearch_SelectionChanged(object sender, RoutedEventArgs e)
        {
            amazonTransactions.SelectWithinDatesAndSearch(amazonStartDate, amazonEndDate, amazonSearchBar.Text);
        }

        private void ClearAmazonStartDate_Click(object sender, RoutedEventArgs e)
        {
            amazonStartDate = DateTime.UnixEpoch;
            amazonStartDateSelector.Text = null;
            amazonTransactions.SelectWithinDatesAndSearch(amazonStartDate, amazonEndDate, amazonSearchBar.Text);
        }

        private void ClearAmazonEndDate_Click(object sender, RoutedEventArgs e)
        {
            amazonEndDate = DateTime.Now;
            amazonEndDateSelector.Text = null;
            amazonTransactions.SelectWithinDatesAndSearch(amazonStartDate, amazonEndDate, amazonSearchBar.Text);
        }

        private void AmazonReport_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.FileName = "report.pdf";
            if (fileDialog.ShowDialog() == true)
            {
                ICollection<AmazonTransaction> transactions = amazonTransactions.GetSelectedTransactions();
                AmazonTransactionDocument doc = new AmazonTransactionDocument(transactions);
                doc.GeneratePdf(fileDialog.FileName);
                MessageBoxResult res = MessageBox.Show("Open Report in default application?", "Report View Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    Process pdfViewer = new Process();
                    pdfViewer.StartInfo.UseShellExecute = true;
                    pdfViewer.StartInfo.FileName = fileDialog.FileName;
                    pdfViewer.Start();
                }
            }
        }

        private void RandomizeAmazonTransactions(object sender, RoutedEventArgs e)
        {
            Random rand = new Random();
            foreach (AmazonTransaction at in amazonTransactions.EnumerateTransactions())
            {
                at.Category = rand.Next(AmazonTransaction.Categories.Count);
            }

        }

        private void AmazonSaveCSV_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.FileName = "amazon-transactions.csv";
            if (fileDialog.ShowDialog() == true)
            {
                if (!fileDialog.FileName.EndsWith(".csv"))
                {
                    fileDialog.FileName += ".csv";
                }
                amazonTransactions.ExportSelectedAsCSV(fileDialog.FileName);
            }
        }

        private void AmazonLoadCSV_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.ShowReadOnly = true;

            if (fileDialog.ShowDialog() == true)
            {
                if (!fileDialog.FileName.EndsWith(".csv"))
                {
                    MessageBox.Show("File is not a csv", "Incorrect File Format");
                }

                var newTransactions = AmazonTransactionTable.FromCSV(fileDialog.FileName);
                amazonTransactions.ClearAll();
                amazonTransactions.AddTransactions(newTransactions);
            }
        }

        private void AmazonLabelOrders_Click(object sender, RoutedEventArgs eventArgs)
        {
            AmazonTransactionLabeler labelWindow = new AmazonTransactionLabeler(this, amazonTransactions.GetSelectedTransactions().ToList());
            if(!labelWindow.IsClosed)
            {
                labelWindow.ShowDialog();
            }
        }

        public void SetCategory(string transactionID, int categoryIdx)
        {
            amazonTransactions.SetCategory(transactionID, categoryIdx);
        }

        public void SetCategory(int transactionIdx, int categoryIdx)
        {
            amazonTransactions.SetCategory(transactionIdx, categoryIdx);
        }

        public void RefreshTransactionDisplay()
        {
            amazonTransactions.RaiseCollectionChanged();
        }
    }
}
