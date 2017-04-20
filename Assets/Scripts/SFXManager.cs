using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SFX {
	Jump,
	HitPlayer,
	PlayerDied,
	ShootLaser,
	BlockSet,
	BlockHit,
	BlockDestroyed,
	StartPilot,
	StopPilot,
	MenuConfirm,
	NasaCountdown,
	AirHorn,

	SmallExplosion,
	LargeExplosion,
	ShipImpact
}

public class SFXManager : MonoBehaviour {
	private static SFXManager			singleton = null;

	// Menu/UI related
	public AudioClip					menuConfirm;
	public AudioClip					nasaCountdown;

	// Player related
	public AudioClip 					playerJump;
	public AudioClip 					playerHit;
	public AudioClip 					playerDied;

	// Projectile related
	public AudioClip	 				laserShoot;

	// Block related
	public AudioClip 					blockSet;
	public AudioClip 					blockHit;
	public AudioClip 					blockDestroyed;
	public AudioClip 					startPilot;
	public AudioClip					stopPilot;

	public AudioClip 					airHorn;

	public AudioClip 					smallExplosion;
	public AudioClip 					largeExplosion;
	public AudioClip 					shipImpact;
	private AudioSource					source;
	private Dictionary<SFX, AudioClip> 	sfxMap;

	public static SFXManager GetSFXManager() {
		return singleton;
	}

	void Awake() {
		singleton = this;
	}

	// Use this for initialization
	void Start () {
		source = GetComponent<AudioSource> ();

		sfxMap = new Dictionary<SFX, AudioClip> ();
		sfxMap [SFX.MenuConfirm] 	= menuConfirm;
		sfxMap [SFX.NasaCountdown]	= nasaCountdown;
			
		sfxMap [SFX.Jump] 			= playerJump;
		sfxMap [SFX.HitPlayer] 		= playerHit;
		sfxMap [SFX.PlayerDied] 	= playerDied;

		sfxMap [SFX.ShootLaser] 	= laserShoot;

		sfxMap [SFX.BlockSet] 		= blockSet;
		sfxMap [SFX.BlockHit] 		= blockHit;
		sfxMap [SFX.BlockDestroyed] = blockDestroyed;
		sfxMap [SFX.StartPilot] 	= startPilot;
		sfxMap [SFX.StopPilot] 		= stopPilot;

		sfxMap [SFX.AirHorn] 		= airHorn;

		sfxMap [SFX.SmallExplosion] = smallExplosion;
		sfxMap [SFX.LargeExplosion] = largeExplosion;
		sfxMap [SFX.ShipImpact] 	= shipImpact;
 	}
		
    public void PlaySFX(SFX sfx, float volume = 1f) {
        if (singleton) {
            source.PlayOneShot (sfxMap [sfx], volume);
        }
	}
}
