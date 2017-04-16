using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeInOut : MonoBehaviour {


    public Image                    ui;
    public Text                     text;
    private bool                    fadeDirection;
    private float                   DELTA_ALPHA = 0.02f;
    private float                   DURATION = 1f;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        FadeImage ();
    }


    void FadeImage()
    {
        if (ui.color.a == 0 || ui.color.a >= 1) {
            fadeDirection = !fadeDirection;
        }

        int invert = fadeDirection ? 1 : -1;
        SetAlpha (DELTA_ALPHA * invert);
    }

    void SetAlpha(float a){
        Color tmp_color = ui.color;
        tmp_color.a = Mathf.Clamp01(tmp_color.a + a);
        ui.color = tmp_color;
        text.color = tmp_color;
    }
}
