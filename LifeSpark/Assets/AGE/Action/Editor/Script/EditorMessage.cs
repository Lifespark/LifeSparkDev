//==================================================================================================
// File      : EditorMessage
// Brief     : Editor Message defined by protobuf
// Create    : 2014-01-15
// Modify    : 2014-01-15
// Author    : figolai <figolai@tencent.com>
// Copyright : (C) 2014 Tencent Engine Technology Center, all rights reserved.
// Website   : http://www.tencent.com
//==================================================================================================

using ProtoBuf;
using System.Collections;

namespace AGE{

	[ProtoContract]
	public enum TransformDataType {
		None					= 0,
		World_Position			= 1,
		Local_Position			= 2,
		Coordinated_Position	= 3,
		Relative_Position		= 4,
		Normalized_Relative_Position = 5,
		Scaling					= 6,
		World_Rotation			= 7,
		Local_Rotation			= 8,
		Coordinated_Rotation	= 9,
		Relative_Rotation		= 10,
		World_Rotation_EulerAngle		= 11,
		Local_Rotation_EulerAngle		= 12,
		Coordinated_Rotation_EulerAngle	= 13,
		Relative_Rotation_EulerAngle	= 14,
	}
		
	[ProtoContract]
	public enum MessageType {
		Unknow_Type        		= 0,
		GameObject_Request 		= 1,
		GameObject_Reply   		= 2,
		Transform_Request  		= 9,  //Translation, Rotation, Orientation
		Transform_Reply    		= 10,
		String_Request     		= 17,
		String_Reply       		= 18,
		ActionState_Request    	= 19,
		ActionState_Reply      	= 20,
		Tracks_Request			= 21,
		Tracks_Reply			= 22,
		CurvlData_Request		= 23,
		CurvlData_Reply			= 24,
		EventTimePos_Request    = 25,
		EventTimePos_Reply      = 26,
		MoveEventPos_Request    = 27,
		MoveEventPos_Reply      = 28,

		HeartBeat_Command	  	= 200,
		Play_Command          	= 201,
		Stop_Command          	= 202,
		Pause_Command         	= 203,
		PlaybackSpeed_Command 	= 204, // X4, X2, X1, X0.5, X0.25	
		Playback_Command	  	= 205,
		StopServer_Command		= 206,
	}

	//------------------------------------------------

	[ProtoContract]
	public class GameObjectRequestMsg{
		[ProtoMember(1, IsRequired = true)]
		public string trackId;
		[ProtoMember(2)]
		public string objectType;
	}

	[ProtoContract]
	public class GameObjectReplyMsg{
		[ProtoMember(1, IsRequired = true)]
		public string trackId;
		[ProtoMember(2)]
		public string objectId;
	}

	//------------------------------------------------

	[ProtoContract]
	public class StringRequestMsg{
		[ProtoMember(1, IsRequired = true)]
		public long ctrlID;
	}
	
	[ProtoContract]
	public class StringReplyMsg{
		[ProtoMember(1, IsRequired = true)]
		public long ctrlID;
		[ProtoMember(2, IsRequired = true)]
		public string strValue;
	}

	//------------------------------------------------

	[ProtoContract]
	public class Vector2Msg{
		[ProtoMember(1, IsRequired = true)]
		public float x;
		[ProtoMember(2, IsRequired = true)]
		public float y;

		public void Set( float _x, float _y )
		{
			x = _x;
			y = _y;
		}
	}
	
	[ProtoContract]
	public class Vector3Msg{
		[ProtoMember(1, IsRequired = true)]
		public float x;
		[ProtoMember(2, IsRequired = true)]
		public float y;
		[ProtoMember(3, IsRequired = true)]
		public float z;

		public void Set( float _x, float _y, float _z )
		{
			x = _x;
			y = _y;
			z = _z;
		}
	}
	
	[ProtoContract]
	public class Vector4Msg{
		[ProtoMember(1, IsRequired = true)]
		public float x;
		[ProtoMember(2, IsRequired = true)]
		public float y;
		[ProtoMember(3, IsRequired = true)]
		public float z;
		[ProtoMember(4, IsRequired = true)]
		public float w;

		public void Set( float _x, float _y, float _z, float _w )
		{
			x = _x;
			y = _y;
			z = _z;
			w = _w;
		}
	}
	
	[ProtoContract]
	public class TransformMsg{
		[ProtoMember(1)]
		public Vector3Msg position;
		[ProtoMember(2)]
		public Vector4Msg rotation;
		[ProtoMember(3)]
		public Vector3Msg rotation_eulerangle;
		[ProtoMember(4)]
		public Vector3Msg scale;
		[ProtoMember(5)]
		public string     transformObj;
	}

	[ProtoContract]
	public class TransformRequestMsg{
		[ProtoMember(1, IsRequired = true)]
		public long ctrlID;
		[ProtoMember(2, IsRequired = true)]
		public TransformDataType dataType;
	}
	
	[ProtoContract]
	public class TransformReplyMsg{
		[ProtoMember(1, IsRequired = true)]
		public long ctrlID;
		[ProtoMember(2, IsRequired = true)]
		public TransformDataType dataType;
		[ProtoMember(3)]
		public TransformMsg dataValue;
	}

	[ProtoContract]
	public class ActionStateRequestMsg{
		[ProtoMember(1, IsRequired = true)]
		public string actionID;
		[ProtoMember(2, IsRequired = true)]
		public float progress;
		[ProtoMember(3, IsRequired = true)]
		public bool playing;
	}

	[ProtoContract]
	public class ActionStateReplyMsg{
		[ProtoMember(1, IsRequired = true)]
		public string actionID;
		[ProtoMember(2, IsRequired = true)]
		public bool  playing;
		[ProtoMember(3, IsRequired = true)]
		public float progress;
		[ProtoMember(4, IsRequired = true)]
		public float timeLength;
	}

	[ProtoContract]
	public class TracksRequestMsg{
		[ProtoMember(1, IsRequired = true)]
		public int reqState; // 0: start; 1: not finished; 1: finished
		[ProtoMember(2, IsRequired = true)]
		public int index;
		[ProtoMember(3, IsRequired = true)]
		public string trackName;
		[ProtoMember(4, IsRequired = true)]
		public bool needClear;
	}

	[ProtoContract]
	public class TracksReplyMsg{
	}

	[ProtoContract]
	public class CurvlDataRequestMsg{
	}

	[ProtoContract]
	public class CurvlDataReplyMsg{
		[ProtoMember(1, IsRequired = true)]
		public int 			reqState; // 0: start; 1: not finished; 1: finished
		[ProtoMember(2, IsRequired = true)]
		public int 			trackIndex;
		[ProtoMember(3, IsRequired = true)]
		public int 			eventIndex;
		[ProtoMember(4, IsRequired = true)]
		public Vector3Msg 	position;
		[ProtoMember(5, IsRequired = true)]
		public Vector3Msg 	rotation_eulerangle;
		[ProtoMember(6, IsRequired = true)]
		public Vector3Msg 	scale;
		[ProtoMember(7, IsRequired = true)]
		public int 			dataModifyType; // eMT_NoOperate = 0, eMT_OnlyApplyData,	eMT_ReCreateTrack, eMT_AddNewTrack,
		[ProtoMember(8, IsRequired = true)]
		public Vector3Msg 	color;
		[ProtoMember(9, IsRequired = true)]
		public int 			transCalType; // 0: no translate
		[ProtoMember(10, IsRequired = true)]
		public int 			rotatCalType; // 0: no rotate
		[ProtoMember(11, IsRequired = true)]
		public int 			scaleCalType; // 0: no scale
		[ProtoMember(12, IsRequired = true)]
		public Vector4Msg 	rotation;
		[ProtoMember(13, IsRequired = true)]
		public bool			isCubic;
	}

	[ProtoContract]
	public class EventTimePosRequestMsg{
	}

	[ProtoContract]
	public class EventTimePosReplyMsg{
		[ProtoMember(1, IsRequired = true)]
		public int 			reqState; // 0: start; 1: not finished; 1: finished
		[ProtoMember(2, IsRequired = true)]
		public int 			trackIndex;
		[ProtoMember(3, IsRequired = true)]
		public int 			eventIndex;
		[ProtoMember(4, IsRequired = true)]
		public float 		position;
	}

	[ProtoContract]
	public class MoveEventPosRequestMsg{
	}
	
	[ProtoContract]
	public class MoveEventPosReplyMsg{
		[ProtoMember(1, IsRequired = true)]
		public int 			trackIndex;
		[ProtoMember(2, IsRequired = true)]
		public float 		offset;
	}
	//------------------------------------------------

	[ProtoContract]
	public class HeartBeatCommandMsg{
		[ProtoMember(1)]
		public float 				timeStamp;
	}

	//  Command Msg
	[ProtoContract]
	public class PlayCommandMsg{
		[ProtoMember(1)]
		public  string 				actionId; 
	}
	
	[ProtoContract]
	public class StopCommandMsg{
		[ProtoMember(1)]
		public  string 				actionId; 
	}
	
	[ProtoContract]
	public class PauseCommandMsg{
		[ProtoMember(1)]
		public  string 				actionId; 
	}
	
	[ProtoContract]
	public class PlaySpeedCommandMsg{
		[ProtoMember(1)]
		public  string 				actionId;
		[ProtoMember(2)]
		public  float  				speed;
	}

	[ProtoContract]
	public class PlaybackCommandMsg{
		[ProtoMember(1)]
		public  string 				actionId; 
	}

	[ProtoContract]
	public class StopServerCommandMsg{
	}

	[ProtoContract]
	public class RequestMsg {
		[ProtoMember(1)]
		public GameObjectRequestMsg gameObjectMsg;
		[ProtoMember(2)]
		public StringRequestMsg 	stringMsg;
		[ProtoMember(3)]
		public TransformRequestMsg 	transformMsg;
		[ProtoMember(4)]
		public ActionStateRequestMsg actionStateMsg;
		[ProtoMember(5)]
		public TracksRequestMsg 	tracksMsg;
		[ProtoMember(6)]
		public CurvlDataRequestMsg	curvlDataMsg;
		[ProtoMember(7)]
		public EventTimePosRequestMsg eventTimePosMsg;
		[ProtoMember(8)]
		public MoveEventPosRequestMsg moveEventPosMsg;
	}

	[ProtoContract]
	public class ReplyMsg {
		[ProtoMember(1)]
		public GameObjectReplyMsg   gameObjectMsg;
		[ProtoMember(2)]
		public StringReplyMsg 		stringMsg; 
		[ProtoMember(3)]
		public TransformReplyMsg 	transformMsg;
		[ProtoMember(4)]
		public ActionStateReplyMsg  actionStateMsg;
		[ProtoMember(5)]
		public TracksReplyMsg 		tracksMsg;
		[ProtoMember(6)]
		public CurvlDataReplyMsg	curvlDataMsg;
		[ProtoMember(7)]
		public EventTimePosReplyMsg eventTimePosMsg;
		[ProtoMember(8)]
		public MoveEventPosReplyMsg moveEventPosMsg;
	}

	[ProtoContract]
	public class CommandMsg {
//		[ProtoMember(1)]
//		public string msgDescCmd;
		[ProtoMember(1)]
		public HeartBeatCommandMsg 	heartbeatMsg;
		[ProtoMember(2)]
		public PlayCommandMsg		playMsg;
		[ProtoMember(3)]
		public StopCommandMsg 		stopMsg;
		[ProtoMember(4)]
		public PauseCommandMsg		pauseMsg;
		[ProtoMember(5)]
		public PlaySpeedCommandMsg  playSpdMsg;
		[ProtoMember(6)]
		public PlaybackCommandMsg   playbackMsg;
		[ProtoMember(7)]
		public StopServerCommandMsg stopserverMsg;

	}

	[ProtoContract]
	public class EditorMessage {
		//[ProtoMember(1), DefaultValue(MessageType.Unknow_Type)]
		[ProtoMember(1, IsRequired = true)]
		public MessageType 			type;

		[ProtoMember(2)]
		public RequestMsg  			requestMsg;
		[ProtoMember(3)]
		public ReplyMsg    			replyMsg;
		[ProtoMember(4)]
		public CommandMsg  			commandMsg;

	}

}
