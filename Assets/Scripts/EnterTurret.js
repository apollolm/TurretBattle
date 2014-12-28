#pragma strict

 //var MovScript : Component; //The PlayerController Script Ref.
 public var Player : Transform;
 var Speed = 50.0;
 var CarSpeed = 50.0;
 var StopSpeed = 0.0;
 var InCarPosition : Transform;
 var PlayerIn = true;
 var ExitPoint : Transform;
 var CanChange = true;
 var turnSpeed: float = 90;
 
 	/// <summary>
	/// The rate of damping on movement.
	/// </summary>
	var Damping: float = 0.3;
 
 var otherScript: OVRPlayerController;
 
 function Start(){
  	//otherScript = GameObject.Find(OVRPlayerController);
  	//Player.forward = transform.right;
 }
 
 function Update () {
 
// 	if(PlayerIn == true){
//	 	transform.Rotate(0, Input.GetAxis("Horizontal")*turnSpeed*Time.deltaTime, 0);
//	}

	 if(PlayerIn == true){
		 //Player.transform.rotation.y  = -transform.rotation.y;
		 //Player.transform.rotation.z  = -transform.rotation.z;
		 //Player.transform.rotation.x  = transform.rotation.x;
	 }else{
	 
	 }
 
// if(Input.GetKey(KeyCode.W)&& PlayerIn == true){
// 	 var motorDamp: float = (1.0f + (Damping * 60f * Time.deltaTime));
//	 rigidbody.velocity = motorDamp * transform.forward * Speed *Input.GetAxis("Vertical");
//	 //rigidbody.velocity.y = Input.GetAxis("root");
// }
 
// if(Vector3.Distance(Player.position,transform.position) < 5){
//	 if(Input.GetKey(KeyCode.E)&& PlayerIn == false){
//		 if(CanChange == true){
//			 CanChange = false;
//			 Change();
////			 MovScript.gravity = 0;
////			 MovScript.runSpeed = 0;
//			 otherScript.Acceleration = 0;
//			 otherScript.GravityModifier = 0;
//			 Player.transform.parent = transform;
//			 //Player.transform.position.y  = InCarPosition.transform.position.y;
//			 //Player.transform.position.x  = InCarPosition.transform.position.x;
//			 //Player.transform.position.z  = InCarPosition.transform.position.z;
//			 PlayerIn = true;
//			 Speed = CarSpeed;
//		 }
//	 }else{
//		 if(Input.GetKey(KeyCode.E)&& PlayerIn == true){
//			 if(CanChange == true){
//				 CanChange = false;
//				 Change();
////				 MovScript.gravity = 20;
////				 MovScript.runSpeed = 30;
//				 Player.transform.parent = null;
//				 otherScript.Acceleration = 0.1;
//				 otherScript.GravityModifier = 0.379;
//				 PlayerIn = false;
//				 Speed = StopSpeed;
//			 }
//		 }
//	}
//   }
 }
 
 function Change(){
	 yield WaitForSeconds(1);
	 CanChange = true;
 }