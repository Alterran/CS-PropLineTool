using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework.UI;
using PropLineTool.UI.Elements;
using PropLineTool.Utility;
using PropLineTool.Parameters;
using PropLineTool.UI.ModeButtons;
using PropLineTool.Sprites;
using UnityEngine;

//debug only
//using PropLineTool.DebugUtils;

namespace PropLineTool.UI.ControlPanel {
    public abstract class UIBasicCalculator : UIPanel {
        //background panels
        protected UIPanel setPanel;
        protected UIPanel adjustPanel;

        //button dictionary
        protected Dictionary<string, UIButton> buttons = new Dictionary<string, UIButton>(20);

        public Color32 hoverTextColor {
            set {
                var _keyList = buttons.Keys;
                foreach (var _key in _keyList) {
                    buttons[_key].hoveredTextColor = value;
                }
            }
        }
        public Color32 focusTextColor {
            set {
                var _keyList = buttons.Keys;
                foreach (var _key in _keyList) {
                    buttons[_key].focusedTextColor = value;
                }
            }
        }



        public UIButton AddButton(UIComponent parent, string buttonKey) {
            if (!buttons.ContainsKey(buttonKey)) {
                UIButton _button = parent.AddUIComponent<UIButton>();

                buttons.Add(buttonKey, _button);

                return _button;
            } else {
                return buttons[buttonKey];
            }
        }

        public UIButton AddButton(UIComponent parent, string buttonKey, string text, float textScale, Vector2 size, Vector3 relativePosition) {
            if (!buttons.ContainsKey(buttonKey)) {
                UIButton _button = UI.Elements.UISimpleElems.CreateCalcButton(parent, text, textScale, size, relativePosition);
                _button.relativePosition = relativePosition;

                buttons.Add(buttonKey, _button);

                return _button;
            } else {
                return buttons[buttonKey];
            }
        }

        public UIButton AddButton(UIComponent parent, string buttonKey, string text, float textScale, Vector2 size, Vector3 relativePosition, out bool result) {
            if (!buttons.ContainsKey(buttonKey)) {
                UIButton _button = UI.Elements.UISimpleElems.CreateCalcButton(parent, text, textScale, size, relativePosition);
                _button.relativePosition = relativePosition;

                buttons.Add(buttonKey, _button);

                result = true;
                return _button;
            } else {
                result = false;
                return buttons[buttonKey];
            }
        }

        public UIButton AddButton(UIComponent parent, string buttonKey, out bool result) {
            if (!buttons.ContainsKey(buttonKey)) {
                UIButton _button = parent.AddUIComponent<UIButton>();

                buttons.Add(buttonKey, _button);

                result = true;
                return _button;
            } else {
                result = false;
                return buttons[buttonKey];
            }
        }

        public bool RemoveButton(string buttonKey) {
            if (!buttons.ContainsKey(buttonKey)) {
                UIButton _button = buttons[buttonKey];
                buttons.Remove(buttonKey);

                UIComponent _parent = _button.parent;
                _parent.RemoveUIComponent(_button);
                UnityEngine.Object.Destroy(_button.gameObject);

                return true;
            } else {
                return false;
            }
        }

        public override void Awake() {
            base.Awake();

            this.size = new Vector2(323f, 40f);
            setPanel.backgroundSprite = "GenericPanelLight";
            setPanel.color = new Color32(90, 100, 105, 255);
            adjustPanel.backgroundSprite = "GenericPanelLight";
            adjustPanel.color = new Color32(74, 83, 88, 255);
        }

        public UIBasicCalculator() {
            setPanel = this.AddUIComponent<UIPanel>();
            adjustPanel = this.AddUIComponent<UIPanel>();
        }

        public virtual void SetVanillaAtlas(UITextureAtlas atlas) {
            setPanel.atlas = atlas;
            adjustPanel.atlas = atlas;

            var _keyList = buttons.Keys;
            foreach (var _key in _keyList) {
                buttons[_key].atlas = atlas;
            }
        }
    }

    public class UIBasicSpacingCalculator : UIBasicCalculator {
        public override void Awake() {
            base.Awake();

            //setup panels
            setPanel.size = new Vector2(112f, 40f);
            setPanel.relativePosition = new Vector3(0f, 0f);
            adjustPanel.size = new Vector2(203f, 40f);
            adjustPanel.relativePosition = new Vector3(120f, 0f);

            float _textScaleDefault = 0.6875f;
            Vector2 _longsize = new Vector2(50f, 14f);
            Vector2 _shortSize = new Vector2(25f, 14f);

            //add buttons
            //set
            AddButton(setPanel, "setDefault", "Default", _textScaleDefault, new Vector2(50f, 32f), new Vector3(4f, 4f));
            AddButton(setPanel, "setLength", "Length", _textScaleDefault, _longsize, new Vector3(58f, 4f));
            AddButton(setPanel, "setWidth", "Width", _textScaleDefault, _longsize, new Vector3(58f, 22f));
            //adjust
            AddButton(adjustPanel, "addZeroPointOne", "+0.1", 0.625f, _shortSize, new Vector3(4f, 4f));
            AddButton(adjustPanel, "subZeroPointOne", "-0.1", 0.625f, _shortSize, new Vector3(4f, 22f));
            AddButton(adjustPanel, "addOne", "+ 1", _textScaleDefault, _shortSize, new Vector3(33f, 4f));
            AddButton(adjustPanel, "subOne", "- 1", _textScaleDefault, _shortSize, new Vector3(33f, 22f));
            AddButton(adjustPanel, "addTen", "+10", _textScaleDefault, _shortSize, new Vector3(62f, 4f));
            AddButton(adjustPanel, "subTen", "-10", _textScaleDefault, _shortSize, new Vector3(62f, 22f));
            AddButton(adjustPanel, "addOneHundred", "+100", _textScaleDefault, new Vector2(31f, 14f), new Vector3(91f, 4f));
            AddButton(adjustPanel, "subOneHundred", "-100", _textScaleDefault, new Vector2(31f, 14f), new Vector3(91f, 22f));
            AddButton(adjustPanel, "round", "Round", _textScaleDefault, new Vector2(73f, 32f), new Vector3(126f, 4f));

            //setup button events
            //set
            buttons["setDefault"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    PropLineTool.placementCalculator.SetDefaultSpacing();
                }
            };
            buttons["setLength"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    PropLineTool.placementCalculator.spacingSingle = PropLineTool.placementCalculator.getLength;
                }
            };
            buttons["setWidth"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    PropLineTool.placementCalculator.spacingSingle = PropLineTool.placementCalculator.getWidth;
                }
            };
            //adjust
            buttons["addZeroPointOne"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    PropLineTool.placementCalculator.spacingSingle += 0.1f;
                }
            };
            buttons["subZeroPointOne"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    PropLineTool.placementCalculator.spacingSingle -= 0.1f;
                }
            };
            buttons["addOne"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    PropLineTool.placementCalculator.spacingSingle += 1f;
                }
            };
            buttons["subOne"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    PropLineTool.placementCalculator.spacingSingle -= 1f;
                }
            };
            buttons["addTen"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    PropLineTool.placementCalculator.spacingSingle += 10f;
                }
            };
            buttons["subTen"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    PropLineTool.placementCalculator.spacingSingle -= 10f;
                }
            };
            buttons["addOneHundred"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    PropLineTool.placementCalculator.spacingSingle += 100f;
                }
            };
            buttons["subOneHundred"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    PropLineTool.placementCalculator.spacingSingle -= 100f;
                }
            };
            buttons["round"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    PropLineTool.placementCalculator.spacingSingle = Mathf.Round(PropLineTool.placementCalculator.spacingSingle);
                }
            };



        }
    }

    public class UIBasicAngleCalculator : UIBasicCalculator {
        public override void Awake() {
            base.Awake();

            //setup panels
            setPanel.size = new Vector2(58f, 40f);
            setPanel.relativePosition = new Vector3(0f, 0f);
            adjustPanel.size = new Vector2(255f, 40f);
            adjustPanel.relativePosition = new Vector3(68f, 0f);

            float _textScaleDefault = 0.6875f;
            Vector2 _longsize = new Vector2(50f, 14f);
            Vector2 _shortSize = new Vector2(25f, 14f);

            //add buttons
            //set
            AddButton(setPanel, "setZero", "Zero", _textScaleDefault, new Vector2(50f, 32f), new Vector3(4f, 4f));
            //adjust
            AddButton(adjustPanel, "addZeroPointOne", "+0.1", 0.625f, _shortSize, new Vector3(4f, 4f));
            AddButton(adjustPanel, "subZeroPointOne", "-0.1", 0.625f, _shortSize, new Vector3(4f, 22f));
            AddButton(adjustPanel, "addOne", "+ 1", _textScaleDefault, _shortSize, new Vector3(33f, 4f));
            AddButton(adjustPanel, "subOne", "- 1", _textScaleDefault, _shortSize, new Vector3(33f, 22f));
            AddButton(adjustPanel, "addTen", "+10", _textScaleDefault, _shortSize, new Vector3(62f, 4f));
            AddButton(adjustPanel, "subTen", "-10", _textScaleDefault, _shortSize, new Vector3(62f, 22f));
            AddButton(adjustPanel, "addThirty", "+30", _textScaleDefault, _shortSize, new Vector3(91f, 4f));
            AddButton(adjustPanel, "subThirty", "-30", _textScaleDefault, _shortSize, new Vector3(91f, 22f));
            AddButton(adjustPanel, "addFortyFive", "+45", _textScaleDefault, _shortSize, new Vector3(120f, 4f));
            AddButton(adjustPanel, "subFortyFive", "-45", _textScaleDefault, _shortSize, new Vector3(120f, 22f));
            AddButton(adjustPanel, "addNinety", "+90", _textScaleDefault, _shortSize, new Vector3(149f, 4f));
            AddButton(adjustPanel, "subNinety", "-90", _textScaleDefault, _shortSize, new Vector3(149f, 22f));
            AddButton(adjustPanel, "round", "Round", _textScaleDefault, new Vector2(73f, 32f), new Vector3(178f, 4f));

            //setup button events
            //set
            //be sure to multiply by Mathf.Deg2Rad!
            buttons["setZero"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    switch (PropLineTool.placementCalculator.angleMode) {
                        case PropLineTool.PlacementCalculator.AngleMode.Dynamic: {
                            PropLineTool.placementCalculator.angleOffset = 0f * Mathf.Deg2Rad;
                            break;
                        }
                        case PropLineTool.PlacementCalculator.AngleMode.Single: {
                            PropLineTool.placementCalculator.angleSingle = 0f * Mathf.Deg2Rad;
                            break;
                        }
                    }
                }
            };
            //adjust
            //be sure to multiply by Mathf.Deg2Rad!
            buttons["addZeroPointOne"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    switch (PropLineTool.placementCalculator.angleMode) {
                        case PropLineTool.PlacementCalculator.AngleMode.Dynamic: {
                            PropLineTool.placementCalculator.angleOffset += 0.1f * Mathf.Deg2Rad;
                            break;
                        }
                        case PropLineTool.PlacementCalculator.AngleMode.Single: {
                            PropLineTool.placementCalculator.angleSingle += 0.1f * Mathf.Deg2Rad;
                            break;
                        }
                    }
                }
            };
            buttons["subZeroPointOne"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    switch (PropLineTool.placementCalculator.angleMode) {
                        case PropLineTool.PlacementCalculator.AngleMode.Dynamic: {
                            PropLineTool.placementCalculator.angleOffset -= 0.1f * Mathf.Deg2Rad;
                            break;
                        }
                        case PropLineTool.PlacementCalculator.AngleMode.Single: {
                            PropLineTool.placementCalculator.angleSingle -= 0.1f * Mathf.Deg2Rad;
                            break;
                        }
                    }
                }
            };
            buttons["addOne"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    switch (PropLineTool.placementCalculator.angleMode) {
                        case PropLineTool.PlacementCalculator.AngleMode.Dynamic: {
                            PropLineTool.placementCalculator.angleOffset += 1f * Mathf.Deg2Rad;
                            break;
                        }
                        case PropLineTool.PlacementCalculator.AngleMode.Single: {
                            PropLineTool.placementCalculator.angleSingle += 1f * Mathf.Deg2Rad;
                            break;
                        }
                    }
                }
            };
            buttons["subOne"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    switch (PropLineTool.placementCalculator.angleMode) {
                        case PropLineTool.PlacementCalculator.AngleMode.Dynamic: {
                            PropLineTool.placementCalculator.angleOffset -= 1f * Mathf.Deg2Rad;
                            break;
                        }
                        case PropLineTool.PlacementCalculator.AngleMode.Single: {
                            PropLineTool.placementCalculator.angleSingle -= 1f * Mathf.Deg2Rad;
                            break;
                        }
                    }
                }
            };
            buttons["addTen"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    switch (PropLineTool.placementCalculator.angleMode) {
                        case PropLineTool.PlacementCalculator.AngleMode.Dynamic: {
                            PropLineTool.placementCalculator.angleOffset += 10f * Mathf.Deg2Rad;
                            break;
                        }
                        case PropLineTool.PlacementCalculator.AngleMode.Single: {
                            PropLineTool.placementCalculator.angleSingle += 10f * Mathf.Deg2Rad;
                            break;
                        }
                    }
                }
            };
            buttons["subTen"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    switch (PropLineTool.placementCalculator.angleMode) {
                        case PropLineTool.PlacementCalculator.AngleMode.Dynamic: {
                            PropLineTool.placementCalculator.angleOffset -= 10f * Mathf.Deg2Rad;
                            break;
                        }
                        case PropLineTool.PlacementCalculator.AngleMode.Single: {
                            PropLineTool.placementCalculator.angleSingle -= 10f * Mathf.Deg2Rad;
                            break;
                        }
                    }
                }
            };
            buttons["addThirty"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    switch (PropLineTool.placementCalculator.angleMode) {
                        case PropLineTool.PlacementCalculator.AngleMode.Dynamic: {
                            PropLineTool.placementCalculator.angleOffset += 30f * Mathf.Deg2Rad;
                            break;
                        }
                        case PropLineTool.PlacementCalculator.AngleMode.Single: {
                            PropLineTool.placementCalculator.angleSingle += 30f * Mathf.Deg2Rad;
                            break;
                        }
                    }
                }
            };
            buttons["subThirty"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    switch (PropLineTool.placementCalculator.angleMode) {
                        case PropLineTool.PlacementCalculator.AngleMode.Dynamic: {
                            PropLineTool.placementCalculator.angleOffset -= 30f * Mathf.Deg2Rad;
                            break;
                        }
                        case PropLineTool.PlacementCalculator.AngleMode.Single: {
                            PropLineTool.placementCalculator.angleSingle -= 30f * Mathf.Deg2Rad;
                            break;
                        }
                    }
                }
            };
            buttons["addFortyFive"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    switch (PropLineTool.placementCalculator.angleMode) {
                        case PropLineTool.PlacementCalculator.AngleMode.Dynamic: {
                            PropLineTool.placementCalculator.angleOffset += 45f * Mathf.Deg2Rad;
                            break;
                        }
                        case PropLineTool.PlacementCalculator.AngleMode.Single: {
                            PropLineTool.placementCalculator.angleSingle += 45f * Mathf.Deg2Rad;
                            break;
                        }
                    }
                }
            };
            buttons["subFortyFive"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    switch (PropLineTool.placementCalculator.angleMode) {
                        case PropLineTool.PlacementCalculator.AngleMode.Dynamic: {
                            PropLineTool.placementCalculator.angleOffset -= 45f * Mathf.Deg2Rad;
                            break;
                        }
                        case PropLineTool.PlacementCalculator.AngleMode.Single: {
                            PropLineTool.placementCalculator.angleSingle -= 45f * Mathf.Deg2Rad;
                            break;
                        }
                    }
                }
            };
            buttons["addNinety"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    switch (PropLineTool.placementCalculator.angleMode) {
                        case PropLineTool.PlacementCalculator.AngleMode.Dynamic: {
                            PropLineTool.placementCalculator.angleOffset += 90f * Mathf.Deg2Rad;
                            break;
                        }
                        case PropLineTool.PlacementCalculator.AngleMode.Single: {
                            PropLineTool.placementCalculator.angleSingle += 90f * Mathf.Deg2Rad;
                            break;
                        }
                    }
                }
            };
            buttons["subNinety"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    switch (PropLineTool.placementCalculator.angleMode) {
                        case PropLineTool.PlacementCalculator.AngleMode.Dynamic: {
                            PropLineTool.placementCalculator.angleOffset -= 90f * Mathf.Deg2Rad;
                            break;
                        }
                        case PropLineTool.PlacementCalculator.AngleMode.Single: {
                            PropLineTool.placementCalculator.angleSingle -= 90f * Mathf.Deg2Rad;
                            break;
                        }
                    }
                }
            };
            buttons["round"].eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                if (p.buttons == UIMouseButton.Left) {
                    switch (PropLineTool.placementCalculator.angleMode) {
                        case PropLineTool.PlacementCalculator.AngleMode.Dynamic: {
                            PropLineTool.placementCalculator.angleOffset = Mathf.Round(PropLineTool.placementCalculator.angleOffset * Mathf.Rad2Deg) * Mathf.Deg2Rad;
                            break;
                        }
                        case PropLineTool.PlacementCalculator.AngleMode.Single: {
                            PropLineTool.placementCalculator.angleSingle = Mathf.Round(PropLineTool.placementCalculator.angleSingle * Mathf.Rad2Deg) * Mathf.Deg2Rad;
                            break;
                        }
                    }
                }
            };



        }

    }

    public class UIBasicSingleParameterPanel : UIPanel {
        //constants
        public static readonly Vector2 SIZE_PANEL_DEFAULT = new Vector2(374f, 140f);
        public static readonly Vector2 SIZE_MINI_CHECKBOX_LISTING = new Vector2(116f, 16f);
        public static readonly Vector2 SIZE_MINI_CHECKBOX_BOX = new Vector2(16f, 16f);
        public static readonly RectOffset PADDING_MINI_CHECKBOX_TEXT = new RectOffset(4, 4, 4, 0);

        //labels
        public UILabel labelTitle;
        //dropdown listing
        public UIDropDownListing dropDownListing;
        //numbox listing
        public UINumboxListing numboxListing;

        public override void Awake() {
            base.Awake();

            //labels
            labelTitle = UISimpleElems.CreateLabelWhite(this, "Section", 1.25f, UIHorizontalAlignment.Left, new RectOffset(5, 5, 5, 5), new Vector2(87f, 34f), new Vector3(12f, 2f));
            labelTitle.relativePosition = new Vector3(12f, 2f);
            //dropdown listing
            dropDownListing = this.AddUIComponent<UIDropDownListing>();
            dropDownListing.relativePosition = new Vector3(88f, 7f);
            //numbox listing
            numboxListing = this.AddUIComponent<UINumboxListing>();
            numboxListing.relativePosition = new Vector3(70f, 49f);
        }

        public virtual void SetVanillaAtlas(UITextureAtlas atlas) {
            dropDownListing.dropDown.atlas = atlas;
            numboxListing.numbox.atlas = atlas;
        }
    }

    public class UIBasicSpacingPanel : UIBasicSingleParameterPanel {
        //spacing calculator
        protected UIBasicSpacingCalculator spacingCalculator;

        //checkbox listings
        protected UICheckboxListing checkboxListingAutoDefault;

        //external events
        private void userParameters_SpacingSingleChanged(object o, float value) {
            numboxListing.numbox.value = value;
        }

        public override void Awake() {
            base.Awake();

            labelTitle.text = "Spacing";

            dropDownListing.label.text = "";
            dropDownListing.Disable();
            dropDownListing.Hide();

            numboxListing.labelText.text = "Spacing";
            numboxListing.labelUnits.text = "m";
            //numboxListing.numbox.value = 8f;
            numboxListing.numbox.value = PropLineTool.placementCalculator.spacingSingle;

            spacingCalculator = this.AddUIComponent<UIBasicSpacingCalculator>();
            spacingCalculator.relativePosition = new Vector3(26f, 96f);

            //checkbox listing
            checkboxListingAutoDefault = UISimpleElems.CreateCheckboxListing(this, 1, SIZE_MINI_CHECKBOX_LISTING, SIZE_MINI_CHECKBOX_BOX, SpriteManager.atlasPLT);
            checkboxListingAutoDefault.label.textScale = 0.75f;
            checkboxListingAutoDefault.label.padding = PADDING_MINI_CHECKBOX_TEXT;
            checkboxListingAutoDefault.label.text = "Auto-Default";
            checkboxListingAutoDefault.tooltip = "If enabled, automatically sets default spacing whenever a different prop/tree type is selected.\n\nSpacing will still auto-default when toggling fence mode.";
            //checkboxListingAutoDefault.isChecked = true;
            checkboxListingAutoDefault.isChecked = PropLineTool.userSettingsControlPanel.autoDefaultSpacing;
            checkboxListingAutoDefault.relativePosition = new Vector3(30f, 41f);

            //events
            //numbox
            numboxListing.numbox.eventValueChanged += delegate (UIComponent c, float value) {
                PropLineTool.placementCalculator.spacingSingle = value;
            };
            //checkbox listing
            checkboxListingAutoDefault.eventCheckChanged += delegate (UIComponent c, bool state) {
                PropLineTool.userSettingsControlPanel.autoDefaultSpacing = state;
            };
            //placement calc event subscriptions
            PropLineTool.placementCalculator.eventSpacingSingleChanged += userParameters_SpacingSingleChanged;
        }

        public override void OnDestroy() {
            //event unsubscriptions
            PropLineTool.placementCalculator.eventSpacingSingleChanged -= userParameters_SpacingSingleChanged;

            base.OnDestroy();
        }

        public override void SetVanillaAtlas(UITextureAtlas atlas) {
            base.SetVanillaAtlas(atlas);

            spacingCalculator.SetVanillaAtlas(atlas);
            //checkboxListingAutoDefault.checkbox.atlas = ResourceLoader.GetAtlas("Ingame");
            checkboxListingAutoDefault.checkbox.atlas = SpriteManager.vanillaAtlasIngame;
            checkboxListingAutoDefault.checkbox.SetVanillaToggleSprites("ToggleBase", "");
        }
    }

    public class UIBasicAnglePanel : UIBasicSingleParameterPanel {
        //angle calculator
        protected UIBasicAngleCalculator angleCalculator;

        //checkbox listings
        protected UICheckboxListing checkboxListingFlip180;

        //external events
        private void userParameters_AngleSingleChanged(object o, float value) {
            numboxListing.numbox.value = value * Mathf.Rad2Deg;
        }
        private void userParameters_AngleOffsetChanged(object o, float value) {
            numboxListing.numbox.value = value * Mathf.Rad2Deg;
        }
        private void userParameters_AngleModeChanged(object o, PropLineTool.PlacementCalculator.AngleMode mode) {
            switch (mode) {
                case PropLineTool.PlacementCalculator.AngleMode.Dynamic: {
                    numboxListing.numbox.value = PropLineTool.placementCalculator.angleOffset;
                    numboxListing.labelText.text = "Relative Angle";
                    numboxListing.labelUnits.text = "Δ°";
                    break;
                }
                case PropLineTool.PlacementCalculator.AngleMode.Single: {
                    numboxListing.numbox.value = PropLineTool.placementCalculator.angleSingle;
                    numboxListing.labelText.text = "Absolute Angle";
                    numboxListing.labelUnits.text = "°";
                    break;
                }
            }
        }
        private void PropLineTool_FenceModeChanged(bool state) {
            if (state == true) {
                dropDownListing.Disable();
                dropDownListing.tooltip = "Fence Mode must be OFF in order to change Angle Mode.";

                dropDownListing.dropDown.selectedIndex = 0;
            } else {
                dropDownListing.Enable();
                dropDownListing.tooltip = "";
            }
        }

        public override void Awake() {
            base.Awake();

            labelTitle.text = "Angle";

            dropDownListing.label.text = "Angle Mode";
            dropDownListing.SetDropDownItems(new string[2]
            {
                "Dynamic",
                "Single"
            });

            switch (PropLineTool.placementCalculator.angleMode) {
                case PropLineTool.PlacementCalculator.AngleMode.Dynamic: {
                    numboxListing.labelText.text = "Relative Angle";
                    numboxListing.labelUnits.text = "Δ°";
                    numboxListing.numbox.maxLength = 6;
                    //numboxListing.numbox.value = 0f;
                    numboxListing.numbox.value = PropLineTool.placementCalculator.angleOffset * Mathf.Rad2Deg;
                    break;
                }
                case PropLineTool.PlacementCalculator.AngleMode.Single: {
                    numboxListing.labelText.text = "Absolute Angle";
                    numboxListing.labelUnits.text = "°";
                    numboxListing.numbox.maxLength = 6;
                    //numboxListing.numbox.value = 0f;
                    numboxListing.numbox.value = PropLineTool.placementCalculator.angleSingle * Mathf.Rad2Deg;
                    break;
                }
                default: {
                    numboxListing.labelText.text = "Relative Angle";
                    numboxListing.labelUnits.text = "Δ°";
                    numboxListing.numbox.maxLength = 6;
                    //numboxListing.numbox.value = 0f;
                    numboxListing.numbox.value = PropLineTool.placementCalculator.angleOffset * Mathf.Rad2Deg;
                    break;
                }
            }

            angleCalculator = this.AddUIComponent<UIBasicAngleCalculator>();
            angleCalculator.relativePosition = new Vector3(26f, 96f);

            //checkbox listings
            checkboxListingFlip180 = UISimpleElems.CreateCheckboxListing(this, 1, SIZE_MINI_CHECKBOX_LISTING, SIZE_MINI_CHECKBOX_BOX, SpriteManager.atlasPLT);
            checkboxListingFlip180.label.textScale = 0.75f;
            checkboxListingFlip180.label.padding = PADDING_MINI_CHECKBOX_TEXT;
            checkboxListingFlip180.label.text = "Flip 180°";
            checkboxListingFlip180.tooltip = "Rotate props by 180° without rotating the change-angle handle in Lock Mode (green).";
            //checkboxListingFlip180.isChecked = false;
            checkboxListingFlip180.isChecked = PropLineTool.userSettingsControlPanel.angleFlip180;
            checkboxListingFlip180.relativePosition = new Vector3(30f, 41f);

            //events
            //numboxes
            numboxListing.numbox.eventValueChanged += delegate (UIComponent c, float value) {
                switch (PropLineTool.placementCalculator.angleMode) {
                    case PropLineTool.PlacementCalculator.AngleMode.Dynamic: {
                        PropLineTool.placementCalculator.angleOffset = value * Mathf.Deg2Rad;
                        break;
                    }
                    case PropLineTool.PlacementCalculator.AngleMode.Single: {
                        PropLineTool.placementCalculator.angleSingle = value * Mathf.Deg2Rad;
                        break;
                    }
                }
            };
            //dropdowns
            dropDownListing.dropDown.eventSelectedIndexChanged += delegate (UIComponent c, int index) {
                switch (index) {
                    case 0: {
                        PropLineTool.placementCalculator.angleMode = PropLineTool.PlacementCalculator.AngleMode.Dynamic;
                        break;
                    }
                    case 1: {
                        PropLineTool.placementCalculator.angleMode = PropLineTool.PlacementCalculator.AngleMode.Single;
                        break;
                    }
                }
            };
            //checkbox listing
            checkboxListingFlip180.eventCheckChanged += delegate (UIComponent c, bool state) {
                PropLineTool.userSettingsControlPanel.angleFlip180 = state;
            };
            //placement calc event subscriptions
            PropLineTool.placementCalculator.eventAngleSingleChanged += userParameters_AngleSingleChanged;
            PropLineTool.placementCalculator.eventAngleOffsetChanged += userParameters_AngleOffsetChanged;
            PropLineTool.placementCalculator.eventAngleModeChanged += userParameters_AngleModeChanged;
            //PropLineTool event subscriptions
            PropLineTool.eventFenceModeChanged += PropLineTool_FenceModeChanged;
        }

        public override void OnDestroy() {
            //event unsubscriptions
            PropLineTool.placementCalculator.eventAngleSingleChanged -= userParameters_AngleSingleChanged;
            PropLineTool.placementCalculator.eventAngleOffsetChanged -= userParameters_AngleOffsetChanged;
            PropLineTool.placementCalculator.eventAngleModeChanged -= userParameters_AngleModeChanged;
            PropLineTool.eventFenceModeChanged -= PropLineTool_FenceModeChanged;

            base.OnDestroy();
        }

        public override void SetVanillaAtlas(UITextureAtlas atlas) {
            base.SetVanillaAtlas(atlas);

            angleCalculator.SetVanillaAtlas(atlas);
            //checkboxListingFlip180.checkbox.atlas = ResourceLoader.GetAtlas("Ingame");
            checkboxListingFlip180.checkbox.atlas = SpriteManager.vanillaAtlasIngame;
            checkboxListingFlip180.checkbox.SetVanillaToggleSprites("ToggleBase", "");
        }
    }

    public abstract class UIPLTTabPanel : UIPanel {
        public abstract void SetVanillaAtlas(UITextureAtlas atlas);
    }

    public class UIBasicParametersPanel : UIPLTTabPanel {
        //constants
        public static readonly Vector2 SIZE_PANEL = new Vector2(374f, 408f);

        //controlMode buttons
        protected UIControlModeTabstrip controlModeTabstrip;

        //buttons
        protected UIButton buttonDecoupleFromPreviousSegment;
        //dividers
        protected UITiledSprite dividerOne;
        protected UITiledSprite dividerTwo;
        //parameter sections
        protected UIBasicSpacingPanel spacingPanel;
        protected UIBasicAnglePanel anglePanel;

        //external events
        private void userParameters_LastContinueParameterChanged() {
            //Debug.Log("[PLTDEBUG]: Start userParameters_LastContinueParameterChanged()");

            if (PropLineTool.placementCalculator.segmentState.AreLastContinueParametersZero()) {
                buttonDecoupleFromPreviousSegment.Hide();
            } else   //curve is coupled to previous segment
              {
                buttonDecoupleFromPreviousSegment.Show();
            }

            //Debug.Log("[PLTDEBUG]: End userParameters_LastContinueParameterChanged()");
        }
        private void PropLineTool_ObjectModeChanged(PropLineTool.ObjectMode mode) {
            switch (mode) {
                case PropLineTool.ObjectMode.Props: {
                    anglePanel.Show();
                    anglePanel.Enable();
                    break;
                }
                case PropLineTool.ObjectMode.Trees: {
                    anglePanel.Disable();
                    anglePanel.Hide();
                    break;
                }
            }
        }

        //awake
        public override void Awake() {
            base.Awake();

            this.size = SIZE_PANEL;

            //control mode buttons
            controlModeTabstrip = this.AddUIComponent<UIControlModeTabstrip>();
            controlModeTabstrip.relativePosition = new Vector3(70f, 7f);

            //buttons
            buttonDecoupleFromPreviousSegment = UISimpleElems.CreateBlueButton(this, "Decouple From Previous Segment", 0.80f, UIHorizontalAlignment.Center, UIVerticalAlignment.Middle, new RectOffset(6, 6, 2, 0), new Vector2(250f, 24f), new Vector3(62f, 25f));
            //buttonDecoupleFromPreviousSegment.relativePosition = new Vector3(62f, 25f);
            buttonDecoupleFromPreviousSegment.relativePosition = new Vector3(62f, 45f);
            buttonDecoupleFromPreviousSegment.Hide();
            //until fix for event to show/hide decouple button starts working again...
            buttonDecoupleFromPreviousSegment.Show();
            buttonDecoupleFromPreviousSegment.color = new Color32(255, 255, 255, 200);

            //dividers
            //one
            dividerOne = UISimpleElems.CreateDivider(this, SpriteManager.atlasPLT, "PLT_BasicDividerTile02x02", new Vector2(350f, 2f), new Vector3(12f, 115f));
            dividerOne.relativePosition = new Vector3(12f, 73f);
            //two
            dividerTwo = UISimpleElems.CreateDivider(this, SpriteManager.atlasPLT, "PLT_BasicDividerTile02x02", new Vector2(350f, 2f), new Vector3(12f, 274f));
            dividerTwo.relativePosition = new Vector3(12f, 232f);

            //parameter sections
            //spacing
            spacingPanel = this.AddUIComponent<UIBasicSpacingPanel>();
            spacingPanel.size = UIBasicSpacingPanel.SIZE_PANEL_DEFAULT;
            spacingPanel.relativePosition = new Vector3(0f, 83f);
            //angle
            anglePanel = this.AddUIComponent<UIBasicAnglePanel>();
            anglePanel.size = UIBasicAnglePanel.SIZE_PANEL_DEFAULT;
            anglePanel.relativePosition = new Vector3(0f, 242f);



            //events
            //buttons
            buttonDecoupleFromPreviousSegment.eventClick += delegate (UIComponent c, UIMouseEventParameter p) {
                PropLineTool.placementCalculator.ResetLastContinueParameters();
            };
            //placement calc event subscriptions
            PropLineTool.placementCalculator.segmentState.eventLastContinueParameterChanged += userParameters_LastContinueParameterChanged;
            //PropLineTool.eventLastContinueParameterChanged += userParameters_LastContinueParameterChanged;
            //PropLineTool.placementCalculator.eventLastContinueParameterChanged += userParameters_LastContinueParameterChanged;
            //PropLineTool event subscriptions
            PropLineTool.eventObjectModeChanged += PropLineTool_ObjectModeChanged;
        }

        public override void Start() {
            base.Start();

            //controlModeTabstrip.Disable();
            //controlModeTabstrip.Hide();
        }

        public override void OnDestroy() {
            //event unsubscriptions
            PropLineTool.placementCalculator.segmentState.eventLastContinueParameterChanged -= userParameters_LastContinueParameterChanged;
            //PropLineTool.eventLastContinueParameterChanged -= userParameters_LastContinueParameterChanged;
            //PropLineTool.placementCalculator.eventLastContinueParameterChanged -= userParameters_LastContinueParameterChanged;
            //PropLineTool event unsubscriptions
            PropLineTool.eventObjectModeChanged -= PropLineTool_ObjectModeChanged;

            base.OnDestroy();
        }

        //set vanilla atlas
        public override void SetVanillaAtlas(UITextureAtlas atlas) {
            spacingPanel.SetVanillaAtlas(atlas);
            anglePanel.SetVanillaAtlas(atlas);

            buttonDecoupleFromPreviousSegment.atlas = atlas;
        }
    }

    public class UIBasicOptionsPanel : UIPLTTabPanel {
        //constants
        public static readonly Vector2 SIZE_PANEL = new Vector2(374f, 408f);
        public static readonly Vector2 SIZE_CHECK_LISTING_TIER_1 = new Vector2(342f, 24f);
        public static readonly Vector2 SIZE_CHECKBOX_24px = new Vector2(24f, 24f);
        public static readonly Vector2 SIZE_CHECK_LISTING_TIER_2 = new Vector2(306f, 24f);
        public static readonly Vector2 SIZE_CHECK_LISTING_TIER_3 = new Vector2(278f, 24f);

        //checkbox listings
        //undo stuff
        public UICheckboxListing checkboxListingShowUndoPreviews;
        //error checking
        public UICheckboxListing checkboxListingErrorChecking;
        public UICheckboxListing checkboxListingShowErrorGuides;
        public UICheckboxListing checkboxListingPLTAnarchy;
        public UICheckboxListing checkboxListingPlaceBlockedItems;
        //item rendering and positioning
        public UICheckboxListing checkboxListingRenderPosResVanilla;
        public UICheckboxListing checkboxListingUseMeshCenterCorrection;
        /// <summary>
        /// Consider moving to the Parameters tab instead.
        /// </summary>
        public UICheckboxListing checkboxListingPerfectCircles;
        public UICheckboxListing checkboxListingLinearFenceFill;

        //dividers
        protected UITiledSprite dividerOne;
        protected UITiledSprite dividerTwo;

        //external events
        //  no need to update checkbox.isChecked, as these states can only be changed via the checkboxes
        private void userSettingsControlPanel_ErrorCheckingSettingChanged() {
            if (PropLineTool.userSettingsControlPanel.errorChecking == true) {
                checkboxListingErrorChecking.label.text = "Error Checking: Enabled";

                checkboxListingShowErrorGuides.Enable();
                checkboxListingPLTAnarchy.Enable();

                checkboxListingShowErrorGuides.tooltip = "Highlights items that are blocked by road/building (Yellow-Orange) and items that are invalid placement (Red).";
                checkboxListingPLTAnarchy.tooltip = "If Enabled, items are placed regardless of collision-errors.";

                if (PropLineTool.userSettingsControlPanel.showErrorGuides == true) {
                    //nothing here...
                } else  //error guides are hidden
                  {
                    //nothing here...
                }

                if (PropLineTool.userSettingsControlPanel.anarchyPLT == true) {
                    checkboxListingPlaceBlockedItems.Disable();
                    checkboxListingPlaceBlockedItems.tooltip = "PLT Anarchy must be OFF to access this feature.";

                    checkboxListingPLTAnarchy.label.text = "PLT Anarchy : ON";
                } else  //anarchy PLT is disabled
                  {
                    checkboxListingPlaceBlockedItems.Enable();
                    checkboxListingPlaceBlockedItems.tooltip = "If Enabled, items blocked by road/building (Yellow-Orange) will be placed, while invalid items (Red) will not.";

                    checkboxListingPLTAnarchy.label.text = "PLT Anarchy : OFF";

                    if (PropLineTool.userSettingsControlPanel.placeBlockedItems == true) {
                        //nothing here...
                    }
                }
            } else  //error checking is disabled
              {
                checkboxListingErrorChecking.label.text = "Error Checking: Disabled";

                checkboxListingShowErrorGuides.Disable();
                checkboxListingPLTAnarchy.Disable();
                checkboxListingPlaceBlockedItems.Disable();

                checkboxListingShowErrorGuides.tooltip = "Error Checking must be ENABLED to access this feature.";
                checkboxListingPLTAnarchy.tooltip = "Error Checking must be ENABLED to access this feature.";
                checkboxListingPlaceBlockedItems.tooltip = "Error Checking must be ENABLED to access this feature.";
            }
        }

        //awake
        public override void Awake() {
            base.Awake();

            Debug.Log("[PLT]: Begin UIBasicControlPanel.Awake()");

            this.size = SIZE_PANEL;

            //checkbox listings
            //show undo previews
            checkboxListingShowUndoPreviews = UISimpleElems.CreateCheckboxListing(this, 1, SIZE_CHECK_LISTING_TIER_1, SIZE_CHECKBOX_24px, SpriteManager.atlasPLT);
            checkboxListingShowUndoPreviews.label.text = "Show Undo Previews";
            checkboxListingShowUndoPreviews.tooltip = "Highlights which items will be deleted in the next Undo command when [Ctrl] key is held.";
            //checkboxListingShowUndoPreviews.isChecked = true;
            checkboxListingShowUndoPreviews.isChecked = PropLineTool.userSettingsControlPanel.showUndoPreviews;
            checkboxListingShowUndoPreviews.relativePosition = new Vector3(22f, 12f);

            //error checking
            //  base
            checkboxListingErrorChecking = UISimpleElems.CreateCheckboxListing(this, 1, SIZE_CHECK_LISTING_TIER_1, SIZE_CHECKBOX_24px, SpriteManager.atlasPLT);
            checkboxListingErrorChecking.label.text = PropLineTool.userSettingsControlPanel.errorChecking ? "Error Checking: Enabled" : "Error Checking: Disabled";
            checkboxListingErrorChecking.tooltip = "Toggles whether collision-error calculations are run. Disabling may improve performance.";
            //checkboxListingErrorChecking.isChecked = true;
            checkboxListingErrorChecking.isChecked = PropLineTool.userSettingsControlPanel.errorChecking;
            checkboxListingErrorChecking.relativePosition = new Vector3(22f, 88f);
            //  error guides
            checkboxListingShowErrorGuides = UISimpleElems.CreateCheckboxListing(this, 1, SIZE_CHECK_LISTING_TIER_2, SIZE_CHECKBOX_24px, SpriteManager.atlasPLT);
            checkboxListingShowErrorGuides.label.text = "Show Error Guides";
            checkboxListingShowErrorGuides.tooltip = "Highlights items that are blocked by road/building (Yellow-Orange) and items that are invalid placement (Red).";
            //checkboxListingShowErrorGuides.isChecked = true;
            checkboxListingShowErrorGuides.isChecked = PropLineTool.userSettingsControlPanel.showErrorGuides;
            checkboxListingShowErrorGuides.relativePosition = new Vector3(58f, 116f);
            //  anarchy PLT
            checkboxListingPLTAnarchy = UISimpleElems.CreateCheckboxListing(this, 1, SIZE_CHECK_LISTING_TIER_2, SIZE_CHECKBOX_24px, SpriteManager.atlasPLT);
            checkboxListingPLTAnarchy.label.text = PropLineTool.userSettingsControlPanel.anarchyPLT ? "PLT Anarchy : ON" : "PLT Anarchy : OFF";
            checkboxListingPLTAnarchy.tooltip = "If Enabled, items are placed regardless of collision-errors.";
            //checkboxListingPLTAnarchy.isChecked = false;
            checkboxListingPLTAnarchy.isChecked = PropLineTool.userSettingsControlPanel.anarchyPLT;
            checkboxListingPLTAnarchy.relativePosition = new Vector3(58f, 144f);
            //  place blocked items
            checkboxListingPlaceBlockedItems = UISimpleElems.CreateCheckboxListing(this, 1, SIZE_CHECK_LISTING_TIER_3, SIZE_CHECKBOX_24px, SpriteManager.atlasPLT);
            checkboxListingPlaceBlockedItems.label.text = "Place Blocked Items";
            checkboxListingPlaceBlockedItems.tooltip = "If Enabled, items blocked by road/building (Yellow-Orange) will be placed, while invalid items (Red) will not.";
            //checkboxListingPlaceBlockedItems.isChecked = false;
            checkboxListingPlaceBlockedItems.isChecked = PropLineTool.userSettingsControlPanel.placeBlockedItems;
            checkboxListingPlaceBlockedItems.relativePosition = new Vector3(94f, 172f);

            //item rendering
            //  vanilla prop grid
            checkboxListingRenderPosResVanilla = UISimpleElems.CreateCheckboxListing(this, 1, SIZE_CHECK_LISTING_TIER_1, SIZE_CHECKBOX_24px, SpriteManager.atlasPLT);
            checkboxListingRenderPosResVanilla.label.text = "Render & Place at Vanilla Resolution";
            checkboxListingRenderPosResVanilla.tooltip = "Legacy Option\nEnable to place items at the game's default positional-resolution, ~26.37 cm/item.\n\n(Use the PropPrecision mod to place at high-resolution).";
            //checkboxListingRenderPosResVanilla.isChecked = false;
            checkboxListingRenderPosResVanilla.isChecked = PropLineTool.userSettingsControlPanel.renderAndPlacePosResVanilla;
            checkboxListingRenderPosResVanilla.relativePosition = new Vector3(22f, 256f);
            //  mesh center correction
            checkboxListingUseMeshCenterCorrection = UISimpleElems.CreateCheckboxListing(this, 1, SIZE_CHECK_LISTING_TIER_1, SIZE_CHECKBOX_24px, SpriteManager.atlasPLT);
            checkboxListingUseMeshCenterCorrection.label.text = "Use Mesh for Center Correction";
            checkboxListingUseMeshCenterCorrection.tooltip = "Attempt to center items within their footprint.\n\nGood for props that are not centered within their bounds.";
            checkboxListingUseMeshCenterCorrection.isChecked = PropLineTool.userSettingsControlPanel.useMeshCenterCorrection;
            checkboxListingUseMeshCenterCorrection.relativePosition = new Vector3(22f, 284f);
            //  perfect circles
            checkboxListingPerfectCircles = UISimpleElems.CreateCheckboxListing(this, 1, SIZE_CHECK_LISTING_TIER_1, SIZE_CHECKBOX_24px, SpriteManager.atlasPLT);
            checkboxListingPerfectCircles.label.text = "Perfect Circles";
            checkboxListingPerfectCircles.tooltip = "If Enabled, circles snap to the nearest size that would perfectly fit all items on the curve.\n\nThe nearest size depends on the Spacing parameter.";
            //checkboxListingRenderPosResVanilla.isChecked = false;
            checkboxListingPerfectCircles.isChecked = PropLineTool.userSettingsControlPanel.perfectCircles;
            checkboxListingPerfectCircles.relativePosition = new Vector3(22f, 312f);
            //  linear fence fill
            checkboxListingLinearFenceFill = UISimpleElems.CreateCheckboxListing(this, 1, SIZE_CHECK_LISTING_TIER_1, SIZE_CHECKBOX_24px, SpriteManager.atlasPLT);
            checkboxListingLinearFenceFill.label.text = "Linear Fence Fill";
            checkboxListingLinearFenceFill.tooltip = "When drawing straight fences, overlaps the final piece to end at exact mouse position.";
            checkboxListingLinearFenceFill.isChecked = PropLineTool.userSettingsControlPanel.linearFenceFill;
            checkboxListingLinearFenceFill.relativePosition = new Vector3(22f, 340f);

            //dividers
            //one
            dividerOne = UISimpleElems.CreateDivider(this, SpriteManager.atlasPLT, "PLT_BasicDividerTile02x02", new Vector2(350f, 2f), new Vector3(12f, 115f));
            dividerOne.relativePosition = new Vector3(12f, 62f);
            //two
            dividerTwo = UISimpleElems.CreateDivider(this, SpriteManager.atlasPLT, "PLT_BasicDividerTile02x02", new Vector2(350f, 2f), new Vector3(12f, 274f));
            dividerTwo.relativePosition = new Vector3(12f, 225f);

            //update checkboxes
            userSettingsControlPanel_ErrorCheckingSettingChanged();

            //events
            //undo stuff
            checkboxListingShowUndoPreviews.eventCheckChanged += delegate (UIComponent c, bool state) {
                PropLineTool.userSettingsControlPanel.showUndoPreviews = state;
            };
            //error checking
            //  base
            checkboxListingErrorChecking.eventCheckChanged += delegate (UIComponent c, bool state) {
                PropLineTool.userSettingsControlPanel.errorChecking = state;
            };
            //  error guides
            checkboxListingShowErrorGuides.eventCheckChanged += delegate (UIComponent c, bool state) {
                PropLineTool.userSettingsControlPanel.showErrorGuides = state;
            };
            //  anarchy PLT
            checkboxListingPLTAnarchy.eventCheckChanged += delegate (UIComponent c, bool state) {
                PropLineTool.userSettingsControlPanel.anarchyPLT = state;
            };
            //  place blocked items
            checkboxListingPlaceBlockedItems.eventCheckChanged += delegate (UIComponent c, bool state) {
                PropLineTool.userSettingsControlPanel.placeBlockedItems = state;
            };
            //item rendering and positioning
            //  vanilla prop grid
            checkboxListingRenderPosResVanilla.eventCheckChanged += delegate (UIComponent c, bool state) {
                PropLineTool.userSettingsControlPanel.renderAndPlacePosResVanilla = state;
            };
            //  mesh center correction
            checkboxListingUseMeshCenterCorrection.eventCheckChanged += delegate (UIComponent c, bool state) {
                PropLineTool.userSettingsControlPanel.useMeshCenterCorrection = state;
            };
            //  perfect circles
            checkboxListingPerfectCircles.eventCheckChanged += delegate (UIComponent c, bool state) {
                PropLineTool.userSettingsControlPanel.perfectCircles = state;
            };
            //  linear fence fill
            checkboxListingLinearFenceFill.eventCheckChanged += delegate (UIComponent c, bool state) {
                PropLineTool.userSettingsControlPanel.linearFenceFill = state;
            };

            //user settings event subscription
            PropLineTool.userSettingsControlPanel.eventErrorCheckingSettingChanged += userSettingsControlPanel_ErrorCheckingSettingChanged;


            //finally
            if (SpriteManager.atlasPLT == null) {
                CheckAndRevertToVanillaSprites();
            }

            Debug.Log("[PLT]: End UIBasicControlPanel.Awake()");
        }

        public override void Start() {
            base.Start();

            //checkboxListingPerfectCircles.Disable();
            //checkboxListingPerfectCircles.Hide();
        }

        public override void OnDestroy() {
            //event unsubscriptions
            PropLineTool.userSettingsControlPanel.eventErrorCheckingSettingChanged -= userSettingsControlPanel_ErrorCheckingSettingChanged;

            base.OnDestroy();
        }

        //set vanilla atlas
        public override void SetVanillaAtlas(UITextureAtlas atlas) {
            //nothing so far...
        }

        public void CheckAndRevertToVanillaSprites() {
            if (SpriteManager.atlasPLT == null) {
                var _checkboxes = this.components;

                foreach (UIComponent _component in _checkboxes) {
                    if (_component is UICheckboxListing) {
                        UICheckboxListing _checkboxListing = _component as UICheckboxListing;

                        _checkboxListing.atlas = atlas;

                        _checkboxListing.checkbox.SetVanillaToggleSprites("ToggleBase", "");
                    }
                }
            }
        }
    }

    //BASIC CONTROL PANEL
    public class UIBasicControlPanel : UIPanel {
        //debug
        //PerformanceMeter _DEBUG_meterPanelUpdate = new PerformanceMeter("UIBasicControlPanel.Update");

        //constants
        public static readonly Vector2 CONTROL_PANEL_SIZE = new Vector2(374f, 450f);
        public const float TITLEBAR_HEIGHT = 42f;
        public const float TABSTRIP_HEIGHT = 32f;
        public const float PADDING_TABSTRIP_SIDES = 7f;
        public const float PADDING_PANEL = 10f;
        public static readonly RectOffset PADDING_TAB_BUTTON = new RectOffset(10, 10, 8, 8);

        //titlebar
        protected UIDragHandle titleBar;
        //tabstrip and tabPages
        protected UITabstrip tabstrip;
        protected Dictionary<string, UIPanel> tabPanels = new Dictionary<string, UIPanel>(5);
        protected Dictionary<string, UIPLTTabPanel> sectionPanels = new Dictionary<string, UIPLTTabPanel>(5);
        protected float localTabX = 0f;
        //labels
        protected UILabel labelMainTitle;

        //events ?

        public override void Awake() {
            base.Awake();

            //base panel
            atlas = SpriteManager.vanillaAtlasIngame;
            backgroundSprite = "MenuPanel2";
            size = CONTROL_PANEL_SIZE;

            //add tabs
            AddTabPage("Parameters");
            AddTabPage("Options");
            //set tabs to correct panels
            SetSectionPanel<UIBasicParametersPanel>("Parameters", UIBasicParametersPanel.SIZE_PANEL);
            SetSectionPanel<UIBasicOptionsPanel>("Options", UIBasicOptionsPanel.SIZE_PANEL);

            //tabstrip.FitChildren();

            //reset relative positions
            titleBar.relativePosition = new Vector3(0f, 0f);
            tabstrip.relativePosition = new Vector3(PADDING_TABSTRIP_SIDES, PADDING_PANEL);
            tabstrip.tabPages.relativePosition = new Vector3(0f, TITLEBAR_HEIGHT);

            //labels
            //main title
            labelMainTitle = UISimpleElems.CreateLabelWhite(this, "PLT vAlpha", 1f, UIHorizontalAlignment.Right, new RectOffset(2, 7, 2, 5), new Vector2(180f, 30f), new Vector3(187f, 7f));
            labelMainTitle.textColor = new Color32(164, 164, 164, 255);
            labelMainTitle.disabledTextColor = new Color32(82, 82, 82, 255);
            labelMainTitle.relativePosition = new Vector3(187f, 7f);


            //penultimately
            ICities.LoadMode _loadMode = PropLineToolMod.GetLoadMode();
            //if ((_loadMode == ICities.LoadMode.LoadMap) || (_loadMode == ICities.LoadMode.LoadAsset) || (_loadMode == ICities.LoadMode.NewAsset) || (_loadMode == ICities.LoadMode.NewMap))
            if (!_loadMode.IsMainGameplay()) {
                //UITextureAtlas _ingameAtlas = GameObject.Find("SnappingToggle").GetComponent<UIMultiStateButton>().atlas;
                //UITextureAtlas _ingameAtlas = ResourceLoader.GetAtlas("Ingame");
                UITextureAtlas _ingameAtlas = SpriteManager.vanillaAtlasIngame;

                if (_ingameAtlas == null) {
                    Debug.LogError("[PLT]: UIBasicControlPanel.Awake(): Could not find Ingame atlas!");
                    _ingameAtlas = UIView.GetAView().defaultAtlas;
                } else {
                    SetAllSectionsVanillaAtlas(_ingameAtlas);
                    Debug.Log("[PLT]: UIBasicControlPanel.Awake(): Vanilla atlas set from Ingame atlas.");
                }
            }

            //ensure first tab is visible
            tabPanels["Parameters"].isVisible = true;

            ////check for scenario editor
            //CheckAndFixScenarioEditorAtlas();

            //finally
            titleBar.BringToFront();
        }


        public override void Start() {
            base.Start();

            absolutePosition = new Vector3(Mathf.Floor(base.GetUIView().GetScreenResolution().x - base.width - 50f), Mathf.Floor(base.GetUIView().GetScreenResolution().y - base.height - 300f));

            this.Hide();
        }

        public UIPanel AddTabPage(string tabName) {
            if (titleBar == null) {
                titleBar = this.AddUIComponent<UIDragHandle>();
                titleBar.width = this.width;
                titleBar.height = TITLEBAR_HEIGHT;
                titleBar.target = this;
                titleBar.relativePosition = new Vector3(0f, 0f);
            }

            if (tabstrip == null) {
                tabstrip = titleBar.AddUIComponent<UITabstrip>();
                tabstrip.width = this.width - (2f * PADDING_TABSTRIP_SIDES);
                tabstrip.height = TABSTRIP_HEIGHT;
                tabstrip.padding = new RectOffset(0, 0, 0, 0);
                tabstrip.relativePosition = new Vector3(PADDING_TABSTRIP_SIDES, PADDING_PANEL);
            }

            if (tabstrip.tabPages == null) {
                UITabContainer _tabContainer = this.AddUIComponent<UITabContainer>();
                _tabContainer.size = new Vector2(CONTROL_PANEL_SIZE.x, CONTROL_PANEL_SIZE.y - TITLEBAR_HEIGHT);
                _tabContainer.relativePosition = new Vector3(0f, TITLEBAR_HEIGHT);

                tabstrip.tabPages = _tabContainer;
            }

            if (!tabPanels.ContainsKey(tabName)) {
                //post-ND: this tabstrip no longer exists
                //UIButton _buttonTemplate = GameObject.Find("KeyMappingTabStrip").GetComponent<UITabstrip>().GetComponentInChildren<UIButton>();

                //UIButton _tab = tabstrip.AddTab(tabName, _buttonTemplate, true);
                UIButton _tab = tabstrip.AddTab(tabName);
                //_tab.atlas = ResourceLoader.GetAtlas("Ingame");
                _tab.atlas = SpriteManager.vanillaAtlasIngame;
                _tab.normalBgSprite = "GenericTab";
                _tab.focusedBgSprite = "GenericTabFocused";
                _tab.hoveredBgSprite = "GenericTabHovered";
                _tab.pressedBgSprite = "GenericTabPressed";
                _tab.disabledBgSprite = "GenericTabDisabled";
                _tab.textPadding = PADDING_TAB_BUTTON;
                _tab.autoSize = true;
                _tab.textScale = 0.9f;
                _tab.playAudioEvents = true;
                _tab.pressedTextColor = new Color32(255, 255, 255, 255);
                _tab.focusedTextColor = new Color32(230, 230, 230, 255);
                _tab.focusedColor = new Color32(205, 220, 255, 255);
                //_tab.disabledTextColor = _buttonTemplate.disabledTextColor;
                _tab.disabledTextColor = new Color32(230, 230, 230, 140);

                UIPanel _panel = tabstrip.tabPages.components.Last<UIComponent>() as UIPanel;
                _panel.autoLayoutDirection = LayoutDirection.Vertical;
                _panel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
                _panel.isVisible = false;
                tabPanels.Add(tabName, _panel);

                return _panel;
            } else {
                return tabPanels[tabName];
            }
        }

        public UIPanel GetTabPage(string tabName) {
            if (tabPanels.ContainsKey(tabName)) {
                return tabPanels[tabName];
            } else {
                return AddTabPage(tabName);
            }
        }

        public bool SetSectionPanel<TPanel>(string tabName, Vector2 sectionSize) where TPanel : UIPLTTabPanel {
            if (tabPanels.ContainsKey(tabName)) {
                if (!sectionPanels.ContainsKey(tabName)) {
                    UIPanel _tabPage = GetTabPage(tabName);

                    TPanel _newPanel = _tabPage.AddUIComponent<TPanel>();

                    _newPanel.relativePosition = new Vector3(0f, 0f);
                    _newPanel.size = sectionSize;

                    sectionPanels.Add(tabName, _newPanel);

                    return true;
                } else {
                    return false;
                }
            } else {
                return false;
            }
        }

        public void SetAllSectionsVanillaAtlas(UITextureAtlas atlas) {
            var _keyList = sectionPanels.Keys;
            foreach (var _key in _keyList) {
                sectionPanels[_key].SetVanillaAtlas(atlas);
            }
        }

        public override void Update() {
            //_DEBUG_meterPanelUpdate.FrameStart();
            base.Update();
            //_DEBUG_meterPanelUpdate.FrameEnd();
        }
    }

}