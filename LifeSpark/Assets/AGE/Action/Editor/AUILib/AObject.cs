//==================================================================================================
// File      : AObject.cs
// Brief     : AObject for signal/slot 
// Create    : 2014-01-07
// Author    : figolai <figolai@tencent.com>
// Copyright : (C) 2014 Tencent Engine Technology Center, all rights reserved.
// Website   : http://www.tencent.com
//==================================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace AGE{

public class AObject : EditorWindow {
		
	// connect when OnEnable() and disconnect when OnDestory()	
	public delegate void SIGNAL(Hashtable args);
	
	public virtual void CONNECT(ref SIGNAL signal, SIGNAL slot){
		signal += slot;
	}
	
	public virtual void DISCONNECT(ref SIGNAL signal, SIGNAL slot){
		signal -= slot;
	}
	
	public void EMIT(SIGNAL signal, Hashtable args){
		if(signal != null)
			signal(args);
	}

}
}
