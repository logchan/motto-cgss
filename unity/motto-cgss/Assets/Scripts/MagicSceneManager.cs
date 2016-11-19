using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class MagicSceneManager : MonoBehaviour {

    private GameObject _imgObject;
    private float _t;
    private readonly float _v = 0.01f;

    private void SetObjectPos(GameObject obj, float x, float y)
    {
        obj.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(x, y, 10.0f));
    }

    public void Start () {
	    var skinPath = Path.Combine(Path.Combine(GameManager.DataPath, "Skins"), "Default");
        var btnTexture = UnityHelper.TextureFromFile(Path.Combine(skinPath, "touchpad.png"), SceneSettings.SpriteSize);

        var texture = UnityHelper.SpriteFromTexture(btnTexture);

        var obj = new GameObject();
        SetObjectPos(obj, 0, 0);

        SpriteRenderer objRenderer = obj.AddComponent<SpriteRenderer>();
        objRenderer.sprite = texture;
        _imgObject = obj;
    }
	
	public void Update ()
	{
        _t += _v;
	    if (_t > 1.0f)
	        _t = 0;

	    SetObjectPos(_imgObject, _t*Screen.width, _t*Screen.height);

	    if (Input.touchCount > 0 || Input.anyKey)
	    {
	        SceneManager.LoadScene("SelectScene");
	    }
	}
}
