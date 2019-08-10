using System;
using PropLineTool.Utility;
using UnityEngine;
using ColossalFramework;

namespace PropLineTool.Undo {

    public struct TreePlacementInfo {
        private uint m_treeID;
        private Vector3 m_position;
        private float m_assetLength;
        private Vector3 m_meshPosition;

        public uint treeID {
            get {
                return m_treeID;
            }
            set {
                m_treeID = value;
            }
        }
        public Vector3 position {
            get {
                return m_position;
            }
            set {
                m_position = value;
            }
        }
        public float assetLength {
            get {
                return m_assetLength;
            }
            set {
                m_assetLength = value;
            }
        }
        public Vector3 meshPosition {
            get {
                return m_meshPosition;
            }
            set {
                m_meshPosition = value;
            }
        }

    }

    public struct PropPlacementInfo {
        private ushort m_propID;
        private Vector3 m_position;
        private float m_angle;
        private float m_assetLength;
        private Vector3 m_meshPosition;

        public ushort propID {
            get {
                return m_propID;
            }
            set {
                m_propID = value;
            }
        }
        public Vector3 position {
            get {
                return m_position;
            }
            set {
                m_position = value;
            }
        }
        /// <summary>
        /// Angle in radians.
        /// </summary>
        public float angle {
            get {
                return m_angle;
            }
            set {
                value %= 360f;
                m_angle = value;
            }
        }
        public float assetLength {
            get {
                return m_assetLength;
            }
            set {
                m_assetLength = value;
            }
        }
        public Vector3 meshPosition {
            get {
                return m_meshPosition;
            }
            set {
                m_meshPosition = value;
            }
        }

    }

    public struct TreeSubEntry {
        private TreePlacementInfo m_treePlacementInfo;
        private bool m_stillExists;
        public TreePlacementInfo treePlacementInfo {
            get {
                return m_treePlacementInfo;
            }
            set {
                m_treePlacementInfo = value;
                stillExists = true;
            }
        }
        public bool stillExists {
            get {
                return m_stillExists;
            }
            set {
                m_stillExists = value;
            }
        }

        public void CheckTreeStillExists() {
            uint _treeID = treePlacementInfo.treeID;
            TreeInstance _treeInstance = Singleton<TreeManager>.instance.m_trees.m_buffer[(int)((UIntPtr)_treeID)];
            //bool _samePosition = _treeInstance.Position.EqualOnGameShortGridXZ(treePlacementInfo.position);
            bool _samePosition = _treeInstance.Position.EqualOnGameShortGridXZ(treePlacementInfo.meshPosition);
            bool _flagsNotEmpty = _treeInstance.m_flags != 0;
            if (_samePosition == true && _flagsNotEmpty == true) {
                stillExists = true;
            } else {
                stillExists = false;
            }
        }

        public bool ReleaseTree(bool dispatchPlacementEffect) {
            CheckTreeStillExists();

            if (stillExists == false) {
                return false;
            } else {
                TreeManager instance = Singleton<TreeManager>.instance;
                instance.ReleaseTree(treePlacementInfo.treeID);
                if (dispatchPlacementEffect) {
                    PropLineTool.DispatchPlacementEffect(treePlacementInfo.position, true);
                }

                return true;
            }
        }
    }

    public struct PropSubEntry {
        private PropPlacementInfo m_propPlacementInfo;
        private bool m_stillExists;
        public PropPlacementInfo propPlacementInfo {
            get {
                return m_propPlacementInfo;
            }
            set {
                m_propPlacementInfo = value;
                stillExists = true;
            }
        }
        public bool stillExists {
            get {
                return m_stillExists;
            }
            set {
                m_stillExists = value;
            }
        }

        public void CheckPropStillExists() {
            ushort _propID = propPlacementInfo.propID;
            PropInstance _propInstance = Singleton<PropManager>.instance.m_props.m_buffer[(int)((UIntPtr)_propID)];
            //pre-PropPrecision:
            //bool _samePosition = _propInstance.Position.EqualOnGameShortGridXZ(propPlacementInfo.position);
            //post-PropPrecision:
            //bool _samePosition = _propInstance.Position.NearlyEqualOnGameShortGridXZ(propPlacementInfo.position);
            bool _samePosition = _propInstance.Position.NearlyEqualOnGameShortGridXZ(propPlacementInfo.meshPosition);
            bool _sameAngle = true;
            bool _flagsNotEmpty = _propInstance.m_flags != 0;
            if (_samePosition == true && _sameAngle == true && _flagsNotEmpty == true) {
                stillExists = true;
            } else {
                stillExists = false;
            }
        }

        public bool ReleaseProp(bool dispatchPlacementEffect) {
            CheckPropStillExists();

            if (stillExists == false) {
                return false;
            } else {
                if (Singleton<PropManager>.instance.m_props.m_buffer[(int)((UIntPtr)propPlacementInfo.propID)].m_flags != 0) {
                    PropManager instance = Singleton<PropManager>.instance;
                    instance.ReleaseProp(propPlacementInfo.propID);
                    if (dispatchPlacementEffect) {
                        PropLineTool.DispatchPlacementEffect(propPlacementInfo.position, true);
                    }

                    return true;
                }

                return false;
            }
        }
    }

    public struct UndoEntry {
        private TreeSubEntry[] m_trees;
        private PropSubEntry[] m_props;
        private PropLineTool.ObjectMode m_objectModeSegment;
        public TreeSubEntry[] trees {
            get {
                return m_trees;
            }
            set {
                m_trees = value;
            }
        }
        public PropSubEntry[] props {
            get {
                return m_props;
            }
            set {
                m_props = value;
            }
        }
        public PropLineTool.ObjectMode objectModeSegment {
            get {
                return m_objectModeSegment;
            }
            set {
                m_objectModeSegment = value;
            }
        }

        private int m_itemCount;
        public int itemCount {
            get {
                return m_itemCount;
            }
            set {
                value = Mathf.Clamp(value, 0, PropLineTool.MAX_ITEM_ARRAY_LENGTH);
                m_itemCount = value;
            }
        }

        //new stuff for max fill continue 170527
        //only if class-based
        //private SegmentState m_segmentState = new SegmentState();
        private SegmentInfo m_segmentInfo;
        public SegmentInfo segmentInfo {
            get {
                return m_segmentInfo;
            }
            set {
                m_segmentInfo = value;
            }
        }
        private bool m_fenceMode;
        public bool fenceMode {
            get {
                return m_fenceMode;
            }
            set {
                m_fenceMode = value;
            }
        }

        private bool IsIndexWithinBounds(int index) {
            if (index < 0 || index > itemCount - 1) {
                return false;
            }
            return true;
        }

        public bool GetItemPosition(int index, out Vector3 itemPosition) {
            itemPosition = Vector3.zero;

            if (IsIndexWithinBounds(index)) {
                switch (objectModeSegment) {
                    case PropLineTool.ObjectMode.Props: {
                        if (props == null) {
                            return false;
                        }
                        itemPosition = props[index].propPlacementInfo.position;
                        break;
                    }
                    case PropLineTool.ObjectMode.Trees: {
                        if (trees == null) {
                            return false;
                        }
                        itemPosition = trees[index].treePlacementInfo.position;
                        break;
                    }
                    default: {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public bool GetItemMeshPosition(int index, out Vector3 itemPosition) {
            itemPosition = Vector3.zero;

            if (IsIndexWithinBounds(index)) {
                switch (objectModeSegment) {
                    case PropLineTool.ObjectMode.Props: {
                        if (props == null) {
                            return false;
                        }
                        itemPosition = props[index].propPlacementInfo.meshPosition;
                        break;
                    }
                    case PropLineTool.ObjectMode.Trees: {
                        if (trees == null) {
                            return false;
                        }
                        itemPosition = trees[index].treePlacementInfo.meshPosition;
                        break;
                    }
                    default: {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public bool GetItemAngle(int index, out float itemAngle) {
            itemAngle = 0f;

            if (IsIndexWithinBounds(index)) {
                switch (objectModeSegment) {
                    case PropLineTool.ObjectMode.Props: {
                        if (props == null) {
                            return false;
                        }
                        itemAngle = props[index].propPlacementInfo.angle;
                        break;
                    }
                    case PropLineTool.ObjectMode.Trees: {
                        return false;
                    }
                    default: {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public bool GetItemAssetLength(int index, out float itemAssetLength) {
            itemAssetLength = 8f;

            if (IsIndexWithinBounds(index)) {
                switch (objectModeSegment) {
                    case PropLineTool.ObjectMode.Props: {
                        if (props == null) {
                            return false;
                        }
                        itemAssetLength = props[index].propPlacementInfo.assetLength;
                        break;
                    }
                    case PropLineTool.ObjectMode.Trees: {
                        if (trees == null) {
                            return false;
                        }
                        itemAssetLength = trees[index].treePlacementInfo.assetLength;
                        break;
                    }
                    default: {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public bool GetItemStillExists(int index, out bool itemStillExists) {
            itemStillExists = false;

            if (IsIndexWithinBounds(index)) {
                switch (objectModeSegment) {
                    case PropLineTool.ObjectMode.Props: {
                        if (props == null) {
                            return false;
                        }
                        itemStillExists = props[index].stillExists;
                        break;
                    }
                    case PropLineTool.ObjectMode.Trees: {
                        if (trees == null) {
                            return false;
                        }
                        itemStillExists = trees[index].stillExists;
                        break;
                    }
                    default: {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public bool GetPropID(int index, out ushort propID) {
            propID = 0;

            if (IsIndexWithinBounds(index)) {
                if (props == null) {
                    return false;
                }
                propID = props[index].propPlacementInfo.propID;
                return true;
            }
            return false;
        }

        public bool GetTreeID(int index, out uint treeID) {
            treeID = 0;

            if (IsIndexWithinBounds(index)) {
                if (trees == null) {
                    return false;
                }
                treeID = trees[index].treePlacementInfo.treeID;
                return true;
            }
            return false;
        }

        public bool ReleaseProp(int index) {
            if (IsIndexWithinBounds(index)) {
                if (props == null) {
                    return false;
                }
                if (props[index].ReleaseProp(true)) {
                    return true;
                }
                return false;
            }
            return false;
        }

        public bool ReleaseTree(int index) {
            if (IsIndexWithinBounds(index)) {
                if (trees == null) {
                    return false;
                }
                if (trees[index].ReleaseTree(true)) {
                    return true;
                }
                return false;
            }
            return false;
        }


    }

    public class UndoManager {
        public const int MAX_UNDO_COUNT = 64;

        protected UndoEntry[] m_undoList = new UndoEntry[MAX_UNDO_COUNT];
        public UndoEntry[] undoList {
            get {
                return m_undoList;
            }
            protected set {
                m_undoList = value;
            }
        }

        public UndoEntry this[int index] {
            get {
                //only if class-based
                //if (undoList[index] != null)
                //{
                //    return undoList[index];
                //}
                //else
                //{
                //    return new UndoEntry();
                //}

                return undoList[index];
            }
            protected set {
                undoList[index] = value;
            }
        }
        public UndoEntry latestEntry {
            get {
                //only if class-based
                //if (undoList[latestEntryIndex] != null)
                //{
                //    return undoList[latestEntryIndex];
                //}
                //else
                //{
                //    return new UndoEntry();
                //}

                return undoList[latestEntryIndex];
            }
            protected set {
                undoList[latestEntryIndex] = value;
            }
        }
        protected int m_latestEntryIndex = 0;
        public int latestEntryIndex {
            get {
                return m_latestEntryIndex;
            }
            protected set {
                if (value >= MAX_UNDO_COUNT) {
                    value %= MAX_UNDO_COUNT;
                } else if (value < 0) {
                    value = Mathf.Abs(value);
                    value = MAX_UNDO_COUNT - (value % MAX_UNDO_COUNT);
                }
                m_latestEntryIndex = value;
            }
        }
        protected int m_actualEntryCount = 0;
        public int actualEntryCount {
            get {
                return m_actualEntryCount;
            }
            protected set {
                value = Mathf.Clamp(value, 0, MAX_UNDO_COUNT);
                m_actualEntryCount = value;
            }
        }

        protected PropPlacementInfo SetPropPlacementInfo(PropLineTool.ItemPlacementInfo itemInfo) {
            PropPlacementInfo _placementInfo = new PropPlacementInfo {
                propID = itemInfo.propID,
                position = itemInfo.position,
                angle = itemInfo.angle,
                meshPosition = itemInfo.meshPosition
            };

            return _placementInfo;
        }

        protected TreePlacementInfo SetTreePlacementInfo(PropLineTool.ItemPlacementInfo itemInfo) {
            TreePlacementInfo _placementInfo = new TreePlacementInfo {
                treeID = itemInfo.treeID,
                position = itemInfo.position,
                meshPosition = itemInfo.meshPosition
            };

            return _placementInfo;
        }

        public bool AddEntry(int numItems, PropLineTool.ItemPlacementInfo[] placementInfoArray, PropLineTool.ObjectMode objectMode) {
            if (objectMode == PropLineTool.ObjectMode.Undefined) {
                return false;
            }
            if (numItems <= 0) {
                return false;
            }
            if (placementInfoArray.Length <= 0) {
                return false;
            }
            if (numItems > placementInfoArray.Length) {
                numItems = placementInfoArray.Length;
            }

            int _indexNewUndoEntry = actualEntryCount == 0 ? 0 : (latestEntryIndex + 1);
            if (_indexNewUndoEntry >= MAX_UNDO_COUNT) {
                _indexNewUndoEntry %= MAX_UNDO_COUNT;
            }

            //only if class-based
            //setup if null
            //if (undoList[_indexNewUndoEntry] == null)
            //{
            //    undoList[_indexNewUndoEntry] = new UndoEntry();
            //}

            switch (objectMode) {
                case PropLineTool.ObjectMode.Props: {
                    undoList[_indexNewUndoEntry].props = new PropSubEntry[numItems];
                    for (int i = 0; i < numItems; i++) {
                        undoList[_indexNewUndoEntry].props[i].propPlacementInfo = SetPropPlacementInfo(placementInfoArray[i]);
                        undoList[_indexNewUndoEntry].props[i].stillExists = true;
                    }
                    undoList[_indexNewUndoEntry].objectModeSegment = PropLineTool.ObjectMode.Props;
                    break;
                }
                case PropLineTool.ObjectMode.Trees: {
                    undoList[_indexNewUndoEntry].trees = new TreeSubEntry[numItems];

                    for (int i = 0; i < numItems; i++) {
                        undoList[_indexNewUndoEntry].trees[i].treePlacementInfo = SetTreePlacementInfo(placementInfoArray[i]);
                        undoList[_indexNewUndoEntry].trees[i].stillExists = true;
                    }

                    undoList[_indexNewUndoEntry].objectModeSegment = PropLineTool.ObjectMode.Trees;
                    break;
                }
                default: {
                    return false;
                }
            }

            undoList[_indexNewUndoEntry].itemCount = numItems;

            latestEntryIndex = _indexNewUndoEntry;
            actualEntryCount++;

            CheckItemsStillExist();

            return true;
        }

        public bool AddEntry(int numItems, PropLineTool.ItemPlacementInfo[] placementInfoArray, PropLineTool.ObjectMode objectMode, bool fenceMode, SegmentState segmentState) {
            bool _result = AddEntry(numItems, placementInfoArray, objectMode);

            undoList[latestEntryIndex].fenceMode = fenceMode;
            undoList[latestEntryIndex].segmentInfo = segmentState.segmentInfo;

            return _result;
        }

        public bool UndoLatestEntry(out SegmentInfo latestEntrySegmentInfo) {
            bool _result = false;

            if (actualEntryCount <= 0) {
                latestEntrySegmentInfo = SegmentInfo.defaultValue;
                return false;
            }

            int _itemCount = latestEntry.itemCount;
            if (_itemCount <= 0) {
                latestEntrySegmentInfo = SegmentInfo.defaultValue;
                return false;
            }

            //only if class-based
            //check if null
            //if (latestEntry == null)
            //{
            //    return false;
            //}

            CheckItemsStillExist();

            switch (latestEntry.objectModeSegment) {
                case PropLineTool.ObjectMode.Props: {
                    for (int i = 0; i < _itemCount; i++) {
                        if (latestEntry.ReleaseProp(i) == false) {

                        }
                    }
                    _result = true;
                    break;
                }
                case PropLineTool.ObjectMode.Trees: {
                    for (int i = 0; i < _itemCount; i++) {
                        if (latestEntry.ReleaseTree(i) == false) {

                        }
                    }
                    _result = true;
                    break;
                }
                default: {
                    latestEntrySegmentInfo = SegmentInfo.defaultValue;
                    return false;
                }
            }

            //bool _undoMaxFillContinue = latestEntry.segmentInfo.m_isMaxFillContinue || latestEntry.segmentInfo.isReadyForMaxContinue;
            //if (_result == true && _undoMaxFillContinue)
            //{
            //    segmentState.RevertLastContinueParameters(latestEntry.segmentInfo.m_lastFinalOffset, latestEntry.segmentInfo.m_lastFenceEndpoint);
            //}

            latestEntrySegmentInfo = latestEntry.segmentInfo;

            undoList[latestEntryIndex].itemCount = 0;

            latestEntryIndex--;
            actualEntryCount--;

            return _result;
        }

        public bool RenderLatestEntryCircles(RenderManager.CameraInfo cameraInfo, Color32 renderColor) {
            if (actualEntryCount <= 0) {
                return false;
            } else {
                int _itemCount = latestEntry.itemCount;
                Color32 _pinpointColor = new Color32(renderColor.r, renderColor.g, renderColor.b, 225);
                Color32 _pointColor = new Color32(renderColor.r, renderColor.g, renderColor.b, 204);
                Color32 _boundsColor = new Color32(renderColor.r, renderColor.g, renderColor.b, 153);

                bool _itemStillExists = false;
                Vector3 _position = new Vector3();
                //Vector3 _meshPosition = new Vector3();
                float _assetLength = 8f;

                for (int i = 0; i < _itemCount; i++) {
                    latestEntry.GetItemStillExists(i, out _itemStillExists);
                    //if (latestEntry.GetItemMeshPosition(i, out _meshPosition) && latestEntry.GetItemAssetLength(i, out _assetLength) && _itemStillExists == true)
                    if (latestEntry.GetItemPosition(i, out _position) && latestEntry.GetItemAssetLength(i, out _assetLength) && _itemStillExists == true) {
                        RenderCircle(cameraInfo, _position, 0.10f, _pinpointColor, false, false);
                        RenderCircle(cameraInfo, _position, 2f, _pointColor, false, false);
                        RenderCircle(cameraInfo, _position, 8f, _boundsColor, false, true);
                    }
                }


                return true;
            }
        }

        protected void RenderCircle(RenderManager.CameraInfo cameraInfo, Vector3 position, float size, Color color, bool renderLimits, bool alphaBlend) {
            PropLineTool.RenderCircle(cameraInfo, position, size, color, renderLimits, alphaBlend);
        }

        public void CheckItemsStillExist() {
            if (actualEntryCount <= 0) {
                return;
            }
            //only if class-based
            //if (latestEntry == null)
            //{
            //    return;
            //}
            if (latestEntry.itemCount <= 0) {
                return;
            }

            switch (latestEntry.objectModeSegment) {
                case PropLineTool.ObjectMode.Props: {
                    if (latestEntry.props == null) {
                        return;
                    }

                    for (int i = 0; i < latestEntry.itemCount; i++) {
                        if (latestEntry.props[i].stillExists == true) {
                            latestEntry.props[i].CheckPropStillExists();
                        }
                    }
                    break;
                }
                case PropLineTool.ObjectMode.Trees: {
                    if (latestEntry.trees == null) {
                        return;
                    }

                    for (int i = 0; i < latestEntry.itemCount; i++) {
                        if (latestEntry.trees[i].stillExists == true) {
                            latestEntry.trees[i].CheckTreeStillExists();
                        }
                    }
                    break;
                }
                default: {
                    return;
                }
            }
        }

        public UndoManager() {

        }
    }
}