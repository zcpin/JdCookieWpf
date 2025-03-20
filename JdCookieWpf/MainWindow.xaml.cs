using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using JdCookieWpf.ViewModels;

namespace JdCookieWpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();

        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        try
        {
            await WebView.EnsureCoreWebView2Async();
            WebView.CoreWebView2.Navigate("https://plogin.m.jd.com/login/login");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            throw; // TODO handle exception
        }
    }
}