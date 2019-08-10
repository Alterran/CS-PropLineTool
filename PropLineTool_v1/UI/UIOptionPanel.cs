using ColossalFramework.UI;
using PropLineTool.UI.Elements;
using PropLineTool.Utility;
using PropLineTool.Sprites;
using UnityEngine;

//debug only
//using PropLineTool.DebugUtils;

//Reference: NetworkSkins.UI.UINetworkSkinsPanel
//Thanks to boformer's Network Skins for providing the template for this class


//Much help from: BloodyPenguin's NaturalResourcesBrush.ToolbarButtonSpawner.SpawnSubEntry
//Without which I could not figure out how to use the game's built-in sprites

namespace PropLineTool.UI.OptionPanel {
    public class UIOptionPanel : UIPanel {
        //DEBUG Testing for FPS hit
        //private PerformanceMeter _DEBUG_meterToolSwitch = new PerformanceMeter("ToolSwitch.SwitchTools", "UIOptionPanel.Update");
        //private PerformanceMeter _DEBUG_meterPanelUpdate = new PerformanceMeter("UIOptionPanel.Update", "UIOptionPanel itself");

        public const int PaddingTop = 9;

        public const int Padding = 7;

        public const int PagePadding = 10;

        public const int TabHeight = 32;

        public const int PageHeight = 500;

        public const int Width = 360;

        public static readonly Vector3 SIZE_DEFAULT_INGAME = new Vector3(256f, 36f);
        public static readonly Vector3 SIZE_DEFAULT_EDITORS = new Vector3(266f, 46f);
        public static readonly Vector3 OFFSET_HOLDERPANEL_EDITORS = new Vector3(5f, 5f);

        public PropLineTool.SnapMode[] m_snapModes = new PropLineTool.SnapMode[]
        {
            PropLineTool.SnapMode.Off,
            PropLineTool.SnapMode.Objects,
            PropLineTool.SnapMode.ZoneLines
        };

        public PropLineTool.DrawMode[] m_drawModes = new PropLineTool.DrawMode[]
        {
            PropLineTool.DrawMode.Single,
            PropLineTool.DrawMode.Straight,
            PropLineTool.DrawMode.Curved,
            PropLineTool.DrawMode.Freeform,
            PropLineTool.DrawMode.Circle
        };

        private UIPanel _panel;

        private UIMultiStateButton _snappingToggle;

        //private UICheckBox _fenceModeToggle;
        private UIMultiStateButton _fenceModeToggle;

        private UITabstrip _tabstrip;

        //private UICheckBox _controlPanelToggle;
        internal UIMultiStateButton _controlPanelToggle;

        //private readonly UIPanel[] _specificSettingPages;

        public static readonly string[] TOOL_MODE_NAMES = new string[]
        {
            "Single/Default",
            "Straight",
            "Curved",
            "Freeform",
            "Circle"
        };

        //external events
        private void PropLineTool_ActiveStateChanged(PropLineTool.ActiveState activeState) {
            switch (activeState) {
                case PropLineTool.ActiveState.CreatePointFirst:
                case PropLineTool.ActiveState.CreatePointSecond:
                case PropLineTool.ActiveState.CreatePointThird: {
                    AllDrawModeButtonsEnabler(true);
                    break;
                }
                case PropLineTool.ActiveState.LockIdle:
                case PropLineTool.ActiveState.ItemwiseLock: {
                    switch (PropLineTool.drawMode) {
                        case PropLineTool.DrawMode.Straight:
                        case PropLineTool.DrawMode.Circle: {
                            LockModeButtonEnabler(PropLineTool.DrawMode.Straight, true);
                            LockModeButtonEnabler(PropLineTool.DrawMode.Curved, false);
                            LockModeButtonEnabler(PropLineTool.DrawMode.Freeform, false);
                            LockModeButtonEnabler(PropLineTool.DrawMode.Circle, true);
                            break;
                        }
                        case PropLineTool.DrawMode.Curved: {
                            LockModeButtonEnabler(PropLineTool.DrawMode.Straight, false);
                            LockModeButtonEnabler(PropLineTool.DrawMode.Curved, true);
                            LockModeButtonEnabler(PropLineTool.DrawMode.Freeform, true);
                            LockModeButtonEnabler(PropLineTool.DrawMode.Circle, false);
                            break;
                        }
                        case PropLineTool.DrawMode.Freeform: {
                            LockModeButtonEnabler(PropLineTool.DrawMode.Straight, false);
                            LockModeButtonEnabler(PropLineTool.DrawMode.Curved, true);
                            LockModeButtonEnabler(PropLineTool.DrawMode.Freeform, true);
                            LockModeButtonEnabler(PropLineTool.DrawMode.Circle, false);
                            break;
                        }
                        default: {
                            //do nothing...
                            break;
                        }
                    }
                    break;
                }
                //case PropLineTool.ActiveState.MovePointFirst:
                //case PropLineTool.ActiveState.MovePointSecond:
                //case PropLineTool.ActiveState.MovePointThird:
                //case PropLineTool.ActiveState.MoveSegment:
                //case PropLineTool.ActiveState.ChangeSpacing:
                //case PropLineTool.ActiveState.ChangeAngle:
                //    {

                //        break;
                //    }
                default: {
                    //do nothing
                    break;
                }
            }
        }

        public override void Awake() {
            base.Awake();

            Debug.Log("[PLT] begin UIOptionPanel.Awake()");

            //old as of 161129
            //Util.LoadSprites();
            //new as of 161129
            SpriteManager.CreateAtlasPLT();

            //setup panel
            //base.width = 256;
            //base.height = 36;
            base.size = SIZE_DEFAULT_INGAME;
            this._panel = base.AddUIComponent<UIPanel>();
            this._panel.name = "HolderPanel";
            this._panel.width = base.width;
            this._panel.height = base.height;
            this._panel.relativePosition = new Vector3(0f, 0f);

            //setup snapping toggle
            this._snappingToggle = UISimpleElems.AddAThreeStateButton(this._panel, "PLT_ToggleSnapping", SpriteManager.atlasPLT, "PLT_MultiStateZero", "PLT_MultiStateOne", "PLT_MultiStateTwo", "PLT_SnappingModeZero", "PLT_SnappingModeOne", "PLT_SnappingModeTwo");
            this._snappingToggle.height = 36f;
            this._snappingToggle.width = 36f;
            this._snappingToggle.relativePosition = new Vector3(0f, 0f, 0f);
            this._snappingToggle.playAudioEvents = true;
            this._snappingToggle.tooltip = "[PLT]: Toggle Snapping";
            if (this._snappingToggle != null) {
                this._snappingToggle.eventActiveStateIndexChanged += delegate (UIComponent sender, int index) {
                    PropLineTool.m_snapMode = this.m_snapModes[index];
                };
            }

            //setup fenceMode toggle
            this._fenceModeToggle = UISimpleElems.AddAToggleButton(this._panel, "PLTToggleFenceMode", SpriteManager.atlasPLT, "PLT_MultiStateZero", "PLT_MultiStateOne", "PLT_FenceModeZero", "PLT_FenceModeOne");
            this._fenceModeToggle.height = 36f;
            this._fenceModeToggle.width = 36f;
            //this._fenceModeToggle.relativePosition = new Vector3(36f, 0f, 0f);
            //new for circle draw mode
            this._fenceModeToggle.relativePosition = new Vector3(0f, 0f, 0f);
            this._fenceModeToggle.playAudioEvents = true;
            this._fenceModeToggle.tooltip = "[PLT]: Toggle Fence Mode";
            if (this._fenceModeToggle != null) {
                this._fenceModeToggle.eventActiveStateIndexChanged += delegate (UIComponent sender, int index) {
                    bool _fenceModeActive = false;
                    if (index >= 1) {
                        _fenceModeActive = true;
                    } else if (index <= 0) {
                        _fenceModeActive = false;
                    }
                    PropLineTool.fenceMode = _fenceModeActive;
                };
            }

            //setup tabstrip
            this._tabstrip = this._panel.AddUIComponent<UITabstrip>();
            //this._tabstrip.relativePosition = new Vector3(72f, 0f, 0f);
            //this._tabstrip.width = 144f;
            //new for circle draw mode
            this._tabstrip.relativePosition = new Vector3(36f, 0f, 0f);
            this._tabstrip.width = 180f;
            this._tabstrip.height = 36f;
            this._tabstrip.padding.right = 0;
            UIButton componentInChildren = GameObject.Find("ToolMode").GetComponent<UITabstrip>().GetComponentInChildren<UIButton>();
            UITextureAtlas atlas = UIView.GetAView().defaultAtlas;
            for (int i = 0; i < TOOL_MODE_NAMES.Length; i++) {
                UIButton _button = this._tabstrip.AddTab(TOOL_MODE_NAMES[i], componentInChildren, false);
                _button.autoSize = false;

                _button.tooltip = "[PLT]: " + TOOL_MODE_NAMES[i];

                _button.height = 36f;
                _button.width = 36f;

                if (i == 0 || i == 4) {
                    _button.name = i == 4 ? "PLTCircle" : "PLTSingle";

                    _button.normalFgSprite = "";
                    _button.focusedFgSprite = "";
                    _button.hoveredFgSprite = "";
                    _button.pressedFgSprite = "";
                    _button.disabledFgSprite = "";

                    _button.text = i == 4 ? "○" : "•";
                    _button.textScale = i == 4 ? 3.0f : 1.5f;
                    _button.textPadding.left = i == 4 ? -2 : 0;
                    _button.textPadding.right = 1;
                    _button.textPadding.top = i == 4 ? -13 : 4;
                    //_button.textPadding.bottom = i == 4? 2 : 0;
                    _button.textPadding.bottom = 0;
                    _button.textColor = new Color32(119, 124, 126, 255);
                    _button.hoveredTextColor = new Color32(110, 113, 114, 255);
                    _button.pressedTextColor = new Color32(172, 175, 176, 255);
                    _button.focusedTextColor = new Color32(187, 224, 235, 255);
                    _button.disabledTextColor = new Color32(66, 69, 70, 255);
                }


                if (i > 0 && i < 4) {
                    _button.name = "PLT" + TOOL_MODE_NAMES[i];

                    string str = "RoadOption" + TOOL_MODE_NAMES[i];
                    _button.normalFgSprite = str;
                    _button.focusedFgSprite = str + "Focused";
                    _button.hoveredFgSprite = str + "Hovered";
                    _button.pressedFgSprite = str + "Pressed";
                    _button.disabledFgSprite = str + "Disabled";
                }

                _button.playAudioEvents = componentInChildren.playAudioEvents;

            }
            //setup selected index in case of re-Load from in-game
            this._tabstrip.selectedIndex = (int)PropLineTool.drawMode;
            if (this._tabstrip != null) {
                this._tabstrip.eventSelectedIndexChanged += delegate (UIComponent sender, int index) {
                    PropLineTool.drawMode = this.m_drawModes[index];
                };
            }

            //setup controlPanel toggle
            this._controlPanelToggle = UISimpleElems.AddAToggleButton(this._panel, "PLTToggleControlPanel", SpriteManager.atlasPLT, "PLT_ToggleCPZero", "PLT_ToggleCPOne", "", "");
            this._controlPanelToggle.height = 36f;
            this._controlPanelToggle.width = 36f;
            this._controlPanelToggle.relativePosition = new Vector3(216f, 0f, 0f);
            this._controlPanelToggle.playAudioEvents = true;
            this._controlPanelToggle.tooltip = "[PLT]: Toggle Control Panel";
            if (this._controlPanelToggle != null) {
                this._controlPanelToggle.eventActiveStateIndexChanged += delegate (UIComponent sender, int index) {
                    if (index >= 1) {
                        PropLineToolMod.basicControlPanel.Show();
                    } else if (index <= 0) {
                        PropLineToolMod.basicControlPanel.Hide();
                    }
                };
            }

            base.FitChildren();

            //this is only important to set to zero if we call it after the _tabstrip.eventSelectedIndexChanged event subscription
            //***VERY IMPORTANT THAT THIS EQUALS ZERO***
            //so that TreeTool or PropTool can initialize before PropLineTool
            this._tabstrip.startSelectedIndex = 0;


            //fix sprites if PLTAtlas could not be created
            if (SpriteManager.atlasPLT == null) {
                RevertToBackupToggleButtons();
            }

            //event subscriptions
            PropLineTool.eventActiveStateChanged += PropLineTool_ActiveStateChanged;

            Debug.Log("[PLT] end UIOptionPanel.Awake()");
        }

        public override void OnDestroy() {
            //event unsubscriptions
            PropLineTool.eventActiveStateChanged -= PropLineTool_ActiveStateChanged;

            base.OnDestroy();
        }


        public override void Start() {
            //initialize ToolSwitch
            ToolSwitch.PLTToolSwitch.Initialize();

            // setup intial position
            UIComponent optionsBar = GameObject.Find("OptionsBar").GetComponent<UIComponent>();
            if (optionsBar == null) {
                Debug.LogError("[PLT]: OptionsBar not found!");
                base.absolutePosition = new Vector3(261f, 542f);
            } else {
                //this.absolutePosition = optionsBar.absolutePosition;
                base.absolutePosition = optionsBar.absolutePosition;
                //re-center
                float widthDifference = base.width - optionsBar.width;
                if (widthDifference != 0f) {
                    float absX = base.absolutePosition.x;
                    float absY = base.absolutePosition.y;
                    float newX = (float)Mathf.RoundToInt(absX - (widthDifference / 2));
                    if (newX < 0) {
                        newX = 0;
                    }
                    float newY = absY + optionsBar.height - base.height - 6f;

                    //in map or asset editor
                    ICities.LoadMode _loadMode = PropLineToolMod.GetLoadMode();
                    //if (_loadMode != ICities.LoadMode.LoadGame && _loadMode != ICities.LoadMode.NewGame)
                    if (!_loadMode.IsMainGameplay()) {
                        base.size = SIZE_DEFAULT_EDITORS;
                        base.backgroundSprite = "GenericPanel";
                        base.color = new Color32(91, 97, 106, 255);
                        this._panel.relativePosition = OFFSET_HOLDERPANEL_EDITORS;
                    }

                    base.absolutePosition = new Vector3(newX, newY);
                }
            }

            //until snapping is setup
            _snappingToggle.Disable();
            _snappingToggle.Hide();

            //_tabstrip.DisableTab(4);
            //_tabstrip.HideTab(TOOL_MODE_NAMES[4]);
        }

        public override void Update() {
            //_DEBUG_meterPanelUpdate.FrameStart();
            UpdateImpl();
            //_DEBUG_meterPanelUpdate.FrameEnd();
        }
        public void UpdateImpl() {
            base.Update();

            //DEBUG
            //_DEBUG_meterToolSwitch.FrameStart();
            //DEBUG

            //mmmmm-magic!
            ToolSwitch.PLTToolSwitch.SwitchTools(out bool _allThreeToolsNull);

            //DEBUG
            //_DEBUG_meterToolSwitch.FrameEnd();
            //DEBUG

            if (_allThreeToolsNull) {
                //hide option panel
                if (base.isVisible) {
                    base.isVisible = false;

                    _controlPanelToggle.activeStateIndex = 0;
                }
            } else {
                //show option panel
                base.isVisible = true;
                if (PropLineTool.drawMode == PropLineTool.DrawMode.Single) {
                    _snappingToggle.isVisible = false;
                    _fenceModeToggle.isVisible = false;
                    _controlPanelToggle.isVisible = false;

                    _controlPanelToggle.activeStateIndex = 0;
                } else {
                    //_snappingToggle.isVisible = true;
                    _fenceModeToggle.isVisible = true;
                    _controlPanelToggle.isVisible = true;
                }
            }

        }

        private void LockModeButtonEnabler(PropLineTool.DrawMode drawModeButton, bool isEnabled) {
            int _index = 0;
            switch (drawModeButton) {
                case PropLineTool.DrawMode.Single: {
                    _index = 0;
                    break;
                }
                case PropLineTool.DrawMode.Straight: {
                    _index = 1;
                    break;
                }
                case PropLineTool.DrawMode.Curved: {
                    _index = 2;
                    break;
                }
                case PropLineTool.DrawMode.Freeform: {
                    _index = 3;
                    break;
                }
                case PropLineTool.DrawMode.Circle: {
                    _index = 4;
                    break;
                }
            }

            //if(_index == 4)
            //{
            //    return;
            //}

            if (isEnabled) {
                _tabstrip.EnableTab(_index);
            } else {
                _tabstrip.DisableTab(_index);
            }
        }

        private void AllDrawModeButtonsEnabler(bool isEnabled) {
            if (isEnabled) {
                //_tabstrip.EnableTab(0);
                //_tabstrip.EnableTab(1);
                //_tabstrip.EnableTab(2);
                //_tabstrip.EnableTab(3);

                for (int i = 0; i < _tabstrip.tabCount; i++) {
                    _tabstrip.EnableTab(i);
                }
            } else {
                //_tabstrip.DisableTab(0);
                //_tabstrip.DisableTab(1);
                //_tabstrip.DisableTab(2);
                //_tabstrip.DisableTab(3);

                for (int i = 0; i < _tabstrip.tabCount; i++) {
                    _tabstrip.DisableTab(i);
                }
            }
        }


        private void RevertToBackupToggleButtons() {
            UITextureAtlas _vanillaAtlas = GameObject.Find("SnappingToggle").GetComponent<UIMultiStateButton>().atlas;

            if (_vanillaAtlas != null) {
                //fence mode
                _fenceModeToggle.atlas = atlas;

                _fenceModeToggle.SetVanillaToggleSprites("ToggleBase", "IconPolicyProHipster");


                //control panel
                _controlPanelToggle.atlas = atlas;

                _controlPanelToggle.SetVanillaToggleSprites("InfoIconMaintenance", "");

                //_controlPanelToggle.text = "CP";
                //_controlPanelToggle.textPadding = new RectOffset(4, 4, 4, 4);
                //_controlPanelToggle.textColor = new Color32(255, 255, 255, 200);
            }
        }
    }
}
