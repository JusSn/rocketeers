using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreHealth : Health {

    public GameObject            largeHealthBar;

    Transform                    largeGreenBG;


    protected override void Start(){
        base.Start ();
        if (PhaseManager.S.in_tutorial) {
            largeGreenBG = largeHealthBar.transform.Find ("GreenBackground");
        } else {
            largeGreenBG = largeHealthBar.transform.Find ("Canvas/GreenBackground");
        }
        largeHealthBar.SetActive (false);
    }

    public void DisplayHealthBars(){
        largeHealthBar.SetActive (true);
    }

    protected override void FlashHealthBar(){
        base.FlashHealthBar ();
        largeGreenBG.transform.localScale = new Vector3 (Mathf.Max(cur_health/MAX_HEALTH, 0f), 1f, 0f);
    }
}
