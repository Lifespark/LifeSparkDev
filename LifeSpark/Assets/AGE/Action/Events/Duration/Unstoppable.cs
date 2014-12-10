using UnityEngine;
using System.Collections;

namespace AGE{

[EventCategory("ActionControl")]
public class Unstoppable : DurationEvent
{
	public override void Enter(Action _action, Track _track)
	{
		_action.unstoppable = true;
	}
	
	public override void Leave(Action _action, Track _track)
	{
		_action.unstoppable = false;
	}
	
}
}
