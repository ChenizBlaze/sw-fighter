using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {

	enum ComboButton { Up, Down, Left, Right, Fire1 };
	enum Combo { None, Snap, SpeedUp, SpeedDown, SpeedLeft, SpeedRight };
	enum Direction { Up, Down, Left, Right };

	public float speed = 4f;
	public float comboTiming = 1.0f; // time in seconds to hit the next button for a combo
	public float speedMoveDistance = 12;
    public float speedMoveTrailTime = 0.4f;
    public float minXPos = 20;
	public float maxXPos = 285;
	public float minYPos = 28;
	public float maxYPos = 48;
	public AudioClip snapAudio;
	public AudioClip slapAudio;
	public AudioClip swooshAudio;
	private AudioSource audioSource;

	private SpriteRenderer spriteRenderer;
	private Rigidbody playerRigidbody;
	private Animator animator;
	private Vector3 movement;

	private Combo combo = Combo.None;
	private float attackCoolDown = 0f;
    private float trailTimer = 0f;

    private float comboTimer;
	private List<ComboButton> buttonSequence; // List of the last buttons pressed, each within timing

    private ParticleSystem trailParticleSystemHorizontal;
    private ParticleSystem trailParticleSystemVertical;


    // Use this for initialization
    void Start () {
		spriteRenderer = GetComponent<SpriteRenderer>();
		animator = GetComponent<Animator>();
		playerRigidbody = GetComponent<Rigidbody>();
		audioSource = GetComponent<AudioSource>();
		buttonSequence = new List<ComboButton> ();
		comboTimer = 0f;

        ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();
        foreach(ParticleSystem particleSystem in particleSystems) {
            if (particleSystem.name == "TrailParticleSystemHorizontal") {
                trailParticleSystemHorizontal = particleSystem;
            }
            else if (particleSystem.name == "TrailParticleSystemVertical") {
                trailParticleSystemVertical = particleSystem;
            }
            else
            {
                throw new System.Exception("particle system not found");
            }
        }
    }

    void Update() {
		/*
		 * Control: up, dpwn, left, right, fire1
		 * fire1: slap
		 * combo left+right, fire1: snap
		 */

		if (attackCoolDown > 0){
			attackCoolDown = Mathf.Max(0, attackCoolDown - Time.deltaTime);
		}
        trailTimer = Mathf.Max(0, trailTimer - Time.deltaTime);
        if (trailTimer == 0)
        {
            trailParticleSystemHorizontal.Stop();
            trailParticleSystemVertical.Stop();
        }

        // Update combos
        ComboCheck ();

		// We can attack only after cool down)
		if (attackCoolDown == 0){
			if (combo == Combo.Snap) {
				attackSnap();
				combo = Combo.None;
			}
			else if (combo == Combo.SpeedLeft){
				SpeedMove (Direction.Left);
				combo = Combo.None;
			}
			else if (combo == Combo.SpeedRight){
				SpeedMove (Direction.Right);
				combo = Combo.None;
			}
			else if (combo == Combo.SpeedUp){
				SpeedMove (Direction.Up);
				combo = Combo.None;
			}
			else if (combo == Combo.SpeedDown){
				SpeedMove (Direction.Down);
				combo = Combo.None;
			}
			else if (Input.GetButton("Fire1") && Input.GetAxisRaw ("Horizontal") != 0) {
				attackSlap();
			}
			else if (Input.GetButton("Fire1") && Input.GetAxisRaw ("Horizontal") == 0
			         && Input.GetAxisRaw ("Vertical") == 0){
				attackSlapUp();
			}
			else if (Input.GetAxisRaw ("Horizontal") != 0 || Input.GetAxisRaw ("Vertical") != 0) {
				Walk ();
			}
			else if (Input.GetAxisRaw ("Horizontal") == 0 && Input.GetAxisRaw ("Vertical") == 0
			         && !Input.GetButton("Fire1")){
				Idle();
			}
		}
	}

	private void Idle(){
		clearAnimatorBools();
		animator.SetBool("Standing", true);
	}

	private void Walk(){
		clearAnimatorBools();
		animator.SetBool("Walking", true);
		Move();
	}

	private void attackSlap(){
		clearAnimatorBools();
		animator.SetBool("AttackingSlap", true);
		Move();
		// enemyHitSlap() is called from the animation at the corresponding frames
	}
	
	private void attackSlapUp(){
		attackCoolDown = 1.2f;
		clearAnimatorBools();
		animator.SetBool("AttackingSlapUp", true);
		// enemyHitSlap() is called from the animation at the corresponding frames
	}
	
	private void attackSnap(){
		attackCoolDown = 1.6f;
		clearAnimatorBools();
		animator.SetBool("AttackingSnap", true);
		// enemyHitSnap() is called from the animation at the corresponding frames
	}
	
	private void enemyHitSlap(){
		GameObject colliderObject = transform.Find("SlapAttackCollider").gameObject;
		List<GameObject> enemiesList = colliderObject.GetComponent<AttackCollider>().enemies;
		foreach (GameObject enemy in enemiesList){
			// check if enemy was not killed before, somehow
			if (enemy != null){
				playAudio("slap");
				enemy.GetComponent<Surfer>().hitByPlayer();
			}
		}
	}
	
	private void enemyHitSnap(){
		GameObject colliderObject = transform.Find("SnapAttackCollider").gameObject;
		List<GameObject> enemiesList = colliderObject.GetComponent<AttackCollider>().enemies;
		playAudio("snap");
		foreach (GameObject enemy in enemiesList){
			// check if enemy was not killed before, somehow
			if (enemy != null){
				enemy.GetComponent<Surfer>().hitByPlayer();
			}
		}
	}
	
	private void Move(){
		// Get input
		float horizontal = Input.GetAxisRaw("Horizontal");
		float vertical = Input.GetAxisRaw("Vertical");
		
		// if player is moving horizontally ensure correct orientation of sprite
		if (horizontal != 0) {
			spriteRenderer.flipX = (horizontal > 0) ? false : true;
		}
		
		// Move player
		movement.Set (horizontal, 0f, vertical);
		movement = movement.normalized;
		movement.z *= 2;
		movement = movement * speed * Time.deltaTime;
		Vector3 targetPosition = transform.position + movement;

		// enforce bounds
		targetPosition.x = Mathf.Max (targetPosition.x, minXPos);
		targetPosition.x = Mathf.Min (targetPosition.x, maxXPos);

		playerRigidbody.MovePosition (targetPosition);
	}

	private void SpeedMove(Direction direction){
        trailTimer = speedMoveTrailTime;

        // direction == -1 for movement to the left and 1 for right
        Vector3 targetPosition = transform.position;
		if (direction == Direction.Up){
            trailParticleSystemVertical.Play();
			targetPosition.z = Mathf.Min (targetPosition.z + speedMoveDistance, maxYPos);
		}
		else if (direction == Direction.Down){
            trailParticleSystemVertical.Play();
            targetPosition.z = Mathf.Max (targetPosition.z - speedMoveDistance, minYPos);
		}
		else if (direction == Direction.Left){
            trailParticleSystemHorizontal.Play();
			targetPosition.x = Mathf.Max (targetPosition.x - speedMoveDistance, minXPos);
		}
		else if (direction == Direction.Right){
            trailParticleSystemHorizontal.Play();
            targetPosition.x = Mathf.Min (targetPosition.x + speedMoveDistance, maxXPos);
		}

		RaycastHit[] hits = Physics.RaycastAll(playerRigidbody.position, targetPosition - playerRigidbody.position, speedMoveDistance);

		playerRigidbody.MovePosition(targetPosition);

		foreach (RaycastHit hit in hits){
			if (hit.transform.tag == "Enemy"){
				hit.transform.GetComponent<Surfer>().hitByPlayer();
			}
		}
	}

	private void clearAnimatorBools(){
		animator.SetBool("Standing", false);
		animator.SetBool("Walking", false);
		animator.SetBool("AttackingSlap", false);
		animator.SetBool("AttackingSlapUp", false);
		animator.SetBool("AttackingSnap", false);
	}

	void ComboCheck () {
		// Update timer and check if the timer has elapsed
		comboTimer += Time.deltaTime;
		if (comboTimer > comboTiming) {
			buttonSequence.Clear();
			comboTimer = 0f;
		}
		
		// Check if more than one button is pressed
		//...
		
		// Check if some button is pressed and add to beginning of list
		if (Input.GetButtonDown("Fire1")) {
			buttonSequence.Insert(0, ComboButton.Fire1);
			comboTimer = 0;
		}
		else if (Input.GetButtonDown("Vertical") && Input.GetAxisRaw("Vertical") > 0) {
			buttonSequence.Insert(0, ComboButton.Up);
			comboTimer = 0;
		}
		else if (Input.GetButtonDown("Vertical") && Input.GetAxisRaw("Vertical") < 0) {
			buttonSequence.Insert(0, ComboButton.Down);
			comboTimer = 0;
		}
		else if (Input.GetButtonDown("Horizontal") && Input.GetAxisRaw("Horizontal") < 0) {
			buttonSequence.Insert(0, ComboButton.Left);
			comboTimer = 0;
		}
		else if (Input.GetButtonDown("Horizontal") && Input.GetAxisRaw("Horizontal") > 0) {
			buttonSequence.Insert(0, ComboButton.Right);
			comboTimer = 0;
		}
		
		ComboButton[] attackSnapComboSequence = new ComboButton[] {ComboButton.Fire1, ComboButton.Right, ComboButton.Left};
		ComboButton[] speedMoveLeftSequence = new ComboButton[] {ComboButton.Left, ComboButton.Left};
		ComboButton[] speedMoveRightSequence = new ComboButton[] {ComboButton.Right, ComboButton.Right};
		ComboButton[] speedMoveUpSequence = new ComboButton[] {ComboButton.Up, ComboButton.Up};
		ComboButton[] speedMoveDownSequence = new ComboButton[] {ComboButton.Down, ComboButton.Down};
		// Detect and activate combo
		if (ComboComplete(buttonSequence.ToArray(), attackSnapComboSequence)){
			combo = Combo.Snap;
			buttonSequence.Clear();
		}
		if (ComboComplete(buttonSequence.ToArray(), speedMoveUpSequence)){
			combo = Combo.SpeedUp;
			buttonSequence.Clear();
		}
		if (ComboComplete(buttonSequence.ToArray(), speedMoveDownSequence)){
			combo = Combo.SpeedDown;
			buttonSequence.Clear();
		}
		if (ComboComplete(buttonSequence.ToArray(), speedMoveLeftSequence)){
			combo = Combo.SpeedLeft;
			buttonSequence.Clear();
		}
		if (ComboComplete(buttonSequence.ToArray(), speedMoveRightSequence)){
			combo = Combo.SpeedRight;
			buttonSequence.Clear();
		}

		//Keep only the last five buttons
		if (buttonSequence.Count == 6){
			buttonSequence.RemoveAt (5);
		}
	}
	
	private bool ComboComplete(ComboButton[] sequence, ComboButton[] combo){
		if (sequence.Length < combo.Length){
			return false;}
		else {
			for (int i = 0; i<combo.Length; i++){
				if (sequence[i] != combo[i]) {
					return false;
				}
			}
		}
		return true;
	}

	public void playAudio(string clip){
		if (clip == "snap"){
			audioSource.clip = snapAudio;
			audioSource.Play ();
		}
		else if (clip == "slap"){
			audioSource.clip = slapAudio;
			audioSource.Play ();
		}
		else if (clip == "swoosh"){
			audioSource.clip = swooshAudio;
			audioSource.Play ();
		}
	}
}
