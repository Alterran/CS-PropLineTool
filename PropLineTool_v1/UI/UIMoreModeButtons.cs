using System;
using ColossalFramework.UI;
using PropLineTool.Parameters;
using PropLineTool.UI.Elements;
using PropLineTool.Utility;
using PropLineTool.Sprites;
using UnityEngine;

namespace PropLineTool.UI.ModeButtons {
    //place at top of control panel for now
    public class UIControlModeTabstrip : UIPanel {
        private UITabstrip m_tabstrip;
        public UITabstrip tabstrip {
            get {
                return m_tabstrip;
            }
            set {
                m_tabstrip = value;
            }
        }

        public static readonly Vector3 DEFAULT_BUTTON_SIZE = new Vector3(80f, 30f);
        public static readonly Vector3 DEFAULT_TABSTRIP_SIZE = new Vector3(175f, 30f);
        public static readonly Vector3 DEFAULT_TAB_SPACING = new Vector3(5f, 0f);
        public static readonly RectOffset DEFAULT_TAB_PADDING = new RectOffset(0, 5, 0, 0);

        public override void Awake() {
            base.Awake();

            //setup tabstrip
            this.size = DEFAULT_TABSTRIP_SIZE;
            this.tabstrip = base.AddUIComponent<UITabstrip>();
            this.tabstrip.size = DEFAULT_TABSTRIP_SIZE;
            this.tabstrip.relativePosition = new Vector3(0f, 0f);
            this.tabstrip.padding = DEFAULT_TAB_PADDING;

            //setup first button
            UIButton _buttonItemwise = tabstrip.AddTab();
            _buttonItemwise.autoSize = false;
            _buttonItemwise.playAudioEvents = true;
            UISimpleElems.ModifyToCustomButton(ref _buttonItemwise, DEFAULT_BUTTON_SIZE, new Vector3(0f, 0f), "PLT_ItemwiseZero", "", SpriteManager.atlasPLT);
            //setup second button
            UIButton _buttonSpacing = tabstrip.AddTab("Spacing", _buttonItemwise, false);
            UISimpleElems.ModifyToCustomButton(ref _buttonSpacing, DEFAULT_BUTTON_SIZE, DEFAULT_BUTTON_SIZE + DEFAULT_TAB_SPACING, "PLT_SpacingwiseZero", "", SpriteManager.atlasPLT);
            //finalize buttons
            _buttonItemwise.focusedBgSprite = "PLT_ItemwiseOneFocused";
            _buttonItemwise.tooltip = "[PLT]: Itemwise Control\n\nPlace one item at a time along the curve.";
            _buttonSpacing.focusedBgSprite = "PLT_SpacingwiseOneFocused";
            _buttonSpacing.tooltip = "[PLT]: Spacingwise Control (Default)\n\nPlace items at discrete intervals.";

            //penultimately
            tabstrip.startSelectedIndex = (int)PropLineTool.controlMode;
            tabstrip.selectedIndex = (int)PropLineTool.controlMode;

            //finally
            //event subscriptions
            this.tabstrip.eventSelectedIndexChanged += delegate (UIComponent c, int index) {
                PropLineTool.controlMode = (PropLineTool.ControlMode)index;
            };
        }
    }
}
