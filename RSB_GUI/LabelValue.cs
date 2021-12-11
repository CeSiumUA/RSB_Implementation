using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSB_GUI
{
    public class LabelValue
    {
        public double? Label { get; set; }
        public double? Value { get; set; }
        public LabelValue(double? label, double? value)
        {
            Label = label;
            Value = value;
        }
    }
}
