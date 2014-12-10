using UnityEngine;
using System.Collections;

namespace AGE{

[EventCategory("Movement")]
public class CameraLookAt : TickEvent
{
	public Vector3 worldOffset = Vector3.zero;
	public Vector3 localOffset = Vector3.zero;

	// old data
	public bool overrideUpDir = true;
	public Vector3 upDir = Vector3.up;

	// new data
	public enum EUpDirType
	{
		NoOverrideUp = 0,
		RowAngleByZ,
	}
	public EUpDirType UpDirType = EUpDirType.NoOverrideUp;
	public float rowAngleByZ = 0.0f;


	[ObjectTemplate]
	public int cameraId = 0;

	[ObjectTemplate]
	public int targetId = -1;

	public override bool SupportEditMode ()
	{
		return true;
	}
	
	public override void Process (Action _action, Track _track)
	{
		if (_action.GetGameObject(cameraId) == null) return;

		_action.GetGameObject(cameraId).transform.rotation = GetLookRotation(_action);
	}
	
	public override void ProcessBlend(Action _action, Track _track, TickEvent _prevEvent, float _blendWeight)
	{
		if (_action.GetGameObject(cameraId) == null || _prevEvent == null) return;

		_action.GetGameObject(cameraId).transform.rotation = Quaternion.Slerp((_prevEvent as CameraLookAt).GetLookRotation(_action), GetLookRotation(_action), _blendWeight);
	}

	//calculate rotation based on given object space
	Quaternion GetLookRotation(Action _action)
	{
		GameObject cameraObject = _action.GetGameObject(cameraId);
		if (cameraObject == null) return Quaternion.identity;

		GameObject targetObject = _action.GetGameObject(targetId);
		Vector3 targetPos = new Vector3(0, 0, 0);
		if (targetObject == null)
			targetPos = localOffset + worldOffset;
		else
			targetPos = targetObject.transform.position + targetObject.transform.TransformDirection(localOffset) + worldOffset;
		Vector3 lookDir = targetPos - cameraObject.transform.position;

//		// old cal method
//		if (overrideUpDir)
//			return Quaternion.LookRotation(lookDir, upDir);
//		else
//			return Quaternion.LookRotation(lookDir, cameraObject.transform.up);

		if( UpDirType == EUpDirType.NoOverrideUp )
			return Quaternion.LookRotation(lookDir, cameraObject.transform.up);
		else
		{
			Quaternion rot = Quaternion.AngleAxis( rowAngleByZ, Vector3.forward );
			Quaternion oldrot = Quaternion.LookRotation(lookDir, Vector3.up);
			return oldrot * rot;
		}

	}
}
}

