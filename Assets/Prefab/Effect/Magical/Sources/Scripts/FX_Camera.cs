using UnityEngine;
using System.Collections;

namespace MagicalFX
{
	public class FX_Camera : MonoBehaviour
	{

		private Vector3 positionTemp;
	
		void Start ()
		{
			CameraEffect.CameraFX = this;
			positionTemp = this.transform.position;
		}
	
		Vector3 forcePower;

		public void Shake (Vector3 power)
		{
			forcePower = -power;
		}

		void Update ()
		{
			forcePower = Vector3.Lerp (forcePower, Vector3.zero, Time.deltaTime * 5);	
			this.transform.position = positionTemp + new Vector3 (Mathf.Cos (Time.time * 80) * forcePower.x, Mathf.Cos (Time.time * 80) * forcePower.y, Mathf.Cos (Time.time * 80) * forcePower.z);
		}
	}

	public static class CameraEffect
	{
		public static FX_Camera CameraFX;
	
		public static void Shake (Vector3 power)
		{
			if (CameraFX != null)
				CameraFX.Shake (power);
		}
	}
}
