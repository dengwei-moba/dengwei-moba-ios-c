using UnityEngine;
using System.Collections;
using ClientGame.Net;
using TrueSync;

public class BloodStrip : ScriptBase
{
	//NPC模型高度
	private float npcHeight;
	//红色血条贴图
	private Texture2D blood_red;
	//黑色血条贴图
	private Texture2D blood_black;
	private ActorAttr _ActorAttr;

	void Start()
	{
		////注解1
		////得到模型原始高度
		//float size_y = collider.bounds.size.y;
		////得到模型缩放比例
		//float scal_y = transform.localScale.y;
		//它们的乘积就是高度
		npcHeight = 2.3f;// (size_y * scal_y);
		UnityEngine.Texture2D assetObj = (UnityEngine.Texture2D)_AssetManager.GetAsset<UnityEngine.Object>("textrue/bloodstrip_xue_png");
		if (assetObj != null)
		{
			blood_red = assetObj;
		}
		UnityEngine.Texture2D assetObj2 = (UnityEngine.Texture2D)_AssetManager.GetAsset<UnityEngine.Object>("textrue/bloodstrip_bg_png");
		if (assetObj2 != null)
		{
			blood_black = assetObj2;
		}
	}

	public void InitBloodStrip(ActorAttr mActorAttr)
	{
		_ActorAttr = mActorAttr;
		//一直面朝主角
		//transform.LookAt(_FightCamera.transform);
	}

	void OnGUI()
	{
		if (_ActorAttr == null) return;//|| _FightCamera == null
		//得到NPC头顶在3D世界中的坐标
		//默认NPC坐标点在脚底下，所以这里加上npcHeight它模型的高度即可
		Vector3 worldPosition = new Vector3(transform.position.x, transform.position.y + npcHeight, transform.position.z);
		//Debug.LogErrorFormat("OnGUI=================================>{0}", worldPosition.ToString());
		//根据NPC头顶的3D坐标换算成它在2D屏幕中的坐标
		Vector2 position = _FightCamera.WorldToScreenPoint(worldPosition);
		//得到真实NPC头顶的2D坐标
		position = new Vector2(position.x, Screen.height - position.y);
		//注解2
		//计算出血条的宽高
		Vector2 bloodSize = GUI.skin.label.CalcSize(new GUIContent(blood_red));

		//通过血值计算红色血条显示区域
		int blood_width = blood_red.width * FP.ToInt(_ActorAttr.Hp) / FP.ToInt(_ActorAttr.HpMax);
		//先绘制黑色血条
		GUI.DrawTexture(new Rect(position.x - (bloodSize.x / 2), position.y - bloodSize.y, bloodSize.x, bloodSize.y), blood_black);
		//在绘制红色血条
		GUI.DrawTexture(new Rect(position.x - (bloodSize.x / 2), position.y - bloodSize.y, blood_width, bloodSize.y), blood_red);

		//注解3
		//计算NPC名称的宽高
		Vector2 nameSize = GUI.skin.label.CalcSize(new GUIContent(_ActorAttr.Name));
		//设置显示颜色为黄色
		GUI.color = Color.yellow;
		//绘制NPC名称
		GUI.Label(new Rect(position.x - (nameSize.x / 2), position.y - nameSize.y - bloodSize.y, nameSize.x, nameSize.y), _ActorAttr.Name);

	}
}