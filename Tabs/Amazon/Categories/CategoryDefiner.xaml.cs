using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

using SpendingInfo.Dialogs;
using SpendingInfo.Transactions.Transactions;

namespace SpendingInfo.Tabs.Amazon.Categories
{
    /// <summary>
    /// Interaction logic for CategoryDefiner.xaml
    /// </summary>
    public partial class CategoryDefiner : Window
    {
        protected ObservableCollection<string> categories = new ObservableCollection<string>();

        public IList<string> Categories 
        { 
            get { return categories; } 
        }

        public CategoryDefiner(IReadOnlyList<string>? defaultCategories=null)
        {
            defaultCategories = defaultCategories ?? AmazonTransaction.Categories;
            foreach(string category in defaultCategories)
            {
                categories.Add(category);
            }

            InitializeComponent();
            categoriesBox.SelectedIndex = -1;
            categoriesBox.ItemsSource = categories;
        } 

        protected string GetDefaultCategoryName()
        {
            return $"category{categories.Count()}";
        }

        private void RemoveCategory(int idx)
        {
            categories.RemoveAt(idx);
        }

        private void doneBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            int idx = categoriesBox.SelectedIndex;
            RemoveCategory(idx);
        }

        private void newCatBtn_Click(object sender, RoutedEventArgs e)
        {
            string defaultCat = this.GetDefaultCategoryName();
            AddCategoryDialog newCatDiag = new AddCategoryDialog(defaultCat);
            if(newCatDiag.ShowDialog() == true)
            {
                string cat = newCatDiag.Response;
                categories.Add(cat);
            }

            this.categoriesBox.Items.Refresh();
        }
    }
}
