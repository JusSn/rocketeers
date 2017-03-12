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

    // used to control how long the main points text blinks red and black
    private float                       blink_length = 2f;

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
        NotEnoughPoints ();
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

    // starts a coroutine that flashes the main points text red and black
    public void NotEnoughPoints(){
        StartCoroutine(FlashPoints());
    }

    // flashes the main points text from red to black for blink_length
    IEnumerator FlashPoints(){
        float start_time = Time.time;
        while (Time.time - start_time < blink_length) {
            // sets the text to be red for 0.5 seconds
            ui_pts_left.color = Color.red;
            yield return new WaitForSeconds (0.25f);

            // sets the text to be black for 0.5 seconds
            ui_pts_left.color = Color.black;
            yield return new WaitForSeconds (0.25f);
        }
    }
}