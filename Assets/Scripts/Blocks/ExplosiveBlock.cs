using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveBlock : Block {

    public GameObject                   smoke_plume;
	private float                       EXPLOSION_RADIUS = 2f;

    private float                       SMOKE_SCALE = 1.2f;

	/// <summary>
	/// Sent when an incoming collider makes contact with this object's
	/// collider (2D physics only).
	/// </summary>
	/// <param name="other">The Collision2D data associated with this collision.</param>

	// On contact with enemy block, explode this block, the block it contacts, and deal lesser area damage to surrounding enemy blocks
	protected override void OnCollisionEnter2D(Collision2D other)
	{
        if (!gameObject) {
            return;
        }
		LayerMask enemyBlockMask = LayerMask.NameToLayer("Team" + Utils.GetOppositeTeamNum (teamNum) + "Block");
		if (other.gameObject.layer == enemyBlockMask) {
			// Explode this block
			ExplodeBlock (enemyBlockMask);
		}
	}

	private void ExplodeBlock (LayerMask enemyBlockMask) {
		// JF: EXPLOSIONS!
        if (explosion != null) {
            GameObject boom0 = Instantiate(explosion, transform.position, Quaternion.identity);
            boom0.GetComponent<LoopingAnimation>().StartAnimation();
            GameObject smoke = Instantiate(smoke_plume, transform.position, Quaternion.identity);
            smoke.transform.localScale = Vector3.one * SMOKE_SCALE;
            smoke.GetComponent<LoopingAnimation>().StartAnimation();
            CameraShake.Shake (0.5f, 0.3f);
        }

        int otherteamNum = Utils.GetOppositeTeamNum (teamNum);

        // get all colliders we interfere with
        Collider2D[] blockCols = Physics2D.OverlapCircleAll (transform.position, EXPLOSION_RADIUS);
//            LayerMask.NameToLayer("ImpenetrableToTeam" + otherteamNum) | LayerMask.NameToLayer("Team" + otherteamNum + "Block") | LayerMask.NameToLayer("Team" + otherteamNum + "Platform"));
		foreach (Collider2D col in blockCols) {

			Block blockScript = col.GetComponent<Block> ();
			
            // make sure the layer of the collider is not our own block
            if (col.gameObject && blockScript != null
                && col.gameObject.layer != gameObject.layer
                && col.gameObject.layer != LayerMask.NameToLayer("ImpenetrableToTeam" + teamNum)) {
                blockScript.ExplosionDamage ();
			}
		}
        UnhingeAndFall ();
	}
}
