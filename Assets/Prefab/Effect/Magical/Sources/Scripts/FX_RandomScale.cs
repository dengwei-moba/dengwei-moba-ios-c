using UnityEngine;
using System.Collections;

namespace MagicalFX
{
	public class FX_RandomScale : MonoBehaviour
	{

		public float ScaleMin = 0;
		public float ScaleMax = 1;

		void Start ()
		{
			this.transform.localScale *= Random.Range (ScaleMin, ScaleMax);
		}
	
		// Update is called once per frame
		void Update ()
		{
	
		}
	}
}