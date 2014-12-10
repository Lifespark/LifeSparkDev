using UnityEngine;
using UnityEditor;
using System;
using System.Runtime.InteropServices;
using System.Collections;

namespace AGE{

public class FbxLoaderPanel : EditorWindow {

	static FbxLoaderPanel sWinInst = null;

	bool bPlay = false;
	int  beginFrame = 0;
	
	[@MenuItem ("AGEEditor/Fbx Loader Panel")]
	public static void ShowWindow () 
	{
		if( sWinInst == null )
			sWinInst = (FbxLoaderPanel)EditorWindow.GetWindow(typeof(FbxLoaderPanel));
		sWinInst.Show();
	}
	
	public static FbxLoaderPanel GetInstance()
	{
		if( sWinInst == null )
			sWinInst = (FbxLoaderPanel)EditorWindow.GetWindow(typeof(FbxLoaderPanel));
		return sWinInst;
	}
	

	bool bLoaded = false;
	float playProgress = 0.0f;
	float animDuration = 0.0f;
	GameObject currentObject = null;
	float globalScale = 0.01f;
	UnityEngine.Object animObject = null;

	
	GameObject animatorObject = null;

	enum PLAY_SPEED
	{
		X_OCTANT  = -3,
		X_QUARTER = -2,
		X_HALF    = -1,
		X1        =  0,
		X2        =  1,
		X4        =  2,
		X8        =  3,

	}

	PLAY_SPEED playSpeedMode = PLAY_SPEED.X1;


	//load animation from obj
	bool LoadFbxAnimation(UnityEngine.Object _animObj, ref float _duration, ref float _gobalScale)
	{
		if(_animObj == null )
		{
			return false;
		}
		
		string projectPath = Application.dataPath;
		projectPath = projectPath.Substring(0, projectPath.LastIndexOf("/") + 1);
		string resPath = AssetDatabase.GetAssetPath(_animObj);
		resPath = resPath.ToLower();
		string path = projectPath + resPath;
		int sceneId= resPath.GetHashCode();
		
		bool load = AgeFbxLoader.LoadFbx( sceneId, path);

		bool getInfo = AgeFbxLoader.GetSceneInfo( sceneId, ref _duration);
		// get scale
		ModelImporter importor = AssetImporter.GetAtPath(resPath) as ModelImporter;
		_gobalScale = importor.globalScale;
		
		return load;
	}

	void Load()
	{
		bLoaded = LoadFbxAnimation(animObject, ref animDuration, ref globalScale);
		if(!bLoaded)
		{
			AgeLogger.Log("Select Anim clip resource(.fbx file), please");
			return;
		}
		else
		{
			AgeLogger.Log("Anim clip loaded.");
		}
		
	}
	
	void Update()
	{
		if(!bPlay)
			return;

		int tick = Time.frameCount - beginFrame;
		float speed = (float)Math.Pow(2, ((int)playSpeedMode));
		tick =  (int)(tick *33.3333f * speed);

		if(tick <=  (animDuration * 1000))
		{
			//AgeLogger.Log("Playing Speed Mode: " + playSpeedMode);
			//AgeLogger.Log("Playing Speed : " + speed);
			
		    PlayAnim(tick);
		}
		else
		{
			bPlay = false;
			AgeLogger.Log("Stop Play");
		}

	}


	void PlayAnim(int _tick)
	{
		
		if(currentObject == null)
		{
			AgeLogger.LogError("Please select current object");
			return;
		}
		
		if(!bLoaded)
		{
			AgeLogger.LogError("Please select anim object and load ");
			return;
		}

		string resPath = AssetDatabase.GetAssetPath(animObject);
		resPath = resPath.ToLower();
		int sceneId = resPath.GetHashCode();
		AgeFbxLoader.PlayFbxAnimation(sceneId, currentObject, _tick, globalScale);
	}


	void OnGUI()
	{	

		if(GUILayout.Button("Init SDK"))
		{
			bool bInit= AgeFbxLoader.Initialize();
			AgeLogger.Log("Init Fbx SDK:" + bInit);
		}

		if(GUILayout.Button(" Destory"))
		{
			bLoaded = false;
			bPlay  =  false;
			AgeFbxLoader.Destory();
			AgeLogger.Log("Destoryed Fbx SDK");
		}

		EditorGUILayout.LabelField("Select GameObject");
		GameObject newSelectedObj = EditorGUILayout.ObjectField( currentObject,typeof(GameObject), true) as GameObject;
		if(newSelectedObj != null)
		{
			currentObject = newSelectedObj;
		}
		
		EditorGUILayout.LabelField("Select Animation clip(FBX)");
		UnityEngine.Object newAnimObj = EditorGUILayout.ObjectField( animObject,typeof(UnityEngine.Object), false) as UnityEngine.Object;
		if(newAnimObj != animObject)
		{
			animObject = newAnimObj;
			//reload
			Load();
		}


		if(GUILayout.Button("Load"))
		{
			Load();

		}



		if(GUILayout.Button(" Play pose 0"))
		{
			if(!bLoaded)
				return;
			PlayAnim(0);
		}

		if(GUILayout.Button("Play Anim"))
		{
			bPlay = !bPlay;
			if(bPlay)
			{
				beginFrame = Time.frameCount;
				AgeLogger.Log("PLAY");
			}
			else
			{
				AgeLogger.Log("Stop");
			}
		}
		playSpeedMode = (PLAY_SPEED)EditorGUILayout.EnumPopup("PlaySpeed(30 FPS):", playSpeedMode);

		EditorGUILayout.LabelField("Anim Duartion: "+ animDuration.ToString() + " s");
		//process
		float newPlayProgress = EditorGUILayout.Slider(playProgress, 0.0f, animDuration, GUILayout.Height(20));

		if (newPlayProgress != playProgress)
		{
			int tick = (int)(newPlayProgress * 1000);
			PlayAnim(tick);
			playProgress = newPlayProgress;
		}

		EditorGUILayout.Space();
		EditorGUILayout.Space();


		GameObject animatorObj = EditorGUILayout.ObjectField( animatorObject,typeof(GameObject), true) as GameObject;
		if(animatorObj != null)
		{
			animatorObject = animatorObj;
		}

	}
}
}

