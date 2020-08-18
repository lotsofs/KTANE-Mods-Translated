using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Translation : MonoBehaviour {

	public string Name;
	public string NativeName;
	public string Iso639;

	[Space]
	[Space]

	public string Red = "Red";
	public string Yellow = "Yellow";
	public string Green = "Green";
	public string Blue = "Blue";
	public string Magenta = "Magenta";
	public string White = "White";
	public string Yes = "Yes";
	public string No = "No";

	[Space]
	[Header("Log File")]
	public string LogFileSequenceHeader = "Module generated with the following word-color sequence:";
	public string LogFileSequenceLabels = "# | Word    | Color   | Valid Response";
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





	private Dictionary<string,string> _dictionary;

	public void Choose() {
		_dictionary = new Dictionary<string, string>();
		_dictionary.Add("Red", Red);
		_dictionary.Add("Yellow", Yellow);
		_dictionary.Add("Green", Green);
		_dictionary.Add("Blue", Blue);
		_dictionary.Add("Magenta", Magenta);
		_dictionary.Add("White", White);
		_dictionary.Add("Yes", Yes);
		_dictionary.Add("No", No);
	}

	public string GetFromEnglishName(string str) {
		return _dictionary[str];
	}
}
