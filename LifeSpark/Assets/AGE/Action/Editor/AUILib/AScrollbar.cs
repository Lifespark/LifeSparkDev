using UnityEngine;
using System.Collections;

namespace AGE{

public class AScrollbar : ASlider {

	public float mSize { get; set;}

	public void InitUI( Rect posRect, OrientationType orientation, float value, float size, float minValue, float maxValue){
		base.InitUI( posRect, orientation, value, minValue, maxValue);
		mSize = size;
	}

	protected override void BeginDrawSelf(){
		float newValue = 0f;
		if( mOrientation == OrientationType.Vertical){
			newValue = GUI.VerticalScrollbar( mRect, mValue, mSize, mMinValue, mMaxValue);
		}
		else{
			newValue = GUI.HorizontalScrollbar( mRect, mValue, mSize, mMinValue, mMaxValue);
		}

		if( newValue != mValue){
			mValue = newValue;
			OnValueChanged();
		}
	}
}
}

