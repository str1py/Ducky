using System;
using System.Windows;
using System.Windows.Controls;

namespace Ducky.Helpers
{
    public class WebBrowserExtension
    {
        public static WebBrowser webBrowser;
        public static readonly DependencyProperty BindableSourceProperty =
                               DependencyProperty.RegisterAttached("BindableSource", typeof(string),
                               typeof(WebBrowserExtension), new UIPropertyMetadata(null,
                               BindableSourcePropertyChanged));
        public static WebBrowser GetWebBrowser()
        {
            return webBrowser;
        }
        public static string GetBindableSource(DependencyObject obj)
        {
            return (string)obj.GetValue(BindableSourceProperty);
        }

        public static void SetBindableSource(DependencyObject obj, string value)
        {
            obj.SetValue(BindableSourceProperty, value);
        }

        public static void BindableSourcePropertyChanged(DependencyObject o,
                                                         DependencyPropertyChangedEventArgs e)
        {
            webBrowser = (WebBrowser)o;
            try
            {
                webBrowser.Navigate((string)e.NewValue);
            }
            catch { }
        }


        
    }
}
