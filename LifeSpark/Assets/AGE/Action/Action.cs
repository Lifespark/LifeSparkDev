using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AGE{

public class Action : MonoBehaviour 
{
	float playSpeedOnPause = 0.0f;
	int instanceId = Random.Range(0, int.MaxValue);
	public float deltaTime = 0.0f;

	void Start()
	{
		ForceStart();
	}
	
	public void ForceStart()
	{
		time = 0.0f;
		if( tracks != null )
		{
			foreach (Track track in tracks)
			{
				if( track.execOnForceStopped || track.execOnActionCompleted )
					continue;
				if (track.waitForConditions.Count == 0)
					track.Start(this);
			}
		}
	}

	public void Play()
	{
		if( enabled )
			return;
		enabled = true;
		playSpeed = playSpeedOnPause;
		SetPlaySpeed(playSpeedOnPause);
	}
	
	public void Pause()
	{
		if( !enabled )
			return;
		enabled = false;
		playSpeedOnPause = playSpeed;
		SetPlaySpeed(0.0f);
	}
	
	public void Stop()
	{
		if( tracks != null )
		{
			foreach (Track track in tracks)
			{
				if( track.execOnForceStopped || track.execOnActionCompleted )
				{
					track.Start(this);
					float oldDeltaTime = deltaTime;
					deltaTime = length+0.00001f;
					track.Process(deltaTime);
					track.Stop(this);
					deltaTime = oldDeltaTime;
				}
				else if (track.started)
					track.Stop(this);
			}
		}
		if( tempObjsAffectedByPlaySpeed != null )
			tempObjsAffectedByPlaySpeed.Clear();
		if( ActionManager.Instance != null )
		{
			ActionManager.Instance.RemoveAction(this);
			ActionManager.DestroyGameObject(gameObject);
		}
	}
	
	void Update ()
	{
		if( !enabled || playSpeed == 0.0f )
			return;
		ForceUpdate(time+Time.deltaTime*playSpeed);
	}

	public void ForceUpdate(float _time)
	{
		deltaTime = _time - time;
		time = _time;
		if (time > length)
		{
			if (loop)
			{
				time -= length;
				foreach (Track track in tracks)
				{
					if (track.waitForConditions.Count == 0)
						track.DoLoop();
				}
			}
			else
			{
				time = length + 0.00001f;
				Process(time);
				if (parentAction == null) //sub action won't self destroy
					Stop();
				return;
			}
		}
		Process(time);
		//UpdateTempObjectSpeed();
	}

	public void LoadAction(Action _actionResource, params GameObject[] _gameObjects)
	{
		//TODO: load action resource
		length = _actionResource.length;
		loop = _actionResource.loop;
		time = 0.0f;
		actionName = _actionResource.actionName;
		templateObjectIds = _actionResource.templateObjectIds;
		refGameObjectsCount = _actionResource.refGameObjectsCount;
		
		foreach (Track track in _actionResource.tracks)
		{
#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isPlaying || UnityEditor.EditorApplication.isPaused)
			{
				AddTrack(track.Clone());
			}
			else
			{
				Track newTrack = AddTrack(track.Clone());
				if (!track.SupportEditMode())
					newTrack.enabled = false;
			}
#else
			AddTrack(track.Clone());
#endif
		}
		
		//gameObjects = _gameObjects;
		foreach (GameObject target in _gameObjects)
			gameObjects.Add(target);

		CopyRefParam( _actionResource );
	}
	
	public void Process(float _time)
	{
		if( tracks != null )
		{
			foreach (Track track in tracks)
			{
				if (track.waitForConditions.Count == 0)
				{
					if (track.started)
						track.Process(_time);
				}
				else
				{
					if (!track.started && track.CheckConditions(this))
					{
						track.Start(this);
						if (!loop)
						{
							float trackLength = track.GetEventEndTime();
							if (length < trackLength)
								length = trackLength;
						}
					}
					if (track.started)
					{
						track.Process(track.curTime+deltaTime);
						if (track.curTime > track.GetEventEndTime() && track.stopAfterLastEvent)
							track.Stop(this);
					}
				}
			}
			foreach (Track track in tracks)
				conditionChanges[track] = false;
		}
	}
	
	public Track AddTrack(System.Type _eventType)
	{
		Track newTrack = new Track(this, _eventType);
		newTrack.trackIndex = tracks.Count;
		tracks.Add(newTrack);
		conditions.Add(newTrack, false);
		conditionChanges.Add(newTrack, false);
		return newTrack;
	}
	
	public Track AddTrack(Track _track)
	{
		_track.action = this;
		_track.trackIndex = tracks.Count;
		tracks.Add(_track);
		conditions.Add(_track, false);
		conditionChanges.Add(_track, false);
		return _track;
	}
	
	public GameObject GetGameObject(int _index)
	{
		if (_index < 0 || _index >= gameObjects.Count) return null;
		else return gameObjects[_index];
	}

    public void SetGameObject(int _index, GameObject _obj) {
        if (_index < 0 || _index >= gameObjects.Count) return;
        else gameObjects[_index] = _obj;
    }

	public void ClearGameObject(GameObject _gameObject)
	{
		for (int i=0; i<gameObjects.Count; i++)
			if (gameObjects[i] == _gameObject)
				gameObjects[i] = null;
	}
		
	public float CurrentTime
	{
		get
		{
			return time;
		}
	}

	public void GetTracks( System.Type evtType, ref ArrayList resLst )
	{
		if( resLst == null )
			resLst = new ArrayList();
		foreach (Track track in tracks)
		{
			if( track != null && track.EventType == evtType )
				resLst.Add( track );
		}
	}

	public bool GetCondition(Track _track)
	{
		return conditions[_track];
	}

	public bool GetConditionChange(Track _track)
	{
		return conditionChanges[_track];
	}

	public int GetConditionCount()
	{
		return conditions.Count;
	}

	public void SetCondition(Track _track, bool _status)
	{
		bool oldStatus = conditions[_track];
		if (oldStatus != _status)
		{
			conditions[_track] = _status;
			conditionChanges[_track] = true;
		}
	}

	public void SetRefParam( string paramName, object value )
	{
		refParams.SetRefParamAndData( paramName, value );
	}

	public void CopyRefParam( Action resource )
	{
		refParams.refParamList.Clear();
		refParams.refDataList.Clear();
		foreach( string k in resource.refParams.refParamList.Keys )
		{
			refParams.AddRefParam( k, resource.refParams.refParamList[k].value );
		}

		foreach( string k in resource.refParams.refDataList.Keys )
		{
			List<RefData> dl = resource.refParams.refDataList[k];
			foreach( RefData data in dl )
			{
				if( data.dataObject is Track )
				{
					Track _track = (Track)(data.dataObject);
					refParams.AddRefData( k, data.fieldInfo, tracks[_track.trackIndex] );
				}
				else if(data.dataObject is BaseEvent )
				{
					BaseEvent _event = (BaseEvent)(data.dataObject);
					int eid = _event.track.GetIndexOfEvent(_event);
					Track ct = (Track)tracks[_event.track.trackIndex];
					BaseEvent ce = ct.GetEvent(eid);
					refParams.AddRefData( k, data.fieldInfo, ce );
				}
			}
		}
	}

	public void AddTemplateObject(string str, int id)
	{
		templateObjectIds.Add(str, id);
	}

	public Dictionary<string, int> TemplateObjectIds 
	{
		get 
		{
			return templateObjectIds;
		}
	}

	public void GetRefParamNames( ref List<string> names )
	{
		if( refParams == null || refParams.refParamList == null )
			return;
		foreach( string key in refParams.refParamList.Keys )
			names.Add( key );
	}

		public Dictionary<string, bool> GetAssociatedActions()
		{
			Dictionary<string, bool> result = new Dictionary<string, bool>();
			foreach (Track track in tracks)
			{
				Dictionary<string, bool> trackResources = track.GetAssociatedActions();
				if (trackResources != null)
				{
					foreach (string resName in trackResources.Keys)
					{
						if (result.ContainsKey(resName))
							result[resName] |= trackResources[resName];
						else
							result.Add(resName, trackResources[resName]);
					}
				}
			}
			return result;
		}

		//get resource names/paths associated with this action
		//result.key stands for resource paths
		//result.value stands for whether the resource needs to be re-loaded
		public Dictionary<string, bool> GetAssociatedResources()
		{
			Dictionary<string, bool> result = new Dictionary<string, bool>();
			foreach (Track track in tracks)
			{
				Dictionary<string, bool> trackResources = track.GetAssociatedResources();
				if (trackResources != null)
				{
					foreach (string resName in trackResources.Keys)
					{
						if (result.ContainsKey(resName))
							result[resName] |= trackResources[resName];
						else
							result.Add(resName, trackResources[resName]);
					}
				}
			}
			return result;
		}

	public void AddTempObject( PlaySpeedAffectedType type, GameObject obj )
	{
		if( tempObjsAffectedByPlaySpeed == null )
			tempObjsAffectedByPlaySpeed = new Dictionary<PlaySpeedAffectedType, List<GameObject>>();
		if( !tempObjsAffectedByPlaySpeed.ContainsKey(type) )
			tempObjsAffectedByPlaySpeed.Add( type, new List<GameObject>() );
		List<GameObject> tempObjects = tempObjsAffectedByPlaySpeed[type];
		foreach( GameObject lobj in tempObjects )
		{
			if( lobj == obj )
				return;
		}
		tempObjects.Add(obj);

		//update temp object at once
		if (type == PlaySpeedAffectedType.ePSAT_Anim)
		{
			Animation[] animations = obj.GetComponentsInChildren<Animation>();
			foreach(Animation animation in animations)
			{
				if (animation.playAutomatically && animation.clip)
				{
					AnimationState state = animation[animation.clip.name];
					if( state )
						state.speed = playSpeed;
				}
			}
			
			Animator[] animators = obj.GetComponentsInChildren<Animator>();
			foreach(Animator animtor in animators )
			{
				animtor.speed = playSpeed;
			}
		}
		else
		{
			ParticleSystem[] pslst = obj.GetComponentsInChildren<ParticleSystem>();
			foreach( ParticleSystem ps in pslst )
			{
				if ( type == PlaySpeedAffectedType.ePSAT_SelfSpeed )
				{
				}
				else
					ps.playbackSpeed = playSpeed;
			}
		}
	}

	public void RemoveTempObject( PlaySpeedAffectedType type, GameObject obj )
	{
		if( tempObjsAffectedByPlaySpeed == null )
			return;
		if( !tempObjsAffectedByPlaySpeed.ContainsKey(type) )
			return;
		tempObjsAffectedByPlaySpeed[type].Remove(obj);
	}

	void UpdateTempObjectSpeed()
	{
		if (tempObjsAffectedByPlaySpeed == null) return;
		foreach( PlaySpeedAffectedType type in tempObjsAffectedByPlaySpeed.Keys )
		{
			List<GameObject> tempObjects = tempObjsAffectedByPlaySpeed[type];
			foreach( GameObject obj in tempObjects )
			{
				if( obj == null ) continue;
				
				if( type == PlaySpeedAffectedType.ePSAT_Anim )
				{
					Animation[] animations = obj.GetComponentsInChildren<Animation>();
					foreach(Animation animation in animations)
					{
						if (animation.playAutomatically && animation.clip)
						{
							AnimationState state = animation[animation.clip.name];
							if( state )
								state.speed = playSpeed;
						}
					}
					
					Animator[] animators = obj.GetComponentsInChildren<Animator>();
					foreach(Animator animtor in animators )
					{
						animtor.speed = playSpeed;
					}
				}
				else if( type == PlaySpeedAffectedType.ePSAT_Fx )
				{
					ParticleSystem[] pslst = obj.GetComponentsInChildren<ParticleSystem>();
					foreach( ParticleSystem ps in pslst )
					{
						ps.playbackSpeed = playSpeed;
					}
				}
			}
		}
	}
	
	public void SetPlaySpeed( float _speed )
	{
		playSpeed = _speed;
		if( playSpeed == 0.0f )
		{
			ForceUpdate(time);
			enabled = false;
		}
		else
		{
			enabled = true;
		}

		UpdateTempObjectSpeed();
	}

	public void UpdateTempObjectForPreview(float _oldProgress, float _newProgress)
	{
#if UNITY_EDITOR
		if( tempObjsAffectedByPlaySpeed == null )
			return;
		foreach( PlaySpeedAffectedType type in tempObjsAffectedByPlaySpeed.Keys )
		{
			List<GameObject> tempObjects = tempObjsAffectedByPlaySpeed[type];
			foreach( GameObject obj in tempObjects )
			{
				if( obj == null )
					continue;
				LifeTimeHelper lifeTimeHelper = obj.GetComponent<LifeTimeHelper>();
				
				if( type == PlaySpeedAffectedType.ePSAT_Fx )
				{
					ParticleSystem[] pslst = obj.GetComponentsInChildren<ParticleSystem>();
					foreach( ParticleSystem ps in pslst )
					{
						ps.Simulate((_newProgress - _oldProgress)/playSpeed, false, false);
					}
				}
				else 
				{
					if( type == PlaySpeedAffectedType.ePSAT_SelfSpeed )
					{
						ParticleSystem[] pslst = obj.GetComponentsInChildren<ParticleSystem>();
						foreach( ParticleSystem ps in pslst )
						{
							ps.Simulate((_newProgress - _oldProgress)/playSpeed, false, false);
						}
					}

					Animation[] animations = obj.GetComponentsInChildren<Animation>();
					foreach(Animation animation in animations)
					{
						if (animation.playAutomatically && animation.clip)
						{
							animation.gameObject.SampleAnimation( animation.clip, _newProgress - lifeTimeHelper.startTime );
						}
					}
					
					Animator[] animators = obj.GetComponentsInChildren<Animator>();
					foreach(Animator animtor in animators )
					{
						for( int i = 0; i < animtor.layerCount; ++i )
						{
							AnimatorStateInfo sinfo = animtor.GetCurrentAnimatorStateInfo(i);
							animtor.Play( sinfo.nameHash );
							AnimationInfo[] infos = animtor.GetCurrentAnimationClipState(i);
							foreach( AnimationInfo info in infos )
							{
								animtor.gameObject.SampleAnimation( info.clip, _newProgress - lifeTimeHelper.startTime );
							}
						}
					}
				}
			}
		}
#endif
	}

	public float length = 5.0f;
	public bool loop = false;

	public float PlaySpeed
	{
		set	{ SetPlaySpeed(value); }
		get { return playSpeed; }
	}

	float playSpeed = 1.0f;

	public bool unstoppable = false;
	public string actionName = "";
	
	public Action parentAction = null;

	public int refGameObjectsCount = 0;
	
	float time = 0.0f;
	//public GameObject[] gameObjects;
	public List<GameObject> gameObjects = new List<GameObject>();
	
	public ArrayList tracks = new ArrayList();
	Dictionary<Track, bool> conditions = new Dictionary<Track, bool>();
	Dictionary<Track, bool> conditionChanges = new Dictionary<Track, bool>();

	public RefParamOperator refParams = new RefParamOperator();
		
	public Dictionary<string, int> templateObjectIds = new Dictionary<string, int>();	

	public enum PlaySpeedAffectedType
	{
		ePSAT_SelfSpeed = 0,
		ePSAT_Anim = 1,
		ePSAT_Fx = 2,
	};
	Dictionary<PlaySpeedAffectedType, List<GameObject>> tempObjsAffectedByPlaySpeed = new Dictionary<PlaySpeedAffectedType, List<GameObject>>();

}
}
