using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Xps.Packaging;

namespace RSB_GUI.Windows
{
    /// <summary>
    /// Interaction logic for CommentDocument.xaml
    /// </summary>
    public partial class CommentDocument : Window
    {
        private string _tempFilePath = null;
        public CommentDocument(string tempFilePath = null)
        {
            InitializeComponent();
            _tempFilePath = tempFilePath;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var commentFile = RSB_GUI.Properties.Resources.Comment;

            if (string.IsNullOrEmpty(_tempFilePath))
            {
                _tempFilePath = System.IO.Path.GetTempFileName();
            }

            File.WriteAllBytes(_tempFilePath, commentFile);

            CommentDocumentViewer.Document = (new XpsDocument(_tempFilePath, FileAccess.Read)).GetFixedDocumentSequence();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
