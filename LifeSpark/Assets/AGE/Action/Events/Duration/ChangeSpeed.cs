using UnityEngine;
using System.Collections;

namespace AGE
{
	
	[EventCategory("ActionControl")]
	public class ChangeSpeed : DurationEvent
	{
		public enum Mode
		{ 
			Specified = 0, 
			AverageSpeed = 1, 
		};
		
		public Mode mode = Mode.Specified;
		
		//specified
		public CurveContainer playSpeed = null;
		
		//by average speed of object
		public float averageSpeed = 1.0f;
		public int fromId = -1;
		public int toId = -1;

		public override bool SupportEditMode ()
		{
			return true;
		}

		public override void Enter(Action _action, Track _track)
		{
			switch (mode)
			{
			case Mode.Specified:
			{
				_action.SetPlaySpeed(playSpeed.SampleFloat(0));
				break;
			}
			case Mode.AverageSpeed:
			{
				GameObject fromObject = _action.GetGameObject(fromId);
				GameObject toObject = _action.GetGameObject(toId);
				if (fromObject != null && toObject != null)
				{
					float range = averageSpeed * length;
					float realRange = (toObject.transform.position - fromObject.transform.position).magnitude;
					_action.SetPlaySpeed(range / realRange);
				}
				break;
			}
			}
		}

		public override void Process(Action _action, Track _track, float _localTime) 
		{
			if (mode == Mode.Specified)
			{
				_action.SetPlaySpeed(playSpeed.SampleFloat(_localTime/length));
			}
		}
		
		public override void Leave(Action _action, Track _track)
		{
			if( _track.IsBlending() )
				return;
			_action.SetPlaySpeed(1.0f);
		}
		
	}
}
