using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AGE
{

	public class AssetLoader
	{
		public AssetLoader()
		{
		}

		public virtual void Destroy()
		{

		}

		public virtual Object Load(string path)
		{
			return Resources.Load(path);
		}

		public virtual Object Load(string path, System.Type type )
		{
			return Resources.Load(path, type);
		}

		public virtual Object[] LoadAll(string path)
		{
			return Resources.LoadAll(path);
		}

		public virtual Object LoadAssetAtPath(string assetpath, System.Type type)
		{
			return Resources.LoadAssetAtPath( assetpath, type );
		}

		public virtual void UnloadAsset( Object asset )
		{
			Resources.UnloadAsset( asset );
		}

		public virtual Object Instantiate(Object original)
		{
			return Object.Instantiate(original);
		}

		public virtual Object Instantiate(Object original, Vector3 position, Quaternion rotation)
		{
			return Object.Instantiate(original, position, rotation);
		}
		
		public virtual void DestroyObject(Object obj)
		{
	#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isPlaying || UnityEditor.EditorApplication.isPaused)
				Object.Destroy(obj);
			else
				Object.DestroyImmediate(obj);
	#else
			Object.Destroy(obj);
	#endif
		}

		public bool preloading = true;
	}


	public class AssetLoaderOptimise : AssetLoader
	{
		public class PrefabIndexInfo
		{
			public Object prefab;
			public int    index;

			public PrefabIndexInfo()
			{
				prefab = null;
				index = -1;
			}
		}

		public class InstStateInfo
		{
			public Object inst;
			public bool   inuse;

			public InstStateInfo()
			{
				inst = null;
				inuse = true;
			}
		}

		Dictionary<string, Object> mPrefabPool = new Dictionary<string, Object>();
		Dictionary<Object, List<InstStateInfo>> mInstancePool = new Dictionary<Object, List<InstStateInfo>>();
		Dictionary<Object, PrefabIndexInfo> mInstPrefabIndexMap = new Dictionary<Object, PrefabIndexInfo>();

		enum DiffInstDestroyType
		{
			KillDiffInst,
			KillDiffComponent,
			DisableDiffComponent,
		}
		DiffInstDestroyType mDiffInstDestroyType = DiffInstDestroyType.KillDiffInst;

		public AssetLoaderOptimise()
		{
		}

		public override void Destroy()
		{
			// destroy all instances
			if( mInstancePool != null )
			{
				foreach( List<InstStateInfo> instlst in mInstancePool.Values )
				{
					if( instlst == null )
						continue;
					foreach( InstStateInfo inst in instlst )
					{
						if( inst.inst != null )
						{
							#if UNITY_EDITOR
								if (UnityEditor.EditorApplication.isPlaying || UnityEditor.EditorApplication.isPaused)
									Object.Destroy(inst.inst);
								else
									Object.DestroyImmediate(inst.inst);
							#else
								Object.Destroy(inst.inst);
							#endif
						}
					}
					instlst.Clear();
				}
				mInstancePool.Clear();
			}
			// destroy all prefabs
			if( mPrefabPool != null )
			{
//				foreach( Object prefab in mPrefabPool.Values )
//				{
//					if( prefab != null )
//						Resources.UnloadAsset( prefab );
//				}
				mPrefabPool.Clear();
			}
			// destroy inst-prefab map
			if( mInstPrefabIndexMap != null )
				mInstPrefabIndexMap.Clear();
		}

		public override Object Load(string path)
		{
			if( mPrefabPool == null )
				mPrefabPool = new Dictionary<string, Object>();
			if( mPrefabPool.ContainsKey(path) )
				return mPrefabPool[path];
			Object obj = Resources.Load(path);
			if( obj == null )
			{
				if( path!= null && path.Length > 0 )
					AgeLogger.LogError( "not find resource [" + path + "]" );
			}
			else if( obj.GetType() == typeof(GameObject) )
				mPrefabPool.Add( path, obj );
			return obj;
		}
		
		public override Object Load(string path, System.Type type )
		{
			if( mPrefabPool == null )
				mPrefabPool = new Dictionary<string, Object>();
			if( mPrefabPool.ContainsKey(path) )
				return mPrefabPool[path];
			Object obj = Resources.Load(path, type);
			if( obj == null && path.Length > 0 )
			{
				if( path!= null && path.Length > 0 )
					AgeLogger.LogError( "not find resource [" + path + "]" );
			}
			else if( obj.GetType() == typeof(GameObject) )
				mPrefabPool.Add( path, obj );
			return obj;
		}

		public override Object LoadAssetAtPath(string assetpath, System.Type type)
		{
			if( mPrefabPool == null )
				mPrefabPool = new Dictionary<string, Object>();
			if( mPrefabPool.ContainsKey(assetpath) )
				return mPrefabPool[assetpath];
			Object obj = Resources.LoadAssetAtPath( assetpath, type );
			if( obj == null )
			{
				if( assetpath!= null && assetpath.Length > 0 )
					AgeLogger.LogError( "not find resource [" + assetpath + "]" );
			}
			else if( obj.GetType() == typeof(GameObject) )
				mPrefabPool.Add( assetpath, obj );
			return obj;
		}
		
		public override void UnloadAsset( Object asset )
		{
			if( asset != null )
			{
				if( mPrefabPool != null && asset.GetType() == typeof(GameObject) )
				{
					foreach( string path in mPrefabPool.Keys )
					{
						if( mPrefabPool[path] == asset )
						{
							mPrefabPool.Remove( path );
							break;
						}
					}
				}
				Resources.UnloadAsset( asset );
			}
		}

		public override Object Instantiate(Object original)
		{
			if( original == null )
			{
				AgeLogger.LogError( "empty prefab!" );
				return null;
			}
			// create and return the none gameobject instance
			if( original.GetType() != typeof(GameObject) )
				return Object.Instantiate(original);

			// find instance list created by the same prefab
			List<InstStateInfo> objInfoLst;
			Object inst;
			if( mInstancePool == null )
				mInstancePool = new Dictionary<Object, List<InstStateInfo>>();
			if( mInstancePool.ContainsKey(original) )
			{
				objInfoLst = mInstancePool[original];
				if( objInfoLst == null )
				{
					objInfoLst = new List<InstStateInfo>();
					mInstancePool[original] = objInfoLst;
				}
			}
			else
			{
				objInfoLst = new List<InstStateInfo>();
				mInstancePool.Add( original, objInfoLst );
			}

			// loop instance list and search the none use obj
			foreach( InstStateInfo instStateInfo in objInfoLst )
			{
				if( instStateInfo != null && !instStateInfo.inuse )
				{
					inst = instStateInfo.inst;
					instStateInfo.inuse = true;
					if( inst == null )
						inst = Object.Instantiate(original);
					GameObject go = inst as GameObject;
					go.SetActive(true);
					ResetComponents( go );
					return inst;
				}
			}

			// create new instance and build search info
			inst = Object.Instantiate(original);
			PrefabIndexInfo prefabIndexInfo = new PrefabIndexInfo();
			prefabIndexInfo.prefab = original;
			prefabIndexInfo.index = objInfoLst.Count;
			mInstPrefabIndexMap.Add( inst, prefabIndexInfo );

			InstStateInfo isinfo = new InstStateInfo();
			isinfo.inst = inst;
			isinfo.inuse = true;
			objInfoLst.Add(isinfo);
			return inst;
		}

		public override Object Instantiate(Object original, Vector3 position, Quaternion rotation)
		{
			if( original == null )
			{
				AgeLogger.LogError( "empty prefab!" );
				return null;
			}
			// create and return the none gameobject instance
			if( original.GetType() != typeof(GameObject) )
				return Object.Instantiate(original, position, rotation);

			// find instance list created by the same prefab
			List<InstStateInfo> objInfoLst;
			Object inst;
			if( mInstancePool == null )
				mInstancePool = new Dictionary<Object, List<InstStateInfo>>();
			if( mInstancePool.ContainsKey(original) )
			{
				objInfoLst = mInstancePool[original];
				if( objInfoLst == null )
				{
					objInfoLst = new List<InstStateInfo>();
					mInstancePool[original] = objInfoLst;
				}
			}
			else
			{
				objInfoLst = new List<InstStateInfo>();
				mInstancePool.Add( original, objInfoLst );
			}

			// loop instance list and search the none use obj
			foreach( InstStateInfo instStateInfo in objInfoLst )
			{
				if( instStateInfo != null && !instStateInfo.inuse )
				{
					inst = instStateInfo.inst;
					instStateInfo.inuse = true;
					if( inst == null )
						inst = Object.Instantiate(original, position, rotation);
					GameObject go = inst as GameObject;
					go.SetActive(true);
					go.transform.position = position;
					go.transform.rotation = rotation;
					ResetComponents( go );
					return inst;
				}
			}

			// create new instance and build search info
			inst = Object.Instantiate(original, position, rotation);
			PrefabIndexInfo prefabIndexInfo = new PrefabIndexInfo();
			prefabIndexInfo.prefab = original;
			prefabIndexInfo.index = objInfoLst.Count;
			if( mInstPrefabIndexMap == null )
				mInstPrefabIndexMap = new Dictionary<Object, PrefabIndexInfo>();
			mInstPrefabIndexMap.Add( inst, prefabIndexInfo );
			
			InstStateInfo isinfo = new InstStateInfo();
			isinfo.inst = inst;
			isinfo.inuse = true;
			objInfoLst.Add(isinfo);
			return inst;
		}
		
		public override void DestroyObject(Object obj)
		{
			if( obj == null )
				return;
			if( mInstancePool != null && obj.GetType() == typeof(GameObject) )
			{
				if( mInstPrefabIndexMap == null || !mInstPrefabIndexMap.ContainsKey(obj) )
				{
					// loop the instance pool
					foreach( Object prefab in mInstancePool.Keys )
					{
						List<InstStateInfo> infolst = mInstancePool[prefab];
						if( infolst == null )
							continue;
						for( int i = 0; i < infolst.Count; ++i )
						{
							InstStateInfo info = infolst[i];
							// obj in the pool
							if( info != null && info.inst == obj )
							{
								// recycle the obj
								(obj as GameObject).SetActive(false);
								info.inuse = false;

								if( CheckPrefabInstDiff((prefab as GameObject), (obj as GameObject)) )
								{

								}
								else
								{
									// rebuild search info
									PrefabIndexInfo prefabIndexInfo = new PrefabIndexInfo();
									prefabIndexInfo.prefab = prefab;
									prefabIndexInfo.index = i;
									if( mInstPrefabIndexMap == null )
										mInstPrefabIndexMap = new Dictionary<Object, PrefabIndexInfo>();
									mInstPrefabIndexMap.Add( info.inst, prefabIndexInfo );
								}
								return;
							}
						}
					}
				}
				else
				{
					PrefabIndexInfo prefabIndexInfo = mInstPrefabIndexMap[obj];
					if( prefabIndexInfo != null )
					{
						List<InstStateInfo> infolst = mInstancePool[prefabIndexInfo.prefab];
						if( infolst != null && infolst.Count > prefabIndexInfo.index )
						{
							InstStateInfo info = infolst[prefabIndexInfo.index];
							if( info != null )
							{
								// recycle the obj
								(obj as GameObject).SetActive(false);
								info.inuse = false;

								if( CheckPrefabInstDiff((prefabIndexInfo.prefab as GameObject), (obj as GameObject)) )
								{
									if( mDiffInstDestroyType == DiffInstDestroyType.KillDiffInst )
									{
										// remove search info
										mInstPrefabIndexMap.Remove( obj );
										info.inst = null;
										info.inuse = false;
									}
									else 
										return;
								}
								else
								{
									return;
								}
							}
						}
					}
				}
			}
			#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isPlaying || UnityEditor.EditorApplication.isPaused)
				Object.Destroy(obj);
			else
				Object.DestroyImmediate(obj);
			#else
			Object.Destroy(obj);
			#endif
		}

		void ResetComponents( GameObject inst )
		{
			if( inst == null )
				return;
			if( mDiffInstDestroyType != DiffInstDestroyType.KillDiffInst )
			{
				// reset all animation component
				Animation[] animations = inst.GetComponentsInChildren<Animation>(true);
				foreach(Animation animation in animations)
				{
					if (animation.playAutomatically && animation.clip)
					{
						animation.gameObject.SampleAnimation( animation.clip, 0.0f );
					}
				}
				// reset all animator component
				Animator[] animators = inst.GetComponentsInChildren<Animator>(true);
				foreach(Animator animtor in animators )
				{
					for( int i = 0; i < animtor.layerCount; ++i )
					{
						AnimatorStateInfo sinfo = animtor.GetCurrentAnimatorStateInfo(i);
						animtor.Play( sinfo.nameHash );
						AnimationInfo[] infos = animtor.GetCurrentAnimationClipState(i);
						foreach( AnimationInfo info in infos )
						{
							animtor.gameObject.SampleAnimation( info.clip, 0.0f );
						}
					}
				}
				// reset all particlesystem component
				ParticleSystem[] pslst = inst.GetComponentsInChildren<ParticleSystem>(true);
				foreach( ParticleSystem ps in pslst )
				{
					ps.Simulate(0, false, false);
				}
			}
		}

		bool CheckPrefabInstDiff( GameObject prefab, GameObject inst )
		{
			Component[] instCompLst = inst.GetComponents( typeof(Component) );
			Component[] prebCompLst = prefab.GetComponents( typeof(Component) );

			bool isDiff = false;
			foreach( Component instcomp in instCompLst )
			{
				if( instcomp == null )
				{
					//AgeLogger.LogWarning( "null instance component..." );
					continue;
				}
				bool existTheSameComp = false;
				foreach( Component prebcomp in prebCompLst )
				{
					if( prebcomp == null )
					{
						//AgeLogger.LogWarning( "null prefab component..." );
						continue;
					}
					if( instcomp.GetType() == prebcomp.GetType() )
					{
						existTheSameComp = true;
						break;
					}
				}
				if( !existTheSameComp )
				{
					isDiff = true;

					if( mDiffInstDestroyType == DiffInstDestroyType.DisableDiffComponent )
					{
						try
						{
							instcomp.active = false;
							MonoBehaviour scomp = (instcomp as MonoBehaviour);
							if( scomp != null )
								scomp.enabled = false;
						}
						catch( UnityException e )
						{
							AgeLogger.LogException(e);
						}
					}
					else if( mDiffInstDestroyType == DiffInstDestroyType.KillDiffComponent )
					{
						#if UNITY_EDITOR
							if (UnityEditor.EditorApplication.isPlaying || UnityEditor.EditorApplication.isPaused)
								Component.Destroy(instcomp);
							else
								Component.DestroyImmediate(instcomp);
						#else
							Component.Destroy(instcomp);
						#endif
					}
				}
			}

			return isDiff;
		}


	}
}