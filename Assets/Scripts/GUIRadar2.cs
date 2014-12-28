using UnityEngine;
using System.Collections;
using System.Collections.Generic;                //Use this so you can manipulate Lists

public class GUIRadar2 : MonoBehaviour
{
	[SerializeField] private Texture2D targetIcon;                    //The target icon that will be placed in front of targets
	
	[SerializeField] private float maxTargetIconSize = 300f;        //The maximum size of icons when close to the target
	[SerializeField] private float minTargetIconSize = 0f;            //The minimum size of icons when they appear (far from the target)
	[SerializeField] private float maxDistanceDisplay = 10f;        //The maximum distance from where you can see the icons appearing
	[SerializeField] private float minDistanceDisplay = 2f;            //The minimum distance from where you see the icons disappearing when too close
	[SerializeField] private float smoothGrowingParameter = 25f;    //The speed of the growing effect on icons
	[SerializeField] private float smoothMovingParameter = 25f;        //The moving speed of items : high values means the icon is "attached" to the item
	[SerializeField] private string tagToFollow = "RadarDetectable";
	[SerializeField] private bool directViewOnly = true;            //If you want to allow icon display even if targets aren't in direct view for the player, set it to false
	[SerializeField] private bool activateAlphaBlending = true;        //If alpha blending is needed when the icon approachs the camera viewport edges
	[SerializeField] private float alphaStartPercentage = 0.2f;     //Indicates when the icon will start going from alpha=1 to alpha=0 (0.2 = 20% of the viewport size)
	
	// Handle to OVRCameraRig
	private OVRCameraRig CameraController = null;
	
	private Camera LeftEyeCamera = null;
	
	private float  XL 				  = 0.0f;
	private float  YL 				  = 0.0f;
	
	private float  ScreenWidth		  = 1280.0f;
	private float  ScreenHeight 	  =  800.0f;
	
	// We can set the layer to be anything we want to, this allows
	// a specific camera to render it
	public string 			LayerName 		 = "Default";
	
	// Replace the GUI with our own texture and 3D plane that
	// is attached to the rendder camera for true 3D placement
	private OVRGUI  		GuiHelper 		 = new OVRGUI();
	private GameObject      GUIRenderObject  = null;
	private RenderTexture	GUIRenderTexture = null;
	
	public struct TargetStruct                    //Structure that contain every icon informations
	{
		public GameObject item;                    //-the item the icon is linked to
		public Vector3 screenPos;                //-the screen position of the icon (!= world position)
		public float xSize, ySize;                //-the current size of the icon on the screen
		public float xPos, yPos;                //-the current coordinates of the screen position
		public float xTargetSize, yTargetSize;    //-the coordinates you want the icon to reach
		public float xTargetPos, yTargetPos;    //-the size you want the icon to reach on the screen
		public float distance;                    //-the distance between player and the item linked to the icon
		public bool directView;                    //-tells you if the item is in direct view or not
		public float alpha;                        //-indicates the alpha level to apply to the icon texture
	}
	
	private List<TargetStruct> TargetList = new List<TargetStruct>();//Your ICONS LIST
	private GameObject[] Targets;                                    //The GameObjects considered as targets
	
	private int targetsCount;                                        //Number of targets in the scene
	
	private float cameraXSize;
	private float cameraYSize;
	private float cameraXPos;
	private float cameraYPos;
	
	private Color guiGolor = Color.white;
	
	void Awake()                                                      
	{
		//Check if the tag exists in the project
		#if UNITY_EDITOR
		bool checkTag = false;
		for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
		{
			if (UnityEditorInternal.InternalEditorUtility.tags[i].Equals(tagToFollow))
				checkTag = true;
		}
		if (!checkTag)
			Debug.LogWarning(tagToFollow + " tag not found in the project.");
		#endif
		
		guiGolor = Color.white;
		
		SetCameraSpecifications();
		
		UpdateTargets();
	}
	
	void Start()
	{
		
		ScreenWidth  = Screen.width;
		ScreenHeight = Screen.height;
		
		// Initialize screen location of cursor
		XL = ScreenWidth * 0.5f;
		YL = ScreenHeight * 0.5f;
		
		// Set the GUI target
		GUIRenderObject = GameObject.Instantiate(Resources.Load("OVRGUIObjectMain")) as GameObject;
		
		if(GUIRenderObject != null)
		{
			// Chnge the layer
			GUIRenderObject.layer = LayerMask.NameToLayer(LayerName);
			
			if(GUIRenderTexture == null)
			{
				int w = Screen.width;
				int h = Screen.height;
				
				// We don't need a depth buffer on this texture
				GUIRenderTexture = new RenderTexture(w, h, 0);	
				GuiHelper.SetPixelResolution(w, h);
				// NOTE: All GUI elements are being written with pixel values based
				// from DK1 (1280x800). These should change to normalized locations so 
				// that we can scale more cleanly with varying resolutions
				GuiHelper.SetDisplayResolution(1280.0f, 800.0f);
			}
		}
		
		// Attach GUI texture to GUI object and GUI object to Camera
		if(GUIRenderTexture != null && GUIRenderObject != null)
		{
			GUIRenderObject.GetComponent<Renderer>().material.mainTexture = GUIRenderTexture;
			
			if(CameraController != null)
			{
				// Grab transform of GUI object
				Vector3 ls = GUIRenderObject.transform.localScale;
				Vector3 lp = GUIRenderObject.transform.localPosition;
				Quaternion lr = GUIRenderObject.transform.localRotation;
				
				// Attach the GUI object to the camera
				GUIRenderObject.transform.parent = CameraController.centerEyeAnchor;
				// Reset the transform values (we will be maintaining state of the GUI object
				// in local state)
				
				GUIRenderObject.transform.localScale = ls;
				GUIRenderObject.transform.localPosition = lp;
				GUIRenderObject.transform.localRotation = lr;
				
				// Deactivate object until we have completed the fade-in
				// Also, we may want to deactive the render object if there is nothing being rendered
				// into the UI
				//GUIRenderObject.SetActive(false);
			}
		}
		
		//Every second, check for new enemies
		InvokeRepeating ("UpdateTargets", 1f, 1f);
		
	}
	
	void Update()
	{      
		//UpdateTargets();
		for (int i = 0; i<targetsCount; i++)                                                        //You have to repeat it for every icons : be aware that if you have too much that could use a lot of ressoures
		{
			TargetStruct target = new TargetStruct();                                                //You have to create a temporary TargetStruct since you can't access a variable of an element in a list directly
			target = TargetList[i];                                                                    //You take the item attached to the icon
			
			if (target.item)
			{
				target.screenPos = LeftEyeCamera.WorldToScreenPoint(target.item.transform.position);    //Convert world coordinates of the item into screen ones
				target.distance = Vector3.Distance(target.item.transform.position, transform.position);    //Get the distance between item and player
			}
			
			if (target.distance > maxDistanceDisplay || target.distance < minDistanceDisplay)            //If the item is too far or too close
			{
				target.xTargetSize = minTargetIconSize;                                                //you want it to disappear
				target.yTargetSize = minTargetIconSize;                                                //or at least to be in its smaller size
			}
			else
			{
				target.xTargetSize = maxTargetIconSize / (target.distance);                            //Else you get its size with the
				target.yTargetSize = maxTargetIconSize / (target.distance);                            //distance : far<=>small / close<=>big
			}
			
			if (target.distance>maxDistanceDisplay)                                                    //If the item is too far, you set its screen position : (this way it seems as if the icon was coming away from the screen to focus your target)
			{
				if (target.screenPos.x < cameraXPos + cameraXSize * 0.5f)                            //-if it's under the center of the view field
					target.xTargetPos = cameraXPos;                                                        //to the bottom of the screen
				else                                                                                 //-else
					target.xTargetPos = cameraXPos + cameraXSize;                                        //to the top of the screen
				
				if ((Screen.height - target.screenPos.y) < cameraYPos + cameraYSize * 0.5f)            //-if it's on the right of the view field
					target.yTargetPos = cameraYPos;                                                        //to the right of the screen
				else                                                                                 //-else
					target.yTargetPos = cameraYPos + cameraYSize;                                        //to the left of the screen
			}
			else                                                                                     //If the item is NOT too far, you set its screen position :
			{
				//target.xTargetPos = target.screenPos.x - target.xSize * 0.5f;                        //in x-axis to the item's x-position minus half of the icon's size
				//target.yTargetPos = Screen.height - target.screenPos.y - target.ySize * 0.5f;        //in y-axis to the item's y-position minus half of the icon's size
				
				
				target.xTargetPos = target.screenPos.x + 300;                        //in x-axis to the item's x-position minus half of the icon's size
				target.yTargetPos = Screen.height - target.screenPos.y - target.ySize * 0.5f;    
			}
			
			if (activateAlphaBlending)
			{
				target.alpha = 1f;
				
				if (target.xPos < cameraXPos + cameraXSize * alphaStartPercentage)
					target.alpha = Mathf.Clamp((target.xPos - cameraXPos) / (cameraXSize * alphaStartPercentage), 0f, 1f);
				else if (target.xPos + target.xSize > cameraXPos + cameraXSize * (1f - alphaStartPercentage))
					target.alpha = 1f - Mathf.Clamp((target.xPos + target.xSize - (cameraXPos + cameraXSize * (1f - alphaStartPercentage))) / (cameraXSize * alphaStartPercentage), 0f, 1f);
				
				if (target.alpha == 1f)
				{
					if (target.yPos < cameraYPos + cameraYSize * alphaStartPercentage)
						target.alpha = Mathf.Clamp((target.yPos - cameraYPos) / (cameraYSize * alphaStartPercentage), 0f, 1f);
					else if (target.yPos + target.ySize > cameraYPos + cameraYSize * (1f - alphaStartPercentage))
						target.alpha = 1f - Mathf.Clamp(((target.yPos + target.ySize) - (cameraYPos + cameraYSize * (1f - alphaStartPercentage))) / (cameraYSize * alphaStartPercentage), 0f, 1f);
				}
				else
				{
					if (target.yPos < cameraYPos + cameraYSize * alphaStartPercentage)
						target.alpha -= 1f - Mathf.Clamp((target.yPos - cameraYPos) / (cameraYSize * alphaStartPercentage), 0f, 1f);
					else if (target.yPos + target.ySize > cameraYPos + cameraYSize * (1f - alphaStartPercentage))
						target.alpha -= Mathf.Clamp(((target.yPos + target.ySize) - (cameraYPos + cameraYSize * (1f - alphaStartPercentage))) / (cameraYSize * alphaStartPercentage), 0f, 1f);
				}
			}
			
			target.xSize = Mathf.Lerp(target.xSize, target.xTargetSize, smoothGrowingParameter*Time.deltaTime);    //You do lerps on your icons size so you can adjust
			target.ySize = Mathf.Lerp(target.xSize, target.yTargetSize, smoothGrowingParameter*Time.deltaTime);    //the speed of their resizing
			
			target.xPos = Mathf.Lerp(target.xPos, target.xTargetPos, smoothMovingParameter*Time.deltaTime);        //You do lerps on your icons position so you can adjust
			target.yPos = Mathf.Lerp(target.yPos, target.yTargetPos, smoothMovingParameter*Time.deltaTime);        //their moving speed
			
			if (target.item)
			{
				//				RaycastHit hitInfos = new RaycastHit();                                                                    //You create a new variable to stock the information of the coming raycast
				//				Physics.Raycast(transform.position, target.item.transform.position-transform.position, out hitInfos);    //and you RayCast from the player's position to the item's position
				//				
				//				if(hitInfos.collider.gameObject.layer==8)                                                                //HERE IS A BIT TRICKY : you have to creat new layers (I called them "Interactive Items" and "Obstacles") and to apply them to your various items.
				//					target.directView=true;                                                                                //Then you select EVERY items in your scene and set their layer to "Ignore Raycast". After that you select your interactive items biggest shape (if you have big trigger colliders on them select the item that hold it),
				//				else                                                                                                     //and set their layers to "Interactive Items". Last part is setting every potential obstacle item layer to "Obstacles".
				target.directView=false;                                                                            //NOTE : Here my "Interactive Items" layer number is 8
				
				TargetList[i] = target;                                                                    //You apply all the variables to your index-i icon in the ICONS LIST
			}
		}
	}
	
	void OnGUIContent()
	{
		GUI.color = guiGolor;
		for (int i = 0; i<targetsCount; i++)                                                                                        //For every icon
		{
			if (TargetList[i].screenPos.z>0 && (!directViewOnly || (directViewOnly && TargetList[i].directView)))                    //If the icon is in front of you and all the required conditions are okay
			{
				if (activateAlphaBlending)
				{
					guiGolor.a = TargetList[i].alpha;
					GUI.color = guiGolor;
				}
				GUI.DrawTexture(new Rect(TargetList[i].xPos, TargetList[i].yPos, TargetList[i].xSize, TargetList[i].ySize), targetIcon);//you display the icon with it's size and position
			}
		}
	}
	
	
	void OnGUI()
	{
		//RW - this was taken from the OVRMainMenu.cs file.
		//It swaps the default renderTexture with a custom one for OVR.
		//This has to happen before any of your typical OnGUI commands execute.
		
		//It is paired with a ending SwapRenderer function that puts stuff back in the right place at the end of OnGUI
		
		//***
		// Set the GUI matrix to deal with portrait mode
		Vector3 scale = Vector3.one;
		Matrix4x4 svMat = GUI.matrix; // save current matrix
		// substitute matrix - only scale is altered from standard
		GUI.matrix = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, scale);
		
		// Cache current active render texture
		RenderTexture previousActive = RenderTexture.active;
		
		// if set, we will render to this texture
		if (GUIRenderTexture != null && GUIRenderObject.activeSelf) {
			RenderTexture.active = GUIRenderTexture;
			GL.Clear (false, true, new Color (0.0f, 0.0f, 0.0f, 0.0f));
		}
		
		
		
		
		//********************************Regular OnGUI Functionality Here
		
		OnGUIContent ();
		
		//********************************End Regular OnGUI Functionality
		
		
		//RW - this was taken from the OVRMainMenu.cs file.
		//This snippet restores the previous renderTexture stored above.
		// Restore active render texture
		if (GUIRenderObject.activeSelf) {
			RenderTexture.active = previousActive;
		}
		
		// ***
		// Restore previous GUI matrix
		GUI.matrix = svMat;
	}
	
	
	private void SetCameraSpecifications()
	{
		//Rect cameraViewport =  this.camera.rect;
		//		Rect cameraViewport = cameraRig.camera.rect;
		//		cameraXPos = cameraViewport.x * Screen.width;
		//		cameraYPos = (1f - cameraViewport.y - cameraViewport.height) * Screen.height;
		//		cameraXSize = cameraViewport.width * Screen.width;
		//		cameraYSize = cameraViewport.height * Screen.height;
		
		// Find camera controller
		OVRCameraRig[] CameraControllers;
		CameraControllers = gameObject.GetComponentsInChildren<OVRCameraRig>();
		
		if(CameraControllers.Length == 0)
			Debug.LogWarning("OVRMainMenu: No OVRCameraRig attached.");
		else if (CameraControllers.Length > 1)
			Debug.LogWarning("OVRMainMenu: More then 1 OVRCameraRig attached.");
		else{
			CameraController = CameraControllers[0];
			#if USE_NEW_GUI
			OVRUGUI.CameraController = CameraController;
			#endif
		}
		
		LeftEyeCamera = GameObject.Find ("LeftEyeAnchor").camera;
		Rect cameraViewport = LeftEyeCamera.rect;
		cameraXPos = cameraViewport.x * Screen.width;
		cameraYPos = (1f - cameraViewport.y - cameraViewport.height) * Screen.height;
		cameraXSize = cameraViewport.width * Screen.width;
		cameraYSize = cameraViewport.height * Screen.height;
		
	}
	
	public void UpdateTargets()
	{
		Targets = GameObject.FindGameObjectsWithTag(tagToFollow);            //Get all the potential targets in the scene (just replace it by your own tag : "MyTag")
		
		foreach (GameObject target in Targets)                                    //Put every detected GameObject into your ICONS LIST
		{
			TargetStruct newTarget = new TargetStruct();
			newTarget.item = target;                                            //and attach each icon its GameObject
			newTarget.alpha = 1f;
			TargetList.Add(newTarget);
		}
		
		targetsCount = TargetList.Count;                                        //Count the number of target in the scene
	}
	
	public void DeleteTarget(GameObject obj, bool resetIconsPositions = false)        // 'obj' is the item tagged followed by the script
	{                                                                                // 'resetIconsPositions' allow to reset the icons positions as if the script was just being started
		for (int i = 0; i<targetsCount; i++)
		{
			TargetStruct target = new TargetStruct();
			target = TargetList[i];
			if (target.item.Equals(obj))
			{
				TargetList.RemoveAt(i);
				obj.tag = "Untagged";
				targetsCount --;
				break;
			}
		}
		
		if (resetIconsPositions)
			UpdateTargets();
	}
}
