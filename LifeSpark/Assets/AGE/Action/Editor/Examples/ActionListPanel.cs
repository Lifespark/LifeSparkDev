using UnityEngine;
using UnityEditor;

namespace AGE{

public class ActionListPanel : EditorWindow {

	static ActionListPanel sWinInst = null;

	public GameObject currentObject = null;

	//edit var
	Vector2     scrollPos;
	string      currentActionName;
	bool        showSetTarget = false;
	GameObject  currentTarget = null;
	bool        allPlayOnStart = false;

	string      selActionName ="";
	TextAsset   selAction = null;

	[@MenuItem ("AGEEditor/Action List Panel")]
	public static void ShowWindow () 
	{
		if( sWinInst == null )
			sWinInst = (ActionListPanel)EditorWindow.GetWindow(typeof(ActionListPanel));
		sWinInst.Show();
	}
	
	public static ActionListPanel GetInstance()
	{
		if( sWinInst == null )
			sWinInst = (ActionListPanel)EditorWindow.GetWindow(typeof(ActionListPanel));
		return sWinInst;
	}

	void OnEnable()
	{

	}

	void OnDrawActionNameHelper()
	{
			EditorGUILayout.LabelField ("Action Name Helper:");
			EditorGUILayout.BeginVertical ();
			{
				//TextAsset ageAction = ActionManager.Instance.ResLoader.Load(selActionName) as TextAsset;
				selAction = EditorGUILayout.ObjectField("Action(xml):", selAction, typeof(TextAsset), false) as TextAsset;
				selActionName = AGE.AgeEditorUtility.GetActionNameByAsset(selAction);	

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Action Name(id):");
				EditorGUILayout.SelectableLabel ( selActionName );
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();
	}

	void OnGUI()
	{	
		// Action Name Helper:get action name by dragging xml file to objectFeild
		OnDrawActionNameHelper ();
		
		//new Rect(10, 10, 300, 20)
		EditorGUILayout.LabelField("Select ActionList:");

		GameObject newSelectedObj = EditorGUILayout.ObjectField( currentObject,typeof(GameObject), true) as GameObject;
		if(newSelectedObj != null)
		{
			currentObject = newSelectedObj;
		}

		if(currentObject == null)
			return;
		ActionHelper actionlist = currentObject.GetComponent<ActionHelper>();
		if(actionlist == null)
			return;

		EditorGUILayout.Space();
		showSetTarget = EditorGUILayout.Foldout ( showSetTarget, "Options");
		if(showSetTarget)
		{
			allPlayOnStart = EditorGUILayout.ToggleLeft("All play on start", allPlayOnStart);
			SetAllActionsPlayOnStart(actionlist, allPlayOnStart);

			EditorGUILayout.LabelField("Select target gameobject:");
			currentTarget = EditorGUILayout.ObjectField( currentTarget,typeof(GameObject), true) as GameObject;			
			if( GUILayout.Button("Set target"))
			{
				SetAllActionsDefualtTarget(actionlist, currentTarget);
			}
		}

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Current Action:");
		EditorGUILayout.SelectableLabel(currentActionName);

		float verHeight = showSetTarget ? (this.position.height-20*11): (this.position.height - 20*7);
		EditorGUILayout.BeginVertical();
		scrollPos = 
			EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width (this.position.width), GUILayout.Height (verHeight) );

		foreach (ActionHelperStorage helper in actionlist.actionHelpers)
		{
			string name = helper.actionName;
			name = GetActionNameOnly(name);
			if(GUILayout.Button(name))
			{
				currentActionName = helper.actionName;

				if(UnityEditor.EditorApplication.isPlaying)
					helper.PlayAction();
				else
					AgeLogger.Log("<color=yellow></color> Trigger action in play mode, please");
			}

		}

		EditorGUILayout.EndScrollView();
		EditorGUILayout.EndVertical();

	}

	string GetActionNameOnly( string actionName)
	{
		return System.IO.Path.GetFileNameWithoutExtension(actionName);
	}

	void SetAllActionsDefualtTarget( ActionHelper _actionlist, GameObject _targetObject)
	{
		if(_actionlist == null)
			return;

		foreach (ActionHelperStorage helper in _actionlist.actionHelpers)
		{
			helper.targets[0] = _targetObject;
		}

	}

	void SetAllActionsPlayOnStart(ActionHelper _actionlist, bool _playOnStart)
	{
		if(_actionlist == null)
			return;
		
		foreach (ActionHelperStorage helper in _actionlist.actionHelpers)
		{
			helper.playOnStart = _playOnStart;
		}
	}
}
}
