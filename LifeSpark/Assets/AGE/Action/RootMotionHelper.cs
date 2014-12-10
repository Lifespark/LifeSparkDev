using UnityEngine;
using System.Collections;

namespace AGE{

public class RootMotionHelper : MonoBehaviour
{
	public Transform rootTransform = null;

	Vector3 posOffset = new Vector3();
	//Quaternion rotOffset = new Quaternion();

	void Start ()
	{
		posOffset = rootTransform.localPosition;
		//rotOffset = rootTransform.localRotation;
	}
	
	void LateUpdate () 
	{
		rootTransform.localPosition = posOffset;
		//rootTransform.localRotation = rotOffset;
	}

	public void ForceStart()
	{
		posOffset = rootTransform.localPosition;
	}

	public void ForceLateUpdate()
	{
		rootTransform.localPosition = posOffset;
	}
}
}
