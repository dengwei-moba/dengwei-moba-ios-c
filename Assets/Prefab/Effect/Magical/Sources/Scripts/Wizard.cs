using UnityEngine;
using System.Collections;

namespace MagicalFX
{
	public class Wizard : MonoBehaviour
	{
		public GameObject[] Skills;
		private Vector3 positionLook;
		public int Index;
		public bool Showtime;
		public float Delay = 1;
		public float RandomSize = 10;
		public bool RandomSkill = false;
		private float timeTemp;
	
		void Start ()
		{
			timeTemp = Time.time;
		}

		void Update ()
		{
		
			if (Showtime) {
				if (Time.time >= timeTemp + Delay) {
				
					Ray ray = new Ray (this.transform.position + new Vector3 (Random.Range (-RandomSize, RandomSize), 0, Random.Range (-RandomSize, RandomSize)), -Vector3.up);
					RaycastHit hit;
					if (Physics.Raycast (ray, out hit, 100))
						positionLook = hit.point;

				
					Quaternion look = Quaternion.LookRotation ((positionLook - this.transform.position).normalized);
					look.eulerAngles = new Vector3 (0, look.eulerAngles.y, 0);
					this.transform.rotation = look;
					if (RandomSkill) {
						Index = Random.Range (0, Skills.Length);
					} else {
						Index += 1;
					}
					Deploy ();
					timeTemp = Time.time;	
				}
			} else {
				Aim ();
				if (Input.GetButtonDown ("Fire1")) {
					Deploy ();
				}
			}
		}

		void Deploy ()
		{
			if (Index >= Skills.Length || Index < 0)
				Index = 0;
			
			FX_Position fx = Skills [Index].GetComponent<FX_Position> ();
			if (fx) {
				if (fx.Mode == SpawnMode.Static) {
					Place (Skills [Index]);	
				}
				if (fx.Mode == SpawnMode.OnDirection) {
					PlaceDirection (Skills [Index]);	
				}	
				
			} else {
				Shoot (Skills [Index]);	
			}
		}
	
		void Aim ()
		{
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, 100))
				positionLook = hit.point;
		
			Quaternion look = Quaternion.LookRotation ((positionLook - this.transform.position).normalized);
			look.eulerAngles = new Vector3 (0, look.eulerAngles.y, 0);
			this.transform.rotation = Quaternion.Lerp (this.transform.rotation, look, 0.5f);
        
		}
	
		void Shoot (GameObject skill)
		{
			GameObject sk = (GameObject)GameObject.Instantiate (skill, this.transform.position + (Vector3.up * 0.5f) + this.transform.forward, skill.transform.rotation);
			sk.transform.forward = (positionLook - this.transform.position).normalized;
			//GameObject.Destroy (sk, 3);
		}

		void Place (GameObject skill)
		{
			GameObject sk = (GameObject)GameObject.Instantiate (skill, positionLook, skill.transform.rotation);
			//GameObject.Destroy (sk, 3);
		}

		void PlaceDirection (GameObject skill)
		{
			GameObject sk = (GameObject)GameObject.Instantiate (skill, this.transform.position + this.transform.forward, skill.transform.rotation);
			FX_Position fx = sk.GetComponent<FX_Position> ();
			if (fx.Mode == SpawnMode.OnDirection)
				fx.transform.forward = this.transform.forward;
			//GameObject.Destroy (sk, 3);
		}
	
		void OnGUI ()
		{
			string FXname = "";
			if (Index >= 0 && Index < Skills.Length && Skills.Length > 0) {
				FXname = Skills [Index].name;
			}
			GUI.Label (new Rect (30, 30, Screen.width, 100), "" + FXname);
		
		
		
			if (GUI.Button (new Rect (30, Screen.height - 40, 100, 30), "Prev")) {
				Index -= 1;
			}
		
			if (GUI.Button (new Rect (140, Screen.height - 40, 100, 30), "Next")) {
				Index += 1;
			}
		
			if (GUI.Button (new Rect (250, Screen.height - 40, 100, 30), "Show time")) {
				Showtime = !Showtime;
			}
		
			if (Index < 0) {
				Index = Skills.Length - 1;
			}

		}
	}
}
