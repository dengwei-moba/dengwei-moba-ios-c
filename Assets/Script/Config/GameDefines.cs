using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 阵营
/// </summary>
public enum GameCamp
{
	BLUE = 1,
	RED = 2,
	YELLOW = 3,
	MONSTER =4,
}

/// <summary>
/// Acter类型
/// </summary>
public enum ActerType
{
	PLAYER = 1,			//玩家
	SOLDIERS = 2,		//士兵 
	MONSTER = 3,		//野怪
	BUILD = 4,			//建筑物
}

/// <summary>
/// 选择不同目标的策略
/// </summary>
public enum SelectTargetTactics
{
	FRIEND_PALYER_LOWEST_HP = 1,			//最低血量的友军玩家
	FRIEND_PALYER_NEAREST_DISTANCE = 2,		//最近距离的友军玩家
	ENEMY_PALYER_LOWEST_HP = 3,				//最低血量的敌人玩家
	ENEMY_PALYER_NEAREST_DISTANCE = 4,		//最近距离的敌人玩家
	ENEMY_GUAI_LOWEST_HP = 5,				//最低血量的敌人怪物(野怪和士兵)
	ENEMY_GUAI_NEAREST_DISTANCE = 6,		//最近距离的敌人怪物(野怪和士兵)
	ENEMY_ALL_LOWEST_HP = 7,				//最低血量的敌人怪物(野怪和士兵)或敌人玩家
	ENEMY_ALL_NEAREST_DISTANCE = 8,			//最近距离的敌人怪物(野怪和士兵)或敌人玩家
	BUILD = 9,								//优先建筑物
}

public class GameDefines
{

}


