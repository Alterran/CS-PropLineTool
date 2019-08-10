using ColossalFramework.UI;
using UnityEngine;
using PropLineTool.Utility;

namespace PropLineTool.ToolSwitch {
    public class PLTToolSwitch {
        //used to determine whether to reset PLT when choosing new propInfo from FindIt! mod
        private static bool m_resetCounterActive = false;
        private static bool m_resetCounterExpired = false;
        private static float m_resetCounterStartTimeSeconds;
        private const float RESET_THRESHOLD_SECONDS = 30f; //30 seconds
        private static float resetCounterElapsedTimeSeconds {
            get {
                return Time.time - m_resetCounterStartTimeSeconds;
            }
        }
        private static bool pendingReset {
            get {
                bool _counterFlag = m_resetCounterActive && !m_resetCounterExpired && (resetCounterElapsedTimeSeconds < RESET_THRESHOLD_SECONDS);
                bool _visibilityFlag = PropLineTool.instance.IsOneControlPointVisible();
                return !_visibilityFlag || !_counterFlag;
            }
        }

        //used to determine if switching PLT drawmodes vs (switching tools: any tool --> PLT)
        public static bool m_PLTActive;
        public static bool m_wasPLTActive;
        public static bool m_bulldozeToolActive;
        public static bool m_wasBulldozeToolActive;

        //heavy-duty references
        private static UIPanel m_brushPanel;
        private static bool m_brushPanelFound;

        private static void FindBrushPanel() {
            GameObject _brushPanelGO = GameObject.Find("BrushPanel");
            m_brushPanel = _brushPanelGO == null ? null : _brushPanelGO.GetComponent<UIPanel>();

            //test if BrushPanel was found
            m_brushPanelFound = (m_brushPanel != null);
        }

        //initialize heavy-duty references
        public static void Initialize() {
            ICities.LoadMode _loadMode = PropLineToolMod.GetLoadMode();

            //bool _inMapOrAssetEditor = (   (_loadMode == ICities.LoadMode.LoadMap) || (_loadMode == ICities.LoadMode.LoadAsset) || (_loadMode == ICities.LoadMode.NewAsset) || (_loadMode == ICities.LoadMode.NewMap)   );
            bool _notMainGameplay = (!_loadMode.IsMainGameplay());

            FindBrushPanel();
        }

        //Works BEAUTIFULLY!! :DDD
        public static void SwitchTools(out bool allNull) {
            allNull = true;
            m_wasPLTActive = m_PLTActive;
            m_wasBulldozeToolActive = m_bulldozeToolActive;

            ICities.LoadMode _loadMode = PropLineToolMod.GetLoadMode();

            PropTool _propTool = ToolsModifierControl.GetCurrentTool<PropTool>();
            TreeTool _treeTool = ToolsModifierControl.GetCurrentTool<TreeTool>();
            PropLineTool _propLineTool = ToolsModifierControl.GetCurrentTool<PropLineTool>();

            BulldozeTool _bulldozeTool = ToolsModifierControl.GetCurrentTool<BulldozeTool>();

            if ((_propTool == null) && (_treeTool == null) && (_propLineTool == null)) {
                allNull = true;
                if (!m_wasBulldozeToolActive) {
                    m_PLTActive = false;
                }

                if (m_wasPLTActive == true) {
                    PropLineTool.m_keepActiveState = true;
                }

                if (_bulldozeTool != null) {
                    m_bulldozeToolActive = true;
                } else {
                    m_bulldozeToolActive = false;
                }

                if (m_resetCounterActive && resetCounterElapsedTimeSeconds > RESET_THRESHOLD_SECONDS) {
                    m_resetCounterActive = false;
                    m_resetCounterExpired = true;
                } else if (!m_resetCounterExpired && !m_resetCounterActive) {
                    m_resetCounterActive = true;
                    m_resetCounterStartTimeSeconds = Time.time;

                    //Debug.Log("[PLTDEBUG]: m_resetCounterStartTimeSeconds = " + m_resetCounterStartTimeSeconds);
                }

                return;
            } else {
                allNull = false;
                if (_propLineTool != null) {
                    m_PLTActive = true;
                }
                //continue along
            }

            //single mode: signal tool switch
            //not-single mode: signal standby
            bool _PLTActiveExclusive = ((_propLineTool != null) && (_propTool == null) && (_treeTool == null));

            //single mode: signal standby
            //not-single mode: signal tool switch
            bool _PLTInactiveButPropOrTreeActive = ((_propLineTool == null) && ((_propTool != null) || (_treeTool != null)));

            //error checking?
            bool _multipleActivePropTreeTools = ((_propLineTool != null) && ((_propTool != null) || (_treeTool != null)));
            if (_multipleActivePropTreeTools) {
                Debug.LogError("[PLT]: ToolSwitch: More than one active tool!");
                return;
            }

            //loadmode is in-game
            bool _inGame = ((_loadMode == ICities.LoadMode.NewGame) || (_loadMode == ICities.LoadMode.LoadGame));
            //loadmode is map-editor or asset-editor [EDIT: ACTUALLY JUST MAP EDITOR]
            //bool flag4 = (  (_loadMode == ICities.LoadMode.LoadMap) || (_loadMode == ICities.LoadMode.LoadAsset) || (_loadMode == ICities.LoadMode.NewAsset) || (_loadMode == ICities.LoadMode.NewMap)  );
            bool _mapEditor = ((_loadMode == ICities.LoadMode.LoadMap) || (_loadMode == ICities.LoadMode.NewMap));

            //test if BrushPanel was found
            m_brushPanelFound = (m_brushPanel != null);

            switch (PropLineTool.drawMode) {
                case PropLineTool.DrawMode.Single: {
                    //reset active state
                    PropLineTool.m_keepActiveState = false;

                    if (_PLTActiveExclusive) {
                        switch (PropLineTool.objectMode) {
                            case PropLineTool.ObjectMode.Undefined: {
                                Debug.LogError("[PLT]: ToolSwitch: Object mode is undefined!");
                                break;
                            }
                            case PropLineTool.ObjectMode.Props: {
                                PropInfo oldPropInfo = _propLineTool.propPrefab;
                                PropTool newPropTool = ToolsModifierControl.SetTool<PropTool>();
                                if (oldPropInfo == null) {
                                    Debug.LogError("[PLT]: ToolSwitch: PropLineTool prop prefab is null!");
                                    return;
                                }
                                newPropTool.m_prefab = oldPropInfo;

                                //new as of 190809
                                FindBrushPanel();

                                if (_mapEditor && m_brushPanelFound) {
                                    m_brushPanel.Show();
                                }
                                break;
                            }
                            case PropLineTool.ObjectMode.Trees: {
                                TreeInfo oldTreeInfo = _propLineTool.treePrefab;
                                TreeTool newTreeTool = ToolsModifierControl.SetTool<TreeTool>();
                                if (oldTreeInfo == null) {
                                    Debug.LogError("[PLT]: ToolSwitch: PropLineTool tree prefab is null!");
                                    return;
                                }
                                newTreeTool.m_prefab = _propLineTool.treePrefab;

                                //new as of 190809
                                FindBrushPanel();

                                if ((_mapEditor || (_inGame)) && m_brushPanelFound) {
                                    m_brushPanel.Show();
                                }
                                break;
                            }
                        }
                        return;
                    } else if (_PLTInactiveButPropOrTreeActive) {
                        return;
                    } else {
                        Debug.LogError("[PLT]: ToolSwitch: PropLineTool -> mismatch!");
                        return;
                    }
                }
                case PropLineTool.DrawMode.Straight:
                case PropLineTool.DrawMode.Curved:
                case PropLineTool.DrawMode.Freeform:
                case PropLineTool.DrawMode.Circle: {
                    if (_PLTInactiveButPropOrTreeActive) {
                        if (m_wasPLTActive == true) {
                            PropLineTool.m_keepActiveState = true;
                        } else {
                            if (pendingReset) {
                                PropLineTool.m_keepActiveState = false;
                            } else //do not reset
                              {
                                PropLineTool.m_keepActiveState = true;
                            }
                        }

                        m_resetCounterExpired = false;
                        m_resetCounterActive = false;

                        //continue along (no "return;" on this line)
                        if (_propTool != null) {
                            PropInfo oldPropInfo = _propTool.m_prefab;
                            PropLineTool newPropLineTool = ToolsModifierControl.SetTool<PropLineTool>();
                            //PropLineTool.objectMode = PropLineTool.ObjectMode.Props;
                            if (oldPropInfo == null) {
                                Debug.LogError("[PLT]: ToolSwitch: PropTool prop prefab is null!");
                                return;
                            }
                            newPropLineTool.propPrefab = oldPropInfo;
                            //calling after setting prefab
                            PropLineTool.objectMode = PropLineTool.ObjectMode.Props;

                            //new as of 190809
                            FindBrushPanel();

                            if (m_brushPanelFound) {
                                m_brushPanel.Hide();
                            }
                            return;
                        }
                        if (_treeTool != null) {
                            TreeInfo oldTreeInfo = _treeTool.m_prefab;
                            PropLineTool newPropLineTool = ToolsModifierControl.SetTool<PropLineTool>();
                            //PropLineTool.objectMode = PropLineTool.ObjectMode.Trees;
                            if (oldTreeInfo == null) {
                                Debug.LogError("[PLT]: ToolSwitch: TreeTool tree prefab is null!");
                                return;
                            }
                            newPropLineTool.treePrefab = oldTreeInfo;
                            //calling after setting prefab
                            PropLineTool.objectMode = PropLineTool.ObjectMode.Trees;

                            //new as of 190809
                            FindBrushPanel();

                            if (m_brushPanelFound) {
                                m_brushPanel.Hide();
                            }
                            return;
                        }
                    } else if (_PLTActiveExclusive) {
                        if ((_propLineTool.propPrefab == null) && (_propLineTool.treePrefab == null)) {
                            Debug.LogError("[PLT]: ToolSwitch: PropLineTool prop and tree prefabs are null!");
                        }
                        return;
                    }
                    break;
                }
                default: {
                    Debug.LogError("[PLT]: ToolSwitch: Draw Mode is out of bounds!");
                    return;
                }
            }

            //safety-net return
            Debug.LogError("[PLT]: Reached safety-net return of ToolSwitch.SwitchTools");
            return;
        }
    }
}