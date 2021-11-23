using RSB_GUI.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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

namespace RSB_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel mainViewModel;
        private const string settingsFileName = "settings";
        private Settings settings;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectInputFileButton_Click(object sender, RoutedEventArgs e)
        {
            mainViewModel.SelectInputFile();
        }

        private void SelectOutputFileButton_Click(object sender, RoutedEventArgs e)
        {
            mainViewModel.SelectOutpuFile();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await mainViewModel.StartEncryption();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            mainViewModel.CancelEncryption();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            var settings = mainViewModel.Settings;
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (FileStream fs = new FileStream(settingsFileName, FileMode.OpenOrCreate))
            {
                binaryFormatter.Serialize(fs, settings);
            }
            if (File.Exists("Comment.docx"))
            {
                File.Delete("Comment.docx");
            }
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            Settings settings = null;
            if (File.Exists(settingsFileName))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                using (FileStream fs = new FileStream(settingsFileName, FileMode.Open))
                {
                    if (fs.Length > 0)
                    {
                        settings = (Settings)binaryFormatter.Deserialize(fs);
                    }
                }
            }
            this.settings = settings;
            this.mainViewModel = new MainViewModel(settings);
            this.DataContext = mainViewModel;
        }

        private void InputFileGisto_Click(object sender, RoutedEventArgs e)
        {
            ShowHisto(MainViewModel.HistoFileSource.Input);
        }

        private void OutputFileGisto_Click(object sender, RoutedEventArgs e)
        {
            ShowHisto(MainViewModel.HistoFileSource.Output);
        }
        private void ShowHisto(MainViewModel.HistoFileSource histoFileSource)
        {
            var filePath = histoFileSource == MainViewModel.HistoFileSource.Input ? this.mainViewModel.InputFile : this.mainViewModel.OutputFile;
            var histoWindow = new GistoWindow(filePath, histoFileSource)
            {
                Width = 1200,
                Height = 850
            };
            histoWindow.Show();

            var tableWindow = new TableWindow(new TableViewModel(filePath, settings, histoFileSource))
            {
                Width = 1200,
                Height = 850
            };
            tableWindow.Show();
        }

        private void HistogramDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var rowIndex = e.Row.GetIndex();
            if (rowIndex % 2 == 0)
            {
                e.Row.Background = Brushes.LightGray;
            }
            e.Row.Header = (e.Row.GetIndex() * this.mainViewModel.TableColumnsCount);
            e.Row.HorizontalContentAlignment = HorizontalAlignment.Center;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                var documentWindow = new CommentDocument();
                documentWindow.Show();
            }
            catch(Exception ex)
            {
                var docxName = "Comment.docx";
                var docxBytes = RSB_GUI.Properties.Resources.Comment1;
                File.WriteAllBytes(docxName, docxBytes);
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = docxName;
                    process.Start();
                }
            }
        }
    }
}
