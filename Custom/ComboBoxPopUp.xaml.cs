using System.Windows;

namespace KQMacro.Custom
{
    public partial class ComboBoxPopUp : Window
    {
        public string SelectedOption { get; set; }

        public ComboBoxPopUp(string windowTitle, string content, string[] options)
        {
            InitializeComponent();

            Title = windowTitle;
            this.content.Text = content;
            comboBox.ItemsSource = options;
            comboBox.SelectedIndex = 0;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            SelectedOption = comboBox.SelectedItem?.ToString();
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