using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetpackBar : MonoBehaviour {

    public GameObject                   jetpack_bar;

    public bool                         ____________;
    private float                       max_fuel;


    // Updated when the player uses fuel to adjust the scale of the bar
    // Called by: Player.GetYInputSpeed()
    public void SetFuel(float fuel){
        float scale = fuel / max_fuel;
        SetScale(scale);
    }

    // Sets the max fuel amount so that the bar can scale appropriately
    // Called by: Player.Start()
    public void SetMaxFuel(float in_max_fuel){
        max_fuel = in_max_fuel;
    }

    // Updates the fuel bar to show the current fuel amount
    // Called by: this.SetFuel()
    void SetScale(float scale){
        jetpack_bar.transform.Find("Canvas/FuelValue").transform.localScale = new Vector3 (scale, 1f, 0f);
    }
}
