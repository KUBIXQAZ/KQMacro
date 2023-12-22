using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;
 
namespace KQMacro.Custom
{
    public partial class CustomNumberInputMessageBox : Window
    {
        public int selectedNumber;
        public CustomNumberInputMessageBox(string _title)
        {
            InitializeComponent();

            title.Text = _title;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^0*([1-9][0-9]*)?$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            selectedNumber = int.Parse(UserInput.Text);
            if (selectedNumber == 0) selectedNumber = 1;
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
