--header
require("age");

Type =
{
	bool=0, Bool=0, BOOL=0,
	int=1, Int=1, INT=1,
	uint=2, UInt=2, UINT=2,
	float=3, Float=3, FLOAT=3,
	string=4, String=4, STRING=4,
	vector3=5, Vector3=5, VECTOR3=5,
	quaternion=6, Quaternion=6, QUATERNION=6,
	eulerangle=7, Eulerangle=7, EULERANGLE=7, 
	enum=8, Enum=8, ENUM=8,
	templateobj=9, TemplateObj=9, TEMPLATEOBJ=9,
	array=10, Array=10, ARRAY=10,
};

templates = {};

function AddTemplate(_name, _paramFunc, _generateFunc)
	templates[#templates+1] = { _name, _paramFunc, _generateFunc };
end;

function CreateAction(_name, _loop, _length)
	return { name=_name; loop=_loop; length=_length; tracklist={}; };
end;

function CreateTemplateObject( tempObjList, _objectName, _isTemp )
	local newId = #tempObjList;
	tempObjList[newId+1] = { objectName=_objectName; id=newId; isTemp=_isTemp; };
	return newId;
end

function CreateTrack(_action, _name, _eventType, _enable)
	local track = { name=_name; eventType=_eventType; enable=_enable; eventlist={}; conditionlist={}; };
	local index = #_action.tracklist+1;
	_action.tracklist[index] = track;
	return track;
end;

function CreateTickEvent( _track, _timePos )
	local evt = { name=_track.eventType; timePos=_timePos; timeLength=0.06; isDuration=false; datalist={}; }
	local index = #_track.eventlist+1;
	_track.eventlist[index] = evt;
	return evt;
end

function CreateDurationEvent( _track, _timePos, _timeLength )
	local evt = { name=_track.eventType; timePos=_timePos; timeLength=_timeLength; isDuration=true;  datalist={}; }
	local index = #_track.eventlist+1;
	_track.eventlist[index] = evt;
	return evt;
end

function GetTrackIndex( _action, _track )
	for k, v in pairs( _action.tracklist ) do
		if( v == _track ) then
			return k
		end
	end
	return -1;
end

function CreateCondition( _action, _track, _cdtTrack, _value )
	local _cdtTrackIndex = GetTrackIndex(_action, _cdtTrack);
	local cdt = { trackIndex = _cdtTrackIndex; value = _value; }
	local index = #_track.conditionlist+1;
	_track.conditionlist[index] = cdt;
	return cdt;
end

function SetBool( _event, _name, _value )
	local data = { dataType=Type.bool; name=_name; value=_value; }
	local index = #_event.datalist+1
	_event.datalist[index]=data;
end

function SetInt( _event, _name, _value )
	local data = { dataType=Type.int; name=_name; value=_value; }
	local index = #_event.datalist+1
	_event.datalist[index]=data;
end

function SetTemplateObject( _event, _name, _templateObjectList, _id )
	local data = { dataType=Type.templateobj; name=_name; objectName=_templateObjectList[_id+1].objectName; id=_id; isTemp=_templateObjectList[_id+1].isTemp; }
	local index = #_event.datalist+1
	_event.datalist[index]=data;
end

function SetUInt( _event, _name, _value )
	local data = { dataType=Type.uint; name=_name; value=_value; }
	local index = #_event.datalist+1
	_event.datalist[index]=data;
end

function SetFloat( _event, _name, _value )
	local data = { dataType=Type.float; name=_name; value=_value; }
	local index = #_event.datalist+1
	_event.datalist[index]=data;
end

function SetString( _event, _name, _value )
	local data = { dataType=Type.string; name=_name; value=_value; }
	local index = #_event.datalist+1
	_event.datalist[index]=data;
end

function SetVector3( _event, _name, _x, _y, _z )
	if (type(_x) == "table") then
		local data = { dataType=Type.vector3; name=_name; x=_x.x; y=_x.y; z=_x.z; }
		local index = #_event.datalist+1
		_event.datalist[index]=data;
	else
		local data = { dataType=Type.vector3; name=_name; x=_x; y=_y; z=_z; }
		local index = #_event.datalist+1
		_event.datalist[index]=data;
	end;
end

function SetQuaternion( _event, _name, _x, _y, _z, _w )
	if (type(_x) == "table") then
		local data = { dataType=Type.quaternion; name=_name; x=_x.x; y=_x.y; z=_x.z; w=_x.w; }
		local index = #_event.datalist+1
		_event.datalist[index]=data;
	else
		local data = { dataType=Type.quaternion; name=_name; x=_x; y=_y; z=_z; w=_w; }
		local index = #_event.datalist+1
		_event.datalist[index]=data;
	end;
end

function SetEulerAngle( _event, _name, _x, _y, _z )
	if (type(_x) == "table") then
		local data = { dataType=Type.eulerangle; name=_name; x=_x.x; y=_x.y; z=_x.z; }
		local index = #_event.datalist+1
		_event.datalist[index]=data;
	else
		local data = { dataType=Type.eulerangle; name=_name; x=_x; y=_y; z=_z; }
		local index = #_event.datalist+1
		_event.datalist[index]=data;
	end;
end

function SetEnum( _event, _name, _value, _typelist )
	local data = { dataType=Type.enum; name=_name; value=_value; typelist=_typelist; }
	local index = #_event.datalist+1
	_event.datalist[index]=data;
end

function SetArray(_event, _name, _subType, _value)
	local data = { dataType=Type.array; name=_name; subType=_subType; value=_value; }
	local index = #_event.datalist+1;
	_event.datalist[index]=data;
end;

function print_lua_table (lua_table, indent)
	if( lua_table == nil ) then
		print( "table is nil..." )
		return;
	end
	indent = indent or 0
	for k, v in pairs(lua_table) do
		if type(k) == "string" then
			k = string.format("%q", k)
		end
		local szSuffix = ""
		if type(v) == "table" then
			szSuffix = "{"
		end
		local szPrefix = string.rep("    ", indent)
		formatting = szPrefix.."["..k.."]".." = "..szSuffix
		if type(v) == "table" then
			print(formatting)
			print_lua_table(v, indent + 1)
			print(szPrefix.."},")
		else
			local szValue = ""
			if type(v) == "string" then
				szValue = string.format("%q", v)
			else
				szValue = tostring(v)
			end
			print(formatting..szValue..",")
		end
	end
end
