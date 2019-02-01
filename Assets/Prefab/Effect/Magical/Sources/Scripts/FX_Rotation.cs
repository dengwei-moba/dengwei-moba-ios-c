using UnityEngine;
using System.Collections;

namespace MagicalFX
{
	public class FX_Rotation : MonoBehaviour
	{

		public Vector3 Speed = Vector3.up;

		void Start ()
		{
	
		}
	
		void FixedUpdate ()
		{
			this.transform.Rotate (Speed);
		}
	}
}