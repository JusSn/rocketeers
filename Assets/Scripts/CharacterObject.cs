using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "Character")]
public class CharacterSettings : ScriptableObject {
	public Sprite						sprite;
	public RuntimeAnimatorController	animatorController;
	public int 							teamNumber;
}
