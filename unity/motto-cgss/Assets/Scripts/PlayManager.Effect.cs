using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using motto_cgss_core;
using UnityEngine;

public partial class PlayManager
{
    private List<int> _buttonHitTime;
    private int _resultStartTime;
    private int _comboStartTime;
    
    private void HandleResultAndCombo(int time)
    {
        // set text and combo
        if (_currentFrameWorstResult == Int32.MinValue)
        {
            _combo = 0;
            _comboText.text = "";
            _noteResultText.text = "MISS";
            _resultStartTime = time;
        }
        else if (_currentFrameWorstResult != Int32.MaxValue)
        {
            if (_combo > 5)
                _comboText.text = String.Format("{0} combo", _combo);
            _comboStartTime = time;
            _noteResultText.text = "PERFECT";
            _resultStartTime = time;
        }
        _currentFrameWorstResult = Int32.MaxValue;

        // fade text
        if (_resultStartTime != 0)
        {
            var diff = time - _resultStartTime;
            if (diff < SceneSettings.HitStatusTime)
            {
                var t = 1 - diff / SceneSettings.HitStatusTime;
                _noteResultText.color = new Color(1, 1, 1, t);
            }
            else
            {
                _resultStartTime = 0;
                _noteResultText.color = new Color(1, 1, 1, 0);
            }
        }

        // combo
        if (_comboStartTime != 0)
        {
            var diff = time - _comboStartTime;
            if (diff < SceneSettings.ComboAnimationTime)
            {
                var t = 1 - diff/SceneSettings.ComboAnimationTime;
                t = 1 + t*SceneSettings.ComboAnimationScale;
                _comboText.transform.localScale = new Vector3(t, t);
            }
            else
            {
                _comboStartTime = 0;
                _comboText.transform.localScale = new Vector3(1, 1);
            }
        }
    }

    private void ShowHitEffect(int time)
    {
        for (int i = 0; i < CurrentGame.NumberOfButtons; ++i)
        {
            var hitTime = _buttonHitTime[i];
            if (hitTime == 0)
                continue;

            var diff = time - hitTime;
            if (diff > SceneSettings.HitEffectTime)
            {
                _hitEffectImages[i].color = new Color(1, 1, 1, 0);
                _buttonHitTime[i] = 0;
                continue;
            }

            var t = 1 - diff/SceneSettings.HitEffectTime;

            _hitEffectImages[i].color = new Color(1, 1, 1, t*SceneSettings.HitEffectAlpha);
        }
    }
}
