using System;
using UnityEngine;

namespace AGE{

public sealed class EventCategory : System.Attribute
{
	public EventCategory (string _category)
	{
		category = _category;
	}
	public string category;
}

}
