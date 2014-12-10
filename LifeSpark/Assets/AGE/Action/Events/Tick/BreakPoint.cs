using UnityEngine;
using System.Collections;

namespace AGE{

[EventCategory("Debug")]
public class BreakPoint : TickEvent
{
	public bool enabled = true;
	public string info = "";

	public override void Process (Action _action, Track _track)
	{
#if UNITY_EDITOR
		if (enabled && ActionManager.Instance.debugMode)
		{
			AgeLogger.Log("Action \"" + _action.name + "\" triggered break point on time: " + time.ToString() + "s\nInfo: " + info);
			UnityEditor.EditorApplication.isPaused = true;
		}
#endif
	}
}
}
