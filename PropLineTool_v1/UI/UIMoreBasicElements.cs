using System;
using System.Globalization;
using ColossalFramework.UI;
using ColossalFramework.Globalization;
using PropLineTool.Parameters;
using UnityEngine;

namespace PropLineTool.UI.Elements {
    public class UINumbox2 : UITextField {
        protected float m_rawValue = 1f;
        protected int m_numDecimalDigits = 0;
        public float value {
            get {
                return m_rawValue;
            }
            set {
                float _oldValue = m_rawValue;
                m_rawValue = value;

                if (value != _oldValue) {
                    OnValueChanged(value);
                }

                SetFieldText(value, this.numDecimalDigits);
            }
        }
        /// <summary>
        /// Number of digits to display after the decimal point, range: [0, 7].
        /// </summary>
        public int numDecimalDigits {
            get {
                return m_numDecimalDigits;
            }
            set {
                value = Mathf.Clamp(value, 0, 7);
                m_numDecimalDigits = value;

                SetFieldText(value, this.numDecimalDigits);
            }
        }

        public event PropertyChangedEventHandler<float> eventValueChanged;

        public override void Awake() {
            base.Awake();

            this.numericalOnly = true;
            this.maxLength = 8;
            this.allowFloats = true;
            this.size = new Vector2(90f, 30f);
            this.padding = new RectOffset(6, 6, 8, 6);
            this.builtinKeyNavigation = true;
            this.isInteractive = true;
            this.readOnly = false;
            this.horizontalAlignment = UIHorizontalAlignment.Center;
            this.selectionSprite = "EmptySprite";
            this.selectionBackgroundColor = new Color32(0, 172, 234, 255);
            this.normalBgSprite = "TextFieldPanelHovered";
            this.disabledBgSprite = "TextFieldPanel";
            this.textColor = new Color32(0, 0, 0, 255);
            this.disabledTextColor = new Color32(0, 0, 0, 128);
            this.color = new Color32(255, 255, 255, 255);
            this.disabledColor = new Color32(180, 180, 180, 255);


            this.eventTextSubmitted += delegate (UIComponent c, string text) {
                float _result = 0f;
                if (TryParseNumbox(out _result)) {
                    value = _result;
                } else {
                    SetFieldText(this.value, this.numDecimalDigits);
                }
            };
        }

        //new as of 160815
        public override void OnDestroy() {
            this.eventTextSubmitted -= delegate (UIComponent c, string text) { };

            base.OnDestroy();
        }


        protected void OnValueChanged(float value) {
            if (this.eventValueChanged != null) {
                eventValueChanged(this, this.value);
            }
        }

        protected void SetFieldText(float value) {
            text = value.ToString("F", LocaleManager.cultureInfo);
        }

        protected void SetFieldText(float value, int numDecimalPlaces) {
            numDecimalPlaces = Mathf.Clamp(numDecimalPlaces, 0, 7);
            text = value.ToString("F" + numDecimalPlaces.ToString("F0"), LocaleManager.cultureInfo);
        }

        protected bool TryParseNumbox(out float value) {
            value = 0f;
            //attempts to parse text based on LocaleManager's cultureInfo
            if (float.TryParse(this.text, NumberStyles.Number, LocaleManager.cultureInfo, out value)) {
                return true;
            }
            //attempts to parse text based on current culture
            else if (float.TryParse(this.text, NumberStyles.Number, CultureInfo.CurrentCulture, out value)) {
                return true;
            }
            //assumes en-US is current culture
            else if (float.TryParse(this.text, out value)) {
                return true;
            }
            return false;
        }

        public static bool TryParseText(string text, out float value) {
            value = 0f;
            if (float.TryParse(text, NumberStyles.Number, LocaleManager.cultureInfo, out value)) {
                return true;
            } else if (float.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out value)) {
                return true;
            } else if (float.TryParse(text, out value)) {
                return true;
            }
            return false;
        }


    }

    public class UINumboxListing : UIPanel {
        //constants
        public static readonly Vector2 SIZE_LABEL_TEXT_DEFAULT = new Vector2(180f, 30f);
        public static readonly Vector2 SIZE_NUMBOX_DEFAULT = new Vector2(90f, 30f);
        public static readonly Vector2 SIZE_LABEL_UNITS_DEFAULT = new Vector2(24f, 30f);
        public static readonly Vector2 SIZE_PANEL_DEFAULT = new Vector2(294f, 30f);


        //ui components
        protected UILabel m_labelText;
        protected UILabel m_labelUnits;
        protected UINumbox2 m_numbox;
        public UILabel labelText {
            get {
                return m_labelText;
            }
            set {
                m_labelText = value;
            }
        }
        public UILabel labelUnits {
            get {
                return m_labelUnits;
            }
            set {
                m_labelUnits = value;
            }
        }
        public UINumbox2 numbox {
            get {
                return m_numbox;
            }
            set {
                m_numbox = value;
            }
        }

        public override void Awake() {
            base.Awake();

            this.size = SIZE_PANEL_DEFAULT;

            //labels
            labelText = UISimpleElems.CreateLabelWhite(this, "Numbox", 1f, UIHorizontalAlignment.Right, new RectOffset(2, 6, 2, 4), SIZE_LABEL_TEXT_DEFAULT, new Vector3(0f, 0f));
            labelText.relativePosition = new Vector3(0f, 0f);
            labelUnits = UISimpleElems.CreateLabelWhite(this, "u", 1f, UIHorizontalAlignment.Left, new RectOffset(4, 2, 2, 4), SIZE_LABEL_UNITS_DEFAULT, new Vector3(270f, 0f));
            labelUnits.relativePosition = new Vector3(SIZE_LABEL_TEXT_DEFAULT.x + SIZE_NUMBOX_DEFAULT.x, 0f);
            //numbox
            numbox = this.AddUIComponent<UINumbox2>();
            numbox.numDecimalDigits = 2;
            numbox.value = 1f;
            numbox.size = SIZE_NUMBOX_DEFAULT;
            numbox.relativePosition = new Vector3(SIZE_LABEL_TEXT_DEFAULT.x, 0f);
            numbox.disabledColor = new Color32(255, 255, 255, 128);
        }
    }

    public class UIDropDownListing : UIPanel {
        //constants
        public static readonly Vector2 SIZE_PANEL_DEFAULT = new Vector2(261f, 30f);
        public static readonly Vector2 SIZE_LABEL_DEFAULT = new Vector2(145f, 30f);
        public static readonly Vector2 SIZE_DROPDOWN_DEFAULT = new Vector2(116f, 30f);
        public const int ITEM_WIDTH = 116;
        public const int ITEM_HEIGHT = 26;
        public const int ITEM_SPACING = 4;

        //ui components
        protected UILabel m_label;
        protected UIDropDown m_dropDown;
        public UILabel label {
            get {
                return m_label;
            }
            set {
                m_label = value;
            }
        }
        public UIDropDown dropDown {
            get {
                return m_dropDown;
            }
            set {
                m_dropDown = value;
            }
        }

        public override void Awake() {
            base.Awake();

            this.size = SIZE_PANEL_DEFAULT;

            //label
            label = UISimpleElems.CreateLabelWhite(this, "Angle Mode", 0.875f, UIHorizontalAlignment.Right, new RectOffset(2, 6, 7, 9), SIZE_LABEL_DEFAULT, new Vector3(0f, 0f));
            label.relativePosition = new Vector3(0f, 0f);
            //dropdown
            dropDown = UISimpleElems.CreateDropDown(this, 0.875f, new RectOffset(8, 8, 8, 0), SIZE_DROPDOWN_DEFAULT, ITEM_WIDTH, ITEM_HEIGHT, 12, ITEM_SPACING);
            dropDown.items = new string[12]
            {
                "Option 1",
                "Option 2",
                "Option 3",
                "Option 4",
                "Option 5",
                "Option 6",
                "Option 7",
                "Option 8",
                "Option 9",
                "Option 10",
                "Option 11",
                "Option 12",
            };
            dropDown.relativePosition = new Vector3(SIZE_LABEL_DEFAULT.x, 0f);
        }

        public void SetDropDownItems(string[] items) {
            if (items != null && items.Length > 0) {
                dropDown.items = items;
                dropDown.listHeight = ITEM_SPACING + ((ITEM_HEIGHT + ITEM_SPACING) * items.Length);
            }
        }
    }

    public class UICheckboxListing : UIPanel {
        //ui components
        protected UIMultiStateButton m_checkbox;
        protected UILabel m_label;
        public UIMultiStateButton checkbox {
            get {
                return m_checkbox;
            }
            protected set {
                m_checkbox = value;
            }
        }
        public UILabel label {
            get {
                return m_label;
            }
            protected set {
                m_label = value;
            }
        }

        //layout stuff
        protected Vector2 m_checkboxSize = new Vector2(24f, 24f);
        public Vector2 checkboxSize {
            get {
                return m_checkboxSize;
            }
            set {
                checkbox.size = value;
                m_checkboxSize = value;

                SetLabelLines(numLabelLines);
            }
        }
        protected int m_numLabelLines = 1;
        public int numLabelLines {
            get {
                return m_numLabelLines;
            }
            set {
                value = Mathf.Clamp(value, 1, 50);

                m_numLabelLines = value;
                SetLabelLines(value);
            }
        }
        protected UITextureAtlas m_checkboxAtlas;
        public UITextureAtlas checkboxAtlas {
            get {
                return m_checkboxAtlas;
            }
            set {
                checkbox.atlas = value;
                m_checkboxAtlas = value;
            }
        }

        //check stuff
        public bool isChecked {
            get {
                if (checkbox.activeStateIndex == 0) {
                    return false;
                } else if (checkbox.activeStateIndex == 1) {
                    return true;
                } else {
                    return false;
                }
            }
            set {
                if (value == true) {
                    checkbox.activeStateIndex = 1;
                }
                if (value == false) {
                    checkbox.activeStateIndex = 0;
                }
            }
        }

        //events
        public event PropertyChangedEventHandler<bool> eventCheckChanged;
        protected void OnCheckChanged(bool value) {
            eventCheckChanged?.Invoke(this, value);
        }

        //awake
        public override void Awake() {
            base.Awake();

            this.size = new Vector2(342f, 24f);

            checkbox = UISimpleElems.AddAToggleButton(this, "", UIView.GetAView().defaultAtlas, "PLT_MultiStateZero", "PLT_MultiStateOne", "", "", new Vector2(24f, 24f));
            checkbox.relativePosition = new Vector3(0f, 0f);

            label = UISimpleElems.CreateLabelWhite(this, "[Toggle Option]", 1f, UIHorizontalAlignment.Left, new RectOffset(6, 6, 5, 0), new Vector2(318f, 42f), new Vector3(24f, 0f));
            label.verticalAlignment = UIVerticalAlignment.Top;
            label.wordWrap = true;
            label.relativePosition = new Vector3(24f, 0f);

            checkbox.eventActiveStateIndexChanged += delegate (UIComponent c, int index) {
                if (index == 0) {
                    OnCheckChanged(false);
                } else if (index == 1) {
                    OnCheckChanged(true);
                }
            };
        }

        public override void OnDestroy() {
            base.OnDestroy();

            checkbox.eventActiveStateIndexChanged -= delegate (UIComponent c, int index) {
                if (index == 0) {
                    OnCheckChanged(false);
                } else if (index == 1) {
                    OnCheckChanged(true);
                }
            };
        }

        public void SetLabelLines(int numLabelLines) {
            numLabelLines = Mathf.Clamp(numLabelLines, 1, 50);

            float _height = checkbox.height * numLabelLines;
            float _width = Mathf.Clamp(this.width, 48f, 720f);

            this.size = new Vector2(_width, _height);
            label.size = new Vector2(_width - checkbox.width, _height);

            checkbox.relativePosition = new Vector3(0f, 0f);
            label.relativePosition = new Vector3(checkbox.width, 0f);
        }
    }

    //public class UISimpleTextOverlay : UIComponent
    //{
    //    private UILabel m_label;
    //    public UILabel label
    //    {
    //        get
    //        {
    //            return m_label;
    //        }
    //        private set
    //        {
    //            m_label = value;
    //        }
    //    }

    //    //Special Thanks to ToolBase.ShowExtraInfo()
    //    public void SetTextInfo(string text, Color textColor, Vector3 position, float elevation)
    //    {
    //        if (text == null)
    //        {
    //            return;
    //        }

    //        Vector3 _worldPos = new Vector3(position.x, position.y + elevation, position.z);

    //        UIView _UIView = ToolBase.extraInfoLabel.GetUIView();
    //        Vector3 _screenPoint = Camera.main.WorldToScreenPoint(_worldPos);
    //        _screenPoint /= _UIView.inputScale;

    //        Vector3 _relativePosition = _UIView.ScreenPointToGUI(_screenPoint) - ToolBase.extraInfoLabel.size * 0.5f;

    //        label.relativePosition = relativePosition;
    //    }
    //}

    public static class UISimpleElems {
        //public static UISimpleTextOverlay CreateSimpleTextOverlay()
        //{
        //    UISimpleTextOverlay _simpleTextOverlay = UIView.GetAView().AddUIComponent(typeof(UISimpleTextOverlay)) as UISimpleTextOverlay;

        //    return _simpleTextOverlay;
        //}

        public static UIButton CreateCalcButton(UIComponent parent, string text, float textScale, Vector2 size, Vector3 relativePosition) {
            UIButton _button = parent.AddUIComponent<UIButton>();

            _button.normalBgSprite = "ButtonMenu";
            _button.focusedBgSprite = "ButtonMenuFocused";
            _button.hoveredBgSprite = "ButtonMenuHovered";
            _button.pressedBgSprite = "ButtonMenuPressed";
            _button.disabledBgSprite = "ButtonMenuDisabled";

            _button.text = text;
            _button.textScale = textScale;
            _button.textPadding = new RectOffset(1, 1, 3, 0);
            _button.textHorizontalAlignment = UIHorizontalAlignment.Center;
            _button.textVerticalAlignment = UIVerticalAlignment.Middle;
            _button.textColor = new Color32(255, 255, 255, 255);
            _button.disabledTextColor = new Color32(255, 255, 255, 128);
            _button.wordWrap = true;

            _button.playAudioEvents = true;

            _button.size = size;
            _button.relativePosition = relativePosition;

            return _button;
        }

        public static UIButton CreateBlueButton(UIComponent parent, string text, float textScale, UIHorizontalAlignment textHorizontalAlignment, UIVerticalAlignment textVerticalAlignment, RectOffset textPadding, Vector2 size, Vector3 relativePosition) {
            UIButton _button = parent.AddUIComponent<UIButton>();

            _button.normalBgSprite = "ButtonMenu";
            _button.focusedBgSprite = "ButtonMenuFocused";
            _button.hoveredBgSprite = "ButtonMenuHovered";
            _button.pressedBgSprite = "ButtonMenuPressed";
            _button.disabledBgSprite = "ButtonMenuDisabled";

            _button.text = text;
            _button.textScale = textScale;
            _button.textPadding = textPadding;
            _button.textHorizontalAlignment = textHorizontalAlignment;
            _button.textVerticalAlignment = textVerticalAlignment;
            _button.textColor = new Color32(255, 255, 255, 255);
            _button.disabledTextColor = new Color32(255, 255, 255, 128);
            _button.wordWrap = true;

            _button.playAudioEvents = true;

            _button.size = size;
            _button.relativePosition = relativePosition;

            return _button;
        }

        public static void ModifyToBlueButton(ref UIButton button, string text, float textScale, UIHorizontalAlignment textHorizontalAlignment, UIVerticalAlignment textVerticalAlignment, RectOffset textPadding, Vector2 size, Vector3 relativePosition) {
            ModifyToCustomButton(ref button, text, textScale, textHorizontalAlignment, textVerticalAlignment, textPadding, size, relativePosition, "ButtonMenu", "", null);
        }

        /// <param name="atlas">Leave null to keep default atlas.</param>
        public static void ModifyToCustomButton(ref UIButton button, Vector2 size, Vector3 relativePosition, string spriteBgPrefix, string spriteFgPrefix, UITextureAtlas atlas) {
            if (atlas != null) {
                button.atlas = atlas;
            }

            button.normalBgSprite = spriteBgPrefix + "";
            button.focusedBgSprite = spriteBgPrefix + "Focused";
            button.hoveredBgSprite = spriteBgPrefix + "Hovered";
            button.pressedBgSprite = spriteBgPrefix + "Pressed";
            button.disabledBgSprite = spriteBgPrefix + "Disabled";

            button.normalFgSprite = spriteFgPrefix + "";
            button.focusedFgSprite = spriteFgPrefix + "Focused";
            button.hoveredFgSprite = spriteFgPrefix + "Hovered";
            button.pressedFgSprite = spriteFgPrefix + "Pressed";
            button.disabledFgSprite = spriteFgPrefix + "Disabled";

            button.playAudioEvents = true;

            button.size = size;
            button.relativePosition = relativePosition;
        }

        /// <param name="atlas">Leave null to keep default atlas.</param>
        public static void ModifyToCustomButton(ref UIButton button, string text, float textScale, UIHorizontalAlignment textHorizontalAlignment, UIVerticalAlignment textVerticalAlignment, RectOffset textPadding, Vector2 size, Vector3 relativePosition, string spriteBgPrefix, string spriteFgPrefix, UITextureAtlas atlas) {
            if (atlas != null) {
                button.atlas = atlas;
            }

            button.normalBgSprite = spriteBgPrefix + "";
            button.focusedBgSprite = spriteBgPrefix + "Focused";
            button.hoveredBgSprite = spriteBgPrefix + "Hovered";
            button.pressedBgSprite = spriteBgPrefix + "Pressed";
            button.disabledBgSprite = spriteBgPrefix + "Disabled";

            button.normalFgSprite = spriteFgPrefix + "";
            button.focusedFgSprite = spriteFgPrefix + "Focused";
            button.hoveredFgSprite = spriteFgPrefix + "Hovered";
            button.pressedFgSprite = spriteFgPrefix + "Pressed";
            button.disabledFgSprite = spriteFgPrefix + "Disabled";

            button.text = text;
            button.textScale = textScale;
            button.textPadding = textPadding;
            button.textHorizontalAlignment = textHorizontalAlignment;
            button.textVerticalAlignment = textVerticalAlignment;
            button.textColor = new Color32(255, 255, 255, 255);
            button.disabledTextColor = new Color32(255, 255, 255, 128);
            button.wordWrap = true;

            button.playAudioEvents = true;

            button.size = size;
            button.relativePosition = relativePosition;
        }

        public static UILabel CreateLabelWhite(UIComponent parent, string text, float textScale, UIHorizontalAlignment textAlignment, RectOffset padding, Vector2 size, Vector3 relativePosition) {
            UILabel _label = parent.AddUIComponent<UILabel>();

            _label.autoHeight = false;
            _label.autoSize = false;

            _label.textScale = textScale;
            _label.textAlignment = textAlignment;
            _label.verticalAlignment = UIVerticalAlignment.Bottom;
            _label.textColor = new Color32(255, 255, 255, 255);
            _label.disabledTextColor = new Color32(128, 128, 128, 255);
            _label.text = text;

            _label.size = size;
            _label.padding = padding;
            _label.relativePosition = relativePosition;

            return _label;
        }

        //Special Thanks to boformer's NetworkSkins.UI.Util for this!
        public static UIDropDown CreateDropDown(UIComponent parent, float textScale, RectOffset textPadding, Vector2 size, int itemWidth, int itemHeight, int maxItems, int itemSpacing) {
            //dropdown
            UIDropDown _dropDown = parent.AddUIComponent<UIDropDown>();
            _dropDown.size = size;
            _dropDown.horizontalAlignment = UIHorizontalAlignment.Left;
            _dropDown.verticalAlignment = UIVerticalAlignment.Middle;

            itemSpacing = Mathf.Clamp(itemSpacing, 0, itemHeight * 10);
            maxItems = Mathf.Clamp(maxItems, 1, 50);

            _dropDown.autoListWidth = false;
            _dropDown.autoSize = false;
            _dropDown.listBackground = "OptionsDropboxListbox";
            _dropDown.listWidth = itemWidth;
            _dropDown.listHeight = itemSpacing + ((itemHeight + itemSpacing) * maxItems);
            _dropDown.listPosition = UIDropDown.PopupListPosition.Below;
            _dropDown.listPadding = new RectOffset(itemSpacing, itemSpacing, itemSpacing, itemSpacing);

            _dropDown.itemHeight = itemHeight;
            _dropDown.itemHover = "ListItemHover";
            _dropDown.itemHover = "ListItemHover";
            _dropDown.itemHighlight = "ListItemHighlight";
            _dropDown.itemPadding = new RectOffset(14, 0, itemSpacing * 2, 0);

            _dropDown.normalBgSprite = "OptionsDropbox";
            _dropDown.disabledBgSprite = "OptionsDropboxDisabled";
            _dropDown.hoveredBgSprite = "OptionsDropboxHovered";
            _dropDown.focusedBgSprite = "OptionsDropboxFocused";

            _dropDown.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;

            _dropDown.popupColor = new Color32(45, 52, 61, 255);
            _dropDown.popupTextColor = new Color32(170, 170, 170, 255);

            _dropDown.textScale = textScale;
            _dropDown.textFieldPadding = textPadding;

            _dropDown.playAudioEvents = true;

            _dropDown.zOrder = 1;
            _dropDown.selectedIndex = 0;

            //button
            UIButton _button = _dropDown.AddUIComponent<UIButton>();
            _dropDown.triggerButton = _button;

            _button.size = _dropDown.size;
            _button.relativePosition = new Vector3(0f, 0f);
            _button.horizontalAlignment = UIHorizontalAlignment.Right;
            _button.verticalAlignment = UIVerticalAlignment.Middle;

            _button.text = "";
            _button.textScale = textScale;
            _button.textHorizontalAlignment = UIHorizontalAlignment.Left;
            _button.textVerticalAlignment = UIVerticalAlignment.Middle;
            _button.textPadding = textPadding;

            _button.playAudioEvents = true;

            _button.zOrder = 0;

            //events
            _dropDown.eventSizeChanged += delegate (UIComponent c, Vector2 t) {
                _button.size = t;
                _dropDown.listWidth = (int)t.x;
            };

            //finally
            return _dropDown;
        }

        //creates a checkbox
        public static UICheckBox CreateCheckBox(UIComponent parent, Vector2 size, Vector3 relativePosition) {
            UICheckBox _checkBox = parent.AddUIComponent<UICheckBox>();
            _checkBox.size = size;

            _checkBox.clipChildren = true;
            UISprite _UISprite = _checkBox.AddUIComponent<UISprite>();
            _UISprite.spriteName = "ToggleBase";
            _UISprite.size = size;
            _UISprite.relativePosition = new Vector3(0f, 0f);
            _checkBox.checkedBoxObject = _checkBox.AddUIComponent<UISprite>();
            _checkBox.checkedBoxObject.size = size;
            _checkBox.checkedBoxObject.relativePosition = new Vector3(0f, 0f);
            ((UISprite)_checkBox.checkedBoxObject).spriteName = "ToggleBaseFocused";

            _checkBox.playAudioEvents = true;
            _checkBox.relativePosition = relativePosition;

            return _checkBox;
        }

        //creates a checkbox listing (checkbox multibutton + label) - default 24px square checkbox
        public static UICheckboxListing CreateCheckboxListing(UIComponent parent, int numLabelLines, Vector2 size) {
            UICheckboxListing _checkboxListing = parent.AddUIComponent<UICheckboxListing>();

            _checkboxListing.size = size;

            _checkboxListing.SetLabelLines(numLabelLines);

            return _checkboxListing;
        }

        //creates a checkbox listing (checkbox multibutton + label)
        public static UICheckboxListing CreateCheckboxListing(UIComponent parent, int numLabelLines, Vector2 size, Vector2 checkboxSize, UITextureAtlas checkboxAtlas) {
            UICheckboxListing _checkboxListing = parent.AddUIComponent<UICheckboxListing>();

            _checkboxListing.size = size;
            _checkboxListing.checkboxSize = checkboxSize;

            _checkboxListing.SetLabelLines(numLabelLines);

            _checkboxListing.checkboxAtlas = checkboxAtlas;

            return _checkboxListing;
        }

        //creates a visual divider line
        public static UITiledSprite CreateDivider(UIComponent parent, UITextureAtlas atlas, string spriteName, Vector2 size, Vector3 relativePosition) {
            UITiledSprite _divider = parent.AddUIComponent<UITiledSprite>();
            _divider.atlas = atlas;
            _divider.spriteName = spriteName;
            _divider.tileScale = new Vector2(1f, 1f);
            _divider.tileOffset = new Vector2(0f, 0f);
            _divider.size = size;
            _divider.relativePosition = relativePosition;

            return _divider;
        }


        /// <summary>
        /// Creates a toggle button (multi-state button) with two states: State 0 and State 1.
        /// </summary>
        /// <param name="bgPrefix0">State 0: background sprite prefix</param>
        /// <param name="bgPrefix1">State 1: background sprite prefix</param>
        /// <param name="fgPrefix0">State 0: foreground sprite prefix</param>
        /// <param name="fgPrefix1">State 1: foreground sprite prefix</param>
        /// <returns></returns>
        public static UIMultiStateButton AddAToggleButton(UIComponent parent, string name, UITextureAtlas atlas, string bgPrefix0, string bgPrefix1, string fgPrefix0, string fgPrefix1) {
            UIMultiStateButton _toggleButton = parent.AddUIComponent<UIMultiStateButton>();
            _toggleButton.name = name;
            _toggleButton.cachedName = name;

            _toggleButton.atlas = atlas;

            UIMultiStateButton.SpriteSetState fgSpriteSetState = _toggleButton.foregroundSprites;
            UIMultiStateButton.SpriteSetState bgSpriteSetState = _toggleButton.backgroundSprites;

            if (fgSpriteSetState == null || bgSpriteSetState == null) {
                Debug.LogError("[PLT]: UIMultiStateButton missing SpriteSetState");
            }

            UIMultiStateButton.SpriteSet fgSpriteSet0 = fgSpriteSetState[0];
            UIMultiStateButton.SpriteSet bgSpriteSet0 = bgSpriteSetState[0];

            if (fgSpriteSet0 == null) {
                fgSpriteSetState.AddState();
                fgSpriteSet0 = fgSpriteSetState[0];
            }
            if (bgSpriteSet0 == null) {
                bgSpriteSetState.AddState();
                bgSpriteSet0 = bgSpriteSetState[0];
            }

            //add state '0'
            if (fgPrefix0 != "") {
                fgSpriteSet0.normal = (fgPrefix0 + "");
                fgSpriteSet0.focused = (fgPrefix0 + "Focused");
                fgSpriteSet0.hovered = (fgPrefix0 + "Hovered");
                fgSpriteSet0.pressed = (fgPrefix0 + "Pressed");
                fgSpriteSet0.disabled = (fgPrefix0 + "Disabled");
            }
            if (bgPrefix0 != "") {
                bgSpriteSet0.normal = (bgPrefix0 + "");
                bgSpriteSet0.focused = (bgPrefix0 + "Focused");
                bgSpriteSet0.hovered = (bgPrefix0 + "Hovered");
                bgSpriteSet0.pressed = (bgPrefix0 + "Pressed");
                bgSpriteSet0.disabled = (bgPrefix0 + "Disabled");
            }

            //add state '1'
            fgSpriteSetState.AddState();
            bgSpriteSetState.AddState();
            UIMultiStateButton.SpriteSet fgSpriteSet1 = fgSpriteSetState[1];
            UIMultiStateButton.SpriteSet bgSpriteSet1 = bgSpriteSetState[1];
            if (fgPrefix1 != "") {
                fgSpriteSet1.normal = (fgPrefix1 + "");
                fgSpriteSet1.focused = (fgPrefix1 + "Focused");
                fgSpriteSet1.hovered = (fgPrefix1 + "Hovered");
                fgSpriteSet1.pressed = (fgPrefix1 + "Pressed");
                fgSpriteSet1.disabled = (fgPrefix1 + "Disabled");
            }
            if (bgPrefix1 != "") {
                bgSpriteSet1.normal = (bgPrefix1 + "");
                bgSpriteSet1.focused = (bgPrefix1 + "Focused");
                bgSpriteSet1.hovered = (bgPrefix1 + "Hovered");
                bgSpriteSet1.pressed = (bgPrefix1 + "Pressed");
                bgSpriteSet1.disabled = (bgPrefix1 + "Disabled");
            }

            //initial value
            _toggleButton.state = UIMultiStateButton.ButtonState.Normal;
            _toggleButton.activeStateIndex = 0;
            _toggleButton.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            _toggleButton.spritePadding = new RectOffset(0, 0, 0, 0);
            _toggleButton.autoSize = false;
            _toggleButton.canFocus = false;
            _toggleButton.enabled = true;
            _toggleButton.isInteractive = true;
            _toggleButton.isVisible = true;

            return _toggleButton;
        }

        /// <summary>
        /// Creates a toggle button (multi-state button) with two states: State 0 and State 1.
        /// </summary>
        /// <param name="bgPrefix0">State 0: background sprite prefix</param>
        /// <param name="bgPrefix1">State 1: background sprite prefix</param>
        /// <param name="fgPrefix0">State 0: foreground sprite prefix</param>
        /// <param name="fgPrefix1">State 1: foreground sprite prefix</param>
        /// <returns></returns>
        public static UIMultiStateButton AddAToggleButton(UIComponent parent, string name, UITextureAtlas atlas, string bgPrefix0, string bgPrefix1, string fgPrefix0, string fgPrefix1, Vector2 size) {
            UIMultiStateButton _button = AddAToggleButton(parent, name, atlas, bgPrefix0, bgPrefix1, fgPrefix0, fgPrefix1);
            _button.size = size;
            _button.playAudioEvents = true;

            return _button;
        }

        /// <summary>
        /// Creates a toggle button (multi-state button) with THREE states: State 0, State 1, and State 2.
        /// </summary>
        /// <param name="bgPrefix0">State 0: background sprite prefix</param>
        /// <param name="bgPrefix1">State 1: background sprite prefix</param>
        /// <param name="bgPrefix2">State 2: background sprite prefix</param>
        /// <param name="fgPrefix0">State 0: foreground sprite prefix</param>
        /// <param name="fgPrefix1">State 1: foreground sprite prefix</param>
        /// <param name="fgPrefix2">State 2: foreground sprite prefix</param>
        /// <returns></returns>
        public static UIMultiStateButton AddAThreeStateButton(UIComponent parent, string name, UITextureAtlas atlas, string bgPrefix0, string bgPrefix1, string bgPrefix2, string fgPrefix0, string fgPrefix1, string fgPrefix2) {
            UIMultiStateButton _toggleButton = parent.AddUIComponent<UIMultiStateButton>();
            _toggleButton.name = name;
            _toggleButton.cachedName = name;

            _toggleButton.atlas = atlas;

            UIMultiStateButton.SpriteSetState fgSpriteSetState = _toggleButton.foregroundSprites;
            UIMultiStateButton.SpriteSetState bgSpriteSetState = _toggleButton.backgroundSprites;

            if (fgSpriteSetState == null || bgSpriteSetState == null) {
                Debug.LogError("[PLT]: UIMultiStateButton missing SpriteSetState");
            }

            UIMultiStateButton.SpriteSet fgSpriteSet0 = fgSpriteSetState[0];
            UIMultiStateButton.SpriteSet bgSpriteSet0 = bgSpriteSetState[0];

            if (fgSpriteSet0 == null) {
                fgSpriteSetState.AddState();
                fgSpriteSet0 = fgSpriteSetState[0];
            }
            if (bgSpriteSet0 == null) {
                bgSpriteSetState.AddState();
                bgSpriteSet0 = bgSpriteSetState[0];
            }

            //add state '0'
            if (fgPrefix0 != "") {
                fgSpriteSet0.normal = (fgPrefix0 + "");
                fgSpriteSet0.focused = (fgPrefix0 + "Focused");
                fgSpriteSet0.hovered = (fgPrefix0 + "Hovered");
                fgSpriteSet0.pressed = (fgPrefix0 + "Pressed");
                fgSpriteSet0.disabled = (fgPrefix0 + "Disabled");
            }
            if (bgPrefix0 != "") {
                bgSpriteSet0.normal = (bgPrefix0 + "");
                bgSpriteSet0.focused = (bgPrefix0 + "Focused");
                bgSpriteSet0.hovered = (bgPrefix0 + "Hovered");
                bgSpriteSet0.pressed = (bgPrefix0 + "Pressed");
                bgSpriteSet0.disabled = (bgPrefix0 + "Disabled");
            }

            //add state '1'
            fgSpriteSetState.AddState();
            bgSpriteSetState.AddState();
            UIMultiStateButton.SpriteSet fgSpriteSet1 = fgSpriteSetState[1];
            UIMultiStateButton.SpriteSet bgSpriteSet1 = bgSpriteSetState[1];
            if (fgPrefix1 != "") {
                fgSpriteSet1.normal = (fgPrefix1 + "");
                fgSpriteSet1.focused = (fgPrefix1 + "Focused");
                fgSpriteSet1.hovered = (fgPrefix1 + "Hovered");
                fgSpriteSet1.pressed = (fgPrefix1 + "Pressed");
                fgSpriteSet1.disabled = (fgPrefix1 + "Disabled");
            }
            if (bgPrefix1 != "") {
                bgSpriteSet1.normal = (bgPrefix1 + "");
                bgSpriteSet1.focused = (bgPrefix1 + "Focused");
                bgSpriteSet1.hovered = (bgPrefix1 + "Hovered");
                bgSpriteSet1.pressed = (bgPrefix1 + "Pressed");
                bgSpriteSet1.disabled = (bgPrefix1 + "Disabled");
            }

            //add state '2'
            fgSpriteSetState.AddState();
            bgSpriteSetState.AddState();
            UIMultiStateButton.SpriteSet fgSpriteSet2 = fgSpriteSetState[2];
            UIMultiStateButton.SpriteSet bgSpriteSet2 = bgSpriteSetState[2];
            if (fgPrefix2 != "") {
                fgSpriteSet2.normal = (fgPrefix2 + "");
                fgSpriteSet2.focused = (fgPrefix2 + "Focused");
                fgSpriteSet2.hovered = (fgPrefix2 + "Hovered");
                fgSpriteSet2.pressed = (fgPrefix2 + "Pressed");
                fgSpriteSet2.disabled = (fgPrefix2 + "Disabled");
            }
            if (bgPrefix2 != "") {
                bgSpriteSet2.normal = (bgPrefix2 + "");
                bgSpriteSet2.focused = (bgPrefix2 + "Focused");
                bgSpriteSet2.hovered = (bgPrefix2 + "Hovered");
                bgSpriteSet2.pressed = (bgPrefix2 + "Pressed");
                bgSpriteSet2.disabled = (bgPrefix2 + "Disabled");
            }

            //initial value
            _toggleButton.state = UIMultiStateButton.ButtonState.Normal;
            _toggleButton.activeStateIndex = 0;
            _toggleButton.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            _toggleButton.spritePadding = new RectOffset(0, 0, 0, 0);
            _toggleButton.autoSize = false;
            _toggleButton.canFocus = false;
            _toggleButton.enabled = true;
            _toggleButton.isInteractive = true;
            _toggleButton.isVisible = true;

            return _toggleButton;
        }

    }

}