using UnityEngine;
using System.Collections;

namespace MagicalFX
{
	[RequireComponent (typeof(Rigidbody))]

public class FX_Mover : MonoBehaviour
	{

		public float Speed = 1;
		public Vector3 Noise = Vector3.zero;
		public float Damping = 0.3f;
		Quaternion direction;

		void Start ()
		{
			direction = Quaternion.LookRotation (this.transform.forward * 1000);
			this.transform.Rotate (new Vector3 (Random.Range (-Noise.x, Noise.x), Random.Range (-Noise.y, Noise.y), Random.Range (-Noise.z, Noise.z)));
		}
	
		void LateUpdate ()
		{
			this.transform.rotation = Quaternion.Lerp (this.transform.rotation, direction, Damping);
			this.transform.position += this.transform.forward * Speed * Time.deltaTime;
		}
	}
}