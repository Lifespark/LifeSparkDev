using UnityEngine;
using System.Collections;

namespace AGE{

public class AToggle : AButton {
	
	public bool mCheckState { get; set;}

	//signal
	public SIGNAL stateChangedSignal;

	public void InitUI( Rect posRect, bool value, string text){
		this.InitUI( posRect, new GUIContent(text));
	}

	public void InitUI( Rect posRect, bool value, Texture icon){
		this.InitUI( posRect, new GUIContent(icon));
	}

	public void InitUI( Rect posRect, bool value, GUIContent content){
		base.InitUI(posRect, content);
		mCheckState = value;
	}
	
	protected override void BeginDrawSelf(){
		bool newState = GUI.Toggle( mRect, mCheckState, mContent);
		if( newState != mCheckState){
			mCheckState = newState;
			OnStatedChanged();
		}
	}

	void OnStatedChanged(){
		EMIT(stateChangedSignal, new Hashtable(){
			{"sender",		this},
			{"state",	    mCheckState},
		});	
	}
}
}
