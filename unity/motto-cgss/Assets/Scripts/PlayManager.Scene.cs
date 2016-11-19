using System;
using motto_cgss_core;
using motto_cgss_core.Utility;
using UnityEngine;

public partial class PlayManager
{
    private void SetZ(GameObject obj, float z)
    {
        var pos = new Vector3(obj.transform.position.x, obj.transform.position.y, z);
        obj.transform.position = pos;
    }

    private Vector3 OriginalScreenToWorld(float x, float y, float setZ)
    {
        var vec = _originalViewportToWorldMatrix*new Vector4(x/_width, y/_height, 0, 1);
        return new Vector3(vec.x/vec.w, vec.y/vec.w, setZ);
    }

    private void SetObjectPos(GameObject obj, float x, float y)
    {
        var z = obj.transform.position.z;

        obj.transform.position = OriginalScreenToWorld(x, y, z);
    }

    private GameObject CreateNoteObject(int textureId, int id = 1)
    {
        var obj = new GameObject();
        obj.name = String.Format("Note {0}", id);
        SetObjectPos(obj, -100, -100);
        SetZ(obj, CurrentGame.NotesCount - id);

        var sprite = _noteSprites[textureId];
        var objRenderer = obj.AddComponent<SpriteRenderer>();
        obj.transform.localScale = _noteScaleV;

        objRenderer.sprite = sprite;

        return obj;
    }

    public GameObject CreateLineObject()
    {
        GameObject obj = new GameObject();
        SetObjectPos(obj, -100, -100);
        SetZ(obj, _lineZ);

        obj.AddComponent<LineRenderer>();
        var lr = obj.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Transparent/Diffuse"));
        lr.material.SetColor("_EmissionColor", Color.white);
        lr.SetWidth(0.1f, 0.1f);
        return obj;
    }

    private GameObject GetManagedObject(int handle, string sender)
    {
        var idx = handle - 1;
        if (idx < 0 || idx >= _managedObjects.Count)
        {
            Debug.LogWarning(String.Format("[{0}] Invalid handle {1}", sender, handle));
            return null;
        }

        var obj = _managedObjects[idx];
        if (obj == null)
        {
            Debug.LogWarning(String.Format("[{0}] Using destroyed handle {1}", sender, handle));
        }

        return obj;
    }

    private LineRenderer SetLine(GameObject obj, GameObject note1, GameObject note2)
    {
        var lr = obj.GetComponent<LineRenderer>();

        var p1 = new Vector3(note1.transform.position.x, note1.transform.position.y, _lineZ);
        var p2 = new Vector3(note2.transform.position.x, note2.transform.position.y, _lineZ);

        lr.SetPositions(new[] { p1, p2 });
        return lr;
    }

    private LineRenderer SetHalfLine(GameObject obj, GameObject note, int startPos)
    {
        var lr = obj.GetComponent<LineRenderer>();

        var p1 = new Vector3(note.transform.position.x, note.transform.position.y, _lineZ);

        var p2Temp = Camera.main.ScreenToWorldPoint(new Vector3(_shooterX[startPos], SceneSettings.ShooterHeight, 10.0f));
        var p2 = new Vector3(p2Temp.x, p2Temp.y, _lineZ);
        lr.SetPositions(new[] { p1, p2 });
        return lr;
    }

    private void GetPosition(int start, int end, double t, out float x, out float y)
    {
        t = MathHelper.Clamp(t, 0, 1);

        // from official CGSS
        var xfactor = 2 * t / (t + 1);
        x = (float)(_shooterX[start] + (_buttonX[end] - _shooterX[start]) * xfactor);
        y = (float)(_yA * t * t + _yB * t + _yC);
    }

    private float GetNoteScale(double t)
    {
        t = MathHelper.Clamp(t, 0, 1);

        // from official CGSS
        return t < 0.25 ? 
            (float) (t*4*0.3) : 
            (float) ((t - 0.25)/0.75*0.7 + 0.3);
    }
}

