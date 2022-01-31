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
using CsvHelper;
using Newtonsoft.Json;
using RSB_GUI.Utils;
using Path = System.IO.Path;

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
            var variant = this.mainViewModel.UseVariant1 ? "1" : (this.mainViewModel.UseVariant2 ? "2" : "4");
            var label = $"R={this.mainViewModel.SelectedRoundValues}; L={this.mainViewModel.BlockLength}; V={variant}";
            var filePath = histoFileSource == MainViewModel.HistoFileSource.Input ? this.mainViewModel.InputFile : this.mainViewModel.OutputFile;
            label = histoFileSource == MainViewModel.HistoFileSource.Output ? label : string.Empty;
            var histoWindow = new GistoWindow(filePath, histoFileSource, label)
            {
                Width = 1200,
                Height = 850
            };
            histoWindow.Show();

            var tableWindow = new TableWindow(new TableViewModel(filePath, settings, histoFileSource, label))
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
            var xpsTempFilePath = System.IO.Path.GetTempFileName();
            _removeableFiles.Add(xpsTempFilePath);
            try
            {
                var documentWindow = new CommentDocument(xpsTempFilePath);
                documentWindow.Show();
            }
            catch(Exception ex)
            {
                var docxName = "Comment.docx";
                _removeableFiles.Add(docxName);
                var docxBytes = RSB_GUI.Properties.Resources.Comment1;
                File.WriteAllBytes(docxName, docxBytes);
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = docxName;
                    process.Start();
                }
            }
        }

        private void ExportCsv(object sender, RoutedEventArgs e)
        {
            if (!File.Exists("Images/values.json"))
            {
                return;
            }

            var tests = JsonConvert.DeserializeObject<Testing[]>(File.ReadAllText("Images/values.json"));
            tests = tests.OrderBy(x => x.Rounds).ThenBy(x => x.BlockLength).ToArray();
            var groupedTests = tests.GroupBy(x => new {x.BlockLength, x.Rounds});
            using (var writer = new StreamWriter("Images/values.csv"))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    foreach (var recgrp in groupedTests)
                    {
                        var recEntr = new
                        {
                            Round = recgrp.First().Rounds,
                            BlockLength = recgrp.First().BlockLength,
                            Indicator = "Н",
                            Value1 = recgrp.First().Entropy,
                            Value2 = recgrp.Skip(1).First().Entropy,
                            Value3 = recgrp.Skip(2).First().Entropy
                        };
                        csv.WriteRecord(recEntr);
                        csv.NextRecord();
                        var recTime = new
                        {
                            Round = recgrp.First().Rounds,
                            BlockLength = recgrp.First().BlockLength,
                            Indicator = "Т",
                            Value1 = recgrp.First().TimeToComplete,
                            Value2 = recgrp.Skip(1).First().TimeToComplete,
                            Value3 = recgrp.Skip(2).First().TimeToComplete
                        };
                        csv.WriteRecord(recTime);
                        csv.NextRecord();
                    }
                }
            }
            
            
        }
        class CsvLine
        {
            public int BlockLength { get; set; }
            public int Round { get; set; }
        }
        private async Task MakeAutoDoc(object sender, RoutedEventArgs e)
        {
            List<Testing> testingValues = new List<Testing>();
            foreach (var roundsCount in mainViewModel.RoundValues)
            {
                this.mainViewModel.SelectedRoundValues = roundsCount;

                foreach (var blockLength in mainViewModel.BlockLengthValues)
                {
                    var imagesDir = Directory.CreateDirectory("Images");
                    var tablesDir = Directory.CreateDirectory(Path.Combine(imagesDir.FullName, "Tables"));
                    var histoDir = Directory.CreateDirectory(Path.Combine(imagesDir.FullName, "Gisto"));
                    this.mainViewModel.LogorithmicalBlockLength = blockLength;

                    this.mainViewModel.UseVariant1 = true;
                    this.mainViewModel.UseVariant2 = false;
                    this.mainViewModel.UseVariant3 = false;
                    await this.mainViewModel.StartEncryption();
                    while (this.mainViewModel.IsEncryptionRunning)
                    {

                    }
                    string gistoFileName1 = $"Блок{blockLength}_Варіант1_Раунди{roundsCount}.png";
                    var entropy1 = await MakeHistoScreenshot(tablesDir.FullName, gistoFileName1, $"{Path.Combine(histoDir.FullName, gistoFileName1)}", $"R={roundsCount}; L={blockLength}; V=1");
                    var testing1 = new Testing()
                    {
                        BlockLength = blockLength,
                        Rounds = roundsCount,
                        Variant = 1,
                        TimeToComplete = mainViewModel.ElapsedTime.TotalMilliseconds,
                        Entropy = entropy1
                    };
                    testingValues.Add(testing1);

                    this.mainViewModel.UseVariant1 = false;
                    this.mainViewModel.UseVariant2 = true;
                    this.mainViewModel.UseVariant3 = false;
                    await this.mainViewModel.StartEncryption();
                    while (this.mainViewModel.IsEncryptionRunning)
                    {

                    }
                    string gistoFileName2 = $"Блок{blockLength}_Варіант2_Раунди{roundsCount}.png";
                    var entropy2 = await MakeHistoScreenshot(tablesDir.FullName, gistoFileName2, $"{Path.Combine(histoDir.FullName, gistoFileName2)}", $"R={roundsCount}; L={blockLength}; V=2");
                    var testing2 = new Testing()
                    {
                        BlockLength = blockLength,
                        Rounds = roundsCount,
                        Variant = 2,
                        TimeToComplete = mainViewModel.ElapsedTime.TotalMilliseconds,
                        Entropy = entropy2
                    };
                    testingValues.Add(testing2);

                    this.mainViewModel.UseVariant1 = false;
                    this.mainViewModel.UseVariant2 = false;
                    this.mainViewModel.UseVariant3 = true;
                    await this.mainViewModel.StartEncryption();
                    while (this.mainViewModel.IsEncryptionRunning)
                    {

                    }
                    string gistoFileName3 = $"Блок{blockLength}_Варіант4_Раунди{roundsCount}.png";
                    var entropy3 = await MakeHistoScreenshot(tablesDir.FullName, gistoFileName3, $"{Path.Combine(histoDir.FullName, gistoFileName3)}", $"R={roundsCount}; L={blockLength}; V=2");
                    var testing3 = new Testing()
                    {
                        BlockLength = blockLength,
                        Rounds = roundsCount,
                        Variant = 4,
                        TimeToComplete = mainViewModel.ElapsedTime.TotalMilliseconds,
                        Entropy = entropy3
                    };
                    testingValues.Add(testing3);

                    var json = JsonConvert.SerializeObject(testingValues);
                    File.WriteAllText(Path.Combine(imagesDir.FullName, "values.json"), json);

                    async Task<double> MakeHistoScreenshot(string tableDirName, string tableFileName, string histoFileName, string label)
                    {
                        var filePath = this.mainViewModel.OutputFile;
                        var histoWindow = new GistoWindow(filePath, MainViewModel.HistoFileSource.Output, label)
                        {
                            Width = 1200,
                            Height = 850
                        };
                        histoWindow.InitializeComponent();
                        histoWindow.Show();
                        histoWindow.MakeScreenshot(histoFileName);
                        histoWindow.Close();

                        mainViewModel.TableColumnsCount = 8;
                        var tableWindow = new TableWindow(new TableViewModel(filePath, settings, MainViewModel.HistoFileSource.Output, label))
                        {
                            Width = 450,
                            Height = 850
                        };
                        tableWindow.InitializeComponent();
                        tableWindow.Show();
                        await Task.Delay(2000);
                        var dir8 = Directory.CreateDirectory(Path.Combine(tableDirName, "Табл-8"));
                        tableWindow.MakeScreenshot(Path.Combine(dir8.FullName, tableFileName));
                        tableWindow.Close();

                        mainViewModel.TableColumnsCount = 10;
                        tableWindow = new TableWindow(new TableViewModel(filePath, settings, MainViewModel.HistoFileSource.Output, label))
                        {
                            Width = 566,
                            Height = 700
                        };
                        tableWindow.InitializeComponent();
                        tableWindow.Show();
                        await Task.Delay(2000);
                        var dir10 = Directory.CreateDirectory(Path.Combine(tableDirName, "Табл-10"));
                        tableWindow.MakeScreenshot(Path.Combine(dir10.FullName, tableFileName));
                        tableWindow.Close();
                        
                        mainViewModel.TableColumnsCount = 16;
                        tableWindow = new TableWindow(new TableViewModel(filePath, settings, MainViewModel.HistoFileSource.Output, label))
                        {
                            Width = 900,
                            Height = 500
                        };
                        tableWindow.InitializeComponent();
                        tableWindow.Show();
                        await Task.Delay(2000);
                        var dir16 = Directory.CreateDirectory(Path.Combine(tableDirName, "Табл-16"));
                        tableWindow.MakeScreenshot(Path.Combine(dir16.FullName, tableFileName));
                        tableWindow.Close();
                        
                        return histoWindow.Histogram.Entropy;
                    }
                }
            }
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            await MakeAutoDoc(sender, e);
            ExportCsv(sender, e);
            MessageBox.Show("Завершено!", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
