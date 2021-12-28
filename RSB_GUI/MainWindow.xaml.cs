using RSB_GUI.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
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
        private List<string> _removeableFiles = new List<string>();
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
            foreach(var file in _removeableFiles)
            {
                if (File.Exists(file))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch { }
                }
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
            ShowKey();
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
            
        }

        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            ShowKey();
        }

        private void ShowKey()
        {
            KeyGrid.Children.Clear();
            var key = mainViewModel.Key;
            var textBoxes = key.Length / 8;
            for (int x = 0; x < textBoxes; x++)
            {
                var textBox = new TextBox();
                textBox.SetValue(Grid.RowProperty, x);
                textBox.SetValue(Grid.ColumnProperty, 0);
                var bytesToShow = new byte[8];
                Array.Copy(key, x * 8, bytesToShow, 0, 8);
                textBox.Text = BitConverter.ToString(bytesToShow);
                textBox.TextChanged += UpdateHexKey;
                KeyGrid.Children.Add(textBox);
            }
        }

        private void UpdateHexKey(object sender, TextChangedEventArgs e)
        {
            List<string> hexes = new List<string>();
            foreach (var textBox in KeyGrid.Children)
            {
                if (textBox is TextBox)
                {
                    hexes.Add((textBox as TextBox).Text.Replace("-", string.Empty));
                }
            }

            var hexString = string.Join(string.Empty, hexes);
            var bytesArray = new byte[hexString.Length / 2];
            if (hexString.Length % 2 == 1)
            {
                return;
            }
            for (int index = 0; index < bytesArray.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                bytesArray[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            mainViewModel.Key = bytesArray;
        }
        private void ComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            ShowKey();
        }
    }
}
