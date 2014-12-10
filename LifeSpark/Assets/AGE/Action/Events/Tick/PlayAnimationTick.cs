using UnityEngine;
using System.Collections;

namespace AGE
{

	[EventCategory("Animation")]
	public class PlayAnimationTick : TickEvent
	{
		public override bool SupportEditMode ()
		{
			return true;
		}

		[ObjectTemplate(typeof(Animation))]
		public int targetId = 0;
		
		public string clipName = "";
		public float crossFadeTime = 0.0f;
		public float playSpeed = 1.0f;

		public bool applyActionSpeed = false;
		
		public override void Process (Action _action, Track _track)
		{
			GameObject targetObject = _action.GetGameObject(targetId);
			if (targetObject == null || targetObject.animation == null)
			{
				AgeLogger.LogError(" Event PlayAnimationTick can't find targetObject or animation of target! by " + "Action:[ " +_action.name + " ]" );
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
				if (targetObject.animation[clipName].enabled)
					targetObject.animation.Stop();//stop same animation to allow replay
				if (crossFadeTime > 0)
					targetObject.animation.CrossFade(clipName, crossFadeTime);
				else
					targetObject.animation.Play(clipName);
				targetObject.animation[clipName].speed = playSpeed * (applyActionSpeed ? _action.PlaySpeed : 1.0f);
			}
	#else
			if (crossFadeTime > 0)
				targetObject.animation.CrossFade(clipName, crossFadeTime);
			else
				targetObject.animation.Play(clipName);
			targetObject.animation[clipName].speed = playSpeed * (applyActionSpeed ? _action.PlaySpeed : 1.0f);
	#endif
		}

		public override void ProcessBlend (Action _action, Track _track, TickEvent _prevEvent, float _blendWeight)
		{
			GameObject targetObject = _action.GetGameObject(targetId);
			if (targetObject == null || targetObject.animation == null || _prevEvent == null) return;

			if (targetObject.animation.GetClip(clipName) == null )
			{
				string objMsg = targetObject.name + " -- Parent: ";
				objMsg += (targetObject.transform.parent != null) ? targetObject.transform.parent.gameObject.name : "Null";
				AgeLogger.LogError(" Failed to find animation clip: <color=red>[ " + clipName + " ]</color>! "+ " for TargetObject: <color=red>[ " + objMsg + " ] </color> " +
				               "by Action:<color=yellow>[ "+_action.name +" ] </color>");
				return;
			}

	#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying && !UnityEditor.EditorApplication.isPaused)
			{
				PlayAnimationTick prevEvent = _prevEvent as PlayAnimationTick;
				AnimationClip clip = targetObject.animation.GetClip(prevEvent.clipName);
				float localTime = _action.CurrentTime - prevEvent.time;
				targetObject.SampleAnimation(clip, localTime / prevEvent.playSpeed / (applyActionSpeed ? 1.0f : _action.PlaySpeed));
			}
			else
			{
				targetObject.animation[clipName].speed = playSpeed * (applyActionSpeed ? _action.PlaySpeed : 1.0f);
			}
	#else
			targetObject.animation[clipName].speed = playSpeed * (applyActionSpeed ? _action.PlaySpeed : 1.0f);
	#endif
		}

		public override void PostProcess (Action _action, Track _track, float _localTime)
		{
			GameObject targetObject = _action.GetGameObject(targetId);
			if (targetObject == null) return;
			
	#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying && !UnityEditor.EditorApplication.isPaused)
			{
				if(targetObject.animation != null)
				{
					AnimationClip clip = targetObject.animation.GetClip(clipName);
					if ( clip == null )
					{
						string objMsg = targetObject.name + " -- Parent: ";
						objMsg += (targetObject.transform.parent != null) ? targetObject.transform.parent.gameObject.name : "Null";
						AgeLogger.LogError(" Failed to find animation clip: <color=red>[ " + clipName + " ]</color>! "+ " for TargetObject: <color=red>[ " + objMsg + " ] </color> " +
						               "by Action:<color=yellow>[ "+_action.name +" ] </color>");
						return;
					}
					targetObject.SampleAnimation(clip, _localTime / playSpeed / (applyActionSpeed ? 1.0f : _action.PlaySpeed));
				}
				else
				{   //sample fbx file
					AgeFbxLoader.SampleFbxAnimation(targetObject, clipName, (int)(_localTime / playSpeed * 1000 / (applyActionSpeed ? 1.0f : _action.PlaySpeed)));
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
				targetObject.animation[clipName].speed = playSpeed * (applyActionSpeed ? _action.PlaySpeed : 1.0f);
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
			targetObject.animation[clipName].speed = playSpeed * (applyActionSpeed ? _action.PlaySpeed : 1.0f);
	#endif
		}
	}
}
