using SpendingInfo.Transactions.Tables;
using SpendingInfo.Transactions.Transactions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using static SpendingInfo.Transactions.Transactions.AmazonTransaction;

namespace SpendingInfo.Tabs.Amazon
{
    /// <summary>
    /// Interaction logic for AmazonTransactionLabeler.xaml
    /// </summary>
    public partial class AmazonTransactionLabeler : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private protected void NotifyPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private IList<AmazonTransaction> transactionList;
        private int transactionIndex = 0;

        public bool IsClosed { get; private set; }

        private Button[] defaultButtons;
        private List<Button> enabledButtons;

        private IList<string> categoryNames;

        private AmazonTabContent parentWindow;

        public AmazonTransactionLabeler(AmazonTabContent parentWindow, IList<AmazonTransaction> amazonTransactions)
        {
            InitializeComponent();

            // keeps track from where the transactions came from so it can update the appropriate table
            this.parentWindow = parentWindow;

            // Initialize transaction display
            // TODO: fix bug where it shows '1/X' on first transaction
            transactionList = amazonTransactions;
            ValidateTransactionsInput();
            SetTransactionDisplay(0);

            // Category Selection
            defaultButtons = new Button[10] 
                { 
                    category1Button, category2Button, category3Button, category4Button, category5Button,                     
                    category6Button, category7Button, category8Button, category9Button, category10Button, 
                };

            categoryNames = new List<string>();
            enabledButtons = new List<Button>();
            DetermineCategoryInput();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsClosed = true;
        }

        private void cancelLabelButton_Click(object sender, RoutedEventArgs e)
        {
            parentWindow.RefreshTransactionDisplay();
            this.Close();
        }

        [Obsolete("Fails to maintain transaction counter")]
        private void SetTransactionDisplay(AmazonTransaction transaction)
        {
            if(transaction == null) { return; }
            transactionDisplay.Transaction = transaction;
        }

        private void SetTransactionDisplay(int idx)
        {
            AmazonTransaction transaction = transactionList[idx];
            transactionDisplay.Transaction = transaction;
            transactionCounterBlock.Text = $"{idx+1}/{transactionList.Count()}";
            NotifyPropertyChanged(transactionCounterBlock.Text);
        }

        private bool ValidateTransactionsInput()
        {
            if (transactionList.Count() == 0)
            {
                MessageBox.Show("No currently selected transactions");
                this.Close();
                return false;
            }

            return true;
        }

        // determines how many category button to show, and if a listbox is necessary
        // if we can just use the buttons, then update button text to 
        private void DetermineCategoryInput()
        {
            if (AmazonTransaction.Categories.Count() > defaultButtons.Length)
            {
                SwitchToListBoxInput();
            } else
            {
                HideExtraButtons();
                LabelButtons();
                RegisterButtons();
            }
        }

        private void HideButton(int buttonIdx)
        {
            // differentiate unknown with btnIdx of -1
            if(buttonIdx == -1)
            {
                unknownButton.Visibility = Visibility.Collapsed;
                return;
            }

            defaultButtons[buttonIdx].Visibility = Visibility.Collapsed;
        }

        private void ShowComboBoxSelection()
        {

            comboxBoxSelector.Visibility = Visibility.Visible;
            comboBoxPanel.Visibility = Visibility.Visible;
            foreach (string category in AmazonTransaction.Categories)
            {
                categoryNames.Add(category);
            }
            categoryNames.Add(AmazonTransaction.UNKNOWN_CATEGORY);
            comboxBoxSelector.ItemsSource = categoryNames;
            comboxBoxSelector.SelectedIndex = -1;
        }

        private void SwitchToListBoxInput()
        {
            for(int i = 0; i < defaultButtons.Length; i++)
            {
                HideButton(i);
            }
            HideButton(-1); // hides unknown button

            ShowComboBoxSelection();
            NotifyPropertyChanged(nameof(comboBoxConfirm));
        }

        private void HideExtraButtons()
        {
            for(int i = AmazonTransaction.Categories.Count; i < defaultButtons.Length; i++)
            {
                HideButton(i);
                Debug.WriteLine($"Category Button {i} disabled.");
            }
        }

        private void LabelButtons()
        {
            for (int i = 0; i < AmazonTransaction.Categories.Count; i++)
            {
                var btn = defaultButtons[i];
                btn.Content = AmazonTransaction.Categories[i];
            }
        }

        private void RegisterButtons()
        {
            for(int i = 0; i < AmazonTransaction.Categories.Count;i++)
            {
                var btn = defaultButtons[i];
                enabledButtons.Add(btn);
            }
            enabledButtons.Add(unknownButton);
        }

        private void SetCategory(int categoryIdx)
        {
            int idx = this.transactionIndex;
            parentWindow.SetCategory(idx, categoryIdx);
            transactionList.ElementAt(idx).Category = categoryIdx;
            SetTransactionDisplay(idx);
        }

        private void prevTransaction()
        {
            if (transactionIndex == 0) return;
            transactionIndex = transactionIndex - 1 % transactionList.Count();
            SetTransactionDisplay(transactionIndex);
        }

        private void prevTransaction_Click(object sender, RoutedEventArgs e)
        {
            prevTransaction();
        }

        private void nextTransaction()
        {
            if (transactionIndex == transactionList.Count() - 1) return;
            transactionIndex = transactionIndex + 1 % transactionList.Count();
            SetTransactionDisplay(transactionIndex);
        }
        private void nextTransaction_Click(object sender, RoutedEventArgs e)
        {
            nextTransaction();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.KeyDown += HandleKeyPress;
        }

        private void HandleKeyPress(object sender, KeyEventArgs key)
        {
            switch(key.Key)
            {
                case Key.Left:
                    prevTransaction_Click(new object(), new RoutedEventArgs()); 
                    break;
                case Key.Right:
                    nextTransaction_Click(new object(), new RoutedEventArgs());
                    break;
            }
        }

        // TODO: switch dir to be a enum or something similar for readability
        public void SetCategoryHandler(int categoryIdx, bool dir=true) 
        {
            SetCategory(categoryIdx);
            if(dir)
            {
                nextTransaction();
            } else
            {
                prevTransaction();
            }
        }

        private void category1Button_Click(object sender, RoutedEventArgs e) { SetCategoryHandler(0); }
        private void category2Button_Click(object sender, RoutedEventArgs e) { SetCategoryHandler(1); }
        private void category3Button_Click(object sender, RoutedEventArgs e) { SetCategoryHandler(2); }
        private void category4Button_Click(object sender, RoutedEventArgs e) { SetCategoryHandler(3); }
        private void category5Button_Click(object sender, RoutedEventArgs e) { SetCategoryHandler(4); }
        private void category6Button_Click(object sender, RoutedEventArgs e) { SetCategoryHandler(5); }
        private void category7Button_Click(object sender, RoutedEventArgs e) { SetCategoryHandler(6); }
        private void category8Button_Click(object sender, RoutedEventArgs e) { SetCategoryHandler(7); }
        private void category9Button_Click(object sender, RoutedEventArgs e) { SetCategoryHandler(8); }
        private void category10Button_Click(object sender, RoutedEventArgs e) { SetCategoryHandler(9); }
        private void unknownButton_Click(object sender, RoutedEventArgs e) { SetCategoryHandler(-1); }

        private void confirmCategory_click(object sender, RoutedEventArgs e)
        {
            int categoryIdx = comboxBoxSelector.SelectedIndex;
            SetCategory(categoryIdx);
            nextTransaction();
        }

        private void categoryConfirm_Click(object sender, RoutedEventArgs e)
        {
            int categoryIdx = comboxBoxSelector.SelectedIndex;
            SetCategory(categoryIdx);
            NotifyPropertyChanged("transactionList");
            transactionDisplay.RefreshDisplay();
        }
    }
}
