using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Net.Http;
enum DirectoryStatus
{
    VALID,
    INVALID,
    NO_EXIST
}
namespace Stormworks_Japanese_translation_downloader
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //ディレクトリの設定
            if (!Properties.Settings.Default.SelectDirectory.Equals("")) directory_text_box.Text = Properties.Settings.Default.SelectDirectory;
            else directory_text_box.Text = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Stormworks\\data\\languages";
            //ディレクトリの存在確認
            switch (CheckDirectory(directory_text_box.Text))
            {
                case DirectoryStatus.VALID:
                    if(File.Exists(directory_text_box.Text + "\\japanese.tsv") && File.Exists(directory_text_box.Text + "\\japanese.xml"))
                    {
                        if(File.Exists(directory_text_box.Text + "\\japanesetranslation_version.txt"))
                        {
                            local_version.Content = "翻訳データのバージョン確認済み。";
                            local_version.Foreground = Brushes.Black;
                        }
                        else
                        {
                            local_version.Content = "翻訳データのバージョンが不明です。";
                            local_version.Foreground = Brushes.Black;
                        }
                    }
                    else
                    {
                        local_version.Content = "翻訳データが存在しません。";
                        local_version.Foreground = Brushes.Black;
                    }
                    break;
                case DirectoryStatus.INVALID:
                    local_version.Content = "ディレクトリが違います。";
                    local_version.Foreground = Brushes.Red;
                    break;
                case DirectoryStatus.NO_EXIST:
                    local_version.Content = "選択したディレクトリは存在しません。";
                    local_version.Foreground = Brushes.Red;
                    break;
            }
        }
        private DirectoryStatus CheckDirectory(string path)
        {
            //ディレクトリを確認する。
            if (Directory.Exists(path))
            {
                if (path.EndsWith("\\Stormworks\\data\\languages")) return DirectoryStatus.VALID;
                else return DirectoryStatus.INVALID;
            }
            else return DirectoryStatus.NO_EXIST;
        }
        private string[] getTags()
        {
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/Gakuto1112/Stormworks-JapaneseTranslation/tags");
                request.Headers.Add("User-Agent", ".NET Foundation Repository Reporter");
                using HttpResponseMessage response = client.SendAsync(request).Result;
                string responseBody = response.Content.ReadAsStringAsync().Result;
                MessageBox.Show(responseBody);

            }
            return null;
        }
        private void directory_select_Click(object sender, RoutedEventArgs e)
        {
            using CommonOpenFileDialog directorySelect = new CommonOpenFileDialog()
            {
                Title = "Stormworksの翻訳データのディレクトリを選択して下さい。",
                InitialDirectory = directory_text_box.Text,
                IsFolderPicker = true,
                RestoreDirectory = true
            };
            if (directorySelect.ShowDialog() == CommonFileDialogResult.Ok) //ダイヤログの表示
            {
                switch (CheckDirectory(directorySelect.FileName))
                {
                    case DirectoryStatus.VALID:
                        break;
                    case DirectoryStatus.INVALID:
                        MessageBox.Show("選択したディレクトリはStormworksの翻訳データのディレクトリではありません。", "異なるディレクトリ", MessageBoxButton.OK, MessageBoxImage.Error);
                        local_version.Content = "ディレクトリが違います。";
                        local_version.Foreground = Brushes.Red;
                        break;
                    case DirectoryStatus.NO_EXIST:
                        MessageBox.Show("選択したディレクトリ存在しません。", "存在しないディレクトリ", MessageBoxButton.OK, MessageBoxImage.Error);
                        local_version.Content = "選択したディレクトリは存在しません。";
                        local_version.Foreground = Brushes.Red;
                        break;
                }
                directory_text_box.Text = directorySelect.FileName;
                Properties.Settings.Default.SelectDirectory = directory_text_box.Text;
                Properties.Settings.Default.Save();
            }
        }
        private void check_update_Click(object sender, RoutedEventArgs e)
        {
            getTags();
        }
    }
}
