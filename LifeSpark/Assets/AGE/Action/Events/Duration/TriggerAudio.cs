using UnityEngine;
using System.Collections;

namespace AGE{

[EventCategory("Animation")]
public class TriggerAudio : DurationEvent
{
	[ObjectTemplate]
	public int targetId = -1;
	public bool mutilSource = false;
	[AssetReference]
	public string audioPath = "";
	public bool   loop = false;
	public float  volume = 0.0f;
	public int  priority = 128;

	public bool enableRand = false;
	// TODO:[AssetReference]
	public string[] randPaths     = new string[0];
	public float[] probabilities  = new float[0];
	
	private AudioSource audioComp = null;
	
	public override bool SupportEditMode ()
	{
		return true;
	}
	
	public override void Enter(Action _action, Track _track)
	{
		string path = audioPath;
		if( enableRand )
		{
			path = GetRandomAudioPath(_action);
			if( path == "") return;
		}
		
		AudioClip clipLoaded = ActionManager.Instance.ResLoader.Load(path, typeof(AudioClip)) as AudioClip;
		if(clipLoaded == null)
		{
			AgeLogger.LogError(" Failed to find AudioClip: <color=red>[ " + audioPath + " ]</color>! "+ " for Event: <color=red>[ " + this.GetType().ToString() + " ] </color> " +
			               "by Action:<color=yellow>[ "+_action.name +" ] </color>");
			return;
		}
		GameObject targetObject = _action.GetGameObject(targetId);
		if( targetObject == null )
		{
			AgeLogger.LogError(" TriggerAudio failed to find targetObject !"+ "by Action:<color=yellow>[ "+_action.name +" ] </color>");
			return;
		}
		
		if(mutilSource)
		{
		    audioComp =	targetObject.AddComponent ("AudioSource") as AudioSource;
			audioComp.playOnAwake = false;
			audioComp.loop = loop;
			audioComp.volume = volume;
			audioComp.priority = priority;
			audioComp.clip = clipLoaded;
			audioComp.Play ();
		}
		else
		{
			if(targetObject.audio == null)
			{
				AgeLogger.LogError(" TargetObject without AudioSource in single source mode:" + targetObject.name + 
				               "by Action:<color=yellow>[ "+_action.name +" ] </color>");
				return;
			}

			targetObject.audio.loop = loop;
			targetObject.audio.volume = volume;
			targetObject.audio.priority = priority;
	
			targetObject.audio.clip = clipLoaded;
			targetObject.audio.Play();
		}

	}
	
	public override void Leave(Action _action, Track _track)
	{
		GameObject targetObject = _action.GetGameObject(targetId);
		if (targetObject != null )
		{
			if(mutilSource && audioComp != null)
			{
				audioComp.Stop();
				ActionManager.DestroyGameObject(audioComp);
			}
			else
			{
				targetObject.audio.Stop();
			}
		}
	}

	string GetRandomAudioPath(Action _action)
	{
		string res = "";
		if( randPaths.Length == 0)
		{
			AgeLogger.LogError("Failed to find any Random Audio path! "+ " for Event: <color=red>[ " + this.GetType().ToString() + " ] </color> " +
			               "by Action:<color=yellow>[ "+_action.name +" ] </color>");
			return res;
		}

		if( probabilities.Length > 0 && probabilities.Length != randPaths.Length)
		{
			AgeLogger.LogError("[Error Array Length for AudioPaths and Probabilities! "+ " for Event: <color=red>[ " + this.GetType().ToString() + " ] </color> " +
			               "by Action:<color=yellow>[ "+_action.name +" ] </color>");
			return res;
		}
		
		int randIndex = -1;

		float sum = 0.0f;
		for( int i=0; i < probabilities.Length; i++)
		{
			sum += probabilities[i];
		}
		
		if( sum <= 0.0f )
		{   // average probability: probabilities.Length =0 or error sum of probabilities
			randIndex = Mathf.FloorToInt( Random.Range( 0.0f, (float)(randPaths.Length)));
			// maybe randIndex = randPaths.length
			randIndex = Mathf.Min( randIndex, randPaths.Length - 1);
		}
		else
		{
			float rand = Random.value;
			float check = 0.0f;
			for( int i=0; i < probabilities.Length; i++)
			{
				check += probabilities[i] / sum;
				if( rand <= check )
				{
					randIndex = i;
					break;
				}
			}
		}
		
		if ( randIndex >= 0 && randIndex < randPaths.Length) 
		{
			res = randPaths[randIndex];
		}
		else
		{
			AgeLogger.LogError("Error Index for AudioPaths ! "+ " for Event: <color=red>[ " + this.GetType().ToString() + " ] </color> " +
			               "by Action:<color=yellow>[ "+_action.name +" ] </color>");
		}
		
		return res;
	}

}
}
