using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace AGE
{
	public class TransInfo
	{
		public Vector3 pos;
		public Quaternion rot;

		public TransInfo()
		{
		}
	}

	public class ActionPreviewPanel : EditorWindow
	{
		public Rect mRect = new Rect(0, 0, 300, 300);
		
		public void ResetSize( float w, float h )
		{
			mRect.Set( mRect.x, mRect.y, w, h );
		}

		public GameObject helperObject = null;
		public ActionHelper actionHelper = null;
		public int helperIndex = 0;
		public float playProgress = 0.0f;
		public Action action = null;
		public ActionManager actionManager = null;
		public bool isPlaying = false;
		public bool playFinished = false;

		FileSystemWatcher watcher = null;
		bool reloadAction = false;

		float lastActionLength = 0.0f;
		string lastActionName = "";

		float lastRealTime = 0.0f;

		CurvlHelper mCurvlHelper = null;
		int[] mCurSelCurvls = null;
		//float[] mCurActionSimulateKeyTimes = null;
		Dictionary<Track, List<float>> mCurActionTrackKeyTimes = new Dictionary<Track, List<float>>();
		float mCurSimulateTime = 0.0f;

		public ActionPreviewPanel instance = null;

		public Dictionary<int, TransInfo> mInitGameObjTransInfo;




		public static GUILayoutOption[] GetLayoutOptions( float width, float height )
		{
			return new GUILayoutOption[]{GUILayout.Width(width), GUILayout.Height(height)};
		}
		
		public void Draw()
		{
			bool updateload = false;
			GUI.BeginGroup( mRect );
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("ActionHelper", GUILayout.Width(80));
			helperObject = EditorGUILayout.ObjectField(helperObject, typeof(GameObject), true) as GameObject;

			if (helperObject)
			{
				actionHelper = helperObject.GetComponent<ActionHelper>();
				if (actionHelper == null)
				{
					EditorGUILayout.EndHorizontal();
					GUI.EndGroup();
					return;
				}

				GUIContent[] contents = new GUIContent[actionHelper.actionHelpers.Length];

				for (int i=0; i<actionHelper.actionHelpers.Length; i++)
					contents[i] = new GUIContent(actionHelper.actionHelpers[i].helperName);

				if (helperIndex >= actionHelper.actionHelpers.Length)
					helperIndex = 0;
				int newHelperIndex = EditorGUILayout.Popup(helperIndex, contents);
				if (newHelperIndex != helperIndex && action != null)
				{
					StopAction();
					helperIndex = newHelperIndex;
				}
				else
				{
					helperIndex = newHelperIndex;
				}
			}

			EditorGUILayout.EndHorizontal();

			if (helperObject != null && (actionHelper=helperObject.GetComponent<ActionHelper>())!=null &&
			    actionHelper.actionHelpers.Length > 0)
			{
				actionHelper.ForceStart();

				EditorGUILayout.BeginHorizontal();

				if (GUILayout.Button(isPlaying ? "||" : "[>", GetLayoutOptions(50, 20))) 
				{
					if(playFinished)
					{
						Play(playProgress, 0.0f);
						ReplyActionState();
					}
					PlayAction();
				}

				if (action != null)
				{
					if (GUILayout.Button("<<", GetLayoutOptions(50, 20)))
					{
						Play(playProgress, 0.0f);
						//RollBack();
						ReplyActionState();
					}

					float newPlayProgress = EditorGUILayout.Slider(playProgress, 0.0f, action.length, GUILayout.Height(20));
					if (Mathf.Abs(newPlayProgress-playProgress)>0.0001f)
					{
						Play(playProgress, newPlayProgress);
						ReplyActionState();
					}

					if (GUILayout.Button("[]", GetLayoutOptions(50, 20)))
						StopAction();
				}

				EditorGUILayout.EndHorizontal();
			}
			
			GUI.EndGroup();

			if( updateload && mCurvlHelper != null )
			{

			}
		}

		public void PlayTo( float newTimePos )
		{
			Play(playProgress, newTimePos);
		}

		public void Awake()
		{
			EditorApplication.playmodeStateChanged += PlaymodeCallback;
		}

		public void PlaymodeCallback()
		{
			StopAction();
		}

		float lastSyncTimePos = 0.0f;
		public void Update()
		{
			//reload modified action
			if (action != null && reloadAction)
			{
				float oldPlayProgress = playProgress;
				ActionManager.Instance.ForceReloadAction(action.name);
				action = actionHelper.PlayAction(actionHelper.actionHelpers[helperIndex].helperName);
				//RollBack();

				lastActionName = action.name;
				lastActionLength = action.length;

				RollBack();
				BuildSimulateKeyTimeList();
				SetInitTransInfoToGameObj();
				mCurvlHelper = BuildCurvlFromAction();
				RollBack();
				Play(0, oldPlayProgress);

				reloadAction = false;
			}

			if (action != null && isPlaying == true)
			{
				float newPlayProgress = playProgress + (Time.realtimeSinceStartup - lastRealTime) * action.PlaySpeed;
				playFinished = false;
				if (newPlayProgress > action.length)
				{
					if (action.loop)
					{
						newPlayProgress -= action.length;
					}
					else
					{
						newPlayProgress = action.length;
						playFinished = true;
					}
				}
				Play(playProgress, newPlayProgress);
				if (playFinished)
					isPlaying = false;
				if( Mathf.Abs(newPlayProgress - lastSyncTimePos) > 0.033f )
				{
					ReplyActionState();
					lastSyncTimePos = newPlayProgress;
				}
			}

			int[] curSelCurvls = CurvlHelper.GetSelectedCurvl();
			if( mCurSelCurvls == null && curSelCurvls != null )
				mCurSelCurvls = curSelCurvls;

			if( mCurvlHelper != null )
			{
				if( CheckArrayDiff(curSelCurvls, mCurSelCurvls) )
				{
					if( UploadCurvlDataToEditor(mCurSelCurvls) )
						mCurSelCurvls = curSelCurvls;
				}
			}
		}

		void ReplyActionState()
		{
			if( action == null || !NetclientSample.GetInstance().GetClient().IsConnected() )
				return;
			AGE.EditorMessage msg = new AGE.EditorMessage{
				type = AGE.MessageType.ActionState_Reply,
				replyMsg = new AGE.ReplyMsg{
					actionStateMsg = new AGE.ActionStateReplyMsg{
						actionID = action.actionName,
						playing = isPlaying,
						progress = playProgress,
						timeLength = lastActionLength,
					}
				}
			};
			NetclientSample.GetInstance().GetClient().SendMessageToServer(msg);
			//AgeLogger.Log( "reply playstate : " + lastActionName + ", " + isPlaying + ", " + playProgress );
		}
		
		public void RollBack()
		{
			//replay
			ParticleSystem[] particleSystems = FindObjectsOfType<ParticleSystem>() as ParticleSystem[];
			foreach (ParticleSystem particleSystem in particleSystems)
			{
				if (particleSystem && particleSystem.time > 0.0f)
				{
					if (action.gameObjects.Contains(particleSystem.gameObject))
					{
						particleSystem.Stop();
						particleSystem.time = 0.0f;
					}
					else
					{
						ActionManager.DestroyGameObject(particleSystem.gameObject);
					}
				}
			}

			if( action != null )
			{
				action.Stop();
				action = null;
			}
			
			playProgress = 0.0f;

			if( actionHelper != null )
			{
				action = actionHelper.PlayAction(actionHelper.actionHelpers[helperIndex].helperName);
				action.ForceStart();
				
				foreach (GameObject gameObject in action.gameObjects)
				{
					if (gameObject == null) continue;
					RootMotionHelper helper = gameObject.GetComponent<RootMotionHelper>();
					if (helper)	helper.ForceStart();
				}
			}
			if( action != null )
			{
				lastActionName = action.name;
				lastActionLength = action.length;
				SetInitTransInfoToGameObj();
			}

			ReplyActionState();
		}

		public void Play(float _oldProgress, float _newProgress)
		{
			bool bRollback = false;
			if (_newProgress < _oldProgress)
			{
				RollBack();
				_oldProgress = 0.0f;
				bRollback = true;

				if (_newProgress < 0.0001f)
					_newProgress = 0.0001f;
			}

			if( action != null )
			{
				action.ForceUpdate(_newProgress);
				playProgress = _newProgress;

				foreach (GameObject gameObject in action.gameObjects)
				{
					if (gameObject == null) continue;
					RootMotionHelper helper = gameObject.GetComponent<RootMotionHelper>();
					if (helper && helper.enabled) helper.ForceLateUpdate();
				}

//				ParticleSystem[] particleSystems = FindObjectsOfType<ParticleSystem>() as ParticleSystem[];
//				foreach (ParticleSystem particleSystem in particleSystems)
//					particleSystem.Simulate((_newProgress - _oldProgress)/action.PlaySpeed, false, false);
//
//				Animation[] animations = FindObjectsOfType<Animation>() as Animation[];
//				foreach(Animation animation in animations)
//				{
//					if (animation.playAutomatically && !action.gameObjects.Contains(animation.gameObject))
//					{
//						Transform transform = animation.transform;
//						LifeTimeHelper lifeTimeHelper = null;
//						while (lifeTimeHelper == null && transform )
//						{
//							lifeTimeHelper = transform.GetComponent<LifeTimeHelper>();
//							transform = transform.parent;
//						}
//						if (lifeTimeHelper != null && _newProgress >= lifeTimeHelper.startTime)
//							animation.gameObject.SampleAnimation(animation.clip, _newProgress - lifeTimeHelper.startTime);
//					}
//				}
				action.UpdateTempObjectForPreview(_oldProgress, _newProgress);

				lastRealTime = Time.realtimeSinceStartup;

				if( bRollback )
					ReplyActionState();
			}
		}

		public void PlayAction()
		{
			if (action != null) 
			{
				isPlaying = !isPlaying;
				lastRealTime = Time.realtimeSinceStartup;
				ReplyActionState();
				return;
			}
			EditorApplication.SaveScene(EditorApplication.currentScene, false);

			actionManager = ActionManager.Instance;
			if (actionManager == null)
				actionManager = GameObject.Find("ActionManager").GetComponent<ActionManager>();
			actionManager.ForceStart();

			if( actionHelper != null )
			{
				action = actionHelper.PlayAction(actionHelper.actionHelpers[helperIndex].helperName);
				action.ForceStart();

				//watch action change
				TextAsset textAsset = ActionManager.Instance.ResLoader.Load(actionHelper.actionHelpers[helperIndex].actionName) as TextAsset;
				if (textAsset != null)
				{
					string actionPath = UnityEditor.AssetDatabase.GetAssetPath(textAsset);
					watcher = new FileSystemWatcher();
					watcher.Path = Path.GetDirectoryName(actionPath);
					watcher.Filter = Path.GetFileName(actionPath);
					watcher.NotifyFilter = NotifyFilters.LastWrite;
					watcher.Changed += this.OnActionModified;
					watcher.EnableRaisingEvents = true;
				}

				instance = this;
				isPlaying = true;
				lastRealTime = Time.realtimeSinceStartup;

				foreach (GameObject gameObject in action.gameObjects)
				{
					if (gameObject == null)
						continue;
					RootMotionHelper helper = gameObject.GetComponent<RootMotionHelper>();
					if (helper)
						helper.ForceStart();
				}
			}

			if( action != null )
			{
				lastActionName = action.name;
				lastActionLength = action.length;
				BuildInitGameObjTransInfo();
				BuildSimulateKeyTimeList();
				mCurvlHelper = BuildCurvlFromAction();
				RollBack();
			}
		}

		public void StopAction()
		{
			if (action == null) 
			{
				ReplyActionState();
				return;
			}

			string objName = helperObject.name;
			int oldHelperIndex = helperIndex;

			action.Stop();
			action = null;
			instance = null;
			isPlaying = false;
			helperIndex = 0;

			actionManager.ForceStop();

			watcher.EnableRaisingEvents = false;
			watcher.Changed -= this.OnActionModified;

			playProgress = 0.0f;

			EditorApplication.OpenScene(EditorApplication.currentScene);

			helperObject = GameObject.Find(objName);
			helperIndex = oldHelperIndex;

			ReplyActionState();
			mCurvlHelper = null;
		}

		void OnActionModified(object _source, FileSystemEventArgs _e)
		{
			reloadAction = true;
			AgeLogger.Log("Previewing action has been modified.");
		}


		bool UploadCurvlDataToEditor(int[] selCurvls)
		{
			if( action == null || mCurvlHelper == null || selCurvls == null || selCurvls.Length == 0 )
				return false;

			float lastPlayProgress = playProgress;
			List<TempData> resList = new List<TempData>();
			UpdateTracks(ref resList, ref selCurvls);
			//PreSimulate(lastPlayProgress, false);
			Play( mCurSimulateTime, lastPlayProgress );
			
			for( int i = 0; i < resList.Count; ++i )
			{
				TempData data = resList[i];
				int state = 0;
				if( i == resList.Count-1 )
					state = 2;
				else if( i > 0 )
					state = 1;
				AGE.EditorMessage msg = new AGE.EditorMessage();
				msg.type = AGE.MessageType.CurvlData_Reply;
				msg.replyMsg = new AGE.ReplyMsg();
				msg.replyMsg.curvlDataMsg = new AGE.CurvlDataReplyMsg();
				msg.replyMsg.curvlDataMsg.color = new AGE.Vector3Msg();
				msg.replyMsg.curvlDataMsg.color.x = data.evt.track.color.r;
				msg.replyMsg.curvlDataMsg.color.y = data.evt.track.color.g;
				msg.replyMsg.curvlDataMsg.color.z = data.evt.track.color.b;
				msg.replyMsg.curvlDataMsg.dataModifyType = (int)CurvlEditPanel.ModifyType.eMT_OnlyApplyData;
				msg.replyMsg.curvlDataMsg.eventIndex = data.nodeID;
				msg.replyMsg.curvlDataMsg.position = new AGE.Vector3Msg();
				msg.replyMsg.curvlDataMsg.position.x = data.pos.x;
				msg.replyMsg.curvlDataMsg.position.y = data.pos.y;
				msg.replyMsg.curvlDataMsg.position.z = data.pos.z;
				msg.replyMsg.curvlDataMsg.reqState = state;
				msg.replyMsg.curvlDataMsg.rotation = new AGE.Vector4Msg();
				msg.replyMsg.curvlDataMsg.rotation.x = data.rot.x;
				msg.replyMsg.curvlDataMsg.rotation.y = data.rot.y;
				msg.replyMsg.curvlDataMsg.rotation.z = data.rot.z;
				msg.replyMsg.curvlDataMsg.rotation.w = data.rot.w;
				msg.replyMsg.curvlDataMsg.rotation_eulerangle = new AGE.Vector3Msg();
				msg.replyMsg.curvlDataMsg.rotation_eulerangle.x = data.rot.eulerAngles.x;
				msg.replyMsg.curvlDataMsg.rotation_eulerangle.y = data.rot.eulerAngles.y;
				msg.replyMsg.curvlDataMsg.rotation_eulerangle.z = data.rot.eulerAngles.z;
				msg.replyMsg.curvlDataMsg.scale = new AGE.Vector3Msg();
				msg.replyMsg.curvlDataMsg.scale.x = data.scl.x;
				msg.replyMsg.curvlDataMsg.scale.y = data.scl.y;
				msg.replyMsg.curvlDataMsg.scale.z = data.scl.z;
				msg.replyMsg.curvlDataMsg.trackIndex = data.trackID;
				msg.replyMsg.curvlDataMsg.transCalType = 0;
				msg.replyMsg.curvlDataMsg.rotatCalType = 0;
				msg.replyMsg.curvlDataMsg.scaleCalType = 0;
				msg.replyMsg.curvlDataMsg.isCubic = data.evt.cubic;
				AGE.EditorNetClient.GetInstance().SendMessageToServer(msg);
			}

			return true;
		}

		void UpdateTracks(ref List<TempData> datalist, ref int[] selCurvls)
		{
			datalist.Clear();
			RollBack();
			if( selCurvls != null )
			{
				for( int i = 0; i < selCurvls.Length; ++i )
				{
					CurvlData cd = mCurvlHelper.GetCurvl(selCurvls[i]);
					CollectUpdatedNodes( ref datalist, cd );
				}
			}
			else
			{
				foreach( CurvlData cd in mCurvlHelper.mCurvlTrackMap.Keys )
				{
					CollectUpdatedNodes( ref datalist, cd );
				}
			}
		}
		
		void CollectUpdatedNodes(ref List<TempData> datalist, CurvlData cd)
		{
			Track track = mCurvlHelper.mCurvlTrackMap[cd];
			int count = track.GetEventsCount();
			if( count < 1 )
				return;

			int lastUpdateIndex = -1;
			for( int i = 0; i < count; ++i )
			{
				ModifyTransform evt = (ModifyTransform)track.GetEvent( i );
				TransNode node = cd.GetNode(i);
				Vector3 npos = node.pos;
				Quaternion nrot = node.rot;
				Vector3 nscl = node.scl;
				
				if( evt.HasDependObject(action) != 0 )
					PreSimulate( track, ref lastUpdateIndex, evt.time, false );	
				
				GameObject fromObject = action.GetGameObject(evt.fromId);
				GameObject toObject = action.GetGameObject(evt.toId);
				GameObject targetObject = cd.GetNodeObject(i);
				GameObject objectSpaceObject = action.GetGameObject(evt.objectSpaceId);
				Transform fromTransform = null;
				Transform toTransform = null;
				Transform coordTransform = null;
				Transform targetTransform = targetObject.transform;
				if( fromObject != null ) 		fromTransform = fromObject.transform;
				if( toObject != null ) 			toTransform = toObject.transform;
				if( objectSpaceObject != null ) coordTransform = objectSpaceObject.transform;
				bool res = CurvlHelper.CalTransform( evt, targetTransform, fromTransform, toTransform, coordTransform, ref npos, ref nrot, ref nscl );
				if( res )
				{
					TempData data = new TempData();
					data.evt = evt;
					data.pos = new Vector3( npos.x, npos.y, npos.z );
					data.rot = new Quaternion( nrot.x, nrot.y, nrot.z, nrot.w );
					data.scl = new Vector3( nscl.x, nscl.y, nscl.z );
					data.trackID = track.trackIndex;
					data.nodeID = i;
					datalist.Add( data );
				}
			}
		}

		void AddKeyTime( ref List<float> keyTimes, float time )
		{
			for( int i = 0; i < keyTimes.Count; ++i )
			{
				if( time < keyTimes[i] )
				{
					keyTimes.Insert( i, time );
					return;
				}
				if( time == keyTimes[i] )
					return;
			}
			keyTimes.Add( time );
		}

		void BuildInitGameObjTransInfo()
		{
			if( action == null )
				return;
			if( mInitGameObjTransInfo != null )
				mInitGameObjTransInfo.Clear();
			else
				mInitGameObjTransInfo = new Dictionary<int, TransInfo>();
			foreach( int id in action.TemplateObjectIds.Values )
			{
				GameObject obj = action.GetGameObject(id);
				if( obj != null )
				{
					TransInfo info = new TransInfo();
					info.pos = obj.transform.position;
					info.rot = obj.transform.rotation;
					mInitGameObjTransInfo.Add( id, info );
				}
			}
		}

		void SetInitTransInfoToGameObj()
		{
			if( mInitGameObjTransInfo != null )
			{
				foreach( int id in mInitGameObjTransInfo.Keys )
				{
					GameObject obj = action.GetGameObject(id);
					if( obj != null )
					{
						TransInfo info = mInitGameObjTransInfo[id];
						obj.transform.position = info.pos;
						obj.transform.rotation = info.rot;
					}
				}
			}
		}

		void BuildSimulateKeyTimeList()
		{
			if( mCurActionTrackKeyTimes != null )
			{
				foreach( List<float> l in mCurActionTrackKeyTimes.Values )
					l.Clear();
				mCurActionTrackKeyTimes.Clear();
			}
			else
				mCurActionTrackKeyTimes = new Dictionary<Track, List<float>>();

			ArrayList tracks = new ArrayList();
			action.GetTracks( typeof(ModifyTransform), ref tracks );
			foreach( Track track in tracks )
			{
				List<float> kts = new List<float>();
				int evtc = track.GetEventsCount();

				for( int i = 0; i < evtc; ++i )
				{
					AddKeyTime( ref kts, track.GetEvent(i).time );
				}

				mCurActionTrackKeyTimes.Add( track, kts );
			}
			mCurSimulateTime = -1.0f;
		}

		void PreSimulate( Track track, ref int lastUpdateIndex, float dstTime, bool needRebuildKeys )
		{
			float delta = 0.00001f;
			dstTime += delta;
			if( mCurActionTrackKeyTimes == null || needRebuildKeys )
				BuildSimulateKeyTimeList();
			if( mCurActionTrackKeyTimes != null && mCurActionTrackKeyTimes.ContainsKey(track) )
			{
				List<float> keyTimeList = mCurActionTrackKeyTimes[track];
				for( int i = lastUpdateIndex + 1; i < keyTimeList.Count; ++i )
				{
					if( keyTimeList[i] < dstTime )
					{
						Play(mCurSimulateTime, keyTimeList[i] + delta);
						mCurSimulateTime = keyTimeList[i] + delta;
						lastUpdateIndex = i;
					}
					else
					{
						Play(mCurSimulateTime, dstTime);
						mCurSimulateTime = dstTime;
						break;
					}
				}
			}
		}

		bool CheckArrayDiff( int[] a, int[] b )
		{
			if( a == null && b == null )
				return false;
			if( (a == null && b != null) || (a != null && b == null) || (a.Length != b.Length) )
				return true;
			for( int i = 0; i < a.Length; ++i )
			{
				bool notfind = true;
				for( int j = 0; j < b.Length; ++j )
				{
					if( a[i] == b[j] )
					{
						notfind = false;
						continue;
					}
				}
				if( notfind )
					return true;
			}
			return false;
		}

		public CurvlHelper BuildCurvlFromAction()
		{
			mCurSimulateTime = -1.0f;
			GameObject goCurvlHelper = GameObject.Find(CurvlHelper.sCurvlHelperObjName);
			CurvlHelper comp = null;
			if( goCurvlHelper == null )
			{
				goCurvlHelper = new GameObject(CurvlHelper.sCurvlHelperObjName);
				comp = goCurvlHelper.AddComponent<CurvlHelper>();
			}
			else
				comp = goCurvlHelper.GetComponent<CurvlHelper>();
			
			if( comp != null )
			{
				comp.RemoveAllCurvl(true);
				SetCurvlDataFromAction(ref comp);
			}
			return comp;
		}

		public void SetCurvlDataFromAction( ref CurvlHelper helper )
		{
			if( action == null )
				return;
			helper.mCurvlTrackMap.Clear();
			helper.mPreviewAction = action;
			helper.RemoveAllCurvl(true);
			ArrayList lst = new ArrayList();
			action.GetTracks( typeof(ModifyTransform), ref lst );
			// loop all tracks
			foreach( Track track in lst )
			{
				if( !track.enabled )
					continue;
				int evtCount = track.GetEventsCount();
				if( evtCount > 0 )
				{
					CurvlData cd = null;

					// create curvl
					cd = helper.CreateCurvl();
					cd.closeCurvl = action.loop;
					cd.lineColor = track.color;
					cd.displayName = track.trackName;
					helper.mCurvlTrackMap.Add( cd, track );

					int lastUpdateIndex = -1;
					// loop all ModifyTransform node
					for( int i = 0; i < evtCount; ++i )
					{
						ModifyTransform mt = (ModifyTransform)track.GetEvent(i);
						// if it depend on some object, should simulate the action to the time, and get the real object transform to calculate
						if( mt.HasDependObject(action) != 0 )
							PreSimulate( track, ref lastUpdateIndex, mt.time, false );
						TransNode node = cd.AddNode(false);
						node.isCubic = mt.cubic;

						node.pos = mt.GetTranslation(action);
						node.rot = mt.GetRotation(action);
						node.scl = mt.scaling;
					}
					cd.RefreshDataToGfx();

					RollBack();
					mCurSimulateTime = -1;
					SetInitTransInfoToGameObj();
				}
			}
		}
	}
}
