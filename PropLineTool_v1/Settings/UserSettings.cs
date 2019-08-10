using ColossalFramework;
using System;
using UnityEngine;
using PropLineTool.Utility;

namespace PropLineTool.Settings {
    public class UserSettingsControlPanel {
        // ======================= Parameters Tab Settings =======================
        public event VoidEventHandler eventParametersTabSettingChanged;
        protected void OnParametersTabSettingChanged() {
            eventParametersTabSettingChanged?.Invoke();
        }
        protected bool m_autoDefaultSpacing = true;
        public bool autoDefaultSpacing {
            get {
                return m_autoDefaultSpacing;
            }
            set {
                bool _oldValue = m_autoDefaultSpacing;
                m_autoDefaultSpacing = value;

                if (value != _oldValue) {
                    OnParametersTabSettingChanged();
                }
            }
        }
        protected bool m_angleFlip180 = false;
        public bool angleFlip180 {
            get {
                return m_angleFlip180;
            }
            set {
                bool _oldValue = m_angleFlip180;
                m_angleFlip180 = value;

                if (value != _oldValue) {
                    OnParametersTabSettingChanged();
                }
            }
        }


        // ======================= Undo Manager Settings =======================
        protected bool m_showUndoPreviews = true;
        public bool showUndoPreviews {
            get {
                return m_showUndoPreviews;
            }
            set {
                m_showUndoPreviews = value;
            }
        }



        // ======================= Error Checking =======================
        public event VoidEventHandler eventErrorCheckingSettingChanged;
        protected void OnErrorCheckingSettingChanged() {
            eventErrorCheckingSettingChanged?.Invoke();
        }

        protected bool m_errorChecking = true;
        public bool errorChecking {
            get {
                return m_errorChecking;
            }
            set {
                bool _oldValue = m_errorChecking;
                m_errorChecking = value;

                if (value != _oldValue) {
                    OnErrorCheckingSettingChanged();
                    OnErrorCheckingEnabledChanged(value);
                }
            }
        }
        public event VoidObjectPropertyChangedEventHandler<bool> eventErrorCheckingEnabledChanged;
        protected void OnErrorCheckingEnabledChanged(bool state) {
            eventErrorCheckingEnabledChanged?.Invoke(state);
        }

        protected bool m_showErrorGuides = true;
        public bool showErrorGuides {
            get {
                return m_showErrorGuides;
            }
            set {
                bool _oldValue = m_showErrorGuides;
                m_showErrorGuides = value;

                if (value != _oldValue) {
                    OnErrorCheckingSettingChanged();
                }
            }
        }

        protected bool m_anarchyPLT = false;
        public bool anarchyPLT {
            get {
                return m_anarchyPLT;
            }
            set {
                bool _oldValue = m_anarchyPLT;
                m_anarchyPLT = value;

                if (value != _oldValue) {
                    OnErrorCheckingSettingChanged();
                }
            }
        }

        protected bool m_placeBlockedItems = false;
        public bool placeBlockedItems {
            get {
                return m_placeBlockedItems;
            }
            set {
                bool _oldValue = m_placeBlockedItems;
                m_placeBlockedItems = value;

                if (value != _oldValue) {
                    OnErrorCheckingSettingChanged();
                }
            }
        }



        // ======================= Item Rendering and Positioning =======================
        public event VoidEventHandler eventRenderingPositioningSettingChanged;
        protected void OnRenderingPositioningSettingChanged() {
            eventRenderingPositioningSettingChanged?.Invoke();
        }
        protected bool m_renderPosResVanilla = false;
        public bool renderAndPlacePosResVanilla {
            get {
                return m_renderPosResVanilla;
            }
            set {
                bool _oldValue = m_renderPosResVanilla;
                m_renderPosResVanilla = value;

                if (value != _oldValue) {
                    OnRenderingPositioningSettingChanged();
                }
            }
        }
        protected bool m_useMeshCenterCorrection = true;
        public bool useMeshCenterCorrection {
            get {
                return m_useMeshCenterCorrection;
            }
            set {
                bool _oldValue = m_useMeshCenterCorrection;
                m_useMeshCenterCorrection = value;

                if (value != _oldValue) {
                    OnRenderingPositioningSettingChanged();
                }
            }
        }
        protected bool m_perfectCircles = false;
        public bool perfectCircles {
            get {
                return m_perfectCircles;
            }
            set {
                bool _oldValue = m_perfectCircles;
                m_perfectCircles = value;

                if (value != _oldValue) {
                    OnRenderingPositioningSettingChanged();
                }
            }
        }
        protected bool m_linearFenceFill = false;
        public bool linearFenceFill {
            get {
                return m_linearFenceFill;
            }
            set {
                bool _oldValue = m_linearFenceFill;
                m_linearFenceFill = value;

                if (value != _oldValue) {
                    OnRenderingPositioningSettingChanged();
                }
            }
        }

    }

    public static class UserSettingsMainMenu {
        public const string FILENAME = "PropLineTool";

        public static SavedBool anarchyPLTOnByDefault = new SavedBool("anarchyPLTOnByDefault", FILENAME, false, true);
    }


}