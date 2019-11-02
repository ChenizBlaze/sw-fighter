using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Surfer : MonoBehaviour {

	public float speed = 4f;
	public float timeBetweenAttacks = 0.5f;
	public int activateRange = 20;

	private float attackTimer;
	private GameObject player;
	public bool isActive = false;
	private bool isDead = false;
	private bool isDissolving = false;
	private bool isClosingIn = false;
	private bool isStumbling = false;
	public bool playerInRange;

	public AudioClip punchAudio;
	public List<AudioClip> hurtAudio;
	public AudioClip impactAudio;
	private AudioSource audioSource = null;

	private Animator animator;  // Use to set parameters or triggers for animations

	// Use this for initialization
	void Start () {
		attackTimer = 0f;

		player = GameObject.FindGameObjectWithTag ("Player");
		//Debug.Log (string.Format ("Player, Object: {0}, Tag: {1}", player.name, player.tag));

		animator = GetComponent<Animator>();

		audioSource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {

		if (!isActive){
			if (Mathf.Abs(player.transform.position.x - transform.position.x) < activateRange){
				isActive = true;
			}
		}
		else if (isDissolving){
			SpriteRenderer sprite = GetComponent<SpriteRenderer>();
			sprite.color = new Color(1f, 1f, 1f, sprite.color.a*(1f-Time.deltaTime));
		}
		else if (isDead){
			// do nothing
		}
		else if(isStumbling){
			Stumble();
		}
		// Is the player in attacking range?
		else if (playerInRange){
			// Stop walking or stumbling
			animator.SetBool("IsStumbling", false);
			animator.SetBool("IsWalking", false);
			// Can we attack? Check attackTimer

			if (attackTimer > timeBetweenAttacks){
				Attack();
			}
			else {
				// do nothing
			}
		}
		else{
			// Move towards player
			if (attackTimer > timeBetweenAttacks){
				Move();
			}
		}
		attackTimer += Time.deltaTime;
	}

	void Attack(){
		turnTowardsPlayer();
		animator.SetTrigger("Attack");
		attackTimer = 0f;
	}

	void Move(){
		// Always turn towards the player
		turnTowardsPlayer();
		
		// Start animation and stop stumbling
		animator.SetBool("IsStumbling", false);
		animator.SetBool("IsWalking", true);
		
		// Randomly set the point int time when the enemy closes in to the player
		if (Mathf.Abs(player.transform.position.x - transform.position.x) > 10){
			// Never close in when too far away
			isClosingIn = false;
		}
		else if (!isClosingIn){
			// This should spread the point when enemies close in over 1 second
			if (Random.value < Time.deltaTime){
				isClosingIn = true;
			}
		}
		
		// targetVector: direction in which the enemy is going to move
		Vector3 targetVector = new Vector3(0f, 0f, 0f);
		
		// If closing in, move towards the player, otherwise move only along x axis
		if (isClosingIn){
			targetVector.Set(player.transform.position.x - transform.position.x, 0f,
			                 player.transform.position.z - transform.position.z);
		}
		else{
			targetVector.Set(player.transform.position.x - transform.position.x, 0f, 0f);
		}
		
		Vector3 movement = targetVector;
		
		// Normalize movement and move with double speed up/down, looks and feels better
		movement = movement.normalized;
		movement.z *= 2;
		movement = movement * speed * Time.deltaTime;
		
		transform.position += movement;
	}
	
	void Stumble(){
		turnTowardsPlayer();
		animator.SetBool("IsStumbling", true);

		// Move a little away from the player
		Vector3 directionVector = new Vector3(transform.position.x - player.transform.position.x, 0f,
		                              transform.position.z - player.transform.position.z);
		directionVector = directionVector.normalized;
		directionVector = directionVector * (speed/2) * Time.deltaTime;
		transform.position += directionVector;
	}

	void OnTriggerEnter (Collider other)
	{
		//Debug.Log (string.Format ("Surfer OnTriggerEnter(), Object: {0}, Tag: {1}", other.gameObject.name, other.gameObject.tag));
		if(other.gameObject == player)
		{
			playerInRange = true;
		}
	}
	
	
	void OnTriggerExit (Collider other)
	{
		if(other.gameObject == player)
		{
			playerInRange = false;
		}
	}

	void OnCollisionEnter(Collision collision){
		if(collision.gameObject.tag == "Player")
		{
			//isStumbling = true;
		}
	}
	
	void OnCollisionExit(Collision collision){
		if(collision.gameObject.tag == "Player")
		{
			isStumbling = false;
		}
	}
	
	void turnTowardsPlayer(){
		Vector3 tempVector = transform.localScale;
		if(transform.position.x < player.transform.position.x){
			tempVector.x = 1;
		}
		else{
			tempVector.x = -1;
		}
		transform.localScale = tempVector;
	}

	public void hitByPlayer(){
		if (!isDead){
			// Enemy is killed
			isDead = true;

			// Set the animation
			animator.SetBool("IsWalking", false);
			animator.SetBool("IsStumbling", false);
			animator.SetTrigger("Hit");

			playAudio ("hurt");

			// Make enemy intangible
			CapsuleCollider[] colliders = GetComponents<CapsuleCollider>();
			foreach (CapsuleCollider collider in colliders){
				collider.isTrigger = true;
			}

			// Throw enemy back by the hit
			int direction = player.transform.position.x - transform.position.x > 0 ? -1 : 1;
			Rigidbody rigidbody = GetComponent<Rigidbody>();
			rigidbody.drag = 1;
			// Unfreeze position Y and keep all othe constraint flags
			rigidbody.velocity = new Vector3(direction * 10,0,0);

			// Call startDissolving() in 0.5 seconds
			Invoke ("startDissolving",0.5f);
		}
	}

	public void startDissolving(){
		isDissolving = true;
		Destroy (gameObject, 1f);
	}

	public void playAudio(string clip){
		if (clip == "punch"){
			audioSource.clip = punchAudio;
			audioSource.Play ();
		}
		else if (clip == "hurt"){
			// select random hurt clip
			audioSource.clip = hurtAudio[(int) Random.Range(0,hurtAudio.Count-1)];
			audioSource.Play ();
		}
		else if (clip == "impact"){
			audioSource.clip = impactAudio;
			audioSource.Play ();
		}
	}

}
