using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace motto_editor.UI.Control
{
    internal partial class EditorCanvas
    {
        private const double CanvasMargin = 20;
        private const double NoteRadiusFactor = 0.06;
        private const double EndYFactor = 0.6181;
        private const double ButtonMarginFactor = 0.14;
        private const double StartYFactor = 0.1563;

        private double _areaWidth;
        private double _areaHeight;

        private double _noteRadius;

        private double _startY;
        private double _endY;
        private List<double> _startX;
        private List<double> _endX;

        private double _yA;
        private double _yB;
        private double _yC;

        private void ComputeNotePosition(double t, int start, int end, out double x, out double y)
        {
            x = (2*t/(t + 1))*(_endX[end] - _startX[start]) + _startX[start];
            y = _yA * t * t + _yB * t + _yC;
        }

        private double ComputeScale(double t)
        {
            return t < 0.25
                ? t*4*0.3
                : (t - 0.25)/0.75*0.7 + 0.3;
        }

        public void RecomputePositions()
        {
            var w = ActualWidth;
            var h = ActualHeight;
            if (w <= 0 || h <= 0)
                return;

            // TODO
            var ratio = w / h;
            if (ratio > 4.0/3.0)
            {
                _areaHeight = h - 2*CanvasMargin;
                _areaWidth = _areaHeight*4.0/3.0;
            }
            else
            {
                _areaWidth = w - 2*CanvasMargin;
                _areaHeight = _areaWidth*3.0/4.0;
            }

            var xOffset = (w - _areaWidth)/2;
            var yOffset = (h - _areaHeight)/2;

            _noteRadius = _areaHeight*NoteRadiusFactor;

            _startY = _areaHeight*StartYFactor + yOffset;
            _endY = _areaHeight*EndYFactor + yOffset;
            _yC = _startY;
            _yB = 41.0f / 39.0f * (_yC - _endY);
            _yA = -80.0f / 41.0f * _yB;

            var numBtn = EditorStatus.Current.EditingMap.NumberOfButtons;
            var buttonMargin = _areaWidth*ButtonMarginFactor;
            var buttonsMargin = _areaWidth
            - numBtn * _noteRadius * 2 // total width of buttons
            - (numBtn - 1) * buttonMargin; // total width between buttons

            _startX = new List<double>(numBtn);
            _endX = new List<double>(numBtn);

            for (int i = 0; i < numBtn; ++i)
            {
                _endX.Add(buttonsMargin/2 + i*(buttonMargin + _noteRadius*2) + _noteRadius + xOffset);
            }

            _startX.Add(_endX[0] + buttonMargin / 2);

            var shooterTotalWidth = _endX[numBtn - 1] - _endX[0] - buttonMargin;
            var betweenShooters = shooterTotalWidth / (numBtn - 1);
            for (int i = 1; i < numBtn; ++i)
            {
                _startX.Add(_startX[i - 1] + betweenShooters);
            }
        }
    }
}
