using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace motto_editor.UI.Control
{
    internal partial class EditorCanvas : Canvas
    {
        private readonly EventWaitHandle _renderHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        internal void RenderBlocked()
        {
            Dispatcher.Invoke(InvalidateVisual);
            _renderHandle.WaitOne();
        }

        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawEllipse(Brushes.AliceBlue, null, new Point(ActualWidth/2, ActualHeight/2), EditorStatus.Current.CurrentTime/400d, EditorStatus.Current.CurrentTime / 400d);
            _renderHandle.Set();
        }
    }
}
