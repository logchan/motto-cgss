using System;
using motto_cgss_core.Utility;
using UnityEngine;

public partial class PlayManager
{
    public int CreateNote(int textureId, int noteId)
    {
        _managedObjects.Add(CreateNoteObject(textureId, noteId));
        _alphas.Add(1);
        return _managedObjects.Count;
    }

    public int CreateLine()
    {
        _managedObjects.Add(CreateLineObject());
        _alphas.Add(1);
        return _managedObjects.Count;
    }

    public void Destroy(int handle)
    {
        _destroyQueue.Enqueue(handle);
    }

    public void SetNote(int handle, int from, int to, double t)
    {
        var obj = GetManagedObject(handle, "SetNote");
        if (obj == null)
            return;

        float x, y;
        GetPosition(from, to, t, out x, out y);
        SetObjectPos(obj, x, y);

        var scale = GetNoteScale(t);
        obj.transform.localScale = new Vector3(scale * _noteScale, scale * _noteScale);

        if (SceneSettings.Hidden && t >= SceneSettings.HiddenStart)
        {
            var alpha = (float)(1 - MathHelper.Clamp((t - SceneSettings.HiddenStart) / SceneSettings.HiddenLength, 0, 1));
            var rend = obj.GetComponent<SpriteRenderer>();
            rend.color = new Color(1, 1, 1, alpha);
            _alphas[handle - 1] = alpha;
        }
    }

    public void SetLineBetweenNotes(int line, int note1, int note2)
    {
        var lineObj = GetManagedObject(line, "SetLineBetweenNotes.Line");
        var obj1 = GetManagedObject(note1, "SetLineBetweenNotes.Note1");
        var obj2 = GetManagedObject(note2, "SetLineBetweenNotes.Note2");

        if (lineObj == null || obj1 == null || obj2 == null)
            return;

        var lr = SetLine(lineObj, obj1, obj2);
        if (SceneSettings.Hidden)
        {
            var color = new Color(1, 1, 1, _alphas[note1 - 1]);
            lr.material.color = color;
        }
    }

    public void SetLineBetweenNoteAndStart(int line, int note, int start)
    {
        var lineObj = GetManagedObject(line, "SetLineBetweenNoteAndStart.Line");
        var obj = GetManagedObject(note, "SetLineBetweenNoteAndStart.Note1");

        if (lineObj == null || obj == null)
            return;

        var lr = SetHalfLine(lineObj, obj, start);
        if (SceneSettings.Hidden)
        {
            var color = new Color(1, 1, 1, _alphas[note - 1]);
            lr.material.color = color;
        }
    }

    public void SetLineOnPath(int line, int from, int to, double s, double t)
    {
        var lineObj = GetManagedObject(line, "SetLineOnPath.Line");
        var lr = lineObj.GetComponent<LineRenderer>();
        var points = Precomputed.GetLinePath(from, to, s, t);

        lr.SetWidth(GetNoteScale(s), GetNoteScale(t));
        lr.SetVertexCount(points.Length);
        lr.SetPositions(points);
        if (SceneSettings.Hidden)
        {
            lr.material.color = new Color(1, 1, 1, 1 - (float)s);
        }
    }

    public void SetButtonHit(int pos)
    {
        _buttonHitSize[pos] = SceneSettings.ButtonHitFrames;
    }

    public void AddNoteResult(int id, int level)
    {
        if (level > 0)
            level = -level;

        if (level < _currentFrameWorstResult)
            _currentFrameWorstResult = level;

        if (level != Int32.MinValue)
            ++_combo;
    }

    public void AddLog(string msg)
    {
        Debug.Log(msg);
    }
}

