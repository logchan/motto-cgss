using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using motto_cgss_core.Model;
using UnityEngine;

public partial class PlayManager
{
    private int _bmEventHead = 0;

    private bool _isKiai = false;
    private int _kiaiBpm = 0;

    private int _rotateStartTime = -1;
    private float _rotateStartAngle = 0;
    private float _prevRotateAngle = 0;
    private float _rotateTargetAngle;
    private float _rotateTargetTime;

    private void ProcessEvents(int time)
    {
        while (_bmEventHead < _currentMap.Events.Count)
        {
            var ev = _currentMap.Events[_bmEventHead];
            if (ev.Time > time)
                break;

            ProcessOneEvent(ev, time);
            ++_bmEventHead;
        }

        ProcessEventRotate(time);
    }

    private void ProcessOneEvent(BeatmapEvent ev, int time)
    {
        switch (ev.EventId)
        {
            case 3:
                _rotateStartTime = time;
                _rotateStartAngle = _prevRotateAngle;
                _rotateTargetAngle = 360;
                _rotateTargetTime = 1000;

                if (ev.EventArgs != null)
                {
                    var arr = ev.EventArgs.Split(';');
                    if (arr.Length > 0)
                        if (!float.TryParse(arr[0], out _rotateTargetAngle))
                            _rotateTargetAngle = 360;
                    if (arr.Length > 1)
                        if (!float.TryParse(arr[1], out _rotateTargetTime))
                            _rotateTargetTime = 1000;
                }

                break;
        }
    }

    private void ProcessEventRotate(int time)
    {
        if (_rotateStartTime < 0)
            return;

        var diff = time - _rotateStartTime;
        if (diff > _rotateTargetTime)
        {
            _rotateStartTime = -1;
            Camera.main.transform.eulerAngles = new Vector3(0, 0, _rotateTargetAngle);
            return;
        }

        var t = diff/_rotateTargetTime;
        t = Mathf.SmoothStep(-1, 1, Mathf.Lerp(0.5f, 1, t));
        _prevRotateAngle = (_rotateTargetAngle - _rotateStartAngle)*t + _rotateStartAngle;
        Camera.main.transform.eulerAngles = new Vector3(0, 0, _prevRotateAngle);
    }
}