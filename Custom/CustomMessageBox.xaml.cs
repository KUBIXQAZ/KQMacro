using System.Windows;

namespace KQMacro.Custom
{
    public partial class CustomMessageBox : Window
    {
        public string SelectedOption { get; private set; }
        public CustomMessageBox(string[] options)
        {
            InitializeComponent();

            comboBoxOptions.ItemsSource = options;
            comboBoxOptions.SelectedIndex = 0;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            SelectedOption = comboBoxOptions.SelectedItem?.ToString();
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
