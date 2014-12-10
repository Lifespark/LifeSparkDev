
using UnityEngine;
using System.Collections;

namespace AGE{

public class AListItem : ALabel 
{
	public Vector2 mItemSize;
	
	
	
	// draw
	protected override void BeginDrawSelf(){
		if( mFixSize )
			GUI.Label( mRect, mContent);
		else
			GUILayout.Label( mContent );
	}
}
}
