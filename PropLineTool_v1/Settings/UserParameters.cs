using ColossalFramework;
using System;
using UnityEngine;

namespace PropLineTool.Parameters {
    //user-set parameters, set from the PLT control panel
    public class UserParameters {
        //LIMITS ON FIELD VALUES
        //   spacing
        /// <summary>
        /// The length of one map tile.
        /// </summary>
        public const float SPACING_TILE_MAX = 1920f;
        public const float SPACING_MAX = 2000f;
        public const float SPACING_MIN = 0.10f;

    }
}