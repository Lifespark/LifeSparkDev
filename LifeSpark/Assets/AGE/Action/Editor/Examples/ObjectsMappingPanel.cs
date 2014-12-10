

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace AGE{

public class ObjectsMappingPanel : EditorPanelBase
{
	public List<GameObject> 		mObjects = new List<GameObject>();
	bool 							mExpand = false;



	public void Update()
	{
	}

	public void Draw()
	{
		bool addItem = false;
		bool delItem = false;
		int indexChange = -1;

		GUI.BeginGroup( mRect );
		
		GUILayout.Box( "ObjectsMappingPanel" );

		EditorGUILayout.BeginHorizontal();
		bool cv = GUILayout.Button( GetExpandTxt(mExpand), GetLayoutOptions(20,20) );
		if( cv )
			mExpand = !mExpand;
		GUILayout.Label( "Objects" );
		if( GUILayout.Button( "+", GetLayoutOptions(20,20) ) )
			addItem = true;
		EditorGUILayout.EndHorizontal();

		if( mExpand )
		{
			GameObject[] tmpobjs = new GameObject[mObjects.Count];

			for( int i = 0; i < mObjects.Count; ++i )
			{
				tmpobjs[i] = mObjects[i];
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space( 20 );
				tmpobjs[i] = EditorGUILayout.ObjectField( "Obj_" + i, tmpobjs[i], typeof(GameObject), true ) as GameObject;
				if( GUILayout.Button( "+", GetLayoutOptions(20,20) ) )
				{
					addItem = true;
					indexChange = i;
				}
				if( GUILayout.Button( "-", GetLayoutOptions(20,20) ) )
				{
					delItem = true;
					indexChange = i;
				}
				EditorGUILayout.EndHorizontal();
			}

			for( int i = 0; i < mObjects.Count; ++i )
			{
				mObjects[i] = tmpobjs[i];
			}
		}

		GUI.EndGroup();

		if( addItem )
		{
			if( indexChange == -1 )
				mObjects.Add(null);
			else
				mObjects.Insert( indexChange, null );
		}
		if( delItem )
		{
			if( indexChange != -1 )
			{
				if( mObjects[indexChange] != null )
					mObjects[indexChange] = null;
				else
					mObjects.RemoveAt( indexChange );
			}
		}
	}
}
}
