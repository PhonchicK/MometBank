using System.Configuration;
using System.Data;
using System.Windows;

namespace MometBank
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.Resources.Add("ByteArrayToImageConverter", new ByteArrayToImageConverter());
        }
    }

}
