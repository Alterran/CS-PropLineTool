using ICities;
using ColossalFramework.UI;
using PropLineTool.UI.OptionPanel;
using PropLineTool.UI.ControlPanel;
using PropLineTool.Utility;
using UnityEngine;


namespace PropLineTool
{
    public class PropLineToolMod : LoadingExtensionBase, IUserMod
    {
        private static ICities.LoadMode m_loadMode;

        private static int m_onLevelLoadedCount = 0;

        public static ICities.LoadMode GetLoadMode()
        {
            ICities.LoadMode mode = m_loadMode;
            return mode;
        }
        
        public static UIOptionPanel optionPanel;
        
        public static UIBasicControlPanel basicControlPanel;
        
        public string Name
        {
            get
            {
                return "Prop Line Tool";
            }
        }

        public string Description
        {
            get
            {
                return "Place props and trees along curves. Also: fences!";
            }
        }
        
        //Detour Deploy code here
        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            
            //no detours
        }

        //Detour Revert code here
        public override void OnReleased()
        {
            base.OnReleased();
            
            //no detours
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            Debug.Log("[PropLineTool, hereafter PLT]: start PropLineToolMod.OnLevelLoaded");
            m_loadMode = mode;
            m_onLevelLoadedCount++;
            
            string _ordinal = Util.OrdinalSuffix(m_onLevelLoadedCount);

            Debug.Log("[PLT]: PropLineToolMod.OnLevelLoaded(): This is the " + m_onLevelLoadedCount + _ordinal + " time this method has been called, and the LoadMode is " + mode.ToString() + ".");


            bool _initializeToolMan = false;
            _initializeToolMan = ToolMan.ToolMan.Initialize();

            //PropLineTool Initialization
            PropLineTool.PopulateRandIntArray(0, 10000);

            if (_initializeToolMan == true)
            {
                Debug.Log("[PLT]: PropLineToolMod.OnLevelLoaded(): ToolMan.Initialize() returned true.");
                Debug.Log("[PLT]: PropLineToolMod.OnLevelLoaded(): Adding/Initializing UI components...");

                if (optionPanel == null)
                {
                    optionPanel = (UIView.GetAView().AddUIComponent(typeof(UIOptionPanel)) as UIOptionPanel);
                }

                if (basicControlPanel == null)
                {
                    basicControlPanel = (UIView.GetAView().AddUIComponent(typeof(UIBasicControlPanel)) as UIBasicControlPanel);
                }

                Debug.Log("[PLT]: PropLineToolMod.OnLevelLoaded(): UI components addition/initialization finished.");

                //debug purposes only
                //UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("[DEBUG] Prop Line Tool [PLT] Success in Initialization", "Prop Line Tool succeeded in registering itself with the game's tool controllers!", false);
            }
            else
            {
                Debug.LogError("[PLT]: PropLineToolMod.OnLevelLoaded(): ToolMan.Initialize() returned false.");

                //special thanks to RushHour.Compatibility for this
                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Prop Line Tool [PLT] Failed to Initialize", "Prop Line Tool failed to register itself with the game's tool controllers. Please save your output_log from this game session and post a link to it on the workshop page for PLT.", true);
            }
            
            
            Debug.Log("[PLT]: end PropLineToolMod.OnLevelLoaded");
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            //new as of 160816 0041
            //in reference to NetworkSkinsMod.OnLevelUnloading
            //destroy in reverse order of creation
            if (basicControlPanel != null)
            {
                UnityEngine.GameObject.Destroy(basicControlPanel);
            }
            if (optionPanel != null)
            {
                UnityEngine.GameObject.Destroy(optionPanel);
            }
            
        }

    }
}
