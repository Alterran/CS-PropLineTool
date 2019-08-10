using ICities;

using ColossalFramework;
using ColossalFramework.UI;
using ColossalFramework.Globalization;

using UnityEngine;

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace PropLineTool.Sprites {
    internal static class SpriteManager {
        //sprite path
        public const string SPRITE_PATH = "PropLineTool_v1.Icons.";

        //atlas name
        public const string ATLAS_NAME_PLT = "PLTAtlas";

        //atlas
        private static UITextureAtlas m_atlasPLT;
        public static UITextureAtlas atlasPLT {
            get {
                if (m_atlasPLT == null) {
                    CreateAtlasPLT();
                }

                return m_atlasPLT;
            }
            private set {
                m_atlasPLT = value;
            }
        }

        //vanilla atlases
        private static UITextureAtlas m_vanillaAtlasIngame;
        public static UITextureAtlas vanillaAtlasIngame {
            get {
                if (m_vanillaAtlasIngame == null) {
                    m_vanillaAtlasIngame = ResourceLoader.GetAtlas("Ingame");
                }

                return m_vanillaAtlasIngame;
            }
        }
        private static UITextureAtlas m_vanillaAtlasInMapEditor;
        public static UITextureAtlas vanillaAtlasInMapEditor {
            get {
                if (m_vanillaAtlasInMapEditor == null) {
                    m_vanillaAtlasInMapEditor = ResourceLoader.GetAtlas("InMapEditor");
                }

                return m_vanillaAtlasInMapEditor;
            }
        }
        private static UITextureAtlas m_vanillaAtlasInScenarioEditor;
        public static UITextureAtlas vanillaAtlasInScenarioEditor {
            get {
                if (m_vanillaAtlasInScenarioEditor == null) {
                    m_vanillaAtlasInScenarioEditor = ResourceLoader.GetAtlas("InScenarioEditor");
                }

                return m_vanillaAtlasInScenarioEditor;
            }
        }

        //sprites
        private static string[] m_spriteNamesPLT = new string[61]
        {
            "PLT_MultiStateZero",
            "PLT_MultiStateZeroFocused",
            "PLT_MultiStateZeroHovered",
            "PLT_MultiStateZeroPressed",
            "PLT_MultiStateZeroDisabled",
            "PLT_MultiStateOne",
            "PLT_MultiStateOneFocused",
            "PLT_MultiStateOneHovered",
            "PLT_MultiStateOnePressed",
            "PLT_MultiStateOneDisabled",
            "PLT_MultiStateTwo",
            "PLT_MultiStateTwoFocused",
            "PLT_MultiStateTwoHovered",
            "PLT_MultiStateTwoPressed",
            "PLT_MultiStateTwoDisabled",
            "PLT_ToggleCPZero",
            "PLT_ToggleCPZeroFocused",
            "PLT_ToggleCPZeroHovered",
            "PLT_ToggleCPZeroPressed",
            "PLT_ToggleCPZeroDisabled",
            "PLT_ToggleCPOne",
            "PLT_ToggleCPOneFocused",
            "PLT_ToggleCPOneHovered",
            "PLT_ToggleCPOnePressed",
            "PLT_ToggleCPOneDisabled",
            "PLT_FenceModeZero",
            "PLT_FenceModeZeroFocused",
            "PLT_FenceModeZeroHovered",
            "PLT_FenceModeZeroPressed",
            "PLT_FenceModeZeroDisabled",
            "PLT_FenceModeOne",
            "PLT_FenceModeOneFocused",
            "PLT_FenceModeOneHovered",
            "PLT_FenceModeOnePressed",
            "PLT_FenceModeOneDisabled",
            "PLT_FenceModeTwo",
            "PLT_FenceModeTwoFocused",
            "PLT_FenceModeTwoHovered",
            "PLT_FenceModeTwoDisabled",
            "PLT_FenceModeTwo",
            "PLT_ItemwiseZero",
            "PLT_ItemwiseZeroFocused",
            "PLT_ItemwiseZeroHovered",
            "PLT_ItemwiseZeroPressed",
            "PLT_ItemwiseZeroDisabled",
            "PLT_ItemwiseOne",
            "PLT_ItemwiseOneFocused",
            "PLT_ItemwiseOneHovered",
            "PLT_ItemwiseOnePressed",
            "PLT_ItemwiseOneDisabled",
            "PLT_SpacingwiseZero",
            "PLT_SpacingwiseZeroFocused",
            "PLT_SpacingwiseZeroHovered",
            "PLT_SpacingwiseZeroPressed",
            "PLT_SpacingwiseZeroDisabled",
            "PLT_SpacingwiseOne",
            "PLT_SpacingwiseOneFocused",
            "PLT_SpacingwiseOneHovered",
            "PLT_SpacingwiseOnePressed",
            "PLT_SpacingwiseOneDisabled",
            "PLT_BasicDividerTile02x02"
        };
        public static string[] spriteNamesPLT {
            get {
                return m_spriteNamesPLT;
            }
            private set {
                m_spriteNamesPLT = value;
            }
        }

        public static void CreateAtlasPLT() {
            if (m_atlasPLT == null) {
                m_atlasPLT = ResourceLoader.CreateTextureAtlas(ATLAS_NAME_PLT, spriteNamesPLT, SPRITE_PATH);
            }
        }
    }
}