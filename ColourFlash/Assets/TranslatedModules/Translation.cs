using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Translation : MonoBehaviour {

	[Tooltip("Disabled languages are never chosen, eg. because they're still a work in progress.")]
	public bool Disabled = false;
	[Header("Language Information")]
	public string Name;
	public string NativeName;
	public string Iso639;
	[Tooltip("Ticking this will swap the order of the Yes/No buttons. Used for right to left reading languages.")]
	public bool SwapButtons = false;
	[Space]
	public int Version = 1;
	public bool ManualAvailable = false;
	
	[Header("Display")]
	[Tooltip("Leave null to use the standard font for the module.")]
	public Font Font;
	[Tooltip("Leave null to use the standard font for the module.")]
	public Material FontMaterial;
	public float VerticalOffset = 0f;

	[Space]
	[Header("Module Text")]
	public string Red = "RED";
	public string Yellow = "YELLOW";
	public string Green = "GREEN";
	public string Blue = "BLUE";
	public string Magenta = "MAGENTA";
	public string White = "WHITE";
	public string Yes = "YES";
	public string No = "NO";

	[Header("Log File Text")]
	public string LogRed = "Red";
	public string LogYellow = "Yellow";
	public string LogGreen = "Green";
	public string LogBlue = "Blue";
	public string LogMagenta = "Magenta";
	public string LogWhite = "White";
	public string LogYes = "Yes";
	public string LogNo = "No";

	[Header("Sprites")]
	[Tooltip("Tick this to use sprites instead of textmesh, for when textmesh doesn't support your script.")]
	public bool UseSprites = false;
	public Sprite SpriteRed;
	public Sprite SpriteYellow;
	public Sprite SpriteGreen;
	public Sprite SpriteBlue;
	public Sprite SpriteMagenta;
	public Sprite SpriteWhite;
	public Sprite SpriteYes;
	public Sprite SpriteNo;

	[Space]
	[Header("Log File Ruling Text")]
	public string LogFileSequenceHeader = "Module generated with the following word-color sequence:";
	public string LogFileWord = "Word";
	public string LogFileColor = "Color";
	public string LogFileValidResponse = "Valid Response";
	public string LogFileRuleHeader = "The sequence matched the following block title and condition statement:";
	public string LogFileLastColor = "The color of the last word in the sequence is {0}.";
	public string LogFileUnknownTime = "[{0}] button was pressed at an unknown time!";
	public string LogFileButtonPressed = "[{0}] button was pressed on entry #{1} ('{2}' word in '{3}' color)";
	public string LogFileCorrect = "Valid answer! Module defused!";
	public string LogFileWrong = "Invalid answer! Module triggered a strike!";

	[Space]

	public string RuleRed1 = "1. If Green is used as the word at least three times in the sequence, press Yes on the third time Green is used as either the word or the color of the word in the sequence.";
	public string RuleRed2 = "2. (Otherwise,) if Blue is used as the color of the word exactly once, press No when the word Magenta is shown.";
	public string RuleRed3 = "3. (Otherwise,) press Yes the last time White is either the word or the color of the word in the sequence.";
	public string RuleYellow1 = "1. If the word Blue is shown in Green color, press Yes on the first time Green is used as the color of the word.";
	public string RuleYellow2 = "2. (Otherwise,) if the word White is shown in either White or Red color, press Yes on the second time in the sequence where the color of the word does not match the word itself.";
	public string RuleYellow3 = "3. (Otherwise,) count the number of times Magenta is used as either the word or the color of the word in the sequence (the word Magenta in Magenta color only counts as one), and press No on the color in the total's position (e.g. a total of 4 means the fourth color in sequence).";
	public string RuleGreen1 = "1. If a word occurs consecutively with different colors, press No on the fifth entry in the sequence.";
	public string RuleGreen2 = "2. (Otherwise,) if Magenta is used as the word as least three times in the sequence, press No on the first time Yellow is used as either the word or the color of the word in the sequence.";
	public string RuleGreen3 = "3. (Otherwise,) press Yes on any color where the color of the word matches the word itself.";
	public string RuleBlue1 = "1. If the color of the word does not match the word itself three times or more in the sequence, press Yes on the first time in the sequence where the color of the word does not match the word itself.";
	public string RuleBlue2 = "2. (Otherwise,) if the word Red is shown in Yellow color, or the word Yellow is shown in White color, press No when the word White is shown in Red color.";
	public string RuleBlue3 = "3. (Otherwise,) press Yes the last time Green is either the word or the color of the word in the sequence.";
	public string RuleMagenta1 = "1. If a color occurs consecutively with different words, press Yes on the third entry in the sequence.";
	public string RuleMagenta2 = "2. (Otherwise,) if the number of times the word Yellow appears is greater than the number of times that the color of the word is Blue, press No the last time the word Yellow is in the sequence.";
	public string RuleMagenta3 = "3. (Otherwise,) press No on the first time in the sequence where the color of the word matches the word of the seventh entry in the sequence.";
	public string RuleWhite1 = "1. If the color of the third word matches the word of the fourth word or fifth word, press No the first time that Blue is used as the word or the color of the word in the sequence.";
	public string RuleWhite2 = "2. (Otherwise,) if the word Yellow is shown in Red color, press Yes on the last time Blue is used as the color of the word.";
	public string RuleWhite3 = "3. (Otherwise,) press No.";





	private Dictionary<string,string> _displayLabels;
	private Dictionary<string,string> _logLabels;
	private Dictionary<string,Sprite> _spriteLabels;

	public void Choose() {
		_displayLabels = new Dictionary<string, string> {
			{ "Red", Red },
			{ "Yellow", Yellow },
			{ "Green", Green },
			{ "Blue", Blue },
			{ "Magenta", Magenta },
			{ "White", White },
			{ "Yes", Yes },
			{ "No", No }
		};

		_logLabels = new Dictionary<string, string> {
			{ "Red", LogRed },
			{ "Yellow", LogYellow },
			{ "Green", LogGreen },
			{ "Blue", LogBlue },
			{ "Magenta", LogMagenta },
			{ "White", LogWhite },
			{ "Yes", LogYes },
			{ "No", LogNo }
		};

		_spriteLabels = new Dictionary<string, Sprite> {
			{ "Red", SpriteRed },
			{ "Yellow", SpriteYellow },
			{ "Green", SpriteGreen },
			{ "Blue", SpriteBlue },
			{ "Magenta", SpriteMagenta },
			{ "White", SpriteWhite },
			{ "Yes", SpriteYes },
			{ "No", SpriteNo }
		};
	}

	public string GetDisplayFromEnglishName(string str) {
		return _displayLabels[str];
	}

	public string GetLogFromEnglishName(string str) {
		return _logLabels[str];
	}

	public Sprite GetSpriteFromEnglishName(string str) {
		return _spriteLabels[str];
	}
}
