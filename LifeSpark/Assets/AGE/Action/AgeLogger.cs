using UnityEngine;
using System.Collections;

namespace AGE
{
	public class AgeLogger 
	{
		public const string logTitle = "<color=cyan>[AGE]</color> ";
		public const string warnTitle = "<color=yellow>[AGE]</color> ";
		public const string errorTitle = "<color=red>[AGE]</color> ";

		public static void Log(object message)
		{
		#if !DISTRIBUTION_VERSION
			Debug.Log(logTitle + message);
		#endif
		}
		
		public static void Log(object message, Object context)
		{
		#if !DISTRIBUTION_VERSION
			Debug.Log(logTitle + message, context);
		#endif
		}
		
		public static void LogWarning(object message)
		{
		#if !DISTRIBUTION_VERSION
			Debug.LogWarning(warnTitle + message);
		#endif
		}
		
		public static void LogWarning(object message, Object context)
		{
		#if !DISTRIBUTION_VERSION
			Debug.LogWarning(warnTitle + message, context);
		#endif
		}
		
		public static void LogError(object message)
		{
		#if !DISTRIBUTION_VERSION
			Debug.LogError(errorTitle + message);
		#endif
		}
		
		public static void LogError(object message, Object context)
		{
		#if !DISTRIBUTION_VERSION
			Debug.LogError(errorTitle + message, context);
		#endif
		}

		public static void LogException(System.Exception e)
		{
		#if !DISTRIBUTION_VERSION
			Debug.LogException(e);
		#endif
		}

		public static void LogException(System.Exception e, Object context)
		{
		#if !DISTRIBUTION_VERSION
			Debug.LogException(e, context);
		#endif
		}

	}

}
