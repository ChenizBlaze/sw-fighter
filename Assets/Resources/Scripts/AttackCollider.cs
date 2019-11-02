using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackCollider : MonoBehaviour {

	public int enemiesCount;
	public List<GameObject> enemies;

	// Use this for initialization
	void Start () {
		enemies = new List<GameObject> ();
		enemiesCount = enemies.Count;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter (Collider other){
		if (other.gameObject.tag == "Enemy" && !enemies.Contains(other.gameObject)){
			enemies.Add(other.gameObject);
		}
		enemiesCount = enemies.Count;
	}

	void OnTriggerExit (Collider other) {
		if (enemies.Contains (other.gameObject)){
			enemies.Remove (other.gameObject);
		}
		enemiesCount = enemies.Count;
	}
}
