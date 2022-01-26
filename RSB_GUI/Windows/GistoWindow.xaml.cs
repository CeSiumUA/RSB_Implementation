using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using RSB_GUI.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static RSB_GUI.MainViewModel;
using TickStyle = OxyPlot.Axes.TickStyle;

namespace RSB_GUI.Windows
{
    /// <summary>
    /// Interaction logic for GistoWindow.xaml
    /// </summary>
    public partial class GistoWindow : Window
    {
        public Histogram Histogram
        {
            get
            {
                return _histogram;
            }
            set
            {
                _histogram = value;
            }
        }
        public GistoWindow(string filePath, HistoFileSource histoFileSource = HistoFileSource.Input, string label = "")
        {
            InitializeComponent();
            this.Plot.Model = CreatePlotModel(filePath, histoFileSource);
            this.DescriptiveLabel.Content = label;
        }
        private void FileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var screenShot = GenerateScreenshot();
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = "gisto.png";
                saveFileDialog.Filter = "Image files (*.png)|*.png|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(screenShot));
                    using (var stream = File.Create(saveFileDialog.FileName))
                    {
                        encoder.Save(stream);
                    }
                    System.Windows.Forms.MessageBox.Show($"Файл збережено!{Environment.NewLine}Шлях: {saveFileDialog.FileName}", "OK!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString(), "Виникла помилка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void MakeScreenshot(string fileName)
        {
            var screenShot = GenerateScreenshot();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(screenShot));
            using (var stream = File.Create(fileName))
            {
                encoder.Save(stream);
            }
        }
        
        private void ClipboardMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var screenShot = GenerateScreenshot();
                System.Windows.Clipboard.SetImage(screenShot);
                System.Windows.Forms.MessageBox.Show("Збережено у буфер обміну!", "OK!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString(), "Виникла помилка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private RenderTargetBitmap GenerateScreenshot()
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(this);
            RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, 96, 96, PixelFormats.Pbgra32);

            DrawingVisual drawingVisual = new DrawingVisual();

            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                VisualBrush brush = new VisualBrush(this);
                drawingContext.DrawRectangle(brush, null, new Rect(new Point(), bounds.Size));
            }

            renderTarget.Render(this);
            return renderTarget;
        }
        private PlotModel CreatePlotModel(string filePath, HistoFileSource histoFileSource = HistoFileSource.Input)
        {
            PlotModel model = new PlotModel();
            if (File.Exists(filePath))
            {
                var fileBytes = ReadFileBytes(filePath);
                if (fileBytes is LargeFileBytes) return model;
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
                model = new PlotModel()
                {
                    Title = $"Гістограма {fileType} файлу, ентропія: {this.Histogram.Entropy}"
                };
                model.Axes.Add(new CategoryAxis
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
                model.Axes.Add(new LinearAxis
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
                model.Series.Add(new ColumnSeries
                {
                    ItemsSource = pointsList,
                    ValueField = "Value",
                    StrokeThickness = 0.1,
                    FillColor = OxyColor.FromArgb(255, 255, 0, 0),
                    StrokeColor = OxyColor.FromArgb(255, 255, 0, 0)
                });
                return model;
            }
            return model;
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
        private Histogram _histogram = new Histogram();

    }
}
