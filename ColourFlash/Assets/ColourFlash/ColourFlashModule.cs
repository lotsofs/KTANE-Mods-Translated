using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// </summary>
public class ColourFlashModule : MonoBehaviour {
    #region Nested Types
    private class ColourPair {
        public ColourPair(Colours colourText, Colours colourValue) {
            ColourText = colourText;
            ColourValue = colourValue;
        }

        public Colours ColourText;
        public Colours ColourValue;

        public string Text {
            get {
                return ColourText.ToString();
            }
        }

        public Color DisplayColour {
            get {
                return ColourAttribute.GetColourFromEnum(ColourValue);
            }
        }

        public bool HasColour(Colours colour) {
            return ColourText == colour || ColourValue == colour;
        }
    }

    private class ColourAttribute : Attribute {
        public ColourAttribute(float r, float g, float b) {
            Colour = new Color(r, g, b, 1.0f);
        }

        public static Color GetColourFromEnum<T>(T enumValue) {
            var type = enumValue.GetType();
            var memInfo = type.GetMember(enumValue.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(ColourAttribute), false);
            return (attributes.Length > 0) ? ((ColourAttribute)attributes[0]).Colour : Color.white;
        }

        public readonly Color Colour;
    }
    #endregion

    #region Enumerations
    public enum Colours {
        [Colour(1.0f, 0.0f, 0.0f)]
        Red,

        [Colour(1.0f, 1.0f, 0.0f)]
        Yellow,

        [Colour(0.0f, 1.0f, 0.0f)]
        Green,

        [Colour(0.0f, 0.0f, 1.0f)]
        Blue,

        [Colour(1.0f, 0.0f, 1.0f)]
        Magenta,

        [Colour(1.0f, 1.0f, 1.0f)]
        White
    }
    #endregion

    #region Related Components
    public KMBombInfo BombInfo;
    public KMBombModule BombModule;
    public ColourFlashButton ButtonYes;
    public ColourFlashButton ButtonNo;
    public TextMesh Indicator;
    public TranslatedModule Translation;
    #endregion

    #region Extra Preset Data
    public float TimePerCycleTick;
    public float TimePerCycleEnd;
    public const int NumberOfColoursInSequence = 8;
    public const float ProbabilityOfSameColourAndValue = 30.0f;
    #endregion

    #region Per-Module Data
    private delegate bool RuleButtonPressHandler(bool yesPress);
    private RuleButtonPressHandler _ruleButtonPressHandler = null;
    private ColourPair[] _colourSequence = null;
    private int _currentColourSequenceIndex = -1;
    private bool _solved = false;

    private ColourPair CurrentColourPair {
        get {
            if (_currentColourSequenceIndex == -1 || _colourSequence == null) {
                return null;
            }

            return _colourSequence[_currentColourSequenceIndex];
        }
    }

    #endregion

    #region Unity Lifecycle Methods
    void Awake() {
        BombModule.GenerateLogFriendlyName();
    }

    void SetLanguage() {
        // pick a language
        Translation.SelectLanguage();
        TextMesh yesMesh = ButtonYes.transform.GetChild(0).GetChild(0).GetComponent<TextMesh>();
        TextMesh noMesh = ButtonNo.transform.GetChild(0).GetChild(0).GetComponent<TextMesh>();
        yesMesh.text = Translation.Language.Yes.ToUpperInvariant();
        noMesh.text = Translation.Language.No.ToUpperInvariant();

        // swap button positions for RTL languages
        if (Translation.Language.SwapButtons) {
            Vector3 yesPos = ButtonYes.transform.localPosition;
            ButtonYes.transform.localPosition = ButtonNo.transform.localPosition;
            ButtonNo.transform.localPosition = yesPos;
        }
        // change the font if requested by the language
        if (Translation.Language.Font != null) {
            Indicator.font = Translation.Language.Font;
            Indicator.GetComponent<MeshRenderer>().material = Translation.Language.FontMaterial;
            yesMesh.font = Translation.Language.Font;
            noMesh.font = Translation.Language.Font;
            yesMesh.GetComponent<MeshRenderer>().material = Translation.Language.FontMaterial;
            noMesh.GetComponent<MeshRenderer>().material = Translation.Language.FontMaterial;
        }
        // scale text down of buttons in case it's too large.
        float width = GetTextMeshWidth(yesMesh, yesMesh.fontSize);
        int fallThrough = 630;
        while (width > 10 && fallThrough > 0) {
            yesMesh.fontSize -= 1;
            width = GetTextMeshWidth(yesMesh, yesMesh.fontSize);
            fallThrough--;
        }
        width = GetTextMeshWidth(noMesh, noMesh.fontSize);
        fallThrough = 303;
        while (width > 10 && fallThrough > 0) {
            noMesh.fontSize -= 1;
            width = GetTextMeshWidth(noMesh, noMesh.fontSize);
            fallThrough--;
        }
        // twitch
        TwitchHelpMessage = string.Format("{1}, {2} - Submit the correct response with !{0} press {3} 3, or !{0} press {4} 5.", "{0}", Translation.Language.NativeName, Translation.Language.Name, Translation.Language.Yes, Translation.Language.No);
    }

    void Start() {
        SetLanguage();
        StringBuilder logString = new StringBuilder();

        logString.Append(Translation.Language.LogFileSequenceHeader);
        logString.Append("\n");
        logString.AppendFormat("# | {0,-15} | {1,-15} | {2}", Translation.Language.LogFileWord, Translation.Language.LogFileColor, Translation.Language.LogFileValidResponse);
        logString.Append("\n--+-----------------+-----------------+----------------\n");

        string blockTitle = "";
        string condition = "";

        GenerateModuleInformation();

        RuleButtonPressHandler ruleHandler = SetupRules(ref blockTitle, ref condition);
        SetRuleButtonPressHandler(ruleHandler);

        //Logging section
        for (int colourSequenceIndex = 0; colourSequenceIndex < _colourSequence.Length; ++colourSequenceIndex) {
            _currentColourSequenceIndex = colourSequenceIndex;
            ColourPair pair = _colourSequence[_currentColourSequenceIndex];
            string response = ruleHandler(true) ? string.Format("[{0}]", Translation.Language.Yes) : (ruleHandler(false) ? string.Format("[{0}]", Translation.Language.No) : "---");

            logString.AppendFormat("{0} | {1,-12} | {2,-12} | {3}\n", colourSequenceIndex + 1, Translation.Language.GetFromEnglishName(pair.ColourText.ToString()), Translation.Language.GetFromEnglishName(pair.ColourValue.ToString()), response);
        }
        logString.Append("\n");
        logString.Append(Translation.Language.LogFileRuleHeader);
        logString.Append("\n");
        logString.AppendFormat("\"{0}\"\n-> \"{1}\"\n", blockTitle, condition);

        BombModule.Log(logString.ToString());

        _currentColourSequenceIndex = -1;

        BombModule.OnActivate += HandleModuleActive;

        ButtonYes.KMSelectable.OnInteract += HandlePressYes;
        ButtonNo.KMSelectable.OnInteract += HandlePressNo;
    }

    void OnDestroy() {
        HandleModuleInactive();
    }
    #endregion

    #region Handlers
    void HandleModuleActive() {
        StartCoroutine("ColourCycleCoroutine");
    }

    void HandleModuleInactive() {
        StopCoroutine("ColourCycleCoroutine");
    }

    bool HandlePressYes() {
        CheckRuleButtonPressHandler(true);
        return false;
    }

    bool HandlePressNo() {
        CheckRuleButtonPressHandler(false);
        return false;
    }
    #endregion

    #region Module Generation
    void GenerateModuleInformation() {
        Array colourChoices = Enum.GetValues(typeof(Colours));

        _colourSequence = new ColourPair[NumberOfColoursInSequence];
        for (int colourIndex = 0; colourIndex < NumberOfColoursInSequence; ++colourIndex) {
            float sameRoll = UnityEngine.Random.Range(0.0f, 100.0f);
            int textIndex = UnityEngine.Random.Range(0, colourChoices.Length);
            int valueIndex = textIndex;

            if (sameRoll >= ProbabilityOfSameColourAndValue) {
                valueIndex = UnityEngine.Random.Range(0, colourChoices.Length);
            }

            Colours colourText = (Colours)colourChoices.GetValue(textIndex);
            Colours colourValue = (Colours)colourChoices.GetValue(valueIndex);

            _colourSequence[colourIndex] = new ColourPair(colourText, colourValue);
        }
    }
    #endregion

    float GetTextMeshWidth(TextMesh mesh, int fontSize) {
        float width = 0;
        foreach (char symbol in mesh.text) {
            CharacterInfo info;
            if (mesh.font.GetCharacterInfo(symbol, out info, fontSize, mesh.fontStyle)) {
                width += info.advance;
            }
        }
        return width * mesh.characterSize * 0.1f; ;
    }

    #region Module Updates
    IEnumerator ColourCycleCoroutine() {
        while (true) {
            for (int colourSequenceIndex = 0; colourSequenceIndex < _colourSequence.Length; ++colourSequenceIndex) {
                _currentColourSequenceIndex = colourSequenceIndex;
                Indicator.text = Translation.Language.GetFromEnglishName(_colourSequence[colourSequenceIndex].Text).ToUpperInvariant();
                Indicator.color = _colourSequence[colourSequenceIndex].DisplayColour;
                Indicator.fontSize = 60;
                float width = GetTextMeshWidth(Indicator, Indicator.fontSize);
                int fallThrough = 50;
                while (width > 19 && fallThrough > 0) {
                    Indicator.fontSize -= 1;    // todo: This looks like a resource hog.
                    width = GetTextMeshWidth(Indicator, Indicator.fontSize);
                    fallThrough--;
                }
                yield return new WaitForSeconds(TimePerCycleTick);
            }

            _currentColourSequenceIndex = -1;

            Indicator.text = "";
            yield return new WaitForSeconds(TimePerCycleEnd);
        }
    }
    #endregion

    #region Module Rules
    private RuleButtonPressHandler SetupRules(ref string blockTitle, ref string condition) {
        CheckForClashingColours();

        blockTitle = string.Format(Translation.Language.LogFileLastColor, _colourSequence[_colourSequence.Length - 1].ColourValue.ToString());

        switch (_colourSequence[_colourSequence.Length - 1].ColourValue) {
            case Colours.Red:
                return SetupRulesForBlockA(ref blockTitle, ref condition);
            case Colours.Yellow:
                return SetupRulesForBlockB(ref blockTitle, ref condition);
            case Colours.Green:
                return SetupRulesForBlockC(ref blockTitle, ref condition);
            case Colours.Blue:
                return SetupRulesForBlockD(ref blockTitle, ref condition);
            case Colours.Magenta:
                return SetupRulesForBlockE(ref blockTitle, ref condition);
            case Colours.White:
                return SetupRulesForBlockF(ref blockTitle, ref condition);

            default:
                return null;
        }
    }

    private void CheckForClashingColours() {
        Array colourChoices = Enum.GetValues(typeof(Colours));

        for (int colourIndex = 1; colourIndex < _colourSequence.Length; ++colourIndex) {
            if (_colourSequence[colourIndex - 1].ColourText == _colourSequence[colourIndex].ColourText && _colourSequence[colourIndex - 1].ColourValue == _colourSequence[colourIndex].ColourValue) {
                if (UnityEngine.Random.Range(0, 2) == 0) {
                    _colourSequence[colourIndex].ColourText = (Colours)colourChoices.GetValue(((int)_colourSequence[colourIndex].ColourText + 1) % colourChoices.Length);
                }
                else {
                    _colourSequence[colourIndex].ColourValue = (Colours)colourChoices.GetValue(((int)_colourSequence[colourIndex].ColourValue + 1) % colourChoices.Length);
                }
            }
        }
    }

    private RuleButtonPressHandler SetupRulesForBlockA(ref string blockTitle, ref string condition) {
        if (_colourSequence.Count((x) => x.ColourText == Colours.Green) >= 3) {
            condition = Translation.Language.RuleRed1;
            return delegate (bool yesPress) {
                if (!yesPress) {
                    return false;
                }

                int greenCount = 0;
                for (int colourSequenceIndex = 0; colourSequenceIndex < _colourSequence.Length; ++colourSequenceIndex) {
                    if (_colourSequence[colourSequenceIndex].HasColour(Colours.Green)) {
                        greenCount++;
                        if (greenCount == 3) {
                            return _currentColourSequenceIndex == colourSequenceIndex;
                        }
                    }
                }

                return false;
            };
        }

        if (_colourSequence.Count((x) => x.ColourValue == Colours.Blue) == 1) {
            condition = Translation.Language.RuleRed2;

            if (!_colourSequence.Any((x) => x.ColourText == Colours.Magenta)) {
                _colourSequence[UnityEngine.Random.Range(0, _colourSequence.Length)].ColourText = Colours.Magenta;

                //Modification has been made to match this rule's exit condition - check against all pre-existing rules
                return SetupRules(ref blockTitle, ref condition);
            }

            return delegate (bool yesPress) {
                return !yesPress && CurrentColourPair != null && CurrentColourPair.ColourText == Colours.Magenta;
            };
        }

        condition = Translation.Language.RuleRed3;

        if (!_colourSequence.Any((x) => x.HasColour(Colours.White))) {
            if (UnityEngine.Random.Range(0, 2) == 0) {
                _colourSequence[UnityEngine.Random.Range(0, _colourSequence.Length - 1)].ColourText = Colours.White;
            }
            else {
                _colourSequence[UnityEngine.Random.Range(0, _colourSequence.Length - 1)].ColourValue = Colours.White;
            }

            //Modification has been made to match this rule's exit condition - check against all pre-existing rules
            return SetupRules(ref blockTitle, ref condition);
        }

        return delegate (bool yesPress) {
            return yesPress && _currentColourSequenceIndex == Array.FindLastIndex(_colourSequence, (x) => x.HasColour(Colours.White));
        };
    }

    private RuleButtonPressHandler SetupRulesForBlockB(ref string blockTitle, ref string condition) {
        if (_colourSequence.Any((x) => x.ColourText == Colours.Blue && x.ColourValue == Colours.Green)) {
            condition = Translation.Language.RuleYellow1;

            return delegate (bool yesPress) {
                return yesPress && _currentColourSequenceIndex == Array.FindIndex(_colourSequence, (x) => x.ColourValue == Colours.Green);
            };
        }

        if (_colourSequence.Any((x) => x.ColourText == Colours.White && (x.ColourValue == Colours.White || x.ColourValue == Colours.Red))) {
            condition = Translation.Language.RuleYellow2;

            bool modified = false;
            while (_colourSequence.Count((x) => x.ColourText != x.ColourValue) < 2) {
                modified = true;
                ColourPair colourPair = _colourSequence.First((x) => x.ColourText == x.ColourValue);
                colourPair.ColourText = (Colours)(((int)colourPair.ColourText + 1) % Enum.GetValues(typeof(Colours)).Length);
            }

            if (modified) {
                //Modification has been made to match this rule's exit condition - check against all pre-existing rules
                return SetupRules(ref blockTitle, ref condition);
            }

            return delegate (bool yesPress) {
                if (!yesPress) {
                    return false;
                }

                int matchCount = 0;
                for (int colourSequenceIndex = 0; colourSequenceIndex < _colourSequence.Length; ++colourSequenceIndex) {
                    if (_colourSequence[colourSequenceIndex].ColourText != _colourSequence[colourSequenceIndex].ColourValue) {
                        matchCount++;
                        if (matchCount == 2) {
                            return _currentColourSequenceIndex == colourSequenceIndex;
                        }
                    }
                }

                return false;
            };
        }

        condition = Translation.Language.RuleYellow3;

        //Otherwise, count the number of times Magenta is used as either the word or the colour of the word in the sequence, and press No on the colour in the total's position (i.e. a total of 4 means the fourth colour in sequence)
        if (!_colourSequence.Any((x) => x.HasColour(Colours.Magenta))) {
            if (UnityEngine.Random.Range(0, 2) == 0) {
                _colourSequence[UnityEngine.Random.Range(0, _colourSequence.Length - 1)].ColourText = Colours.Magenta;
            }
            else {
                _colourSequence[UnityEngine.Random.Range(0, _colourSequence.Length - 1)].ColourValue = Colours.Magenta;
            }

            //Modification has been made to match this rule's exit condition - check against all pre-existing rules
            return SetupRules(ref blockTitle, ref condition);
        }

        return delegate (bool yesPress) {
            return !yesPress && _currentColourSequenceIndex == _colourSequence.Count((x) => x.HasColour(Colours.Magenta)) - 1;
        };
    }

    private RuleButtonPressHandler SetupRulesForBlockC(ref string blockTitle, ref string condition) {
        for (int colourSequenceIndex = 0; colourSequenceIndex < _colourSequence.Length - 1; ++colourSequenceIndex) {
            condition = Translation.Language.RuleGreen1;

            if (_colourSequence[colourSequenceIndex + 1].ColourText == _colourSequence[colourSequenceIndex].ColourText) {
                return delegate (bool yesPress) {
                    return !yesPress && _currentColourSequenceIndex == 4;
                };
            }
        }

        if (_colourSequence.Count((x) => x.ColourText == Colours.Magenta) >= 3) {
            condition = Translation.Language.RuleGreen2;

            if (!_colourSequence.Any((x) => x.HasColour(Colours.Yellow))) {
                if (UnityEngine.Random.Range(0, 2) == 0) {
                    _colourSequence[UnityEngine.Random.Range(0, _colourSequence.Length - 1)].ColourText = Colours.Yellow;
                }
                else {
                    _colourSequence[UnityEngine.Random.Range(0, _colourSequence.Length - 1)].ColourValue = Colours.Yellow;
                }

                //Modification has been made to match this rule's exit condition - check against all pre-existing rules
                return SetupRules(ref blockTitle, ref condition);
            }

            return delegate (bool yesPress) {
                return !yesPress && _currentColourSequenceIndex == Array.FindIndex(_colourSequence, (x) => x.HasColour(Colours.Yellow));
            };
        }

        condition = Translation.Language.RuleGreen3;

        if (!_colourSequence.Any((x) => x.ColourText == x.ColourValue)) {
            int colourSequenceIndex = UnityEngine.Random.Range(0, _colourSequence.Length - 1);
            _colourSequence[colourSequenceIndex].ColourText = _colourSequence[colourSequenceIndex].ColourValue;

            //Modification has been made to match this rule's exit condition - check against all pre-existing rules
            return SetupRules(ref blockTitle, ref condition);
        }

        return delegate (bool yesPress) {
            return yesPress && CurrentColourPair.ColourText == CurrentColourPair.ColourValue;
        };
    }

    private RuleButtonPressHandler SetupRulesForBlockD(ref string blockTitle, ref string condition) {
        if (_colourSequence.Count((x) => x.ColourText != x.ColourValue) >= 3) {
            condition = Translation.Language.RuleBlue1;

            return delegate (bool yesPress) {
                return yesPress && _currentColourSequenceIndex == Array.FindIndex(_colourSequence, (x) => x.ColourText != x.ColourValue);
            };
        }

        if (_colourSequence.Any((x) => (x.ColourText == Colours.Red && x.ColourValue == Colours.Yellow) || (x.ColourText == Colours.Yellow && x.ColourValue == Colours.White))) {
            condition = Translation.Language.RuleBlue2;

            if (!_colourSequence.Any((x) => x.ColourText == Colours.White && x.ColourValue == Colours.Red)) {
                int colourSequenceIndex = Array.FindIndex(_colourSequence, (x) => !((x.ColourText == Colours.Red && x.ColourValue == Colours.Yellow) || (x.ColourText == Colours.Yellow && x.ColourValue == Colours.White)));
                _colourSequence[colourSequenceIndex].ColourText = Colours.White;
                _colourSequence[colourSequenceIndex].ColourValue = Colours.Red;

                //Modification has been made to match this rule's exit condition - check against all pre-existing rules
                return SetupRules(ref blockTitle, ref condition);
            }

            return delegate (bool yesPress) {
                return !yesPress && CurrentColourPair.ColourText == Colours.White && CurrentColourPair.ColourValue == Colours.Red;
            };
        }

        condition = Translation.Language.RuleBlue3;

        if (!_colourSequence.Any((x) => x.HasColour(Colours.Green))) {
            if (UnityEngine.Random.Range(0, 2) == 0) {
                _colourSequence[UnityEngine.Random.Range(0, _colourSequence.Length - 1)].ColourText = Colours.Green;
            }
            else {
                _colourSequence[UnityEngine.Random.Range(0, _colourSequence.Length - 1)].ColourValue = Colours.Green;
            }

            //Modification has been made to match this rule's exit condition - check against all pre-existing rules
            return SetupRules(ref blockTitle, ref condition);
        }

        return delegate (bool yesPress) {
            return yesPress && _currentColourSequenceIndex == Array.FindLastIndex(_colourSequence, (x) => x.HasColour(Colours.Green));
        };
    }

    private RuleButtonPressHandler SetupRulesForBlockE(ref string blockTitle, ref string condition) {
        for (int colourSequenceIndex = 0; colourSequenceIndex < _colourSequence.Length - 1; ++colourSequenceIndex) {
            condition = Translation.Language.RuleMagenta1;

            if (_colourSequence[colourSequenceIndex + 1].ColourValue == _colourSequence[colourSequenceIndex].ColourValue) {
                return delegate (bool yesPress) {
                    return yesPress && _currentColourSequenceIndex == 2;
                };
            }
        }

        if (_colourSequence.Count((x) => x.ColourText == Colours.Yellow) > _colourSequence.Count((x) => x.ColourValue == Colours.Blue)) {
            condition = Translation.Language.RuleMagenta2;

            return delegate (bool yesPress) {
                return !yesPress && _currentColourSequenceIndex == Array.FindLastIndex(_colourSequence, (x) => x.ColourText == Colours.Yellow);
            };
        }

        condition = Translation.Language.RuleMagenta3;

        Colours colourToMatch = _colourSequence[6].ColourText;
        if (!_colourSequence.Any((x) => x.ColourValue == colourToMatch)) {
            _colourSequence[UnityEngine.Random.Range(0, _colourSequence.Length - 1)].ColourValue = colourToMatch;

            //Modification has been made to match this rule's exit condition - check against all pre-existing rules
            return SetupRules(ref blockTitle, ref condition);
        }

        return delegate (bool yesPress) {
            return !yesPress && _currentColourSequenceIndex == Array.FindIndex(_colourSequence, (x) => x.ColourValue == colourToMatch);
        };
    }

    private RuleButtonPressHandler SetupRulesForBlockF(ref string blockTitle, ref string condition) {
        Colours colourToMatch = _colourSequence[2].ColourValue;
        if (_colourSequence[3].ColourText == colourToMatch || _colourSequence[4].ColourText == colourToMatch) {
            condition = Translation.Language.RuleWhite1;

            if (!_colourSequence.Any((x) => x.HasColour(Colours.Blue))) {
                if (UnityEngine.Random.Range(0, 2) == 0) {
                    _colourSequence[UnityEngine.Random.Range(0, _colourSequence.Length - 1)].ColourText = Colours.Blue;
                }
                else {
                    _colourSequence[UnityEngine.Random.Range(0, _colourSequence.Length - 1)].ColourValue = Colours.Blue;
                }

                //Modification has been made to match this rule's exit condition - check against all pre-existing rules
                return SetupRules(ref blockTitle, ref condition);
            }

            return delegate (bool yesPress) {
                return !yesPress && _currentColourSequenceIndex == Array.FindIndex(_colourSequence, (x) => x.HasColour(Colours.Blue));
            };
        }

        if (_colourSequence.Any((x) => x.ColourText == Colours.Yellow && x.ColourValue == Colours.Red)) {
            condition = Translation.Language.RuleWhite2;

            if (!_colourSequence.Any((x) => x.ColourValue == Colours.Blue)) {
                _colourSequence[UnityEngine.Random.Range(0, _colourSequence.Length - 1)].ColourValue = Colours.Blue;

                //Modification has been made to match this rule's exit condition - check against all pre-existing rules
                return SetupRules(ref blockTitle, ref condition);
            }

            return delegate (bool yesPress) {
                return yesPress && _currentColourSequenceIndex == Array.FindLastIndex(_colourSequence, (x) => x.ColourValue == Colours.Blue);
            };
        }

        condition = Translation.Language.RuleWhite3;

        return delegate (bool yesPress) {
            return !yesPress;
        };
    }

    private void SetRuleButtonPressHandler(RuleButtonPressHandler handler) {
        _ruleButtonPressHandler = handler;
    }

    private void CheckRuleButtonPressHandler(bool yesButton) {
        if (_currentColourSequenceIndex < 0) {
            BombModule.LogFormat(Translation.Language.LogFileUnknownTime, yesButton ? Translation.Language.Yes : Translation.Language.No);
        }
        else {
            ColourPair pair = _colourSequence[_currentColourSequenceIndex];
            BombModule.LogFormat(Translation.Language.LogFileButtonPressed, yesButton ? Translation.Language.Yes : Translation.Language.No, _currentColourSequenceIndex + 1, pair.ColourText.ToString(), pair.ColourValue.ToString());
        }

        if (_ruleButtonPressHandler != null && _ruleButtonPressHandler(yesButton)) {
            BombModule.Log(Translation.Language.LogFileCorrect);
            BombModule.HandlePass();
            FinishModule();
        }
        else {
            BombModule.Log(Translation.Language.LogFileWrong);
            BombModule.HandleStrike();
        }
    }

    private void FinishModule() {
        HandleModuleInactive();
        _solved = true;
    }
    #endregion

    #region Twitch Plays

    public string TwitchHelpMessage = "I changed this a hundred times now and it still shows the message I wrote half a week ago. If you're reading this, please tell me so I know something magically fixed itself.";

    public IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Trim();
        List<string> split = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        KMSelectable buttonToPress;
        if (split[0] == "press") {
            split.RemoveAt(0);
        }
        if (split.Count == 0) {
            yield break;
        }
        Debug.Log(split[0]);
        if(split[0] == Translation.Language.Yes.ToLowerInvariant()) {
            buttonToPress = ButtonYes.KMSelectable;
        }
        else if (split[0] == Translation.Language.No.ToLowerInvariant()) {
            buttonToPress = ButtonNo.KMSelectable;
        }
        else {
            yield break;
        }

        int position;
        if (split.Count == 1 || split[1] == "any") {
            // press whenever
            yield return buttonToPress;
        }
        else if (char.IsDigit(split[1][0]) && split[1].Length == 1) {
            // press at specific entry
            bool success = int.TryParse(split[1], out position);
            if (!success || position > 8 || position <= 0) {
                yield break;
            }
            position--;
            while (position != _currentColourSequenceIndex) {
                yield return new WaitForSeconds(0.1f);
            }
            yield return buttonToPress;
        }
        else {
            // invalid command
            yield break;
        }

        yield return null;
    }

    public IEnumerator TwitchHandleForcedSolve() {
        while (!_solved) {
            if (_ruleButtonPressHandler != null && _ruleButtonPressHandler(true)) {
                ButtonYes.KMSelectable.OnInteract();
            }
            else if (_ruleButtonPressHandler != null && _ruleButtonPressHandler(false)) {
                ButtonNo.KMSelectable.OnInteract();
            }
            else {
                yield return true;
            }
        }
    }
    #endregion
}
