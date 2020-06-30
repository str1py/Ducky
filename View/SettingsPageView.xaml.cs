using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;
namespace Ducky.View
{
    /// <summary>
    /// Логика взаимодействия для SettingsPageView.xaml
    /// </summary>
    public partial class SettingsPageView : UserControl
    {
        public SettingsPageView()
        {
            InitializeComponent();
        }

        private void Vkpass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            { ((dynamic)this.DataContext).VkPasswordText = ((WatermarkPasswordBox)sender).Password; }
        }
    }
}
