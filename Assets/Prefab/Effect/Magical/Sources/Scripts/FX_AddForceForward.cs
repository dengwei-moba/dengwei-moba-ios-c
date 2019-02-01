using UnityEngine;
using System.Collections;

namespace MagicalFX
{
	[RequireComponent (typeof(Rigidbody))]

public class FX_AddForceForward : MonoBehaviour
	{
	
		public float Force = 300;
		void Start ()
		{
			Rigidbody rig = GetComponent<Rigidbody>();
			if (rig) {
				rig.AddForce (this.transform.forward * Force);
			}
		}

		void Update ()
		{
		
		}
	}
}
