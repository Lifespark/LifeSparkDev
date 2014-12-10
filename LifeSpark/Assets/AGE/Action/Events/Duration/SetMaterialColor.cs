using UnityEngine;
using System.Collections;


namespace AGE
{
	[EventCategory("Utility")]
	public class SetMaterialColor : DurationEvent 
	{
		[ObjectTemplate]
		public int targetId = 0;

		public string colorName = "";
		public CurveContainer color = null;

		public override void Enter(Action _action, Track _track)
		{
			GameObject targetObject = _action.GetGameObject(targetId);
			if (targetObject == null || targetObject.renderer == null)
			{
				AgeLogger.LogError("[AGE] Action:["+_action.name +" ] Event SetMaterialColor can't find targetObject or renderer of target!");
				return;
			}
			if (targetObject != null)
			{
				targetObject.renderer.material.SetColor(colorName, color.SampleColor(0));
			}
		}

		public override void Process(Action _action, Track _track, float _localTime) 
		{
			GameObject targetObject = _action.GetGameObject(targetId);
			if (targetObject != null)
			{
				targetObject.renderer.material.SetColor(colorName, color.SampleColor(_localTime/length));
			}
		}
	}
}
