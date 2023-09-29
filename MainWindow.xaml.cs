using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SpendingInfo.Transactions.Classifier;
using SpendingInfo.Transactions.ReportDocuments;
using SpendingInfo.Transactions.Tables;
using SpendingInfo.Transactions.Transactions;
using SpendingInfo.Transactions.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SpendingInfo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        BankTransactionTable bankTransactions;

        // information for the amazon tab
        AmazonTransactionTable amazonTransactions;
        AmazonClassifier amazonClassifier = new AmazonClassifier();

        DateTime amazonStartDate = DateTime.UnixEpoch;
        DateTime amazonEndDate = DateTime.Now;

        public MainWindow()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            bankTransactions = new BankTransactionTable();
            amazonTransactions = new AmazonTransactionTable();

            InitializeComponent();
            bankTable.ItemsSource = bankTransactions;
            amazonTable.ItemsSource = amazonTransactions;
        }

        private void ClearTable(object sender, RoutedEventArgs e)
        {
            bankTransactions.Clear();
        }

        private void ShowBankStats_Click(object sender, RoutedEventArgs e)
        {
            List<BankTransaction> transactions = bankTransactions.GetAllTransactions().ToList();
            (float income, float loss, float net) = BankUtil.BankMath.CalculateStatistics(transactions);
            String content = $"Income: {income}\nLosses: {loss}\nNet Income/Loss: {net}";
            MessageBox.Show(content, "Income Summary");
        }

        private void BankReport_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.FileName = "report.pdf";
            if(fileDialog.ShowDialog() == true)
            {
                ICollection<BankTransaction> transactions = bankTransactions.GetAllTransactions();
                BankUtil.BankTransactionDocument doc = new BankUtil.BankTransactionDocument(transactions);
                doc.GeneratePdf(fileDialog.FileName);
                MessageBoxResult res = MessageBox.Show("Open Report in default application?", "Report View Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if(res == MessageBoxResult.Yes)
                {
                    //Process.Start("file:///" + fileDialog.FileName);
                }
            }

        }

        private void BankCSV_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.ShowReadOnly = true;

            if (fileDialog.ShowDialog() == true)
            {
                String fileName = fileDialog.FileName;

                // TODO: debug this line (why does it not have any keys during reload?)
/*                
                HashSet<String> loadedFiles = bankTransactions.LoadedFiles().ToHashSet();
                bool reloadFlag = Util.CheckReload(fileName, loadedFiles, true);
*/                
                List<BankTransaction>? currentTransactions = BankUtil.LoadBankCSV(fileDialog);

                if (currentTransactions != null)
                {
/*                    if(reloadFlag)
                    {
                        // remove old transactions
                        bankTransactions.RemoveTransactions(fileName);
                    }
*/
                    /*                    // add to observable, bank, and general transaction table
                                        foreach (BankTransaction bt in currentTransactions) bankTransactions.Add(bt);
                                        AddToBankTransactions(fileDialog.FileName, currentTransactions);
                                        AddToTransactionTable(fileDialog.FileName, currentTransactions.Cast<Transaction>().ToList());*/
                    bankTransactions.AddTransactions(currentTransactions);
                    Debug.WriteLine($"Got {currentTransactions.Count()} transactions excluding balance updates.");
                }

            }
            else // fileDialog.ShowDialog() returned false
            {
                Debug.WriteLine("Did not select a file.");
            }

        }


        private void AmazonLoadZip_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.ShowReadOnly = true;

            if(fileDialog.ShowDialog() == true)
            {
                String fileName = fileDialog.FileName;
                
/*                HashSet<String> loadedFiles = amazonTransactions.LoadedFiles().ToHashSet();
                bool reloadFlag = Util.CheckReload(fileName, loadedFiles, true);
*/                
                ICollection<AmazonTransaction> currentTransactions = AmazonUtil.LoadFromZIP(fileName);

/*                if(reloadFlag) { amazonTransactions.RemoveTransactions(fileName); }
*/
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
            Debug.WriteLine(amazonSearchBar.Text);
            //UpdateAmazonSelection(amazonSearchBar.Text);
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
                    //Process.Start("file:///" + fileDialog.FileName);
                }
            }
        }

        private void RandomizeAmazonTransactions(object sender, RoutedEventArgs e)
        {
            Random rand = new Random();
            foreach(AmazonTransaction at in amazonTransactions.EnumerateTransactions())
            {
                at.Category = rand.Next(AmazonTransaction.Categories.Count);
            }

        }

        private void AmazonSaveCSV_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.FileName = "amazon-transactions.csv";
            if(fileDialog.ShowDialog() == true)
            {
                if(!fileDialog.FileName.EndsWith(".csv"))
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

            if(fileDialog.ShowDialog() == true)
            {
                if(!fileDialog.FileName.EndsWith(".csv"))
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
            
        }
    }
}
