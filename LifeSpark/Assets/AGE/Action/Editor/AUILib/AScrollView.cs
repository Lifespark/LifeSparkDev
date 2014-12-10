using UnityEngine;
using System.Collections;

namespace AGE{

public class AScrollView : AWidget {

	public Vector2 mScrollPosition { get; set; }

	protected Rect mViewRect;
	protected bool mShowHorizontal;
	protected bool mShowVertical;

	public void InitUI(Rect posRect, Rect viewRect){
		this.InitUI( posRect, viewRect, false, false );
	}

	public void InitUI( Rect posRect, Rect viewRect, bool showHorizontal, bool showVertical){
		base.InitUI( posRect );
		mViewRect        = viewRect;
		mShowHorizontal  = showHorizontal;
		mShowVertical    = showVertical;
	}
	
	public void SetViewRectSize( float width, float height )
	{
		mRect.width = width;
		mRect.height = height;
		//mRect.Set( mRect.x, mRect.y, width, height );
	}
	
	public void SetRealRectSize( float width, float height )
	{
		mViewRect.width = width;
		mViewRect.height = height;
	}

	protected override void BeginDrawSelf(){
		if( mRect.width >= mViewRect.width )
		{
			mShowHorizontal = false;
			mScrollPosition.Set(0f, mScrollPosition.y);
		}
		else
			mShowHorizontal = true;
		
		if( mRect.height >= mViewRect.height )
		{
			mShowVertical = false;
			mScrollPosition.Set(mScrollPosition.x, 0f);
		}
		else
			mShowVertical = true;

		mScrollPosition = GUI.BeginScrollView( mRect, mScrollPosition, mViewRect, mShowHorizontal, mShowVertical);
	}

	protected override void EndDrawSelf(){
		GUI.EndScrollView();
	}
}
}

