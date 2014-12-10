
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AGE{

public class EditorPanelBase : EditorWindow
{
	public Rect mRect = new Rect(0, 0, 300, 300);
	static List<GUILayoutOption> sOpts = new List<GUILayoutOption>();

	public EditorPanelBase()
	{
	}

	public void ResetSize( float w, float h )
	{
		mRect.Set( mRect.x, mRect.y, w, h );
	}

	public static GUILayoutOption[] GetLayoutOptions( float width, float height )
	{
		sOpts.Clear();
		if( width > 0f )
			sOpts.Add( GUILayout.Width( width ) );
		if( height > 0f )
			sOpts.Add( GUILayout.Height( height ) );
		return sOpts.ToArray();
	}

	public string GetExpandTxt( bool b )
	{
		if( b ) return "-";
		else return "+";
	}
}



public class PanelBase : AGroup
{
	public float mRealHeight = 0f;
	
	protected static float sItemHeight = 25f;
	protected static float sLevelSpaceGap = 25f;
	protected static List<GUILayoutOption> sOpts = new List<GUILayoutOption>();

	protected Dictionary<string, bool> mExpandFlags = new Dictionary<string, bool>();	
	protected bool mExecExpandAll = false;
	protected bool mExecClapseAll = false;
	
	public void SetAllExpandFlags( bool v )
	{
		foreach( string k in mExpandFlags.Keys )
			mExpandFlags[k] = v;
	}
	
	public void SetPanelSize( float w, float h )
	{
		mRect.width = w;
		mRect.height = h;
	}

	public static GUILayoutOption[] GetLayoutOptions( float width, float height )
	{
		sOpts.Clear();
		if( width > 0f )
			sOpts.Add( GUILayout.Width( width ) );
		if( height > 0f )
			sOpts.Add( GUILayout.Height( height ) );
		return sOpts.ToArray();
	}

	protected void SetExpand( string prop, bool value )
	{
		if( !mExpandFlags.ContainsKey(prop) )
			mExpandFlags.Add( prop, value );
		else
			mExpandFlags[prop] = value;
	}
	
	protected bool GetExpand( string prop )
	{
		if( !mExpandFlags.ContainsKey(prop) )
			mExpandFlags.Add( prop, true );
		if( mExecExpandAll )
			mExpandFlags[prop] = true;
		if( mExecClapseAll )
			mExpandFlags[prop] = false;
		return mExpandFlags[prop];
	}

	public string GetExpandTxt( bool b )
	{
		if( b ) return "-";
		else return "+";
	}
	
	protected void DrawSpace( int gapCount )
	{
		float spaceSize = gapCount * sLevelSpaceGap;
		GUILayout.Space( spaceSize );
	}
		
	protected override void OnEnable()
	{
		base.OnEnable();
		mExpandFlags.Clear();
	}
	
	protected float GetTagBoxWidth()
	{
		float v = mRect.width * 0.3f;
		if( v < 100 )
			v = 100;
		if( v > 200 )
			v = 200;
		return v;
	}
	
	public static void DrawEmptyLine(int lineCount)
	{
		for( int i = 0; i < lineCount; ++i )
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(" ", (GUILayoutOption[])null);
			GUILayout.EndHorizontal();
		}
	}
}
}

