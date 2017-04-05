using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum UFOForm {
    FlyingTowards,
    Towing,
    FlyingAway,
}

public class UFOManager : MonoBehaviour {

    public GameObject UFOLakitu;

    public void SetPlayerToRespawn(Player player){
        GameObject ufo = Instantiate<GameObject> (UFOLakitu);
        ufo.GetComponent<UFO>().SetPlayer (player);
    }
}