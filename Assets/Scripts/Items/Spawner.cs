// Justin Fan
// To be attached to Spawner GameObject
// Spawns items (blocks and weapons) in a set direction
// Features item pooling to recycle items that travel off screen

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Spawner : MonoBehaviour {
	// Items to be spawned
	public GameObject[] itemPrefabs;
	// Likelihood of each item to be spawned, respectively
	// Probability = weight/total weight of all objects in spawnerScript
	public float[] weights;
	// Direction for spawned objects to travel in
	public Vector3 outputDir;
	// Maximum number of items in pool
	public int maxCount;
	public float spawnInt;
	public float repoolTime;

	public bool __________________;

	public bool on = true;

	// Stack of itemPrefabs in pool	
	Stack<GameObject> poolStack;

	// For returning a weighted random
	private float totalWeight;

	// Use this for initialization
	void Start () {
		// Pool instances of itemPrefab
		poolStack = new Stack<GameObject> ();

		// Sum up weights 
		totalWeight = 0;
		foreach (float f in weights) {
			totalWeight += f;
		}

		// Crash if number of weights provided does not match number of itemPrefabs
		Assert.AreEqual(itemPrefabs.Length, weights.Length);

		for (int i = 0; i < maxCount; ++i) {
			// Choose item to be spawned
			int idx = weightedRandomIndex ();
			GameObject item = Instantiate (itemPrefabs[idx], transform.position, Quaternion.identity) as GameObject;

			item.SetActive(false);
			item.GetComponent<Item> ().spawnerScript = this;
			poolStack.Push(item);
		}
		// Invoke repeating spawns of prefab
		InvokeRepeating ("SpawnItem", 0f, spawnInt);
	}
	
	// Update is called once per frame
	void Update () {
		// Not much to do here?
	}

	// Use: Spawns an item from this spawner with velocity and rate set in editor
	void SpawnItem () {
		if (poolStack.Count == 0 || !on) {
			return;
		}

		GameObject item = poolStack.Pop();

		item.SetActive(true);
		item.GetComponent<Item> ().ScheduleRepool(repoolTime);
		item.transform.position = transform.position;
		item.GetComponent<Rigidbody2D> ().velocity = outputDir;
		item.GetComponent<Rigidbody2D> ().bodyType = RigidbodyType2D.Kinematic;
	}

	// Use: Resets GameObject and adds it to this spawner's pool stack for reuse.
	// Called by: Object that destroys or removes the item.
	public void Repool (GameObject go) {
		go.SetActive(false);
		poolStack.Push(go);
	}


	// Returns a random index to choose which item to pool
	// Based on weights provided in Inspector
	private int weightedRandomIndex () {
		float f = Random.Range (0, totalWeight);

		float counter = 0;
		for (int i = 0; i < weights.Length; ++i) {
			counter += weights[i];

			if (counter > f) {
				return i;
			}
		}

		return weights.Length - 1;
	}
}
