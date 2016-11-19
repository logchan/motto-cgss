using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using motto_cgss_core;
using motto_cgss_core.Model;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public partial class PlayManager : MonoBehaviour, ISceneController {

    private List<int> _shooterX;
    
    private float _yA;
    private float _yB;
    private float _yC;

    private float _noteScale;
    private Vector3 _noteScaleV;
    private int _lineZ;

    private bool _isPlaying;
    private Beatmap _currentMap;
    private string _skinPath;
    private AudioSource _audio;
    private AudioSource _seAudio;
    private AudioClip _audioClip;
    private volatile bool _shallStartPlay;
    private AudioClip _hitsound;
    private AudioClip _swipesound;

    private List<Sprite> _noteSprites;
    private List<int> _buttonX;
    private List<GameObject> _buttonObjects;
    private int _noteHead;
    private List<int> _buttonHitSize;
    private List<bool> _btnHasInput;

    private List<GameObject> _managedObjects;
    private List<float> _alphas;
    private Queue<int> _destroyQueue;

    private int _currentFrameWorstResult = Int32.MaxValue;
    private int _resultShowTime = 60;
    private int _resultCountdown;
    private int _combo;

    private Button _backButton;
    private Text _fpsText;
    private Text _noteResultText;
    private Text _comboText;
    private Text _debugText;

    private readonly System.Diagnostics.Stopwatch _watch = new System.Diagnostics.Stopwatch();
    private int _passedFrames;

    private float _lastAudioTime;
    private float _lastComputedTime;

    private float _width;
    private float _height;
    private Matrix4x4 _originalViewportToWorldMatrix; 

    // Use this for initialization
    // ReSharper disable once UnusedMember.Local
    void Start ()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;

        _skinPath = Path.Combine(Path.Combine(GameManager.DataPath, "Skins"), "Default");
        _audio = GameObject.Find("Audio Source").GetComponent<AudioSource>();
        _seAudio = GameObject.Find("SE Audio Source").GetComponent<AudioSource>();
        _backButton = GameObject.Find("BackButton").GetComponent<Button>();
        _backButton.onClick.AddListener(BackClicked);
        _fpsText = GameObject.Find("FpsText").GetComponent<Text>();
        _noteResultText = GameObject.Find("NoteResultText").GetComponent<Text>();
        _comboText = GameObject.Find("ComboText").GetComponent<Text>();
        _debugText = GameObject.Find("DebugText").GetComponent<Text>();

        string modText = "";
        if (SceneSettings.Hidden)
            modText += "HD";
        if (SceneSettings.DoubleTime)
            modText += "DT";
        if (SceneSettings.HalfTime)
            modText += "HT";
        GameObject.Find("ModText").GetComponent<Text>().text = modText;

        _managedObjects = new List<GameObject>();
        _alphas = new List<float>();
        _destroyQueue = new Queue<int>();

        Initialize(Screen.width, Screen.height);
	}
	
	// Update is called once per frame
    // ReSharper disable once UnusedMember.Local
	void Update () {
        if (_shallStartPlay && _audioClip.loadState == AudioDataLoadState.Loaded && _hitsound.loadState == AudioDataLoadState.Loaded && _swipesound.loadState == AudioDataLoadState.Loaded)
        {
            StartPlay();
        }

	    if (_isPlaying)
        {
            /*
             * To sync the notes to music, we must use audio time in computation.
             * However, AudioSource.Time may give identical values in 3~5 consecutive frames,
             * resulting in a 15~30Hz update frequency of the notes.
             * Therefore, when we see the same time returned again, we add deltaTime to obtain a more accurate time for the notes
             * i.e.
             * Frames      1       2        3        4
             * AudioTime   a       a        a        b    [Notes only updated at F1, fps_notes == fps / 3]
             * GameTime    a       a+d1     a+d1+d2  b    [Notes always updated, fps_notes == fps]
             */
            if (_audio.time - _lastAudioTime < 0.001f)
            {
                _lastComputedTime += Time.deltaTime;
            }
            else
            {
                _lastAudioTime = _audio.time;
                _lastComputedTime = _lastAudioTime;
            }
            CurrentGame.Time = (int)(_lastComputedTime * 1000) + _currentMap.Offset;

            // fps counter
            ++_passedFrames;
            if (_passedFrames == 10)
            {
                var timediff = _watch.ElapsedMilliseconds;
                var fps = 1f/timediff*1000*_passedFrames;
                _fpsText.text = String.Format("{0:N2}", fps);
                _passedFrames = 0;
                _watch.Reset();
                _watch.Start();
            }

            // handle input
            HandleInput(CurrentGame.Time);

            // compute notes
            bool headUpdated = false;
            int oldHead = _noteHead;
            int end = 0;
            for (int i = _noteHead; i < _currentMap.Notes.Count; ++i)
            {
                var note = _currentMap.Notes[i];
                if (note.Status == NoteStatus.Done || note.Time < SceneSettings.SkipTime * 1000)
                    continue;

                note.ComputeNote();
                end = i;

                if (note.Status == NoteStatus.NotShown)
                    break;

                if (!headUpdated && note.Status != NoteStatus.Done)
                {
                    headUpdated = true;
                    _noteHead = i;
                }
            }

            // draw notes
            for (int i = oldHead; i <= end; ++i)
            {
                _currentMap.Notes[i].DrawNote();
            }

            // reset computation state
            for (int i = _noteHead; i <= end; ++i)
            {
                _currentMap.Notes[i].FrameComputed = false;
            }

            // set text and combo
            if (_currentFrameWorstResult == Int32.MinValue)
            {
                _combo = 0;
                _comboText.text = "";
                _noteResultText.text = "MISS";
                _resultCountdown = _resultShowTime;
            }
            else if (_currentFrameWorstResult != Int32.MaxValue)
            {
                if (_combo > 5)
                    _comboText.text = String.Format("{0} combo", _combo);
                _noteResultText.text = "PERFECT";
                _resultCountdown = _resultShowTime;
            }
            _currentFrameWorstResult = Int32.MaxValue;

            if (_resultCountdown > 0)
            {
                --_resultCountdown;
                _noteResultText.color = new Color(1, 1, 1, _resultCountdown / (float)_resultShowTime);
            }

            // animate the buttons being hit
            for (int i = 0; i < CurrentGame.NumberOfButtons; ++i)
            {
                var state = _buttonHitSize[i];
                if (state == 0)
                    continue;

                float scale = _noteScale + _noteScale * state / SceneSettings.ButtonHitFrames / 4;
                _buttonObjects[i].transform.localScale = new Vector3(scale, scale);
                --_buttonHitSize[i];
            }

            // play hitsound
            Hitsound();

            // destroy if needed
            while (_destroyQueue.Count > 0)
            {
                var handle = _destroyQueue.Dequeue();
                var idx = handle - 1;

                if (idx >= 0 && idx < _managedObjects.Count && _managedObjects[idx] != null)
                {
                    Destroy(_managedObjects[idx]);
                    _managedObjects[idx] = null;
                }
            }
        }
	}

    public void BackClicked()
    {
        _shallStartPlay = false;
        if (_isPlaying)
        {
            _audio.Stop();
            _isPlaying = false;
        }
        SceneManager.LoadScene("SelectScene");
    }

    private void StartPlay()
    {
        if (_isPlaying)
            return;

        _audio.clip = _audioClip;
        _audio.Play();
        _audio.pitch = (float)CurrentGame.SpeedFactor;
        _audio.time = SceneSettings.SkipTime;
        _isPlaying = true;
        _shallStartPlay = false;
    }

    private void Hitsound()
    {
        int se = CurrentGame.SeToPlay;
        if ((se & Constants.HitSound) > 0)
            _seAudio.PlayOneShot(_hitsound);
        if ((se & Constants.SwipeSound) > 0)
            _seAudio.PlayOneShot(_swipesound);
        CurrentGame.SeToPlay = 0;
    }

    private void Initialize(int width, int height)
    {
        var bm = GameManager.BeatmapToPlay;
        _currentMap = bm;
        _combo = 0;

        ////// Compute sizes

        SceneSettings.NoteSize = (int) (height*SceneSettings.NoteSizeFactor);
        SceneSettings.NoteRadius = SceneSettings.NoteSize/2;
        SceneSettings.ButtonY = (int) (height * SceneSettings.ButtonYFactor);
        SceneSettings.BetweenButtons = (int) (height * SceneSettings.BetweenButtonsFactor);
        SceneSettings.ShooterHeight = (int) (height*SceneSettings.ShooterHeightFactor);

        ////// Set data in CurrentGame

        CurrentGame.NumberOfButtons = bm.NumberOfButtons;
        CurrentGame.ApproachTime = GameManager.ArToTime(SceneSettings.ApproachRate);
        CurrentGame.Time = 0;
        CurrentGame.SpeedFactor = SceneSettings.DoubleTime ? 1.5 : 1.0;
        CurrentGame.SpeedFactor *= SceneSettings.HalfTime ? 0.5 : 1.0;
        CurrentGame.Scene = this;
        CurrentGame.NoteDelay = 100;
        CurrentGame.NotesCount = bm.Notes.Count;
        CurrentGame.MissTime = 200;
        CurrentGame.EarliestTime = 100;
        CurrentGame.ButtonHandled = new List<bool>();
        CurrentGame.ButtonStates = new List<ButtonState>();
        CurrentGame.AutoPlay = SceneSettings.Auto;

        ////// parameters

        _lineZ = CurrentGame.NotesCount + 1;
        _noteScale = SceneSettings.NoteSize / (float)SceneSettings.SpriteSize;
        _noteScaleV = new Vector3(_noteScale, _noteScale);

        _width = width;
        _height = height;
        var magicMatrix = new Matrix4x4
        {
            m00 = 1,
            m03 = -0.5f,
            m11 = 1,
            m13 = -0.5f,
            m22 = 1,
            m33 = 0.5f
        };
        _originalViewportToWorldMatrix = Camera.main.cameraToWorldMatrix*Camera.main.projectionMatrix.inverse*magicMatrix;

        // parameters for y-axis

        _yC = SceneSettings.ShooterHeight;
        _yB = 41.0f / 39.0f * (_yC - SceneSettings.ButtonY);
        _yA = -80.0f / 41.0f * _yB;

        ////// init buttons

        // total blank width to two sides of buttons (w1+w2):
        // |<--w1-->o  o  o  o  o<--w2-->|
        int notesMargin = width
            - bm.NumberOfButtons * SceneSettings.NoteSize              // total width of buttons
            - (bm.NumberOfButtons - 1) * SceneSettings.BetweenButtons; // total width between buttons

        var btnTexture = UnityHelper.TextureFromFile(Path.Combine(_skinPath, "touchpad.png"), SceneSettings.SpriteSize);

        _buttonX = new List<int>();
        _buttonObjects = new List<GameObject>();
        _buttonHitSize = new List<int>();
        _btnHasInput = new List<bool>();
        for (int i = 0; i < bm.NumberOfButtons; ++i)
        {
            _buttonHitSize.Add(0);
            _btnHasInput.Add(false);
            CurrentGame.ButtonStates.Add(ButtonState.None);
            CurrentGame.ButtonHandled.Add(false);

            int x = notesMargin / 2 + i * (SceneSettings.BetweenButtons + SceneSettings.NoteSize) + SceneSettings.NoteSize / 2;

            _buttonX.Add(x);

            var btn = UnityHelper.SpriteFromTexture(btnTexture);

            var obj = new GameObject();
            SetObjectPos(obj, x, SceneSettings.ButtonY);
            SetZ(obj, CurrentGame.NotesCount); // buttons behind notes

            obj.transform.localScale = _noteScaleV;

            SpriteRenderer objRenderer = obj.AddComponent<SpriteRenderer>();
            objRenderer.sprite = btn;
            _buttonObjects.Add(obj);
        }

        ////// init shooters
        // shooter positions like this (.. means some shooters in between)
        // |-----S..S..S-----|
        // |----o o o o o----|

        _shooterX = new List<int>();
        for (int i = 0; i < bm.NumberOfButtons; ++i)
        {
            _shooterX.Add(0);
        }

        _shooterX[0] = _buttonX[0] + SceneSettings.BetweenButtons / 2; // first shooter at between 1st and 2nd buttons

        int shooterTotalWidth = _buttonX[bm.NumberOfButtons - 1] - _buttonX[0] - SceneSettings.BetweenButtons;
        double betweenShooters = shooterTotalWidth / (double)(bm.NumberOfButtons - 1);

        for (int i = 1; i < bm.NumberOfButtons; ++i)
        {
            _shooterX[i] = (int)(_shooterX[0] + betweenShooters * i);
        }

        ////// Precompute path points

        Precomputed.PathPoints = new List<Vector3>();
        for (int i = 0; i < bm.NumberOfButtons; ++i)
        {
            for (int j = 0; j < bm.NumberOfButtons; ++j)
            {
                for (int qt = 0; qt <= Precomputed.QuantizeFactor; ++qt)
                {
                    var t = qt / (double)Precomputed.QuantizeFactor;
                    float x, y;
                    GetPosition(i, j, t, out x, out y);
                    var pos = Camera.main.ScreenToWorldPoint(new Vector3(x, y, 10.0f));
                    Precomputed.PathPoints.Add(new Vector3(pos.x, pos.y, _lineZ));
                }
            }
        }

        // init textures

        _noteSprites = new List<Sprite>();
        var textureNames = new[] { "circle.png", "left.png", "right.png", "hold.png" };
        foreach (var textureName in textureNames)
        {
            var texture = UnityHelper.TextureFromFile(Path.Combine(_skinPath, textureName), SceneSettings.SpriteSize);
            _noteSprites.Add(UnityHelper.SpriteFromTexture(texture));
        }

        // init nodes

        _noteHead = 0;
        foreach (var note in bm.Notes)
        {
            note.GameInit();
        }

        // init audio

        var wwwHitsound = new WWW("file://" + Path.Combine(_skinPath, "se-hit.wav"));
        _hitsound = wwwHitsound.audioClip;
        var wwwSwipesound = new WWW("file://" + Path.Combine(_skinPath, "se-swipe.wav"));
        _swipesound = wwwSwipesound.audioClip;
        var www = new WWW("file://" + Path.Combine(bm.Info.Path, bm.Info.FileName + ".wav"));
        _audioClip = www.audioClip;
        _shallStartPlay = true;
    }
}
