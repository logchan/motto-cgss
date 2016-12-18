using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace motto_editor.UI.Control
{
    internal partial class RulerCanvas : Canvas
    {
        public int BeatDivision { get; set; } = 4;
        public double BeatWidth { get; set; } = 100;
    }
}
