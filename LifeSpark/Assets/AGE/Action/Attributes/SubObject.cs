using System;
using UnityEngine;

namespace AGE{
	

public sealed class SubObject : System.Attribute
{
	public static GameObject FindSubObject(GameObject _targetObject, string _subObjectNamePath)
	{
		if (_subObjectNamePath.IndexOf('/') >= 0)
		{
			//treat as path
			Transform resultTransform = _targetObject.transform.Find(_subObjectNamePath);
			if (resultTransform)
				return resultTransform.gameObject;
			else
				return null;
		}
		else
		{
			//treat as object name, search recursively
			Transform resultTransform = _targetObject.transform.Find(_subObjectNamePath);
			if (resultTransform == null)
			{
				for (int i=0; i<_targetObject.transform.childCount; i++)
				{
					GameObject result = FindSubObject(_targetObject.transform.GetChild(i).gameObject, _subObjectNamePath);
					if (result != null)
						return result;
				}
				return null;
			}
			else
			{
				return resultTransform.gameObject;
			}
		}
	}
}

}

