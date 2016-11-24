using System;
using UnityEngine;
using System.Collections;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utilities;

public class ParameterSceneManager : MonoBehaviour
{

    private Text _infoText;

    private void PutValue(string propname, float value)
    {
        GameObject.Find(propname + "Input").GetComponent<InputField>().text = value.ToString("N4");
    }

    private float GetValue(string propname, float defaultvalue)
    {
        var f = GameObject.Find(propname + "Input").GetComponent<InputField>();
        float v;
        if (!Single.TryParse(f.text, out v))
        {
            v = defaultvalue;
        }

        f.text = v.ToString("N4");
        return v;
    }

	public void Start () {
        var settings = GameManager.Settings;
	    PutValue("NoteSizeFactor", settings.NoteSizeFactor);
	    PutValue("ButtonYFactor", settings.ButtonYFactor);
	    PutValue("BetweenButtonsFactor", settings.BetweenButtonsFactor);
	    PutValue("ShooterHeightFactor", settings.ShooterHeightFactor);

        GameObject.Find("BackButton").GetComponent<Button>().onClick.AddListener(OnBack);
        GameObject.Find("SaveButton").GetComponent<Button>().onClick.AddListener(OnSave);

	    _infoText = GameObject.Find("InformationText").GetComponent<Text>();
	    SetInfo();
	}

    private void OnBack()
    {
        SceneManager.LoadScene("SelectScene");
    }

    private void OnSave()
    {
        var settings = GameManager.Settings;
        settings.NoteSizeFactor = GetValue("NoteSizeFactor", settings.NoteSizeFactor);
        settings.ButtonYFactor = GetValue("ButtonYFactor", settings.ButtonYFactor);
        settings.BetweenButtonsFactor = GetValue("BetweenButtonsFactor", settings.BetweenButtonsFactor);
        settings.ShooterHeightFactor = GetValue("ShooterHeightFactor", settings.ShooterHeightFactor);

        GameManager.WriteSettings();
    }

    private void SetInfo()
    {
        var sb = new StringBuilder();

        sb.AppendLine("Device info");
        sb.AppendLine("width {0}, height {1}", Screen.width, Screen.height);
        sb.AppendLine("dpi: {0}", Screen.dpi);
        sb.AppendLine("orientation: {0}", Screen.orientation);

        _infoText.text = sb.ToString();
    }
}
