using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AGE{

[System.Serializable]
public class ActionHelperStorage
{
	public string helperName = "";
	public string actionName = "";
	public bool playOnStart = false;
	public string detectStatePath = "";
	public bool waitForEvents = false;
	public bool autoPlay = true;
	public bool stopConflictActions = false;
	public GameObject[] targets = new GameObject[0];
	
	int detectStatePathHash = 0;
	public int GetDetectStatePathHash()
	{
		if (detectStatePathHash == 0)
			detectStatePathHash = Animator.StringToHash(detectStatePath);
		return detectStatePathHash;
	}
	
	Action lastAction = null;
	int lastActionFrame = -1;
	public bool IsLastActionActive()
	{
		if (!ActionManager.Instance.IsActionValid(lastAction))
			lastAction = null;
		return lastAction != null;
	}
	public void StopLastAction()
	{
		if (lastAction != null)
		{
			if (ActionManager.Instance.IsActionValid(lastAction))
				lastAction.Stop();
			lastAction = null;
		}
	}
	public Action PlayAction()
	{
#if UNITY_EDITOR
		if (UnityEditor.EditorApplication.isPlaying || UnityEditor.EditorApplication.isPaused)
		{
			if (Time.frameCount <= lastActionFrame + 1)
				return null;
		}
#else
		if (Time.frameCount <= lastActionFrame + 1)
			return null;
#endif

		lastAction = ActionManager.Instance.PlayAction(actionName, autoPlay, stopConflictActions, targets);
		lastActionFrame = Time.frameCount;
		return lastAction;
	}

	public Action PlayAction(Dictionary<string, GameObject> dictionary)
	{
		//change part of target
		Action actionResource = ActionManager.Instance.LoadActionResource(actionName);
		GameObject[] copyTargets = (GameObject[])targets.Clone();

		foreach(KeyValuePair<string, GameObject> pair in dictionary)
		{
			int idx = -1;
			bool bFind = actionResource.TemplateObjectIds.TryGetValue(pair.Key, out idx);
			if (bFind)
			{
				copyTargets[idx] = pair.Value;
			}
		}

		//play Action
		#if UNITY_EDITOR
		if (UnityEditor.EditorApplication.isPlaying || UnityEditor.EditorApplication.isPaused)
		{
			if (Time.frameCount <= lastActionFrame + 1)
				return null;
		}
		#else
		if (Time.frameCount <= lastActionFrame + 1)
			return null;
		#endif
		
		lastAction = ActionManager.Instance.PlayAction(actionName, autoPlay, stopConflictActions, copyTargets);
		lastActionFrame = Time.frameCount;
		return lastAction;
	}
}

public class ActionHelper : MonoBehaviour
{
	public void ForceStart()
	{
		actionHelperMap.Clear();
		foreach (ActionHelperStorage helper in actionHelpers)
			actionHelperMap.Add(helper.helperName, helper);
	}

	void Start () 
	{
		actionHelperMap.Clear();
		foreach (ActionHelperStorage helper in actionHelpers)
		{
			actionHelperMap.Add(helper.helperName, helper);
			helper.autoPlay = true;
			if (helper.playOnStart)
				helper.PlayAction();
		}
	}
	
	void OnDestroy()
	{
		foreach (ActionHelperStorage helper in actionHelpers)
		{
			helper.StopLastAction();
		}
		
	}

	void Update()
	{
		Animator animator = gameObject.GetComponent<Animator>();
		if (animator)
		{
			foreach (ActionHelperStorage helper in actionHelpers)
			{
				if (helper.detectStatePath.Length > 0)
				{
					bool active = false;
					for (int i=0; i<animator.layerCount; i++)
					{
						AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(i);
						if (stateInfo.nameHash == helper.GetDetectStatePathHash())
						{
							active = true;
							break;
						}
						stateInfo = animator.GetNextAnimatorStateInfo(i);
						if (animator.IsInTransition(i) && stateInfo.nameHash == helper.GetDetectStatePathHash())
						{
							active = true;
							break;
						}
					}
					if (active)
					{
						if (!helper.waitForEvents && !helper.IsLastActionActive())
							helper.PlayAction();
					}
					else
					{
						helper.StopLastAction();
					}
				}
			}
		}
	}

	public void BeginAction(string _actionHelperName)
	{
		//AgeLogger.Log(_actionHelperName + " " + Time.frameCount);

		ActionHelperStorage helper = null;
		bool hasHelper = actionHelperMap.TryGetValue(_actionHelperName, out helper);
		if (!hasHelper) return;
		if (!helper.waitForEvents) return;

		Animator animator = gameObject.GetComponent<Animator>();
		if (animator == null) return;

		if (helper.detectStatePath.Length > 0)
		{
			bool active = false;
			for (int i=0; i<animator.layerCount; i++)
			{
				AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(i);
				if (stateInfo.nameHash == helper.GetDetectStatePathHash())
				{
					active = true;
					break;
				}
				stateInfo = animator.GetNextAnimatorStateInfo(i);
				if (animator.IsInTransition(i) && stateInfo.nameHash == helper.GetDetectStatePathHash())
				{
					active = true;
					break;
				}
			}
			if (active)
				helper.PlayAction();
		}
		else
		{
			helper.PlayAction();
		}
	}

	public void EndAction(string _actionHelperName)
	{
		ActionHelperStorage helper = null;
		bool hasHelper = actionHelperMap.TryGetValue(_actionHelperName, out helper);
		if (!hasHelper) return;
		if (!helper.waitForEvents) return;
		
		Animator animator = gameObject.GetComponent<Animator>();
		if (animator == null) return;

		helper.StopLastAction();
	}

	public Action PlayAction(string _actionHelperName)
	{
		ActionHelperStorage helper = null;
		bool hasHelper = actionHelperMap.TryGetValue(_actionHelperName, out helper);
		if (!hasHelper) 
			return null;
		helper.autoPlay = true;
		return helper.PlayAction();
	}

	public Action PlayAction(string _actionHelperName, Dictionary<string, GameObject> dictionary)
	{
		ActionHelperStorage helper = null;
		bool hasHelper = actionHelperMap.TryGetValue(_actionHelperName, out helper);
		if (!hasHelper) 
			return null;
		helper.autoPlay = true;
		return helper.PlayAction(dictionary);
	}
	
	public Action PlayAction(int index)
	{
		if (index < 0 || index > actionHelpers.Length)
			return null;
		ActionHelperStorage helper = actionHelpers[index];
		if (helper == null)
			return null;
		helper.autoPlay = true;
		return helper.PlayAction();
	}
	
	public void Restart()
	{
		foreach (ActionHelperStorage helper in actionHelpers)
		{
			if (helper.autoPlay)
				ActionManager.Instance.PlayAction(helper.actionName, helper.autoPlay, helper.stopConflictActions, helper.targets);
		}
	}

	public ActionHelperStorage[] actionHelpers = new ActionHelperStorage[0];
	Dictionary<string, ActionHelperStorage> actionHelperMap = new Dictionary<string, ActionHelperStorage>();
}
}

