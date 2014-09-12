using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour {

    public int scrollSpeed = 50;
    public int scrollThreshold = 30; // how close to the edge of the screen cursor must be to move camera
    
    // Camera's boundaries in each direction
    public int boundary_North = 25;
    public int boundary_East = 50;
    public int boundary_South = 75;
    public int boundary_West = 50;

    public int zoomSpeed = 40;
    public int zoomMin = 10;
    public int zoomMax = 60;

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {


        // Initialize Camera translation for this frame
        Vector3 translation = Vector3.zero;


        // Zoom in or out
        var zoomDelta = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * Time.deltaTime;
        {
            translation += camera.transform.forward * zoomSpeed * zoomDelta;
        }


        // Move Camera if player's mouse cursor reaches screen borders
        if (Input.mousePosition.x < scrollThreshold)
        {
            translation += Vector3.right * -scrollSpeed * Time.deltaTime;
        }

        if (Input.mousePosition.x >= Screen.width - scrollThreshold)
        {
            translation += Vector3.right * scrollSpeed * Time.deltaTime;
        }

        if (Input.mousePosition.y < scrollThreshold)
        {
            translation += Vector3.forward * -scrollSpeed * Time.deltaTime;
        }

        if (Input.mousePosition.y > Screen.height - scrollThreshold)
        {
            translation += Vector3.forward * scrollSpeed * Time.deltaTime;
        }


        // Keep camera within level and zoom area
        Vector3 desiredPosition = camera.transform.position + translation;
        if (desiredPosition.x < -boundary_West || desiredPosition.x > boundary_East)
        {
            translation.x = 0;
        }
        if (desiredPosition.z < -boundary_South || desiredPosition.z > boundary_North)
        {
            translation.z = 0;
        }
        if (desiredPosition.y < zoomMin || desiredPosition.y > zoomMax)
        {
            translation.y = 0;
        }


        // Move camera
        camera.transform.position += translation;
    
	}
}
