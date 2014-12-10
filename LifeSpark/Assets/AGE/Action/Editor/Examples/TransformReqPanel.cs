
using UnityEngine;
using UnityEditor;

namespace AGE{

public class TransformReqPanel : EditorPanelBase
{
	static Vector3 axisWeight = new Vector3(1, 0, 1);
	
	Transform fromTransform = null;
	Transform toTransform = null;
	Transform coordTransform = null;
	
	public bool coordinatedLocal = false;
	public bool normalizedRelative = false;

	public AGE.TransformDataType mDataType;
	public long mCtrlID;

	public bool mNeedReplyTransMsg = false;


	public void SetData( AGE.TransformDataType dt, long ctrlid )
	{
		mDataType = dt;
		mCtrlID = ctrlid;
		switch( mDataType )
		{
			case AGE.TransformDataType.Local_Position:
				coordinatedLocal = false;
				break;
			case AGE.TransformDataType.Coordinated_Position:
				coordinatedLocal = true;
				break;
			case AGE.TransformDataType.Relative_Position:
				normalizedRelative = false;
				break;
			case AGE.TransformDataType.Normalized_Relative_Position:
				normalizedRelative = true;
				break;
			case AGE.TransformDataType.Local_Rotation:
			case AGE.TransformDataType.Local_Rotation_EulerAngle:
				coordinatedLocal = false;
				break;
			case AGE.TransformDataType.Coordinated_Rotation:
			case AGE.TransformDataType.Coordinated_Rotation_EulerAngle:
				coordinatedLocal = true;
				break;
		}
	}

	public void Update()
	{
	}
	
	public void Draw()
	{
		GUI.BeginGroup( mRect );

		GUILayout.Label ("Relative Space", EditorStyles.boldLabel);
		fromTransform = EditorGUILayout.ObjectField("From", fromTransform, typeof(Transform), true) as Transform;
		toTransform = EditorGUILayout.ObjectField("To", toTransform, typeof(Transform), true) as Transform;
		
		GUILayout.Label ("Object Space", EditorStyles.boldLabel);
		coordTransform = EditorGUILayout.ObjectField("Coordinator", coordTransform, typeof(Transform), true) as Transform;
		
		GUILayout.Label ("Get Transforms", EditorStyles.boldLabel);

		string strCoordinatedLocal = "coordinatedLocal = " + (coordinatedLocal ? "true" : "false");
		string strNormalizedRelative = "normalizedRelative = " + (normalizedRelative ? "true" : "false");

		GUILayout.Label (strCoordinatedLocal, EditorStyles.boldLabel);
		GUILayout.Label (strNormalizedRelative, EditorStyles.boldLabel);

		GUI.EndGroup();
	}

	public void SendTransformMsg()
	{
		if( mNeedReplyTransMsg )
		{
			Vector3 pos = Vector3.zero;
			Quaternion rot = Quaternion.identity;
			Vector3 scl = Vector3.zero;

			bool setEuler = false;
			if ( mDataType == AGE.TransformDataType.World_Rotation_EulerAngle ||
			    mDataType == AGE.TransformDataType.Local_Rotation_EulerAngle ||
			    mDataType == AGE.TransformDataType.Coordinated_Rotation_EulerAngle ||
			    mDataType == AGE.TransformDataType.Relative_Rotation_EulerAngle )
				setEuler = true;
			
			switch(mDataType)
			{
			case AGE.TransformDataType.World_Position:
			{
				if (GetWorldTransform(ref pos, ref rot, ref scl))
					SendTransformReplyMsg(pos, rot, scl, true, false, false, setEuler);
				break;
			}
				
			case AGE.TransformDataType.Local_Position:
			case AGE.TransformDataType.Coordinated_Position:
			{
				if (GetLocalTransform(ref pos, ref rot, ref scl))
					SendTransformReplyMsg(pos, rot, scl, true, false, false, setEuler);
				break;
			}
				
			case AGE.TransformDataType.Relative_Position:
			case AGE.TransformDataType.Normalized_Relative_Position:
			{
				if (GetRelativeTransform(ref pos, ref rot, ref scl))
					SendTransformReplyMsg(pos, rot, scl, true, false, false, setEuler);
				break;
			}
				
			case AGE.TransformDataType.Scaling:
			{
				if (Selection.activeGameObject != null)
					SendTransformReplyMsg(pos, rot, Selection.activeTransform.localScale, false, false, true, setEuler);
				break;
			}
				
			case AGE.TransformDataType.World_Rotation:
			case AGE.TransformDataType.World_Rotation_EulerAngle:
			{
				if (GetWorldTransform(ref pos, ref rot, ref scl))
					SendTransformReplyMsg(pos, rot, scl, false, true, false, setEuler);
				break;
			}
				
			case AGE.TransformDataType.Local_Rotation:
			case AGE.TransformDataType.Coordinated_Rotation:
			case AGE.TransformDataType.Local_Rotation_EulerAngle:
			case AGE.TransformDataType.Coordinated_Rotation_EulerAngle:
			{
				if (GetLocalTransform(ref pos, ref rot, ref scl))
					SendTransformReplyMsg(pos, rot, scl, false, true, false, setEuler);
				break;
			}
				
			case AGE.TransformDataType.Relative_Rotation:
			case AGE.TransformDataType.Relative_Rotation_EulerAngle:
			{
				if (GetRelativeTransform(ref pos, ref rot, ref scl))
					SendTransformReplyMsg(pos, rot, scl, false, true, false, setEuler);
				break;
			}
			}
		}
	}

	void SendTransformReplyMsg(Vector3 pos, Quaternion rot, Vector3 scale, bool bSetPos, bool bSetRot, bool bSetScale, bool bSetEuler)
	{
		AGE.EditorMessage msg = new AGE.EditorMessage();
		msg.type = AGE.MessageType.Transform_Reply;
		msg.replyMsg = new AGE.ReplyMsg();
		msg.replyMsg.transformMsg = new AGE.TransformReplyMsg();
		msg.replyMsg.transformMsg.ctrlID = mCtrlID;
		msg.replyMsg.transformMsg.dataType = mDataType;
		AGE.TransformMsg trnmsg = new AGE.TransformMsg();
		msg.replyMsg.transformMsg.dataValue = trnmsg;
		if( bSetPos )
		{
			trnmsg.position = new AGE.Vector3Msg();
			trnmsg.position.Set( pos.x, pos.y, pos.z );
		}
		if( bSetRot )
		{
			trnmsg.rotation = new AGE.Vector4Msg();
			trnmsg.rotation.Set( rot.x, rot.y, rot.z, rot.w );
		}
		if( bSetScale )
		{
			trnmsg.scale = new AGE.Vector3Msg();
			trnmsg.scale.Set(scale.x, scale.y, scale.z);
		}
		if (bSetEuler)
		{
			trnmsg.rotation_eulerangle = new AGE.Vector3Msg();
			Vector3 euler = rot.eulerAngles;
			trnmsg.rotation_eulerangle.Set (euler.x, euler.y, euler.z);
		}
		AGE.EditorNetClient.GetInstance().SendMessageToServer(msg);
		mNeedReplyTransMsg = false;
	}
	
	void OnGUI()
	{

	}
	
	bool GetWorldTransform(ref Vector3 _pos, ref Quaternion _rot, ref Vector3 _scl)
	{
		if (Selection.activeGameObject == null) return false;
		_pos = Selection.activeTransform.position;
		_rot = Selection.activeTransform.rotation;
		_scl = Selection.activeTransform.localScale;
		return true;
	}
	
	bool GetLocalTransform(ref Vector3 _pos, ref Quaternion _rot, ref Vector3 _scl)
	{
		if (Selection.activeGameObject == null) return false;
		if (coordinatedLocal && coordTransform == null) return false;
		
		if (coordinatedLocal)
		{
			_pos = coordTransform.InverseTransformPoint(Selection.activeTransform.position);
			_rot = Quaternion.Inverse(coordTransform.rotation) * Selection.activeTransform.rotation;
		}
		else
		{
			_pos = Selection.activeTransform.localPosition;
			_rot = Selection.activeTransform.localRotation;
		}
		_scl = Selection.activeTransform.localScale;
		return true;
	}
	
	bool GetRelativeTransform(ref Vector3 _pos, ref Quaternion _rot, ref Vector3 _scl)
	{
		if (Selection.activeGameObject == null) return false;
		if (normalizedRelative && (fromTransform == null || toTransform == null)) return false;
		
		Vector3 lookDir = toTransform.position - fromTransform.position;
		lookDir = new Vector3(lookDir.x*axisWeight.x, lookDir.y*axisWeight.y, lookDir.z*axisWeight.z);
		float length = (new Vector2(lookDir.x, lookDir.z)).magnitude;
		lookDir = Vector3.Normalize(lookDir);
		Quaternion invLookRotation = Quaternion.Inverse(Quaternion.LookRotation(lookDir, Vector3.up));
		Vector3 relativePos = Selection.activeTransform.position - fromTransform.position;
		if (normalizedRelative)
		{
			_pos = invLookRotation * (Selection.activeTransform.position - fromTransform.position);
			_pos = new Vector3(_pos.x / length, _pos.y, _pos.z / length);
			_pos -= new Vector3(0, 1, 0) * _pos.z * (toTransform.position.y - fromTransform.position.y);
		}
		else
		{
			_pos = invLookRotation * (Selection.activeTransform.position - fromTransform.position);
			_pos -= new Vector3(0, 1, 0) * (_pos.z / length) * (toTransform.position.y - fromTransform.position.y);
		}
		_rot = invLookRotation * Selection.activeTransform.rotation;
		_scl = Selection.activeTransform.localScale;
		
		return true;
	}
	
	void PrintTransform(Vector3 _pos, Quaternion _rot, Vector3 _scl)
	{
		AgeLogger.Log
			("\t\t<Vector3 name=\"translation\" " + 
			 "x=\"" + _pos.x + 
			 "\" y=\"" + _pos.y + 
			 "\" z=\"" + _pos.z + 
			 "\"/>\n\t\t<Quaternion name=\"rotation\" " +
			 "x=\"" + _rot.x +
			 "\" y=\"" + _rot.y +
			 "\" z=\"" + _rot.z +
			 "\" w=\"" + _rot.w +
			 "\"/>\n\t\t<Vector3 name=\"scaling\" " +
			 "x=\"" + _scl.x +
			 "\" y=\"" + _scl.y +
			 "\" z=\"" + _scl.z +
			 "\"/>"
			 );
	}
}
}
