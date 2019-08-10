using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using ColossalFramework.UI;
using ColossalFramework.Math;
using ColossalFramework.PlatformServices;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.Plugins;

namespace PropLineTool.Utility {
    public delegate void ObjectPropertyChangedEventHandler<T>(object sender, T value);

    public delegate void VoidObjectPropertyChangedEventHandler<T>(T value);

    public delegate void VoidEventHandler();

    public static class Util {
        public const ulong STEAM_WORKSHOP_ID = 694512541uL;

        /// <summary>
        /// Generates a string array from the symbolic names of an input enum.
        /// </summary>
        /// <typeparam name="TEnum">enum type</typeparam>
        /// <returns></returns>
        public static string[] EnumToStringArray<TEnum>() where TEnum : struct, IComparable, IConvertible, IFormattable {
            if (!typeof(TEnum).IsEnum) {
                throw new ArgumentException("[PLT]: Util.EnumToStringArray<TEnum>(): TEnum must be an enum! (It was entered/detected as otherwise).");
            }

            string[] _stringArray = Enum.GetNames(typeof(TEnum));

            return _stringArray;
        }

        //source: http://stackoverflow.com/questions/4107625/how-can-i-convert-assembly-codebase-into-a-filesystem-path-in-c/28319367#28319367
        public static string GetAssemblyFullPathCodebaseToFileSystem(Assembly assembly) {
            string codeBasePseudoUrl = assembly.CodeBase; // "pseudo" because it is not properly escaped
            if (codeBasePseudoUrl != null) {
                const string filePrefix3 = @"file:///";
                if (codeBasePseudoUrl.StartsWith(filePrefix3)) {
                    string sPath = codeBasePseudoUrl.Substring(filePrefix3.Length);
                    string bsPath = sPath.Replace('/', '\\');
                    //Console.WriteLine("bsPath: " + bsPath);
                    string fp = Path.GetFullPath(bsPath);
                    //Console.WriteLine("fp: " + fp);
                    return fp;
                }
            }
            Debug.Log("[PLT]: Util.GetAssemblyFullPathCodebaseToFileSystem(): Codebase evaluation failed. Using default Location as fallback.");
            return Path.GetFullPath(assembly.Location);
        }

        //seems to return only the folder of the game's executable file
        //source: http://stackoverflow.com/questions/52797/how-do-i-get-the-path-of-the-assembly-the-code-is-in
        public static string GetExecutingAssemblyDirectory() {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            //Assembly _assembly = Assembly.GetCallingAssembly();

            string codeBase = _assembly.CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);

            string _directory = Path.GetDirectoryName(path);

            if (Directory.Exists(_directory)) {
                Debug.Log("[PLT]: Util.GetExecutingAssemblyDirectory(): Found directory exists according to Directory.Exists()");
                Debug.Log("[PLT]: Util.GetExecutingAssemblyDirectory(): found directory = " + _directory);

                return _directory;
            } else {
                Debug.Log("[PLT]: Util.GetExecutingAssemblyDirectory(): An executing assembly path was found, but it does not exist according to Directory.Exists()");
                Debug.Log("[PLT]: Util.GetExecutingAssemblyDirectory(): found 'nonexisting' directory = " + _directory);

                Debug.Log("[PLT]: Util.GetExecutingAssemblyDirectory(): Attempting to fix executing assembly path...");

                _directory = GetAssemblyFullPathCodebaseToFileSystem(_assembly);
                if (Directory.Exists(_directory)) {
                    Debug.Log("[PLT]: Util.GetExecutingAssemblyDirectory(): Success in fixing executing assembly path according to Directory.Exists()");
                    Debug.Log("[PLT]: Util.GetExecutingAssemblyDirectory(): found fixed 'existing' directory = " + _directory);
                    return _directory;
                } else {
                    Debug.Log("[PLT]: Util.GetExecutingAssemblyDirectory(): Failed to fix executing assembly path according to Directory.Exists()");
                    Debug.Log("[PLT]: Util.GetExecutingAssemblyDirectory(): found try-fixed 'nonexisting' directory = " + _directory);
                }
            }

            Debug.Log("[PLT]: Util.GetExecutingAssemblyDirectory(): return null.");

            return null;
        }

        public static string GetSteamSubscribedItemPath(ulong fileID) {
            PublishedFileId _publishedFileID = new PublishedFileId(fileID);

            string _path = PlatformService.workshop.GetSubscribedItemPath(_publishedFileID);

            return _path;
        }

        public static string OrdinalSuffix(int cardinalNumber) {
            string _ordinal = "th";

            int _mod100 = cardinalNumber % 100;
            if (_mod100 >= 11 && _mod100 <= 19) {
                _ordinal = "th";
            } else {
                switch (cardinalNumber % 10) {
                    case 1: {
                        _ordinal = "st";
                        break;
                    }
                    case 2: {
                        _ordinal = "nd";
                        break;
                    }
                    case 3: {
                        _ordinal = "rd";
                        break;
                    }
                    default: {
                        _ordinal = "th";
                        break;
                    }
                }
            }


            return _ordinal;
        }

        //*************
        public static string GetModPath() {
            foreach (PluginManager.PluginInfo mod in Singleton<PluginManager>.instance.GetPluginsInfo()) {
                //if (current.publishedFileID.AsUInt64 == STEAM_WORKSHOP_ID || current.name.Contains("PropLineTool_v1"))
                //these extra conditions are here for users who like to move their mods to the Local folder
                if (mod.publishedFileID.AsUInt64 == STEAM_WORKSHOP_ID || mod.name.Contains("PropLineTool") || mod.name.Contains(STEAM_WORKSHOP_ID.ToString())) {
                    string _modpath = mod.modPath;

                    Debug.Log("[PLT]: Util.GetModPath(): Success in finding mod path from searching plugins.");
                    Debug.Log("[PLT]: Util.GetModPath(): found modpath = " + _modpath);

                    return _modpath;
                }
            }
            //check if user moved to Local folder and renamed PropLineTool folder to "PLT"
            foreach (PluginManager.PluginInfo mod in Singleton<PluginManager>.instance.GetPluginsInfo()) {
                if (mod.name.Contains("PLT") && mod.isEnabled) {
                    string _modpath = mod.modPath;

                    Debug.Log("[PLT]: Util.GetModPath(): Success in finding mod path from searching plugins.");
                    Debug.Log("[PLT]: Util.GetModPath(): found modpath = " + _modpath);

                    return _modpath;
                }
            }

            Debug.Log("[PLT]: Util.GetModPath(): Could not find mod path from searching plugins.");

            string _steamPath = GetSteamSubscribedItemPath(STEAM_WORKSHOP_ID);
            if (_steamPath != null) {
                if (Directory.Exists(_steamPath)) {
                    Debug.Log("[PLT]: Util.GetModPath(): Defaulting to using PLT workshop subscription directory from ColossalFramework.Steamworks.Steam");
                    Debug.Log("[PLT]: Util.GetModPath(): steam directory = " + _steamPath);

                    return _steamPath;
                } else {
                    Debug.Log("[PLT]: Util.GetModPath(): Found PLT workshop subscription directory from ColossalFramework.Steamworks.Steam, BUT Directory.Exists() returns false");
                    Debug.Log("[PLT]: Util.GetModPath(): found 'nonexisting' steam directory = " + _steamPath);
                }
            } else {
                Debug.Log("[PLT]: Util.GetModPath(): Could not find mod path from ColossalFramework.Steamworks.Steam");
            }




            Debug.Log("[PLT]: Util.GetModPath(): Could not find mod path from any method.");

            return "";
        }

        /// <summary>
        /// Sets up the spriteNames for a UIButton. Leaves prefixes blank to not change those sprites.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="bgSpritePrefix"></param>
        /// <param name="fgSpritePrefix"></param>
        public static void SetButtonSprites(ref UIButton button, string bgSpritePrefix, string fgSpritePrefix) {
            if (bgSpritePrefix != "") {
                button.normalBgSprite = bgSpritePrefix;
                button.focusedBgSprite = bgSpritePrefix + "Focused";
                button.hoveredBgSprite = bgSpritePrefix + "Hovered";
                button.pressedBgSprite = bgSpritePrefix + "Pressed";
                button.disabledBgSprite = bgSpritePrefix + "Disabled";
            }
            if (fgSpritePrefix != "") {
                button.normalFgSprite = fgSpritePrefix;
                button.focusedFgSprite = fgSpritePrefix + "Focused";
                button.hoveredFgSprite = fgSpritePrefix + "Hovered";
                button.pressedFgSprite = fgSpritePrefix + "Pressed";
                button.disabledFgSprite = fgSpritePrefix + "Disabled";
            }
        }

    }

    public struct ShortVector3 {
        public short x;
        public ushort y;
        public short z;

        public ShortVector3(short x, ushort y, short z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public ShortVector3(Vector3 floatVector3) {
            this = floatVector3.ToShortVector3();
        }

        public Vector3 ToVector3() {
            Vector3 _result = Vector3.zero;

            if (Singleton<ToolManager>.instance.m_properties.m_mode == ItemClass.Availability.AssetEditor) {
                _result.x = (float)this.x * 0.0164794922f;
                _result.y = (float)this.y * 0.015625f;
                _result.z = (float)this.z * 0.0164794922f;
            } else {
                _result.x = (float)this.x * 0.263671875f;
                _result.y = (float)this.y * 0.015625f;
                _result.z = (float)this.z * 0.263671875f;
            }

            return _result;
        }
    }

    public static class Vector3Extensions {
        public static bool Approximately(this Vector3 value, Vector3 comparisonVector) {
            bool _approxX = Mathf.Approximately(value.x, comparisonVector.x);
            bool _approxY = Mathf.Approximately(value.y, comparisonVector.y);
            bool _approxZ = Mathf.Approximately(value.z, comparisonVector.z);

            if (_approxX == true && _approxY == true && _approxZ == true) {
                return true;
            }
            return false;
        }
        public static bool ApproximatelyXZ(this Vector3 value, Vector3 comparisonVector) {
            bool _approxX = Mathf.Approximately(value.x, comparisonVector.x);
            bool _approxZ = Mathf.Approximately(value.z, comparisonVector.z);

            if (_approxX == true && _approxZ == true) {
                return true;
            }
            return false;
        }

        public static bool EqualOnGameShortGridXZ(this Vector3 value, Vector3 comparisonVector) {
            ShortVector3 _shortValue = new ShortVector3(value);
            ShortVector3 _shortComparisonValue = new ShortVector3(comparisonVector);

            if (_shortValue.x == _shortComparisonValue.x && _shortValue.z == _shortComparisonValue.z) {
                return true;
            } else {
                return false;
            }
        }

        public static bool NearlyEqualOnGameShortGridXZ(this Vector3 value, Vector3 comparisonVector) {
            ShortVector3 _shortValue = new ShortVector3(value);
            ShortVector3 _shortComparisonValue = new ShortVector3(comparisonVector);

            int _tolerance = 1;

            bool _NearlyEqualX = (_shortValue.x >= _shortComparisonValue.x - _tolerance) && (_shortValue.x <= _shortComparisonValue.x + _tolerance);
            bool _NearlyEqualZ = (_shortValue.z >= _shortComparisonValue.z - _tolerance) && (_shortValue.z <= _shortComparisonValue.z + _tolerance);

            if (_NearlyEqualX && _NearlyEqualZ) {
                return true;
            } else {
                return false;
            }
        }

        public static ShortVector3 ToShortVector3(this Vector3 value) {
            short _x = 0;
            ushort _y = 0;
            short _z = 0;

            if (Singleton<ToolManager>.instance.m_properties.m_mode == ItemClass.Availability.AssetEditor) {
                _x = (short)Mathf.Clamp(Mathf.RoundToInt(value.x * 60.68148f), -32767, 32767);
                _z = (short)Mathf.Clamp(Mathf.RoundToInt(value.z * 60.68148f), -32767, 32767);
                _y = (ushort)Mathf.Clamp(Mathf.RoundToInt(value.y * 64f), 0, 65535);
            } else {
                _x = (short)Mathf.Clamp(Mathf.RoundToInt(value.x * 3.79259253f), -32767, 32767);
                _z = (short)Mathf.Clamp(Mathf.RoundToInt(value.z * 3.79259253f), -32767, 32767);
                _y = (ushort)Mathf.Clamp(Mathf.RoundToInt(value.y * 64f), 0, 65535);
            }

            return new ShortVector3(_x, _y, _z);
        }

        public static Vector3 QuantizeToGameShortGridXYZ(this Vector3 value) {
            ShortVector3 _shortValue = value.ToShortVector3();
            Vector3 _result = _shortValue.ToVector3();

            return _result;
        }

        public static float MagnitudeXZ(this Vector3 value) {
            Vector3 _vectorXZ = new Vector3(value.x, 0f, value.z);

            return _vectorXZ.magnitude;
        }

        public static float SqrMagnitudeXZ(this Vector3 value) {
            return value.x * value.x + value.z * value.z;
        }
    }

    public static class Segment3Extensions {
        public static float LengthXZ(this Segment3 line) {
            Vector3 _start = new Vector3(line.a.x, 0f, line.a.z);
            Vector3 _end = new Vector3(line.b.x, 0f, line.b.z);

            return Vector3.Distance(_start, _end);
        }
    }

    public static class UIMultiStateButtonExtensions {
        public static void SetVanillaToggleSprites(this UIMultiStateButton multiButton, string bgPrefix, string fgPrefix) {
            if (!bgPrefix.IsNullOrWhiteSpace()) {
                UIMultiStateButton.SpriteSetState _backgroundSprites = multiButton.backgroundSprites;

                if (_backgroundSprites.Count >= 2) {
                    _backgroundSprites[0].normal = bgPrefix + "";
                    _backgroundSprites[0].hovered = bgPrefix + "Hovered";
                    _backgroundSprites[0].pressed = bgPrefix + "Pressed";
                    _backgroundSprites[0].disabled = bgPrefix + "Disabled";
                    _backgroundSprites[1].normal = bgPrefix + "Focused";
                    _backgroundSprites[1].disabled = bgPrefix + "Disabled";
                }
            }
            if (!fgPrefix.IsNullOrWhiteSpace()) {
                UIMultiStateButton.SpriteSetState _foregroundSprites = multiButton.foregroundSprites;

                if (_foregroundSprites.Count >= 2) {
                    _foregroundSprites[0].normal = fgPrefix + "";
                    _foregroundSprites[0].hovered = fgPrefix + "Hovered";
                    _foregroundSprites[0].pressed = fgPrefix + "Pressed";
                    _foregroundSprites[0].disabled = fgPrefix + "Disabled";
                    _foregroundSprites[1].normal = fgPrefix + "Focused";
                    _foregroundSprites[1].disabled = fgPrefix + "Disabled";
                }
            }
        }
    }

    public static class PrefabExtensions {
        public const float CENTER_AREA_FRACTION = 0.00390625f;

        private static bool IsCenterAreaSignificant(Vector3 center, Vector3 size, bool constrainToXZ, out Vector3 centerCorrectionOrtho) {
            centerCorrectionOrtho = Vector3.zero;

            if (constrainToXZ) {
                if (center.SqrMagnitudeXZ() >= CENTER_AREA_FRACTION * size.SqrMagnitudeXZ()) {
                    //negate center vector
                    centerCorrectionOrtho = -center;
                    centerCorrectionOrtho.y = 0f;
                    return true;
                }
                return false;
            } else {
                if (center.sqrMagnitude >= CENTER_AREA_FRACTION * size.sqrMagnitude) {
                    //negate center vector
                    centerCorrectionOrtho = -center;
                    return true;
                }
                return false;
            }
        }

        public static bool IsMeshCenterOffset(this PropInfo prop, bool constrainToXZ, out Vector3 centerCorrectionOrthogonal) {
            centerCorrectionOrthogonal = Vector3.zero;

            Mesh _mesh = prop.m_mesh;
            if (_mesh == null) {
                return false;
            }
            Bounds _bounds = _mesh.bounds;

            Vector3 _center = _bounds.center;
            Vector3 _size = _bounds.size;

            bool _result = IsCenterAreaSignificant(_center, _size, constrainToXZ, out centerCorrectionOrthogonal);
            return _result;
        }

        public static bool IsMeshCenterOffset(this TreeInfo tree, bool constrainToXZ, out Vector3 centerCorrectionOrthogonal) {
            centerCorrectionOrthogonal = Vector3.zero;

            Mesh _mesh = tree.m_mesh;
            if (_mesh == null) {
                return false;
            }
            Bounds _bounds = _mesh.bounds;

            Vector3 _center = _bounds.center;
            Vector3 _size = _bounds.size;

            bool _result = IsCenterAreaSignificant(_center, _size, constrainToXZ, out centerCorrectionOrthogonal);
            return _result;
        }
    }

    public static class EnumExtensions {
        public static bool IsMainGameplay(this ICities.LoadMode loadMode) {
            switch (loadMode) {
                case ICities.LoadMode.NewGame:
                case ICities.LoadMode.LoadGame:
                case ICities.LoadMode.NewGameFromScenario: {
                    return true;
                }
                default: {
                    return false;
                }
            }
        }
    }

    //special Thanks to BP's NaturalResourcesBrush.Redirection
    public static class ATuple {
        public static ATuple<T1, T2> New<T1, T2>(T1 first, T2 second) {
            return new ATuple<T1, T2>(first, second);
        }
    }
    public class ATuple<T1, T2> {
        public T1 First {
            get;
            private set;
        }

        public T2 Second {
            get;
            private set;
        }

        internal ATuple(T1 first, T2 second) {
            this.First = first;
            this.Second = second;
        }
    }
}