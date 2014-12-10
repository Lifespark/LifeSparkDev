//==================================================================================================
// File      : AButton.cs
// Brief     : AButton 
// Create    : 2014-01-07
// Author    : figolai <figolai@tencent.com>
// Copyright : (C) 2014 Tencent Engine Technology Center, all rights reserved.
// Website   : http://www.tencent.com
//==================================================================================================

using UnityEngine;
using System.Collections;

namespace AGE{

public class AButton : AWidget {

	//property

	protected GUIContent mContent;

	//signal
	public SIGNAL clickedSignal;
	public SIGNAL pressedSignal;
	public SIGNAL releasedSignal;

	// init
	public void InitUI(Rect posRect, string text){
		this.InitUI(posRect, new GUIContent(text));
	}

	public void InitUI(Rect posRect,Texture icon){
		this.InitUI(posRect, new GUIContent(icon));
	}

	public void InitUI(Rect posRect, GUIContent content){
		base.InitUI(posRect);
		mContent = content;
	}

	public void InitUI( Rect posRect, string text, Texture icon, string tooltip){
		this.InitUI(posRect, new GUIContent(text, icon, tooltip));
	}
	
/*  // no use style now
	public void InitUI( Rect posRect, string text, GUIStyle style){
		this.InitUI(posRect, new GUIContent(text), style);
	}

	public void InitUI( Rect posRect, Texture icon, GUIStyle style){
		this.InitUI(posRect, new GUIContent(icon), style);
	}

	public void InitUI(Rect posRect, GUIContent content, GUIStyle style){
		base.InitUI(posRect);
		mContent = content;
		mStyle   = style;
	}

	public void InitUI( Rect posRect, string text, Texture icon, string tooltip, GUIStyle style){
		this.InitUI(posRect, new GUIContent(text, icon, tooltip), style);
	}
*/
	
	// draw
	protected override void BeginDrawSelf()
	{
		if( mFixSize )
		{
			if(GUI.Button( mRect, mContent))
				OnMouseClicked();
		}
		else
		{
			if(GUILayout.Button( mContent ))
				OnMouseClicked();
		}
	}


	void OnMouseClicked(){
		EMIT(clickedSignal, new Hashtable(){
			{"sender",		this}
		});	
	}

	void OnMousePressed(){
		//emit
	}

	void OnMouseRelease(){
		//emit
	}

	void OnMouseEnter(){
	}

	void OnMouseLeave(){
	}
	
}
}

