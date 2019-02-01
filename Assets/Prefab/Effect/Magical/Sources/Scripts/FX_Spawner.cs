using UnityEngine;
using System.Collections;

namespace MagicalFX
{
	public class FX_Spawner : MonoBehaviour
	{
		public bool FixRotation = false;
		public bool Normal;
		public GameObject FXSpawn;
		public float LifeTime = 0;
		public float TimeSpawn = 0;
		private float timeTemp;
	
		void Start ()
		{
			timeTemp = Time.time;
			if (FXSpawn != null && TimeSpawn <= 0) {
				Quaternion rotate = this.transform.rotation;
				if (!FixRotation)
					rotate = FXSpawn.transform.rotation;
			
				GameObject fx = (GameObject)GameObject.Instantiate (FXSpawn, this.transform.position, rotate);
				if (Normal)
					fx.transform.forward = this.transform.forward;
				if (LifeTime > 0)
					GameObject.Destroy (fx.gameObject, LifeTime);
			}
		}
	
		void Update ()
		{
			if (TimeSpawn > 0.0f) {
				if (Time.time > timeTemp + TimeSpawn) {
					Debug.Log("spawn");
					
					if (FXSpawn != null) {
						Quaternion rotate = this.transform.rotation;
						if (!FixRotation)
							rotate = FXSpawn.transform.rotation;
					
						GameObject fx = (GameObject)GameObject.Instantiate (FXSpawn, this.transform.position, rotate);
						if (Normal)
							fx.transform.forward = this.transform.forward;
						if (LifeTime > 0)
							GameObject.Destroy (fx.gameObject, LifeTime);
					}
					
					timeTemp = Time.time;
				}
			}
		}

	}
}
