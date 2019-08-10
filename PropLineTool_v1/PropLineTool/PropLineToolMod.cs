using ICities;
using ColossalFramework.UI;
using PropLineTool.Settings;
using PropLineTool.UI.OptionPanel;
using PropLineTool.UI.ControlPanel;
using PropLineTool.Utility;
using UnityEngine;
using ColossalFramework;

using System;

namespace PropLineTool {
    public class PropLineToolMod : LoadingExtensionBase, IUserMod {
        private static ICities.LoadMode m_loadMode;

        private static int m_onLevelLoadedCount = 0;

        //TODO: Keep this up to date.
        //Cannot automate using BuildDate.txt Resource as it does not reflect properly.
        public const string BUILD_VERSION = "190809";

        public static ICities.LoadMode GetLoadMode() {
            ICities.LoadMode mode = m_loadMode;
            return mode;
        }

        public static UIOptionPanel optionPanel;

        public static UIBasicControlPanel basicControlPanel;

        public string Name => "Prop Line Tool";

        public string Description => "Place props and trees along curves. Also: fences!";

        //Detour Deploy code here
        public override void OnCreated(ILoading loading) {
            base.OnCreated(loading);

            //no detours
        }

        //Detour Revert code here
        public override void OnReleased() {
            base.OnReleased();

            //no detours
        }

        public override void OnLevelLoaded(LoadMode mode) {
            base.OnLevelLoaded(mode);
            Debug.Log("[PropLineTool, hereafter PLT]: start PropLineToolMod.OnLevelLoaded");
            m_loadMode = mode;
            m_onLevelLoadedCount++;

            Debug.Log("[PLT]: Build Version: " + BUILD_VERSION);

            string _ordinal = Util.OrdinalSuffix(m_onLevelLoadedCount);

            Debug.Log("[PLT]: PropLineToolMod.OnLevelLoaded(): This is the " + m_onLevelLoadedCount + _ordinal + " time this method has been called, and the LoadMode is " + mode.ToString() + ".");
            Debug.Log("[PLT]: PropLineToolMod.OnLevelLoaded(): ItemClass.Availabilty of TMC tool controller is " + ToolsModifierControl.toolController.m_mode.ToString() + ".");

            bool _initializeToolMan = false;
            _initializeToolMan = ToolMan.ToolMan.Initialize();

            //PropLineTool Initialization
            PropLineTool.PopulateRandIntArray(0, 10000);

            if (_initializeToolMan == true) {
                Debug.Log("[PLT]: PropLineToolMod.OnLevelLoaded(): ToolMan.Initialize() returned true.");
                Debug.Log("[PLT]: PropLineToolMod.OnLevelLoaded(): Adding/Initializing UI components...");

                if (optionPanel == null) {
                    optionPanel = (UIView.GetAView().AddUIComponent(typeof(UIOptionPanel)) as UIOptionPanel);
                }

                if (basicControlPanel == null) {
                    basicControlPanel = (UIView.GetAView().AddUIComponent(typeof(UIBasicControlPanel)) as UIBasicControlPanel);
                }

                Debug.Log("[PLT]: PropLineToolMod.OnLevelLoaded(): UI components addition/initialization finished.");

                //debug purposes only
                //UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("[DEBUG] Prop Line Tool [PLT] Success in Initialization", "Prop Line Tool succeeded in registering itself with the game's tool controllers!", false);
            } else {
                Debug.LogError("[PLT]: PropLineToolMod.OnLevelLoaded(): ToolMan.Initialize() returned false.");

                //special thanks to RushHour.Compatibility for this
                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Prop Line Tool [PLT] Failed to Initialize", "Prop Line Tool failed to register itself with the game's tool controllers. Please save your output_log from this game session and post a link to it on the workshop page for PLT.", true);
            }

            Debug.Log("[PLT]: end PropLineToolMod.OnLevelLoaded");
        }

        public override void OnLevelUnloading() {
            base.OnLevelUnloading();

            PropLineTool.OnLevelUnloading();

            //new as of 160816 0041
            //in reference to NetworkSkinsMod.OnLevelUnloading
            //destroy in reverse order of creation
            if (basicControlPanel != null) {
                UnityEngine.GameObject.Destroy(basicControlPanel);
            }
            if (optionPanel != null) {
                UnityEngine.GameObject.Destroy(optionPanel);
            }

        }

        //Special Thanks to SamsamTS' mod MoveIt! on how to add Main Menu settings.
        public void OnSettingsUI(UIHelperBase helper) {
            try {
                UIHelper _UIHelper = helper.AddGroup(this.Name) as UIHelper;
                UIPanel _UIPanel = _UIHelper.self as UIPanel;
                UICheckBox _UICheckbox = (UICheckBox)_UIHelper.AddCheckbox("PLT Anarchy ON by default               >> see tooltip <<", UserSettingsMainMenu.anarchyPLTOnByDefault.value, OnAnarchyPLTDefaultChecked);
                _UICheckbox.tooltip = "If enabled, automatically enables PLT Anarchy on map load.\n\n>> Recommended to also enable:\nProp & Tree Anarchy: \"Anarchy ON by default\"\n(separate mod). <<";
                _UICheckbox.playAudioEvents = true;
            }
            catch (Exception e) {
                Debug.LogError("[PLT]: OnSettingsUI failed!");
                Debug.LogException(e);
            }
        }

        private void OnAnarchyPLTDefaultChecked(bool state) {
            UserSettingsMainMenu.anarchyPLTOnByDefault.value = state;
        }

        public PropLineToolMod() {
            //initialize main menu settings file
            try {
                GameSettings.AddSettingsFile(new SettingsFile[]
                {
                    new SettingsFile
                    {
                        fileName = UserSettingsMainMenu.FILENAME
                    }
                });
            }
            catch (Exception e) {
                Debug.LogError("[PLT]: PropLineToolMod.ctor(): Error in loading/creating the settings file!");
                Debug.LogException(e);
            }
        }

        //public void DEBUG_TestCircle3XZ()
        //{
        //    Debug.Log("[DEBUG PLT]: Testing Circle3XZ............");

        //    Debug.Log("[DEBUG PLT]: New Circle: center = (5, 3, 2), radius = 4, angleStart = 30.");
        //    Circle3XZ _circle = new Circle3XZ(new Vector3(5f, 3f, 2f), 4f, 30f);

        //    Debug.Log("[DEBUG PLT]: circle.Position(0) = " + _circle.Position(0));  //Expected (8.464, 3, 4) [ ] Actual: (7, 3, 5.5) -> theta  = ~60deg
        //    Debug.Log("[DEBUG PLT]: -30deg: circle.Position(-0.0833333) = " + _circle.Position(-0.08333333f));  //Expected(9, 3, 2) [ ] Actual: (5, 3, 6) -> theta = 0deg
        //    Debug.Log("[DEBUG PLT]: +60deg: circle.Position(0.16666667) = " + _circle.Position(0.16666667f));   //Expected(5, 3, 6) [ ] Actual: (9, 3, 2) -> theta = 90 deg

        //    Debug.Log("[DEBUG PLT]: Testing Circle3XZ.StepRotationFast()...");
        //    float _distance = Mathf.PI;
        //    Debug.Log("[DEBUG PLT]: float _distance = " + _distance);
        //    Quaternion _rotation = _circle.RotationByDistance(_distance);
        //    float _tStart = 0.20f;
        //    //deltaT = 0.125
        //    //tFinal = (angleStart/360) + tStart + deltaT = 0.40833333
        //    //thetaFinal = tFinal * 360 = 147
        //    //Expected (1.645, 3, 4.179) [ ] Actual: (8.9, 3, 1.2) -> theta = ~-12deg
        //    Debug.Log("[DEBUG PLT]: circle.StepRotationFast("+ _tStart + ", Circle3XZ.RotationByDistance(_distance)) =" + _circle.StepRotationFast(_tStart, _rotation));

        //    Debug.Log("[DEBUG PLT]: Testing Circle3XZ.DistanceSqr()...");
        //    Vector3 _testPoint = _circle.center + new Vector3(0f, 0f, 3f);
        //    Debug.Log("[DEBUG PLT]: Vector3 _testPoint = " + _testPoint);
        //    float _tOut = 0f;
        //    float _distanceSqr = _circle.DistanceSqr(_testPoint, out _tOut);
        //    Debug.Log("[DEBUG PLT]: Result _distanceSqr = " + _distanceSqr);    //Expected (3^2 = 9) [X] Actual: (9)
        //    Debug.Log("[DEBUG PLT]: Result _tOut = " + _tOut);                  //Expected (1/4 - 30/360 = 0.1667) [ ] Actual: (0.416667) -> off by 0.25
        //}
    }
}
