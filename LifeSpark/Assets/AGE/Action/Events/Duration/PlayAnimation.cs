using UnityEngine;
using System.Collections;

namespace AGE
{
	[EventCategory("Animation")]
	public class PlayAnimation : DurationEvent
	{
		[ObjectTemplate(typeof(Animation))]
		public int targetId = 0;

		public string clipName = "";

		public float startTime = 0.0f;
		public float endTime = 99999.0f;

		public bool applyActionSpeed = false;

		public override bool SupportEditMode ()
		{
			return true;
		}
		
		public override void Enter (Action _action, Track _track)
		{
			GameObject targetObject = _action.GetGameObject(targetId);
			if (targetObject == null || targetObject.animation == null)
			{
				AgeLogger.LogError(" Action:["+_action.name +" ] Event PlayAnimationTick can't find targetObject or animation of target!");
				return;
			}
			
			if (targetObject.animation.GetClip(clipName) == null )
			{
				string objMsg = targetObject.name + " -- Parent: ";
				objMsg += (targetObject.transform.parent != null) ? targetObject.transform.parent.gameObject.name : "Null";
				AgeLogger.LogError(" Failed to find animation clip: <color=red>[ " + clipName + " ]</color>! "+ " for TargetObject: <color=red>[ " + objMsg + " ] </color> " +
				               "by Action:<color=yellow>[ "+_action.name +" ] </color>");
				return;
			}

	#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isPlaying || UnityEditor.EditorApplication.isPaused)
			{
				targetObject.animation.Stop();
				targetObject.animation.Play(clipName);

				if (startTime < 0)
					startTime = 0;
				if (startTime > targetObject.animation[clipName].length)
					startTime = targetObject.animation[clipName].length;
				if (endTime > targetObject.animation[clipName].length)
					endTime = targetObject.animation[clipName].length;
				if (endTime < startTime)
					endTime = startTime;

				float playLength = endTime - startTime;

				targetObject.animation[clipName].time = startTime;
				targetObject.animation[clipName].speed = playLength / length * (applyActionSpeed ? _action.PlaySpeed : 1.0f);
				targetObject.animation[clipName].enabled = true;
			}
	#else
			targetObject.animation.Stop();
			targetObject.animation.Play(clipName);
			
			if (startTime < 0)
				startTime = 0;
			if (startTime > targetObject.animation[clipName].length)
				startTime = targetObject.animation[clipName].length;
			if (endTime > targetObject.animation[clipName].length)
				endTime = targetObject.animation[clipName].length;
			if (endTime < startTime)
				endTime = startTime;

			float playLength = endTime - startTime;
			
			targetObject.animation[clipName].time = startTime;
			targetObject.animation[clipName].speed = playLength / length * (applyActionSpeed ? _action.PlaySpeed : 1.0f);
			targetObject.animation[clipName].enabled = true;
	#endif
		}

		public override void EnterBlend (Action _action, Track _track, BaseEvent _prevEvent, float _blendTime)
		{
			GameObject targetObject = _action.GetGameObject(targetId);
			if (targetObject == null || targetObject.animation == null) return;

			if (targetObject.animation.GetClip(clipName) == null )
			{
				string objMsg = targetObject.name + " -- Parent: ";
				objMsg += (targetObject.transform.parent != null) ? targetObject.transform.parent.gameObject.name : "Null";
				AgeLogger.LogError(" Failed to find animation clip: <color=red>[ " + clipName + " ]</color>! "+ " for TargetObject: <color=red>[ " + objMsg + " ] </color> " +
				               "by Action:<color=yellow>[ "+_action.name +" ] </color>");
				return;
			}

	#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isPlaying || UnityEditor.EditorApplication.isPaused)
			{
				targetObject.animation.CrossFade(clipName, _blendTime);

				if (startTime < 0)
					startTime = 0;
				if (startTime > targetObject.animation[clipName].length)
					startTime = targetObject.animation[clipName].length;
				if (endTime > targetObject.animation[clipName].length)
					endTime = targetObject.animation[clipName].length;
				if (endTime < startTime)
					endTime = startTime;

				float playLength = endTime - startTime;
				
				targetObject.animation[clipName].time = startTime;
				targetObject.animation[clipName].speed = playLength / length * (applyActionSpeed ? _action.PlaySpeed : 1.0f);
				targetObject.animation[clipName].enabled = true;
			}
	#else
			targetObject.animation.CrossFade(clipName, _blendTime);

			if (startTime < 0)
				startTime = 0;
			if (startTime > targetObject.animation[clipName].length)
				startTime = targetObject.animation[clipName].length;
			if (endTime > targetObject.animation[clipName].length)
				endTime = targetObject.animation[clipName].length;
			if (endTime < startTime)
				endTime = startTime;

			float playLength = endTime - startTime;
			
			targetObject.animation[clipName].time = startTime;
			targetObject.animation[clipName].speed = playLength / length * (applyActionSpeed ? _action.PlaySpeed : 1.0f);
			targetObject.animation[clipName].enabled = true;
	#endif
		}

		public override void Process (Action _action, Track _track, float _localTime)
		{
			GameObject targetObject = _action.GetGameObject(targetId);
			if (targetObject == null ) return;

	#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying && !UnityEditor.EditorApplication.isPaused)
			{
				if(targetObject.animation != null)
				{
					AnimationClip clip = targetObject.animation.GetClip(clipName);
					if (clip == null )
					{
						string objMsg = targetObject.name + " -- Parent: ";
						objMsg += (targetObject.transform.parent != null) ? targetObject.transform.parent.gameObject.name : "Null";
						AgeLogger.LogError(" Failed to find animation clip: <color=red>[ " + clipName + " ]</color>! "+ " for TargetObject: <color=red>[ " + objMsg + " ] </color> " +
						               "by Action:<color=yellow>[ "+_action.name +" ] </color>");
						return;
					}

					if (startTime < 0)
						startTime = 0;
					if (startTime > clip.length)
						startTime = clip.length;
					if (endTime > clip.length)
						endTime = clip.length;
					if (endTime < startTime)
						endTime = startTime;
					
					float playLength = endTime - startTime;
					targetObject.SampleAnimation(clip, _localTime * playLength / length / (applyActionSpeed ? 1.0f : _action.PlaySpeed));
				}
				else
				{  //sample fbx file
					float duration = 0.0f;
					AgeFbxLoader.GetFbxAnimDuration(targetObject, clipName, ref duration);

					if (startTime < 0)
						startTime = 0;
					if (startTime > duration)
						startTime = duration;
					if (endTime > duration)
						endTime = duration;
					if (endTime < startTime)
						endTime = startTime;

					float playLength = endTime - startTime;

					AgeFbxLoader.SampleFbxAnimation(targetObject, clipName, (int)(_localTime * playLength / length * 1000 / (applyActionSpeed ? 1.0f : _action.PlaySpeed)));
				
				}
			}
			else
			{
				if (targetObject.animation.GetClip(clipName) == null )
				{
					string objMsg = targetObject.name + " -- Parent: ";
					objMsg += (targetObject.transform.parent != null) ? targetObject.transform.parent.gameObject.name : "Null";
					AgeLogger.LogError(" Failed to find animation clip: <color=red>[ " + clipName + " ]</color>! "+ " for TargetObject: <color=red>[ " + objMsg + " ] </color> " +
					               "by Action:<color=yellow>[ "+_action.name +" ] </color>");
					return;
				}
				float playLength = endTime - startTime;
				targetObject.animation[clipName].speed = playLength / length * (applyActionSpeed ? _action.PlaySpeed : 1.0f);
			}
	#else
			if (targetObject.animation.GetClip(clipName) == null )
			{
				string objMsg = targetObject.name + " -- Parent: ";
				objMsg += (targetObject.transform.parent != null) ? targetObject.transform.parent.gameObject.name : "Null";
				AgeLogger.LogError(" Failed to find animation clip: <color=red>[ " + clipName + " ]</color>! "+ " for TargetObject: <color=red>[ " + objMsg + " ] </color> " +
				               "by Action:<color=yellow>[ "+_action.name +" ] </color>");
				return;
			}
			float playLength = endTime - startTime;
			targetObject.animation[clipName].speed = playLength / length * (applyActionSpeed ? _action.PlaySpeed : 1.0f);
	#endif
		}

		public override void Leave (Action _action, Track _track)
		{
			GameObject targetObject = _action.GetGameObject(targetId);
			if (targetObject == null || targetObject.animation == null) return;

			if (targetObject.animation.GetClip(clipName) == null )
			{
				string objMsg = targetObject.name + " -- Parent: ";
				objMsg += (targetObject.transform.parent != null) ? targetObject.transform.parent.gameObject.name : "Null";
				AgeLogger.LogError(" Failed to find animation clip: <color=red>[ " + clipName + " ]</color>! "+ " for TargetObject: <color=red>[ " + objMsg + " ] </color> " +
				               "by Action:<color=yellow>[ "+_action.name +" ] </color>");
				return;
			}

	#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isPlaying || UnityEditor.EditorApplication.isPaused)
			{
				targetObject.animation[clipName].enabled = false;
			}
	#else
			targetObject.animation[clipName].enabled = false;
	#endif
		}
	}
}

