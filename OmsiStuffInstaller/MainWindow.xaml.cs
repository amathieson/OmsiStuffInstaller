using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using System.Text.Json;
using Microsoft.Win32;
using System.Security.AccessControl;

namespace OmsiStuffInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string API_KEY = "5XKoz7dKU3";
        public MainWindow()
        {

            InitializeComponent();
            Hide();
            try
            {
                string currentImage = Get("https://omsistuff.fr/external/trending?key=" + API_KEY);
                CurrentImages data = JsonSerializer.Deserialize<CurrentImages>(currentImage);
                ImageDescription.Content = data.name;
                ImageAuthorName.Content = data.username;

                BitmapImage imageOfTheMoment = new();
                imageOfTheMoment.BeginInit();
                imageOfTheMoment.UriSource = new Uri(data.image); ;
                imageOfTheMoment.EndInit();
                ImageOfTheMoment.Source = imageOfTheMoment;

                BitmapImage profilePic = new();
                profilePic.BeginInit();
                profilePic.UriSource = new Uri(data.profilPic); ;
                profilePic.EndInit();
                ImageAuthor.Source = profilePic;
                OMSIPath.Text = @"C:\Program Files (x86)\Steam\steamapps\common\OMSI 2\";
                Show();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }

        }

        private string Get(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private void openFile_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog openFileDialog = new();
            openFileDialog.InitialDirectory = OMSIPath.Text;
            openFileDialog.Filter = "OMSI 2|omsi.exe";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                //Get the path of specified file
                this.OMSIPath.Text = openFileDialog.FileName;
                this.OMSIPath.Text = this.OMSIPath.Text.Trim().Substring(0, this.OMSIPath.Text.Length - 8);
            }
        }

        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            InstallButton.IsEnabled = false;
            ProgressIndicatorBar.IsIndeterminate = true;
            StatusLabel.Content = "Downloading Tool...";
            WebClient wc = new();
            wc.DownloadFile(@"https://firebasestorage.googleapis.com/v0/b/objects-omsistuff.appspot.com/o/programs%2Fassistant-d-installation.exe?alt=media", OMSIPath.Text + "assistant-d-installation.exe");
            ProgressIndicatorBar.IsIndeterminate = false;
            ProgressIndicatorBar.Value = 10;

            StatusLabel.Content = "Creating Link...";
            File.WriteAllText(OMSIPath.Text + "confirmer-l-installation.bat", "powershell -Command \"cd '" + OMSIPath.Text + "'; Start-Process '" + OMSIPath.Text + "assistant-d-installation.exe' -Verb runAs\"");

            ProgressIndicatorBar.Value = 20;
            StatusLabel.Content = "Configuring Registry...";
            RegistryKey key = Registry.ClassesRoot.CreateSubKey("omsistuffinstallassist", true);
            key = Registry.ClassesRoot.OpenSubKey("omsistuffinstallassist", true);
            key.SetValue("", "Omsistuff installation automatique");
            key.SetValue("URL Protocol", "");
            ProgressIndicatorBar.Value = 40;
            key.CreateSubKey("shell", true);
            key = key.OpenSubKey("shell", true);
            key.SetValue("", "");
            ProgressIndicatorBar.Value = 60;
            key.CreateSubKey("open", true);
            key = key.OpenSubKey("open", true);
            key.SetValue("", "");
            ProgressIndicatorBar.Value = 80;
            key.CreateSubKey("command", true);
            key = key.OpenSubKey("command", true);
            key.SetValue("", OMSIPath.Text + "confirmer-l-installation.bat");
            ProgressIndicatorBar.Value = 100;
            StatusLabel.Content = "Done!";

        }
    }
}
