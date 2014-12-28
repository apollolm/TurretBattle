using UnityEngine;
using System.Collections;

//This script should aim a Quad towrads a camera, and auto size the Quad based on the distance to the camera.

public class ReticleOVR : MonoBehaviour {

	public Camera CameraFacing;
	private Vector3 originalScale;

	// Use this for initialization
	void Start () {

		originalScale = transform.localScale;
	}
	
	// Update is called once per frame
	void Update () {
		if (!CameraFacing)
						return;
		float distance;

		//transform.position = CameraFacing.transform.position + CameraFacing.transform.rotation * Vector3.forward * 2.0f;
		transform.LookAt (CameraFacing.transform.position);
		transform.Rotate (1.0f, 180.0f, 1.0f);

		RaycastHit hit;

		//if (Physics.Raycast (new Ray (CameraFacing.transform.position, CameraFacing.transform.rotation * Vector3.forward), out hit)) {
//		if (Physics.Raycast (new Ray (transform.position, transform.rotation * Vector3.back), out hit)) {
//				distance = hit.distance;
//		} else {
//			distance = CameraFacing.farClipPlane * 0.95f;
//		}
		distance = Vector3.Distance (transform.position, CameraFacing.transform.position);

		if (distance < 10) {
			distance = 1 + 5 * Mathf.Exp (-distance);
		}

		transform.localScale = originalScale * distance;

	}
}
