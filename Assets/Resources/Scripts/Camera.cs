using UnityEngine;
using System.Collections;

public class Camera : MonoBehaviour {

	public Transform target;	// target that the camera should follow (e.g. the player)
	public float maxXPos = 270;
	public float smoothFactor = 0.6f;
	public float directionBias = 2;  // number of units that the camera should go further to the right than the player

	//private Vector3 offset;

	// Use this for initialization
	void Start () {
		//offset = transform.position - target.position;
		//offset.Set(transform.position.x - target.position.x, 0, 0);  // Camera moves only in x direction
	}
	
	// FixedUpdate is called once per fixed framerate frame
	//void FixedUpdate () {
	void Update () {

		//Vector3 camCurrentPosition = transform.position;

		// Find out to what position the camera should move
		//Vector3 camTargetPosition;
		//camTargetPosition.Set (target.position.x, transform.position.y, transform.position.z);

		// How far should the camera be translated?
		// Linearly interpolate between current and target x value using the smoothing factor
//		float xTranslate = (target.position.x - transform.position.x) * smoothFactor * Time.deltaTime;

		// Translate the camera position
		//transform.position.x = 2;
//		transform.Translate (Vector3.right * xTranslate);

		Vector3 targetCamPosition = new Vector3 (target.position.x + directionBias, transform.position.y, transform.position.z);
		Vector3 newPosition = Vector3.Lerp (transform.position, targetCamPosition, smoothFactor * Time.deltaTime);
		newPosition.x = Mathf.Min(newPosition.x, maxXPos);
		transform.position = newPosition;
	}
}
