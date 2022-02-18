using RSB_GUI.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using Clipboard = System.Windows.Clipboard;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using TextBox = System.Windows.Controls.TextBox;

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
            this.settings = mainViewModel.Settings;
            SaveSettings();
        }

        private void SaveSettings()
        {
            var settings = mainViewModel.Settings;
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (FileStream fs = new FileStream(settingsFileName, FileMode.OpenOrCreate))
            {
                binaryFormatter.Serialize(fs, settings);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.settings = mainViewModel.Settings;
            SaveSettings();
            foreach (var file in _removeableFiles)
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
                Width = 450,
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
            var toRemove = new List<TextBox>();
            foreach (var tb in KeyGrid.Children)
            {
                if (tb is TextBox)
                {
                    toRemove.Add(tb as TextBox);
                }
            }

            foreach (var rm in toRemove)
            {
                KeyGrid.Children.Remove(rm);
            }
            var key = mainViewModel.Key;
            if (key != null)
            {
                var textBoxes = key.Length / 16;
                for (int x = 0; x < textBoxes; x++)
                {
                    var textBox = new TextBox();
                    textBox.SetValue(Grid.RowProperty, x);
                    textBox.SetValue(Grid.ColumnProperty, 0);
                    var bytesToShow = new byte[16];
                    Array.Copy(key, x * 16, bytesToShow, 0, 16);
                    textBox.Text = BitConverter.ToString(bytesToShow);
                    textBox.TextChanged += UpdateHexKey;
                    KeyGrid.Children.Add(textBox);
                }
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

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.mainViewModel.GenerateCommonKey();
            ShowKey();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.mainViewModel.SaveCommonKey(sfd.FileName);
                }
            }
            ShowKey();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.mainViewModel.LoadKeyFromFile(ofd.FileName);
                }
            }
            ShowKey();
        }

        private void Label_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Clipboard.SetText(this.mainViewModel.ElapsedTime.TotalSeconds.ToString());
        }

        //private void Open_Results(object sender, RoutedEventArgs e)
        //{
        //    var filePath = Path.Combine("Images", "values.json");
        //    List<Testing> testingValues = new List<Testing>();
        //    if (File.Exists(filePath))
        //    {
        //        using (FileStream fs = new FileStream(filePath, FileMode.Open))
        //        {
        //            BinaryFormatter bf = new BinaryFormatter();
        //            testingValues = bf.Deserialize(fs) as List<Testing>;
        //        }
        //    }

        //    var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(testingValues.ToArray());
        //    File.WriteAllText(filePath, serialized);
        //}
        //private async void Button_Click_6(object sender, RoutedEventArgs e)
        //{
        //    List<Testing> testingValues = new List<Testing>();
        //    foreach (var keyLength in mainViewModel.KeyLengthOptions)
        //    {
        //        this.mainViewModel.CommonKeyLength = keyLength;

        //        foreach (var blockLength in mainViewModel.BlockLengthValues)
        //        {
        //            var imagesDir = Directory.CreateDirectory("Images");
        //            var tablesDir = Directory.CreateDirectory(Path.Combine(imagesDir.FullName, "Tables"));
        //            var histoDir = Directory.CreateDirectory(Path.Combine(imagesDir.FullName, "Gisto"));
        //            this.mainViewModel.LogorithmicalBlockLength = blockLength;

        //            this.mainViewModel.UseVariant1 = true;
        //            this.mainViewModel.UseVariant2 = false;
        //            this.mainViewModel.UseVariant3 = false;
        //            await this.mainViewModel.StartEncryption();
        //            while (this.mainViewModel.IsEncryptionRunning)
        //            {

        //            }
        //            string gistoFileName1 = $"K{keyLength}_R8_N{blockLength}.png";
        //            var entropy1 = await MakeHistoScreenshot($"{Path.Combine(tablesDir.FullName, gistoFileName1)}", $"{Path.Combine(histoDir.FullName, gistoFileName1)}");
        //            var testing1 = new Testing()
        //            {
        //                BlockLength = blockLength,
        //                KeyLength = keyLength,
        //                SC = 8,
        //                TimeToComplete = mainViewModel.ElapsedTime.TotalMilliseconds,
        //                Entropy = entropy1
        //            };
        //            testingValues.Add(testing1);

        //            this.mainViewModel.UseVariant1 = false;
        //            this.mainViewModel.UseVariant2 = true;
        //            this.mainViewModel.UseVariant3 = false;
        //            await this.mainViewModel.StartEncryption();
        //            while (this.mainViewModel.IsEncryptionRunning)
        //            {

        //            }
        //            string gistoFileName2 = $"K{keyLength}_R16_N{blockLength}.png";
        //            var entropy2 = await MakeHistoScreenshot($"{Path.Combine(tablesDir.FullName, gistoFileName2)}", $"{Path.Combine(histoDir.FullName, gistoFileName2)}");
        //            var testing2 = new Testing()
        //            {
        //                BlockLength = blockLength,
        //                KeyLength = keyLength,
        //                SC = 16,
        //                TimeToComplete = mainViewModel.ElapsedTime.TotalMilliseconds,
        //                Entropy = entropy2
        //            };
        //            testingValues.Add(testing2);

        //            this.mainViewModel.UseVariant1 = false;
        //            this.mainViewModel.UseVariant2 = false;
        //            this.mainViewModel.UseVariant3 = true;
        //            await this.mainViewModel.StartEncryption();
        //            while (this.mainViewModel.IsEncryptionRunning)
        //            {

        //            }
        //            string gistoFileName3 = $"K{keyLength}_R32_N{blockLength}.png";
        //            var entropy3 = await MakeHistoScreenshot($"{Path.Combine(tablesDir.FullName, gistoFileName3)}", $"{Path.Combine(histoDir.FullName, gistoFileName3)}");
        //            var testing3 = new Testing()
        //            {
        //                BlockLength = blockLength,
        //                KeyLength = keyLength,
        //                SC = 32,
        //                TimeToComplete = mainViewModel.ElapsedTime.TotalMilliseconds,
        //                Entropy = entropy3
        //            };
        //            testingValues.Add(testing3);

        //            using (FileStream fs = new FileStream(Path.Combine(imagesDir.FullName, "values.json"), FileMode.Create))
        //            {
        //                BinaryFormatter bf = new BinaryFormatter();
        //                bf.Serialize(fs, testingValues);
        //            }

        //            async Task<double> MakeHistoScreenshot(string tableFileName, string histoFileName)
        //            {
        //                var filePath = this.mainViewModel.OutputFile;
        //                var histoWindow = new GistoWindow(filePath, MainViewModel.HistoFileSource.Output)
        //                {
        //                    Width = 1200,
        //                    Height = 850
        //                };
        //                histoWindow.InitializeComponent();
        //                histoWindow.Show();
        //                histoWindow.MakeScreenshot(histoFileName);
        //                histoWindow.Close();

        //                var tableWindow = new TableWindow(new TableViewModel(filePath, settings, MainViewModel.HistoFileSource.Output))
        //                {
        //                    Width = 450,
        //                    Height = 850
        //                };
        //                tableWindow.InitializeComponent();
        //                tableWindow.Show();
        //                await Task.Delay(2000);
        //                tableWindow.MakeScreenshot(tableFileName);
        //                tableWindow.Close();

        //                return histoWindow.Histogram.Entropy;
        //            }
        //        }
        //    }
        //}
    }
}
