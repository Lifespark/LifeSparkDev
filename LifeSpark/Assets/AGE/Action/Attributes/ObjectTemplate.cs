using System;
using UnityEngine;

namespace AGE{
	

public sealed class ObjectTemplate : System.Attribute
{
	public ObjectTemplate (bool _dynamicObject)
	{
		dynamicObject = _dynamicObject;
	}
	
	public ObjectTemplate (params System.Type[] _dependencies)
	{
		dependencies = _dependencies;
	}
	
	public bool CheckForDependencies(GameObject _gameObject)
	{
		foreach (System.Type type in dependencies)
		{
			if (!_gameObject.GetComponent(type))
				return false;
		}
		return true;
	}
	
	public System.Type[] dependencies;
	public bool dynamicObject = false;
}

}
