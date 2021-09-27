using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSB_GUI
{
    public class Histogram
    {
        public double Entropy
        {
            get
            {
                return _entropy;
            }
        }
        public int[] HistogramValues
        {
            get
            {
                return _histogramValues ?? new int[0];
            }
        }
        public Histogram(byte[] bytes)
        {
            InitializeHistogram(bytes);
        }
        public Histogram()
        {

        }
        public void InitializeHistogram(byte[] bytes)
        {
            _histogramValues = BytesHisto(bytes);
            _entropy = GetEntropy(_histogramValues, bytes.Length);
        }
        private double GetEntropy(int[] histogram, int N)
        {
            double entropy = 0;
            foreach(var val in histogram)
            {
                double divide = (double)val / (double)N;
                if(divide != 0)
                {
                    double step = divide * -Math.Log(divide, 2.0);
                    entropy += step;
                }
            }
            return entropy;
        }
        private int[] BytesHisto(byte[] bytes)
        {
            int[] frequentBytes = new int[256];
            foreach (byte num1 in bytes)
            {
                int num2 = frequentBytes[(int)num1] + 1;
                frequentBytes[(int)num1] = num2;
            }
            return frequentBytes;
        }
        private double _entropy = 0;
        private int[] _histogramValues;
    }
}
