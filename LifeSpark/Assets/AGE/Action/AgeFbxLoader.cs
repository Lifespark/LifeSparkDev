using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

namespace AGE{

public class AgeFbxLoader {
#if UNITY_EDITOR
	const string pluginName = "AgeFbxAnimationLoader" ;

	[DllImport(pluginName)]
	public static extern bool Initialize();
	
	[DllImport(pluginName)]
	public static extern bool LoadFbx(int sceneId, string path);

	[DllImport(pluginName)]
	public static extern bool GetSceneInfo(int sceneId, ref float duration);

	[DllImport(pluginName)]
	public static extern bool GetBoneTransform(int sceneId, string boneName, int tick, ref Vector3 pos, ref Vector4 rot, ref Vector3 sca);
	//public static extern bool GetBoneTransform(string boneName, int tick, ref AFAVector3 pos, ref AFAVector4 rot, ref AFAVector3 sca);
	
	[DllImport(pluginName)]
	public static extern bool Destory();


	public static string GetAssetPathByClipName( GameObject _animObject, string _clipName)
	{
		string path = "";
		
		if(_animObject == null)
			return path;
#if UNITY_EDITOR	
		if(_animObject.animation != null)
		{
			if(_animObject.animation[_clipName] != null)
			{
				path = UnityEditor.AssetDatabase.GetAssetPath(_animObject.animation[_clipName].clip);
				path = path.ToLower();
				return path;
			}
		}	
		Animator animator = _animObject.GetComponent<Animator>();
		if(animator != null)
		{
			RuntimeAnimatorController rtac = animator.runtimeAnimatorController;
			UnityEditorInternal.AnimatorController ac = rtac as UnityEditorInternal.AnimatorController;
			int layerCount = ac.layerCount;
			
			for(int i = 0; i < layerCount; ++i)
			{
				UnityEditorInternal.AnimatorControllerLayer aclayer = ac.GetLayer(i);
				UnityEditorInternal.StateMachine sm = aclayer.stateMachine;
				int stateCount = sm.stateCount;
				
				for( int sidx = 0; sidx < stateCount; ++sidx)
				{
					UnityEditorInternal.State state = sm.GetState(sidx);
					Motion motion = state.GetMotion();
					if( motion != null && (motion.name == _clipName))
					{
						path = UnityEditor.AssetDatabase.GetAssetPath(motion.GetInstanceID());
						path = path.ToLower();
						return path;
					}					
				}
			}
			
		}
#endif
		return path;
	}

	public static bool GetFbxAnimDuration(GameObject _targetObject, string _clipName, ref float _duration)
	{
		string resPath = AgeFbxLoader.GetAssetPathByClipName(_targetObject, _clipName);//to lower
		int sceneId = resPath.GetHashCode();
		
		string projectPath = Application.dataPath;
		projectPath = projectPath.Substring(0, projectPath.LastIndexOf("/") + 1);
		string path = projectPath + resPath;
		
		// load ( load at first time, will cache in the plugin)
		bool loaded = AgeFbxLoader.LoadFbx(sceneId, path);
		return AgeFbxLoader.GetSceneInfo(sceneId, ref _duration);
	}

	public static bool SampleFbxAnimation( GameObject _targetObject, string _clipName, int tick)
	{
		string resPath = AgeFbxLoader.GetAssetPathByClipName(_targetObject, _clipName);//to lower
		int sceneId = resPath.GetHashCode();

		string projectPath = Application.dataPath;
		projectPath = projectPath.Substring(0, projectPath.LastIndexOf("/") + 1);
		string path = projectPath + resPath;

		// load ( load at first time, will cache in the plugin)
		bool loaded = AgeFbxLoader.LoadFbx(sceneId, path);
		if(loaded)
		{
			float gobalScale = 0.01f;
#if UNITY_EDITOR
			UnityEditor.ModelImporter importor = UnityEditor.AssetImporter.GetAtPath(resPath) as UnityEditor.ModelImporter;
			gobalScale = importor.globalScale;
#endif

			return AgeFbxLoader.PlayFbxAnimation(sceneId, _targetObject, tick, gobalScale);
		}
		return false;
	}



	// call by every frame , must after loaded
	public static bool PlayFbxAnimation( int _sceneId, GameObject _targetObject, int _tick, float _globalScale)
	{
		if(_targetObject == null)
		{
			return false;
		}

		//tick
		int tick = _tick;
		
		Transform[] allChildren = _targetObject.GetComponentsInChildren<Transform>();
		foreach (Transform child in allChildren)
		{
			GameObject childobj = child.gameObject;
			string boneName = childobj.name;
			
			Vector3 posVec = new Vector3();
			Vector4 rotVec = new Vector4();
			Vector3 scaVec = new Vector3();
			
			bool getRet = AgeFbxLoader.GetBoneTransform(_sceneId, boneName, tick, ref posVec, ref rotVec, ref scaVec);
			
			if(!getRet)
			{
				//AgeLogger.Log("Failed to get bone:" + boneName);
				continue;
			}
			
			Vector3    pos = AgeFbxLoader.PosFromFBX2Unity(posVec) * _globalScale;
			Quaternion rot = AgeFbxLoader.QuatFromFBX2Unity(rotVec);
			
			//animate
			child.transform.localPosition = pos;
			child.transform.localRotation = rot;
			child.transform.localScale    = scaVec;
			
			if(boneName == "Bip01")
			{
				//rot 
				Quaternion rx = Quaternion.AngleAxis(-90.0f, Vector3.right);
				child.transform.localRotation = rx * rot;
			}
			
		}

		return true;

	}

	// helper
	public static Quaternion QuatFromFBX2Unity( Vector4 rot)
	{
		//FBX: ZYX
		Quaternion ret1 = Quaternion.AngleAxis( rot.z, Vector3.forward); // zAxis
		Quaternion ret2 = Quaternion.AngleAxis( rot.y, Vector3.up);      // yAxis
		Quaternion ret3 = Quaternion.AngleAxis( rot.x, Vector3.right);   // xAxis

		Quaternion ret = ret1 * ret2 * ret3;
		ret.y = -ret.y;
		ret.z = -ret.z;
		return ret;
		//return new Quaternion(rot.x, -rot.y, -rot.z, rot.w);
	}

	public static Vector3 PosFromFBX2Unity( Vector3 pos)
	{
		return new Vector3(-pos.x, pos.y, pos.z) ;
	}
#endif
}

}
