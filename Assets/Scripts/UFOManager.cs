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

    public GameObject               UFOLakitu;

    private Vector3                 SPAWN_POS = new Vector3(0f, 30f, 0f);

    public void SetPlayerToRespawn(Player player, Vector3 pos_to_spawn_UFO){
        InstantiateUFO (player, pos_to_spawn_UFO);
    }

    public void SetPlayerToRespawn(Player player){
        InstantiateUFO (player, SPAWN_POS);
    }

    void InstantiateUFO(Player player, Vector3 ufo_spawn_pos){
        GameObject ufo = Instantiate<GameObject> (UFOLakitu, ufo_spawn_pos, Quaternion.identity);
        ufo.GetComponent<UFO>().SetPlayer (player);
    }
}