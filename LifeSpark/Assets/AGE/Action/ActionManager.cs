using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace AGE
{

	//WARNING!!!
	//set execute order to before default time!!!
	//WARNING!!!
	public class ActionManager : MonoBehaviour
	{
		public bool debugMode = false;

		public static void DestroyGameObject(Object obj)
		{
			if (obj == null) return;

			if( ActionManager.Instance != null && ActionManager.Instance.ResLoader != null )
			{
				ActionManager.Instance.ResLoader.DestroyObject( obj );
				return;
			}
#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isPlaying || UnityEditor.EditorApplication.isPaused)
				Destroy(obj);
			else
				DestroyImmediate(obj);
#else
			Destroy(obj);
#endif
		}

		public static Object InstantiateObject(Object prefab)
		{
			if( ActionManager.Instance != null && ActionManager.Instance.ResLoader != null )
				return ActionManager.Instance.ResLoader.Instantiate(prefab);
			return Object.Instantiate(prefab);
		}

		public static Object InstantiateObject(Object prefab, Vector3 pos, Quaternion rot)
		{
			if( ActionManager.Instance != null && ActionManager.Instance.ResLoader != null )
				return ActionManager.Instance.ResLoader.Instantiate(prefab, pos, rot);
			return Object.Instantiate(prefab, pos, rot);
		}

		void Initialize()
		{
			//clear all sub objects
			foreach (Transform child in transform) DestroyGameObject(child.gameObject);

			//create asset loader
			if( resLoader == null )
			{
				System.Type assetLoaderType = System.Type.GetType(assetLoaderClass);
				if (assetLoaderType != null)
					resLoader = System.Activator.CreateInstance(assetLoaderType) as AssetLoader;
				if (resLoader == null)
				{
					if( useOptimiseLoader )
						resLoader = new AssetLoaderOptimise();
					else
						resLoader = new AssetLoader();
				}
			}

			resLoader.preloading = true; //entering preloading state

			{
				Dictionary<string, bool> actionsToLoad = new Dictionary<string, bool>();
				Dictionary<string, bool> loadedActions = new Dictionary<string, bool>();
				foreach (string actionName in preloadActions)
					actionsToLoad.Add(actionName, true);
				if (preloadActionHelpers)
				{
					ActionHelper[] helpers = GameObject.FindObjectsOfType<ActionHelper>();
					foreach (ActionHelper helper in helpers)
					{
						foreach (ActionHelperStorage helperStorage in helper.actionHelpers)
						{
							if (helperStorage.actionName.Length>0 && !actionsToLoad.ContainsKey(helperStorage.actionName))
								actionsToLoad.Add(helperStorage.actionName, true);
						}
					}
				}
				bool preloadFinished = false;
				while (!preloadFinished)
				{
					preloadFinished = true;
					List<string> newActionsToLoad = new List<string>();
					foreach (string actionName in actionsToLoad.Keys)
					{
						if (!loadedActions.ContainsKey(actionName))
						{
							Action actionResource = LoadActionResource(actionName);
							loadedActions.Add(actionName, true);

							Dictionary<string, bool> associatedActions = actionResource.GetAssociatedActions();
							foreach (string associatedActionName in associatedActions.Keys)
							{
								if (associatedActionName.Length == 0) continue;
								if (!actionsToLoad.ContainsKey(associatedActionName))
								{
									preloadFinished = false;
									newActionsToLoad.Add(associatedActionName);
								}
							}
						}
					}
					foreach (string newActionName in newActionsToLoad)
					{
						if (!actionsToLoad.ContainsKey(newActionName))
							actionsToLoad.Add(newActionName, true);
					}
				}
			}

			//pre-load associated resources
			if (preloadResources)
			{
				Dictionary<string, bool> loadedResources = new Dictionary<string, bool>();
				Action[] loadedActions = GetComponentsInChildren<Action>();
				foreach (Action action in loadedActions)
				{
					Dictionary<string, bool> associatedResources = action.GetAssociatedResources();
					foreach (string resName in associatedResources.Keys)
					{
						if (!loadedResources.ContainsKey(resName))
						{
							resLoader.Load(resName);
							loadedResources.Add(resName, true);
						}
					}
				}
			}

			resLoader.preloading = false; //preloading state finished
		}

		public void ForceStart()
		{
			if (instance != null) return;
			instance = this;

			Initialize();
		}

		public void ForceStop()
		{
			if (instance == null) return;

			instance = null;

			actionSet.Clear();
			actionResourceSet.Clear();
			objectReferenceSet.Clear();

	#if UNITY_EDITOR
			actionWatcherMap.Clear();
			watcherActionMap.Clear();
			reloadActions.Clear();
	#endif
			
			//clear all sub objects
			foreach (Transform child in transform) DestroyGameObject(child.gameObject);
			
			if( resLoader != null )
			{
				resLoader.Destroy();
				resLoader = null;
			}
		}

		void Start()
		{
			if (instance == null)
			{
				instance = this;
			}
			else if (instance != this)
			{
				instance.enabled = false;
				instance = this;
			}

			Initialize();
		}

		public void ForceReloadAction(string _actionName)
		{
#if UNITY_EDITOR
			UnityEditor.AssetDatabase.Refresh();

			//delete action resource to wait for reload
			Action actionResource = null;
			if (actionResourceSet.TryGetValue(_actionName, out actionResource) && actionResource != null)
			{
				actionResourceSet.Remove(_actionName);
				DestroyGameObject(actionResource.gameObject);
			}
			
			//kill and replay all related actions
			List<Action> relatedActions = new List<Action>();
			foreach (Action action in actionSet.Keys)
			{
				if (action.gameObject.name == _actionName)
					relatedActions.Add(action);
			}
			foreach (Action action in relatedActions)
				action.Stop();
			AgeLogger.Log("Action " + _actionName + " has been reloaded.");
#endif
		}

		void Update()
		{
	#if UNITY_EDITOR
			if (debugMode && UnityEditor.EditorApplication.isPlaying)
			{
				foreach (string actionName in reloadActions.Keys)
				{
					UnityEditor.AssetDatabase.Refresh();
					//delete action resource to wait for reload
					Action actionResource = null;
					if (actionResourceSet.TryGetValue(actionName, out actionResource) && actionResource != null)
					{
						actionResourceSet.Remove(actionName);
						DestroyGameObject(actionResource.gameObject);
					}
					
					//kill and replay all related actions
					List<Action> relatedActions = new List<Action>();
					foreach (Action action in actionSet.Keys)
					{
						if (action.gameObject.name == actionName)
							relatedActions.Add(action);
					}
					foreach (Action action in relatedActions)
					{
						GameObject[] gameObjects = new GameObject[action.gameObjects.Count];
						for (int i=0; i<action.gameObjects.Count; i++)
							gameObjects[i] = action.gameObjects[i];
						bool autoPlay = action.enabled;
						action.Stop();
						PlayAction(actionName, autoPlay, false, gameObjects);
					}
					AgeLogger.Log("Action " + actionName + " has been modified and reloaded.");
				}
				reloadActions.Clear();
			}
	#endif
		}
		
		//use this method to create an ACTION OBJECT, that is:
		//a GameObject which contains an Action Component
		public Action PlayAction(string _actionName, bool _autoPlay, bool _stopConflictAction, params GameObject[] _gameObjects)
		{
			Action actionResource = LoadActionResource(_actionName);
			if (actionResource == null)
			{
				AgeLogger.LogError("Playing \"" + _actionName + "\" failed. Asset not found!");
				return null;
			}

			GameObject conflictObject = null;
			//foreach(GameObject target in _gameObjects)
			for (int i=0; i<Mathf.Min(_gameObjects.Length, actionResource.refGameObjectsCount); i++)
			{
				GameObject target = _gameObjects[i];
				if(target == null)
					continue;
				if (objectReferenceSet.ContainsKey(target))
				{
					conflictObject = target;
					break;
				}
			}
			if (conflictObject && _stopConflictAction)
			{
				List<Action> conflictActionsToStop = new List<Action>();
				foreach (Action conflictAction in objectReferenceSet[conflictObject])
				{
					if (!conflictAction.unstoppable)
						conflictActionsToStop.Add(conflictAction);
				}

				foreach (Action action in conflictActionsToStop)
				{
					action.Stop();
				} 

			}

			GameObject actionObject = new GameObject(actionResource.gameObject.name);
			try
			{
				actionObject.tag = actionResource.gameObject.tag;
			}
			catch (UnityException e)
			{
				actionObject.tag = "Untagged";
				AgeLogger.LogException(e);
			}

			Action result = actionObject.AddComponent<Action>();
			result.enabled = _autoPlay;
			//record old gameobjects count
			//result.refGameObjectsCount = _gameObjects.Length;

			result.LoadAction(actionResource, _gameObjects);
			
			actionSet.Add(result, true);
			//foreach(GameObject target in _gameObjects)
			for (int i=0; i<Mathf.Min(_gameObjects.Length, actionResource.refGameObjectsCount); i++)
			{
				GameObject target = _gameObjects[i];
				if(target == null)
					continue;
				//objectReferenceSet.Add(target, result);
				List<Action> conflictActions = null;
				if (objectReferenceSet.TryGetValue(target, out conflictActions))
				{
					conflictActions.Add(result);
				}
				else
				{
					conflictActions = new List<Action>();
					conflictActions.Add(result);
					objectReferenceSet.Add(target, conflictActions);
				}
			}

			return result;
		}
		
		//sub action is forced to be non-loop, and length must be specified
		//won't worry about object conflict
		//not managed by ActionManager
		public Action PlaySubAction(Action _parentAction, string _actionName, float _length, params GameObject[] _gameObjects)
		{
			Action actionResource = LoadActionResource(_actionName);
			if (actionResource == null)
			{
				AgeLogger.LogError("Playing \"" + _actionName + "\" failed. Asset not found!");
				return null;
			}
			
			GameObject actionObject = new GameObject(actionResource.gameObject.name);
			Action result = actionObject.AddComponent<Action>();
			try
			{
				actionObject.tag = actionResource.gameObject.tag;
			}
			catch (UnityException e)
			{
				actionObject.tag = "Untagged";
				AgeLogger.LogException(e);
			}

			result.LoadAction(actionResource, _gameObjects);
			result.loop = false;
			result.length = _length;
			result.parentAction = _parentAction;
			
			return result;
		}
		
		public void StopAction(Action _action)
		{
			_action.Stop();
		}

		public void DestroyObject(GameObject _gameObject)
		{
			List<Action> actions = null;
			if (objectReferenceSet.TryGetValue(_gameObject, out actions))
			{
				foreach (Action action in actions)
					action.ClearGameObject(_gameObject);
			}
			DestroyGameObject(_gameObject);
		}

		//should only be called in Action's OnDestroy() to remove itself from actionSet
		public void RemoveAction(Action _action)
		{
			if (_action == null) return;

			foreach (GameObject target in _action.gameObjects)
			{
				if (target == null)
					continue;

				if (objectReferenceSet.ContainsKey(target))
				{
					List<Action> conflictActions = objectReferenceSet[target];
					conflictActions.Remove(_action);
					if (conflictActions.Count == 0)
						objectReferenceSet.Remove(target);
				}
				else if( _action.gameObjects.IndexOf(target) >= _action.refGameObjectsCount)
				{
					DestroyGameObject(target); //release action-created objects
				}
			}
			
			instance.actionSet.Remove(_action);
		}
		
		public bool IsActionValid(Action _action)
		{
			if (_action == null) return false;
			return actionSet.ContainsKey(_action);
		}


		public Dictionary<string, int> LoadTemplateObjectList(Action action)
		{
			string actionName = action.actionName;
			//Dictionary<string, int>  = new Dictionary<string, int>();
			
			//TextAsset textAsset = Resources.Load(actionName) as TextAsset;
			TextAsset textAsset = ActionManager.Instance.resLoader.Load(actionName) as TextAsset;
			if (textAsset == null)
				return new Dictionary<string, int>();
			
			//read from xml
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(textAsset.text);
			
			XmlNode projectNode = doc.SelectSingleNode("Project");
			XmlNode templateObjectListNode = projectNode.SelectSingleNode("TemplateObjectList");
			
			//load TemplateObjectList
			if (templateObjectListNode != null)
			{
				action.templateObjectIds.Clear();
				foreach (XmlNode templateObjectNode in templateObjectListNode.ChildNodes)
				{
					string str = templateObjectNode.Attributes["objectName"].Value;
					string idStr = templateObjectNode.Attributes["id"].Value;
					int id = int.Parse(idStr);
					string isTempStr = templateObjectNode.Attributes["isTemp"].Value;
					if (isTempStr == "false")
					{
						action.templateObjectIds.Add(str, id);
					}
				}
			}
			
			return action.templateObjectIds;
		}

		
		public Action LoadActionResource(string _actionName)
		{
			Action result = null;
			if (actionResourceSet.TryGetValue(_actionName, out result))
			{
				if( result != null )
					return result;
				else
					actionResourceSet.Remove(_actionName);
			}
			
			//TextAsset textAsset = Resources.Load(_actionName) as TextAsset;
			TextAsset textAsset = ActionManager.Instance.resLoader.Load(_actionName) as TextAsset;
			if (textAsset == null)
				return null;
			
			GameObject resourceObject = new GameObject(_actionName);
			resourceObject.transform.parent = transform;
			result = resourceObject.AddComponent<Action>();
			result.enabled = false;
			result.actionName = _actionName;

			actionResourceSet.Add(_actionName, result);
			
			//read from xml
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(textAsset.text);
			
			XmlNode projectNode = doc.SelectSingleNode("Project");
			XmlNode templateObjectListNode = projectNode.SelectSingleNode("TemplateObjectList");
			XmlNode actionNode = projectNode.SelectSingleNode("Action");
			XmlNode refParamNode = projectNode.SelectSingleNode("RefParamList");
			result.length = float.Parse(actionNode.Attributes["length"].Value);
			result.loop = bool.Parse(actionNode.Attributes["loop"].Value);
	//		result.actionName = actionNode.Attributes["actionName"].Value;
			try
			{
				if (actionNode.Attributes["tag"] != null && actionNode.Attributes["tag"].Value.Length>0)
					resourceObject.tag = actionNode.Attributes["tag"].Value;
			}
			catch (UnityException e)
			{
				resourceObject.tag = "Untagged";
				AgeLogger.LogException(e);
			}

			ActionCommonData srcData = new ActionCommonData();

			//load TemplateObjectList
			result.refGameObjectsCount = 0;
			if (templateObjectListNode != null)
			{
				foreach (XmlNode templateObjectNode in templateObjectListNode.ChildNodes)
				{
					string str = templateObjectNode.Attributes["objectName"].Value;
					string idStr = templateObjectNode.Attributes["id"].Value;
					bool isTemp = bool.Parse(templateObjectNode.Attributes["isTemp"].Value);
					if (!isTemp)
						result.refGameObjectsCount++;
					int id = int.Parse(idStr);
					result.AddTemplateObject(str, id);

					TemplateObject obj = new TemplateObject();
					obj.name = templateObjectNode.Attributes["objectName"].Value as string;
					obj.id   = int.Parse(templateObjectNode.Attributes["id"].Value);
					obj.isTemp = bool.Parse(templateObjectNode.Attributes["isTemp"].Value);
					srcData.templateObjects.Add(obj);
				}
			}

			//load refparam objects
			if( refParamNode != null )
			{
				foreach (XmlNode paramNode in refParamNode.ChildNodes)
				{
					LoadRefParamNode(result, paramNode);
					
					string name = paramNode.Attributes["name"].Value;
					if( name.StartsWith("_") )
						srcData.predefRefParamNames.Add(name);
				}
			}

			if (!actionCommonDataSet.ContainsKey(_actionName))
				actionCommonDataSet.Add( _actionName, srcData );

			//load action
			foreach (XmlNode trackNode in actionNode.ChildNodes)
			{
				string typename = trackNode.Attributes["eventType"].Value;
				if(!typename.Contains(".") && typename.Length > 0)
				{  // default name space
					typename = "AGE." + typename;
				}
				System.Type eventType = System.Type.GetType(typename);
				if (eventType != null)
				{
					string refParamName = "";
					bool useRefParam = false;
					if( trackNode.Attributes["refParamName"] != null )
						refParamName = trackNode.Attributes["refParamName"].Value;
					if( trackNode.Attributes["useRefParam"] != null )
						useRefParam = bool.Parse(trackNode.Attributes["useRefParam"].Value);
					bool enabled = bool.Parse(trackNode.Attributes["enabled"].Value);
					if( useRefParam )
					{
						object v = result.refParams.GetRefParamValue(refParamName);
						if( v != null )
							enabled = (bool)v;
					}

					Track newTrack = result.AddTrack(eventType);
					newTrack.enabled = enabled;

					newTrack.trackName = trackNode.Attributes["trackName"].Value;
					if( trackNode.Attributes["execOnActionCompleted"] != null )
						newTrack.execOnActionCompleted = bool.Parse(trackNode.Attributes["execOnActionCompleted"].Value);
					if( trackNode.Attributes["execOnForceStopped"] != null )
						newTrack.execOnForceStopped = bool.Parse(trackNode.Attributes["execOnForceStopped"].Value);
					if( trackNode.Attributes["stopAfterLastEvent"] != null )
						newTrack.stopAfterLastEvent = bool.Parse(trackNode.Attributes["stopAfterLastEvent"].Value);

					if( useRefParam )
					{
						System.Reflection.FieldInfo fieldInfo = eventType.GetField(trackNode.Attributes["enabled"].Value);
						result.refParams.AddRefData( refParamName, fieldInfo, newTrack );
					}

					if( trackNode.Attributes["r"] != null )
						newTrack.color.r = float.Parse(trackNode.Attributes["r"].Value);
					if( trackNode.Attributes["g"] != null )
						newTrack.color.g = float.Parse(trackNode.Attributes["g"].Value);
					if( trackNode.Attributes["b"] != null )
						newTrack.color.b = float.Parse(trackNode.Attributes["b"].Value);

					List<XmlNode> commonFieldNodes = new List<XmlNode>();
					
					//scan for common field values
					foreach (XmlNode commonFieldNode in trackNode.ChildNodes)
					{
						if (commonFieldNode.Name != "Event" && commonFieldNode.Name != "Condition")
							commonFieldNodes.Add(commonFieldNode);
					}
					
					//scan for events
					foreach (XmlNode eventNode in trackNode.ChildNodes)
					{
						if (eventNode.Name == "Condition")
						{
							XmlNode conditionNode = eventNode;
							int conditionId = int.Parse(conditionNode.Attributes["id"].Value);
							bool status = bool.Parse(conditionNode.Attributes["status"].Value);
							newTrack.waitForConditions.Add(conditionId, status);
							continue;
						}

						if (eventNode.Name != "Event")
							continue;

						float time = float.Parse(eventNode.Attributes["time"].Value);
						float length = 0.0f;
						if (newTrack.IsDurationEvent)
							length = float.Parse(eventNode.Attributes["length"].Value);
						BaseEvent newEvent = newTrack.AddEvent(time, length);
						
						//set common field values first to allow individual value overrides
						foreach (XmlNode commonFieldNode in commonFieldNodes)
							SetEventField(result, newEvent, commonFieldNode);
						
						foreach (XmlNode fieldNode in eventNode.ChildNodes)
							SetEventField(result, newEvent, fieldNode);
					}				
				}
				else
				{
					AgeLogger.LogError("Invalid event type \"" + trackNode.Attributes["eventType"].Value + "\"!");
				}
			}

	#if UNITY_EDITOR
			if (debugMode && !actionWatcherMap.ContainsKey(_actionName))
			{
				string actionPath = UnityEditor.AssetDatabase.GetAssetPath(textAsset);

				FileSystemWatcher newWatcher = new FileSystemWatcher();
				//string fullPath = Path.GetFullPath(_actionName);
				newWatcher.Path = Path.GetDirectoryName(actionPath);
				newWatcher.Filter = Path.GetFileName(actionPath);
				newWatcher.NotifyFilter = NotifyFilters.LastWrite;
				newWatcher.Changed += new FileSystemEventHandler(this.OnActionModified);
				newWatcher.EnableRaisingEvents = true;
				actionWatcherMap.Add(_actionName, newWatcher);
				watcherActionMap.Add(newWatcher, _actionName);
			}
	#endif

			return result;
		}

		void LoadRefParamNode(Action result, XmlNode paramNode)
		{
			string fieldTypeName = paramNode.Name.ToLower();
			string objname = paramNode.Attributes["name"].Value;
			if (fieldTypeName == "float")
			{
				float value = float.Parse(paramNode.Attributes["value"].Value);
				result.refParams.AddRefParam(objname, value);
			}
			else if (fieldTypeName == "int")
			{
				int value = int.Parse(paramNode.Attributes["value"].Value);
				result.refParams.AddRefParam(objname, value);
			}
			else if (fieldTypeName == "templateobject")
			{
				int value = int.Parse(paramNode.Attributes["id"].Value);
				result.refParams.AddRefParam(objname, value);
			}
			else if (fieldTypeName == "uint")
			{
				uint value = uint.Parse(paramNode.Attributes["value"].Value);
				result.refParams.AddRefParam(objname, value);
			}
			else if (fieldTypeName == "bool")
			{
				bool value = bool.Parse(paramNode.Attributes["value"].Value);
				result.refParams.AddRefParam(objname, value);
			}
			else if (fieldTypeName == "string")
			{
				string value = paramNode.Attributes["value"].Value;
				result.refParams.AddRefParam(objname, value);
			}
			else if (fieldTypeName == "vector3")
			{
				float x = float.Parse(paramNode.Attributes["x"].Value);
				float y = float.Parse(paramNode.Attributes["y"].Value);
				float z = float.Parse(paramNode.Attributes["z"].Value);
				Vector3 value = new Vector3(x, y, z);
				result.refParams.AddRefParam(objname, value);
			}
			else if (fieldTypeName == "quaternion")
			{
				float x = float.Parse(paramNode.Attributes["x"].Value);
				float y = float.Parse(paramNode.Attributes["y"].Value);
				float z = float.Parse(paramNode.Attributes["z"].Value);
				float w = float.Parse(paramNode.Attributes["w"].Value);
				Quaternion value = new Quaternion(x, y, z, w);
				result.refParams.AddRefParam(objname, value);
			}
			else if (fieldTypeName == "eulerangle")
			{
				float x = float.Parse(paramNode.Attributes["x"].Value);
				float y = float.Parse(paramNode.Attributes["y"].Value);
				float z = float.Parse(paramNode.Attributes["z"].Value);
				Quaternion value = Quaternion.Euler(x, y, z);
				result.refParams.AddRefParam(objname, value);
			}
			else if (fieldTypeName == "enum")
			{
				string v = paramNode.Attributes["value"].Value;
				int value = int.Parse(v);
				result.refParams.AddRefParam(objname, value);
			}
		}
		
	#if UNITY_EDITOR
		void OnActionModified(object _source, FileSystemEventArgs _e)
		{
			FileSystemWatcher watcher = _source as FileSystemWatcher;
			string actionName = watcherActionMap[watcher];
			reloadActions.Add(actionName, true);
		}
	#endif

		void SetEventField(Action action, BaseEvent _trackEvent, XmlNode _fieldNode)
		{
			string fieldTypeName = _fieldNode.Name.ToLower();
			System.Type eventType = _trackEvent.GetType();
			
			System.Reflection.FieldInfo fieldInfo = eventType.GetField(_fieldNode.Attributes["name"].Value);
			if (fieldInfo == null)
			{
				AgeLogger.LogError("Field \"" + eventType.Name + "." + _fieldNode.Attributes["name"].Value + "\" doesn't exist!");
				return;
			}

			bool useRefParam = false;
			string refParamName = "";
			if( _fieldNode.Attributes["useRefParam"] != null )
				useRefParam = bool.Parse(_fieldNode.Attributes["useRefParam"].Value);
			if( useRefParam && _fieldNode.Attributes["refParamName"] != null )
				refParamName = _fieldNode.Attributes["refParamName"].Value;
			
			if (fieldTypeName == "float")
			{
				float value = float.Parse(_fieldNode.Attributes["value"].Value);
				if (fieldInfo.FieldType.Name == "CurveContainer")    //hack. Convert Float to CurveContainer for ChangeSpeed Event
				{
					AGE.CurveContainer curve = new CurveContainer(value); 
					fieldInfo.SetValue(_trackEvent, curve);
				}
				else
				{
					fieldInfo.SetValue(_trackEvent, value);
				}
				if( useRefParam )
				{
					action.refParams.AddRefData( refParamName, fieldInfo, _trackEvent );
					object v = action.refParams.GetRefParamValue(refParamName);
					if( v != null )
						fieldInfo.SetValue(_trackEvent, v);
				}
			}
			else if (fieldTypeName == "int")
			{
				int value = int.Parse(_fieldNode.Attributes["value"].Value);
				fieldInfo.SetValue(_trackEvent, value);
				if( useRefParam )
				{
					action.refParams.AddRefData( refParamName, fieldInfo, _trackEvent );
					object v = action.refParams.GetRefParamValue(refParamName);
					if( v != null )
						fieldInfo.SetValue(_trackEvent, v);
				}
			}
			else if (fieldTypeName == "templateobject" || fieldTypeName == "trackobject")
			{
				int value = int.Parse(_fieldNode.Attributes["id"].Value);
				fieldInfo.SetValue(_trackEvent, value);
				if( useRefParam )
				{
					action.refParams.AddRefData( refParamName, fieldInfo, _trackEvent );
					object v = action.refParams.GetRefParamValue(refParamName);
					if( v != null )
						fieldInfo.SetValue(_trackEvent, v);
				}
			}
			else if (fieldTypeName == "uint")
			{
				uint value = uint.Parse(_fieldNode.Attributes["value"].Value);
				fieldInfo.SetValue(_trackEvent, value);
				if( useRefParam )
				{
					action.refParams.AddRefData( refParamName, fieldInfo, _trackEvent );
					object v = action.refParams.GetRefParamValue(refParamName);
					if( v != null )
						fieldInfo.SetValue(_trackEvent, v);
				}
			}
			else if (fieldTypeName == "bool")
			{
				bool value = bool.Parse(_fieldNode.Attributes["value"].Value);
				fieldInfo.SetValue(_trackEvent, value);
				if( useRefParam )
				{
					action.refParams.AddRefData( refParamName, fieldInfo, _trackEvent );
					object v = action.refParams.GetRefParamValue(refParamName);
					if( v != null )
						fieldInfo.SetValue(_trackEvent, v);
				}
			}
			else if (fieldTypeName == "string")
			{
				string value = _fieldNode.Attributes["value"].Value;
				fieldInfo.SetValue(_trackEvent, value);
				if( useRefParam )
				{
					action.refParams.AddRefData( refParamName, fieldInfo, _trackEvent );
					object v = action.refParams.GetRefParamValue(refParamName);
					if( v != null )
						fieldInfo.SetValue(_trackEvent, v);
				}
			}
			else if (fieldTypeName == "vector3")
			{
				float x = float.Parse(_fieldNode.Attributes["x"].Value);
				float y = float.Parse(_fieldNode.Attributes["y"].Value);
				float z = float.Parse(_fieldNode.Attributes["z"].Value);
				Vector3 value = new Vector3(x, y, z);
				fieldInfo.SetValue(_trackEvent, value);
				if( useRefParam )
				{
					action.refParams.AddRefData( refParamName, fieldInfo, _trackEvent );
					object v = action.refParams.GetRefParamValue(refParamName);
					if( v != null )
						fieldInfo.SetValue(_trackEvent, v);
				}
			}
			else if (fieldTypeName == "quaternion")
			{
				float x = float.Parse(_fieldNode.Attributes["x"].Value);
				float y = float.Parse(_fieldNode.Attributes["y"].Value);
				float z = float.Parse(_fieldNode.Attributes["z"].Value);
				float w = float.Parse(_fieldNode.Attributes["w"].Value);
				Quaternion value = new Quaternion(x, y, z, w);
				fieldInfo.SetValue(_trackEvent, value);
				if( useRefParam )
				{
					action.refParams.AddRefData( refParamName, fieldInfo, _trackEvent );
					object v = action.refParams.GetRefParamValue(refParamName);
					if( v != null )
						fieldInfo.SetValue(_trackEvent, v);
				}
			}
			else if (fieldTypeName == "eulerangle")
			{
				float x = float.Parse(_fieldNode.Attributes["x"].Value);
				float y = float.Parse(_fieldNode.Attributes["y"].Value);
				float z = float.Parse(_fieldNode.Attributes["z"].Value);
				Quaternion value = Quaternion.Euler(x, y, z);
				fieldInfo.SetValue(_trackEvent, value);
				if( useRefParam )
				{
					action.refParams.AddRefData( refParamName, fieldInfo, _trackEvent );
					object v = action.refParams.GetRefParamValue(refParamName);
					if( v != null )
						fieldInfo.SetValue(_trackEvent, v);
				}
			}
			else if (fieldTypeName == "curvecontainer")
			{
				CurveContainer curveContainer = new CurveContainer();
				foreach(XmlNode curveNode in _fieldNode.ChildNodes)
				{
					string curveName = curveNode.Attributes["curveName"].Value;
					CurveContainer.Curve curve = new CurveContainer.Curve(curveName);
					foreach(XmlNode curvePointNode in curveNode)
					{
						float time = float.Parse(curvePointNode.Attributes["time"].Value);
						float value = float.Parse(curvePointNode.Attributes["value"].Value);
						float slope = float.Parse(curvePointNode.Attributes["slope"].Value);
						curve.AddPoint(time, value, slope, false);
					}
					curveContainer.AddCurve(curve);
				}
				fieldInfo.SetValue(_trackEvent, curveContainer);
			}
			else if (fieldTypeName == "enum")
			{
				fieldInfo.SetValue(_trackEvent, System.Enum.Parse(fieldInfo.FieldType, _fieldNode.Attributes["value"].Value));
				if( useRefParam )
				{
					action.refParams.AddRefData( refParamName, fieldInfo, _trackEvent );
					object v = action.refParams.GetRefParamValue(refParamName);
					if( v != null )
						fieldInfo.SetValue(_trackEvent, v);
				}
			}
			else if (fieldTypeName == "array")
			{
				string subTypeName = _fieldNode.Attributes["type"].Value.ToLower();
				if (subTypeName == "int")
				{
					int[] array = new int[_fieldNode.ChildNodes.Count];
					int i = 0;
					foreach (XmlNode subFieldNode in _fieldNode.ChildNodes)
					{
						string valueStr = subFieldNode.Attributes["value"].Value;
						int value = int.Parse(valueStr);
						array[i++] = value;
					}
					fieldInfo.SetValue(_trackEvent, array);
				}
				if (subTypeName == "templateobject" || subTypeName == "trackobject")
				{
					int[] array = new int[_fieldNode.ChildNodes.Count];
					int i = 0;
					foreach (XmlNode subFieldNode in _fieldNode.ChildNodes)
					{
						string valueStr = subFieldNode.Attributes["id"].Value;
						int value = int.Parse(valueStr);
						array[i++] = value;
					}
					fieldInfo.SetValue(_trackEvent, array);
				}
				else if (subTypeName == "uint")
				{
					uint[] array = new uint[_fieldNode.ChildNodes.Count];
					int i = 0;
					foreach (XmlNode subFieldNode in _fieldNode.ChildNodes)
					{
						string valueStr = subFieldNode.Attributes["value"].Value;
						uint value = uint.Parse(valueStr);
						array[i++] = value;
					}
					fieldInfo.SetValue(_trackEvent, array);
				}
				else if (subTypeName == "bool")
				{
					bool[] array = new bool[_fieldNode.ChildNodes.Count];
					int i = 0;
					foreach (XmlNode subFieldNode in _fieldNode.ChildNodes)
					{
						string valueStr = subFieldNode.Attributes["value"].Value;
						bool value = bool.Parse(valueStr);
						array[i++] = value;
					}
					fieldInfo.SetValue(_trackEvent, array);
				}
				else if (subTypeName == "float")
				{
					float[] array = new float[_fieldNode.ChildNodes.Count];
					int i = 0;
					foreach (XmlNode subFieldNode in _fieldNode.ChildNodes)
					{
						string valueStr = subFieldNode.Attributes["value"].Value;
						float value = float.Parse(valueStr);
						array[i++] = value;
					}
					fieldInfo.SetValue(_trackEvent, array);
				}
				else if (subTypeName == "string")
				{
					string[] array = new string[_fieldNode.ChildNodes.Count];
					int i = 0;
					foreach (XmlNode subFieldNode in _fieldNode.ChildNodes)
					{
						string value = subFieldNode.Attributes["value"].Value;
						array[i++] = value;
					}
					fieldInfo.SetValue(_trackEvent, array);
				}
			}
			else
			{
				AgeLogger.LogError("Invalid field type \"" + fieldTypeName + "\"!");
			}
		}

		public bool GetActionTemplateObjectsAndPredefRefParams( string actionName, ref List<TemplateObject> objs, ref List<string> refnames )
		{
			if(actionName == null)
				return false;
			
			ActionCommonData srcData;
			if( !actionCommonDataSet.ContainsKey(actionName) )
				LoadActionResource(actionName);
			srcData = actionCommonDataSet[actionName];
//			if( actionCommonDataSet.ContainsKey(actionName) )
//				srcData = actionCommonDataSet[actionName];
//			else
//			{
//				//TextAsset textAsset = Resources.Load(actionName) as TextAsset;
//				TextAsset textAsset = ActionManager.Instance.resLoader.Load(actionName) as TextAsset;
//				if (textAsset == null)
//					return false;
//
//				srcData = new ActionCommonData();
//				
//				//read from xml
//				XmlDocument doc = new XmlDocument();
//				doc.LoadXml(textAsset.text);
//				
//				XmlNode projectNode = doc.SelectSingleNode("Project");
//				XmlNode objectlistNode = projectNode.SelectSingleNode("TemplateObjectList");
//				if( objectlistNode != null )
//				{
//					foreach (XmlNode objectNode in objectlistNode.ChildNodes)
//					{
//						if(objectNode.Name != "TemplateObject")
//							continue;
//						TemplateObject obj = new TemplateObject();
//						obj.name = objectNode.Attributes["objectName"].Value as string;
//						obj.id   = int.Parse(objectNode.Attributes["id"].Value);
//						obj.isTemp = bool.Parse(objectNode.Attributes["isTemp"].Value);
//						srcData.templateObjects.Add(obj);
//					}
//				}
//				
//				XmlNode refparamlistNode = projectNode.SelectSingleNode("RefParamList");
//				if( refparamlistNode != null )
//				{
//					foreach (XmlNode paramNode in refparamlistNode.ChildNodes)
//					{
//						string name = paramNode.Attributes["name"].Value;
//						if( name.StartsWith("_") )
//							srcData.predefRefParamNames.Add(name);
//					}
//				}
//
//				actionCommonDataSet.Add( actionName, srcData );
//			}
			
			if( srcData != null )
			{
				objs.Clear();
				refnames.Clear();
				foreach( TemplateObject obj in srcData.templateObjects )
					objs.Add(obj);
				foreach( string paramname in srcData.predefRefParamNames )
					refnames.Add(paramname);
			}
			
			return true;
		}

		public ActionSet FilterActionsByGameObject(GameObject obj, string nameInAction) 
		{
			ActionSet result = new ActionSet();
			foreach (Action action in actionSet.Keys)
			{
				int idx = 0;
				action.templateObjectIds = LoadTemplateObjectList(action);
				bool bFind = action.TemplateObjectIds.TryGetValue(nameInAction, out idx);
				if (bFind)
				{
					if (action.gameObjects[idx] == obj)
					{
						result.actionSet.Add(action, true);
					}
				}
			}
			return result;
		}
		public ActionSet GetAllValidActionSet()
		{
			return new ActionSet(ActionManager.Instance.actionSet);
		}

		public AssetLoader ResLoader
		{
			get
			{
				return resLoader;
			}
		}

		public void SetResLoader( AssetLoader loader )
		{
			resLoader = loader;
		}

		public static ActionManager Instance
		{
			get
			{
				return instance;
			}
		}
		static ActionManager instance = null;

		public Dictionary<Action, bool> actionSet = new Dictionary<Action, bool>();
		public Dictionary<string, Action> actionResourceSet = new Dictionary<string, Action>();
		public Dictionary<GameObject, List<Action>> objectReferenceSet = new Dictionary<GameObject, List<Action>>();

		public Dictionary<string, ActionCommonData> actionCommonDataSet = new Dictionary<string, ActionCommonData>();

	#if UNITY_EDITOR
		Dictionary<string, FileSystemWatcher> actionWatcherMap = new Dictionary<string, FileSystemWatcher>();
		Dictionary<FileSystemWatcher, string> watcherActionMap = new Dictionary<FileSystemWatcher, string>();
		Dictionary<string, bool> reloadActions = new Dictionary<string, bool>();
	#endif

		AssetLoader resLoader = null;
		public string assetLoaderClass = "";
		public bool useOptimiseLoader = true;

		public string[] preloadActions = new string[0];
		public bool preloadActionHelpers = false;
		public bool preloadResources = false;

	}
}
