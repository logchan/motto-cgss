using System;
using UnityEngine;
using UnityEngine.UI;
using motto_cgss_core.Model;
using UnityEngine.SceneManagement;
using Debug = System.Diagnostics.Debug;

public class SelectManager : MonoBehaviour
{
    private ScrollRect _scrollRect;
    private RectTransform _scrollContent;
    private Toggle _hdToggle;
    private Toggle _dtToggle;
    private Toggle _htToggle;
    private InputField _skipInput;
    private Text _pathText;
    private Toggle _autoToggle;
    private Slider _arSlider;
    private Text _arValue;
    private Toggle _autoBotToggle;
    private InputField _swipeThresholdInput;

    private GameObject _songInfoPrefab;
    private GameObject _diffBtnPrefab;

    public void Start()
	{
        // special: magic (for Unity test)
	    GameObject.Find("MagicButton").GetComponent<Button>().onClick.AddListener(GotoMagic);
	    GameObject.Find("ParamsButton").GetComponent<Button>().onClick.AddListener(GotoParameters);

        // set references
        _scrollRect = FindObjectOfType<ScrollRect>();
        _scrollContent = _scrollRect.content;
	    _songInfoPrefab = Resources.Load("SongInfo") as GameObject;
	    _diffBtnPrefab = Resources.Load("DiffButton") as GameObject;

        _pathText = GameObject.Find("PathText").GetComponent<Text>();

        _hdToggle = GameObject.Find("HiddenToggle").GetComponent<Toggle>();
        _dtToggle = GameObject.Find("DoubleTimeToggle").GetComponent<Toggle>();
        _htToggle = GameObject.Find("HalfTimeToggle").GetComponent<Toggle>();
        _skipInput = GameObject.Find("SkipInput").GetComponent<InputField>();
        _autoToggle = GameObject.Find("AutoToggle").GetComponent<Toggle>();
	    _arSlider = GameObject.Find("ArSlider").GetComponent<Slider>();
	    _arValue = GameObject.Find("ArValue").GetComponent<Text>();
        _autoBotToggle = GameObject.Find("AutoBotToggle").GetComponent<Toggle>();
        _swipeThresholdInput = GameObject.Find("SwipeThresholdInput").GetComponent<InputField>();

        // set values
        _pathText.text = GameManager.DataPath;
        _hdToggle.isOn = SceneSettings.Hidden;
        _dtToggle.isOn = SceneSettings.DoubleTime;
        _htToggle.isOn = SceneSettings.HalfTime;
	    _skipInput.text = SceneSettings.SkipTime.ToString("N2");
        _autoToggle.isOn = SceneSettings.Auto;
        _arSlider.onValueChanged.AddListener(ArSlider);
        _arSlider.value = SceneSettings.ApproachRate / 10.0f;
	    _autoBotToggle.isOn = SceneSettings.AutoBot;
        _swipeThresholdInput.text = SceneSettings.SwipeThreshold.ToString("N2");

        // display beatmaps
        float y = -10;
	    foreach (var info in GameManager.Beatmaps.Keys)
	    {
	        var obj = Instantiate(_songInfoPrefab);
	        var titleRect = obj.transform.GetChild(0) as RectTransform;
            var artistRect = obj.transform.GetChild(1) as RectTransform;
	        Debug.Assert(titleRect != null, "titleRect != null");
	        titleRect.gameObject.GetComponent<Text>().text = info.Title;
	        Debug.Assert(artistRect != null, "artistRect != null");
	        artistRect.gameObject.GetComponent<Text>().text = info.Artist;
	        obj.transform.position = new Vector3(40, y);
	        obj.transform.SetParent(_scrollContent);

	        y -= titleRect.rect.height + artistRect.rect.height + 10;

	        foreach (var map in GameManager.Beatmaps[info])
	        {
	            var btnObj = Instantiate(_diffBtnPrefab);
	            var btnRect = btnObj.transform.GetChild(0) as RectTransform;
	            Debug.Assert(btnRect != null, "btnRect != null");
	            btnRect.gameObject.GetComponentInChildren<Text>().text = map.DifficultyName;

	            var btnScript = btnRect.gameObject.AddComponent<SelectButton>();
                btnScript.Beatmap = map;
                btnScript.Manager = this;

	            btnRect.gameObject.GetComponent<Button>().onClick.AddListener(btnScript.OnClick);

                btnObj.transform.position = new Vector3(45, y);
	            btnObj.transform.SetParent(_scrollContent);

	            y -= btnRect.rect.height + 10;
	        }
	    }

	    _scrollContent.rect.Set(_scrollContent.rect.x, _scrollContent.rect.y, _scrollContent.rect.width, Mathf.Max(_scrollContent.rect.height, -y));
	}
	
    public void SelectMap(Beatmap map)
    {
        SceneSettings.Hidden = _hdToggle.isOn;
        SceneSettings.DoubleTime = _dtToggle.isOn;
        SceneSettings.HalfTime = _htToggle.isOn;
        SceneSettings.Auto = _autoToggle.isOn;
        SceneSettings.AutoBot = _autoBotToggle.isOn;

        float tmp;
        Single.TryParse(_skipInput.text, out tmp);
        SceneSettings.SkipTime = tmp;

        Single.TryParse(_swipeThresholdInput.text, out tmp);
        SceneSettings.SwipeThreshold = tmp;

        SceneSettings.ApproachRate = (int)(_arSlider.value*10);

        GameManager.BeatmapToPlay = map;
        SceneManager.LoadScene("PlayScene");
    }

    public void ArSlider(float val)
    {
        _arValue.text = val.ToString("N1");
    }

    private void GotoMagic()
    {
        SceneManager.LoadScene("MagicScene");
    }

    private void GotoParameters()
    {
        SceneManager.LoadScene("ParameterScene");
    }
}
