using UnityEngine;
using System.Collections;

namespace MagicalFX
{
	public class FX_Position : MonoBehaviour
	{

		public Vector3 Offset = new Vector3 (0, 0.001f, 0);
		public bool Normal;
		public SpawnMode Mode = SpawnMode.Static;
	
		void Start ()
		{
			

		}
		void Awake(){
			if (Normal)
				PlaceNormal (this.transform.position);
		}
	
		public void PlaceNormal (Vector3 position)
		{
			RaycastHit hit;
			if (Physics.Raycast (position, -Vector3.up * 100, out hit)) {
				this.transform.position = hit.point + Offset;
				this.transform.forward = hit.normal;
			} else {
				this.transform.position = position + Offset;	
			}

		}

	}

	public enum SpawnMode
	{
		Static,
		OnDirection
	}

}