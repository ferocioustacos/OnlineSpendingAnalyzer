using Microsoft.Win32;
using SpendingInfo.Transactions.Transactions;
using System;
using System.Collections.Generic;
using System.IO.Packaging;
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

namespace SpendingInfo.Tabs.Amazon.Categories
{
    /// <summary>
    /// Interaction logic for CategoryLoader.xaml
    /// </summary>
    [Obsolete("Removed in favor of populating CategoryDefiner entries with those from file")]
    public partial class CategoryLoader : Window
    {
        AmazonTabContent parent;
        IList<string> categories = new List<string>();
        public CategoryLoader(AmazonTabContent parent, IList<string> paths)
        {
            this.parent = parent;
            InitializeComponent();
            this.categories = LoadCategories(paths);
            loadedCategories.ItemsSource = categories;
        }

        [Obsolete("Create new CategoryLoader window instead, this function does not prompt for user confirmation.")]
        public static void Load()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Text and Data files (*.txt, *.dat)|*.txt;*.dat" + "|All files (*.*)|*.*";
            dialog.ShowReadOnly = true;
            dialog.Multiselect = true;
            if(dialog.ShowDialog() == true)
            {
                AmazonTransaction.Categories = new List<string>(); // clear Categories
                foreach(String file in dialog.FileNames)
                {
                    IList<string> categories = (IList<string>)AmazonTransaction.CategoryUtil.ReadCategories(file);
                    AmazonTransaction.CategoryUtil.AddCategories(categories);
                }
            }
        }

        private List<string> LoadCategories(IList<string> paths)
        {
            List<string> categories = new List<string>();
            foreach(string file in paths)
            {
                categories.AddRange(AmazonTransaction.CategoryUtil.ReadCategories(file));
            }
            return categories;
        }

        private void confirmButton_Click(object sender, RoutedEventArgs e)
        {
            AmazonTransaction.Categories = new List<string>();
            AmazonTransaction.CategoryUtil.AddCategories(this.categories);
            this.Close();
        }

        private void MenuItemDelete_Click(object sender, object e)
        {
            string deletedCategory = categories.ElementAt(loadedCategories.SelectedIndex);
            categories.RemoveAt(loadedCategories.SelectedIndex);
            loadedCategories.Items.Refresh();
            MessageBox.Show($"Deleted category {deletedCategory}");
        }
    }
}
