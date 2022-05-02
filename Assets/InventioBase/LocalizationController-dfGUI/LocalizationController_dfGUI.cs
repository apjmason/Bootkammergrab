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
	public class LocalizationController_dfGUI : dfLanguageManager, ILocalizationController {
		public dfLanguageCode fallbackLanguage = dfLanguageCode.EN;
		public TextAsset applicationDataFile = null;
		public string nativeTableName = null;

		private dfLanguageManager appLocalizationManager = null;
		new void Start () {
			base.Start ();

			string validator = "Base.LanguageValidator";
			string value = LocalizedStringForKey (validator);
			if (value == validator) {
				Debug.LogWarning ("LocalizationController: System language not localized, setting fallback language.");
				base.LoadLanguage (fallbackLanguage);
				value = LocalizedStringForKey (validator);
				if (value == validator) {
					Debug.LogError ("LocalizationController: Could not localize!!!");
				}
			} else {
				Debug.Log ("LocalizationController: " + LocalizedStringForKey (validator));
			}

			if (applicationDataFile != null) {
				GameObject appLocalizationObject = new GameObject ("LocalizationController(Application)");
				appLocalizationManager = appLocalizationObject.AddComponent<dfLanguageManager> ();
				appLocalizationManager.DataFile = applicationDataFile;
				appLocalizationManager.LoadLanguage (base.CurrentLanguage);
			}

			string languageCode = base.CurrentLanguage.ToString();
			LocalizationManager.InitializeLanguage (languageCode, nativeTableName);
			if (!LocalizationManager.TestLanguage (languageCode, validator, value)) {
				Debug.LogError ("LocalizationController_dfGUI: Correlation of unity and native locale failed!");
			}

			LocalizationControllerProxy.SharedLocalizer = this;
		}


		/*
		 * ILocalizationController
		 */
		public string LocalizedStringForKey (string key) {
			string value = base.GetValue (key);
			if ((value == key) && (appLocalizationManager != null)) {
				value = appLocalizationManager.GetValue (key);
			}
			return value;
		}

	}
}