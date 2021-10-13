using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
using System.Net;
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
        private List<TagData> tagData;
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
                        if(!Properties.Settings.Default.InstalledVersion.Equals(""))
                        {
                            local_version.Content = "インストール済バージョン：" + Properties.Settings.Default.InstalledVersion;
                            local_version.Foreground = Brushes.Black;
                            generate_translation.Content = "翻訳データの更新";
                            generate_translation.IsEnabled = true;
                            remove_translation.IsEnabled = true;
                        }
                        else
                        {
                            local_version.Content = "翻訳データのバージョンが不明です。";
                            local_version.Foreground = Brushes.Black;
                            generate_translation.Content = "翻訳データの更新";
                            generate_translation.IsEnabled = true;
                            remove_translation.IsEnabled = true;
                        }
                    }
                    else
                    {
                        local_version.Content = "翻訳データが存在しません。";
                        local_version.Foreground = Brushes.Black;
                        generate_translation.Content = "翻訳データの生成";
                        generate_translation.IsEnabled = true;
                        remove_translation.IsEnabled = false;
                    }
                    break;
                case DirectoryStatus.INVALID:
                    local_version.Content = "ディレクトリが違います。";
                    local_version.Foreground = Brushes.Red;
                    generate_translation.Content = "翻訳データの生成";
                    generate_translation.IsEnabled = false;
                    remove_translation.IsEnabled = false;
                    break;
                case DirectoryStatus.NO_EXIST:
                    local_version.Content = "選択したディレクトリは存在しません。";
                    local_version.Foreground = Brushes.Red;
                    generate_translation.Content = "翻訳データの生成";
                    generate_translation.IsEnabled = false;
                    remove_translation.IsEnabled = false;
                    break;
            }
            getData();
            CheckVersion();
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
        private void getData()
        {
            //データをAPIを通じて取得する。
            tagData = new List<TagData>();
            string responseBody = "";
            //メッセージ変更
            remote_version.Content = "データ取得中です...";
            remote_version.Foreground = Brushes.Black;
            check_update.IsEnabled = false;
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/Gakuto1112/Stormworks-JapaneseTranslation/tags");
                request.Headers.Add("User-Agent", ".NET Foundation Repository Reporter");
                try
                {
                    using HttpResponseMessage response = client.SendAsync(request).Result;
                    responseBody = response.Content.ReadAsStringAsync().Result;
                    if(response.StatusCode != HttpStatusCode.OK)
                    {
                        remote_version.Content = "エラー";
                        remote_version.Foreground = Brushes.Red;
                        MessageBox.Show("正常な応答が得られませんでした。ステータスコード：" + response.StatusCode.ToString(), "エラー", MessageBoxButton.OK, MessageBoxImage.Error);

                        return;
                    }
                }
                catch (ArgumentException exception)
                {
                    remote_version.Content = "エラー";
                    remote_version.Foreground = Brushes.Red;
                    MessageBox.Show("データ取得中にエラーが発生しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                catch (InvalidOperationException exception)
                {
                    remote_version.Content = "エラー";
                    remote_version.Foreground = Brushes.Red;
                    MessageBox.Show("既にデータ取得中です。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                catch (HttpRequestException exception)
                {
                    remote_version.Content = "エラー";
                    remote_version.Foreground = Brushes.Red;
                    MessageBox.Show("ネットワークに問題が発生したため、データが取得できませんでした。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                catch (TaskCanceledException expeption)
                {
                    remote_version.Content = "エラー";
                    remote_version.Foreground = Brushes.Red;
                    MessageBox.Show("データ要求がタイムアウトしました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            List<ResponseData> responseData = JsonSerializer.Deserialize<List<ResponseData>>(responseBody);
            foreach(ResponseData data in responseData)
            {
                TagData tag = new TagData();
                tag.name = data.name;
                tag.url = data.zipball_url;
                tagData.Add(tag);
            }
        }
        private void CheckVersion()
        {
            //インストールされているバージョン確認する。
            if (Properties.Settings.Default.InstalledVersion.Equals("")){
                remote_version.Content = "最新バージョン利用可能（" + tagData[0].name + "）";
                remote_version.Foreground = Brushes.Black;
                check_update.IsEnabled = true;
                return;
            }
            for (int i = 0; i < tagData.Count; i++)
            {
                if (Properties.Settings.Default.InstalledVersion.Equals(tagData[0].name))
                {
                    if(i == 0)
                    {
                        remote_version.Content = "最新バージョンインストール済";
                        remote_version.Foreground = Brushes.Black;
                    }
                    else
                    {
                        remote_version.Content = "最新バージョン利用可能（" + tagData[0].name +"）";
                        remote_version.Foreground = Brushes.Black;
                    }
                    check_update.IsEnabled = true;
                    break;
                }
            }

        }
        private void DownloadFile()
        {
            //ファイルをダウンロードして適用する。
            remote_version.Content = "データのダウンロード中...";
            remote_version.Foreground = Brushes.Black;
            WebClient client = new WebClient();
            client.Headers.Add("User-Agent", ".NET Foundation Repository Reporter");
            try
            {
                client.DownloadFile(tagData[0].url, directory_text_box.Text + "\\temp.zip");
            }
            catch (WebException exception)
            {
                remote_version.Content = "エラー";
                remote_version.Foreground = Brushes.Red;
                MessageBox.Show("データのダウンロード中にエラーが発生しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                generate_translation.IsEnabled = true;
                check_update.IsEnabled = true;
                return;
            }
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
                        if(File.Exists(directory_text_box.Text + "\\japanese.tsv") && File.Exists(directory_text_box.Text + "\\japanese.xml"))
                        {
                            if(!Properties.Settings.Default.InstalledVersion.Equals(""))
                            {
                                local_version.Content = "インストール済バージョン：" + Properties.Settings.Default.InstalledVersion;
                                local_version.Foreground = Brushes.Black;
                                generate_translation.Content = "翻訳データの更新";
                                generate_translation.IsEnabled = true;
                                remove_translation.IsEnabled = true;
                            }
                            else
                            {
                                local_version.Content = "翻訳データのバージョンが不明です。";
                                local_version.Foreground = Brushes.Black;
                                generate_translation.Content = "翻訳データの更新";
                                generate_translation.IsEnabled = true;
                                remove_translation.IsEnabled = true;
                            }
                        }
                        else
                        {
                            local_version.Content = "翻訳データが存在しません。";
                            local_version.Foreground = Brushes.Black;
                            generate_translation.Content = "翻訳データの生成";
                            generate_translation.IsEnabled = true;
                            remove_translation.IsEnabled = false;
                        }
                        break;
                    case DirectoryStatus.INVALID:
                        MessageBox.Show("選択したディレクトリはStormworksの翻訳データのディレクトリではありません。", "異なるディレクトリ", MessageBoxButton.OK, MessageBoxImage.Error);
                        local_version.Content = "ディレクトリが違います。";
                        local_version.Foreground = Brushes.Red;
                        generate_translation.Content = "翻訳データの生成";
                        generate_translation.IsEnabled = false;
                        remove_translation.IsEnabled = false;
                        break;
                    case DirectoryStatus.NO_EXIST:
                        MessageBox.Show("選択したディレクトリ存在しません。", "存在しないディレクトリ", MessageBoxButton.OK, MessageBoxImage.Error);
                        local_version.Content = "選択したディレクトリは存在しません。";
                        local_version.Foreground = Brushes.Red;
                        generate_translation.Content = "翻訳データの生成";
                        generate_translation.IsEnabled = false;
                        remove_translation.IsEnabled = false;
                        break;
                }
                directory_text_box.Text = directorySelect.FileName;
                Properties.Settings.Default.SelectDirectory = directory_text_box.Text;
                Properties.Settings.Default.Save();
            }
        }
        private void directory_text_box_LostForcus(object sender, RoutedEventArgs e)
        {
            //テキストボックスが変更された時
            switch (CheckDirectory(directory_text_box.Text))
            {
                case DirectoryStatus.VALID:
                    if (File.Exists(directory_text_box.Text + "\\japanese.tsv") && File.Exists(directory_text_box.Text + "\\japanese.xml"))
                    {
                        if (!Properties.Settings.Default.InstalledVersion.Equals(""))
                        {
                            local_version.Content = "インストール済バージョン：" + Properties.Settings.Default.InstalledVersion;
                            local_version.Foreground = Brushes.Black;
                            generate_translation.Content = "翻訳データの更新";
                            generate_translation.IsEnabled = true;
                            remove_translation.IsEnabled = true;
                        }
                        else
                        {
                            local_version.Content = "翻訳データのバージョンが不明です。";
                            local_version.Foreground = Brushes.Black;
                            generate_translation.Content = "翻訳データの更新";
                            generate_translation.IsEnabled = true;
                            remove_translation.IsEnabled = true;
                        }
                    }
                    else
                    {
                        local_version.Content = "翻訳データが存在しません。";
                        local_version.Foreground = Brushes.Black;
                        generate_translation.Content = "翻訳データの生成";
                        generate_translation.IsEnabled = true;
                        remove_translation.IsEnabled = false;
                    }
                    break;
                case DirectoryStatus.INVALID:
                    MessageBox.Show("選択したディレクトリはStormworksの翻訳データのディレクトリではありません。", "異なるディレクトリ", MessageBoxButton.OK, MessageBoxImage.Error);
                    local_version.Content = "ディレクトリが違います。";
                    local_version.Foreground = Brushes.Red;
                    generate_translation.Content = "翻訳データの生成";
                    generate_translation.IsEnabled = false;
                    remove_translation.IsEnabled = false;
                    break;
                case DirectoryStatus.NO_EXIST:
                    MessageBox.Show("選択したディレクトリ存在しません。", "存在しないディレクトリ", MessageBoxButton.OK, MessageBoxImage.Error);
                    local_version.Content = "選択したディレクトリは存在しません。";
                    local_version.Foreground = Brushes.Red;
                    generate_translation.Content = "翻訳データの生成";
                    generate_translation.IsEnabled = false;
                    remove_translation.IsEnabled = false;
                    break;
            }
            Properties.Settings.Default.SelectDirectory = directory_text_box.Text;
            Properties.Settings.Default.Save();
        }
        private void generate_translation_Click(object sender, RoutedEventArgs e)
        {
            if(CheckDirectory(directory_text_box.Text) == DirectoryStatus.NO_EXIST)
            {
                if (MessageBox.Show("翻訳データをデータをダウンロードして生成しますか？", "翻訳データのダウンロード", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            else
            {
                if (MessageBox.Show("翻訳データをデータをダウンロードして更新しますか？", "翻訳データのダウンロード", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            generate_translation.IsEnabled = false;
            remove_translation.IsEnabled = false;
            check_update.IsEnabled = false;
            DownloadFile();
        }
        private void remove_translation_Click(object sender, RoutedEventArgs e)
        {
            //翻訳データ削除ボタン
            if (MessageBox.Show("翻訳データを削除しますか？","翻訳データの削除", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                local_version.Content = "翻訳データの削除中...";
                local_version.Foreground = Brushes.Black;
                generate_translation.IsEnabled = false;
                remove_translation.IsEnabled = false;
                check_update.IsEnabled = false;
                try
                {
                    File.Delete(directory_text_box.Text + "\\japanese.tsv");
                    File.Delete(directory_text_box.Text + "\\japanese.xml");
                }
                catch (DirectoryNotFoundException exception)
                {
                    local_version.Content = "エラー";
                    local_version.Foreground = Brushes.Red;
                    MessageBox.Show("ファイルが見つかりません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    generate_translation.IsEnabled = true;
                    remove_translation.IsEnabled = true;
                    check_update.IsEnabled = true;
                    return;
                }
                catch(PathTooLongException exception)
                {
                    local_version.Content = "エラー";
                    local_version.Foreground = Brushes.Red;
                    MessageBox.Show("ファイルへのパスが長過ぎます。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    generate_translation.IsEnabled = true;
                    remove_translation.IsEnabled = true;
                    check_update.IsEnabled = true;
                    return;
                }
                catch (IOException exceotion)
                {
                    local_version.Content = "エラー";
                    local_version.Foreground = Brushes.Red;
                    MessageBox.Show("ファイルは現在使用中で削除できません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    generate_translation.IsEnabled = true;
                    remove_translation.IsEnabled = true;
                    check_update.IsEnabled = true;
                    return;
                }
                catch (UnauthorizedAccessException exception)
                {
                    local_version.Content = "エラー";
                    local_version.Foreground = Brushes.Red;
                    MessageBox.Show("ファイルへアクセスできません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    generate_translation.IsEnabled = true;
                    remove_translation.IsEnabled = true;
                    check_update.IsEnabled = true;
                    return;
                }
                local_version.Content = "翻訳データが存在しません。";
                local_version.Foreground = Brushes.Black;
                generate_translation.Content = "翻訳データの生成";
                generate_translation.IsEnabled = true;
                remove_translation.IsEnabled = false;
            }
        }
        private void check_update_Click(object sender, RoutedEventArgs e)
        {
            //更新確認ボタン
            getData();
            CheckVersion();
        }
    }
}