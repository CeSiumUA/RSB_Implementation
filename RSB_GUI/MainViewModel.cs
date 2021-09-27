using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using TickStyle = OxyPlot.Axes.TickStyle;

namespace RSB_GUI
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public Settings Settings { get; set; }
        public string InputFile
        {
            get
            {
                return Settings.InputFilePath;
            }
            set
            {
                Settings.InputFilePath = value;
                Histo(HistoFileSource.Input);
                this.OnPropertyChanged();
            }
        }
        public string OutputFile
        {
            get
            {
                return Settings.OutputFilePath;
            }
            set
            {
                Settings.OutputFilePath = value;
                Histo(HistoFileSource.Output);
                this.OnPropertyChanged();
            }
        }
        public int BlockPower
        {
            get
            {
                return Settings.BlockPower;
            }
            set
            {
                Settings.BlockPower = value;
                this.OnPropertyChanged();
            }
        }
        public int BlockLength
        {
            get
            {
                return 64 * (int)Math.Pow(2, this.BlockPower);
            }
        }
        public int ShiftValue
        {
            get
            {
                return Settings.ShiftValue;
            }
            set
            {
                Settings.ShiftValue = value;
                this.OnPropertyChanged();
            }
        }
        public bool UseCommonKey
        {
            get
            {
                return Settings.UseCommonKey;
            }
            set
            {
                Settings.UseCommonKey = value;
                this.OnPropertyChanged();
            }
        }

        public int TotalEncryptionSteps
        {
            get
            {
                return encryptor?.TotalSteps ?? 0;
            }
        }

        public int CurrentStep
        {
            get
            {
                return encryptor?.Step ?? 0;
            }
        }

        public bool AllowEncryption
        {
            get
            {
                return !string.IsNullOrEmpty(this.InputFile) && !string.IsNullOrEmpty(this.OutputFile) && File.Exists(this.InputFile) && this.ShiftValue != 0;
            }
        }

        public bool IsEncryptionRunning
        {
            get
            {
                return _isEncryptionRunning;
            }
            set
            {
                this._isEncryptionRunning = value;
                this.OnPropertyChanged();
            }
        }

        public TimeSpan ElapsedTime
        {
            get
            {
                return _elapsedTime;
            }
            set
            {
                _elapsedTime = value;
                this.OnPropertyChanged();
            }
        }

        public bool UseEncryption
        {
            get
            {
                return Settings.UseEncryption;
            }
            set
            {
                Settings.UseEncryption = value;
                this.OnPropertyChanged();
            }
        }

        public bool UseDecryption
        {
            get
            {
                return !Settings.UseEncryption;
            }
            set
            {
                Settings.UseEncryption = !value;
                this.OnPropertyChanged();
            }
        }

        public PlotModel HistogramPlotModel
        {
            get
            {
                return _histogramPlotModel;
            }
            set
            {
                _histogramPlotModel = value;
                this.OnPropertyChanged();
            }
        }

        public Histogram Histogram
        {
            get
            {
                return _histogram;
            }
            set
            {
                _histogram = value;
                this.OnPropertyChanged();
            }
        }

        public DataView HistogramDatatable
        {
            get
            {
                int columns = 8;
                var valuesList = this.HistogramAsLabeledValues.ToList();
                var remnantValues = columns - (valuesList.Count % columns);
                valuesList.AddRange(Enumerable.Repeat<LabelValue>(new LabelValue(null, null), remnantValues));

                DataTable dataTable = new DataTable();

                for (int clmn = 0; clmn < columns; clmn++)
                {
                    dataTable.Columns.Add(new DataColumn(clmn.ToString()));
                }

                for (int row = 0; row < valuesList.Count / columns; row++)
                {
                    var newRow = dataTable.NewRow();
                    for (int column = 0; column < columns; column++)
                    {
                        newRow[column] = valuesList[row * columns + column].Value ?? null;
                    }
                    dataTable.Rows.Add(newRow);
                }

                return dataTable.DefaultView;
            }
        }

        public LabelValue[] HistogramAsLabeledValues
        {
            get
            {
                LabelValue[] labelValues = new LabelValue[Histogram.HistogramValues.Length];
                for( int x = 0; x < labelValues.Length; x++)
                {
                    labelValues[x] = new LabelValue(x, Histogram.HistogramValues[x]);
                }
                return labelValues;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public MainViewModel(Settings settings = null)
        {
            Settings = settings ?? new Settings();
        }

        public async Task StartEncryption()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Restart();
            this.IsEncryptionRunning = true;
            this._cancellationTokenSource = new CancellationTokenSource();
            encryptor = new RSBEcnryptor(this.BlockLength, this.ShiftValue, () =>
            {
                this.ElapsedTime = stopWatch.Elapsed;
                this.OnPropertyChanged();
            });
            new Task(async () =>
            {
                var fileBytes = File.ReadAllBytes(this.InputFile);
                byte[] processedBytes = null;
                if (UseEncryption)
                {
                    processedBytes = await encryptor.EncryptBytes(fileBytes, _cancellationTokenSource.Token);
                }
                else
                {
                    processedBytes = await encryptor.DecryptBytes(fileBytes, _cancellationTokenSource.Token);
                }
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    this.IsEncryptionRunning = false;
                    return;
                }
                File.WriteAllBytes(this.OutputFile, processedBytes);
                this.IsEncryptionRunning = false;
                this.ElapsedTime = stopWatch.Elapsed;
            }).Start();
        }

        public void CancelEncryption()
        {
            this._cancellationTokenSource.Cancel();
        }

        public void SelectInputFile()
        {
            using(var fileDialog =  new OpenFileDialog())
            {
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    this.InputFile = fileDialog.FileName;
                }
            }
        }

        public void SelectOutpuFile()
        {
            using (var saveFileDialog = new SaveFileDialog())
            {
                if(saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    this.OutputFile = saveFileDialog.FileName;
                }
            }
        }

        public void Histo(HistoFileSource histoFileSource = HistoFileSource.Input)
        {
            var bytes = File.ReadAllBytes(histoFileSource == HistoFileSource.Input ? this.InputFile : this.OutputFile);
            this.Histogram = new Histogram(bytes);
            string fileType = histoFileSource == HistoFileSource.Input ? "вхідного" : "вихідного";
            var pointsList = new LabelValue[this.Histogram.HistogramValues.Length];
            for(int x = 0; x < pointsList.Length; x++)
            {
                pointsList[x] = new LabelValue(x, this.Histogram.HistogramValues[x]);
            }
            pointsList = pointsList.Where(x => x.Value != 0).ToArray();
            this.HistogramPlotModel = new PlotModel()
            {
                Title = $"Гістограма {fileType} файлу, ентропія: {this.Histogram.Entropy}"
            };
            this.HistogramPlotModel.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                ItemsSource = pointsList,
                LabelField = "Label",
                AxislineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.None,
                MajorGridlineStyle = LineStyle.None,
                PositionAtZeroCrossing = true,
                AxislineThickness = 1,
                TickStyle = TickStyle.Crossing,
                Angle = 90
            });
            this.HistogramPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineStyle = LineStyle.Dot,
                MinimumPadding = 0,
                AbsoluteMinimum = 0,
                TickStyle = TickStyle.Outside,
                AxislineThickness = 1,
                AxislineStyle = LineStyle.Solid,
            });
            this.HistogramPlotModel.Series.Add(new ColumnSeries
            {
                ItemsSource = pointsList,
                ValueField = "Value",
                StrokeThickness = 1
            });
        }

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isEncryptionRunning = false;
        RSBEcnryptor encryptor = null;
        private TimeSpan _elapsedTime;
        private Histogram _histogram = new Histogram();
        private PlotModel _histogramPlotModel = new PlotModel();
        private void OnPropertyChanged(string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public enum HistoFileSource
        {
            Input,
            Output
        }
    }
}
