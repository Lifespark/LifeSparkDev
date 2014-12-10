using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraManager : MonoBehaviour {

    static private CameraManager _instance;

    static public CameraManager Instance {
        get {
            if (_instance == null)
                _instance = FindObjectOfType(typeof(CameraManager)) as CameraManager;
            return _instance;
        }
    }

	// PUBLIC DATA 

	// Camera Scroll Parameters
    public float m_scrollSpeed = 50.0f;
    public int m_scrollThreshold = 0; // how close to the edge of the screen cursor must be to move camera
	public float m_iPadScrollSpeed = .1f;
    
    // Camera's boundaries in each direction
    public int m_boundary_North = 25;
    public int m_boundary_East = 50;
    public int m_boundary_South = 75;
    public int m_boundary_West = 50;

	// Zoom parameters
    public float m_zoomSpeed = 40.0f;
    public float m_zoomMin = 5.0f;
    public float m_zoomMax = 50.0f;
	public float m_iPadZoomSpeed = 1.0f;

	// Scoop parameters
	public Vector3 m_vDefaultRotation = new Vector3(45.0f, 0.0f, 0.0f); // as euler angles
	public Vector3 m_vScoopedRotation = new Vector3(10.0f, 0.0f, 0.0f); // as euler angles
	public float m_scoopHeight = 20.0f;	//Camera will start rotating upwards when zooming below y=20


    // Camera Focus
	public float m_fCameraAngle = 0.0f;
    public float m_cameraFlyTime = 0.2f;
    
    // if true, camera boundaries will be shown on screen (for designers)
    public bool showCameraBoundaries = true;

	// PRIVATE DATA

	private Quaternion m_qDefaultRotation = new Quaternion();
	private Quaternion m_qScoopedRotation = new Quaternion();

	// For use when running on iPad
	private int t_prevTouchCount = 0;
    private Vector3 t_prevMousePos;

	float NorthBound, SouthBound, EastBound, WestBound;


    void Awake() {
        _instance = this;

		NorthBound = CalcNorthViewableBoundary();
		SouthBound = CalcSouthViewableBoundary();
		EastBound = CalcEastViewableBoundary();
		WestBound = CalcWestViewableBoundary();

    }

	void Start() {
		m_qDefaultRotation.eulerAngles = m_vDefaultRotation;
		m_qScoopedRotation.eulerAngles = m_vScoopedRotation;
		m_fCameraAngle = (90.0f - transform.rotation.eulerAngles.x) * Mathf.PI / 180;
	}


	/// Update is called once per frame
	void Update() {

        // Initialize Camera translation for this frame
        Vector3 translation = Vector3.zero;



/*
        // Zoom in or out
		float zoomDelta = 0.0f;
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			// if on iPad pinch to zoom
			Touch touchZero = Input.GetTouch(0);
			Touch touchOne = Input.GetTouch (1);

			Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
			Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

			float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
			float touchDeltaMag = (touchZeroPrevPos - touchOne.position).magnitude;

			float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

			zoomDelta = deltaMagnitudeDiff * m_iPadZoomSpeed * Time.deltaTime;

		} 
		else {
			zoomDelta = Input.GetAxis("Mouse ScrollWheel") * m_zoomSpeed * Time.deltaTime;
		}
        
        translation += camera.transform.forward * m_zoomSpeed * zoomDelta;
		Vector3 desiredZoomPosition = camera.transform.position + translation;

		if (desiredZoomPosition.y < m_zoomMin || desiredZoomPosition.y > m_zoomMax) {
			// Don't zoom past predefined min and max
			translation.y = 0;
			translation.z = 0;
		}
		else if (desiredZoomPosition.y < m_scoopHeight && desiredZoomPosition.y > m_zoomMin) {
			// "Scoop" camera near ground if within a predefined height range
			float rotationAmount = 1.0f - ((desiredZoomPosition.y - m_zoomMin) / (m_scoopHeight - m_zoomMin));
			camera.transform.rotation = Quaternion.Slerp(m_qDefaultRotation, m_qScoopedRotation, rotationAmount);
		} 
		else {
			// Make sure camera doesn't scoop above a predefined height
			camera.transform.rotation = m_qDefaultRotation;
		}
*/


        // Camera Scrolling
		if (Application.platform == RuntimePlatform.IPhonePlayer) {

			// if on iPad, use a 1 finger swipe to move camera
			// Warning: never scroll camera here.
			/*if (Input.touchCount == 1 && t_prevTouchCount == 1) {
				translation -= Vector3.right * Input.touches[0].deltaPosition.x * Time.deltaTime * m_iPadScrollSpeed; 
				translation -= Vector3.forward * Input.touches[0].deltaPosition.y * Time.deltaTime * m_iPadScrollSpeed;
			}*/

			t_prevTouchCount = Input.touchCount;

		} 
        else {
            
		}

        // Keep camera within level and zoom area
        Vector3 desiredPosition = camera.transform.position + translation;
        if (desiredPosition.x < -m_boundary_West || desiredPosition.x > m_boundary_East) {
            //Debug.Log(desiredPosition.x);
            translation.x = 0;
        }
        if (desiredPosition.z < -m_boundary_South || desiredPosition.z > m_boundary_North) {
            translation.z = 0;
        }

        // Move camera if chat wheel not open
		if(!GameObject.Find ("ChatWindow").GetComponent<ChatWindow>().isChatButtonPressed)
        	camera.transform.position += translation;

        if (Input.GetKeyUp(KeyCode.Space)) {
            FocusOnPlayer();
        }



       

	}

	public void ScrollCamera(Vector3 translation) {
		// Keep camera within level
		Vector3 desiredPosition = camera.transform.position + (translation * m_iPadScrollSpeed);
		desiredPosition.x = Mathf.Clamp(desiredPosition.x, -m_boundary_West, m_boundary_East);
		desiredPosition.z = Mathf.Clamp(desiredPosition.z, -m_boundary_South, m_boundary_North);
		
		camera.transform.position = desiredPosition;
	}

    public void FocusOnPlayer()
    {
        StartCoroutine(Focus());
    }

	public void FollowPlayer()
	{
		StartCoroutine (Follow ());
	}

	IEnumerator Follow()
	{
		Player me = PlayerManager.Instance.myPlayer.GetComponent<Player>();
		Vector3 trajectory = me.GetComponent<NavMeshAgent>().velocity;
		trajectory.Normalize();
		//Can be tweaked to change the amount the camera is displaced by
		float displacement = 25;
		Vector3 targetPos = transform.position + trajectory * displacement;
		Vector3 originalPos = transform.position;
		float flyTime = 0;

		if (targetPos.x > EastBound)
			targetPos.x = EastBound;

		if (targetPos.x < WestBound)
			targetPos.x = WestBound;

		if (targetPos.z > NorthBound)
			targetPos.z = NorthBound;

		if (targetPos.z < SouthBound)
			targetPos.z = SouthBound;


		
		while (flyTime < m_cameraFlyTime)
		{
			transform.position = Vector3.Lerp(originalPos, targetPos, flyTime / m_cameraFlyTime);
			flyTime += Time.deltaTime;
			yield return null;
		}
		transform.position = targetPos;
	}

    IEnumerator Focus()
    {

        Vector3 originalPos = transform.position;
		Vector3 targetPos = new Vector3(PlayerManager.Instance.myPlayer.transform.position.x,
		                                transform.position.y,
		                                PlayerManager.Instance.myPlayer.transform.position.z - Mathf.Tan(m_fCameraAngle) * transform.position.y);
        float flyTime = 0;
        while (flyTime < m_cameraFlyTime)
        {
            transform.position = Vector3.Lerp(originalPos, targetPos, flyTime / m_cameraFlyTime);
            flyTime += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;
    }

    public void FocusOnMiniMap(Vector3 newPos)
    {
        StartCoroutine(FocusOnMap(newPos));
    }

    IEnumerator FocusOnMap(Vector3 newPos)
    {
        Vector3 originalPos = transform.position;
		Vector3 targetPos = newPos - new Vector3(0,0,Mathf.Tan(m_fCameraAngle) * transform.position.y);
        targetPos.y = originalPos.y;
        float flyTime = 0;
        while (flyTime < m_cameraFlyTime)
        {
            transform.position = Vector3.Lerp(originalPos, targetPos, flyTime / m_cameraFlyTime);
            flyTime += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;
    }
    
    
    void OnDrawGizmos() {
    
        // Draw red boundary Lines if enabled
    	if (showCameraBoundaries) {

			float northViewableBoundary = CalcNorthViewableBoundary();
			float southViewableBoundary = CalcSouthViewableBoundary();
			float eastViewableBoundary = CalcEastViewableBoundary();
            float westViewableBoundary = CalcWestViewableBoundary();
			
			// Establish the four corners of the viewable area
			Vector3 northWestCorner = new Vector3(westViewableBoundary, 0f, northViewableBoundary);
			Vector3 northEastCorner = new Vector3(eastViewableBoundary, 0f, northViewableBoundary);
			Vector3 southEastCorner = new Vector3(eastViewableBoundary, 0f, southViewableBoundary);
			Vector3 southWestCorner = new Vector3(westViewableBoundary, 0f, southViewableBoundary);
			
			// Set line color
			Gizmos.color = Color.red;
			
			
			// Draw north border
			Gizmos.DrawLine(northWestCorner, northEastCorner);
			
			// Draw east border
			Gizmos.DrawLine(northEastCorner, southEastCorner);
			
			// Draw south border
			Gizmos.DrawLine(southEastCorner, southWestCorner);
			
			// Draw west border
			Gizmos.DrawLine(southWestCorner, northWestCorner);
		}
    	
    }


    /// Calculates and returns the horizontal field of view (apparently this is not built into unity??)
    private float CalculateHorizFOV() {
        float radAngle = camera.fieldOfView * Mathf.Deg2Rad;
        float radHFOV = 2 * Mathf.Atan(Mathf.Tan(radAngle / 2) * camera.aspect);
        return Mathf.Rad2Deg * radHFOV;
    }


    private float CalcNorthViewableBoundary() {
        float camHeight = transform.position.y;
        float camAngle = transform.rotation.eulerAngles.x;
        float vFOV = camera.fieldOfView;

        float theta = camAngle - (vFOV/2);
        float d = camHeight * (1 / Mathf.Tan(theta * Mathf.Deg2Rad));

        return m_boundary_North + d;
    }


    private float CalcSouthViewableBoundary() {
        float camHeight = transform.position.y;
        float camAngle = transform.rotation.eulerAngles.x;
        float vFOV = camera.fieldOfView;

        float theta = (90 - camAngle) - (vFOV/2);
        float d = camHeight * Mathf.Tan(theta * Mathf.Deg2Rad);

        return -m_boundary_South + d;
    }


    private float CalcEastViewableBoundary() {
        float camHeight = transform.position.y;
        float camAngle = transform.rotation.eulerAngles.x;
        float hFOV = CalculateHorizFOV();
        float vFOV = camera.fieldOfView;
        float lookAtDistToGround = camHeight / Mathf.Cos((90 - camAngle) * Mathf.Deg2Rad); // distance from camera to ground along the lookAt vector

        float theta = hFOV / 2;
        float d = Mathf.Tan(theta * Mathf.Deg2Rad) * lookAtDistToGround;

        return m_boundary_East + d;
    }


    private float CalcWestViewableBoundary() {
        float camHeight = transform.position.y;
        float camAngle = transform.rotation.eulerAngles.x;
        float hFOV = CalculateHorizFOV();
        float vFOV = camera.fieldOfView;
        float lookAtDistToGround = camHeight / Mathf.Cos((90 - camAngle) * Mathf.Deg2Rad); // distance from camera to ground along the lookAt vector

        float theta = hFOV / 2;
        float d = Mathf.Tan(theta * Mathf.Deg2Rad) * lookAtDistToGround;

        

        return -m_boundary_West - d;
    }


    
    
}
