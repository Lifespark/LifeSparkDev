using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour {


	// PUBLIC DATA 

	// Camera Scroll Parameters
    public float m_scrollSpeed = 50.0f;
    public int m_scrollThreshold = 30; // how close to the edge of the screen cursor must be to move camera
	public float m_iPadScrollSpeed = 1.0f;
    
    // Camera's boundaries in each direction
    public int m_boundary_North = 25;
    public int m_boundary_East = 50;
    public int m_boundary_South = 75;
    public int m_boundary_West = 50;

	// Zoom parameters
    public float m_zoomSpeed = 40.0f;
    public float m_zoomMin = 5.0f;
    public float m_zoomMax = 50.0f;

	// Scoop parameters
	public Vector3 m_vDefaultRotation = new Vector3(45.0f, 0.0f, 0.0f); // as euler angles
	public Vector3 m_vScoopedRotation = new Vector3(10.0f, 0.0f, 0.0f); // as euler angles
	public float m_scoopHeight = 20.0f;	//Camera will start rotating upwards when zooming below y=20




	// PRIVATE DATA

	private Quaternion m_qDefaultRotation = new Quaternion();
	private Quaternion m_qScoopedRotation = new Quaternion();

	// For use when running on iPad
	private int t_prevTouchCount = 0;


	void Start() {
		m_qDefaultRotation.eulerAngles = m_vDefaultRotation;
		m_qScoopedRotation.eulerAngles = m_vScoopedRotation;
	}


	/// Update is called once per frame
	void Update() {


        // Initialize Camera translation for this frame
        Vector3 translation = Vector3.zero;


        // Zoom in or out
        float zoomDelta = Input.GetAxis("Mouse ScrollWheel") * m_zoomSpeed * Time.deltaTime;
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



        // Camera Scrolling
		if (Application.platform == RuntimePlatform.IPhonePlayer) {

			// if on iPad, use 2 finger swipe to move camera
			if (Input.touchCount == 2 && t_prevTouchCount == 2) {
				translation -= Vector3.right * Input.touches[0].deltaPosition.x * Time.deltaTime * m_iPadScrollSpeed; 
				translation -= Vector3.forward * Input.touches[0].deltaPosition.y * Time.deltaTime * m_iPadScrollSpeed;
			}

			t_prevTouchCount = Input.touchCount;

		} 
        else {
			// otherwise (i.e. on PC/Mac) use traditional RTS camera controls
			if (Input.mousePosition.x < m_scrollThreshold) {
				translation += Vector3.right * -m_scrollSpeed * Time.deltaTime;
			}
			if (Input.mousePosition.x >= Screen.width - m_scrollThreshold) {
				translation += Vector3.right * m_scrollSpeed * Time.deltaTime;
			}

			if (Input.mousePosition.y < m_scrollThreshold) {
				translation += Vector3.forward * -m_scrollSpeed * Time.deltaTime;
			}

			if (Input.mousePosition.y > Screen.height - m_scrollThreshold) {
				translation += Vector3.forward * m_scrollSpeed * Time.deltaTime;
			}
		}

        // Keep camera within level and zoom area
        Vector3 desiredPosition = camera.transform.position + translation;
        if (desiredPosition.x < -m_boundary_West || desiredPosition.x > m_boundary_East) {
            translation.x = 0;
        }
        if (desiredPosition.z < -m_boundary_South || desiredPosition.z > m_boundary_North) {
            translation.z = 0;
        }
       


        // Move camera
        camera.transform.position += translation;
    
	}
}
