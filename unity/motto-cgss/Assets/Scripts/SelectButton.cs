using UnityEngine;
using System.Collections;
using motto_cgss_core.Model;
using UnityEngine.SceneManagement;

public class SelectButton : MonoBehaviour {

    public Beatmap Beatmap { get; set; }
    public SelectManager Manager { get; set; }

    public void OnClick()
    {
        Manager.SelectMap(Beatmap);
    }
}
