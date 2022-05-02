//
// Sitsim AR Editor, a Unity3D add-on and framework for creating mobile applications with indirect augmented reality.
// Copyright (c) 2019 University of Oslo, CodeGrind AB and CINE (a project under the NPA EU Interreg program)
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//

using UnityEngine;
using System.Collections;

namespace InventioBase {
	public class MapBalloonTracker : MonoBehaviour, IBalloonObserverInterface {

		private Hashtable sceneBalloons = new Hashtable(); // {<Scene Same> : {<Balloon Name> : {<Position> : <Vector3>, <Activated> : <true/false>}}}
		private Hashtable currentBalloons = null;
		

		void Start () 
		{
			SetupBalloons ();

		}
	
		void OnLevelWasLoaded(int level) {
			SetupBalloons ();
		}

		private void SetupBalloons()
		{
			this.currentBalloons = null;
			if (this.sceneBalloons.ContainsKey (Application.loadedLevelName)) {
				this.currentBalloons = this.sceneBalloons [Application.loadedLevelName] as Hashtable;
			} else {
				this.currentBalloons = CreateBalloonTable();
			}

			// Start observing balloons...
			GameObject balloonsRoot = GameObject.Find ("/Balloons");
			if (balloonsRoot != null) {
				BalloonController[] balloons = balloonsRoot.GetComponentsInChildren<BalloonController>();
				for (int idx = 0; idx < balloons.Length; ++idx) {
					BalloonController balloon = balloons[idx];
					balloon.AddObserver(this);
				}
			}
		}
		
		private Hashtable CreateBalloonTable() {
			Hashtable balloonTable = new Hashtable ();
			GameObject balloonsRoot = GameObject.Find ("/Balloons");
			if (balloonsRoot != null) {
				BalloonController[] balloons = balloonsRoot.GetComponentsInChildren<BalloonController>();
				for (int idx = 0; idx < balloons.Length; ++idx) {
					BalloonController balloon = balloons[idx];
					Hashtable balloonRecord = new Hashtable();
					balloonRecord["Position"] = balloon.transform.position;
					balloonRecord["Activated"] = false;
					balloonTable[balloon.balloonName] = balloonRecord;
				}
			}
			this.sceneBalloons [Application.loadedLevelName] = balloonTable;
			return balloonTable;
		}

		public Vector3[] GetBalloonPositions(bool thatIsActivated)
		{
			ArrayList activated = new ArrayList();
			foreach (Hashtable balloonRecord in this.currentBalloons.Values) {
				bool isActivated = (bool)balloonRecord["Activated"];
				if (isActivated == thatIsActivated) {
					activated.Add(balloonRecord["Position"]);
				}
			}
			return ((Vector3[])activated.ToArray(typeof(Vector3)));
		}

		// IBalloonObserverInterface
		public void BalloonActivated(BalloonController balloon)
		{
			if (this.currentBalloons.ContainsKey (balloon.balloonName)) {
				Hashtable balloonRecord = this.currentBalloons[balloon.balloonName] as Hashtable;
				balloonRecord["Activated"] = true;
			}
		}
	}
}