using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

//Special Thanks to BloodyPenguin for the NaturalResourcesBrush.NaturalResourcesBrush
//as a template.

//Utility for managing extra Tools
namespace PropLineTool.ToolMan {
    public static class ToolMan {
        private static bool SetUpPropLineTool(ref ToolController toolController, ref List<ToolBase> extraTools) {
            PropLineTool propLineTool = toolController.gameObject.GetComponent<PropLineTool>();
            if (propLineTool == null) {
                propLineTool = toolController.gameObject.AddComponent<PropLineTool>();
                extraTools.Add(propLineTool);

                Debug.Log("[PLT]: ToolMan.SetupPropLineTool(): Added PropLineTool to toolController. Returning true...");
                return true;
            } else {
                Debug.Log("[PLT]: ToolMan.SetupPropLineTool(): PropLineTool already exists in the toolController. Returning false...");
                return false;
            }
        }



        //original as of 160815 2312
        //private static List<ToolBase> SetupExtraTools(ref ToolController toolController)
        //{
        //    List<ToolBase> toolBaseList = new List<ToolBase>();

        //    SetUpPropLineTool(ref toolController, ref toolBaseList);

        //    return toolBaseList;
        //}

        //new 160815 2312
        private static bool SetupExtraTools(ref ToolController toolController, out List<ToolBase> extraTools) {
            List<ToolBase> _toolBaseList = new List<ToolBase>();
            extraTools = _toolBaseList;

            bool _setupPropLineToolResult = false;
            _setupPropLineToolResult = SetUpPropLineTool(ref toolController, ref _toolBaseList);

            //return false iff all extraTools already exist
            if (_setupPropLineToolResult == false) {
                Debug.Log("[PLT]: ToolMan.SetupExtraTools(): Returning false...");

                return false;
            } else {
                extraTools = _toolBaseList;

                Debug.Log("[PLT]: ToolMan.SetupExtraTools(): Returning true...");

                return true;
            }
        }

        //Last Updated 160622
        private static bool AddExtraToolsToController(ref ToolController toolController, List<ToolBase> extraTools) {
            bool _result = false;

            Debug.Log("[PLT]: Begin ToolMan.AddExtraToolsToController().");
            Debug.Log("[PLT]: ToolMan.AddExtraToolsToController(): Progress: 0/4 [    ]");

            if (toolController == null) {
                Debug.LogError("[PLT]: ToolMan.AddExtraToolsToController(): Failed to append PropLineTool to tool controllers: toolController parameter is null.");
                return false;
            }
            if (extraTools == null) {
                Debug.LogError("[PLT]: ToolMan.AddExtraToolsToController(): Failed to append PropLineTool to tool controllers: extraTools parameter is null.");
                return false;
            }
            if (extraTools.Count < 1) {
                Debug.LogWarning("[PLT]: ToolMan.AddExtraToolsToController(): No tools were found in the extraTools parameter.");
                return false;
            }

            Debug.Log("[PLT]: ToolMan.AddExtraToolsToController(): Progress: 1/4 [=   ]");

            var _toolsFieldInfo = typeof(ToolController).GetField("m_tools", BindingFlags.Instance | BindingFlags.NonPublic);

            var _tools = (ToolBase[])_toolsFieldInfo.GetValue(toolController);

            if (_tools == null) {
                //old
                //Debug.LogError("[PLT]: ToolMan.AddExtraToolsToController(): Failed to append PropLineTool to tool controllers: Could not FieldInfo.GetValue() from ToolController.m_tools.");
                //return false;

                //new 
                Debug.LogWarning("[PLT]: ToolMan.AddExtraToolsToController(): toolController.m_tools was detected to be null from FieldInfo.GetValue().");
                Debug.Log("[PLT]: ToolMan.AddExtraToolsToController(): Attempting to pre-populate toolController.m_tools by calling toolController.GetComponents<ToolBase>().");

                _tools = toolController.GetComponents<ToolBase>();

                var _tools2 = (ToolBase[])_toolsFieldInfo.GetValue(toolController);

                if (_tools2 == null) {
                    Debug.LogError("[PLT]: ToolMan.AddExtraToolsToController(): Failed to pre-populate toolController.m_tools. Returning false...");
                    return false;
                } else if (_tools.Length > 0) {
                    Debug.Log("[PLT]: ToolMan.AddExtraToolsToController(): The attempt to pre-populate toolController.m_tools appears to have been successful...");
                    Debug.Log("[PLT]: ToolMan.AddExtraToolsToController(): Pre-populated toolController.m_tools with >>" + _tools.Length + "<< tools.");
                } else {
                    Debug.LogError("[PLT]: ToolMan.AddExtraToolsToController(): The attempt to pre-populate toolController.m_tools failed. No tools were found. Returning false...");
                    return false;
                }
            }
            if (_tools.Length < 1) {
                Debug.LogWarning("[PLT]: ToolMan.AddExtraToolsToController(): Initial ToolController.m_tools has a length < 1. Its length is: " + _tools.Length + ".");

                if (extraTools.Count < 1) {
                    Debug.LogError("[PLT]: ToolMan.AddExtraToolsToController(): Returning false since both toolController.m_tools and extraTools have a length < 1.");
                    return false;
                }
            }

            Debug.Log("[PLT]: ToolMan.AddExtraToolsToController(): Progress: 2/4 [==  ]");

            //Prepare toolController.m_tools for appending new elements
            int _initialLength = _tools.Length;
            Array.Resize<ToolBase>(ref _tools, _initialLength + extraTools.Count);
            var index = 0;

            //find ToolsModifierControl tool dictionary
            var _dictionary = (Dictionary<Type, ToolBase>)typeof(ToolsModifierControl).GetField("m_Tools", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            if (_dictionary == null) {
                //old
                //Debug.LogError("[PLT]: ToolMan.AddExtraToolsToController(): Failed to append PropLineTool to tool controllers: Could not find ToolsModifierControl.m_Tools dictionary.");
                //return false;

                //new
                Debug.LogWarning("[PLT]: ToolMan.AddExtraToolsToController(): ToolsModifierControl.m_Tools dictionary was detected to be null from FieldInfo.GetField().");

                Debug.Log("[PLT]: ToolMan.AddExtraToolsToController(): Attempting to pre-populate ToolsModifierControl.m_Tools dictionary by calling ToolsModifierControl.GetTool<PropTool>()...");
                var _propTool = ToolsModifierControl.GetTool<PropTool>();

                var _dictionary2 = (Dictionary<Type, ToolBase>)typeof(ToolsModifierControl).GetField("m_Tools", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

                if (_dictionary2 == null) {
                    Debug.Log("[PLT]: ToolMan.AddExtraToolsToController(): Failed in pre-populating ToolsModifierControl.m_Tools by calling ToolsModifierControl.GetTool<PropTool>()...");

                    Debug.Log("[PLT]: ToolMan.AddExtraToolsToController(): Attempting to pre-populate ToolsModifierControl.m_Tools by copying the >>" + _tools.Length + "<< elements of toolController.m_tools.");
                    //Our own version of ToolsModifierControl.CollectTools()
                    _dictionary = new Dictionary<Type, ToolBase>(_initialLength + extraTools.Count);
                    for (int i = 0; i < _tools.Length; i++) {
                        _dictionary.Add(_tools[i].GetType(), _tools[i]);
                    }

                    var _dictionary3 = (Dictionary<Type, ToolBase>)typeof(ToolsModifierControl).GetField("m_Tools", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

                    if (_dictionary3 == null) {
                        Debug.Log("[PLT]: ToolMan.AddExtraToolsToController(): Failed in pre-populating ToolsModifierControl.m_Tools by copying the >>" + _tools.Length + "<< elements of toolController.m_tools.");

                        Debug.LogError("[PLT]: ToolMan.AddExtraToolsToController(): All attempts to pre-populate ToolsModifierControl.m_Tools failed. ): Returning false...");
                        return false;
                    } else {
                        _dictionary = _dictionary3;
                        Debug.Log("[PLT]: ToolMan.AddExtraToolsToController(): The attempt to pre-populate ToolsModifierControl.m_Tools dictionary by copying the >>" + _tools.Length + "<< elements of toolController.m_tools appears to have been successful...");
                    }
                } else {
                    _dictionary = _dictionary2;
                    Debug.Log("[PLT]: ToolMan.AddExtraToolsToController(): The attempt to pre-populate ToolsModifierControl.m_Tools dictionary by calling ToolsModifierControl.GetTool<PropTool>() appears to have been successful...");
                }

                Debug.Log("[PLT]: ToolMan.AddExtraToolsToController(): Verifying the length of the ToolBase collections for both tool controllers...");
                //check that lengths match
                if (_dictionary == null) {
                    Debug.LogError("[PLT]: ToolMan.AddExtraToolsToController(): _dictionary is null! Returning false...");
                    return false;
                }
                if (_dictionary.Count == _tools.Length) {
                    Debug.Log("[PLT]: ToolMan.AddExtraToolsToController(): Successfully pre-populated ToolsModifierControl.m_Tools dictionary. Its Count is: " + _dictionary2.Count + ".");
                } else {
                    //actually since we resized _tools earlier, _tools should be extraTools.Length (1) longer than _dictionary.
                    Debug.LogWarning("[PLT]: ToolMan.AddExtraToolsToController(): ToolsModifierControl.m_Tools.Count does not equal toolController.m_tools.Length");
                    Debug.Log("[PLT]: ToolMan.AddExtraToolsToController(): ToolsModifierControl.m_Tools.Count = " + _dictionary2.Count + ", and toolController.m_tools.Length = " + _tools.Length + ".");
                }

            } else if (_dictionary.Count < 1) {
                Debug.LogWarning("[PLT]: ToolMan.AddExtraToolsToController(): Initial ToolsModifierControl.m_Tools dictionary has a count < 1. Its count is: " + _dictionary.Count + ".");
            }

            //append PropLineTool to ToolBase collections
            foreach (var _currentTool in extraTools) {
                _dictionary.Add(_currentTool.GetType(), _currentTool);
                _tools[_initialLength + index] = _currentTool;
                index++;
            }

            Debug.Log("[PLT]: ToolMan.AddExtraToolsToController(): Progress: 3/4 [=== ]");

            _toolsFieldInfo.SetValue(toolController, _tools);

            PropLineTool _propLineTool = ToolsModifierControl.GetTool<PropLineTool>();
            if (_propLineTool == null) {
                Debug.LogError("[PLT]: ToolMan.AddExtraToolsToController(): PropLineTool was not found in ToolsModifierControl after appending to the tool dictionary.");
                return false;
            }

            Debug.Log("[PLT]: ToolMan.AddExtraToolsToController(): Progress: 4/4 [====]");
            Debug.Log("[PLT]: ToolMan.AddExtraToolsToController(): Success in appending PropLineTool to tool controllers!");

            //160815 2306
            //These are only really necessary as I suspect two instances of PropLineTool are being created for some users (NullReferenceException when placing each line).
            //I no longer believe this ^ to be the case, so we comment these out.
            //Debug.Log("[PLT]: ToolMan.AddExtraToolsToController(): Outputting toolController lists:");
            //Debug.Log("[PLT]: ToolMan.AddExtraToolsToController(): toolController.m_tools array has " + _tools.Length + " members:");
            //for (int i = 0; i < _tools.Length; i++)
            //{
            //    Debug.Log("[PLT]: ToolMan.AddExtraToolsToController(): toolController.m_tools[" + i + "] = " + _tools[i]);
            //}
            //Debug.Log("[PLT]: ToolMan.AddExtraToolsToController(): ToolsModifierControl.m_Tools dictionary has " + _dictionary.Count + " members:");
            //var _keys = _dictionary.Keys.ToList<Type>();
            //foreach (Type _type in _keys)
            //{
            //    Debug.Log("[PLT]: ToolMan.AddExtraToolsToController(): ToolsModifierControl.m_Tools[" + _type + "] = " + _dictionary[_type]);
            //}

            _result = true;
            return _result;
        }

        //NEW as of 160815 2238
        public static bool Initialize() {
            //Code to add Tools to the ToolController
            ToolController toolController = ToolsModifierControl.toolController;
            if (toolController == null) {
                Debug.LogError("[PLT]: ToolMan.Initialize(): ToolController not found!");
                return false;
            }

            //as of 160624 0400, We should test if tool controllers are already populated AND PropLineTool exists in both before trying to add it to the ToolBase collections.
            //   this is so the error message is not thrown when loading another game (second, third, fourth, etc.) from the same Cities:Skylines session as PLT already exists in the tool controllers.
            bool _addExtraToolsResult = false;
            try {
                Debug.Log("[PLT]: ToolMan.Initialize(): Attempting to create a list of tools to append...");
                List<ToolBase> extraTools = new List<ToolBase>();
                bool _setupExtraToolsResult = false;
                _setupExtraToolsResult = SetupExtraTools(ref toolController, out extraTools);
                if (_setupExtraToolsResult == true) {
                    Debug.Log("[PLT]: ToolMan.Initialize(): Attempting to add extra tools from list to controller...");
                    _addExtraToolsResult = AddExtraToolsToController(ref toolController, extraTools);
                    Debug.Log("[PLT]: ToolMan.Initialize(): ...Reached line immediately after ToolMan.AddExtraToolsToController().");
                } else {
                    Debug.Log("[PLT]: ToolMan.Initialize(): No extra tools were found...");
                }
            }
            catch (Exception e) {
                Debug.LogError(e);
            }
            finally {
                if (toolController.Tools.Length != 0) {
                    toolController.Tools[0].enabled = true;
                }
            }

            return true;
        }
    }
}