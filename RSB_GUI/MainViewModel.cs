using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RSB_GUI
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public string InputFile
        {
            get
            {
                return _inputFile;
            }
            set
            {
                _inputFile = value;
                this.OnPropertyChanged();
            }
        }
        public string OutputFile
        {
            get
            {
                return _outputFile;
            }
            set
            {
                _outputFile = value;
                this.OnPropertyChanged();
            }
        }
        public int BlockPower
        {
            get
            {
                return _blockPower;
            }
            set
            {
                this._blockPower = value;
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
                return _shiftValue;
            }
            set
            {
                _shiftValue = value;
                this.OnPropertyChanged();
            }
        }
        public bool UseCommonKey
        {
            get
            {
                return _useCommonKey;
            }
            set
            {
                _useCommonKey = value;
                this.OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public MainViewModel()
        {

        }

        public async Task StartEncryption()
        {
            this._cancellationTokenSource = new CancellationTokenSource();
            RSBEcnryptor encryptor = new RSBEcnryptor(this.BlockLength, this.ShiftValue);
            var fileBytes = File.ReadAllBytes(this.InputFile);
            var encryptedBytes = await encryptor.EncryptBytes(fileBytes, _cancellationTokenSource.Token);
            if (_cancellationTokenSource.IsCancellationRequested) return;
            File.WriteAllBytes(this.OutputFile, encryptedBytes);
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

        private CancellationTokenSource _cancellationTokenSource;
        private string _inputFile = string.Empty;
        private string _outputFile = string.Empty;
        private int _blockPower = 0;
        private int _shiftValue = 0;
        private bool _useCommonKey = false;
        private void OnPropertyChanged(string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
