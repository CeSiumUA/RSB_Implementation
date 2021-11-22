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
using System.Windows;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using RSB_GUI.Utils;
using TickStyle = OxyPlot.Axes.TickStyle;

namespace RSB_GUI
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public Settings Settings { get; set; }
        public int TableColumnsCount
        {
            get
            {
                return Settings.TableColumnsCount;
            }
            set
            {
                if(Settings.TableColumnsCount != value)
                {
                    Settings.TableColumnsCount = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public int[] AvailableTableColumns
        {
            get
            {
                return new int[] { 4, 8, 16, 32, 64, 128, 256, 512 };
            }
        }
        public string InputFile
        {
            get
            {
                return Settings.InputFilePath;
            }
            set
            {
                Settings.InputFilePath = value;
                //Histo(HistoFileSource.Input);
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
                //Histo(HistoFileSource.Output);
                this.OnPropertyChanged();
            }
        }

        public string Enthrophy
        {
            get
            {
                return $"Ентропія: {this.Histogram?.Entropy ?? 0}";
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
        public int LogorithmicalBlockLength
        {
            get
            {
                return Settings.LogBlockLength;
            }
            set
            {
                Settings.LogBlockLength = value;
                this.OnPropertyChanged("LogorithmicalBlockLength");
                this.OnPropertyChanged("AllowDiagonalModeSelection");
            }
        }
        public int[] BlockLengthValues
        {
            get
            {
                return new int[] { 128, 256, 512 };
            }
        }
        public int[] RoundValues
        {
            get
            {
                int[] values = new int[16];
                for(int i = 1; i <= 16; i++)
                {
                    values[i - 1] = i;
                }
                return values;
            }
        }
        public int SelectedRoundValues
        {
            get
            {
                return Settings.SelectedRoundValues;
            }
            set
            {
                Settings.SelectedRoundValues = value;
            }
        }
        public Visibility AllowDiagonalModeSelection
        {
            get
            {
                bool isVisible = LogorithmicalBlockLength >= 512 && (Math.Sqrt(LogorithmicalBlockLength / 8) % 1 == 0);
                return isVisible ? Visibility.Visible : Visibility.Hidden;
            }
        }
        public bool UseVariant1
        {
            get
            {
                return Settings.UseVariant1;
            }
            set 
            { 
                Settings.UseVariant1 = value;
                this.OnPropertyChanged();
            }
        }
        public bool UseVariant2
        {
            get
            {
                return Settings.UseVariant2;
            }
            set
            {
                Settings.UseVariant2 = value;
                this.OnPropertyChanged();
            }
        }
        public bool UseVariant3
        {
            get
            {
                return Settings.UseVariant3;
            }
            set
            {
                Settings.UseVariant3 = value;
                this.OnPropertyChanged();
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
                return !string.IsNullOrEmpty(this.InputFile) && !string.IsNullOrEmpty(this.OutputFile) && File.Exists(this.InputFile);
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
                int columns = this.TableColumnsCount;
                if (columns > 0)
                {
                    var valuesList = this.HistogramAsLabeledValues.ToList();
                    var remnantValues = columns - (valuesList.Count % columns);
                    valuesList.AddRange(Enumerable.Repeat<LabelValue>(new LabelValue(null, null), remnantValues));

                    DataTable dataTable = new DataTable();

                    for (int clmn = 0; clmn < columns; clmn++)
                    {
                        dataTable.Columns.Add(new DataColumn($"{clmn}"));
                    }

                    for (int row = 0; row < valuesList.Count / columns; row++)
                    {
                        var newRowValues = valuesList.Skip(row * columns).Take(columns);
                        if (newRowValues.All(x => x.Label == null && x.Value == null))
                        {
                            continue;
                        }
                        var newRow = dataTable.NewRow();
                        for (int column = 0; column < columns; column++)
                        {
                            newRow[column] = valuesList[row * columns + column].Value ?? null;
                        }
                        dataTable.Rows.Add(newRow);
                    }
                    return dataTable.DefaultView;
                }
                return null;
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
            encryptor = new SquareEncryption(this.LogorithmicalBlockLength, GetCurrentVariant(), () =>
            {
                this.ElapsedTime = stopWatch.Elapsed;
                this.OnPropertyChanged();
            });
            new Task(async () =>
            {
                var fileBytes = ReadFileBytes(this.InputFile);
                
                if (fileBytes is SmallFileBytes)
                {
                    byte[] processedBytes = (fileBytes as SmallFileBytes).Bytes;
                    encryptor.TotalSteps = SelectedRoundValues;
                    for (int pb = 0; pb < SelectedRoundValues; pb++)
                    {
                        if (UseEncryption)
                        {
                            processedBytes = encryptor.Encrypt(processedBytes, _cancellationTokenSource.Token);
                        }
                        else
                        {
                            processedBytes = encryptor.Decrypt(processedBytes, _cancellationTokenSource.Token);
                        }
                        if (_cancellationTokenSource.IsCancellationRequested)
                        {
                            this.IsEncryptionRunning = false;
                            return;
                        }
                        encryptor.Step = pb + 1;
                    }
                    File.WriteAllBytes(this.OutputFile, processedBytes);
                }
                if(fileBytes is LargeFileBytes)
                {
                    var largeFileBytes = fileBytes as LargeFileBytes;
                    using (FileStream fs = new FileStream(this.OutputFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        foreach (var bytes in largeFileBytes.Bytes)
                        {
                            byte[] processedBytes = null;
                            if (UseEncryption)
                            {
                                processedBytes = encryptor.Encrypt(bytes, _cancellationTokenSource.Token);
                            }
                            else
                            {
                                processedBytes = encryptor.Decrypt(bytes, _cancellationTokenSource.Token);
                            }
                            if (_cancellationTokenSource.IsCancellationRequested)
                            {
                                this.IsEncryptionRunning = false;
                                return;
                            }
                            await fs.WriteAsync(processedBytes, 0, processedBytes.Length, _cancellationTokenSource.Token);
                        }
                    }
                }
                
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
            string path = histoFileSource == HistoFileSource.Input ? this.InputFile : this.OutputFile;
            if (File.Exists(path))
            {
                var fileBytes = ReadFileBytes(histoFileSource == HistoFileSource.Input ? this.InputFile : this.OutputFile);
                if (fileBytes is LargeFileBytes) return;
                var bytes = (fileBytes as SmallFileBytes).Bytes;
                this.Histogram = new Histogram(bytes);
                string fileType = histoFileSource == HistoFileSource.Input ? "вхідного" : "вихідного";
                var pointsList = new LabelValue[this.Histogram.HistogramValues.Length];
                for (int x = 0; x < pointsList.Length; x++)
                {  
                    pointsList[x] = new LabelValue(x, this.Histogram.HistogramValues[x]);
                }
                //FIXME Add option to choose or unchoose 0s
                pointsList = pointsList/*.Where(x => x.Value != 0)*/.ToArray();
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
                    AxislineThickness = 0.4,
                    MinimumPadding = 0,
                    AbsoluteMinimum = 0,
                    TickStyle = TickStyle.Outside,
                    Angle = 90,
                    FontSize = 12,
                    MajorStep = 8
                });
                this.HistogramPlotModel.Axes.Add(new LinearAxis
                {
                    Position = AxisPosition.Left,
                    MinorGridlineStyle = LineStyle.Dot,
                    MajorGridlineStyle = LineStyle.Dot,
                    MinimumPadding = 0,
                    AbsoluteMinimum = 0,
                    TickStyle = TickStyle.Outside,
                    AxislineThickness = 0.4,
                    AxislineStyle = LineStyle.Solid,
                });
                this.HistogramPlotModel.Series.Add(new ColumnSeries
                {
                    ItemsSource = pointsList,
                    ValueField = "Value",
                    StrokeThickness = 0.1,
                    FillColor = OxyColor.FromArgb(255, 255, 0, 0),
                    StrokeColor = OxyColor.FromArgb(255,255, 0, 0)
                });
            }
        }

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isEncryptionRunning = false;
        SquareEncryption encryptor = null;
        private TimeSpan _elapsedTime;
        private Histogram _histogram = new Histogram();
        private PlotModel _histogramPlotModel = new PlotModel();
        private int logorithmicalBlockLength = 8;
        private void OnPropertyChanged(string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public enum HistoFileSource
        {
            Input,
            Output
        }
        private FileBytes ReadFileBytes(string path)
        {
            var fileLength = new FileInfo(path).Length;

            if (fileLength >= (Int32.MaxValue - 100))
            {
                var chunks = fileLength / (Int32.MaxValue - 100);
                chunks += fileLength % (Int32.MaxValue - 100) == 0 ? 0 : 1;

                List<byte[]> fileChunks = new List<byte[]>();
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    for(int x = 0; x <  chunks; x++)
                    {
                        byte[] chunkBytes = new byte[Int32.MaxValue - 100];
                        fs.Read(chunkBytes, 0, Int32.MaxValue - 100);
                        fileChunks.Add(chunkBytes);
                    }
                }
                LargeFileBytes lfb = new LargeFileBytes();
                lfb.Bytes = fileChunks;
                return lfb;
            }

            return new SmallFileBytes()
            {
                Bytes = File.ReadAllBytes(path)
            };
        }
        private BlockSizeVariant GetCurrentVariant()
        {
            if(UseVariant1) return BlockSizeVariant.Variant1;
            if(UseVariant2) return BlockSizeVariant.Variant2;
            if(UseVariant3) return BlockSizeVariant.Variant3;
            return BlockSizeVariant.Variant1;
        }
    }
    public enum BlockSizeVariant : short
    {
        Variant1 = 0,
        Variant2 = 1,
        Variant3 = 2,
    }
}
