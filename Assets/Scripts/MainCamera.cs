using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        CheckForRestart ();
	}

    void CheckForRestart(){
        if (Input.GetButtonDown ("Restart_P1")) {
            SceneManager.LoadScene ("Main");
        }
    }
}
