﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;
using System.Printing.IndexedProperties;
using System.Diagnostics;
using CsvHelper;
using System.Globalization;
using System.CodeDom;
using System.Windows.Documents;
using System.Runtime.Intrinsics.X86;

using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Windows;

namespace SpendingInfo.Transactions.Utils
{
    internal static class AmazonUtil
    {
        // returns path of the file
        public static String[] DEFAULT_ORDER_FILENAMES { get; } = { "OrderHistory" };
        public static String[] DEFAULT_RETURN_FILENAMES { get; } = { "CustomerReturns", "OrdersReturned" };
        public static String[] DEFAULT_ORDER_ID_FORMATS { get; } = { "OrderId", "Order ID", "Order Id" };
        public static String[] OrderFileNames { get; set; } = DEFAULT_ORDER_FILENAMES;
        public static String[] ReturnFileNames { get; set; } = DEFAULT_RETURN_FILENAMES;

        public static String[] OrderIdFormats { get; set; } = DEFAULT_ORDER_ID_FORMATS;

        public static ICollection<AmazonTransaction> LoadFromZIP(String zipFilePath, bool reload=true, bool ignoreReturned=true)
        {
            List<AmazonTransaction> amazonTransactions= new List<AmazonTransaction>();
            if(!reload) { return amazonTransactions; }

            using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
            {
                String tempFolderPath = Path.GetTempPath();
                List<ZipArchiveEntry> orderEntries = new List<ZipArchiveEntry>();
                List<ZipArchiveEntry> returnEntries = new List<ZipArchiveEntry>();


                foreach(ZipArchiveEntry entry in archive.Entries)
                {
                    String fileName = Path.GetFileName(entry.FullName);
                    _AmazonFileType fileType = GetAmazonFileType(fileName);
                    if (ShouldExtract(fileType))
                    {
                        if (!Path.GetExtension(entry.FullName).ToLowerInvariant().Equals(".csv"))
                        {
                            continue; // if not a csv, ignore it
                        }

                        if(fileType == _AmazonFileType.ORDER) orderEntries.Add(entry);
                        else returnEntries.Add(entry);
                    }
                }

                // 1.) Get all returned orders
                HashSet<String> returnedOrders = new HashSet<string>();
                foreach(ZipArchiveEntry entry in returnEntries)
                {
                    returnedOrders.UnionWith(ReadReturnedOrdersFromZIP(entry));
                }

                // 2.) Get all orders that have not been returned
                foreach(ZipArchiveEntry entry in orderEntries)
                {
                    IEnumerable<AmazonTransaction> transactions = ReadPurchasesFromZIP(entry);
                    amazonTransactions.AddRange(transactions.Where(t => !returnedOrders.Contains(t.id)));
                }
            }

            return amazonTransactions;
        }

        private static FileStream ExtractTempFile(ZipArchiveEntry entry,
            FileMode fileMode = FileMode.Open,
            FileAccess fileAccess = FileAccess.Read,
            FileShare fileShare = FileShare.Read,
            int bufferSize = 4096,
            FileOptions fileOptions=FileOptions.SequentialScan | FileOptions.DeleteOnClose)
        {
            String tempPath = Path.GetTempPath();
            String entryName = entry.Name;
            String filePath = Path.Combine(tempPath, entryName);
            entry.ExtractToFile(filePath);
            return new FileStream(filePath, fileMode, fileAccess, fileShare, bufferSize, fileOptions);
        }

        private static List<AmazonTransaction> ReadPurchasesFromZIP(ZipArchiveEntry entry)
        {
            List<AmazonTransaction> transactions = new List<AmazonTransaction>();
            using (FileStream fs = ExtractTempFile(entry))
            using (TextReader tr = new StreamReader(fs))
            using (CsvReader csv = new CsvReader(tr, CultureInfo.CurrentCulture))
            {
                // need to call read() before reading header
                csv.Read();
                csv.ReadHeader();

                if (csv.HeaderRecord == null)
                    return transactions;

                String? idField = OrderHeader.GetIDFormat(csv.HeaderRecord);
                if (idField == null)
                    return transactions; // id header not found

                while (csv.Read())
                {
                    AmazonTransaction? transaction = ReadNextAmazonTransaction(csv, idField, entry.FullName);
                    if (transaction == null)
                        continue;

                    transactions.Add(transaction);
                }
            }

            return transactions;
        }



        private static IEnumerable<String> ReadReturnedOrdersFromZIP(ZipArchiveEntry entry)
        {
            HashSet<String> returnedIds = new HashSet<string>();

            using (FileStream fs = ExtractTempFile(entry))
            using (TextReader tr = new StreamReader(fs))
            using (CsvReader csv = new CsvReader(tr, CultureInfo.CurrentCulture))
            {
                csv.Read();
                csv.ReadHeader();

                if (csv.HeaderRecord == null)
                    return returnedIds;

                // figure out which id header the file uses
                String? idField = OrderHeader.GetIDFormat(csv.HeaderRecord);

                if(idField == null)
                    return returnedIds; // id header not found

                while (csv.Read())
                {
                    String? id = csv.GetField(idField);
                    if (id == null) continue;
                    returnedIds.Add(id);
                }
            }

            return returnedIds;
        }

        private struct OrderHeader
        {
            public static string? GetIDFormat(string[] headerRecord)
            {
                foreach (String header in headerRecord)
                    foreach (String format in OrderIdFormats)
                        if (header.Equals(format)) return header;

                return null;
            }

            // header for id varies
            public const string ORDER_DATE = "Order Date";
            public const string TOTAL_COST = "Total Owed";
            public const string ASIN = "ASIN";
            public const string ITEM_NAME = "Product Name";
        }

        private static IReadOnlyList<String> DATE_FORMATS { get; } = new String[] { "yyyy-MM-ddTHH:mm:ssZ" };
        private static AmazonTransaction? ReadNextAmazonTransaction(CsvReader csv, String idField, String fileName)
        {
            String id = "", dateStr ="", itemName = "", asin = "";
            float amount = 0.0f;

            // all items are required, if any fail then absolute fail
            try
            {
                bool gotID = GetField(csv, idField, out id);
                bool gotDateStr = GetField(csv, OrderHeader.ORDER_DATE, out dateStr);
                bool gotItemName = GetField(csv, OrderHeader.ITEM_NAME, out itemName);
                bool gotASIN= GetField(csv, OrderHeader.ASIN, out asin);
                bool gotAmount = GetField(csv, OrderHeader.TOTAL_COST, out amount);

                if( !(gotID && gotDateStr && gotItemName && gotASIN && gotAmount) )
                {
                    return null;
                }

            } catch (Exception ex)
            {
                Debug.WriteLine("Failed to read order. " + ex.Message);
                return null;
            }

            DateTime date = DateTime.UnixEpoch;
            if (!dateStr.Trim().Equals("Not Available"))
                date = DateTime.ParseExact(dateStr.Trim(), (string[])DATE_FORMATS, CultureInfo.CurrentCulture); 

            return new AmazonTransaction(id, date, amount, itemName, asin, fileName);
        }

        // probably not a good way of doing it
        private static bool GetField<T>(CsvReader csv, String field, out T data)
        {
            data = default;
            try
            {
                T? read_data = csv.GetField<T>(field);
                if(read_data == null)
                    return false;
                data = read_data;
            } catch (Exception ex)
            {
                Debug.WriteLine("Failed to get field " + ex.Message);
                return false;
            }
            return true;            
        }


        private enum _AmazonFileType { ORDER, RETURN, UNKNOWN };
        private static _AmazonFileType GetAmazonFileType(String fileName)
        {
            if(OrderFileNames.Any(fileName.Contains))
            {
                return _AmazonFileType.ORDER;
            }
            else if (ReturnFileNames.Any(fileName.Contains))
            {
                return _AmazonFileType.RETURN;
            }

            return _AmazonFileType.UNKNOWN;

        }

        private static bool ShouldExtract(_AmazonFileType type)
        {
            return type == _AmazonFileType.ORDER || type == _AmazonFileType.RETURN;
        }
    }

    public class AmazonTransactionModel
    {
        public ICollection<AmazonTransaction> transactions { get; }
        public DateTime modelCreateTime { get; }

        public AmazonTransactionModel(ICollection<AmazonTransaction> transactions)
        {
            this.transactions = transactions;
            this.modelCreateTime = DateTime.Now;
        }
    }

    public class AmazonTransactionDocument : IDocument
    {
        public AmazonTransactionModel model { get; }
        public static int MAX_DESC_LENGTH = 45;

        public AmazonTransactionDocument(ICollection<AmazonTransaction> transactions)
        {
            this.model = new AmazonTransactionModel(transactions);
        }

        public AmazonTransactionDocument(AmazonTransactionModel model)
        {
            this.model = model;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(10);
                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
            });
        }
        
        void ComposeHeader(IContainer container)
        {
            TextStyle titleStyle = TextStyle.Default.FontSize(20).SemiBold().FontColor(Colors.Blue.Accent4);
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Amazon Purchases Report").Style(titleStyle);
                    column.Item().Text(text => { text.Span("Report Creation Date: " + this.model.modelCreateTime.ToString()).SemiBold(); });
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
        // TODO: Modify this to make segments based on categories
        void ComposeTransactions(IContainer container)
        {
            container.Table(table =>
            {
                int dateLen = 75;
//                int refCheckLen = 75;
                float descProportion = 3; //  (3/# of cols) == (3/4)
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(dateLen);
//                    columns.ConstantColumn(refCheckLen);
                    columns.RelativeColumn(descProportion);
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Date");
//                    header.Cell().Element(CellStyle).Text("Order ID");
                    header.Cell().Element(CellStyle).Text("Description");
                    header.Cell().Element(CellStyle).Text("Amount");

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).Border(1).BorderLeft(1).BorderRight(1).BorderTop(1).BorderColor(Colors.Black).PaddingHorizontal(5);
                    }
                });

                bool truncateDescription = false;
                bool requestedTruncation = false;
                foreach (AmazonTransaction transaction in model.transactions)
                {
                    DateTime date = transaction.date;
                    String desc = transaction.description;
                    float amount = transaction.amount;


                    table.Cell().Element(CellStyle).Text(date.ToString("MM/dd/yyyy"));
//                    table.Cell().Element(CellStyle).Text(transaction.transactionId);
//                    table.Cell().Element(CellStyle).Text(refCheck);

                    if (desc.Length > MAX_DESC_LENGTH)
                    {
                        if (!requestedTruncation)
                        {
                            var res = MessageBox.Show("Description of excessive length found. Truncate description?\n(Truncating will ensure that no transaction is split across pages)", "Truncate Description", MessageBoxButton.YesNo);
                            if (res == MessageBoxResult.Yes)
                            {
                                truncateDescription = true;
                            }
                            requestedTruncation = true;
                        }

                        if (truncateDescription)
                        {
                            desc = desc.Substring(0, MAX_DESC_LENGTH);
                        }
                    }

                    table.Cell().Element(CellStyle).Text(desc);

                    // logic for formatting credits
                    if (amount < 0.0f)
                    {
                        table.Cell().Element(CellStyle).AlignLeft().Text($"-${Math.Abs(amount)}");
                    }
                    else
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
