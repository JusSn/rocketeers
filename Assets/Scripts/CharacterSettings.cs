using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Character {
	Yellow,
	Pink,
	Green,
	Blue
}

public class CharacterSettings : MonoBehaviour {
	public Character					character;
	public RuntimeAnimatorController	animator;
	public Sprite						charSprite;

	public Character GetCharacterType () {
		return character;
	}

	public RuntimeAnimatorController GetAnimator () {
		return animator;
	}	

	public Sprite GetSprite () {
		return charSprite;
	}
}
