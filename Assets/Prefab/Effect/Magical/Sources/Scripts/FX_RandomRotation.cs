using UnityEngine;
using System.Collections;

namespace MagicalFX
{
	public class FX_RandomRotation : MonoBehaviour
	{

		public Vector3 Rotation;

		void Start ()
		{
			this.transform.Rotate (new Vector3 (Random.Range (-Rotation.x, Rotation.x), Random.Range (-Rotation.y, Rotation.y), Random.Range (-Rotation.z, Rotation.z)));
		}

	}
	
}