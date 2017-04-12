using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using InControl;

public class MainCamera : MonoBehaviour {
   
    public static MainCamera S;

    private float RATIO_MULTIPLIER = 1.875f;

    //SK: camera zoom variables
    private float camera_buffer;
    private float desiredCamSize;
    public float zoomSpeed;

    // JF: Determines how far objects can fall off screen during battle before being destroyed
    private int CAMERA_LOWER_LEEWAY = 5;
    // CG: When the camera is fully zoomed out this is a point where the lower edge cannot be seen
    private float ABSOLUTE_LOWER_BOUND = -23.5f;
    private float minFOV = 8.5f;
    private float maxFOV = 18f;
    private float battlePhaseSize = 9f;

	// Use this for initialization

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        S = this;
    }
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        CheckForRestart ();
        if (!PhaseManager.S.gameOver) {
            CalculateCamSize ();
        }
	}

    private void LateUpdate() {
        if (!PhaseManager.S.inBuildPhase && !PhaseManager.S.gameOver) {
            float newSize = Mathf.Lerp(Camera.main.orthographicSize, desiredCamSize, Time.deltaTime * zoomSpeed);
            Camera.main.orthographicSize = newSize;
        }
    }

    void CheckForRestart(){
		if (Input.GetButtonDown("Back")) {
            SceneManager.LoadScene ("menu");
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

    // JF: Gameplay check to destroy objects and players if they fall too far below the rocket
    public bool IsBelowScreen(Vector3 pos) {
        return (pos.y < GetSouthCameraBound () - CAMERA_LOWER_LEEWAY);
    }

    public bool IsBelowAbsoluteScreen(Vector3 pos){
        return pos.y < ABSOLUTE_LOWER_BOUND;
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

    // SK: Checks if ships are within the camera viewport (with a buffer) and adjusts
    //     desiredCamSize in order to keep them on screen
    void CalculateCamSize() {
        camera_buffer = Camera.main.orthographicSize * .3f;

        float leftBorder = GetWestCameraBound();
        float rightBorder = GetEastCameraBound();
        float topBorder = GetNorthCameraBound();
        float bottomBorder = GetSouthCameraBound();

        Vector3 pos0 = PhaseManager.S.cores[0].transform.position;
        Vector3 pos1 = PhaseManager.S.cores[1].transform.position;

        if(pos0.x < leftBorder + camera_buffer || rightBorder - camera_buffer < pos0.x ||
           pos0.y < bottomBorder + camera_buffer || topBorder - camera_buffer < pos0.y ||
           pos1.x < leftBorder + camera_buffer || rightBorder - camera_buffer < pos1.x ||
           pos1.y < bottomBorder + camera_buffer || topBorder - camera_buffer < pos1.y) {
            //Debug.Log("Moving offscreen");
            desiredCamSize = Camera.main.orthographicSize + .75f;
        }
        else if(pos0.x > leftBorder + camera_buffer * 2 && rightBorder - (camera_buffer * 2) > pos0.x &&
                pos0.y > bottomBorder + camera_buffer * 2 && topBorder - (camera_buffer * 2) > pos0.y ||
                pos1.x > leftBorder + camera_buffer * 2 && rightBorder - (camera_buffer * 2) > pos1.x &&
                pos1.y > bottomBorder + camera_buffer * 2 && topBorder - (camera_buffer * 2) > pos1.y) {
            //Debug.Log("Moving back onscreen");
            desiredCamSize = Camera.main.orthographicSize - .5f;
        }
        
        desiredCamSize = Mathf.Clamp(desiredCamSize, minFOV, maxFOV);
    }
}
