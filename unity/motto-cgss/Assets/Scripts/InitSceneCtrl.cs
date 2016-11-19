using UnityEngine;
using System.Threading;
using UnityEngine.SceneManagement;

public class InitSceneCtrl : MonoBehaviour {

    private volatile bool _shallGo;

	public void Start () {
        GameManager.LoadBeatmaps();

        new Thread(() =>
        {
            Thread.Sleep(1000);
            _shallGo = true;
        }).Start();
	}
	
	public void Update () {
	    if (_shallGo)
        {
            SceneManager.LoadScene("SelectScene");
        }
	}
}
