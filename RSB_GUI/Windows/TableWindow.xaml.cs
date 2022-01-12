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
using Clipboard = System.Windows.Clipboard;

namespace RSB_GUI.Windows
{
    /// <summary>
    /// Interaction logic for TableWindow.xaml
    /// </summary>
    public partial class TableWindow : Window
    {
        private readonly TableViewModel _viewModel;
        public TableWindow(TableViewModel tableViewModel)
        {
            InitializeComponent();
            this._viewModel = tableViewModel;
            this.DataContext = _viewModel;
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

        private void HistogramDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var rowIndex = e.Row.GetIndex();
            if (rowIndex % 2 == 0)
            {
                e.Row.Background = Brushes.LightGray;
            }
            e.Row.Header = (e.Row.GetIndex() * this._viewModel.TableColumnsCount);
            e.Row.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
        }

        private void FileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                var screenShot = GenerateScreenshot();
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = "table.png";
                saveFileDialog.Filter = "Image files (*.png)|*.png|All files (*.*)|*.*";
                if(saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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

        private void ClipboardMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var screenShot = GenerateScreenshot();
                System.Windows.Clipboard.SetImage(screenShot);
                System.Windows.Forms.MessageBox.Show("Збережено у буфер обміну!", "OK!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch(Exception ex)
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

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(this._viewModel.Histogram.Entropy.ToString("##.#######"));
        }
    }
}