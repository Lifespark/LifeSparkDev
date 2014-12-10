using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AGE{

public class Track
{
	public Track(Action _action, System.Type _eventType)
	{
		action = _action;
		
		eventType = _eventType;
		if (eventType.IsSubclassOf(typeof(DurationEvent)))
			isDurationEvent = true;
		if (eventType.IsSubclassOf(typeof(TickCondition)) || eventType.IsSubclassOf(typeof(DurationCondition)))
			isCondition = true;

		BaseEvent test = (BaseEvent)System.Activator.CreateInstance(eventType);
		supportEditMode = test.SupportEditMode();

		curTime = 0;
	}
	
	public Track Clone()
	{
		Track result = new Track(action, eventType);
		foreach (BaseEvent trackEvent in trackEvents)
		{
			BaseEvent ce = trackEvent.Clone();
			ce.track = result;
			result.trackEvents.Add(ce);
		}
		result.waitForConditions = waitForConditions;
		result.enabled = enabled;
		result.color = color;
		result.trackName = trackName;
		result.execOnActionCompleted = execOnActionCompleted;
		result.execOnForceStopped = execOnForceStopped;
		result.stopAfterLastEvent = stopAfterLastEvent;
		return result;
	}
	
	public BaseEvent AddEvent(float _time, float _length)
	{
		BaseEvent newEvent = (BaseEvent)System.Activator.CreateInstance(eventType);
		newEvent.time = _time;
		
		if (isDurationEvent)
			(newEvent as DurationEvent).length = _length;

		float insertPosFloat = 0;
		if (LocateInsertPos(_time, out insertPosFloat))
		{
			int insertPos = (int)(insertPosFloat + 1);
			if (insertPos > trackEvents.Count) insertPos = trackEvents.Count;
			trackEvents.Insert(insertPos, newEvent);
		}
		else
		{
			trackEvents.Add(newEvent);
		}

		newEvent.track = this;
		return newEvent;
	}
	
	public bool LocateEvent(float _curTime, out float _result)
	{
		_result = 0.0f;
		
		int eventCount = trackEvents.Count;
		if (eventCount==0) return false;
	
		if (Loop)
		{
			while (_curTime < 0) _curTime += Length;
			while (_curTime >= Length) _curTime -= Length;
		}
		else
		{
			if (_curTime < 0) _curTime = 0;
			else if (_curTime > Length) _curTime = Length;
		}
	
		int eventIndex = -1;
	
		int begin = 0;
		int end = trackEvents.Count - 1;
	
		while (begin != end)
		{
			int mid = (begin + end) / 2 + 1;
			if (_curTime < (trackEvents[mid] as BaseEvent).time)
			{
				//search left branch
				end = mid - 1;
			}
			else
			{
				//search right branch
				begin = mid;
			}
		}
	
		if (begin == 0 && _curTime < (trackEvents[0] as BaseEvent).time)
			eventIndex = -1;
		else
			eventIndex = begin;
	
		if (eventIndex < 0) //before the first event
		{
			if (Loop)
			{
				float beginSpace = (trackEvents[0] as BaseEvent).time;
				float endSpace = Length - (trackEvents[eventCount-1] as BaseEvent).time;
				_result = (eventCount - 1) + (_curTime + endSpace) / (beginSpace + endSpace);
			}
			else
			{
				_result = -1 + _curTime / (trackEvents[0] as BaseEvent).time;
			}
		}
		else if (eventIndex == eventCount - 1) //the last event
		{
			if (Loop)
			{
				float beginSpace = (trackEvents[0] as BaseEvent).time;
				float endSpace = Length - (trackEvents[eventCount-1] as BaseEvent).time;
				_result = (eventCount - 1) + (_curTime - (trackEvents[eventCount-1] as BaseEvent).time) / (beginSpace + endSpace);
			}
			else
			{
				_result = (eventCount - 1) + (_curTime - (trackEvents[eventCount-1] as BaseEvent).time) / (Length - (trackEvents[eventCount-1] as BaseEvent).time);
			}
		}
		else
		{
			_result = eventIndex + (_curTime - (trackEvents[eventIndex] as BaseEvent).time) / ((trackEvents[eventIndex+1] as BaseEvent).time - (trackEvents[eventIndex] as BaseEvent).time);
		}
		return true;
	}
	bool LocateInsertPos(float _curTime, out float _result)
	{
		_result = 0.0f;
		
		int eventCount = trackEvents.Count;
		if (eventCount==0) return false;
	
		{
			if (_curTime < 0) _curTime = 0;
			else if (_curTime > Length) _curTime = Length;
		}
	
		int eventIndex = -1;
	
		int begin = 0;
		int end = trackEvents.Count - 1;
	
		while (begin != end)
		{
			int mid = (begin + end) / 2 + 1;
			if (_curTime < (trackEvents[mid] as BaseEvent).time)
			{
				//search left branch
				end = mid - 1;
			}
			else
			{
				//search right branch
				begin = mid;
			}
		}
	
		if (begin == 0 && _curTime < (trackEvents[0] as BaseEvent).time)
			eventIndex = -1;
		else
			eventIndex = begin;
	
		if (eventIndex < 0) //before the first event
		{
			{
				_result = -1 + _curTime / (trackEvents[0] as BaseEvent).time;
			}
		}
		else if (eventIndex == eventCount - 1) //the last event
		{
			{
				_result = (eventCount - 1) + (_curTime - (trackEvents[eventCount-1] as BaseEvent).time) / (Length - (trackEvents[eventCount-1] as BaseEvent).time);
			}
		}
		else
		{
			_result = eventIndex + (_curTime - (trackEvents[eventIndex] as BaseEvent).time) / ((trackEvents[eventIndex+1] as BaseEvent).time - (trackEvents[eventIndex] as BaseEvent).time);
		}
		return true;
	}
	
	public void Process(float _curTime)
	{
		curTime = _curTime;

		float eventPosFloat = 0;
		if (!LocateEvent(_curTime, out eventPosFloat) || eventPosFloat < 0)
			return;

		int eventPos = (int)eventPosFloat;
		if (_curTime >= Length && !Loop)
			eventPos = trackEvents.Count - 1;

		if (Loop)
		{
			int lastEventPos = (eventPos-1+trackEvents.Count) % trackEvents.Count;
			int nextEventPos = (eventPos+1+trackEvents.Count) % trackEvents.Count;
			if (isDurationEvent)
			{
				for (int i=lastEventPos, j=0; j<trackEvents.Count; i=(i+1)%trackEvents.Count, j++)
				{
					DurationEvent durEvent = trackEvents[i] as DurationEvent;
					if (CheckSkip(_curTime, durEvent.Start) && durEvent.CheckConditions(action))
					{
						if (activeEvents.Count == 0)
						{
							//not blending
							durEvent.Enter(action, this);
						}
						else
						{
							//blending
							DurationEvent preEvent = activeEvents[0] as DurationEvent;
							if (preEvent.Start < durEvent.Start && preEvent.End < Length)
							{
								//not looping
								float blendTime = preEvent.End - durEvent.Start;
								durEvent.EnterBlend(action, this, preEvent, blendTime);
							}
							else if (preEvent.Start < durEvent.Start && preEvent.End >= Length)
							{
								//looping
								float blendTime = preEvent.End - durEvent.Start;
								durEvent.EnterBlend(action, this, preEvent, blendTime);
							}
							else
							{
								//looped
								float blendTime = preEvent.End - Length - durEvent.Start;
								durEvent.EnterBlend(action, this, preEvent, blendTime);
							}
						}
						activeEvents.Add(durEvent);
					}
					if (CheckSkip(_curTime, durEvent.End) && activeEvents.Contains(durEvent))
					{
						durEvent.Leave(action, this);
						activeEvents.Remove(durEvent);
					}
				}
			}
			else
			{
				for (int i=lastEventPos, j=0; j<trackEvents.Count; i=(i+1)%trackEvents.Count, j++)
				{
					TickEvent ticEvent = trackEvents[i] as TickEvent;
					if (CheckSkip(_curTime, ticEvent.time) && ticEvent.CheckConditions(action))
					{
						ticEvent.Process(action, this);
					}
				}
				
				//process tick event blending (key frames)
				if (eventPos != nextEventPos)
				{
					TickEvent ticEvent = trackEvents[eventPos] as TickEvent;
					TickEvent nexEvent = trackEvents[nextEventPos] as TickEvent;
					if (nexEvent.time > ticEvent.time)
					{
						//not looped
						float blendWeight = (_curTime - ticEvent.time) / (nexEvent.time - ticEvent.time);
						nexEvent.ProcessBlend(action, this, ticEvent, blendWeight);
					}
					else if (nexEvent.time < ticEvent.time)
					{
						if (_curTime >= ticEvent.time)
						{
							//pre loop
							float blendWeight = (_curTime - ticEvent.time) / (nexEvent.time + Length - ticEvent.time);
							nexEvent.ProcessBlend(action, this, ticEvent, blendWeight);
						}
						else
						{
							//looped
							float blendWeight = (_curTime + Length - ticEvent.time) / (nexEvent.time + Length - ticEvent.time);
							nexEvent.ProcessBlend(action, this, ticEvent, blendWeight);
						}
					}
				}
				else
				{
					TickEvent ticEvent = trackEvents[eventPos] as TickEvent;
					if (_curTime > ticEvent.time)
					{
						//not looped
						float localTime = _curTime - ticEvent.time;
						ticEvent.PostProcess(action, this, localTime);
					}
					else if (_curTime < ticEvent.time)
					{
						if (_curTime >= ticEvent.time)
						{
							//pre loop
							float localTime = _curTime - ticEvent.time;
							ticEvent.PostProcess(action, this, localTime);
						}
						else
						{
							//looped
							float localTime = _curTime + Length - ticEvent.time;
							ticEvent.PostProcess(action, this, localTime);
						}
					}
				}
			}
		}
		else
		{
			int lastEventPos = eventPos - 1;
			if (lastEventPos < 0) lastEventPos = 0;
			int nextEventPos = eventPos + 1;
			if (nextEventPos >= trackEvents.Count) nextEventPos = eventPos;
			
			if (isDurationEvent)
			{
				for (int i=lastEventPos; i<trackEvents.Count; i++)
				{
					DurationEvent durEvent = trackEvents[i] as DurationEvent;
					if (CheckSkip(_curTime, durEvent.Start) && durEvent.CheckConditions(action))
					{
						if (activeEvents.Count == 0)
						{
							//not blending
							durEvent.Enter(action, this);
						}
						else
						{
							//blending
							DurationEvent preEvent = activeEvents[0] as DurationEvent;
							float blendTime = preEvent.End - durEvent.Start;
							durEvent.EnterBlend(action, this, preEvent, blendTime);
						}
						activeEvents.Add(durEvent);
					}
					if (CheckSkip(_curTime, durEvent.End) && activeEvents.Contains(durEvent))
					{
						if( activeEvents.Count > 1 )
						{
							//do leave blending
							DurationEvent nextEvent = activeEvents[1] as DurationEvent;
							float blendTime = durEvent.End - nextEvent.Start;
							durEvent.LeaveBlend(action, this, nextEvent, blendTime);
						}
						else
						{
							//leave
							durEvent.Leave(action, this);
						}
						activeEvents.Remove(durEvent);
					}
				}
			}
			else
			{
				for (int i=lastEventPos; i<trackEvents.Count; i++)
				{
					TickEvent ticEvent = trackEvents[i] as TickEvent;
					if (CheckSkip(_curTime, ticEvent.time) && ticEvent.CheckConditions(action))
					{
						ticEvent.Process(action, this);
					}
				}
				
				//process tick event blending (key frames)
				if (eventPos != nextEventPos)
				{
					TickEvent ticEvent = trackEvents[eventPos] as TickEvent;
					TickEvent nexEvent = trackEvents[nextEventPos] as TickEvent;
					float blendWeight = (_curTime - ticEvent.time) / (nexEvent.time - ticEvent.time);
					nexEvent.ProcessBlend(action, this, ticEvent, blendWeight);
				}
				else
				{
					TickEvent ticEvent = trackEvents[eventPos] as TickEvent;
					float localTime = _curTime - ticEvent.time;
					ticEvent.PostProcess(action, this, localTime);
				}
			}
		}
		
		//process duration events
		if (activeEvents.Count == 1)
		{
			//not blending
			DurationEvent durEvent = activeEvents[0] as DurationEvent;
			if (_curTime >= durEvent.Start)
				durEvent.Process(action, this, _curTime - durEvent.Start);
			else
				durEvent.Process(action, this, _curTime + Length - durEvent.Start);
		}
		else if (activeEvents.Count == 2)
		{
			//blending
			DurationEvent preEvent = activeEvents[0] as DurationEvent;
			DurationEvent durEvent = activeEvents[1] as DurationEvent;
			if (preEvent.Start < durEvent.Start && preEvent.End < Length)
			{
				//not looping
				float localTime = _curTime - durEvent.Start;
				float prevLocalTime = _curTime - preEvent.Start;
				float blendWeight = (_curTime - durEvent.Start) / (preEvent.End - durEvent.Start);
				durEvent.ProcessBlend(action, this, localTime, preEvent, prevLocalTime, blendWeight);
			}
			else if (preEvent.Start < durEvent.Start && preEvent.End >= Length)
			{
				//looping
				if (_curTime >= durEvent.Start)
				{
					float localTime = _curTime - durEvent.Start;
					float prevLocalTime = _curTime - preEvent.Start;
					float blendWeight = (_curTime - durEvent.Start) / (preEvent.End - durEvent.Start);
					durEvent.ProcessBlend(action, this, localTime, preEvent, prevLocalTime, blendWeight);
				}
				else
				{
					float localTime = _curTime + Length - durEvent.Start;
					float prevLocalTime = _curTime + Length - preEvent.Start;
					float blendWeight = (_curTime + Length - durEvent.Start) / (preEvent.End - durEvent.Start);
					durEvent.ProcessBlend(action, this, localTime, preEvent, prevLocalTime, blendWeight);
				}
			}
			else
			{
				//looped
				float localTime = _curTime - durEvent.Start;
				float prevLocalTime = _curTime + Length - preEvent.Start;
				float blendWeight = (_curTime - durEvent.Start) / (preEvent.End - Length - durEvent.Start);
				durEvent.ProcessBlend(action, this, localTime, preEvent, prevLocalTime, blendWeight);
			}
		}
		
		looped = false;
		lastTime = _curTime;
	}
	
	protected bool CheckSkip(float _curTime, float _checkTime)
	{
		if (!Loop)
		{
			if (_checkTime < _curTime && _checkTime >= _curTime - action.deltaTime) return true;
			else return false;
		}
		else
		{
			float curTime = _curTime;
			float preTime = curTime - action.deltaTime;
			
			if (_checkTime < 0.0f) _checkTime += Length;
			else if (_checkTime >= Length) _checkTime -= Length;
			
			//note that curTime is always < Length
			if (preTime >= 0.0f)
			{
				if (_checkTime < curTime && _checkTime >= preTime) return true;
				else return false;
			}
			else// if (preTime < 0.0f)
			{
				if (_checkTime < curTime && _checkTime >=0 || _checkTime <= Length && _checkTime >= preTime + Length) return true;
				else return false;
			}
		}
	}
	
	public BaseEvent GetOffsetEvent(BaseEvent _curEvent, int _offset)
	{
		int curIndex = trackEvents.LastIndexOf(_curEvent);
		if (Loop)
		{
			int resultIndex = (curIndex + _offset) % trackEvents.Count;
			if (resultIndex < 0) resultIndex += trackEvents.Count;
			return trackEvents[resultIndex] as BaseEvent;
		}
		else
		{
			int resultIndex = curIndex + _offset;
			if (resultIndex < 0 || resultIndex >= trackEvents.Count) return null;
			else return trackEvents[resultIndex] as BaseEvent;
		}
	}

	public BaseEvent GetEvent( int index )
	{
		if( index >= 0 && index < trackEvents.Count )
			return trackEvents[index] as BaseEvent;
		return null;
	}

	public int GetIndexOfEvent(BaseEvent _curEvent)
	{
		int curIndex = trackEvents.LastIndexOf(_curEvent);
		return curIndex;
	}

	public int GetEventsCount()
	{
		return trackEvents.Count;
	}
	
	public void DoLoop()
	{
		looped = true;
	}
	
	public void Start(Action _action)
	{
		if (!enabled)
			return;

		looped = _action.loop;

		if (!isCondition)
			_action.SetCondition(this, true);

		curTime = 0.0f;

		started = true;
	}
	
	public void Stop(Action _action)
	{
		if (!started) return;
		foreach (DurationEvent activeEvent in activeEvents)
			activeEvent.Leave(action, this);
		activeEvents.Clear();

		if (!isCondition)
			_action.SetCondition(this, false);

		started = false;
	}
	
	public float Length
	{
		get { return action.length; }
	}
	
	public bool Loop
	{
		get { return action.loop; }
	}
	
	public bool IsDurationEvent
	{
		get { return isDurationEvent; }
	}
	
	public System.Type EventType
	{
		get { return eventType; }
	}

	public bool SupportEditMode()
	{
		return supportEditMode;
	}

	System.Type eventType = null;
	bool isDurationEvent = false;
	bool isCondition = false;
	float lastTime = 0.0f;
	public ArrayList trackEvents = new ArrayList();
	public Action action = null;
	bool looped = false;
	public bool started = false;
	public bool enabled = false;

	bool supportEditMode = false;
	
	ArrayList activeEvents = new ArrayList(); //duration events only!

	public bool IsBlending()
	{
		return (activeEvents != null && activeEvents.Count > 1);
	}
	
	public bool CheckConditions(Action _action)
	{
		foreach (int conditionId in waitForConditions.Keys)
		{
			if (conditionId >= 0 && conditionId <_action.GetConditionCount())
			{
				Track conditionTrack = _action.tracks[conditionId] as Track;
				if (!_action.GetConditionChange(conditionTrack) || _action.GetCondition(conditionTrack) != waitForConditions[conditionId])
					return false;
			}
		}
		return true;
	}

	//returns a time to ensure all events can be finished
	public float GetEventEndTime()
	{
		if (trackEvents.Count == 0) return 0;
		if (isDurationEvent)
			return (trackEvents[trackEvents.Count-1] as DurationEvent).End + 0.0333f;
		else
			return (trackEvents[trackEvents.Count-1] as TickEvent).time + 0.0333f;
	}

		public Dictionary<string, bool> GetAssociatedActions()
		{
			Dictionary<string, bool> result = new Dictionary<string, bool>();
			foreach (BaseEvent trackEvent in trackEvents)
			{
				Dictionary<string, bool> eventResources = trackEvent.GetAssociatedActions();
				if (eventResources != null)
				{
					foreach (string resName in eventResources.Keys)
					{
						if (result.ContainsKey(resName))
							result[resName] |= eventResources[resName];
						else
							result.Add(resName, eventResources[resName]);
					}
				}
			}
			return result;
		}

		public Dictionary<string, bool> GetAssociatedResources()
		{
			Dictionary<string, bool> result = new Dictionary<string, bool>();
			foreach (BaseEvent trackEvent in trackEvents)
			{
				Dictionary<string, bool> eventResources = trackEvent.GetAssociatedResources();
				if (eventResources != null)
				{
					foreach (string resName in eventResources.Keys)
					{
						if (result.ContainsKey(resName))
							result[resName] |= eventResources[resName];
						else
							result.Add(resName, eventResources[resName]);
					}
				}
			}
			return result;
		}

	public Dictionary<int, bool> waitForConditions = new Dictionary<int, bool>();
	public float curTime;

	public int trackIndex = -1;

	public Color color = Color.red;
		
	public string trackName = "";
	public bool execOnActionCompleted = false;
	public bool execOnForceStopped = false;

	public bool stopAfterLastEvent = true; //only affect condition-depended tracks
}
}
