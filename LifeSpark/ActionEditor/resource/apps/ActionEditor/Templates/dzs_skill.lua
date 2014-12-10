--斗战神技能Action模板

function ThrowSkillParam()
	local params = 
	{
		{
			Type.string,
			"attackAnim",
			"",
			"攻击动作"
		},
		{
			Type.float,
			"launchDelay",
			0,
			"发射延迟"
		},
		{
			Type.vector3,
			"launchPos",
			"",
			"发射位置"
		},
		{
			Type.int,
			"bulletCount",
			1,
			"发射数量"
		},
		{
			Type.float,
			"launchAngle",
			0,
			"发射角度"
		},
		{
			Type.enum,
			"launchType",
			{ 0, { "linear", "parabola", "only create thrown" } },
			"投掷类型"
		},
		{
			Type.string,
			"hitAction",
			"",
			"受击Action"
		},
		{
			Type.string,
			"bulletPrefab",
			"",
			"投掷物Prefab"
		},
		{
			Type.float,
			"hitTime",
			1.0,
			"飞行时间"
		},
		{
			Type.float,
			"speed",
			5.0,
			"飞行速度"
		},
		{
			Type.string,
			"enemyTag",
			"Enemy",
			"敌人Tag"
		},
		{
			Type.string,
			"terrainTag",
			"Terrain",
			"地形Tag"
		},
	};
	return params;
end;

function ThrowSkillGenerator(_params)

	print_lua_table(_params)

	--创建对象模板
	local templateObjectList = {};
	local attacker = CreateTemplateObject(templateObjectList, "Attacker", false);
	local victim = CreateTemplateObject(templateObjectList, "Victim", false);
	local bullets = {};
	
	--创建Action
	local action = CreateAction("RainSkill", false, _params.launchDelay + _params.hitTime + 1.0);
	
	--攻击动画
	local animTrack = CreateTrack(action, "攻击动画", "PlayAnimationTick", true);
	local animEvent = CreateTickEvent(animTrack, 0.0);
	SetTemplateObject(animEvent, "targetId", templateObjectList, attacker);
	SetString(animEvent, "clipName", _params.attackAnim);
	
	--创建投掷物
	for i = 1, _params.bulletCount do
		local bullet = CreateTemplateObject(templateObjectList, "Bullet_"..i, true);
		bullets[#bullets+1] = bullet;
		
		local createBulletTrack = CreateTrack(action, "创建投掷物_"..i, "CreateObjectTick", true);
		local createBulletEvent = CreateTickEvent(createBulletTrack, _params.launchDelay);
		SetTemplateObject(createBulletEvent, "targetId", templateObjectList, bullet);
		SetString(createBulletEvent, "prefabName", _params.bulletPrefab);
		SetTemplateObject(createBulletEvent, "objectSpaceId", templateObjectList, attacker);
		SetVector3(createBulletEvent, "translation", _params.launchPos);
		SetBool(createBulletEvent, "recreateExisting", true);
		
		local angle = -(_params.launchAngle/2) + (_params.launchAngle * (i-1) / (_params.bulletCount-1));
		SetEulerAngle(createBulletEvent, "rotation", 0, angle, 0);
		
		if (_params.launchType == 0) then --直线
			local moveBulletTrack = CreateTrack(action, "移动投掷物_"..i, "ModifyTransform", true);
			local moveBulletEvent1 = CreateTickEvent(moveBulletTrack, _params.launchDelay);
			SetTemplateObject(moveBulletEvent1, "targetId", templateObjectList, bullet);
			SetTemplateObject(moveBulletEvent1, "objectSpaceId", templateObjectList, attacker);
			SetVector3(moveBulletEvent1, "translation", _params.launchPos);

			local moveBulletEvent2 = CreateTickEvent(moveBulletTrack, _params.launchDelay+_params.hitTime);
			SetTemplateObject(moveBulletEvent2, "targetId", templateObjectList, bullet);
			SetTemplateObject(moveBulletEvent2, "objectSpaceId", templateObjectList, attacker);
			SetBool(moveBulletEvent2, "enableRotation", false);
			
			local localPos = Vector3(0, 0, _params.hitTime*_params.speed);
			local localRot = Quaternion(angle, 0, 0);
			local localPos2 = Vector3();
			localRot:Rotate(localPos2, localPos);
			print(localPos2.x..", "..localPos2.y..", "..localPos2.z);
			SetVector3(moveBulletEvent2, "translation", localPos2.x+_params.launchPos.x, localPos2.y+_params.launchPos.y, localPos2.z+_params.launchPos.z);	
			
		elseif (_params.launchType == 1) then --抛物线
		
			local moveBulletTrack = CreateTrack(action, "移动投掷物_"..i, "ModifyTransform", true);
			local moveBulletEvent1 = CreateTickEvent(moveBulletTrack, _params.launchDelay);
			SetTemplateObject(moveBulletEvent1, "targetId", templateObjectList, bullet);
			SetTemplateObject(moveBulletEvent1, "objectSpaceId", templateObjectList, attacker);
			SetVector3(moveBulletEvent1, "translation", _params.launchPos);
			SetBool(moveBulletEvent1, "cubic", true);

			local moveBulletEvent2 = CreateTickEvent(moveBulletTrack, _params.launchDelay+_params.hitTime/3);
			SetTemplateObject(moveBulletEvent2, "targetId", templateObjectList, bullet);
			SetTemplateObject(moveBulletEvent2, "objectSpaceId", templateObjectList, attacker);
			SetBool(moveBulletEvent2, "enableRotation", false);
			local localPos = Vector3(0, 3, _params.hitTime*_params.speed/3);
			local localRot = Quaternion(angle, 0, 0);
			local localPos2 = Vector3();
			localRot:Rotate(localPos2, localPos);
			print(localPos2.x..", "..localPos2.y..", "..localPos2.z);
			SetVector3(moveBulletEvent2, "translation", localPos2.x+_params.launchPos.x, localPos2.y+_params.launchPos.y, localPos2.z+_params.launchPos.z);
			SetBool(moveBulletEvent2, "cubic", true);
			
			local moveBulletEvent3 = CreateTickEvent(moveBulletTrack, _params.launchDelay+_params.hitTime*2/3);
			SetTemplateObject(moveBulletEvent3, "targetId", templateObjectList, bullet);
			SetTemplateObject(moveBulletEvent3, "objectSpaceId", templateObjectList, attacker);
			SetBool(moveBulletEvent3, "enableRotation", false);
			local localPos = Vector3(0, 3, _params.hitTime*_params.speed*2/3);
			local localRot = Quaternion(angle, 0, 0);
			local localPos2 = Vector3();
			localRot:Rotate(localPos2, localPos);
			print(localPos2.x..", "..localPos2.y..", "..localPos2.z);
			SetVector3(moveBulletEvent3, "translation", localPos2.x+_params.launchPos.x, localPos2.y+_params.launchPos.y, localPos2.z+_params.launchPos.z);
			SetBool(moveBulletEvent3, "cubic", true);

			local moveBulletEvent4 = CreateTickEvent(moveBulletTrack, _params.launchDelay+_params.hitTime);
			SetTemplateObject(moveBulletEvent4, "targetId", templateObjectList, bullet);
			SetTemplateObject(moveBulletEvent4, "objectSpaceId", templateObjectList, attacker);
			SetBool(moveBulletEvent4, "enableRotation", false);
			local localPos = Vector3(0, 0, _params.hitTime*_params.speed);
			local localRot = Quaternion(angle, 0, 0);
			local localPos2 = Vector3();
			localRot:Rotate(localPos2, localPos);
			print(localPos2.x..", "..localPos2.y..", "..localPos2.z);
			SetVector3(moveBulletEvent4, "translation", localPos2.x+_params.launchPos.x, localPos2.y, localPos2.z+_params.launchPos.z);
			SetBool(moveBulletEvent4, "cubic", true);

		else --仅创建投掷物
			
		end;

		local hitTrack = CreateTrack(action, "受击_"..i, "TriggerHit", true);
		local hitEvent = CreateDurationEvent(hitTrack, _params.launchDelay, _params.hitTime);
		SetArray(hitEvent, "tags", Type.string, {_params.enemyTag, _params.terrainTag});
		SetTemplateObject(hitEvent, "triggerId", templateObjectList, bullet);
		SetTemplateObject(hitEvent, "attackerId", templateObjectList, attacker);
		SetString(hitEvent, "actionName", _params.hitAction);
	end;
	
	local res = {templateObjectList, action};
	
	return res;
end;

AddTemplate("ThrowSkill", "ThrowSkillParam", "ThrowSkillGenerator");