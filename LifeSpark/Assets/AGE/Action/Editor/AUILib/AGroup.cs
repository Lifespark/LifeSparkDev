using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace AGE{

public class AGroup : AWidget {

	protected GUIContent mContent;

	public void InitUI(Rect posRect){
		/*base.InitUI( posRect);
		mContent = null;
		*/
		this.InitUI( posRect, GUIContent.none );
	}

	public void InitUI(Rect posRect, string text){
		this.InitUI(posRect, new GUIContent(text));
	}

	public void InitUI(Rect posRect, Texture image){
		this.InitUI(posRect, new GUIContent(image));
	}

	public void InitUI(Rect posRect, GUIContent content){
		base.InitUI(posRect);
		mContent = content;
	}

	protected override void BeginDrawSelf(){
		/*if( mContent == null){
			GUI.BeginGroup( mRect );
		}
		else{
		    GUI.BeginGroup( mRect, mContent);
		}
		*/
		
		GUI.BeginGroup( mRect, mContent);
	}

	protected override void EndDrawSelf(){
		GUI.EndGroup();
	}
}
}
