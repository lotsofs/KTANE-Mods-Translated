using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TranslatedModule : MonoBehaviour {

	const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

	[SerializeField] GameObject _languagesHolder;
	
	public KMBombModule KMModule;
	public KMModSettings KMSettings;
	public TextMesh Sticker;
	public TextMesh StickerID;

	[NonSerialized] public Translation Language;
	[Header("Debug")]
	public Translation Override;

	TranslationSettings _settings;

	public void SelectLanguage() {
		_settings = JsonConvert.DeserializeObject<TranslationSettings>(KMSettings.Settings);

		if (_settings.UseAllLanguages) {
			int index = UnityEngine.Random.Range(0, _languagesHolder.transform.childCount);
			Language = _languagesHolder.transform.GetChild(index).GetComponent<Translation>();
		}
		else {
			Translation transl;
			List<Translation> availableTranslations = new List<Translation>();
			for (int i = _languagesHolder.transform.childCount - 1; i >= 0; i--) {
				transl = _languagesHolder.transform.GetChild(i).GetComponent<Translation>();
				if (_settings.LanguagePool.Contains(transl.Iso639)) {
					availableTranslations.Add(transl);
				}
			}
			if (availableTranslations.Count == 0) {
				KMModule.LogFormat("None of the languages specified in the configuration file exist for this module. Provided languages: {0}.", string.Join(", ", _settings.LanguagePool));
				Language = _languagesHolder.transform.Find("English").GetComponent<Translation>();
			}
			else {
				int index = UnityEngine.Random.Range(0, availableTranslations.Count);
				Language = availableTranslations[index];
			}
		}

		if (Override != null) { 
			Language = Override;
		}
		Language.Choose();
		KMModule.LogFormat("Selected Language: {0}, {1} ({2})\n", Language.NativeName, Language.Name, Language.Iso639);

		Sticker.text = Language.Iso639.ToUpperInvariant();
		GenerateStickerBlackPart();
	}

	/// <summary>
	/// Adds some aesthetics to the sticker
	/// </summary>
	public void GenerateStickerBlackPart() {
		string stickerTop = string.Empty;
		stickerTop += "TLM436F66."; // TransLated Module + 'Cof' in hex
		for (int i = 0; i < 5; i++) {
			stickerTop += chars[UnityEngine.Random.Range(0, chars.Length)];
		}
		stickerTop += string.Format(".{0}{1}{2}", Language.Iso639.ToUpper(), UnityEngine.Random.Range(0, 1000).ToString("000"), Language.Version.ToString());
		StickerID.text = stickerTop;
	}
}
