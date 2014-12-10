//==================================================================================================
// File      : AWidget.cs
// Brief     : AWidget 
// Create    : 2014-01-07
// Author    : figolai <figolai@tencent.com>
// Copyright : (C) 2014 Tencent Engine Technology Center, all rights reserved.
// Website   : http://www.tencent.com
//==================================================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AGE{

public class AWidget : AObject {
	
	public bool mFixSize = true;

	// base property
	public Rect  mRect;
	
	// base
	protected AWidget parentWidget;
	protected List<AWidget> childrenWidget;

	protected virtual void OnEnable()
	{
		childrenWidget = new List<AWidget>();
		parentWidget   = null;
	}

	protected virtual void OnDisable()
	{
		if( childrenWidget != null )
		{
			foreach( AWidget wgt in childrenWidget )
				wgt.OnDisable();
		}
	}

	public void InitUI( Rect posRect){
		mRect = posRect;
	}
	
	public virtual void InitUI( float x, float y, float w, float h )
	{
		if( mRect == null )
			mRect = new Rect(x,y,w,h);
		else
			mRect.Set(x,y,w,h);
	}

	public AWidget GetParent(){
		return parentWidget;
	}

	public void SetParent(AWidget newParent){

		if( newParent && newParent.HasChild(this) )
			return;
		if( parentWidget != newParent && parentWidget)
			parentWidget.RemoveChild(this);
		if( newParent )
			newParent.AddChild(this);

		parentWidget = newParent;
	}

	public bool HasChild( AWidget child){
		return childrenWidget.Contains( child );
	}

	public void AddChild(AWidget child){
		childrenWidget.Add(child);
	}

	public void RemoveChild(AWidget child){
		childrenWidget.Remove(child);
	}

	// draw:
	public virtual void OnDraw(){
		BeginDrawSelf();
		DrawChildren();
		EndDrawSelf();
	}
	
	protected virtual void BeginDrawSelf(){
		//override this in inherited ui obj
	}

	protected virtual void EndDrawSelf(){
		//override this in inherited ui obj
	}

	protected virtual void DrawChildren(){
		if(childrenWidget == null)
			return;
		foreach (AWidget childWidget in childrenWidget) {
			childWidget.OnDraw();
		}
	}
}
}

