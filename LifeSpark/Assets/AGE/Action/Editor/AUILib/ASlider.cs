using UnityEngine;
using System.Collections;

namespace AGE{

public class ASlider : AWidget {

	//signal
	public SIGNAL valueChangedSignal;

	//todo: interval
	float interval = 2.0f;

	public enum OrientationType{
		Vertical,
		Horizontal
	}

	public OrientationType mOrientation{ get; set;}
	public float mValue {get; set;}
	public float mMinValue { get; set;}
	public float mMaxValue { get; set;}

	public void InitUI( Rect posRect, OrientationType orientation, float value, float minValue, float maxValue){
		base.InitUI(posRect);
		mOrientation = orientation;
		mValue = value;
		mMinValue    = minValue;
		mMaxValue    = maxValue;
	}
	
	protected override void BeginDrawSelf(){
		base.BeginDrawSelf();

		float newValue = 0.0f;
		if( mOrientation == OrientationType.Vertical){
			newValue = GUI.VerticalSlider( mRect, mValue, mMinValue, mMaxValue);
		}
		else
		{
			newValue = GUI.HorizontalSlider( mRect, mValue, mMinValue, mMaxValue);
		}

		if( newValue != mValue ){
			mValue = newValue;
			OnValueChanged();
		}
	}

	protected void OnValueChanged(){
		EMIT( valueChangedSignal, new Hashtable{
			{"sender",    this},
			{"value",     mValue}
		});

	}
}
}
