using System.IO;
using System.Windows;
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
