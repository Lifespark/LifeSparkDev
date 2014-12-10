using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;


namespace AGE{

//	public class ObjTransInfo
//	{
//		public Transform 	trans = null;
//		public int 			id = -1;
//
//		public ObjTransInfo()
//		{
//		}
//	}

	public class TempData
	{
		public ModifyTransform 	evt;
		public Vector3 			pos;
		public Quaternion 		rot;
		public Vector3 			scl;
		public int				trackID;
		public int				nodeID;
		public TempData()
		{
		}
	}

	public class TransNode
	{
		public Vector3 		pos;
		public Quaternion 	rot;
		public Vector3 		scl;

//		public ObjTransInfo	targetID;
//		public ObjTransInfo	objectSpaceID;
//		public ObjTransInfo	fromID;
//		public ObjTransInfo	toID;

		public bool			isCubic;

		public TransNode()
		{
			pos = Vector3.zero;
			rot = Quaternion.identity;
			scl = Vector3.one;

//			targetID = new ObjTransInfo();
//			objectSpaceID = new ObjTransInfo();
//			fromID = new ObjTransInfo();
//			toID = new ObjTransInfo();

			isCubic = true;
		}
	}

	public class CurvlData
	{
		public string 			prefabFile = "Assets/AGE/Action/Prefab/Resources/Cube.prefab";
		public GameObject 		transObjPrefb;

		public bool				isHide;
		public bool				isInUse;
		public bool 			isCubic;
		public bool				closeCurvl;

		public int				stepsOfEachSegment;
		public List<TransNode> 	nodeLst;

		int						curvlID;
		public bool				useCamera;
		public Color			lineColor = Color.red;
		public string			displayName = "";

		List<GameObject>		transNodeObjs;

		GameObject				curvlRootObj;

		List<Vector3> 			midpoints;
		public List<Vector3> 			curvePoints;
		List<Vector3> 			extrapoints;
		Vector3[] 				controlPoints;
		int 					curvePointCount = 0;

		public bool				drawCurvlCylinder = false;
		public float			cylinderRadius = 2.0f;
		public int 				circleSubDivision = 8;
		public Texture			cylinderTexture = null;
		MeshFilter				cylinderMF = null;

		public CurvlData(int id)
		{
			isInUse = true;
			isHide = false;
			curvlID = id;
			stepsOfEachSegment = 10;
			isCubic = true;
			closeCurvl = false;

			InitData();
		}

		void InitData()
		{
			if( nodeLst == null )
				nodeLst = new List<TransNode>();
			if( transNodeObjs == null )
				transNodeObjs = new List<GameObject>();
			if( transObjPrefb == null )
				transObjPrefb = (GameObject)Resources.LoadAssetAtPath<GameObject>(prefabFile);
			if( curvlRootObj == null )
			{
				string rootobjname = "CurvlRootObject_" + curvlID;
				curvlRootObj = GameObject.Find(rootobjname);
				if( curvlRootObj == null )
				{
					GameObject rootgo = GameObject.Find(CurvlHelper.sCurvlHelperObjName);
					if( rootgo == null )
					{
						rootgo = new GameObject();
						rootgo.name = CurvlHelper.sCurvlHelperObjName;
					}
					curvlRootObj = new GameObject();
					curvlRootObj.name = rootobjname;
					curvlRootObj.transform.parent = rootgo.transform;
				}
			}

			PrepareCurvlCylinder();
		}

		void PrepareCurvlCylinder()
		{
			if( drawCurvlCylinder )
			{
				if( cylinderMF == null )
					cylinderMF = curvlRootObj.GetComponent<MeshFilter>();
				if( cylinderMF == null )
					cylinderMF = curvlRootObj.AddComponent<MeshFilter>();
				MeshRenderer mr = curvlRootObj.GetComponent<MeshRenderer>();
				if( mr == null )
				{
					while( mr == null )
						mr = curvlRootObj.AddComponent<MeshRenderer>();
					mr.material = new Material(Shader.Find("Diffuse"));
					if( cylinderTexture == null )
						cylinderTexture = Resources.Load( "grid" ) as Texture;
					if( cylinderTexture != null )
						mr.material.mainTexture = cylinderTexture;
				}
			}
		}

		public int GetCurvlID()
		{
			return curvlID;
		}

		public void SetCurvlID( int id )
		{
			curvlID = id;
			if( curvlRootObj != null )
				curvlRootObj.name = "CurvlRootObject_" + curvlID;
		}

		public void Remove( bool destroy )
		{
			isHide = true;
			isInUse = false;

			if( transNodeObjs != null )
			{
				foreach( GameObject go in transNodeObjs )
				{
					if( go != null )
					{
						if( destroy )
						{
							//ActionManager.DestroyGameObject(go);
							CurvlHelper.ForceDestroyObject(go);
						}
						else
							go.SetActive(false);
					}
				}
			}
			if( curvlRootObj != null )
			{
				if( destroy )
				{
					//ActionManager.DestroyGameObject(curvlRootObj);
					CurvlHelper.ForceDestroyObject(curvlRootObj);
				}
				else
					curvlRootObj.SetActive(false);
			}
			if( destroy )
			{
				transNodeObjs.Clear();
				transNodeObjs = null;

				if( nodeLst != null )
				{
					nodeLst.Clear();
					nodeLst = null;
				}
				if( midpoints != null )
				{
					midpoints.Clear();
					midpoints = null;
				}
				if( curvePoints != null )
				{
					curvePoints.Clear();
					curvePoints = null;
				}
				if( extrapoints != null )
				{
					extrapoints.Clear();
					extrapoints = null;
				}
			}
		}

		public void RefreshDataToGfx()
		{
			InitData();

			int objcount = transNodeObjs.Count;
			int i, j;
			int lastID = nodeLst.Count-1;
			for( i = 0; i < nodeLst.Count; ++i )
			{
				TransNode node = nodeLst[i];
				GameObject obj = null;
				string curobjname = "CurvlTransNode_" + i;
				if( i >= objcount )
				{
					string curvlname = "CurvlRootObject_" + curvlID;
					GameObject co = curvlRootObj;
					if( co != null )
					{
						for( int ci = 0; ci < co.transform.childCount; ++ci )
						{
							obj = co.transform.GetChild(ci).gameObject;
							string cn = obj.name;
							string[] res = cn.Split('_');
							int id = -1;
							int.TryParse( res[1], out id );
							if( id != i )
								obj = null;
							else
								break;
						}
					}
					if( obj == null )
					{
						obj = GameObject.Instantiate(transObjPrefb) as GameObject;
						obj.name = curobjname;
						obj.transform.parent = curvlRootObj.transform;
						Camera cam = obj.GetComponent<Camera>();
						if( cam == null && useCamera )
							cam = obj.AddComponent<Camera>();
						else if( cam != null && !useCamera )
							Component.DestroyImmediate( cam );
					}
					transNodeObjs.Add( obj );
				}
				else
				{
					obj = transNodeObjs[i];
					if( obj == null )
					{
						obj = GameObject.Instantiate(transObjPrefb) as GameObject;
						obj.name = curobjname;
						obj.transform.parent = curvlRootObj.transform;
						Camera cam = obj.GetComponent<Camera>();
						if( cam == null && useCamera )
							cam = obj.AddComponent<Camera>();
						else if( cam != null && !useCamera )
							Component.DestroyImmediate( cam );
						transNodeObjs[i] = obj;
					}
				}

				Transform trn = obj.transform;
				obj.SetActive(true);
				trn.position = node.pos;
				trn.rotation = node.rot;
				//trn.localScale = node.scl;
			}

			for( j = i; j < objcount; ++j )
			{
				transNodeObjs[j].SetActive(false);
			}
		}

		public void ReverseNodes()
		{
			if( nodeLst != null && nodeLst.Count > 1 )
			{
				nodeLst.Reverse();
				for( int i = transNodeObjs.Count - 1; i >= 0; --i )
				{
					if( transNodeObjs[i].activeInHierarchy == false )
					{
						//ActionManager.DestroyGameObject(transNodeObjs[i]);
						CurvlHelper.ForceDestroyObject(transNodeObjs[i]);
						transNodeObjs.RemoveAt(i);
					}
				}
				transNodeObjs.Reverse();
			}
		}

		public void ReplaceNodeObjectPrefab( GameObject obj )
		{
			transObjPrefb = obj;
			for( int i = 0; i < transNodeObjs.Count; ++i )
			{
				//ActionManager.DestroyGameObject(transNodeObjs[i]);
				CurvlHelper.ForceDestroyObject(transNodeObjs[i]);
			}
			transNodeObjs.Clear();
			RefreshDataToGfx();
		}

		public GameObject GetNodeObject( int index )
		{
			if( transNodeObjs == null || index < 0 || index >= transNodeObjs.Count )
				return null;
			return transNodeObjs[index];
		}

		public GameObject GetCurvlRootObject()
		{
			return curvlRootObj;
		}

		public TransNode GetNode(int index)
		{
			if( nodeLst == null || index < 0 || index >= nodeLst.Count )
				return null;
			return nodeLst[index];
		}

		public TransNode AddNode(bool needRefreshGfxData)
		{
			if( nodeLst == null )
				nodeLst = new List<TransNode>();
			TransNode node = new TransNode();
			nodeLst.Add(node);
			if( needRefreshGfxData )
				RefreshDataToGfx();
			return node;
		}

		public TransNode InsertNode( int index, bool needRefreshGfxData )
		{
			if( nodeLst == null )
				nodeLst = new List<TransNode>();
			if( index < 0 )
				return null;
			TransNode node = new TransNode();
			if( index >= nodeLst.Count )
			{
				if( nodeLst.Count > 0 )
				{
					TransNode ln = nodeLst[nodeLst.Count-1];
					node.pos = ln.pos;
					node.isCubic = ln.isCubic;
				}
				nodeLst.Add(node);
			}
			else
			{
				TransNode ln = nodeLst[index];
				node.pos = ln.pos;
				node.isCubic = ln.isCubic;
				nodeLst.Insert(index, node);
			}
			if( needRefreshGfxData )
				RefreshDataToGfx();
			return node;
		}

		public void RemoveNode( int index, bool needRefreshGfxData )
		{
			if( nodeLst != null && index >= 0 && index < nodeLst.Count )
			{
				nodeLst.RemoveAt(index);
				if( needRefreshGfxData )
					RefreshDataToGfx();
			}
		}

		public void RemoveAllNodes(bool needRefreshGfxData)
		{
			if( nodeLst != null )
			{
				nodeLst.Clear();
				if(needRefreshGfxData) 
					RefreshDataToGfx();
			}
		}

		void SyncTransformDataToTransNode()
		{
			if( nodeLst == null || nodeLst.Count == 0 )
				return;
			if( transNodeObjs == null || transNodeObjs.Count < nodeLst.Count || transNodeObjs[0] == null )
				RefreshDataToGfx();

			for( int i = 0; i < nodeLst.Count; ++i )
			{
				TransNode node = nodeLst[i];
				Transform tran = transNodeObjs[i].transform;
				node.pos = tran.position;
				node.rot = tran.rotation;
				node.scl = tran.localScale;
			}
		}

		void CreateCurvlPoints( bool useUnifyInterp )
		{
			if( curvePoints == null )
				curvePoints = new List<Vector3>();
			if( controlPoints == null )
				controlPoints = new Vector3[4];
			float scale = CurvlHelper.sCtrlPtScale;
			int originCount = nodeLst.Count;
			curvePointCount = 0;
			float stepLen = 1.0f / stepsOfEachSegment;

			for( int i = 0; i < originCount; ++i )
			{
				if( closeCurvl == false && i == 0 )
					continue;

				int prevID = i-1;
				if( prevID < 0 )
				{
					if( closeCurvl )
						prevID = originCount-1;
					else
						prevID = 0;
				}
				int formID = i-2;
				if( formID < 0 )
				{
					if( closeCurvl )
						formID = originCount-1;
					else
						formID = 0;
				}
				int lattID = i+1;
				if( lattID >= originCount )
				{
					if( closeCurvl )
						lattID = 0;
					else
						lattID = i;
				}

				Vector3 prevPoint = nodeLst[prevID].pos;
				Vector3 curnPoint = nodeLst[i].pos;
				Vector3 formPoint = nodeLst[formID].pos;
				Vector3 lattPoint = nodeLst[lattID].pos;

				if( (useUnifyInterp && !isCubic) || (!useUnifyInterp && !nodeLst[i].isCubic) )
				{
					if( curvePoints.Count <= curvePointCount )
						curvePoints.Add(prevPoint);
					else
						curvePoints[curvePointCount] = prevPoint;
					++curvePointCount;

					if( curvePoints.Count <= curvePointCount )
						curvePoints.Add(curnPoint);
					else
						curvePoints[curvePointCount] = curnPoint;
					++curvePointCount;
					continue;
				}

				Vector3 ctrlPoint1;
				Vector3 ctrlPoint2;
				CurvlData.CalculateCtrlPoint(formPoint, prevPoint, curnPoint, lattPoint, out ctrlPoint1, out ctrlPoint2);
				CalculateCurvlPoints( prevPoint, curnPoint, ctrlPoint1, ctrlPoint2, stepLen );
			}
		}

		public static void CalculateCtrlPoint(Vector3 formPoint, Vector3 prevPoint, Vector3 curnPoint, Vector3 lattPoint, out Vector3 ctrlPoint1, out Vector3 ctrlPoint2)
		{
			Vector3 midForwPrev = (formPoint + prevPoint) * 0.5f;
			Vector3 midPrevCurn = (curnPoint + prevPoint) * 0.5f;
			Vector3 midCurnLatt = (curnPoint + lattPoint) * 0.5f;
			
			Vector3 midForwPrevCurn = (midForwPrev + midPrevCurn) * 0.5f;
			Vector3 midPrevCurnLatt = (midCurnLatt + midPrevCurn) * 0.5f;
			
			Vector3 handle1 = midPrevCurn - midForwPrevCurn;
			Vector3 handle2 = midPrevCurn - midPrevCurnLatt;

			float s1 = CurvlHelper.sCtrlPtScale;
			float s2 = CurvlHelper.sCtrlPtScale;
			float lh1 = handle1.magnitude;
			float lh2 = handle2.magnitude;
			float halfl = (curnPoint - prevPoint).magnitude * 0.5f;
//			float lpc = (prevPoint - curnPoint).magnitude;
//			float lfp = (formPoint - prevPoint).magnitude;
//			float lcl = (curnPoint - lattPoint).magnitude;
//			float max = lh1 + lh2;
//			if( lpc < lfp )
//			{
//				if( max > 0.0f )
//					s1 = lh1 / (lh1 + lh2);
//				s1 *= (lpc / lfp);
//			}
//			if( lpc < lcl )
//			{
//				if( max > 0.0f )
//					s2 = lh2 / (lh1 + lh2);
//				s2 *= (lpc / lcl);
//			}
			if( halfl < lh1 )
				s1 = halfl / lh1;
			if( halfl < lh2 )
				s2 = halfl / lh2;
			ctrlPoint1 = prevPoint + (handle1) * s1;
			ctrlPoint2 = curnPoint + (handle2) * s2;
		}

		void CalculateCurvlPoints( Vector3 prevPoint, Vector3 curnPoint, Vector3 ctrlPoint1, Vector3 ctrlPoint2, float stepLen )
		{
			bool toend = false;
			float _blendWeight = 0.0f;
			while( _blendWeight <= 1.0f )
			{
				float t1 = 1.0f - _blendWeight;
				float t2 = _blendWeight;
				Vector3 resultPos = 
						prevPoint       * t1 * t1 * t1 +
						ctrlPoint1 * 3  * t1 * t1 * t2 +
						ctrlPoint2 * 3  * t1 * t2 * t2 +
						curnPoint       * t2 * t2 * t2;
				
				if( curvePoints.Count <= curvePointCount )
				{
					curvePoints.Add(resultPos);
					++curvePointCount;
				}
				else
				{
					if( curvePointCount > 0 )
					{
						if( (curvePoints[curvePointCount-1] - resultPos).magnitude > 0.000001f )
						{
							curvePoints[curvePointCount] = resultPos;
							++curvePointCount;
						}
					}
					else
					{
						curvePoints[curvePointCount] = resultPos;
						++curvePointCount;
					}
				}
				
				_blendWeight += stepLen;
				if( toend == true )
					break;
				if( _blendWeight >= 1.0f )
				{
					_blendWeight = 1.0f;
					toend = true;
				}
			}
		}

		void UpdateNodeShowState(bool show)
		{
			int realNodeCount = nodeLst.Count;
			for( int i = 0; i < transNodeObjs.Count; ++i )
			{
				if( transNodeObjs[i] != null )
				{
					if( i < realNodeCount )
						transNodeObjs[i].SetActive( show );
					else
						transNodeObjs[i].SetActive(false);
				}
			}
		}

		public void DrawCurvel(bool useUnifyInterp)
		{
			if( isInUse && curvlRootObj != null && curvlRootObj.activeInHierarchy == false )
				curvlRootObj.SetActive(true);
			UpdateNodeShowState( isInUse && !isHide );
			
			if( !isInUse || isHide )
				return;

			curvlRootObj.SetActive(true);
			SyncTransformDataToTransNode();
			CreateCurvlPoints(useUnifyInterp);
			Vector3 vStart, vEnd;
			for( int i = 0; i < curvePointCount-1; ++i )
			{
				vStart = curvePoints[i];
				vEnd = curvePoints[i+1];
				Debug.DrawLine( vStart, vEnd, lineColor );
			}

			if( drawCurvlCylinder )
			{
				PrepareCurvlCylinder();
				GenerateMeshFilters( ref cylinderMF, circleSubDivision, cylinderRadius );
			}
			else if( cylinderMF != null )
			{
				cylinderMF.mesh.Clear();
			}
		}

		public void SetUseCameraComp( bool bEnable )
		{
			useCamera = bEnable;
			foreach( GameObject node in transNodeObjs )
			{
				if( node != null )
				{
					Camera cam = node.GetComponent<Camera>();
					if( cam == null && useCamera )
						cam = node.AddComponent<Camera>();
					else if( cam != null && !useCamera )
						Component.DestroyImmediate( cam );
				}
			}
		}

		public void UnifyCubicPropertyToAllNodes( bool cubic )
		{
			isCubic = cubic;
			foreach( TransNode node in nodeLst )
			{
				node.isCubic = cubic;
			}
		}

		void CalValidForward( ref List<float> angleLst, ref List<Vector3> tangentLst, ref List<Vector3> forwardLst )
		{
			if( angleLst == null )
				angleLst = new List<float>();
			if( tangentLst == null )
				tangentLst = new List<Vector3>();
			if( forwardLst == null )
				forwardLst = new List<Vector3>();
			Vector3 vPC, vNC, vHalf, vTangen, vExt, vL, vR;
			Vector3 vCurForward;
			float angle = 0.0f;

			if( curvePointCount == 2 )
			{
				vTangen = (curvePoints[1] - curvePoints[0]).normalized;
				angle = 180.0f;
				vCurForward = Vector3.right;
				angleLst.Add( angle );
				tangentLst.Add( vTangen );
				forwardLst.Add( vCurForward );
				angleLst.Add( angle );
				tangentLst.Add( vTangen );
				forwardLst.Add( vCurForward );
				return;
			}
			for( int i = 0; i < curvePointCount; ++i )
			{
				if( i == 0 )
				{
					vTangen = (curvePoints[i+1] - curvePoints[i]).normalized;
					vL = (curvePoints[i] - curvePoints[i+1]);
					vR = (curvePoints[i+2] - curvePoints[i+1]);
					angle = Vector3.Angle( vL, vR );
					vCurForward = Vector3.Cross( vL, vR ).normalized;
				}
				else if( i == curvePointCount-1 )
				{
					vTangen = (curvePoints[i] - curvePoints[i-1]).normalized;
					vL = (curvePoints[i-2] - curvePoints[i-1]);
					vR = (curvePoints[i] - curvePoints[i-1]);
					angle = Vector3.Angle( vL, vR );
					vCurForward = Vector3.Cross( vL, vR ).normalized;
				}
				else
				{
					vPC = (curvePoints[i-1] - curvePoints[i]).normalized;
					vNC = (curvePoints[i+1] - curvePoints[i]).normalized;
					angle = Vector3.Angle( vPC, vNC );
					vCurForward = Vector3.Cross( vPC, vNC).normalized;
					float deg = Vector3.Angle( forwardLst[i-1], vCurForward );
					if( deg > 90.0f )
						vCurForward = -vCurForward;
					if( IsTwoVectorParallel(angle) )
						vTangen = vNC;
					else
					{
						vHalf = (vPC + vNC).normalized;
						vTangen = Vector3.Cross( vCurForward, vHalf ).normalized;
					}

					if( Vector3.Angle(vNC, vTangen) > 90.0f )
						vTangen = -vTangen;
				}

				angleLst.Add( angle );
				tangentLst.Add( vTangen );
				forwardLst.Add( vCurForward );
			}
		}

		bool IsTwoVectorParallel( Vector3 vl, Vector3 vr )
		{
			float angle = Vector3.Angle( vl, vr );
			if( angle < 0.1f || angle > 179.9f )
				return true;
			return false;
		}

		bool IsTwoVectorParallel( float degree )
		{
			if( degree < 0.1f || degree > 179.9f )
				return true;
			return false;
		}

		public void GenerateMeshFilters( ref MeshFilter meshfilter, int segOfCircle, float radius )
		{
			Mesh mesh = meshfilter.mesh;

			float degOfSeg = 360.0f / segOfCircle;
			float circleLen = 2.0f * Mathf.PI * radius;
			float startV = 0.0f;
			float startU = 0.0f;
			float deltaU = 1.0f / segOfCircle;

			SyncTransformDataToTransNode();
			CreateCurvlPoints(false);

			if( curvePointCount < 2 )
				return;

			List<float> angleLst = new List<float>();
			List<Vector3> tangentLst = new List<Vector3>();
			List<Vector3> forwardLst = new List<Vector3>();
			CalValidForward( ref angleLst, ref tangentLst, ref forwardLst );

			List<Vector3> vertexs = new List<Vector3>();
			List<Vector2> uvs = new List<Vector2>();

			Vector3 vTangen, vCurForward, vExt, vLastForward = Vector3.right;
			float angle = 0.0f;
			bool bSetLastForward = false;
			for( int i = 0; i < curvePointCount; ++i )
			{
				angle = angleLst[i];
				vTangen = tangentLst[i];
				vCurForward = forwardLst[i];
				if( IsTwoVectorParallel(angle) )
				{
					if( bSetLastForward )
						vCurForward = vLastForward;
					else
					{
						bool bFind = false;
						for( int k = i-1; k >= 0; --k )
						{
							if( !IsTwoVectorParallel(angleLst[k]) )
							{
								vCurForward = forwardLst[k];
								bFind = true;
								break;
							}
						}
						if( !bFind )
						{
							for( int k = i+1; k < curvePointCount; ++k )
							{
								if( !IsTwoVectorParallel(angleLst[k]) )
								{
									vCurForward = forwardLst[k];
									bFind = true;
									break;
								}
							}
						}
						if( !bFind )
						{
							if( !IsTwoVectorParallel( Vector3.up, vTangen ) )
								vCurForward = Vector3.Cross( Vector3.up, vTangen ).normalized;
							else if( !IsTwoVectorParallel( Vector3.forward, vTangen ) )
								vCurForward = Vector3.Cross( Vector3.forward, vTangen ).normalized;
							else
								vCurForward = Vector3.Cross( Vector3.right, vTangen ).normalized;
						}
					}
				}

				Debug.DrawLine( curvePoints[i], (curvePoints[i] + vCurForward * radius * 2), Color.green );
//				Debug.DrawLine( curvePoints[i], (curvePoints[i] + vTangen * radius * 2), Color.blue );

				if( i > 0 )
				{
					float angleOfPrevCurForward = Vector3.Angle( vLastForward, vCurForward );
					float offu = (curvePoints[i] - curvePoints[i-1]).magnitude * Mathf.Tan( Mathf.Deg2Rad * angleOfPrevCurForward ) / circleLen;
					startU += offu;
				}

				float circleBeginU = startU;
				for( int j = 0; j < segOfCircle; ++j )
				{
					vExt = (Quaternion.AngleAxis( j * degOfSeg, vTangen ) * vCurForward) * radius + curvePoints[i];
					vertexs.Add( vExt );

					uvs.Add( new Vector2(circleBeginU, startV ) );
					circleBeginU += deltaU;
				}
				vertexs.Add( Quaternion.AngleAxis( 0, vTangen ) * vCurForward * radius + curvePoints[i] );
				uvs.Add( new Vector2(circleBeginU, startV ) );

				if( i < curvePointCount-1 )
					startV += ((curvePoints[i+1] - curvePoints[i]).magnitude / circleLen);

				vLastForward = vCurForward;
				bSetLastForward = true;
			}
			vertexs.Add( curvePoints[0] );
			vertexs.Add( curvePoints[curvePointCount-1] );
			uvs.Add( new Vector2(0.5f, 0.0f) );
			uvs.Add( new Vector2(startU + 0.5f, startV) );

			List<int> triangles = new List<int>();
			int circlePtCount = segOfCircle + 1;
			int lastCalPt = circlePtCount - 2;
			for( int i = 0; i < curvePointCount-1; ++i )
			{
				for( int j = 0; j <= lastCalPt; ++j )
				{
					int self = j + ( i    * circlePtCount );                
					int next = j + ((i+1) * circlePtCount );

					triangles.Add(self);
					triangles.Add(self + 1);
					triangles.Add(next + 1);
					triangles.Add(self);
					triangles.Add(next + 1);
					triangles.Add(next);

					if( i == 0 )
					{
						triangles.Add( vertexs.Count-2 );
						triangles.Add( self+1 );
						triangles.Add( self );
					}
					else if( i == curvePointCount-2 )
					{
						triangles.Add( vertexs.Count-1 );
						triangles.Add( next );
						triangles.Add( next+1 );
					}
				}
			}

			mesh.vertices = vertexs.ToArray();
			mesh.triangles = triangles.ToArray();
			mesh.uv = uvs.ToArray();
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
		}
	}

	public class CurvlHelper : MonoBehaviour 
	{
		public static string 	sCurvlHelperObjName = "CurvlHelper";
		public static float 	sCtrlPtScale = 0.6f;

		List<CurvlData> 		curvlLst;

		public float			ctrlPointScale = 0.6f;
		public int				lineStep = 15;
		//public bool 			isCubic = true;
		public bool				useSameInterpMode = false;
		public bool				closeLine = false;
		public bool				showOnGame = false;
		public bool				showOnGizmo = true;
		//public bool				showSelected = false;

		public Dictionary<CurvlData, Track> 	mCurvlTrackMap = new Dictionary<CurvlData, Track>();
		public Action 							mPreviewAction = null;

		// Use this for initialization
		void Start () 
		{
		}
		
		// Update is called once per frame
		void Update () 
		{
			sCtrlPtScale = ctrlPointScale;
			if( showOnGame )
				Draw();
		}

		void OnDrawGizmos()
		{
			sCtrlPtScale = ctrlPointScale;
			if( showOnGizmo )
				Draw();
		}

		void HideUnactiveCurvls()
		{
			GameObject rootobj = GameObject.Find( sCurvlHelperObjName );
			if( rootobj != null )
			{
				Transform rt = rootobj.transform;
				for( int i = 0; i < rt.childCount; ++i )
				{
					Transform ct = rt.GetChild(i);
					ct.gameObject.SetActive(false);
				}
			}
		}


		//bool add = true;
		public void Draw()
		{
			if( curvlLst == null )
			{
				HideUnactiveCurvls();
				return;
			}

			foreach( CurvlData curvlData in curvlLst )
			{
				if( curvlData != null )
				{
					curvlData.stepsOfEachSegment = lineStep;
					curvlData.DrawCurvel( useSameInterpMode );
				}
			}
		}

		public static int[] GetSelectedCurvl()
		{
	#if UNITY_EDITOR
			Object[] selobjs = Selection.GetFiltered(typeof(GameObject), SelectionMode.TopLevel);
			if( selobjs == null || selobjs.Length == 0 )
				return null;
			List<int> selcurvls = new List<int>();
			foreach( GameObject obj in selobjs )
			{
				string[] res;
				if( obj.name.Contains("CurvlRootObject_") )
				{
					if( !obj.activeInHierarchy )
					{
						//ActionManager.DestroyGameObject(obj);
						CurvlHelper.ForceDestroyObject(obj);
						continue;
					}
					res = obj.name.Split('_');
					int cid = -1;
					if( int.TryParse( res[1], out cid ) )
						selcurvls.Add( cid );
				}
				else if( obj.name.Contains("CurvlTransNode_") )
				{
					GameObject po = obj.transform.parent.gameObject;
					if( !po.activeInHierarchy )
					{
						//ActionManager.DestroyGameObject(po);
						CurvlHelper.ForceDestroyObject(po);
						continue;
					}
					res = po.name.Split('_');
					int cid = -1;
					if( int.TryParse( res[1], out cid ) )
						selcurvls.Add( cid );
				}
			}
			if( selcurvls.Count > 0 )
				return selcurvls.ToArray();
	#endif
			return null;
		}

		public int GetCurvlCount()
		{
			if( curvlLst == null )
				return 0;
			return curvlLst.Count;
		}

		public CurvlData CreateCurvl()
		{
			if( curvlLst == null )
				curvlLst = new List<CurvlData>();

			CurvlData cc;
			for( int i = 0; i < curvlLst.Count; ++i )
			{
				cc = curvlLst[i];
				if( cc == null )
				{
					cc = new CurvlData(i);
					cc.SetCurvlID( i );
					curvlLst[i] = cc;
					return cc;
				}
				if( !cc.isInUse )
				{
					cc.isInUse = true;
					return cc;
				}
			}
			cc = new CurvlData(curvlLst.Count);
			cc.SetCurvlID(curvlLst.Count);
			curvlLst.Add( cc );
			return cc;
		}

		public CurvlData InsertCurvl( int index )
		{
			if( curvlLst == null )
				curvlLst = new List<CurvlData>();

			if( index < 0 )
				return null;
			CurvlData cc;
			if( index >= curvlLst.Count )
			{
				cc = new CurvlData(curvlLst.Count);
				cc.SetCurvlID( curvlLst.Count );
				curvlLst.Add(cc);
			}
			else
			{
				cc = curvlLst[index];
				if( cc == null )
				{
					cc = new CurvlData(index);
					cc.SetCurvlID( index );
					curvlLst[index] = cc;
				}
				else if( !cc.isInUse )
				{
					cc.isInUse = true;
				}
				else
				{
					curvlLst.Insert(index, null);
					for( int k = index+1; k < curvlLst.Count; ++k )
					{
						CurvlData cd = curvlLst[k];
						if( cd != null )
							cd.SetCurvlID( k );
					}
					cc = new CurvlData(index);
					cc.SetCurvlID(index);
					curvlLst[index] = cc;
				}
			}
			return cc;
		}

		public CurvlData GetCurvl( int index )
		{
			if( curvlLst == null || index < 0 || index >= curvlLst.Count )
				return null;
			return curvlLst[index];
		}

		public void RemoveCurvl( int index, bool destroy )
		{
			CurvlData cc = GetCurvl(index);
			if( cc != null )
			{
				cc.Remove(destroy);
				if( destroy )
				{
					curvlLst.RemoveAt(index);
					for( int i = index; i < curvlLst.Count; ++i )
					{
						cc = curvlLst[i];
						if( cc != null )
							cc.SetCurvlID( i );
					}
				}
			}
		}

		public void RemoveAllCurvl(bool destroy)
		{
			if( curvlLst != null )
			{
				foreach( CurvlData cc in curvlLst )
				{
					if( cc != null )
					{
						cc.Remove(destroy);
						if( !destroy )
							curvlLst[cc.GetCurvlID()] = null;
					}
				}
				if( destroy )
					curvlLst.Clear();
			}
			if( destroy )
			{
				GameObject goCurvlHelper = GameObject.Find(CurvlHelper.sCurvlHelperObjName);
				if( goCurvlHelper )
				{
					List<GameObject> gol = new List<GameObject>();
					int childCount = goCurvlHelper.transform.childCount;
					for( int i = 0; i < childCount; ++i )
					{
						Transform curvlTrans = goCurvlHelper.transform.GetChild(i);
						int nodeCount = curvlTrans.childCount;
						for( int j = 0; j < nodeCount; ++j )
						{
							Transform nodeTrans = curvlTrans.GetChild(j);
							gol.Add(nodeTrans.gameObject);
						}
						gol.Add( curvlTrans.gameObject );
					}

					foreach( GameObject o in gol )
					{
						//ActionManager.DestroyGameObject(o);
						CurvlHelper.ForceDestroyObject(o);
					}
				} 
			}
		}

		static Vector3 axisWeight = new Vector3(1, 0, 1);
		public static bool CalTransform( ModifyTransform evt, Transform dstobj, Transform fromTransform, Transform toTransform, Transform coordTransform, ref Vector3 oPos, ref Quaternion oRot, ref Vector3 oScl )
		{
			if (dstobj == null) return false;

			//calculate relative coord
			if( fromTransform != null && toTransform != null )
			{
				Vector3 lookDir = toTransform.position - fromTransform.position;
				lookDir = new Vector3(lookDir.x*axisWeight.x, lookDir.y*axisWeight.y, lookDir.z*axisWeight.z);
				float length = (new Vector2(lookDir.x, lookDir.z)).magnitude;
				lookDir = Vector3.Normalize(lookDir);
				Quaternion invLookRotation = Quaternion.Inverse(Quaternion.LookRotation(lookDir, Vector3.up));
				Vector3 relativePos = dstobj.position - fromTransform.position;
				// normailize relative
				if (evt.normalizedRelative)
				{
					oPos = invLookRotation * (dstobj.position - fromTransform.position);
					oPos = new Vector3(oPos.x / length, oPos.y, oPos.z / length);
					oPos -= new Vector3(0, 1, 0) * oPos.z * (toTransform.position.y - fromTransform.position.y);
				}
				else
				{
					oPos = invLookRotation * (dstobj.position - fromTransform.position);
					oPos -= new Vector3(0, 1, 0) * (oPos.z / length) * (toTransform.position.y - fromTransform.position.y);
				}
				oRot = invLookRotation * dstobj.rotation;
				oScl = dstobj.localScale;
			}
			// object space coord
			else if( coordTransform != null )
			{
				oPos = coordTransform.InverseTransformPoint(dstobj.position);
				oRot = Quaternion.Inverse(coordTransform.rotation) * dstobj.rotation;
				oScl = dstobj.localScale;
			}
			// world / local
			else
			{
				oPos = dstobj.position;
				oRot = dstobj.rotation;
				oScl = dstobj.localScale;
			}
			return true;
		}

		public static void ForceDestroyObject( Object obj )
		{
			#if UNITY_EDITOR
			Object.DestroyImmediate( obj );
			#else
			Object.Destroy( obj );
			#endif
		}
	}
}

