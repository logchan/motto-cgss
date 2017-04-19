using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace motto_editor.UI
{
    public partial class MainWindow
    {
        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (PlayingMusic)
                PausePlaying();

            CloseBeatmap();
        }

        private void BeatDivisionCombo_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RulerCanvas.BeatDivision = Int32.Parse((BeatDivisionCombo.SelectedItem as ComboBoxItem).Content.ToString());
            RulerCanvas.InvalidateVisual();
        }
    }
}
