using UnityEngine;
using System.Collections;

namespace MagicalFX
{
	public class FX_ShakeCamera : MonoBehaviour
	{

		public Vector3 Power = Vector3.up;

		void Start ()
		{
			CameraEffect.Shake (Power);
		}
	}
}
