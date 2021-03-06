﻿using System;
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
    private int _kiaiStartTime = 0;

    private int _rotateStartTime = -1;
    private float _rotateStartAngle = 0;
    private float _prevRotateAngle = 0;
    private float _rotateTargetAngle;
    private float _rotateTargetTime;

    private bool _isRest = false;
    private int _restFadeStartTime = 0;
    private float _restFadeProgress = 0;
    private float _restFadeStartProgress = 0;

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

        ProcessKiai(time);
        ProcessEventRotate(time);
        ProcessRest(time);
    }

    private void ProcessOneEvent(BeatmapEvent ev, int time)
    {
        switch (ev.EventId)
        {
            case 1:
                _isKiai = true;
                Int32.TryParse(ev.EventArgs, out _kiaiBpm);
                _kiaiStartTime = time;
                break;
            case 2:
                _isKiai = false;
                break;
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
            case 4:
                _prevRotateAngle = 0;
                break;
            case 5:
                _isRest = true;
                _restFadeStartTime = time;
                _restFadeStartProgress = _restFadeProgress;
                break;
            case 6:
                _isRest = false;
                _restFadeStartTime = time;
                _restFadeStartProgress = _restFadeProgress;
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

    private void ProcessKiai(int time)
    {
        if (!_isKiai || _kiaiBpm == 0)
        {
            return;
        }


    }

    private void ProcessRest(int time)
    {
        if (_isRest)
        {
            if (_restFadeProgress >= 1)
                return;

            _restFadeProgress = (time - _restFadeStartTime)/SceneSettings.RestFadeTime + _restFadeStartProgress;
            if (_restFadeProgress > 1)
                _restFadeProgress = 1;
        }
        else
        {
            if (_restFadeProgress <= 0)
                return;

            _restFadeProgress = _restFadeStartProgress - (time - _restFadeStartTime)/SceneSettings.RestFadeTime;
            if (_restFadeProgress < 0)
                _restFadeProgress = 0;
        }

        _bgImage.color = new Color(1, 1, 1, SceneSettings.BgAlpha + (1 - SceneSettings.BgAlpha) * _restFadeProgress);
        foreach (var btnImg in _buttonImages)
        {
            btnImg.color = new Color(1, 1, 1, 1 - _restFadeProgress);
        }
    }
}