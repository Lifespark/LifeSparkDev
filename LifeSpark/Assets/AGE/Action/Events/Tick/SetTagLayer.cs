using UnityEngine;
using System.Collections;

namespace AGE{
	
	[EventCategory("Utility")]
	public class SetTagLayer : TickEvent
	{
		[ObjectTemplate(true)]
		public int targetId = -1;

		public bool   enableLayer = false;
		public int    layer = 0;
		public bool   enableTag = false;
		public string tag = ""; 
		
		public override void Process (Action _action, Track _track)
		{

			GameObject targetObject = _action.GetGameObject(targetId);
			if( targetObject == null )
			{
				AgeLogger.LogWarning( "not find setting layer/tag target object" );
				return;
			}
			
			if(enableLayer)
			{
				targetObject.layer = layer;
				
				Transform[] transforms = targetObject.GetComponentsInChildren<Transform>();
				for (int i=0; i<transforms.Length; ++i)
				{
					transforms[i].gameObject.layer = layer;
				}
			}
			
			if(enableTag)
			{
				targetObject.tag = tag;
				
				Transform[] transforms = targetObject.GetComponentsInChildren<Transform>();
				for (int i=0; i<transforms.Length; ++i)
				{
					transforms[i].gameObject.tag = tag;
				}
			}
		}
	}
}
