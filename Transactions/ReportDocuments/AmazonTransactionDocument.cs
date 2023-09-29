using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SpendingInfo.Transactions.Transactions;
using System;
using System.Collections.Generic;
using System.Windows;

namespace SpendingInfo.Transactions.ReportDocuments
{
    public class AmazonTransactionDocument : IDocument
    {
        public AmazonTransactionModel model { get; }
        public static int MAX_DESC_LENGTH = 45;

        public AmazonTransactionDocument(ICollection<AmazonTransaction> transactions)
        {
            model = new AmazonTransactionModel(transactions);
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
                    column.Item().Text(text => { text.Span("Report Creation Date: " + model.ModelCreateTime.ToString()).SemiBold(); });
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
                foreach (AmazonTransaction transaction in model.Transactions)
                {
                    DateTime date = transaction.Date;
                    String desc = transaction.Description;
                    float amount = transaction.Amount;


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
