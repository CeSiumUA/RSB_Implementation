using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
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
            var histogram = new Histogram(bytes);
            string fileType = histoFileSource == HistoFileSource.Input ? "вхідного" : "вихідного";
            var pointsList = new Point[histogram.HistogramValues.Length];
            for(int x = 0; x < pointsList.Length; x++)
            {
                pointsList[x] = new Point(x, histogram.HistogramValues[x]);
            }
            this.HistogramPlotModel = new PlotModel()
            {
                Title = $"Гістограма {fileType} файлу"
            };
            this.HistogramPlotModel.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                ItemsSource = pointsList,
                LabelField = "X",
                AxislineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineStyle = LineStyle.Solid,
            });
            this.HistogramPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineStyle = LineStyle.Solid,
                MinimumPadding = 0,
                AbsoluteMinimum = 0,
            });
            this.HistogramPlotModel.Series.Add(new ColumnSeries
            {
                ItemsSource = pointsList,
                ValueField = "Y",
            });
        }

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isEncryptionRunning = false;
        RSBEcnryptor encryptor = null;
        private TimeSpan _elapsedTime;
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
