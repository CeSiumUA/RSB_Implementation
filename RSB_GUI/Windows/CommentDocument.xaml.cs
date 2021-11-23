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
        public CommentDocument()
        {
            InitializeComponent();
            var commentFile = RSB_GUI.Properties.Resources.Comment;

            Uri packageUri;
            XpsDocument xpsDocument = null;

            using (MemoryStream xpsStream = new MemoryStream(commentFile))
            {
                using (Package package = Package.Open(xpsStream))
                {
                    packageUri = new Uri("memorystream://Comment.xps");

                    try
                    {
                        PackageStore.AddPackage(packageUri, package);
                        xpsDocument = new XpsDocument(package, CompressionOption.Maximum, packageUri.AbsoluteUri);

                        CommentDocumentViewer.Document = xpsDocument.GetFixedDocumentSequence();
                    }
                    finally
                    {
                        if (xpsDocument != null)
                        {
                            xpsDocument.Close();
                        }
                        PackageStore.RemovePackage(packageUri);
                    }
                }
            }
        }
    }
}
