using CsvHelper;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SpendingInfo.Transactions.Transactions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;

namespace SpendingInfo.Transactions.Utils
{
    internal static class BankUtil
    {
        private const String BANK_DAILY_LEDGER_DESC = "Daily Ledger Bal";

        private static IReadOnlyList<String> DATE_FORMATS { get; } = new String[]{ "M/d/yyyy", "M/dd/yyyy", "MM/d/yyyy", "MM/dd/yyyy" };

        public static List<BankTransaction>? LoadBankCSV(OpenFileDialog fileDialog, bool reload=true)
        {
            if(!VerifyFileType(fileDialog.FileName)) return null;
            if (!reload) return null;
            
            List<BankTransaction> currentTransactions = new List<BankTransaction>();

            using (Stream file = fileDialog.OpenFile())
            using (TextReader fileReader = new StreamReader(file))
            using (CsvReader csv = new CsvReader(fileReader, CultureInfo.CurrentCulture))
            {
                // initialize csv
                csv.Read();
                csv.ReadHeader();

                while(csv.Read())
                {
                    try
                    {
                        BankTransaction? transaction = ReadNextBankTransaction(csv, fileDialog.FileName);
                        if (transaction != null)
                        {
//                            Debug.WriteLine(transaction);
                            currentTransactions.Add(transaction);
                        }
                    } 
                    catch(BankCSVFormatException ex) {
                        throw ex;
                    }
                }
            }

            return currentTransactions;
        }

        // check that file is a csv
        private static bool VerifyFileType(String fileName)
        {
            if(!fileName.EndsWith(".csv"))
            {
                Debug.WriteLine("[DEBUG] Got invalid file: not csv.");
                MessageBox.Show("File must be a CSV.", "Invalid File Type");
                return false;
            }

            return true;
        }

        // check that file has { description: String, transactionDate: DateTime/String, etc }
        // true if valid, false if invalid
        private static bool VerifyFileContents(CsvReader csv)
        {
            return true;
        }


        private static BankTransaction? ReadNextBankTransaction(CsvReader csv, String fileName)
        {
            // get required fields
            String? dateStr, descr;
            float amount, balance;

            try
            {
                // check date
                dateStr = csv.GetField("Date");
                if (dateStr == null) return null;
            } catch (CsvHelperException csvError)
            {
                Debug.WriteLine(csvError.Message);
                throw new BankCSVFormatException("Failed to read date from file.");
            }

            // get date information
            DateTime date = DateTime.ParseExact(dateStr.Trim(), (string[]) DATE_FORMATS, CultureInfo.CurrentCulture);
//            DateTime date = DateTime.Parse(dateStr.Trim(), CultureInfo.CreateSpecificCulture("en-US"));

            Debug.WriteLine(dateStr);
            Debug.WriteLine(date.ToString());

            try
            {
                descr = csv.GetField("Description");
                // didn't have description, invalid transaction
                if (descr == null) return null;

                // make sure transaction is real and not update garbage
                if (descr.Equals(BANK_DAILY_LEDGER_DESC)) return null;
            }
            catch (CsvHelperException csvError)
            {
                Debug.WriteLine(csvError.Message);
                throw new BankCSVFormatException("Failed to read description from file.");
            }

            try { amount = csv.GetField<float>("Amount"); }
            catch(CsvHelperException csvError) { Debug.WriteLine(csvError.Message); throw new BankCSVFormatException("Failed to read transaction amount from file."); }

            try { balance = csv.GetField<float>("Balance"); }
            catch (CsvHelperException csvError) { Debug.WriteLine(csvError.Message); throw new BankCSVFormatException("Failed to read account balance from file."); }

            BankTransaction transaction = new BankTransaction(date, amount, balance, descr, fileName);

            // get and add optional fields
            try
            {
                String? refCheck = csv.GetField<String>("Ref/Check");
                String? memo = csv.GetField<String>("Memo");
                String? category = csv.GetField<String>("Category");
                if (Util.CheckValidValue(refCheck, "")) transaction.refCheck = refCheck;
                if (Util.CheckValidValue(memo, "")) transaction.transactionMemo = memo;
                if (Util.CheckValidValue(category, "")) transaction.transactionCategory = category;
            }
            catch (BankCSVFormatException ex)
            {
                // nonfatal error, catch and ignore (can fail safely without major impact)
                Debug.WriteLine("[EXCEPTION] Failed to get optional information from csv.");
                Debug.WriteLine(ex);
            }

//            Debug.WriteLine(dateStr.Trim());
            return transaction;
        }



        public static class BankMath
        {
            // returns [gross income (+), losses (+), net income/loss (+/-)
            public static (float, float, float) 
                CalculateStatistics(List<BankTransaction> bankTransactions)
            {
                List<float> incomes = new List<float>();
                List<float> losses = new List<float>();

                float income = 0;
                float loss = 0;
                foreach (BankTransaction bt in bankTransactions)
                {
                    if (bt.IsDebit())
                    {
                        income += bt.transactionAmount;
                        incomes.Add(bt.transactionAmount);
                    } else
                    {
                        loss += bt.transactionAmount;
                        losses.Add(bt.transactionAmount);
                    }
                }
/*
                float meanIncome = incomes.Average();
                float meanLoss = losses.Average();

                float maxIncome = Math.Abs(incomes.Max());
                float minIncome = Math.Abs(incomes.Min());

                float maxLoss = Math.Abs(losses.Min());
                float minLoss = Math.Abs(losses.Max());
*/
                income = Math.Abs(income);
                loss = Math.Abs(loss);
                float net = income - loss;
//                String content = $"Income: {income}\nLosses: {loss}\nNet Income/Loss: {net}";

                return (income, loss, net);
            }
        }


        public class BankTransactionsModel
        {
            public ICollection <BankTransaction> transactions { get; }
            public DateTime modelCreateTime { get; }

            public BankTransactionsModel(ICollection <BankTransaction> transactions)
            {
                this.transactions = transactions;
                modelCreateTime = DateTime.Now;
            }
        }

        public class BankTransactionDocument : IDocument
        {
            public BankTransactionsModel model { get; }

            public static int MAX_DESC_LENGTH = 45; // maximum number of characters that can conformtably fit on a single line
            public BankTransactionDocument(ICollection<BankTransaction> transactions)
            {
                model = new BankTransactionsModel(transactions);
            }

            public BankTransactionDocument(BankTransactionsModel model)
            {
                this.model = model;
            }

            public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
            public DocumentSettings GetSettings() => DocumentSettings.Default;

            public void Compose(IDocumentContainer container)
            {
                container
                    .Page(page =>
                    {
                        page.Margin(10);
                        page.Header().Element(ComposeHeader);
                        page.Content().Element(ComposeContent);
                    });
            }

            void ComposeHeader(IContainer container)
            {
                TextStyle titleStyle = TextStyle.Default.FontSize(20).SemiBold().FontColor(Colors.Green.Darken2);

                container.Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text("Bank Transaction Report").Style(titleStyle);

                        column.Item().Text(text =>
                        {
                            text.Span("Report Creation Date: " + model.modelCreateTime.ToString()).SemiBold();
                        });
                    });
                });
            }

            void ComposeContent(IContainer container)
            {
                container.PaddingVertical(20).Column(column =>
                {
                    column.Spacing(5);

                    column.Item().Element(ComposeTransactions);
                });
            }

            void ComposeTransactions(IContainer container)
            {
                container.Table(table =>
                {
                    int dateLen = 75;
                    int refCheckLen = 50;
                    float descProportion = 3; //  (3/# of cols) == (3/4)
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(dateLen);
                        columns.ConstantColumn(refCheckLen);
                        columns.RelativeColumn(descProportion);
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Date");
                        header.Cell().Element(CellStyle).Text("Ref/\nCheck");
                        header.Cell().Element(CellStyle).Text("Description");
                        header.Cell().Element(CellStyle).Text("Amount");
                        
                        static IContainer CellStyle(IContainer container)
                        {
                            return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).Border(1).BorderLeft(1).BorderRight(1).BorderTop(1).BorderColor(Colors.Black).PaddingHorizontal(5);
                        }
                    });

                    bool truncateDescription = false;
                    bool requestedTruncation = false;
                    foreach(BankTransaction transaction in model.transactions)
                    {
                        DateTime date = transaction.transactionDate;
                        String refCheck = transaction.GetRefCheck();
                        String desc = transaction.transactionDescription;
                        float amount = transaction.transactionAmount;


                        table.Cell().Element(CellStyle).Text(date.ToString("MM/dd/yyyy"));
                        table.Cell().Element(CellStyle).Text(refCheck);

                        if(desc.Length > MAX_DESC_LENGTH)
                        {
                            if(!requestedTruncation)
                            {
                                var res = MessageBox.Show("Description of excessive length found. Truncate description?\n(Truncating will ensure that no transaction is split across pages)", "Truncate Description", MessageBoxButton.YesNo);
                                if(res == MessageBoxResult.Yes)
                                {
                                    truncateDescription = true;
                                }
                                requestedTruncation = true;
                            }

                            if(truncateDescription)
                            {
                                desc = desc.Substring(0, MAX_DESC_LENGTH);
                            }
                        }

                        table.Cell().Element(CellStyle).Text(desc);

                        // logic for formatting credits
                        if(amount < 0.0f)
                        {
                           table.Cell().Element(CellStyle).AlignLeft().Text($"-${Math.Abs(amount)}");
                        } else
                        {
                            table.Cell().Element(CellStyle).AlignLeft().Text($"${(amount)}");
                        }

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.BorderBottom(1).Border(1).BorderColor(Colors.Black).PaddingVertical(5).PaddingHorizontal(5);
                        }
                    }

                });
            }
        }

    }
}
