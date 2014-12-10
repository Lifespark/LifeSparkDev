using UnityEngine;
using System.Collections;

namespace AGE{

[EventCategory("Utility")]
public class StopTrack : TickEvent
{
	public int trackId = -1;

	public override void Process (Action _action, Track _track)
	{
		if (trackId >=0 && trackId < _action.tracks.Count)
		{
			(_action.tracks[trackId] as Track).Stop(_action);
		}
	}
}
}
