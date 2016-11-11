﻿using ColossalFramework.UI;
using UnityEngine;

namespace PropLineTool.ToolSwitch
{
    public class PLTToolSwitch
    {
        //used to determine if switching PLT drawmodes vs (switching tools: any tool --> PLT)
        public static bool m_PLTActive;
        public static bool m_wasPLTActive;
        public static bool m_bulldozeToolActive;
        public static bool m_wasBulldozeToolActive;
        
        //Works BEAUTIFULLY!! :DDD
        public static void SwitchTools(out bool allNull)
        {
            allNull = true;
            m_wasPLTActive = m_PLTActive;
            m_wasBulldozeToolActive = m_bulldozeToolActive;

            ICities.LoadMode _loadMode = PropLineToolMod.GetLoadMode();
            
            bool _inMapOrAssetEditor = (   (_loadMode == ICities.LoadMode.LoadMap) || (_loadMode == ICities.LoadMode.LoadAsset) || (_loadMode == ICities.LoadMode.NewAsset) || (_loadMode == ICities.LoadMode.NewMap)   );
            UIPanel _brushPanel;
            if (_inMapOrAssetEditor)
            {
                _brushPanel = GameObject.Find("BrushPanel").GetComponent<UIPanel>();
            }
            else
            {
                _brushPanel = new UIPanel();
            }

            PropTool _propTool = ToolsModifierControl.GetCurrentTool<PropTool>();
            TreeTool _treeTool = ToolsModifierControl.GetCurrentTool<TreeTool>();
            PropLineTool _propLineTool = ToolsModifierControl.GetCurrentTool<PropLineTool>();

            BulldozeTool _bulldozeTool = ToolsModifierControl.GetCurrentTool<BulldozeTool>();

            if ( (_propTool == null) && (_treeTool == null) && (_propLineTool == null) )
            {
                allNull = true;
                if (!m_wasBulldozeToolActive)
                {
                    m_PLTActive = false;
                }

                if (m_wasPLTActive == true)
                {
                    PropLineTool.m_keepActiveState = true;
                }

                if (_bulldozeTool != null)
                {
                    m_bulldozeToolActive = true;
                }
                else
                {
                    m_bulldozeToolActive = false;
                }
                
                return;
            }
            else
            {
                allNull = false;
                if (_propLineTool != null)
                {
                    m_PLTActive = true;
                }
                //continue along
            }
            
            //single mode: signal tool switch
            //not-single mode: signal standby
            bool _PLTActiveExclusive = ((_propLineTool != null) && (_propTool == null) && (_treeTool == null));

            //single mode: signal standby
            //not-single mode: signal tool switch
            bool _PLTInactiveButPropOrTreeActive = (   (_propLineTool == null) && ( (_propTool != null) || (_treeTool != null) )   );

            //error checking?
            bool _multipleActivePropTreeTools = ((_propLineTool != null) && ((_propTool != null) || (_treeTool != null)));
            if (_multipleActivePropTreeTools)
            {
                Debug.LogError("[PLT]: ToolSwitch: More than one active tool!");
                return;
            }

            //loadmode is in-game
            bool _inGame = (  (_loadMode == ICities.LoadMode.NewGame) || (_loadMode == ICities.LoadMode.LoadGame)  );
            //loadmode is map-editor or asset-editor [EDIT: ACTUALLY JUST MAP EDITOR]
            //bool flag4 = (  (_loadMode == ICities.LoadMode.LoadMap) || (_loadMode == ICities.LoadMode.LoadAsset) || (_loadMode == ICities.LoadMode.NewAsset) || (_loadMode == ICities.LoadMode.NewMap)  );
            bool _mapEditor = (  (_loadMode == ICities.LoadMode.LoadMap) || (_loadMode == ICities.LoadMode.NewMap)  );
            
            //test if BrushPanel was found
            bool _brushPanelFound = (_brushPanel != null);
            
            if (PropLineTool.drawMode == PropLineTool.DrawMode.Single)
            {
                //reset active state
                PropLineTool.m_keepActiveState = false;

                if (_PLTActiveExclusive)
                {
                    switch (PropLineTool.objectMode)
                    {
                        case PropLineTool.ObjectMode.Undefined:
                            {
                                Debug.LogError("[PLT]: ToolSwitch: Object mode is undefined!");
                                break;
                            }
                        case PropLineTool.ObjectMode.Props:
                            {
                                PropInfo oldPropInfo = _propLineTool.propPrefab;
                                PropTool newPropTool = ToolsModifierControl.SetTool<PropTool>();
                                if (oldPropInfo == null)
                                {
                                    Debug.LogError("[PLT]: ToolSwitch: PropLineTool prop prefab is null!");
                                    return;
                                }
                                newPropTool.m_prefab = oldPropInfo;

                                if (_mapEditor && _brushPanelFound)
                                {
                                    _brushPanel.Show();
                                }
                                break;
                            }
                        case PropLineTool.ObjectMode.Trees:
                            {
                                TreeInfo oldTreeInfo = _propLineTool.treePrefab;
                                TreeTool newTreeTool = ToolsModifierControl.SetTool<TreeTool>();
                                if (oldTreeInfo == null)
                                {
                                    Debug.LogError("[PLT]: ToolSwitch: PropLineTool tree prefab is null!");
                                    return;
                                }
                                newTreeTool.m_prefab = _propLineTool.treePrefab;

                                if ( (_mapEditor || (_inGame)) && _brushPanelFound)
                                {
                                    _brushPanel.Show();
                                }
                                break;
                            }
                    }
                    return;
                }
                else if (_PLTInactiveButPropOrTreeActive)
                {
                    return;
                }
                else
                {
                    Debug.LogError("[PLT]: ToolSwitch: PropLineTool -> mismatch!");
                    return;
                }
            }
            else if ((PropLineTool.drawMode == PropLineTool.DrawMode.Straight) || (PropLineTool.drawMode == PropLineTool.DrawMode.Curved) || (PropLineTool.drawMode == PropLineTool.DrawMode.Freeform))
            {
                if (_PLTInactiveButPropOrTreeActive)
                {
                    if (m_wasPLTActive == true)
                    {
                        PropLineTool.m_keepActiveState = true;
                    }
                    else
                    {
                        PropLineTool.m_keepActiveState = false;
                    }


                    //continue along (no "return;" on this line)
                    if (_propTool != null)
                    {
                        PropInfo oldPropInfo = _propTool.m_prefab;
                        PropLineTool newPropLineTool = ToolsModifierControl.SetTool<PropLineTool>();
                        //PropLineTool.objectMode = PropLineTool.ObjectMode.Props;
                        if (oldPropInfo == null)
                        {
                            Debug.LogError("[PLT]: ToolSwitch: PropTool prop prefab is null!");
                            return;
                        }
                        newPropLineTool.propPrefab = oldPropInfo;
                        //calling after setting prefab
                        PropLineTool.objectMode = PropLineTool.ObjectMode.Props;

                        if (_brushPanelFound)
                        {
                            _brushPanel.Hide();
                        }
                        return;
                    }
                    if (_treeTool != null)
                    {
                        TreeInfo oldTreeInfo = _treeTool.m_prefab;
                        PropLineTool newPropLineTool = ToolsModifierControl.SetTool<PropLineTool>();
                        //PropLineTool.objectMode = PropLineTool.ObjectMode.Trees;
                        if (oldTreeInfo == null)
                        {
                            Debug.LogError("[PLT]: ToolSwitch: TreeTool tree prefab is null!");
                            return;
                        }
                        newPropLineTool.treePrefab = oldTreeInfo;
                        //calling after setting prefab
                        PropLineTool.objectMode = PropLineTool.ObjectMode.Trees;

                        if (_brushPanelFound)
                        {
                            _brushPanel.Hide();
                        }
                        return;
                    }
                }
                else if (_PLTActiveExclusive)
                {
                    if ( (_propLineTool.propPrefab == null) && (_propLineTool.treePrefab == null))
                    {
                        Debug.LogError("[PLT]: ToolSwitch: PropLineTool prop and tree prefabs are null!");
                    }
                    return;
                }
            }
            else
            {
                Debug.LogError("[PLT]: ToolSwitch: Draw Mode is out of bounds!");
                return;
            }

            //safety-net return
            Debug.LogError("[PLT]: Reached safety-net return of ToolSwitch.SwitchTools");
            return;
        }
    }
}