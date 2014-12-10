//==================================================================================================
// File      : ATextField.cs
// Brief     : TextField control
// Create    : 2014-01-08
// Author    : figolai <figolai@tencent.com>
// Copyright : (C) 2014 Tencent Engine Technology Center, all rights reserved.
// Website   : http://www.tencent.com
//==================================================================================================

using UnityEngine;
using System.Collections;

namespace AGE{

public class ATextField : AWidget {

	protected string    mText;
	protected int       mMaxLength;

	//signal
	public SIGNAL textChangedSignal;
	
	public void InitUI( Rect posRect, string text, int maxLength){
		base.InitUI(posRect);
		mText       = text;
		mMaxLength  = maxLength;
	}

	public void InitUI( Rect posRect, string text){
		this.InitUI(posRect, text, 65535);
	}

	protected override void BeginDrawSelf(){
		string newText = GUI.TextField(mRect, mText, mMaxLength);

		if( newText != mText){
			mText = newText;
			OnTextChanged();
		}
	}

	void OnTextChanged(){
		EMIT(textChangedSignal, new Hashtable(){
			{"sender",		this},
			{"text",	    mText},
		});	
	}

}
}

