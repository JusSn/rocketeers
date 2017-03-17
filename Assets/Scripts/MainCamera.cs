using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainCamera : MonoBehaviour {
   
    public static MainCamera S;
    public float leftBorder;
    public float rightBorder;

    private float RATIO_MULTIPLIER = 1.875f;

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
