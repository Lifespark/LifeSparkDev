//==================================================================================================
// File      : ALabel.cs
// Brief     : ALabel, no interaction
// Create    : 2014-01-07
// Author    : figolai <figolai@tencent.com>
// Copyright : (C) 2014 Tencent Engine Technology Center, all rights reserved.
// Website   : http://www.tencent.com
//==================================================================================================

using UnityEngine;
using System.Collections;

namespace AGE{

public class ALabel : AWidget {

	//property
	protected GUIContent mContent;
	
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

	// draw
	protected override void BeginDrawSelf(){
		GUI.Label( mRect, mContent);
	}
}
}
