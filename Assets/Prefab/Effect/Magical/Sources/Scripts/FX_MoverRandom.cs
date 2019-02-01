using UnityEngine;
using System.Collections;

namespace MagicalFX
{
	public class FX_MoverRandom : MonoBehaviour
	{
	
		public float Speed = 1;
		public Vector3 Noise = Vector3.zero;

		void Start ()
		{

		}
	
		void FixedUpdate ()
		{
		
			this.transform.position += this.transform.forward * Speed * Time.fixedDeltaTime;
			this.transform.position += new Vector3 (Random.Range (-Noise.x, Noise.x), Random.Range (-Noise.y, Noise.y), Random.Range (-Noise.z, Noise.z)) * Time.fixedDeltaTime;
		}
	}
}