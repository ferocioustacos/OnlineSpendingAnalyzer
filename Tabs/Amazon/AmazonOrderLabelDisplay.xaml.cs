using SpendingInfo.Transactions.Transactions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for AmazonOrderLabelDisplay.xaml
    /// </summary>
    public partial class AmazonOrderLabelDisplay : UserControl, INotifyPropertyChanged
    {
        // INotifyPropertyChanged interface
        public event PropertyChangedEventHandler? PropertyChanged;
        private protected void NotifyPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        AmazonTransaction? transaction = null;
        public AmazonTransaction? Transaction
        {
            get { return transaction; }
            set
            {
                if(value == null || transaction == value) return;

                ASIN = value.ASIN;
                ID = value.ID;
                Date = value.Date;
                Description = value.Description;
                Category = value.CategoryName;
                NotifyPropertyChanged(nameof(Transaction));
            }
        }

        public string ASIN
        {
            get { return orderASINField.Text; }
            set { orderASINField.Text = value; }
        }

        public string ID
        {
            get { return orderIDField.Text; }
            set { orderIDField.Text = value; }
        }

        public DateTime Date 
        {
            get { return transaction != null ? transaction.Date : DateTime.UnixEpoch; }
            set { orderDateField.Text = value.ToShortDateString(); }
        }

        public string Description
        {
            get { return orderDescField.Text; }
            set { orderDescField.Text = value; }
        }

        public string Category
        {
            get { return orderCategoryField.Text; }
            set { orderCategoryField.Text = value; }
        }

        public void SetCategory(string category)
        {
            orderCategoryField.Text = category;
            NotifyPropertyChanged(orderCategoryField.Text);
        }

        public AmazonOrderLabelDisplay()
        {
            InitializeComponent();
        }

        public AmazonOrderLabelDisplay(AmazonTransaction transaction)
        {
            InitializeComponent();
            Transaction = transaction;
        }

        public void RefreshDisplay()
        {
            NotifyPropertyChanged("Transaction");
        }
    }
}
