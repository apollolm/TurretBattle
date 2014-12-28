using UnityEngine;
using System.Collections;

public class TurretController : MonoBehaviour {

	public float RotationSpeed = 10.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void FixedUpdate()
	{
		UpdateFunction();		
	}
	
	void UpdateFunction()
	{        
		Quaternion AddRot = Quaternion.identity;
		float y = 0;
		float pitch = 0;
		float yaw = 0;

//		if (Input.GetAxis ("Roll") > 0) {
//			Debug.Log ("Roll:  " + Input.GetAxis ("Roll"));
//		}
		//if (Input.GetAxis ("Roll") > -0.01 && Input.GetAxis ("Roll") < 0.01) {
		y = Input.GetAxis("Roll") * (Time.deltaTime * RotationSpeed);
		//}
		
//		if (Input.GetAxis ("Pitch") > 0) {
//			Debug.Log ("Pitch:  " + Input.GetAxis ("Pitch"));
//		}
		//if (Input.GetAxis ("Pitch") > -0.01 && Input.GetAxis ("Pitch") < 0.01) {
		pitch = Input.GetAxis("Pitch") * (Time.deltaTime * RotationSpeed);

		
//		if(Input.GetAxis ("Yaw") > 0){
//			Debug.Log ("Yaw:  " + Input.GetAxis ("Yaw"));
//		}
//
//		yaw = Input.GetAxis("Yaw") * (Time.deltaTime * RotationSpeed);


		
		//scoreText = "Roll: " + Input.GetAxis ("Roll") + ", Pitch: " + Input.GetAxis ("Pitch") + ", Yaw: " + Input.GetAxis ("Yaw") + ", Throttle: " + throttle;
		
		AddRot.eulerAngles = new Vector3(0, y, pitch);
		rigidbody.rotation *= AddRot;
	}
}
