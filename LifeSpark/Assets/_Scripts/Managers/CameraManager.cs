using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour {

    public int m_scrollSpeed = 50;
    public int m_scrollThreshold = 30; // how close to the edge of the screen cursor must be to move camera
    
    // Camera's boundaries in each direction
    public int m_boundary_North = 25;
    public int m_boundary_East = 50;
    public int m_boundary_South = 75;
    public int m_boundary_West = 50;

    public int m_zoomSpeed = 40;
    public int m_zoomMin = 10;
    public int m_zoomMax = 60;

	// For use when running on iPad
	private int t_prevTouchCount = 0;
	

	/// Update is called once per frame
	void Update () {


        // Initialize Camera translation for this frame
        Vector3 translation = Vector3.zero;


        // Zoom in or out
        var zoomDelta = Input.GetAxis("Mouse ScrollWheel") * m_zoomSpeed * Time.deltaTime;
        {
            translation += camera.transform.forward * m_zoomSpeed * zoomDelta;
        }


        // Move Camera if player's mouse cursor reaches screen borders
		if (Application.platform == RuntimePlatform.IPhonePlayer) {

			// if on iPad, use 2 finger swipe to move camera
			if (Input.touchCount == 2 && t_prevTouchCount == 2) {
				translation -= Vector3.right * Input.touches[0].deltaPosition.x; 
				translation -= Vector3.forward * Input.touches[0].deltaPosition.y;
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
        if (desiredPosition.y < m_zoomMin || desiredPosition.y > m_zoomMax) {
            translation.y = 0;
        }


        // Move camera
        camera.transform.position += translation;
    
	}
}
