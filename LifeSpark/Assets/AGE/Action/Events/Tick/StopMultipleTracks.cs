using UnityEngine;
using System.Collections;

namespace AGE{
	
	[EventCategory("Utility")]
	public class StopMultipleTracks : TickEvent
	{
		public int[] trackIds = new int[0];
		
		public override void Process (Action _action, Track _track)
		{
			foreach(int trackId in trackIds)
			{
				if (trackId >=0 && trackId < _action.tracks.Count)
				{
					(_action.tracks[trackId] as Track).Stop(_action);
				}
			}
		}
	}
}
