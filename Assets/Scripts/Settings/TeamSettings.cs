using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamSettings : MonoBehaviour {
	public int 							teamNum;
	public Sprite						jetpack;
	public Sprite						weapon;
	public LayerMask                    placementMask;
	public LayerMask                    platformsMask;
	public LayerMask 					playerMask;


	public Sprite GetJetpack () {
		return jetpack;
	}

	public Sprite GetWeapon () {
		return weapon;
	}

	public LayerMask GetPlacementMask () {
		return placementMask;
	}

	public LayerMask GetPlatformsMask () {
		return platformsMask;
	}

	public LayerMask GetPlayerMask() {
		return playerMask;
	}
}
