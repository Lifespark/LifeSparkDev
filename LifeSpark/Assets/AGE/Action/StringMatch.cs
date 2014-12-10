using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AGE{

public class StringMatch{

	public static bool IsMatchString(string[] strArray, string pattern)
	{
		foreach(string s in strArray)
		{
			if (IsMatchString(s, pattern))
				return true;
		}
		
		return false;
	}

	public static bool IsMatchString(string str, string pattern)
	{
		//use regex to implement wildcard character
		pattern = pattern.Replace("*", ".*");
		pattern = pattern.Replace("?", ".");
		if ( System.Text.RegularExpressions.Regex.IsMatch(str, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase) )
			return true;

		return false;
	}
}
}












