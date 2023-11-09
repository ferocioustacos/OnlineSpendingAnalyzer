using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
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
using System.Windows.Shapes;

namespace SpendingInfo.Dialogs
{
    /// <summary>
    /// Interaction logic for TextInputDialolg.xaml
    /// </summary>
    public partial class AddCategoryDialog : Window
    {

        public string Response 
        {
            get { return inputText.Text; }
            set {  inputText.Text = value; }
        }

        public AddCategoryDialog(string defaultInput)
        {
            InitializeComponent();
            this.inputText.Text = defaultInput;
            this.inputText.SelectAll();
            this.inputText.Focus();
        }

        protected void okBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        protected void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
