using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GeneralGUI
{
    public class Input
    {
        // Constructor to populate properties 
        public Input(TextBox[] boxes) 
        {
            IDBox = boxes[0];
            NameBox = boxes[1];
        }
        public TextBox? IDBox { get;} // Id TextBox
        public TextBox? NameBox { get; } // Name TextBox
        public double MinValue { get; set; } = double.MinValue; // MinValue an input can be
        public double InvalidValue { get; set; } = double.MinValue; // Value that cannot be in calculation
    }
}
