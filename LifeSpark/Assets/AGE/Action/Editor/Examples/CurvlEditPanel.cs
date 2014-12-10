
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace AGE{

	public class TrackInfo
	{
		public string 		name;
		public int			index;
	}


	public class CurvlEditPanel : PanelBase
	{
		public enum ModifyType
		{
			eMT_NoOperate = 0,
			eMT_OnlyApplyData,
			eMT_ReCreateTrack,
			eMT_AddNewTrack,
		}

		public enum CoordType
		{
			eCT_World = 0,
			eCT_Local,
			eCT_ObjectSpace,
			eCT_Relative
		}

		public enum CloneOffsetType
		{
			eCOT_WorldUserDef = 0,
			eCOT_ObjUserDef,
			eCOT_ObjFront,
			eCOT_ObjBack,
			eCOT_ObjLeft,
			eCOT_ObjRight,
			eCOT_ObjUp,
			eCOT_ObjDown,
		}

		static Vector3 axisWeight = new Vector3(1, 0, 1);

		static GameObject  		updateDummy;
		static GameObject		realdummy;
		static float 			lastUpdateTime = 0.0f;
		static bool				createUpdateDummy = false;
		bool 			expandCurvl = true;
		bool 			showAllCurvl = true;
		GameObject		nodeprefab = null;

		int 			mCurvlSel = -1;
		GUIContent[] 	mCurvlContent;
		int 			mTrackSel = -1;
		GUIContent[] 	mTrackContent;
		GUIContent		mEmptyContent = new GUIContent();
		ModifyType 		mModifyType = ModifyType.eMT_NoOperate;
		CoordType		mCoordType = CoordType.eCT_World;
		bool 			mNormalizeRelative = false;

		bool 			mShowCurvlList = false;
		bool			mShowTrackList = false;
		bool 			mShowEditSubPanel = false;

		public List<TrackInfo>	mTrackInfoList = new List<TrackInfo>();


		Transform fromTransform = null;
		Transform toTransform = null;
		Transform coordTransform = null;
		public bool coordinatedLocal = false;
		public bool normalizedRelative = false;

		float startOffsetTime = 0.0f;
		float endOffsetTime = 0.0f;
		float actionTimeLength = 0.0f;

		CloneOffsetType cloneOffsetType = CloneOffsetType.eCOT_WorldUserDef;
		Vector3 cloneOffset = Vector3.zero;
		float cloneDistance = 1.0f;

		float eventTimeOffset = 0.0f;

		bool expandSelectCurvlAndTrack = false;
		bool expandUploadDataSetting = false;
		bool expandResetEventTimePos = false;
		bool expandOffsetEventTimePos = false;
		bool expandCurvlOperation = false;
		bool expandGenerateSetting = false;

		public void SetData( AGE.TransformDataType dt )
		{
			switch( dt )
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

		bool GetWorldTransform(Transform dstobj, ref Vector3 _pos, ref Quaternion _rot, ref Vector3 _scl)
		{
			if (dstobj == null) return false;
			_pos = dstobj.position;
			_rot = dstobj.rotation;
			_scl = dstobj.localScale;
			return true;
		}
		
		bool GetLocalTransform(Transform dstobj, ref Vector3 _pos, ref Quaternion _rot, ref Vector3 _scl)
		{
			if (dstobj == null) return false;
			if (coordinatedLocal && coordTransform == null) return false;
			
			if (coordinatedLocal)
			{
				_pos = coordTransform.InverseTransformPoint(dstobj.position);
				_rot = Quaternion.Inverse(coordTransform.rotation) * dstobj.rotation;
			}
			else
			{
				_pos = dstobj.localPosition;
				_rot = dstobj.localRotation;
			}
			_scl = dstobj.localScale;
			return true;
		}
		
		bool GetRelativeTransform(Transform dstobj, ref Vector3 _pos, ref Quaternion _rot, ref Vector3 _scl)
		{
			if (dstobj == null) return false;
			if (normalizedRelative && (fromTransform == null || toTransform == null)) return false;
			
			Vector3 lookDir = toTransform.position - fromTransform.position;
			lookDir = new Vector3(lookDir.x*axisWeight.x, lookDir.y*axisWeight.y, lookDir.z*axisWeight.z);
			float length = (new Vector2(lookDir.x, lookDir.z)).magnitude;
			lookDir = Vector3.Normalize(lookDir);
			Quaternion invLookRotation = Quaternion.Inverse(Quaternion.LookRotation(lookDir, Vector3.up));
			Vector3 relativePos = dstobj.position - fromTransform.position;
			if (normalizedRelative)
			{
				_pos = invLookRotation * (dstobj.position - fromTransform.position);
				_pos = new Vector3(_pos.x / length, _pos.y, _pos.z / length);
				_pos -= new Vector3(0, 1, 0) * _pos.z * (toTransform.position.y - fromTransform.position.y);
			}
			else
			{
				_pos = invLookRotation * (dstobj.position - fromTransform.position);
				_pos -= new Vector3(0, 1, 0) * (_pos.z / length) * (toTransform.position.y - fromTransform.position.y);
			}
			_rot = invLookRotation * dstobj.rotation;
			_scl = dstobj.localScale;
			
			return true;
		}

		public bool CalculateTransform(AGE.TransformDataType dt, Transform dstobj, ref Vector3 pos, ref Quaternion rot, ref Vector3 scl)
		{
			SetData( dt );
			bool calres = false;			
			switch(dt)
			{
				case AGE.TransformDataType.World_Position:
				{
					if (GetWorldTransform(dstobj, ref pos, ref rot, ref scl))
						calres = true;
					break;
				}
					
				case AGE.TransformDataType.Local_Position:
				case AGE.TransformDataType.Coordinated_Position:
				{
					if (GetLocalTransform(dstobj, ref pos, ref rot, ref scl))
						calres = true;
					break;
				}
					
				case AGE.TransformDataType.Relative_Position:
				case AGE.TransformDataType.Normalized_Relative_Position:
				{
					if (GetRelativeTransform(dstobj, ref pos, ref rot, ref scl))
						calres = true;
					break;
				}
					
				case AGE.TransformDataType.Scaling:
				{
					scl = dstobj.localScale;
					calres = true;
					break;
				}
					
				case AGE.TransformDataType.World_Rotation:
				case AGE.TransformDataType.World_Rotation_EulerAngle:
				{
					if (GetWorldTransform(dstobj, ref pos, ref rot, ref scl))
						calres = true;
					break;
				}
					
				case AGE.TransformDataType.Local_Rotation:
				case AGE.TransformDataType.Coordinated_Rotation:
				case AGE.TransformDataType.Local_Rotation_EulerAngle:
				case AGE.TransformDataType.Coordinated_Rotation_EulerAngle:
				{
					if (GetLocalTransform(dstobj, ref pos, ref rot, ref scl))
						calres = true;
					break;
				}
					
				case AGE.TransformDataType.Relative_Rotation:
				case AGE.TransformDataType.Relative_Rotation_EulerAngle:
				{
					if (GetRelativeTransform(dstobj, ref pos, ref rot, ref scl))
						calres = true;
					break;
				}
			}
			return calres;
		}

		bool CalNode( Transform node, int trackID, int nodeID, int modifyState, Vector3 color, bool cubic )
		{
			Vector3 finalpos = Vector3.zero;
			Quaternion finalrot = Quaternion.identity;
			Vector3 finalscl = Vector3.zero;
			Vector3 eulerAngles = Vector3.zero;

			Vector3 pos = Vector3.zero;
			Quaternion rot = Quaternion.identity;
			Vector3 scl = Vector3.zero;

			AGE.TransformDataType posDT = AGE.TransformDataType.None;//(AGE.TransformDataType)mPosCalType;
			AGE.TransformDataType rotDT = AGE.TransformDataType.None;//(AGE.TransformDataType)mRotCalType;
			AGE.TransformDataType sclDT = TransformDataType.Scaling;//(AGE.TransformDataType)mSclCalType;
			switch( mCoordType )
			{
				case CoordType.eCT_World:
				{
					posDT = TransformDataType.World_Position;
					rotDT = TransformDataType.World_Rotation_EulerAngle;
					break;
				}
				case CoordType.eCT_Local:
				{
					posDT = TransformDataType.Local_Position;
					rotDT = TransformDataType.Local_Rotation_EulerAngle;
					break;
				}
				case CoordType.eCT_ObjectSpace:
				{
					posDT = TransformDataType.Coordinated_Position;
					rotDT = TransformDataType.Coordinated_Rotation_EulerAngle;
					break;
				}
				case CoordType.eCT_Relative:
				{
					rotDT = TransformDataType.Relative_Rotation_EulerAngle;
					if( mNormalizeRelative )
						posDT = TransformDataType.Relative_Position;
					else
						posDT = TransformDataType.Normalized_Relative_Position;
					break;
				}
			}

			if( CalculateTransform(posDT, node, ref pos, ref rot, ref scl ) )
					finalpos = pos;
			if( CalculateTransform(rotDT, node, ref pos, ref rot, ref scl ) )
			{
				finalrot = rot;
				eulerAngles = rot.eulerAngles;
			}
			if( CalculateTransform(sclDT, node, ref pos, ref rot, ref scl ) )
					finalscl = scl;

			AGE.EditorMessage msg = new AGE.EditorMessage();
			msg.type = AGE.MessageType.CurvlData_Reply;
			msg.replyMsg = new AGE.ReplyMsg();
			msg.replyMsg.curvlDataMsg = new AGE.CurvlDataReplyMsg();
			msg.replyMsg.curvlDataMsg.color = new AGE.Vector3Msg();
			msg.replyMsg.curvlDataMsg.color.x = color.x;
			msg.replyMsg.curvlDataMsg.color.y = color.y;
			msg.replyMsg.curvlDataMsg.color.z = color.z;
			msg.replyMsg.curvlDataMsg.dataModifyType = (int)mModifyType;
			msg.replyMsg.curvlDataMsg.eventIndex = nodeID;
			msg.replyMsg.curvlDataMsg.trackIndex = trackID;
			msg.replyMsg.curvlDataMsg.position = new AGE.Vector3Msg();
			msg.replyMsg.curvlDataMsg.position.x = finalpos.x;
			msg.replyMsg.curvlDataMsg.position.y = finalpos.y;
			msg.replyMsg.curvlDataMsg.position.z = finalpos.z;
			msg.replyMsg.curvlDataMsg.reqState = modifyState;
			msg.replyMsg.curvlDataMsg.rotation = new AGE.Vector4Msg();
			msg.replyMsg.curvlDataMsg.rotation.x = finalrot.x;
			msg.replyMsg.curvlDataMsg.rotation.y = finalrot.y;
			msg.replyMsg.curvlDataMsg.rotation.z = finalrot.z;
			msg.replyMsg.curvlDataMsg.rotation.w = finalrot.w;
			msg.replyMsg.curvlDataMsg.rotation_eulerangle = new AGE.Vector3Msg();
			msg.replyMsg.curvlDataMsg.rotation_eulerangle.x = eulerAngles.x;
			msg.replyMsg.curvlDataMsg.rotation_eulerangle.y = eulerAngles.y;
			msg.replyMsg.curvlDataMsg.rotation_eulerangle.z = eulerAngles.z;
			msg.replyMsg.curvlDataMsg.scale = new AGE.Vector3Msg();
			msg.replyMsg.curvlDataMsg.scale.x = finalscl.x;
			msg.replyMsg.curvlDataMsg.scale.y = finalscl.y;
			msg.replyMsg.curvlDataMsg.scale.z = finalscl.z;
			msg.replyMsg.curvlDataMsg.rotatCalType = (int)rotDT;
			msg.replyMsg.curvlDataMsg.scaleCalType = (int)sclDT;
			msg.replyMsg.curvlDataMsg.transCalType = (int)posDT;
			msg.replyMsg.curvlDataMsg.isCubic = cubic;
			AGE.EditorNetClient.GetInstance().SendMessageToServer(msg);
			
			return true;
		}

		bool SendCurvlData()
		{
			if( mCurvlSel < 0 || mCurvlSel >= mCurvlContent.Length )
			{
				EditorUtility.DisplayDialog( "Warnning!", "Please select curvl first!", "OK" );
				return false;
			}
			GameObject goCurvlHelper = null;
			CurvlHelper comp = null;		
			CurvlData cc = null;
			goCurvlHelper = GameObject.Find(CurvlHelper.sCurvlHelperObjName);
			if( goCurvlHelper )
			{
				comp = goCurvlHelper.GetComponent<CurvlHelper>();
				if( comp )
					cc = comp.GetCurvl(mCurvlSel);
			}
			if( cc == null )
				return false;
			int trackID = -1;
			if( mTrackSel >= 0 && mTrackSel < mTrackInfoList.Count )
				trackID = mTrackInfoList[mTrackSel].index;

			int nodeCount = cc.nodeLst.Count;
			Vector3 col = new Vector3(cc.lineColor.r, cc.lineColor.g, cc.lineColor.b);
			for( int i = 0; i < nodeCount; ++i )
			{
				int state = 0;
				Transform node = cc.GetNodeObject(i).transform;
				TransNode tn = cc.nodeLst[i];
				if( i == nodeCount-1 )
					state = 2;
				else if( i > 0 )
					state = 1;
				CalNode( node, trackID, i, state, col, tn.isCubic );
			}
			return true;
		}

		protected void DrawSpace( int gapCount )
		{
			float spaceSize = gapCount * 20;
			GUILayout.Space( spaceSize );
		}

		protected bool DrawTreeExpandBtn( int level, string propertyName, string extFlag )
		{
			DrawSpace( level );
			string keyname = propertyName + extFlag;
			bool value = GetExpand( keyname );
			bool val = GUILayout.Button( GetExpandTxt(value), GetLayoutOptions(24,24) );
			GUILayout.Label( propertyName, GetLayoutOptions(-1f, 24) );
			if( val )
			{
				value = !value;
				SetExpand( keyname, value );
			}
			return value;
		}

		protected bool DrawTreeExpandBtnWithEditName( int index, int level, string propertyName, string extFlag, ref string lableName )
		{
			string displayname = lableName;
			DrawSpace( level );
			string keyname = propertyName + extFlag;
			bool value = GetExpand( keyname );
//			bool val = GUILayout.Button( GetExpandTxt(value), GetLayoutOptions(24,24) );
//			lableName = GUILayout.TextField( displayname, GetLayoutOptions(200f, 24) );
//			if( val )
//			{
//				value = !value;
//				SetExpand( keyname, value );
//			}
			value = EditorGUILayout.Foldout( value, index.ToString() );
			lableName = GUILayout.TextField( displayname, GetLayoutOptions(200f, 24) );
			SetExpand( keyname, value );
			return value;
		}

		protected bool DrawClickLable( int level, string propertyName, bool sel )
		{
			DrawSpace( level );
			bool click =  GUILayout.Button( (sel ? "S" : " "), GetLayoutOptions(24,24) );
			GUILayout.Label( propertyName, GetLayoutOptions(-1f, 24) );
			return click;
		}

		public static void CreateUpdateDummy()
		{
			string objname = "updateDummy";
			if( createUpdateDummy == false )
			{
				updateDummy = GameObject.Find( objname );
				if( updateDummy )
				{
					ActionManager.DestroyGameObject(updateDummy);
//					if( ActionManager.Instance != null && ActionManager.Instance.ResLoader != null )
//						ActionManager.Instance.ResLoader.DestroyObject(updateDummy);
//					else
//						GameObject.DestroyImmediate( updateDummy );
				}
				updateDummy = null;
				return;
			}

			if( realdummy == null )
			{
				updateDummy = GameObject.Find( objname );
				if( updateDummy == null )
				{
					updateDummy = new GameObject( objname );
					realdummy = new GameObject(objname);
					realdummy.transform.parent = updateDummy.transform;
					realdummy.SetActive(false);
				}
				else
				{
					realdummy = updateDummy.transform.GetChild(0).gameObject;
				}
			}
			if( lastUpdateTime > Time.realtimeSinceStartup )
				lastUpdateTime = Time.realtimeSinceStartup;
			if( Time.realtimeSinceStartup - lastUpdateTime > 0.2 )
			{
				lastUpdateTime = Time.realtimeSinceStartup;
				Camera comp = realdummy.GetComponent<Camera>();
				if( comp != null )
					Component.DestroyImmediate(comp);
				else
					realdummy.AddComponent<Camera>();
			}
		}

		public void Update()
		{
			CreateUpdateDummy();
		}

		public void Draw()
		{
	//		BeginDrawSelf();
	//		DrawChildren();
	//		EndDrawSelf();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			PrepData();
		}
		
		void PrepData()
		{
			List<GUIContent> cnts = new List<GUIContent>();

			GameObject goCurvlHelper = null;
			CurvlHelper comp = null;

			goCurvlHelper = GameObject.Find(CurvlHelper.sCurvlHelperObjName);
			if( goCurvlHelper )
			{
				comp = goCurvlHelper.GetComponent<CurvlHelper>();
				if( comp )
				{
					int count = comp.GetCurvlCount();
					for( int i = 0; i < count; ++i )
					{
						CurvlData cc = comp.GetCurvl(i);
						string curvlname = "Curvl_"+i;
						string displayname = curvlname;
						if( cc.displayName != null && cc.displayName.Length > 0 )
							displayname = cc.displayName;

						GUIContent newCont = new GUIContent(displayname);
						cnts.Add( newCont );
					}
				}
			}

			mCurvlContent = cnts.ToArray();
			if( mCurvlSel >= mCurvlContent.Length )
				mCurvlSel = -1;

			cnts.Clear();
			if( mTrackContent == null )
				mTrackContent = cnts.ToArray();
			if( mTrackSel >= mTrackContent.Length )
				mTrackSel = -1;
		}

		public void SetTrackInfoData( List<TrackInfo> srcLst )
		{
			mTrackInfoList.Clear();
			List<GUIContent> cnts = new List<GUIContent>();
			for( int i = 0; i < srcLst.Count; ++i )
			{
				TrackInfo info = srcLst[i];
				GUIContent newCont = new GUIContent(info.name);
				cnts.Add( newCont );
				mTrackInfoList.Add(info);
			}
			mTrackContent = cnts.ToArray();
			if( mTrackSel >= mTrackContent.Length )
				mTrackSel = -1;
		}

		protected override void BeginDrawSelf()
		{
			mRealHeight = 0f;
			int itemCount = 0;

			Event e = Event.current;
			bool pressCtrl = e.control;

			bool createCurvlEditGO = false;
			bool deleteCurvlEditGO = false;
			bool rebuildCurvl = false;

			bool addCurvl = false;
			bool delCurvl = false;
			bool insertCurvl = false;
			bool addNode = false;
			bool delNode = false;
			int curvelIDChg = -1;
			int nodeIDChg = -1;
			bool uploadData = false;
			bool requestTracks = false;

			bool resetTimePos = false;
			bool cloneCurvl = false;
			bool moveEventTimePos = false;


			GUIStyle titleStype = EditorStyles.boldLabel;
			int originFontSize = titleStype.fontSize;
			Color originColor = titleStype.normal.textColor;
			titleStype.normal.textColor = Color.yellow;
			//titleStype.fontSize = 16;

			GUI.BeginGroup( mRect, mContent);

			DrawEmptyLine(1);
			++itemCount;

			EditorGUILayout.BeginHorizontal();
			if( GUILayout.Button( (mShowEditSubPanel ? "Close Edit Tool Panel" : "Show Edit Tool Panel"), GetLayoutOptions(-1, 24) ) )
				mShowEditSubPanel = !mShowEditSubPanel;
			EditorGUILayout.EndHorizontal();
			++itemCount;

			if( mShowEditSubPanel )
			{
				if( mEmptyContent == null )
					mEmptyContent = new GUIContent();

				EditorGUILayout.BeginHorizontal();
				if( GUILayout.Button( GetExpandTxt(expandSelectCurvlAndTrack), GetLayoutOptions(24,24) ) )
					expandSelectCurvlAndTrack = !expandSelectCurvlAndTrack;
				GUILayout.Label ("Select Curvl and Track", titleStype);
				EditorGUILayout.EndHorizontal();
				++itemCount;

				if( expandSelectCurvlAndTrack )
				{
					EditorGUILayout.BeginHorizontal();
					bool click = GUILayout.Button( "Select Curvl", GetLayoutOptions(150, 24) );
					if( mCurvlSel >= 0 && mCurvlSel < mCurvlContent.Length )
						GUILayout.Label( mCurvlContent[mCurvlSel], GetLayoutOptions(-1, 24) );
					else
						GUILayout.Label( mEmptyContent, GetLayoutOptions(-1, 24) );
					if( click )
						mShowCurvlList = !mShowCurvlList;
					if( mShowCurvlList )
					{
						int newCurvlSel = GUILayout.SelectionGrid(mCurvlSel, mCurvlContent, 1, GetLayoutOptions(200, 24*mCurvlContent.Length));
						if( newCurvlSel != mCurvlSel )
						{
							mCurvlSel = newCurvlSel;
							mShowCurvlList = false;
						}
						itemCount += mCurvlContent.Length;
						if( mCurvlContent.Length == 0 )
							++itemCount;				
					}
					else
						++itemCount;
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.BeginHorizontal();
					click = GUILayout.Button( "Select Track", GetLayoutOptions(150, 24) );
					if( mTrackSel >= 0 && mTrackSel < mTrackContent.Length )
						GUILayout.Label( mTrackContent[mTrackSel], GetLayoutOptions(-1, 24) );
					else
						GUILayout.Label( mEmptyContent, GetLayoutOptions(-1, 24) );
					if( click )
					{
						mShowTrackList = !mShowTrackList;
						requestTracks = true;
					}
					if( mShowTrackList )
					{
						int newTrackSel = GUILayout.SelectionGrid(mTrackSel, mTrackContent, 1, GetLayoutOptions(200, 24*mTrackContent.Length));
						if( newTrackSel != mTrackSel )
						{
							mTrackSel = newTrackSel;
							mShowTrackList = false;
						}
						itemCount += mTrackContent.Length;
						if( mTrackContent.Length == 0 )
							++itemCount;			
					}
					else
						++itemCount;
					EditorGUILayout.EndHorizontal();
					
					DrawEmptyLine(1);
					++itemCount;
				}

				EditorGUILayout.BeginHorizontal();
				if( GUILayout.Button( GetExpandTxt(expandUploadDataSetting), GetLayoutOptions(24,24) ) )
					expandUploadDataSetting = !expandUploadDataSetting;
				GUILayout.Label ("Upload data setting", titleStype);
				EditorGUILayout.EndHorizontal();
				++itemCount;

				if( expandUploadDataSetting )
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label( "SelectCoordType: ", GetLayoutOptions(-1, 24) ); 
					mCoordType = (CoordType)EditorGUILayout.EnumPopup( mCoordType, GetLayoutOptions(200, 24) );
					EditorGUILayout.EndHorizontal();
					++itemCount;

					if( mCoordType == CoordType.eCT_Relative )
					{
						EditorGUILayout.BeginHorizontal();
						DrawSpace(1);
						GUILayout.Label ("Relative Space", EditorStyles.boldLabel, GetLayoutOptions(-1, 24));
						EditorGUILayout.EndHorizontal();
						++itemCount;

						EditorGUILayout.BeginHorizontal();
						DrawSpace(1);
						fromTransform = EditorGUILayout.ObjectField("From", fromTransform, typeof(Transform), true, GetLayoutOptions(-1, 24)) as Transform;
						EditorGUILayout.EndHorizontal();
						++itemCount;

						EditorGUILayout.BeginHorizontal();
						DrawSpace(1);
						toTransform = EditorGUILayout.ObjectField("To", toTransform, typeof(Transform), true, GetLayoutOptions(-1, 24)) as Transform;
						EditorGUILayout.EndHorizontal();
						++itemCount;

						EditorGUILayout.BeginHorizontal();
						DrawSpace(1);
						mNormalizeRelative = EditorGUILayout.Toggle( "Normalize Relative", mNormalizeRelative, GetLayoutOptions(-1, 24) );
						EditorGUILayout.EndHorizontal();
						++itemCount;
					}
					else if( mCoordType == CoordType.eCT_ObjectSpace )
					{
						EditorGUILayout.BeginHorizontal();
						DrawSpace(1);
						GUILayout.Label ("Object Space", EditorStyles.boldLabel, GetLayoutOptions(-1, 24));
						EditorGUILayout.EndHorizontal();
						++itemCount;

						EditorGUILayout.BeginHorizontal();
						DrawSpace(1);
						coordTransform = EditorGUILayout.ObjectField("Coordinate", coordTransform, typeof(Transform), true, GetLayoutOptions(-1, 24)) as Transform;
						EditorGUILayout.EndHorizontal();
						++itemCount;
					}

					EditorGUILayout.BeginHorizontal();
					GUILayout.Label( "UploadData" );
					mModifyType = (ModifyType)EditorGUILayout.EnumPopup( mModifyType, GetLayoutOptions(200, 24) );
					EditorGUILayout.EndHorizontal();
					++itemCount;

					EditorGUILayout.BeginHorizontal();
					uploadData =    GUILayout.Button( "UploadData", GetLayoutOptions(150, 24) );
					EditorGUILayout.EndHorizontal();
					++itemCount;

					DrawEmptyLine(1);
					++itemCount;
				}

				EditorGUILayout.BeginHorizontal();
				if( GUILayout.Button( GetExpandTxt(expandResetEventTimePos), GetLayoutOptions(24,24) ) )
					expandResetEventTimePos = !expandResetEventTimePos;
				GUILayout.Label ("Reset event time position by distance", titleStype);
				EditorGUILayout.EndHorizontal();
				++itemCount;

				if( expandResetEventTimePos )
				{
					EditorGUILayout.BeginHorizontal();
					startOffsetTime = EditorGUILayout.FloatField( "startOffsetTime", startOffsetTime, GetLayoutOptions(-1, 24) );
					EditorGUILayout.EndHorizontal();
					++itemCount;

					EditorGUILayout.BeginHorizontal();
					endOffsetTime = EditorGUILayout.FloatField( "endOffsetTime", endOffsetTime, GetLayoutOptions(-1, 24) );
					EditorGUILayout.EndHorizontal();
					++itemCount;
					
					EditorGUILayout.BeginHorizontal();
					actionTimeLength = EditorGUILayout.FloatField( "actionTimeLength", actionTimeLength, GetLayoutOptions(-1, 24) );
					EditorGUILayout.EndHorizontal();
					++itemCount;
					
					EditorGUILayout.BeginHorizontal();
					resetTimePos =    GUILayout.Button( "ResetEventTimePos", GetLayoutOptions(150, 24) );
					EditorGUILayout.EndHorizontal();
					++itemCount;

					DrawEmptyLine(1);
					++itemCount;
				}
				
				EditorGUILayout.BeginHorizontal();
				if( GUILayout.Button( GetExpandTxt(expandOffsetEventTimePos), GetLayoutOptions(24,24) ) )
					expandOffsetEventTimePos = !expandOffsetEventTimePos;
				GUILayout.Label ("Offset event time position", titleStype);
				EditorGUILayout.EndHorizontal();
				++itemCount;

				if( expandOffsetEventTimePos )
				{
					EditorGUILayout.BeginHorizontal();
					eventTimeOffset = EditorGUILayout.FloatField( "eventTimeOffset", eventTimeOffset, GetLayoutOptions(-1, 24) );
					EditorGUILayout.EndHorizontal();
					++itemCount;

					EditorGUILayout.BeginHorizontal();
					moveEventTimePos =    GUILayout.Button( "MoveEventTimePos", GetLayoutOptions(150, 24) );
					EditorGUILayout.EndHorizontal();
					++itemCount;

					DrawEmptyLine(1);
					++itemCount;
				}
				
				EditorGUILayout.BeginHorizontal();
				if( GUILayout.Button( GetExpandTxt(expandCurvlOperation), GetLayoutOptions(24,24) ) )
					expandCurvlOperation = !expandCurvlOperation;
				GUILayout.Label ("Curvl operation", titleStype);
				EditorGUILayout.EndHorizontal();
				++itemCount;

				if( expandCurvlOperation )
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label( "SelectCloneOffsetType: ", GetLayoutOptions(-1, 24) ); 
					cloneOffsetType = (CloneOffsetType)EditorGUILayout.EnumPopup( cloneOffsetType, GetLayoutOptions(200, 24) );
					EditorGUILayout.EndHorizontal();
					++itemCount;

					if( cloneOffsetType == CloneOffsetType.eCOT_WorldUserDef ||
					    cloneOffsetType == CloneOffsetType.eCOT_ObjUserDef )
					{
						EditorGUILayout.BeginHorizontal();
						cloneOffset = EditorGUILayout.Vector3Field( "cloneOffset", cloneOffset, GetLayoutOptions(-1, 24) );
						EditorGUILayout.EndHorizontal();
						++itemCount;

						DrawEmptyLine(1);
						++itemCount;
					}

					EditorGUILayout.BeginHorizontal();
					cloneDistance = EditorGUILayout.FloatField( "cloneDistance", cloneDistance, GetLayoutOptions(-1, 24) );
					EditorGUILayout.EndHorizontal();
					++itemCount;

					EditorGUILayout.BeginHorizontal();
					cloneCurvl =    GUILayout.Button( "CloneCurvl", GetLayoutOptions(150, 24) );
					EditorGUILayout.EndHorizontal();
					++itemCount;

					DrawEmptyLine(1);
					++itemCount;
				}

				//EditorGUI.DrawRect( new Rect(0, 24 * (itemCount+3), mRect.width, 2), Color.green );
				EditorGUILayout.BeginHorizontal();
				if( GUILayout.Button( GetExpandTxt(expandGenerateSetting), GetLayoutOptions(24,24) ) )
					expandGenerateSetting = !expandGenerateSetting;
				GUILayout.Label ("General setting", titleStype);
				EditorGUILayout.EndHorizontal();
				++itemCount;

				if( expandGenerateSetting )
				{
					EditorGUILayout.BeginHorizontal();
					nodeprefab = EditorGUILayout.ObjectField("NodePrefab", nodeprefab, typeof(GameObject), true) as GameObject;
					EditorGUILayout.EndHorizontal();
					++itemCount;

					DrawEmptyLine(1);
					++itemCount;

					EditorGUILayout.BeginHorizontal();
					createCurvlEditGO = GUILayout.Button( "CreateCurvlEditGO", GetLayoutOptions( 150, 24 ) );
					deleteCurvlEditGO = GUILayout.Button( "DeleteCurvlEditGO", GetLayoutOptions( 150, 24 ) );
					rebuildCurvl = GUILayout.Button( "RebuildCurvlData", GetLayoutOptions( 150, 24 ) );
					EditorGUILayout.EndHorizontal();
					++itemCount;
				}
				DrawEmptyLine(1);
				itemCount += 1;

				EditorGUI.DrawRect( new Rect(0, 24 * (itemCount+3), mRect.width, 2), Color.green );

				DrawEmptyLine(1);
				++itemCount;
			}
			else
				itemCount += 2;

			EditorGUILayout.BeginHorizontal();
			createUpdateDummy = EditorGUILayout.Toggle( "AutoRefresh", createUpdateDummy );
			EditorGUILayout.EndHorizontal();
			++itemCount;

			EditorGUILayout.BeginHorizontal();
			expandCurvl = EditorGUILayout.Foldout(expandCurvl, "Curvls");
//			if( GUILayout.Button( GetExpandTxt(expandCurvl), GetLayoutOptions(24,24) ) )
//				expandCurvl = !expandCurvl;
//			GUILayout.Label( "Curvls" );
			if( GUILayout.Button( "+", GetLayoutOptions(24,24) ) )
				addCurvl = true;
			bool lastshowAllCurvl = showAllCurvl;
			showAllCurvl = GUILayout.Toggle( lastshowAllCurvl, (lastshowAllCurvl ? "Show" : "Hide"), GetLayoutOptions(50,24) );
			DrawSpace(1);
			EditorGUILayout.EndHorizontal();
			++itemCount;

			GameObject goCurvlHelper = null;
			CurvlHelper comp = null;

			bool expand = false;
			if( expandCurvl )
			{
				goCurvlHelper = GameObject.Find(CurvlHelper.sCurvlHelperObjName);
				if( goCurvlHelper )
				{
					comp = goCurvlHelper.GetComponent<CurvlHelper>();
					if( comp )
					{
						int count = comp.GetCurvlCount();
						for( int i = 0; i < count; ++i )
						{
							CurvlData cc = comp.GetCurvl(i);
							EditorGUILayout.BeginHorizontal();

							string curvlname = "Curvl_"+i;
							string displayname = curvlname;
							if( cc.displayName != null && cc.displayName.Length > 0 )
								displayname = cc.displayName;
							//expand = DrawTreeExpandBtn(1, curvlname, "" );
							string oldDspName = displayname;
							expand = DrawTreeExpandBtnWithEditName( i, 1, curvlname, "", ref displayname );
							cc.displayName = displayname;
							if( oldDspName != displayname )
								PrepData();
							
							if( GUILayout.Button( "+", GetLayoutOptions(24,24) ) )
							{
								if( cc == null || !cc.isInUse || (cc.nodeLst != null && cc.nodeLst.Count > 0) )
									insertCurvl = true;
								else
									addNode = true;
								curvelIDChg = i;
							}
							if( GUILayout.Button( "-", GetLayoutOptions(24,24) ) )
							{
								delCurvl = true;
								curvelIDChg = i;
							}
							if( cc != null )
							{
								if( lastshowAllCurvl != showAllCurvl )
								{
									cc.isHide = !showAllCurvl;
								}
								bool oldV = !cc.isHide;
								bool newV = GUILayout.Toggle( oldV, (oldV ? "Show" : "Hide"), GetLayoutOptions(50,24) );
								if( oldV != newV )
									cc.isHide = !cc.isHide;

								bool usecam = cc.useCamera;
								bool nusecam = GUILayout.Toggle( usecam, (usecam ? "OnCam" : "OffCam"), GetLayoutOptions(66,24) );
								if( usecam != nusecam )
									cc.SetUseCameraComp( !cc.useCamera );

								bool oldCubic = cc.isCubic;
								bool newCubic = GUILayout.Toggle( oldCubic, (oldCubic ? "cubic" : "linear"), GetLayoutOptions(50,24) );
								if( oldCubic != newCubic )
								{
									cc.UnifyCubicPropertyToAllNodes( !cc.isCubic );
									if( comp.mCurvlTrackMap != null && comp.mCurvlTrackMap.ContainsKey(cc) )
									{
										Track track = comp.mCurvlTrackMap[cc];
										int evtc = track.GetEventsCount();
										for( int ei = 0; ei < evtc; ++ei )
										{
											(track.GetEvent(ei) as ModifyTransform).cubic = cc.isCubic;
										}
									}
								}

								cc.drawCurvlCylinder = GUILayout.Toggle( cc.drawCurvlCylinder, "Cyclinder", GetLayoutOptions(50,24) );

								Color newColor = EditorGUILayout.ColorField( cc.lineColor, GetLayoutOptions(44,24) );
								if( !newColor.Equals( cc.lineColor ) )
								{
									cc.lineColor = newColor;
									if( comp.mCurvlTrackMap.ContainsKey(cc) )
									{
										if( cc.GetCurvlRootObject() != null )
											Selection.activeGameObject = cc.GetCurvlRootObject();
										comp.mCurvlTrackMap[cc].color = newColor;
									}
								}

								GameObject newPrefab = (GameObject)EditorGUILayout.ObjectField( cc.transObjPrefb, typeof(GameObject) );
								if( cc.transObjPrefb != newPrefab )
								{
									cc.ReplaceNodeObjectPrefab(newPrefab);
								}

								if( GUILayout.Button( "Reverse", GetLayoutOptions(100,24) ) )
								{
									cc.ReverseNodes();
								}
							}
							DrawSpace(1);
							EditorGUILayout.EndHorizontal();
							++itemCount;

							if( expand )
							{
								for( int j = 0; j < cc.nodeLst.Count; ++j )
								{
									GameObject curNodeObj = cc.GetNodeObject( j );
									bool isSel = (curNodeObj == Selection.activeGameObject);
									if( pressCtrl )
									{
										isSel = false;
										foreach( GameObject o in Selection.gameObjects )
										{
											if( o == curNodeObj )
												isSel = true;
										}
									}

									EditorGUILayout.BeginHorizontal();
									bool bClick = DrawClickLable(2, "TransNode_"+j, isSel );
									if( GUILayout.Button( "+", GetLayoutOptions(24,24) ) )
									{
										addNode = true;
										curvelIDChg = i;
										nodeIDChg = j;
									}
									if( GUILayout.Button( "-", GetLayoutOptions(24,24) ) )
									{
										delNode = true;
										curvelIDChg = i;
										nodeIDChg = j;
									}
									TransNode node = cc.nodeLst[j];
									bool oldV = node.isCubic;
									bool newV = GUILayout.Toggle( oldV, (oldV ? "cubic" : "linear"), GetLayoutOptions(50,24) );
									if( oldV != newV )
									{
										node.isCubic = !node.isCubic;
										if( comp.mCurvlTrackMap != null && comp.mCurvlTrackMap.ContainsKey(cc) )
										{
											Track track = comp.mCurvlTrackMap[cc];
											(track.GetEvent(j) as ModifyTransform).cubic = node.isCubic;
										}
									}
									DrawSpace(1);
									EditorGUILayout.EndHorizontal();
									++itemCount;

									if( bClick )
									{
										if( pressCtrl )
										{
											List<Object> arr = new List<Object>();
											bool bexist = false;
											foreach( GameObject o in Selection.gameObjects )
											{
												if( o == curNodeObj )
												{
													bexist = true;
													continue;
												}
												arr.Add(o);
											}
											if( !bexist )
												arr.Add(curNodeObj);
											Selection.objects = arr.ToArray();
											arr = null;
										}
										else
											Selection.activeGameObject = curNodeObj;
									}
								}
							}

						}
					}
				}
			}

			titleStype.normal.textColor = originColor;
			titleStype.fontSize = originFontSize;

			if( createCurvlEditGO )
			{
				if( goCurvlHelper == null )
				{
					goCurvlHelper = new GameObject(CurvlHelper.sCurvlHelperObjName);
					goCurvlHelper.AddComponent<CurvlHelper>();
					PrepData();
				}
			}
			if( deleteCurvlEditGO )
			{
				DestroyAllCurvl();		
				PrepData();
			}
			if( addCurvl )
			{
				if( goCurvlHelper == null )
				{
					goCurvlHelper = new GameObject(CurvlHelper.sCurvlHelperObjName);
					comp = goCurvlHelper.AddComponent<CurvlHelper>();
				}
				if( comp != null )
				{
					CurvlData cc = comp.CreateCurvl();
					cc.transObjPrefb = this.nodeprefab;
				}
				PrepData();
			}
			if( delCurvl )
			{
				if( comp != null )
				{
					comp.RemoveCurvl( curvelIDChg, true );
				}
				PrepData();
			}
			if( insertCurvl )
			{
				if( comp != null )
				{
					CurvlData cc = comp.InsertCurvl( curvelIDChg );
					cc.transObjPrefb = this.nodeprefab;
				}
				PrepData();
			}
			if( addNode )
			{
				if( comp != null )
				{
					CurvlData cc = comp.GetCurvl(curvelIDChg);
					if( cc == null )
						cc = comp.InsertCurvl(curvelIDChg);
					if( nodeIDChg == -1 )
						cc.AddNode(true);
					else
						cc.InsertNode(nodeIDChg,true);
				}
			}
			if( delNode )
			{
				if( comp != null )
				{
					CurvlData cc = comp.GetCurvl(curvelIDChg);
					if( cc != null )
						cc.RemoveNode(nodeIDChg,true);
				}
			}
			if( rebuildCurvl )
				RebuildCurvlData();

			if( comp != null )
			{
//				comp.showSelected = !showAllCurvl;
				if( mCurvlContent.Length != comp.GetCurvlCount() )
					PrepData();
			}

			if( uploadData )
			{
				SendCurvlData();
			}
			if( requestTracks )
			{
				AGE.EditorMessage msg = new AGE.EditorMessage();
				msg.type = AGE.MessageType.Tracks_Reply;
				msg.replyMsg = new AGE.ReplyMsg();
				msg.replyMsg.tracksMsg = new AGE.TracksReplyMsg();
				AGE.EditorNetClient.GetInstance().SendMessageToServer(msg);
			}
			if( resetTimePos )
				ResetTimePos();
			if( cloneCurvl )
				CloneCurvl();
			if( moveEventTimePos )
				MoveEventTimePos();

			mRealHeight = itemCount * (sItemHeight + (mShowEditSubPanel ? 3f : 1f));
		}

		protected override void EndDrawSelf()
		{
			GUI.EndGroup();
		}

		void RebuildCurvlData()
		{
			GameObject goCurvlHelper = GameObject.Find(CurvlHelper.sCurvlHelperObjName);
			if( goCurvlHelper == null )
			{
				return;
			}
			CurvlHelper comp = goCurvlHelper.GetComponent<CurvlHelper>();
			int ccCount = goCurvlHelper.transform.childCount;
			for( int i = 0; i < ccCount; ++i )
			{
				GameObject goCurvl = goCurvlHelper.transform.GetChild(i).gameObject;
				string[] res = goCurvl.name.Split('_');
				if( res[0].Equals("CurvlRootObject") )
				{
					int cid = -1;
					int.TryParse( res[1], out cid );
					if( cid >= 0 )
					{
						CurvlData cd = comp.GetCurvl(i);
						int nc = goCurvl.transform.childCount;
						if( cd == null || cd.nodeLst == null || cd.nodeLst.Count != nc )
						{
							if( cd == null )
								cd = comp.InsertCurvl(i);
							cd.transObjPrefb = this.nodeprefab;
							for( int j = 0; j < nc; ++j )
							{
								Transform trn = goCurvl.transform.GetChild(j);
								TransNode node = cd.AddNode(false);
								node.pos = trn.position;
								node.rot = trn.rotation;
								node.scl = trn.localScale;
							}
						}
					}
				}
			}
			PrepData();
		}

		void DestroyAllCurvl()
		{
			GameObject goCurvlHelper = GameObject.Find(CurvlHelper.sCurvlHelperObjName);
			if( goCurvlHelper )
			{
				CurvlHelper comp = goCurvlHelper.GetComponent<CurvlHelper>();
				if( comp )
				{
					comp.RemoveAllCurvl(true);
				}
				ActionManager.DestroyGameObject(goCurvlHelper);
//				if( ActionManager.Instance != null && ActionManager.Instance.ResLoader != null )
//					ActionManager.Instance.ResLoader.DestroyObject(goCurvlHelper);
//				else
//					Object.DestroyImmediate(goCurvlHelper);
			}
			PrepData();
		}

		void ResetTimePos()
		{
			if( mCurvlSel < 0 || mCurvlSel >= mCurvlContent.Length )
			{
				EditorUtility.DisplayDialog( "Warnning!", "Please select curvl first!", "OK" );
				return;
			}
			GameObject goCurvlHelper = null;
			CurvlHelper comp = null;		
			CurvlData cc = null;
			goCurvlHelper = GameObject.Find(CurvlHelper.sCurvlHelperObjName);
			if( goCurvlHelper )
			{
				comp = goCurvlHelper.GetComponent<CurvlHelper>();
				if( comp )
					cc = comp.GetCurvl(mCurvlSel);
			}
			if( cc == null )
				return;
			int trackID = -1;
			if( mTrackSel >= 0 && mTrackSel < mTrackInfoList.Count )
				trackID = mTrackInfoList[mTrackSel].index;
			else
			{
				EditorUtility.DisplayDialog( "Warnning!", "Please select track first!", "OK" );
				return;
			}

			float realTimeLength = actionTimeLength - startOffsetTime - endOffsetTime;
			if( realTimeLength < 0.0f )
			{
				EditorUtility.DisplayDialog( "Warnning!", "Invalid time setting!", "OK" );
				return;
			}
			
			int nodeCount = cc.nodeLst.Count;
			int lineStep = comp.lineStep;
			cc.DrawCurvel(false);
			List<float> gapLst = new List<float>();
			float totalLen = 0.0f;

			for( int i = 0; i < nodeCount; ++i )
			{
				float len = 0.0f;
				if( i > 0 )
				{
					int startId = (i-1) * lineStep;
					int endId = startId + lineStep;
					for( int j = startId; j <= endId; ++j )
					{
						Vector3 p1 = cc.curvePoints[startId];
						Vector3 p2 = cc.curvePoints[endId];
						len += (p1-p2).magnitude;
					}
				}
				gapLst.Add( len );
				totalLen += len;
			}

			AGE.EditorMessage msg = new AGE.EditorMessage();
			msg.type = AGE.MessageType.EventTimePos_Reply;
			msg.replyMsg = new AGE.ReplyMsg();
			msg.replyMsg.eventTimePosMsg = new AGE.EventTimePosReplyMsg();
			msg.replyMsg.eventTimePosMsg.eventIndex = -1;
			msg.replyMsg.eventTimePosMsg.position = -1;
			msg.replyMsg.eventTimePosMsg.reqState = 0;
			msg.replyMsg.eventTimePosMsg.trackIndex = trackID;
			AGE.EditorNetClient.GetInstance().SendMessageToServer(msg);

			float lastPos = startOffsetTime;
			for( int i = 0; i < nodeCount; ++i )
			{
				if( totalLen < 0.0001f )
					lastPos += 0.0f;
				else
					lastPos += gapLst[i] / totalLen * realTimeLength;

				msg = new AGE.EditorMessage();
				msg.type = AGE.MessageType.EventTimePos_Reply;
				msg.replyMsg = new AGE.ReplyMsg();
				msg.replyMsg.eventTimePosMsg = new AGE.EventTimePosReplyMsg();
				msg.replyMsg.eventTimePosMsg.eventIndex = i;
				msg.replyMsg.eventTimePosMsg.position = lastPos;
				msg.replyMsg.eventTimePosMsg.reqState = 1;
				msg.replyMsg.eventTimePosMsg.trackIndex = trackID;
				AGE.EditorNetClient.GetInstance().SendMessageToServer(msg);
			}

			msg = new AGE.EditorMessage();
			msg.type = AGE.MessageType.EventTimePos_Reply;
			msg.replyMsg = new AGE.ReplyMsg();
			msg.replyMsg.eventTimePosMsg = new AGE.EventTimePosReplyMsg();
			msg.replyMsg.eventTimePosMsg.eventIndex = -1;
			msg.replyMsg.eventTimePosMsg.position = -1;
			msg.replyMsg.eventTimePosMsg.reqState = 2;
			msg.replyMsg.eventTimePosMsg.trackIndex = trackID;
			AGE.EditorNetClient.GetInstance().SendMessageToServer(msg);
		}


		void CloneCurvl()
		{
			if( mCurvlSel < 0 || mCurvlSel >= mCurvlContent.Length )
			{
				EditorUtility.DisplayDialog( "Warnning!", "Please select curvl first!", "OK" );
				return;
			}
			GameObject goCurvlHelper = null;
			CurvlHelper comp = null;		
			CurvlData cc = null;
			goCurvlHelper = GameObject.Find(CurvlHelper.sCurvlHelperObjName);
			if( goCurvlHelper )
			{
				comp = goCurvlHelper.GetComponent<CurvlHelper>();
				if( comp )
					cc = comp.GetCurvl(mCurvlSel);
			}
			if( cc == null )
				return;

			CurvlData newCurvl = comp.CreateCurvl();
			newCurvl.drawCurvlCylinder = cc.drawCurvlCylinder;
			for( int i = 0; i < cc.nodeLst.Count; ++i )
			{
				GameObject srcObj = cc.GetNodeObject(i);
				Transform srcTrns = srcObj.transform;
				TransNode srcNode = cc.nodeLst[i];
				TransNode dstNode = newCurvl.AddNode( false );
				dstNode.pos = srcNode.pos;
				dstNode.rot = srcNode.rot;
				dstNode.isCubic = srcNode.isCubic;

				Vector3 off = Vector3.zero;
				switch( cloneOffsetType )
				{
				case CloneOffsetType.eCOT_ObjBack:
					off = -srcTrns.forward * cloneDistance;
					break;
				case CloneOffsetType.eCOT_ObjDown:
					off = -srcTrns.up * cloneDistance;
					break;
				case CloneOffsetType.eCOT_ObjFront:
					off = srcTrns.forward * cloneDistance;
					break;
				case CloneOffsetType.eCOT_ObjLeft:
					off = -srcTrns.right * cloneDistance;
					break;
				case CloneOffsetType.eCOT_ObjRight:
					off = srcTrns.right * cloneDistance;
					break;
				case CloneOffsetType.eCOT_ObjUp:
					off = srcTrns.up * cloneDistance;
					break;
				case CloneOffsetType.eCOT_ObjUserDef:
					off = srcTrns.localToWorldMatrix.MultiplyVector( cloneOffset ).normalized * cloneDistance;
					break;
				case CloneOffsetType.eCOT_WorldUserDef:
					off = cloneOffset.normalized * cloneDistance;
					break;
				}
				dstNode.pos += off;
			}
		}

		void MoveEventTimePos()
		{
			if( mCurvlSel < 0 || mCurvlSel >= mCurvlContent.Length )
			{
				EditorUtility.DisplayDialog( "Warnning!", "Please select curvl first!", "OK" );
				return;
			}
			GameObject goCurvlHelper = null;
			CurvlHelper comp = null;		
			CurvlData cc = null;
			goCurvlHelper = GameObject.Find(CurvlHelper.sCurvlHelperObjName);
			if( goCurvlHelper )
			{
				comp = goCurvlHelper.GetComponent<CurvlHelper>();
				if( comp )
					cc = comp.GetCurvl(mCurvlSel);
			}
			if( cc == null )
				return;
			int trackID = -1;
			if( mTrackSel >= 0 && mTrackSel < mTrackInfoList.Count )
				trackID = mTrackInfoList[mTrackSel].index;
			else
			{
				EditorUtility.DisplayDialog( "Warnning!", "Please select track first!", "OK" );
				return;
			}

			AGE.EditorMessage msg = new AGE.EditorMessage();
			msg.type = AGE.MessageType.MoveEventPos_Reply;
			msg.replyMsg = new AGE.ReplyMsg();
			msg.replyMsg.moveEventPosMsg = new AGE.MoveEventPosReplyMsg();
			msg.replyMsg.moveEventPosMsg.offset = eventTimeOffset;
			msg.replyMsg.moveEventPosMsg.trackIndex = trackID;
			AGE.EditorNetClient.GetInstance().SendMessageToServer(msg);
		}

	}
}
