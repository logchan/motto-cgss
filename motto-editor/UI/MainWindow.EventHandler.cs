using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
    }
}
