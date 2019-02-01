using UnityEngine;
using System.Collections;

namespace MagicalFX
{
	public class FX_SpawnDirection : MonoBehaviour
	{
		public int Number = 10;
		public float Frequency = 1;
		public bool FixRotation = false;
		public bool Normal;
		public GameObject FXSpawn;
		public float LifeTime = 0;
		public float TimeSpawn = 0;
		private float timeTemp;
		public bool UseObjectForward = true;
		public Vector3 Direction = Vector3.forward;
		
		void Start ()
		{
			counter = 0;
			timeTemp = Time.time;
			if (TimeSpawn <= 0) {
				for (int i=0; i<Number-1; i++) {
					if (UseObjectForward) {
						Direction = this.transform.forward;
					}
					Spawn (this.transform.position + (Direction * Frequency * i));
				}
				Destroy(this.gameObject);
			}
			
		}

		private int counter = 0;

		void Update ()
		{
			if(counter >= Number-1)
				Destroy(this.gameObject);
			
			if (TimeSpawn > 0.0f) {
				if (Time.time > timeTemp + TimeSpawn) {
					if (UseObjectForward) {
						Direction = this.transform.forward;
					}
					Spawn (this.transform.position + (Direction * Frequency * counter));
					counter+=1;
					timeTemp = Time.time;
				}
			}
		}
		
		void Spawn (Vector3 position)
		{
			if (FXSpawn != null) {
				Quaternion rotate = this.transform.rotation;
				if (!FixRotation)
					rotate = FXSpawn.transform.rotation;
					
				GameObject fx = (GameObject)GameObject.Instantiate (FXSpawn, position, rotate);
				if (Normal)
					fx.transform.forward = this.transform.forward;
						
				if (LifeTime > 0)
					GameObject.Destroy (fx.gameObject, LifeTime);
			}
		}
	}
}
