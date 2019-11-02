using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelControl : MonoBehaviour {

	public GameObject player;
    public GameObject canvas;

    private Text finishText;

    void Awake () {
		finishText = canvas.GetComponentInChildren<Text>();
    }

	// Update is called once per frame
	void Update () {
		if (player.transform.position.x > 280){
			LevelComplete();
		}
	}

	void LevelComplete (){
		finishText.enabled = true;
        Invoke("ExitGame", 4);
    }

	void ExitGame() {
		Application.Quit();
	}

}
