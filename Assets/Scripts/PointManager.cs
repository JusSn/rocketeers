/*
 * Author: Cameron Gagnon
 * Created: 3/10
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PointManager : MonoBehaviour {

    // set these values in the inspector
    public Text                         ui_pts_left;
    // the initial amount of points each team starts with
    public int                          starting_pts;

    public bool                         _____________;

    void Start () {
        InitializePoints ();
	}


    // Calling condition: When a player wants to pick up a resource,
    //                    check to see if they can use that resource
    //                    based on how many points their team has
    // Called by: player.TODO()
    public bool UsePoints(int pts_to_be_used){
        // check if we can use the amount of points we want to
        if (CanUsePoints (pts_to_be_used)) {
            // if we can use the points, go ahead and use them
            SubtractPts (pts_to_be_used);
            return true;
        }
        // otherwise return false
        return false;
    }

    void SubtractPts(int pts_to_be_used){
        ui_pts_left.text = (int.Parse(ui_pts_left.text) - pts_to_be_used).ToString();
    }

    bool CanUsePoints(int pts_to_be_used){
        return int.Parse(ui_pts_left.text) - pts_to_be_used > 0;
    }

    void InitializePoints(){
        ui_pts_left.text = (starting_pts).ToString();
    }
}