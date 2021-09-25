using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
            this.mainViewModel.Histo(histoFileSource);
            this.MainTabControl.SelectedIndex = 1;
            int columns = 8;
            var valuesList = this.mainViewModel.HistogramAsLabeledValues.ToList();
            var remnantValues = columns - (valuesList.Count % columns);
            valuesList.AddRange(Enumerable.Repeat<LabelValue>(new LabelValue(null, null), remnantValues));

            DataTable dataTable = new DataTable();

            for (int clmn = 0; clmn < columns; clmn++)
            {
                dataTable.Columns.Add(new DataColumn(clmn.ToString()));
            }

            for(int row = 0; row < valuesList.Count / columns; row++)
            {
                var newRow = dataTable.NewRow();
                for(int column = 0; column < columns; column++)
                {
                    newRow[column] = valuesList[row *  columns + column].Value ?? null;
                }
                dataTable.Rows.Add(newRow);
            }

            HistogramDataGrid.ItemsSource = dataTable.DefaultView;
        }
    }
}
