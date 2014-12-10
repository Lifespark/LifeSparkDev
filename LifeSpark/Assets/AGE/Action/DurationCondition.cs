using UnityEngine;
using System.Collections;

namespace AGE{

public abstract class DurationCondition : DurationEvent
{
	public override void Process(Action _action, Track _track, float _localTime) 
	{
		bool checkResult = Check(_action, _track, _localTime);
		_action.SetCondition(_track, checkResult);
	}

	public override void Enter(Action _action, Track _track) 
	{
		bool checkResult = Check(_action, _track, 0);
		_action.SetCondition(_track, checkResult);
	}

	public override void Leave(Action _action, Track _track) 
	{
		//_action.SetCondition(_track, false);
	}

	public virtual bool Check(Action _action, Track _track) { return true; }
	public virtual bool Check(Action _action, Track _track, float _localTime) { return Check(_action, _track); }
}
}
