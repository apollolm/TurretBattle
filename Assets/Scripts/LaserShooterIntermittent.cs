using UnityEngine;
using System.Collections;

public class LaserShooterIntermittent : MonoBehaviour
{

		LineRenderer line;
		public Light light;
		public float explosionPower = 100f;
		public float explosionRadius = 10f;
		public float explosionUpForce = 0.1f;

		private float perShotDelay = .2f;
		private float timestamp = 0.0f;
		private float shotDuration = 0.3f;

	public float timeDelay = 0.5f;


		// Use this for initialization
		void Start ()
		{
				line = gameObject.GetComponent<LineRenderer> ();
				line.enabled = false;
				light.enabled = false;
				timestamp = + timeDelay;

		        
		}
	
		// Update is called once per frame
		void Update ()
		{
				if (Input.GetButton ("Fire1") && Time.time > timestamp) {
						Debug.Log ("Firing " + Time.time.ToString ());
				
	
						if (line.enabled == false) {
								//timestamp = Time.time + perShotDelay;
								//StopCoroutine("FireLaser");
								StartCoroutine ("FireLaser");
						}
			    
				} else {
						// StopCoroutine("FireLaser");
						//TurnOffLaser ();
				}

				if (Input.GetButtonUp ("Fire1")) {
						StopCoroutine ("FireLaser");
						TurnOffLaser ();
						timestamp = Time.time + timeDelay;
				}
		}

		void DestroyTarget (RaycastHit target)
		{
				//hit.rigidbody.AddForceAtPosition (transform.forward * 5, hit.point);
				if (target.transform.childCount == 0 && target.rigidbody) {
						target.transform.rigidbody.AddExplosionForce (explosionPower, target.transform.position + Random.insideUnitSphere * explosionRadius, explosionRadius, explosionUpForce, ForceMode.Impulse);
				}
		
				foreach (Transform child in target.transform) {
			//Remove from parent object.
			child.transform.parent = null;
						if (child.gameObject) {
								Rigidbody gameObjectsRigidBody = child.gameObject.AddComponent<Rigidbody> (); // Add the rigidbody.
								if (gameObjectsRigidBody) {
										gameObjectsRigidBody.mass = 5; 
										gameObjectsRigidBody.useGravity = false;
								}
						}



						child.transform.rigidbody.useGravity = false;
						child.transform.rigidbody.AddExplosionForce (explosionPower, child.transform.position + Random.insideUnitSphere * explosionRadius, explosionRadius, explosionUpForce, ForceMode.Impulse);
						child.transform.rotation = Random.rotation;
				}

				Destroy(target.transform.gameObject);
		        
				//collider.enabled = false;
		}

		IEnumerator FireLaser ()
		{
				line.enabled = true;
				light.enabled = true;

				timestamp += shotDuration;

				while (Input.GetButton("Fire1") && Time.time < (timestamp)) {

						line.renderer.material.mainTextureOffset = new Vector2 (0, Time.time);
	
						Ray ray = new Ray (transform.position, transform.forward);
						RaycastHit hit;
	
						//Set the start point (0) of the line
						line.SetPosition (0, ray.origin);
	
						//If we hit something within 100 units, let us know what we hit
						if (Physics.Raycast (ray, out hit, 100)) {

								//Stop the laser at the point it hit, so it doesn't go thru walls
								line.SetPosition (1, hit.point);

								Collider[] colliders = Physics.OverlapSphere (hit.point, explosionRadius);
								foreach (Collider c in colliders) {
									
										DestroyTarget (hit);
									
								}
				
				
				
						} else {
								//Not hit, make laser be 100 units long.
								line.SetPosition (1, ray.GetPoint (100));
						}
	
						yield return null;
				}

				timestamp = Time.time + perShotDelay + timeDelay;
				line.enabled = false;
				light.enabled = false;
	}


		void TurnOffLaser ()
		{

				line.enabled = false;
				light.enabled = false;
		}
}



