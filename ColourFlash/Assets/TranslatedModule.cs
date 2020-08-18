using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranslatedModule : MonoBehaviour {

	[SerializeField] GameObject _languagesHolder;
	
	public Translation Language;
	public KMBombModule KMModule;


	public void SelectLanguage() {
		int index = Random.Range(0, _languagesHolder.transform.childCount);
		Language = _languagesHolder.transform.GetChild(index).GetComponent<Translation>();
		Language.Choose();

		KMModule.LogFormat("Selected Language: {0}, {1} ({2})\n", Language.NativeName, Language.Name, Language.Iso639);

	}
}
