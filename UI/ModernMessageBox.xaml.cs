using System.Windows;

namespace WinAutoDarkMode.UI
{
    public partial class ModernMessageBox : Window
    {
        public ModernMessageBox(string message, string title = "提示")
        {
            InitializeComponent();
            MessageText.Text = message;
            this.Title = title;
            
            this.Loaded += (s, e) => {
                bool isDark = System.Windows.Application.Current.Resources["MainTextBrush"].ToString().Contains("#FFFFFFFF");
                WinAutoDarkMode.Core.WindowHelper.EnableBlur(this, isDark);
            };
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public static void Show(Window owner, string message)
        {
            var msg = new ModernMessageBox(message);
            msg.Owner = owner;
            msg.ShowDialog();
        }
    }
}
