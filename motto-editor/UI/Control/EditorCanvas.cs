using motto_cgss_core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using motto_cgss_core;

namespace motto_editor.UI.Control
{
    internal partial class EditorCanvas : Canvas
    {
        public EditorCanvas()
        {
            LayoutUpdated += EditorCanvas_LayoutUpdated;
        }

        private void EditorCanvas_LayoutUpdated(object sender, EventArgs e)
        {
            if (EditorStatus.Current.EditingMap != null)
                RecomputePositions();
        }
    }
}
