
using System.Collections;
using System.Collections.Generic;


namespace AGE{

// this class hold the info of the data object using reference parameter
public class RefData
{
	public System.Reflection.FieldInfo 	fieldInfo;
	public object 						dataObject;
	
	public RefData(System.Reflection.FieldInfo field, object obj)
	{
		fieldInfo = field;
		dataObject = obj;
	}
}

public class RefParamObject
{
	public object						value;
	public bool							dirty;
	
	public RefParamObject( object v )
	{
		value = v;
		dirty = false;
	}
}

public class RefParamOperator
{
	public Dictionary<string, RefParamObject> 	refParamList;
	public Dictionary<string, List<RefData>>	refDataList;
	
	public RefParamOperator()
	{
		refParamList = new Dictionary<string, RefParamObject>();
		refDataList = new Dictionary<string, List<RefData>>();
	}
	
	public RefParamObject AddRefParam( string name, object v )
	{
		if( !refParamList.ContainsKey(name) )
		{
			RefParamObject obj = new RefParamObject(v);
			refParamList.Add( name, obj );
			return obj;
		}
		return refParamList[name];
	}
	
	public RefData AddRefData( string name, System.Reflection.FieldInfo field, object data )
	{
		List<RefData> lst;
		if( refDataList.ContainsKey(name) )
			lst = refDataList[name];
		else
		{
			lst = new List<RefData>();
			refDataList.Add( name, lst );
		}
		RefData obj = new RefData( field, data );
		lst.Add(obj);
		return obj;
	}
	
	public void SetRefParamAndData( string name, object newValue )
	{
		System.Type dstType = newValue.GetType();
       
		bool isValidType = false;
		if( refParamList.ContainsKey(name) )
		{
			object parmVal = refParamList[name].value;
			if( parmVal != null && parmVal.GetType() == dstType )
			{
				isValidType = true;
				refParamList[name].value = newValue;
			}
		}
		if( isValidType && refDataList.ContainsKey(name) )
		{
			List<RefData> lst = refDataList[name];
			foreach( RefData rpd in lst )
			{
				if( rpd != null && rpd.fieldInfo != null )
					rpd.fieldInfo.SetValue( rpd.dataObject, newValue );
			}
		}
	}
	
	public object GetRefParamValue( string name )
	{
		if( refParamList.ContainsKey(name) )
		{
			RefParamObject obj = refParamList[name];
			return obj.value;
		}
		return null;
	}
}


public class ActionCommonData
{
	public List<TemplateObject> templateObjects = new List<TemplateObject>();
	public List<string> 		predefRefParamNames = new List<string>();

	public ActionCommonData()
	{
	}
}

}
