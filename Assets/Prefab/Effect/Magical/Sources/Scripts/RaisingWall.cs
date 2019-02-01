using UnityEngine;
using System.Collections;

namespace MagicalFX
{
	public class RaisingWall : MonoBehaviour
	{
	
		public GameObject Skill;
		public float Offset = -7;
		public float Distance = 2;
	
		void Start ()
		{
		
			if (Skill != null) {
				FX_SpawnDirection spawn = Skill.GetComponent<FX_SpawnDirection> ();
				if (spawn) {
					Offset = -(int)((float)spawn.Number / 2.0f);
					
				}
			}
		
			Raising ();
				
		}

		void Raising ()
		{
			if (Skill != null) {
				GameObject sk = (GameObject)GameObject.Instantiate (Skill, this.transform.position + (this.transform.forward * Distance) + (this.transform.right * Offset), Skill.transform.rotation);
				sk.transform.forward = this.transform.right;
			}
		}
	}
}
