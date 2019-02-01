using UnityEngine;
using System.Collections;

namespace MagicalFX
{
	public class FX_LifeTime : MonoBehaviour
	{

		public float LifeTime = 3;
		public GameObject SpawnAfterDead;
		private float timeTemp;
	
		void Start ()
		{
			if (SpawnAfterDead == null) {
				GameObject.Destroy (this.gameObject, LifeTime);
			} else {
				timeTemp = Time.time;
			}
		}

		void Update ()
		{
			if (SpawnAfterDead != null) {
				if (Time.time > timeTemp + LifeTime) {
					GameObject.Destroy (this.gameObject);
					GameObject.Instantiate (SpawnAfterDead, this.transform.position, SpawnAfterDead.transform.rotation);
				}
			}
		}
	}
}