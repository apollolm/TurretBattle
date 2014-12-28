using UnityEngine;
using System.Collections;

public class TestGUI : MonoBehaviour {

	public Texture ImageCrosshair;
	Rect testRect;

	// Handle to OVRCameraRig
	private OVRCameraRig CameraController = null;

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

	void Awake()
	{    
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
		
		// Find player controller
//		OVRPlayerController[] PlayerControllers;
//		PlayerControllers = gameObject.GetComponentsInChildren<OVRPlayerController>();
//		
//		if(PlayerControllers.Length == 0)
//			Debug.LogWarning("OVRMainMenu: No OVRPlayerController attached.");
//		else if (PlayerControllers.Length > 1)
//			Debug.LogWarning("OVRMainMenu: More then 1 OVRPlayerController attached.");
//		else{
//			PlayerController = PlayerControllers[0];
//			#if USE_NEW_GUI
//			OVRUGUI.PlayerController = PlayerController;
//			#endif
//		}
		
		#if USE_NEW_GUI
		// Create canvas for using new GUI
		NewGUIObject = new GameObject();
		NewGUIObject.name = "OVRGUIMain";
		NewGUIObject.transform.parent = GameObject.Find("LeftEyeAnchor").transform;
		RectTransform r = NewGUIObject.AddComponent<RectTransform>();
		r.sizeDelta = new Vector2(100f, 100f);
		r.localScale = new Vector3(0.001f, 0.001f, 0.001f);
		r.localPosition = new Vector3(0.01f, 0.17f, 0.53f);
		r.localEulerAngles = Vector3.zero;
		
		Canvas c = NewGUIObject.AddComponent<Canvas>();
		#if (UNITY_5_0)
		// TODO: Unity 5.0b11 has an older version of the new GUI being developed in Unity 4.6.
		// Remove this once Unity 5 has a more recent merge of Unity 4.6.
		c.renderMode = RenderMode.World;
		#else
		c.renderMode = RenderMode.WorldSpace;
		#endif
		c.pixelPerfect = false;
		#endif
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

	}

	void OnGUI() {

		//RW - this was taken from the OVRMainMenu.cs file.
		//It swaps the default renderTexture with a custom one for OVR.
		//This has to happen before any of your typical OnGUI commands execute.
		
		//It is paired with a ending SwapRenderer function that puts stuff back in the right place at the end of OnGUI

		//***
		// Set the GUI matrix to deal with portrait mode
		Vector3 scale = Vector3.one;
		Matrix4x4 svMat = GUI.matrix; // save current matrix
		// substitute matrix - only scale is altered from standard
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		
		// Cache current active render texture
		RenderTexture previousActive = RenderTexture.active;
		
		// if set, we will render to this texture
		if(GUIRenderTexture != null && GUIRenderObject.activeSelf)
		{
			RenderTexture.active = GUIRenderTexture;
			GL.Clear (false, true, new Color (0.0f, 0.0f, 0.0f, 0.0f));
		}




		//********************************Regular OnGUI Functionality Here
		if (!ImageCrosshair) {
			Debug.LogError("Assign a Texture in the inspector.");
			return;
		}
		//GUI.DrawTexture(testRect, ImageCrosshair);
		GUI.DrawTexture(new Rect(	XL - (ImageCrosshair.width * 0.5f),
		                         YL - (ImageCrosshair.height * 0.5f), 
		                         ImageCrosshair.width,
		                         ImageCrosshair.height), 
		                ImageCrosshair);

		GUI.color = Color.white;

		//********************************End Regular OnGUI Functionality


		//RW - this was taken from the OVRMainMenu.cs file.
		//This snippet restores the previous renderTexture stored above.
		// Restore active render texture
		if (GUIRenderObject.activeSelf)
		{
			RenderTexture.active = previousActive;
		}
		
		// ***
		// Restore previous GUI matrix
		GUI.matrix = svMat;
	}


	
}