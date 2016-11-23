using System;
using System.Collections.Generic;
using motto_cgss_core;
using motto_cgss_core.Model;
using UnityEngine;

public partial class PlayManager
{
    private void HandleInput(int time)
    {
        for (int i = 0; i < CurrentGame.NumberOfButtons; ++i)
            _btnHasInput[i] = false;

        if (SceneSettings.AutoBot)
        {
            AutoBot(time);
        }
        else
        {
            HandleUserInput(time);
        }

        for (int i = 0; i < CurrentGame.NumberOfButtons; ++i)
        {
            if (!_btnHasInput[i] && CurrentGame.ButtonStates[i] != ButtonState.None)
            {
                CurrentGame.ButtonStates[i] = ButtonState.None;
            }
        }
            
        for (int i = 0; i < CurrentGame.NumberOfButtons; ++i)
        {
            CurrentGame.ButtonHandled[i] = false;
        }
    }

    private int TouchedButton(Touch touch)
    {
        for (int i = 0; i < CurrentGame.NumberOfButtons; ++i)
        {
            if (Math.Abs(touch.position.x - _buttonX[i]) < SceneSettings.NoteRadius &&
                Math.Abs(touch.position.y - SceneSettings.ButtonY) < SceneSettings.NoteRadius)
            {
                return i;
            }
        }
        return -1;
    }

    private List<int> SwipedButtons(Touch touch)
    {
        var result = new List<int>();

        for (int i = 0; i < CurrentGame.NumberOfButtons; ++i)
        {
            var x = _buttonXNormalized[i];
            var y = _buttonYNormalized;

            var x1 = touch.position.x/_width;
            var y1 = touch.position.y/_height;
            var x2 = x1 - touch.deltaPosition.x/_width;
            var y2 = y1 - touch.deltaPosition.y/_height;

            // Debug.Log(String.Format("[{0}] Checking, x = {1}, y = {2}, touch x1 = {3}, y1 = {4}, x2 = {5}, y2 = {6}", i, x, y, x1, y1, x2, y2));

            // x shall be between two touch points
            var xleft = x - SceneSettings.NoteRadiusNormalized;
            var xright = x + SceneSettings.NoteRadiusNormalized;
            if ((x1 < x2 && xleft < x2 && xright > x1) ||
                (x2 < x1 && xleft < x1 && xright > x2))
            {
                var dist = Mathf.Abs((y2 - y1) * x - (x2 - x1) * y + x2 * y1 - y2 * x1) /
                       Mathf.Sqrt((y2 - y1) * (y2 - y1) + (x2 - x1) * (x2 - x1));
                // Debug.Log(String.Format("{0} ({1})", dist, SceneSettings.NoteRadiusNormalized));
                if (dist < SceneSettings.NoteRadiusNormalized)
                    result.Add(i);
            }
        }
        return result;
    }

    private void HandleUserInput(int time)
    {
        for (int i = 0; i < Input.touchCount; ++i)
        {
            var touch = Input.GetTouch(i);
            var spd = Math.Abs(touch.deltaPosition.x)/Time.deltaTime/_width;

            if (touch.phase != TouchPhase.Moved || spd < SceneSettings.SwipeThreshold)
            {
                if (touch.phase == TouchPhase.Moved)
                {
                    _debugText.text = spd.ToString("N3");
                }

                var btnId = TouchedButton(touch);
                if (btnId < 0)
                    continue;

                _btnHasInput[btnId] = true;
                switch (touch.phase)
                {
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        CurrentGame.ButtonStates[btnId] = ButtonState.None;
                        break;
                    case TouchPhase.Began:
                        CurrentGame.ButtonStates[btnId] = ButtonState.Hit;
                        break;
                    case TouchPhase.Stationary:
                        CurrentGame.ButtonStates[btnId] = ButtonState.Hold;
                        break;
                    case TouchPhase.Moved:
                        CurrentGame.ButtonStates[btnId] = ButtonState.Hold;
                        break;
                }
            }
            else
            {
                var btnIds = SwipedButtons(touch);
                var state = touch.deltaPosition.x < 0 ? ButtonState.Left : ButtonState.Right;
                foreach (var btn in btnIds)
                {
                    // Debug.Log(String.Format("[{0}] {1}: {2}", time, btn, state));
                    _btnHasInput[btn] = true;
                    CurrentGame.ButtonStates[btn] = state;
                }
            }
        }
    }

    private void AutoBot(int time)
    {
        for (int i = _noteHead; i < _currentMap.Notes.Count; ++i)
        {
            var note = _currentMap.Notes[i];
            if (note.Status == NoteStatus.Done || _btnHasInput[note.TouchPosition])
                continue;

            _btnHasInput[note.TouchPosition] = true;

            var btn = note.TouchPosition;

            var holdNote = note as HoldNote;
            if (holdNote != null)
            {
                if (time < holdNote.Time - 10)
                    CurrentGame.ButtonStates[btn] = ButtonState.None;
                else if (time < holdNote.EndTime - 10)
                {
                    if (CurrentGame.ButtonStates[btn] == ButtonState.Hit)
                        CurrentGame.ButtonStates[btn] = ButtonState.Hold;
                    else if (CurrentGame.ButtonStates[btn] != ButtonState.Hold)
                        CurrentGame.ButtonStates[btn] = ButtonState.Hit;
                }
                else
                {
                    var gpSwipe = holdNote.EndId == 0 ? null : _currentMap.NotesMap[holdNote.EndId] as SwipeNote;

                    if (gpSwipe == null)
                        CurrentGame.ButtonStates[btn] = ButtonState.None;
                    else
                        CurrentGame.ButtonStates[btn] = gpSwipe.Direction == 0
                            ? ButtonState.Left
                            : ButtonState.Right;
                }
            }

            var hitNote = note as HitNote;
            if (hitNote != null)
            {
                if (time < hitNote.Time - 10)
                    CurrentGame.ButtonStates[btn] = ButtonState.None;
                else
                    CurrentGame.ButtonStates[btn] = ButtonState.Hit;
            }

            var swipeNote = note as SwipeNote;
            if (swipeNote != null)
            {
                if (time < swipeNote.Time - 10)
                    CurrentGame.ButtonStates[btn] = ButtonState.None;
                else
                    CurrentGame.ButtonStates[btn] = swipeNote.Direction == 0 ? ButtonState.Left : ButtonState.Right;
            }

            if (note.Status == NoteStatus.NotShown)
                break;
        }
    }
}

