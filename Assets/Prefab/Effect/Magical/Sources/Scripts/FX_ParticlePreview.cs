using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MagicalFX
{
	public class FX_ParticlePreview : MonoBehaviour
	{

		public GameObject[] Particles;
		public float RotationSpeed = 3;
		public int Index;
		public Texture2D logo;
	
		void Start ()
		{
	
		}
	
		public void AddParticle (Vector3 position)
		{
			if (Input.GetKeyDown (KeyCode.UpArrow)) {
				Index += 1;
				if (Index >= Particles.Length || Index < 0)
					Index = 0;
			}
			if (Input.GetKeyDown (KeyCode.DownArrow)) {
				Index -= 1;
				if (Index < 0)
					Index = Particles.Length - 1;
			}
			if (Index >= Particles.Length || Index < 0)
				Index = 0;
		
			if (Index >= 0 && Index < Particles.Length && Particles.Length > 0) {
				GameObject.Instantiate (Particles [Index], position, Particles [Index].transform.rotation);
			}
		}

		void Update ()
		{
		
			this.transform.Rotate (Vector3.up * RotationSpeed * Time.deltaTime);
			RaycastHit hit = new RaycastHit ();
			if (Input.GetButtonDown ("Fire1")) {
				var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			
				if (Physics.Raycast (ray, out hit, 1000)) {
					AddParticle (hit.point + Vector3.up);
				}
		
			}
		}
	
		void OnGUI ()
		{
			string FXname = "";
			if (Index >= 0 && Index < Particles.Length && Particles.Length > 0) {
				FXname = Particles [Index].name;
			}
			GUI.Label (new Rect (30, 30, Screen.width, 100), "Change FX : Key Up / Down \nCurrent FX " + FXname);
			if (GUI.Button (new Rect (30, 90, 200, 30), "Next")) {
				Index += 1;
				AddParticle (Vector3.up);
			}
			if (GUI.Button (new Rect (30, 130, 200, 30), "Prev")) {
				Index -= 1;
				AddParticle (Vector3.up);
			}
			if (logo)
				GUI.DrawTexture (new Rect (Screen.width - logo.width - 30, 30, logo.width, logo.height), logo);
		}
	
	
	}
}
