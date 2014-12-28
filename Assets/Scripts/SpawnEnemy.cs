using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
	//public PlayerHealth playerHealth;       // Reference to the player's heatlh.
	public GameObject enemy;                // The enemy prefab to be spawned.
	public float spawnTime = 3f;            // How long between each spawn.
	public Transform[] spawnPoints;         // An array of the spawn points this enemy can spawn from.

	private Camera camera;
	
	void Start ()
	{
		// Call the Spawn function after a delay of the spawnTime and then continue to call after the same amount of time.
		InvokeRepeating ("Spawn", spawnTime, spawnTime);
		camera = GameObject.Find ("LeftEyeAnchor").camera;
	}
	
	
	void Spawn ()
	{
		// If the player has no health left...
//		if(playerHealth.currentHealth <= 0f)
//		{
//			// ... exit the function.
//			return;
//		}
		
		// Find a random index between zero and one less than the number of spawn points.
		int spawnPointIndex = Random.Range (0, spawnPoints.Length);


		
		// Create an instance of the enemy prefab at the randomly selected spawn point's position and rotation.
		GameObject enemyObject = Instantiate (Resources.Load ("Rocket", typeof(GameObject)), spawnPoints [spawnPointIndex].position, spawnPoints [spawnPointIndex].rotation) as GameObject;
		ReticleOVR script = enemyObject.GetComponentInChildren<ReticleOVR>();
		script.CameraFacing = camera;
		//script.Init();

		Debug.Log ("Spawned Rocket");
	}
}