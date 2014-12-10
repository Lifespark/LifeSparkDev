using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AGE
{
	public class ActionSet
	{
		public Dictionary<Action, bool> actionSet = new Dictionary<Action, bool>();

		public ActionSet()
		{
			actionSet = new Dictionary<Action, bool>();
		}

		public ActionSet(Dictionary<Action, bool> _actionSet)
		{
			actionSet = new Dictionary<Action, bool>();
			foreach(KeyValuePair<Action, bool> member in _actionSet)
			{
				actionSet.Add(member.Key, member.Value);
			}
		}

		static public ActionSet InvertSet(ActionSet all, ActionSet exclusion)
		{
			ActionSet result = new ActionSet();
			foreach(Action action in all.actionSet.Keys)
			{
				if ( ! exclusion.actionSet.ContainsKey(action))
				{
					result.actionSet.Add(action, true);
				}
			}
			return result;
		}
		
		static public ActionSet AndSet(ActionSet src1, ActionSet src2)
		{
			ActionSet result = new ActionSet();
			foreach(Action action in src1.actionSet.Keys)
			{
				if (src2.actionSet.ContainsKey(action))
				{
					result.actionSet.Add(action, true);
				}
			}
			return result;
		}
		
		static public ActionSet OrSet(ActionSet src1, ActionSet src2)
		{
			ActionSet result = new ActionSet();
			foreach(Action action in src1.actionSet.Keys)
			{
				result.actionSet.Add(action, true);
			}
			foreach(Action action in src2.actionSet.Keys)
			{
				if (! result.actionSet.ContainsKey(action))
				{
					result.actionSet.Add(action, true);
				}
			}
			return result;
		}
	}
}