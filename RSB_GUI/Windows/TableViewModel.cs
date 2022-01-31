using RSB_GUI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RSB_GUI.MainViewModel;

namespace RSB_GUI.Windows
{
    public class TableViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public readonly string FilePath;
        public readonly HistoFileSource HistoFileSource;
        public Settings Settings { get; set; }
        public int TableColumnsCount
        {
            get
            {
                return Settings.TableColumnsCount;
            }
            set
            {
                if (Settings.TableColumnsCount != value)
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
                return new int[] { 8, 10, 16 };
            }
        }
        public string Enthrophy
        {
            get
            {
                return $"Ентропія: {this.Histogram?.Entropy ?? 0}";
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

        public string InfoLabel { get; }

        public LabelValue[] HistogramAsLabeledValues
        {
            get
            {
                LabelValue[] labelValues = new LabelValue[Histogram.HistogramValues.Length];
                for(int x = 0; x<labelValues.Length; x++)
                {
                    labelValues[x] = new LabelValue(x, Histogram.HistogramValues[x]);
                }       
                return labelValues;
            }
        }
        public TableViewModel(string filePath, Settings settings = null, HistoFileSource histoFileSource = HistoFileSource.Input, string label = "")
        {
            this.FilePath = filePath;
            this.HistoFileSource = histoFileSource;
            this.Settings = settings ?? new Settings();
            this.InfoLabel = label;
            Histo();
        }
        public void Histo()
        {
            if (File.Exists(FilePath))
            {
                var fileBytes = ReadFileBytes(this.FilePath);
                if (fileBytes is LargeFileBytes) return;
                var bytes = (fileBytes as SmallFileBytes).Bytes;
                this.Histogram = new Histogram(bytes);
                string fileType = this.HistoFileSource == HistoFileSource.Input ? "вхідного" : "вихідного";
                var pointsList = new LabelValue[this.Histogram.HistogramValues.Length];
                for (int x = 0; x < pointsList.Length; x++)
                {
                    pointsList[x] = new LabelValue(x, this.Histogram.HistogramValues[x]);
                }
                

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
                    for (int x = 0; x < chunks; x++)
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
        private void OnPropertyChanged(string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private Histogram _histogram = new Histogram();
    }
}
