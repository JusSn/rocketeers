using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowIndicator : MonoBehaviour {


    public GameObject           arrowPrefab;

    public bool                 _____________;
    private bool                showIndicator = false;
    GameObject                  arrow;
    private float               Y_EDGE_OFFSET = 1f;
    private float               X_EDGE_OFFSET = 2f;


	// Use this for initialization
	void Start () {
        arrow = Instantiate (arrowPrefab);
        DisableIndicator ();
	}
	
	// Update is called once per frame
	void Update () {
        DisplayIndicator ();
	}

    void DisplayIndicator(){
        if (!showIndicator || MainCamera.S.IsOnScreen (transform.position)) {
            HideIndicator ();
            return;
        }

        ShowIndicator ();

        Vector3 target_indicator_pos = transform.position;
        Vector3 camera_center = Vector3.up;
        Vector3 dir = (transform.position - camera_center).normalized;

        float x_val = transform.position.x;
        float y_val = transform.position.y;

        if (MainCamera.S.IsWestOfCamera (x_val)) {
            target_indicator_pos.x = MainCamera.S.GetWestCameraBound () + X_EDGE_OFFSET;
        } else if (MainCamera.S.IsEastOfCamera (x_val)) {
            target_indicator_pos.x = MainCamera.S.GetEastCameraBound () - X_EDGE_OFFSET;
        }

        if (MainCamera.S.IsNorthOfCamera (y_val)) {
            target_indicator_pos.y = MainCamera.S.GetNorthCameraBound () - Y_EDGE_OFFSET;
        } else if (MainCamera.S.IsSouthOfCamera (y_val)) {
            target_indicator_pos.y = MainCamera.S.GetSouthCameraBound () + Y_EDGE_OFFSET;
        }

        arrow.gameObject.transform.position = target_indicator_pos;

    }

    void HideIndicator(){
        if (arrow)
            arrow.SetActive (false);
    }

    void ShowIndicator(){
        if (arrow)
            arrow.SetActive (true);
    }

    public void DisableIndicator(){
        if (arrow) {
            HideIndicator ();
            showIndicator = false;
        }
    }

    public void EnableIndicator(){
        if (arrow){
            ShowIndicator ();
            showIndicator = true;
        }  
    }

}
