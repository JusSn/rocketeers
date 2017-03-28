using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainCamera : MonoBehaviour {
   
    public static MainCamera S;
    public float leftBorder;
    public float rightBorder;

    private float RATIO_MULTIPLIER = 1.875f;

    // JF: Determines how far objects can fall off screen during battle before being destroyed
    private int CAMERA_LOWER_LEEWAY = 3;
    private float battlePhaseSize = 9f;

	// Use this for initialization
	void Start () {
        S = this;
        float dist = (transform.position - Camera.main.transform.position).z;
        leftBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, dist)).x;
        rightBorder = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, dist)).x;
	}
	
	// Update is called once per frame
	void Update () {
        CheckForRestart ();
	}

    void CheckForRestart(){
        if (Input.GetButtonDown ("Restart")) {
            SceneManager.LoadScene ("Main");
        }
    }

    // CG called by the phase manager when the switch to battle phase begins
    public void SwitchToBattlePhase(){
        StartCoroutine ("LerpToBattlePhaseCamera");
    }

    // CG: Lerps the camera to be 1 unit wider than it starts out as
    IEnumerator LerpToBattlePhaseCamera(){
        // wait 5 seconds before starting to lerp the camera. This results in lerping
        // beginning with 5 seconds left in the countdown
        yield return new WaitForSeconds (5f);
        // don't worry about going the entire distance, just some
        while (GetHalfHeightOfCamera() <= (battlePhaseSize - 0.15f)) {
            GetComponent<Camera> ().orthographicSize = Mathf.Lerp(GetHalfHeightOfCamera(),
                    battlePhaseSize, Time.deltaTime * 0.2f);
            yield return null;
        }
    }

    /************************** CAMERA STUFF **************************/
    // IMPORTANT NOTE: These values and calculations are based on a 16:9
    // aspect ratio.

    // returns true when the vector passed in is on screen
    public bool IsOnScreen(Vector3 pos){
        float x_val = pos.x;
        float y_val = pos.y;

        // if the position is out of bounds in the x-axis, return false
        if (x_val < GetWestCameraBound () || x_val > GetEastCameraBound ()) {
            return false;
        }

        // if the position is out of bounds in the y-axis, return false
        if (y_val < GetSouthCameraBound () || y_val > GetNorthCameraBound ()) {
            return false;
        }

        // otherwise, the position is within the screen!
        return true;
    }

    // JF: Gammeplay check to destroy objects and players if they fall too far below the rocket
    public bool IsBelowScreen(Vector3 pos) {
        return (pos.y < GetSouthCameraBound () - CAMERA_LOWER_LEEWAY);
    }

    // returns the west bound of the camera
    float GetWestCameraBound(){
        return -GetHalfWidthOfCamera ();
    }

    // returns the east bound of the camera
    float GetEastCameraBound(){
        return GetHalfWidthOfCamera ();
    }

    // returns the north bound of the camera
    float GetNorthCameraBound(){
        return GetHalfHeightOfCamera ();
    }

    // returns the south bound of the camera
    float GetSouthCameraBound(){
        return -GetHalfHeightOfCamera ();
    }

    // returns the width of the camera view
    float GetHalfWidthOfCamera(){
        float height = GetHalfHeightOfCamera ();
        return height * RATIO_MULTIPLIER;
    }

    float GetHalfHeightOfCamera(){
        return GetComponent<Camera> ().orthographicSize;
    }
}
