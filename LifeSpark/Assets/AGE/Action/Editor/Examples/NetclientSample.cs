using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


namespace AGE{

public class NetclientSample : AWidget
{

	static NetclientSample sWinInst = null;

	[@MenuItem ("AGEEditor/Network and Transform Panel")]
	public static void ShowWindow () 
	{
		if( sWinInst == null )
			sWinInst = (NetclientSample)EditorWindow.GetWindow(typeof(NetclientSample));
		sWinInst.Show();
	}

	public static NetclientSample GetInstance()
	{
		if( sWinInst == null )
			sWinInst = (NetclientSample)EditorWindow.GetWindow(typeof(NetclientSample));
		return sWinInst;
	}

	float 					mLastTryConnectedTimeStamp = 0f;
	float 					mLastHeartBeatTimeStamp = 0f;
	EditorNetClient 		mClient;
	Object          		mCurSelObj;
	string          		mObjLalbelName;
	AGE.MessageType 		mCurMsgType = AGE.MessageType.Unknow_Type;

	AGE.TransformDataType 	mTransDataType;
	long 					mCtrlID;

	TransformReqPanel		mTransformReqPanel;
	ObjectsMappingPanel		mObjectsMappingPanel;
	ActionPreviewPanel		mActionPreviewPanel;
	CurvlEditPanel			mCurvlEditPanel;
	bool					mAutoConnect = false;

	string[]				mTitles = {"TransformCal", "ActionPreview", "CurvlEdit" };
	int						mCurSel = 0;
	bool 					mIsLastPlaying = false;

	protected AScrollView   mScrollView = null;
	List<TrackInfo>			mTrackInfoList = new List<TrackInfo>();

	GUIStyle 				redStyle = null;
	GUIStyle				greenStyle = null;

	public EditorNetClient  GetClient()
	{
		return mClient;
	}

	void OnEnable()
	{
		base.OnEnable();
		mClient = AGE.EditorNetClient.GetInstance();
		mCurSelObj = null;
		mCurMsgType = AGE.MessageType.Unknow_Type;

		mTransformReqPanel = ScriptableObject.CreateInstance<TransformReqPanel>();
		mTransformReqPanel.mRect.Set( 0, 20, 500, 500 );

		mActionPreviewPanel = ScriptableObject.CreateInstance<ActionPreviewPanel>();
		mActionPreviewPanel.mRect.Set( 0, 20, 500, 500 );

		mScrollView = ScriptableObject.CreateInstance<AScrollView>();
		mScrollView.SetParent( this );
		mScrollView.InitUI( new Rect(0, 60, 500, 500), new Rect(0, 60, 500, 500), true, true );

		mCurvlEditPanel = ScriptableObject.CreateInstance<CurvlEditPanel>();
		mCurvlEditPanel.SetParent( mScrollView );
		mCurvlEditPanel.InitUI( new Rect(0, 0, 500, 500) );

		autoRepaintOnSceneChange = true;
	}

	void DrawGameObjectRequestPanel()
	{
		mCurSelObj = EditorGUI.ObjectField(new Rect(10, 110, 280, 20),
		                                   "CurrentSelected",
		                                   mCurSelObj,typeof(Object), true);
		
		// selected changed and send msg to server
		UpdateCurrentSelected();
		
		// send msg to server
		if (GUI.Button(new Rect(10, 150, 180, 20), "Send msg to host"))
		{
			if(mCurSelObj){
				EditorMessage msg = new EditorMessage{
					type = AGE.MessageType.GameObject_Reply,
					replyMsg = new AGE.ReplyMsg{
						gameObjectMsg = new AGE.GameObjectReplyMsg{
							trackId = "track1",
							objectId = mCurSelObj.name
						}
					}
				};
				
				mClient.SendMessageToServer(msg);
			}
		}
	}

	void DrawTransformRequestPanel()
	{
		if( mTransformReqPanel != null )
		{
			mTransformReqPanel.ResetSize( position.width, position.height );
			mTransformReqPanel.Draw();
		}
		if( mScrollView != null )
			mScrollView.SetParent(null);
	}

	void DrawActionPreviewPanel()
	{
		if( mActionPreviewPanel != null )
		{
			mActionPreviewPanel.ResetSize( position.width, position.height );
			mActionPreviewPanel.Draw();
		}
		if( mScrollView != null )
			mScrollView.SetParent(null);
	}

	void RefreshPanelSize( PanelBase panel )
	{
		if( panel.mRealHeight < position.height )
			panel.mRealHeight = position.height;
		panel.SetPanelSize( position.width, panel.mRealHeight );
		mScrollView.SetRealRectSize( position.width, panel.mRealHeight );
		mScrollView.SetViewRectSize( position.width, position.height );
	}
	
	void DrawCurvlEditPanel()
	{
		RefreshPanelSize(mCurvlEditPanel);
		if( mScrollView != null )
			mScrollView.SetParent(this);
		mScrollView.OnDraw();
	}


	void Update()
	{
		if( Application.isPlaying != mIsLastPlaying )
		{
			mIsLastPlaying = Application.isPlaying;
			mLastTryConnectedTimeStamp = mLastHeartBeatTimeStamp = Time.realtimeSinceStartup;
		}

		if( mClient != null )
		{
			if( mLastTryConnectedTimeStamp > Time.realtimeSinceStartup )
				mLastTryConnectedTimeStamp = Time.realtimeSinceStartup;
			if( mLastHeartBeatTimeStamp > Time.realtimeSinceStartup )
				mLastHeartBeatTimeStamp = Time.realtimeSinceStartup;
			// not connect to server
			if( !mClient.IsConnected() )
			{
				// if open auto connect, try connect to server every 3 seconds
				if( mAutoConnect && Time.realtimeSinceStartup - mLastTryConnectedTimeStamp > 3.0f )
				{
					//AgeLogger.Log( "try connect to server..." );
					mClient.Init();
					mLastTryConnectedTimeStamp = Time.realtimeSinceStartup;
					mLastHeartBeatTimeStamp = mLastTryConnectedTimeStamp;
				}
			}
			// if lose heartbeat msg over 10 seconds, it means server is shutdown 
			else if( Time.realtimeSinceStartup - mLastHeartBeatTimeStamp > 10.0f )
			{
				AgeLogger.Log( " lost heartbeat, disconnected..." );
				mClient.Closed();
			}
		}

		ProcessMessage();

		if( mActionPreviewPanel != null )
			mActionPreviewPanel.Update();
		if( mTransformReqPanel != null )
			mTransformReqPanel.Update();
		if( mCurvlEditPanel != null )
			mCurvlEditPanel.Update();
	}

	void OnGUI()
	{	
//		// The actual window code goes here
//		if (GUI.Button(new Rect(10, 10, 180, 20), "Connecting to host..."))
//		{
//			AgeLogger.Log(" start AGE network");
//			mClient.Init();
//		}
//		
//		if (GUI.Button(new Rect(10, 60, 180, 20), "Disconnecting from host"))
//		{
//			AgeLogger.Log(" stop AGE network");
//			mClient.Closed();
//		}

		if( redStyle == null )
		{
			redStyle = new GUIStyle();
			redStyle.normal.textColor = Color.red;
		}
		if( greenStyle == null )
		{
			greenStyle = new GUIStyle();
			greenStyle.normal.textColor = Color.green;
		}
		bool isConnected = mClient.IsConnected();
		EditorGUILayout.BeginHorizontal();
		bool lastState = mAutoConnect;
		mAutoConnect = GUILayout.Toggle( lastState, "Auto Connect Server" );
		GUILayout.Box( (isConnected ? "Connected!" : "Disconnected!"), (isConnected ? greenStyle : redStyle) );
		if( GUILayout.Button( "ReConnect" ) )
		{
			mClient.Closed();
			mClient.Init();
			mLastTryConnectedTimeStamp = Time.realtimeSinceStartup;
			mLastHeartBeatTimeStamp = mLastTryConnectedTimeStamp;
		}
		EditorGUILayout.EndHorizontal();

		PanelBase.DrawEmptyLine(1);
		EditorGUI.DrawRect( new Rect(0, 24, position.width, 2), (isConnected ? Color.green : Color.red) );

		EditorGUILayout.BeginHorizontal();
		mCurSel = GUILayout.Toolbar(mCurSel, mTitles);  
		EditorGUILayout.EndHorizontal();

		switch( mCurSel )
		{
		case 0:
			DrawTransformRequestPanel();
			break;
		case 1:
			DrawActionPreviewPanel();
			break;
		case 2:
			DrawCurvlEditPanel();
			break;
		}

		if( lastState != mAutoConnect && !isConnected && mAutoConnect && mClient != null )
		{
			mClient.Init();
			mLastTryConnectedTimeStamp = Time.realtimeSinceStartup;
			mLastHeartBeatTimeStamp = mLastTryConnectedTimeStamp;
		}
	}

	void ProcessMessage()
	{
		if( mClient == null || mClient.mMsgList == null )
			return;
		if( mClient.mLock )
			return;

		mClient.mLock = true;
		foreach (EditorMessage msg in mClient.mMsgList)
		{
			mCurMsgType = msg.type;
			switch( msg.type )
			{
				case AGE.MessageType.GameObject_Request:
				{
					string trackid = msg.requestMsg.gameObjectMsg.trackId;
					string objtype = msg.requestMsg.gameObjectMsg.objectType;
					//AgeLogger.Log(" Server request game object:trackid ="+ trackid + ",object Type:"+ objtype );
					SelectObject();
					break;
				}
				case AGE.MessageType.String_Request:
				{
					mCtrlID = msg.requestMsg.stringMsg.ctrlID;
					//AgeLogger.Log(" Server request string:ctrlID ="+ mCtrlID );
					ReplyStringRequest();
					break;
				}
				case AGE.MessageType.Transform_Request:
				{
					mCtrlID = msg.requestMsg.transformMsg.ctrlID;
					mTransDataType = msg.requestMsg.transformMsg.dataType;
					mTransformReqPanel.SetData( mTransDataType, mCtrlID );
					mTransformReqPanel.mNeedReplyTransMsg = true;
					mTransformReqPanel.SendTransformMsg();
					//AgeLogger.Log(" Server request string:ctrlID = "+ mCtrlID + "dataType = " + mTransDataType );
					break;
				}
				case AGE.MessageType.HeartBeat_Command:
				{
					mLastHeartBeatTimeStamp = Time.realtimeSinceStartup;
					float time = msg.commandMsg.heartbeatMsg.timeStamp;
					//AgeLogger.Log( "heart beat at " + time );
					break;
				}
				case AGE.MessageType.Play_Command:
				{
					mActionPreviewPanel.PlayAction();
					break;
				}
				case AGE.MessageType.Pause_Command:
				{
					mActionPreviewPanel.PlayAction();
					break;
				}
				case AGE.MessageType.Playback_Command:
				{
					mActionPreviewPanel.RollBack();
					break;
				}
				case AGE.MessageType.Stop_Command:
				{
					mActionPreviewPanel.StopAction();
					break;
				}
				case AGE.MessageType.StopServer_Command:
				{
					if( mClient != null ) 
						mClient.Closed();
					break;
				}
				case AGE.MessageType.ActionState_Request:
				{
					float progress = msg.requestMsg.actionStateMsg.progress;
					bool playing = msg.requestMsg.actionStateMsg.playing;
					if( mActionPreviewPanel.action == null )
						mActionPreviewPanel.PlayAction();
					mActionPreviewPanel.isPlaying = playing;
					mActionPreviewPanel.Play( mActionPreviewPanel.playProgress, progress );
					mActionPreviewPanel.playProgress = progress;
					mActionPreviewPanel.Repaint();
					break;
				}
				case AGE.MessageType.Tracks_Request:
				{
					if( msg.requestMsg.tracksMsg.needClear == true )
						mTrackInfoList.Clear();
					TrackInfo info = new TrackInfo();
					info.index = msg.requestMsg.tracksMsg.index;
					info.name = msg.requestMsg.tracksMsg.trackName;
					mTrackInfoList.Add( info );
					if( msg.requestMsg.tracksMsg.reqState == 2 )
						mCurvlEditPanel.SetTrackInfoData( mTrackInfoList );
					break;
				}
			}
		}		
		mClient.mMsgList.Clear();
		mClient.mLock = false;
	}


	private void ReplyStringRequest()
	{
		string name = "";
		Object[] selection1 =  Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
		Object[] selection2 =  Selection.GetFiltered(typeof(GameObject), SelectionMode.TopLevel);
//		if( selection1.Length == 1 && selection1[0].GetType() != typeof(AnimationClip))
//		{
//			Object o = selection1[0];
//			string assetpath = AssetDatabase.GetAssetPath(o).ToLower();
//			
//			int id = assetpath.IndexOf("resources/");
//			
//			if( id != -1 )
//				name = assetpath.Substring( id + 10 );
//			else
//				name = o.name;
//		}
//		else if( selection2.Length == 1 )
//			name = selection2[0].name;
//		else if(selection1.Length == 1 && selection1[0].GetType() == typeof(AnimationClip))
//		{
//			name = selection1[0].name;
//		}
		if( selection1.Length == 1 )
		{
			if( Selection.activeObject != selection1[0] )
				name = Selection.activeObject.name;
			else
			{
				string assetpath = AssetDatabase.GetAssetPath(Selection.activeObject).ToLower();
				int id = assetpath.IndexOf("resources/");
				if( id != -1 )
					name = assetpath.Substring( id + 10 );
				else
					name = Selection.activeObject.name;
			}
		}
		else if( selection2.Length == 1 )
		{
			name = selection2[0].name;
		}

		//cut ext name
		if (name.LastIndexOf('.') >= 0)
			name = name.Substring(0, name.LastIndexOf('.'));
		if (name.LastIndexOf('@') >= 0)
		{
			string pre = name.Substring(0, name.LastIndexOf('@'));
			string post = name.Substring(name.LastIndexOf('@')+1);
			if (pre.LastIndexOf('/') >= 0)
				name = pre.Substring(0, pre.LastIndexOf('/')) + "/" + post;
			else
				name = post;
		}

		if( name.Length > 0 )
		{
			EditorMessage msg = new EditorMessage{
				type = AGE.MessageType.String_Reply,
				replyMsg = new AGE.ReplyMsg{
					stringMsg = new AGE.StringReplyMsg{
						ctrlID = mCtrlID,
						strValue = name
					}
				}
			};
			mClient.SendMessageToServer(msg);
		}
	}

	private void SelectObject()
	{
		EditorGUIUtility.ShowObjectPicker<GameObject> (null, true, "", -1);

		//EditorGUILayout.ObjectField (obj, typeof(Object), true);
	}

	private void UpdateCurrentSelected()
	{
		//Object obj = null;
		string commandName = Event.current.commandName;
		if (commandName == "ObjectSelectorUpdated") {
			mCurSelObj = EditorGUIUtility.GetObjectPickerObject();
			//AgeLogger.Log ("selected changed object :" + mCurSelObj.name);
			Repaint ();
		} else if (commandName == "ObjectSelectorClosed") {
			mCurSelObj = EditorGUIUtility.GetObjectPickerObject();
			//AgeLogger.Log ("selected closeed bject :" + mCurSelObj.name);
		}
		
		if(mCurSelObj){
			AgeLogger.Log (" selected object :" + mCurSelObj.name);
		}
	}


}
}
