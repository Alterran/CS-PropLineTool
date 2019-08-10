using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.Globalization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using PropLineTool.Math;
using PropLineTool.Parameters;
using PropLineTool.Utility;
using PropLineTool.Utility.ErrorChecking;
using PropLineTool.Undo;
using PropLineTool.Settings;

//debug only
//using PropLineTool.DebugUtils;

//resolves the ambiguity between UnityEngine.Debug and SystemDiagnostics.Debug (:
using Debug = UnityEngine.Debug;

namespace PropLineTool {
    //Key Presses
    public struct KeyPressEvent {
        //signal events
        public bool _mouseDown;
        public bool _mouseUp;
        public bool _keyDown;
        public bool _keyUp;

        //mouse events
        public bool _leftClick;
        public bool _rightClick;

        //mouseOnly events
        public bool _leftClickOnly;
        public bool _rightClickOnly;

        //mouseUp events
        public bool _leftClickRelease;
        public bool _rightClickRelease;

        //keyboard events
        public bool _alt;
        public bool _ctrl;
        public bool _shift;
        public bool _isModifier;
        public bool _enterKey;
        public bool _ctrlEnter;
        public bool _Z;
        public bool _ctrlZ;
        public bool _esc;

        //keyUp events
        public bool _altRelease;
        public bool _ctrlRelease;
        public bool _shiftRelease;

        //single modifier events
        public bool _altOnly;
        public bool _ctrlOnly;
        public bool _shiftOnly;

        //hybrid events
        public bool _altLeftClick;
        public bool _ctrlLeftClick;
        public bool _shiftLeftClick;
        public bool _altOnlyLeftClick;
        public bool _ctrlOnlyLeftClick;
        public bool _shiftOnlyLeftClick;

        private void Set(Event e) {
            //signals
            _mouseDown = (e.type == EventType.MouseDown);
            _mouseUp = (e.type == EventType.MouseUp);
            _keyDown = (e.type == EventType.KeyDown);
            _keyUp = (e.type == EventType.KeyUp);

            //mouse events
            _leftClick = ((e.button == 0) && _mouseDown);
            _rightClick = ((e.button == 1) && _mouseDown);

            //mouse only events
            _leftClickOnly = (_leftClick && !_isModifier);
            _rightClickOnly = (_rightClick && !_isModifier);

            //mouseUp events
            _leftClickRelease = ((e.button == 0) && _mouseUp);
            _rightClickRelease = ((e.button == 1) && _mouseUp);

            //keyboard events
            _alt = (e.alt && _keyDown);
            _ctrl = (e.control && _keyDown);
            _shift = (e.shift && _keyDown);
            _isModifier = (_alt || _ctrl || _shift);
            _enterKey = (e.keyCode == KeyCode.KeypadEnter && _keyDown);
            _ctrlEnter = (_ctrl && _enterKey);
            _Z = (e.keyCode == KeyCode.Z && _keyDown);
            _ctrlZ = (_ctrl && _Z);
            _esc = e.keyCode == KeyCode.Escape;

            //keyUp events
            _altRelease = (e.alt && _keyUp);
            _ctrlRelease = (e.control && _keyUp);
            _shiftRelease = (e.shift && _keyUp);

            //single modifier events
            _altOnly = (_alt && !_ctrl && !_shift);
            _ctrlOnly = (_ctrl && !_alt && !_shift);
            _shiftOnly = (_shift && !_alt && !_ctrl);

            //hybrid events
            _altOnlyLeftClick = (_alt && _leftClick && !_ctrl && !_shift);
            _ctrlOnlyLeftClick = (_ctrl && _leftClick && !_alt && !_shift);
            _shiftOnlyLeftClick = (_shift && _leftClick && !_alt && !_ctrl);
        }

        /// <summary>
        /// Sets up the keypress event with a fix for the modifiers (ctrl, shift, alt).
        /// </summary>
        /// <param name="e"></param>
        public void Set3(Event e) {
            Set(e);

            //keyboard events
            _alt = (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt));
            _ctrl = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
            _shift = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
            _isModifier = (_alt || _ctrl || _shift);
            _enterKey = Input.GetKey(KeyCode.Return);
            _ctrlEnter = (_ctrl && _enterKey);

            //keyUp events
            _altRelease = (Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt));
            _ctrlRelease = (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl));
            _shiftRelease = (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift));

            //single modifier events
            _altOnly = (_alt && !_ctrl && !_shift);
            _ctrlOnly = (_ctrl && !_alt && !_shift);
            _shiftOnly = (_shift && !_alt && !_ctrl);

            //hybrid events
            _altLeftClick = (_alt && _leftClick);
            _ctrlLeftClick = (_ctrl && _leftClick);
            _shiftLeftClick = (_shift && _leftClick);
            _altOnlyLeftClick = (_alt && _leftClick && !_ctrl && !_shift);
            _ctrlOnlyLeftClick = (_ctrl && _leftClick && !_alt && !_shift);
            _shiftOnlyLeftClick = (_shift && _leftClick && !_alt && !_ctrl);
        }

        public void Reset() {
            //signals
            _mouseDown = false;
            _mouseUp = false;
            _keyDown = false;
            _keyUp = false;

            //mouse events
            _leftClick = false;
            _rightClick = false;

            //mouse only events
            _leftClickOnly = false;
            _rightClickOnly = false;

            //mouseUp events
            _leftClickRelease = false;
            _rightClickRelease = false;

            //keyboard events
            _alt = false;
            _ctrl = false;
            _shift = false;
            _enterKey = false;
            _ctrlEnter = false;
            _ctrlZ = false;
            _esc = false;

            //single modifier events
            _altOnly = false;
            _ctrlOnly = false;
            _shiftOnly = false;

            //hybrid events
            _altLeftClick = false;
            _ctrlLeftClick = false;
            _shiftLeftClick = false;
            _altOnlyLeftClick = false;
            _ctrlOnlyLeftClick = false;
            _shiftOnlyLeftClick = false;
        }
    }

    //where the *magic* happens!
    public class PropLineTool : ToolBase {
        //debug
        //PerformanceMeter _DEBUG_OnToolUpdate = new PerformanceMeter("OnToolUpdate", "PropLineTool");
        //PerformanceMeter _DEBUG_OnToolLateUpdate = new PerformanceMeter("OnToolLateUpdate", "PropLineTool");

        //class instance
        public static PropLineTool instance;

        //SnappingMode definition
        public enum SnapMode {
            Off = 0,
            Objects = 1,
            ZoneLines = 2
        }
        public static PropLineTool.SnapMode m_snapMode;

        //FenceMode definition
        private static bool m_fenceMode = false;
        public static bool fenceMode {
            get {
                return m_fenceMode;
            }
            set {
                bool _oldValue = m_fenceMode;
                m_fenceMode = value;

                if (value != _oldValue) {
                    OnFenceModeChanged(value);
                }

                if (value == true) {
                    placementCalculator.angleOffset = 0f;
                    hoverAngle = 0f;
                    placementCalculator.SetDefaultSpacing();
                    placementCalculator.angleMode = PlacementCalculator.AngleMode.Dynamic;
                } else {
                    placementCalculator.angleSingle = 0f;
                    placementCalculator.SetDefaultSpacing();
                }
            }
        }
        //FenceMode changed event
        public static event VoidObjectPropertyChangedEventHandler<bool> eventFenceModeChanged;
        protected static void OnFenceModeChanged(bool state) {
            eventFenceModeChanged?.Invoke(state);
        }

        //DrawMode definition
        public enum DrawMode {
            Single = 0,
            Straight = 1,
            Curved = 2,
            Freeform = 3,
            Circle = 4
        }
        //drawmode static member
        private static PropLineTool.DrawMode m_drawMode = DrawMode.Single;
        public static PropLineTool.DrawMode drawMode {
            get {
                return m_drawMode;
            }
            set {
                PropLineTool.DrawMode _oldValue = m_drawMode;
                m_drawMode = value;

                if (value != _oldValue) {
                    OnDrawModeChanged(value);
                }
            }
        }
        //drawmode changed event
        public static event VoidObjectPropertyChangedEventHandler<PropLineTool.DrawMode> eventDrawModeChanged;
        protected static void OnDrawModeChanged(PropLineTool.DrawMode drawMode) {
            eventDrawModeChanged?.Invoke(drawMode);

            if (activeState == ActiveState.MaxFillContinue) {
                placementCalculator.UpdateItemPlacementInfo();
            }
        }

        //LockingMode definition
        public enum LockingMode {
            Off = 0,
            Lock = 1
        }
        public static PropLineTool.LockingMode m_lockingMode;
        private static PropLineTool.LockingMode m_wasLockingMode;

        //Backup Values for Locking
        private ControlPoint[] lockBackupControlPoints = new ControlPoint[3];
        private float lockBackupSpacing = 8f;
        private float lockBackupAngleSingle = 0f;
        private float lockBackupAngleOffset = 0f;
        private float lockBackupItemSecondAngle = 0f;
        private Vector3 lockBackupCachedPosition = Vector3.zero;
        private Vector3 lockBackupItemDirection = Vector3.right;
        private float lockBackupItemwiseT = 0f;

        //Hover Objects
        public enum HoverState {
            Unbound,
            SpacingLocus,
            AngleLocus,
            ControlPointFirst,
            ControlPointSecond,
            ControlPointThird,
            Curve,
            ItemwiseItem
        }
        private static HoverState m_hoverState = HoverState.Unbound;
        protected static HoverState hoverState {
            get {
                return m_hoverState;
            }
            set {
                m_hoverState = value;
            }
        }
        //Hover Render Radii
        public static float hoverPointDiameter = 1.5f;
        public static float hoverAngleLocusDiameter = 10f;
        public static float hoverPointDistanceThreshold = 1.5f;
        public static float hoverCurveDistanceThreshold = 1f;
        public static float hoverItemwiseCurveDistanceThreshold = 12f;
        //Hovered Angle
        public static float hoverAngle = 0f;
        //Hovered Curve Position
        private static float hoverCurveT = 0f;
        //Hovered Curve Position for Itemwise Placement
        private static float hoverItemwiseT = 0f;
        //Index for rendering spacing locus and angle locus around
        private static int hoverItemPositionIndex {
            get {
                switch (controlMode) {
                    case ControlMode.Itemwise: {
                        if (fenceMode) {
                            return PlacementCalculator.ITEMWISE_FENCE_INDEX_START;
                        } else {
                            return PlacementCalculator.ITEMWISE_INDEX;
                        }
                    }
                    default: {
                        return 1;
                    }
                }
            }
        }
        //Index for rendering angle locus around
        private static int hoverItemAngleCenterIndex {
            get {
                switch (controlMode) {
                    case ControlMode.Itemwise: {
                        return PlacementCalculator.ITEMWISE_INDEX;
                    }
                    default: {
                        return 1;
                    }
                }
            }
        }

        //ControlMode definition
        public enum ControlMode {
            Itemwise = 0,
            Spacing = 1
        }
        private static PropLineTool.ControlMode m_controlMode = ControlMode.Spacing;
        public static PropLineTool.ControlMode controlMode {
            get {
                return m_controlMode;
            }
            set {
                PropLineTool.ControlMode _oldValue = m_controlMode;
                m_controlMode = value;

                if (value != _oldValue) {
                    OnControlModeChanged(value);
                }
            }
        }
        public static event VoidObjectPropertyChangedEventHandler<PropLineTool.ControlMode> eventControlModeChanged;
        protected static void OnControlModeChanged(PropLineTool.ControlMode controlMode) {
            //send out event signal
            eventControlModeChanged?.Invoke(controlMode);
        }

        //ActiveState definition
        public enum ActiveState {
            Undefined = 0,
            CreatePointFirst = 1,
            CreatePointSecond = 2,
            CreatePointThird = 3,
            LockIdle = 10,
            MovePointFirst = 11,
            MovePointSecond = 12,
            MovePointThird = 13,
            MoveSegment = 14,
            ChangeSpacing = 15,
            ChangeAngle = 16,
            ItemwiseLock = 30,
            MoveItemwiseItem = 31,
            MaxFillContinue = 40
        }
        private static PropLineTool.ActiveState m_activeState = ActiveState.CreatePointFirst;
        private static PropLineTool.ActiveState activeState {
            get {
                return m_activeState;
            }
            set {
                ActiveState _oldValue = m_activeState;
                m_activeState = value;
                if (value != _oldValue) {
                    OnActiveStateChanged(value);
                }
            }
        }
        public static event VoidObjectPropertyChangedEventHandler<ActiveState> eventActiveStateChanged;
        private static void OnActiveStateChanged(ActiveState state) {
            eventActiveStateChanged?.Invoke(state);
        }

        public bool IsActiveStateAnItemRenderState() {
            bool _result = false;

            switch (drawMode) {
                case DrawMode.Straight:
                case DrawMode.Circle: {
                    switch (activeState) {
                        case ActiveState.CreatePointFirst: {
                            _result = false;
                            break;
                        }
                        case ActiveState.CreatePointSecond: {
                            _result = true;
                            break;
                        }
                        case ActiveState.CreatePointThird:
                        case ActiveState.LockIdle:
                        case ActiveState.MovePointFirst:
                        case ActiveState.MovePointSecond:
                        case ActiveState.MovePointThird:
                        case ActiveState.MoveSegment:
                        case ActiveState.ChangeSpacing:
                        case ActiveState.ChangeAngle:
                        case ActiveState.ItemwiseLock:
                        case ActiveState.MoveItemwiseItem:
                        case ActiveState.MaxFillContinue: {
                            _result = true;
                            break;
                        }
                        default:
                            break;
                    }
                    break;
                }
                case DrawMode.Curved:
                case DrawMode.Freeform: {
                    switch (activeState) {
                        case ActiveState.CreatePointFirst:
                        case ActiveState.CreatePointSecond: {
                            _result = false;
                            break;
                        }
                        case ActiveState.CreatePointThird: {
                            _result = true;
                            break;
                        }
                        case ActiveState.LockIdle:
                        case ActiveState.MovePointFirst:
                        case ActiveState.MovePointSecond:
                        case ActiveState.MovePointThird:
                        case ActiveState.MoveSegment:
                        case ActiveState.ChangeSpacing:
                        case ActiveState.ChangeAngle:
                        case ActiveState.MaxFillContinue: {
                            _result = true;
                            break;
                        }
                        default: {
                            _result = false;
                            break;
                        }
                    }
                    break;
                }
                default: {
                    _result = false;
                    break;
                }
            }


            return _result;
        }

        //KeyPressEvent
        private static KeyPressEvent m_keyPressEvent;

        //ObjectMode definition
        public enum ObjectMode {
            Undefined = 0,
            Props = 1,
            Trees = 2
        }
        private static ObjectMode m_objectMode = ObjectMode.Undefined;//same as itemMode -> pick one!
        public static PropLineTool.ObjectMode objectMode {
            get {
                return m_objectMode;
            }
            set {
                ObjectMode _oldValue = m_objectMode;
                m_objectMode = value;

                if (value != _oldValue) {
                    OnObjectModeChanged(value);
                }
            }
        }
        public static event VoidObjectPropertyChangedEventHandler<ObjectMode> eventObjectModeChanged;
        private static void OnObjectModeChanged(ObjectMode mode) {
            eventObjectModeChanged?.Invoke(mode);

            //call before UpdateItemPlacementInfo()
            if (userSettingsControlPanel.autoDefaultSpacing == true) {
                placementCalculator.SetDefaultSpacing();
            }

            placementCalculator.UpdateItemPlacementInfo();

        }

        //control points
        public struct ControlPoint {
            public Vector3 m_position;
            public Vector3 m_direction;
            public bool m_outside;

            public void Clear() {
                m_position = Vector3.zero;
                m_direction = Vector3.zero;
                m_outside = false;
            }
        }

        //used to determine if control points are visible
        public bool IsOneControlPointVisible() {
            bool _result = false;

            RenderManager.CameraInfo _cameraInfo = RenderManager.instance.CurrentCameraInfo;
            if (_cameraInfo != null) {
                Vector3 _p0 = m_controlPoints[0].m_position;
                Vector3 _p1 = m_controlPoints[1].m_position;
                Vector3 _p2 = m_controlPoints[2].m_position;

                float _radius = hoverPointDiameter;

                switch (activeState) {
                    case ActiveState.CreatePointFirst: {
                        _result = _cameraInfo.Intersect(_p0, _radius);
                        break;
                    }
                    case ActiveState.CreatePointSecond: {
                        _result = _cameraInfo.Intersect(_p1, _radius) || _cameraInfo.Intersect(_p0, _radius);
                        break;
                    }
                    case ActiveState.CreatePointThird: {
                        switch (drawMode) {
                            case DrawMode.Straight:
                            case DrawMode.Circle: {
                                _result = _cameraInfo.Intersect(_p1, _radius) || _cameraInfo.Intersect(_p0, _radius);
                                break;
                            }
                            case DrawMode.Curved:
                            case DrawMode.Freeform: {
                                _result = _cameraInfo.Intersect(_p2, _radius) || _cameraInfo.Intersect(_p1, _radius) || _cameraInfo.Intersect(_p0, _radius);
                                break;
                            }
                            default: {
                                break;
                            }
                        }
                        break;
                    }
                    case ActiveState.LockIdle:
                    case ActiveState.MovePointFirst:
                    case ActiveState.MovePointSecond:
                    case ActiveState.MovePointThird:
                    case ActiveState.MoveSegment:
                    case ActiveState.ChangeSpacing:
                    case ActiveState.ChangeAngle:
                    case ActiveState.ItemwiseLock:
                    case ActiveState.MoveItemwiseItem:
                    case ActiveState.MaxFillContinue: {
                        switch (drawMode) {
                            case DrawMode.Straight:
                            case DrawMode.Circle: {
                                _result = _cameraInfo.Intersect(_p1, _radius) || _cameraInfo.Intersect(_p0, _radius);
                                break;
                            }
                            case DrawMode.Curved:
                            case DrawMode.Freeform: {
                                _result = _cameraInfo.Intersect(_p2, _radius) || _cameraInfo.Intersect(_p1, _radius) || _cameraInfo.Intersect(_p0, _radius);
                                break;
                            }
                            default: {
                                break;
                            }
                        }
                        break;
                    }
                    default: {
                        break;
                    }
                }
            }

            return _result;
        }

        //maybe this should be in the userParameters struct
        public static bool m_useCOBezierMethod = true; //if true, rounds out tight re-curves (or tight curves)

        //Geometry
        //   Main
        private static Segment3 m_mainSegment;
        private static Bezier3 m_mainBezier;
        private static Circle3XZ m_mainCircle;
        //   Secondary
        public static Segment3 m_mainArm1;
        public static Segment3 m_mainArm2;
        public static Circle3XZ m_rawCircle;
        //   Trinary
        /// <summary>
        /// Angle (in Radians) between both arms of the elbow to the bezier curve.
        /// </summary>
        private static float m_mainElbowAngle;  //in radians

        //Item Placement Info
        public const int MAX_ITEM_ARRAY_LENGTH = 256;
        public struct ItemPlacementInfo {
            //position stuff
            public float t; //where on the curve it is located (endpoint location for fence mode)
            public Vector3 position;
            public Vector3 itemDirection;
            public Vector3 offsetDirection;   //for radial left/right -/+ offset      //it is perpendicular to and rotated -90deg from m_itemDirection
            /// <summary>
            /// Angle in radians.
            /// </summary>
            public float angle;

            //position correction
            private Vector3 m_centerCorrection;
            /// <summary>
            /// Set only after angle and position have been set.
            /// </summary>
            private Vector3 centerCorrection {
                set {
                    m_centerCorrection = value;
                }
            }
            /// <summary>
            /// Only use for rendering geometry and placing items!
            /// </summary>
            public Vector3 meshPosition {
                get {
                    if (userSettingsControlPanel.useMeshCenterCorrection) {
                        return this.position + this.m_centerCorrection;
                    } else {
                        return this.position;
                    }
                }
            }
            private void CalculateCenterCorrection(Vector3 orthogonalCenterCorrection) {
                if (!PropLineTool.userSettingsControlPanel.useMeshCenterCorrection) {
                    this.centerCorrection = Vector3.zero;
                    return;
                }
                if (orthogonalCenterCorrection.magnitude == 0f) {
                    this.centerCorrection = Vector3.zero;
                    return;
                }

                Vector3 _centerCorrection = orthogonalCenterCorrection;
                //use negative angle since Unity is left-handed / CW rotation
                Quaternion _rotation = Quaternion.AngleAxis(-this.angle * Mathf.Rad2Deg, Vector3.up);

                _centerCorrection = _rotation * _centerCorrection;

                this.centerCorrection = _centerCorrection;
            }

            //prop
            public ushort propID;
            public Color color;
            private ushort propInfoIndex;
            /// <summary>
            /// Set only after angle and position have been set.
            /// </summary>
            public PropInfo propInfo {
                get {
                    return PrefabCollection<PropInfo>.GetPrefab((uint)this.propInfoIndex);
                }
                set {
                    this.propInfoIndex = (ushort)Mathf.Clamp(value.m_prefabDataIndex, 0, 65535);

                    Vector3 _centerCorrectionOrthogonal = new Vector3();
                    if (value.IsMeshCenterOffset(true, out _centerCorrectionOrthogonal)) {
                        CalculateCenterCorrection(_centerCorrectionOrthogonal);
                    } else {
                        this.m_centerCorrection = Vector3.zero;
                    }
                }
            }
            //tree
            public uint treeID;
            public float brightness;
            private ushort treeInfoIndex;
            /// <summary>
            /// Set only after angle and position have been set.
            /// </summary>
            public TreeInfo treeInfo {
                get {
                    return PrefabCollection<TreeInfo>.GetPrefab((uint)this.treeInfoIndex);
                }
                set {
                    this.treeInfoIndex = (ushort)Mathf.Clamp(value.m_prefabDataIndex, 0, 65535);

                    Vector3 _centerCorrectionOrthogonal = new Vector3();
                    if (value.IsMeshCenterOffset(true, out _centerCorrectionOrthogonal)) {
                        CalculateCenterCorrection(_centerCorrectionOrthogonal);
                    } else {
                        this.m_centerCorrection = Vector3.zero;
                    }
                }
            }
            //prop and tree
            public float scale;

            //error checking
            public bool isValidPlacement;
            public ItemCollisionType collisionFlags;



            //use whenever curve changes (tight->round or ->extended or ->shallow etc; whenever a control point is moved)
            public void SetTAndPosition(float t) {
                this.t = t;
                switch (drawMode) {
                    case DrawMode.Straight: {
                        position = Math.MathPLT.LinePosition(m_mainSegment, t);
                        break;
                    }
                    case DrawMode.Curved:
                    case DrawMode.Freeform: {
                        position = m_mainBezier.Position(t);
                        break;
                    }
                    case DrawMode.Circle: {
                        position = m_mainCircle.Position(t);
                        break;
                    }
                    default: {
                        //do nothing
                        break;
                    }
                }
            }

            public void SetTAndPosition(float t, Vector3 position) {
                this.t = t;
                this.position = position;
            }

            public void SetDirectionsXZ(Vector3 itemDirection) {
                Vector3 _itemDir = itemDirection;
                _itemDir.y = 0f;
                _itemDir.Normalize();
                this.itemDirection = _itemDir;
                CalculateOffsetDirectionXZ();
            }

            public void CalculateOffsetDirectionXZ() {
                Vector3 _offsetDir = new Vector3 {
                    x = itemDirection.z,
                    z = -itemDirection.x
                };
                offsetDirection = _offsetDir;
            }



        }
        private static ItemPlacementInfo[] m_placementInfo = new ItemPlacementInfo[MAX_ITEM_ARRAY_LENGTH];
        private static Vector3[] m_fenceEndPoints = new Vector3[MAX_ITEM_ARRAY_LENGTH + 1];
        private static int[] m_randInts = new int[MAX_ITEM_ARRAY_LENGTH];
        public static bool GetRandIntFromIndex(int index, out int randomInteger) {
            randomInteger = 0;

            bool result = false;
            if (PlacementCalculator.IsIndexWithinBounds(index, false)) {
                result = true;
            } else {
                result = false;
            }

            index = Mathf.Clamp(index, 0, MAX_ITEM_ARRAY_LENGTH);
            randomInteger = m_randInts[index];

            return result;
        }
        /// <summary>
        /// Reseeds or first populates the random integer array.
        /// </summary>
        /// <param name="min">Minimum integer value. Generally 0.</param>
        /// <param name="max">Maximum integer value. Generally 10,000.</param>
        public static void PopulateRandIntArray(int min, int max) {
            Randomizer _randomizer = new Randomizer((ulong)DateTime.Now.Ticks);
            for (int i = 0; i < m_randInts.Length; i++) {
                m_randInts[i] = _randomizer.Int32(min, max);
            }
        }

        //becomes true as soon as alt-click is pressed, otherwise false
        private static bool m_isCopyPlacing = false;
        public static bool isCopyPlacing {
            get {
                return m_isCopyPlacing;
            }
            private set {
                m_isCopyPlacing = value;
            }
        }

        //methods to calculate placement
        public class PlacementCalculator {
            private int m_itemCount;
            private float m_spacingSingle = 8f;
            private float m_angleSingle = 0f;     //absolute angle, in radians
            private float m_angleOffset = 0f;   //absolute angle, in radians
            private float m_tolerance = 0.001f;
            public float tolerance {
                get {
                    return m_tolerance;
                }
            }

            public const int ITEMWISE_INDEX = 0;
            public const int ITEMWISE_FENCE_INDEX_START = 0;
            public const int ITEMWISE_FENCE_INDEX_END = 1;

            //170527: now many in one
            private SegmentState m_segmentState = new SegmentState();
            public SegmentState segmentState {
                get {
                    return m_segmentState;
                }
                private set {
                    m_segmentState = value;
                }
            }

            public ItemPlacementInfo finalItem {
                get {
                    return m_placementInfo[Mathf.Clamp(m_itemCount - 1, 0, MAX_ITEM_ARRAY_LENGTH - 1)];
                }
            }
            /// <summary>
            /// Returns the position of the final item in the segment, or the far endpoint of the final item for fenceMode enabled.
            /// </summary>
            public Vector3 finalItemPosition {
                get {
                    if (fenceMode) {
                        return GetFenceEndpoint(m_itemCount);
                    } else {
                        return GetItemPosition(m_itemCount - 1);
                    }
                }
            }
            public ItemPlacementInfo initialItem {
                get {
                    return m_placementInfo[0];
                }
            }
            /// <summary>
            /// Returns the position of the initial item in the segment, or the close endpoint of the initial item for fenceMode enabled.
            /// </summary>
            public Vector3 initialItemPosition {
                get {
                    if (fenceMode) {
                        return GetFenceEndpoint(0);
                    } else {
                        return GetItemPosition(0);
                    }
                }
            }

            private float m_assetModelX;
            private float m_assetModelZ;
            private float m_assetWidth; //m_assetWidth < m_assetLength always
            private float m_assetLength;

            private PropInfo m_propInfo;
            public PropInfo propInfo {
                get {
                    return m_propInfo;
                }
                set {
                    m_propInfo = value;
                }
            }
            private TreeInfo m_treeInfo;
            public TreeInfo treeInfo {
                get {
                    return m_treeInfo;
                }
                set {
                    m_treeInfo = value;
                }
            }

            //randomizer stuff
            private const int SEED_INT = 8675309;
            public static Randomizer randomizerFresh {
                get {
                    switch (objectMode) {
                        case ObjectMode.Props: {
                            ushort seed = Singleton<PropManager>.instance.m_props.NextFreeItem();
                            return new Randomizer(seed);
                        }
                        case ObjectMode.Trees: {
                            uint seed = Singleton<TreeManager>.instance.m_trees.NextFreeItem();
                            return new Randomizer(seed);
                        }
                        default: {
                            return new Randomizer(SEED_INT);
                        }
                    }
                }
            }
            public static Randomizer RandomizerNextRandom(Randomizer seedRandomizer) {
                switch (objectMode) {
                    case ObjectMode.Props: {

                        ushort seed = Singleton<PropManager>.instance.m_props.NextFreeItem(ref seedRandomizer);
                        return new Randomizer(seed);
                    }
                    case ObjectMode.Trees: {
                        uint seed = Singleton<TreeManager>.instance.m_trees.NextFreeItem(ref seedRandomizer);
                        return new Randomizer(seed);
                    }
                    default: {
                        return new Randomizer(SEED_INT);
                    }
                }
            }
            //very bad things
            //private static Randomizer m_randomizer = new Randomizer(SEED_INT);
            //public static Randomizer randomizerFresh
            //{
            //    get
            //    {
            //        switch (objectMode)
            //        {
            //            case ObjectMode.Props:
            //                {
            //                    ushort seed = Singleton<PropManager>.instance.m_props.NextFreeItem(ref m_randomizer);
            //                    return new Randomizer((int)seed); //casted to int in PropTool
            //                }
            //            case ObjectMode.Trees:
            //                {
            //                    uint seed = Singleton<TreeManager>.instance.m_trees.NextFreeItem(ref m_randomizer);
            //                    return new Randomizer(seed); //not casted in TreeTool
            //                }
            //            default:
            //                {
            //                    return new Randomizer(SEED_INT);
            //                }
            //        }
            //    }
            //}



            //base mod params
            //spacing and angle getters and setters
            internal float spacingSingle {
                get {
                    return m_spacingSingle;
                }
                set {
                    float _oldValue = m_spacingSingle;

                    value = Mathf.Clamp(value, UserParameters.SPACING_MIN, UserParameters.SPACING_MAX);
                    m_spacingSingle = value;

                    if (value != _oldValue) {
                        OnSpacingSingleChanged(value);
                    }
                }
            }
            /// <summary>
            /// Angle in radians.
            /// </summary>
            internal float angleSingle {
                get {
                    return m_angleSingle;
                }
                set {
                    float _oldValue = m_angleSingle;

                    value %= (2f * Mathf.PI);
                    m_angleSingle = value;

                    if (value != _oldValue) {
                        OnAngleOffsetChanged(value);
                    }
                }
            }
            /// <summary>
            /// Angle in radians.
            /// </summary>
            internal float angleOffset {
                get {
                    return m_angleOffset;
                }
                set {
                    float _oldValue = m_angleOffset;

                    value %= (2f * Mathf.PI);
                    m_angleOffset = value;

                    if (value != _oldValue) {
                        OnAngleOffsetChanged(value);
                    }
                }
            }
            public enum AngleMode {
                Dynamic,
                Single
            }
            internal AngleMode m_angleMode = AngleMode.Dynamic;
            internal AngleMode angleMode {
                get {
                    return m_angleMode;
                }
                set {
                    AngleMode _oldValue = m_angleMode;
                    m_angleMode = value;

                    if (value != _oldValue) {
                        OnAngleModeChanged(value);
                    }
                }
            }

            //base mod events
            public event VoidEventHandler eventBaseParameterChanged;
            public event ObjectPropertyChangedEventHandler<float> eventSpacingSingleChanged;
            public event ObjectPropertyChangedEventHandler<float> eventAngleSingleChanged;
            public event ObjectPropertyChangedEventHandler<float> eventAngleOffsetChanged;
            public event ObjectPropertyChangedEventHandler<AngleMode> eventAngleModeChanged;
            protected void OnBaseParameterChanged() {
                eventBaseParameterChanged?.Invoke();
                UpdateItemPlacementInfo(segmentState.isContinueDrawing, segmentState.keepLastOffsets);
            }
            protected void OnSpacingSingleChanged(float value) {
                //nullcheck added 161103 2234
                eventSpacingSingleChanged?.Invoke(this, value);

                OnBaseParameterChanged();
            }
            protected void OnAngleSingleChanged(float value) {
                eventAngleSingleChanged?.Invoke(this, value);

                OnBaseParameterChanged();
            }
            protected void OnAngleOffsetChanged(float value) {
                eventAngleOffsetChanged?.Invoke(this, value);

                OnBaseParameterChanged();
            }
            protected void OnAngleModeChanged(AngleMode mode) {
                eventAngleModeChanged?.Invoke(this, mode);

                OnBaseParameterChanged();
            }


            //length and width getters and setters
            public float getLength {
                get {
                    return m_assetLength;
                }
            }
            public float getWidth {
                get {
                    return m_assetWidth;
                }
            }

            //defaults getters and setters
            public float getDefaultSpacing {
                get {
                    return GetDefaultSpacing();
                }
            }

            //called on PropLineTool.OnEnable
            public void SetupPropPrefab(PropInfo propPrefab) {
                if (propPrefab == null) {
                    return;
                }

                m_assetModelX = 2f * propPrefab.m_mesh.bounds.extents.x;
                m_assetModelZ = 2f * propPrefab.m_mesh.bounds.extents.z;
                if (m_assetModelX < m_assetModelZ) {
                    m_assetWidth = m_assetModelX;
                    m_assetLength = m_assetModelZ;
                } else {
                    m_assetWidth = m_assetModelZ;
                    m_assetLength = m_assetModelX;
                }

                propInfo = propPrefab;
            }
            //called on PropLineTool.OnEnable
            public void SetupTreePrefab(TreeInfo treePrefab) {
                if (treePrefab == null) {
                    return;
                }

                m_assetModelX = 2f * treePrefab.m_mesh.bounds.extents.x;
                m_assetModelZ = 2f * treePrefab.m_mesh.bounds.extents.z;
                if (m_assetModelX < m_assetModelZ) {
                    m_assetWidth = m_assetModelX;
                    m_assetLength = m_assetModelZ;
                } else {
                    m_assetWidth = m_assetModelZ;
                    m_assetLength = m_assetModelX;
                }

                treeInfo = treePrefab;
            }

            public void GetPrefabData(PropInfo propInfo, TreeInfo treeInfo) {
                switch (objectMode) {
                    case ObjectMode.Props: {
                        SetupPropPrefab(propInfo);
                        break;
                    }
                    case ObjectMode.Trees: {
                        SetupTreePrefab(treeInfo);
                        break;
                    }
                }
            }


            public void Reset() //used like Awake()
            {
                m_tolerance = 0.001f;
                //ResetLastContinueParameters();
                //segmentState.newFenceEndpoint = Vector3.down;
                //segmentState.newFinalOffset = 0f;
                segmentState = new SegmentState();
            }

            public void SetContinueDrawing(bool continueDrawing) {
                segmentState.isContinueDrawing = continueDrawing;
            }

            internal Vector3 GetLastFenceEndpoint() {
                return segmentState.lastFenceEndpoint;
            }

            public static float GetCurveLength(Bezier3 curve) {
                float result = 0f;
                result = Math.MathPLT.CubicBezierArcLengthXZGauss12(curve, 0f, 1f);
                return result;
            }
            public static float GetCurveLength(Segment3 line) {
                float result = 0f;
                result = line.LengthXZ();
                return result;
            }
            public int GetItemCountActual() {
                int result = 0;
                result = Mathf.Clamp(m_itemCount, 0, MAX_ITEM_ARRAY_LENGTH);
                return result;
            }


            //called primarily in ProcessKeyInputImpl
            public bool FinalizeForPlacement(bool continueDrawing) {
                //new as of 160622
                m_itemCount = 0;

                return segmentState.FinalizeForPlacement(continueDrawing);
            }

            /// <summary>
            /// Used for Undo max-fill-continue.
            /// </summary>
            /// <param name="lastFinalOffset"></param>
            /// <param name="lastFenceEndpoint"></param>
            /// <returns></returns>
            internal bool RevertLastContinueParameters(float lastFinalOffset, Vector3 lastFenceEndpoint) {
                segmentState.RevertLastContinueParameters(lastFinalOffset, lastFenceEndpoint);

                //return FinalizeForPlacement(true);
                return true;
            }

            public void ResetLastContinueParameters() {
                segmentState.lastFenceEndpoint = Vector3.down;
                segmentState.lastFinalOffset = 0f;
                UpdateItemPlacementInfo();
            }

            public bool UpdateItemPlacementInfo() {
                return UpdateItemPlacementInfo(segmentState.isContinueDrawing, segmentState.keepLastOffsets);
            }

            public bool UpdateItemPlacementInfo(bool forceContinueDrawing) {
                return UpdateItemPlacementInfo(forceContinueDrawing, segmentState.keepLastOffsets);
            }

            //called primarily in OnToolLateUpdate
            //and in ProcessKeyInputImpl
            //*** CALL AFTER UPDATECURVES ***
            public bool UpdateItemPlacementInfo(bool forceContinueDrawing, bool forceKeepLastOffsets) {
                bool result = false;
                if (PropLineTool.isCopyPlacing) {
                    segmentState.keepLastOffsets = true;
                    result = CalculateAll(true);
                } else {
                    segmentState.keepLastOffsets = forceKeepLastOffsets;
                    result = CalculateAll(forceContinueDrawing || segmentState.isContinueDrawing);
                }

                segmentState.isContinueDrawing = forceContinueDrawing;

                return result;
            }

            private bool CalculateAll(bool continueDrawing) {
                //MOVED TO: CalculateAllPositions()
                //float _initialOffset = 0f;
                //Vector3 _lastFenceEndPoint = Vector3.down;

                //if (continueDrawing)
                //{
                //    _initialOffset = lastFinalOffset;
                //    _lastFenceEndPoint = lastFenceEndpoint;
                //}
                //else
                //{
                //    switch (drawMode)
                //    {
                //        default:
                //        case DrawMode.Straight:
                //            {
                //                _lastFenceEndPoint = PropLineTool.m_mainSegment.b;
                //                break; 
                //            }
                //    }
                //}

                int _userMaxCount = 256;
                int _count = Mathf.Min(MAX_ITEM_ARRAY_LENGTH, _userMaxCount);
                _count = Mathf.Clamp(_count, 0, _count);
                m_itemCount = _count;   //not sure about setting m_itemCount here, before CalculateAllPositions

                //original as of 161111 2308
                //if (CalculateAllPositions(_initialOffset, _lastFenceEndPoint))
                //new as of 161111 2308
                if (CalculateAllPositions(continueDrawing)) {

                    if (CalculateAllDirections()) {

                        CalculateAllAnglesBase();

                        SetAllItemPrefabInfos();

                        UpdatePlacementErrors();

                        return true;
                    }

                } else {
                    //Debug.Log("[PLT]: CalculateAllPositions returned false");

                    segmentState.maxItemCountExceeded = false;
                }
                return false;
            }

            public void TranslateAllPositions(Vector3 originalRefPoint, Vector3 finalRefPoint) {
                Vector3 _translation = finalRefPoint - originalRefPoint;
                //items
                for (int i = 0; i < m_itemCount; i++) {
                    IncrementPosition(i, _translation);
                }
                //fence endpoints
                for (int i = 0; i < m_itemCount + 1; i++) {
                    IncrementFenceEndpoint(i, _translation);
                }
            }
            public void TranslateAllPositions(Vector3 translation) {
                //items
                for (int i = 0; i < m_itemCount; i++) {
                    IncrementPosition(i, translation);
                }

                //fence endpoints
                for (int i = 0; i < m_itemCount + 1; i++) {
                    IncrementFenceEndpoint(i, translation);
                }
            }

            //================================  SPACING  ================================|================================================================

            public void SetDefaultSpacing() {
                spacingSingle = GetDefaultSpacing();
            }
            public float GetDefaultSpacing() {
                float _result = 8f;

                if (fenceMode == true) {
                    _result = DefaultSpacingFenceMode();
                } else {
                    _result = DefaultSpacingNonFence();
                }

                if (fenceMode == false && objectMode == ObjectMode.Props && _result < 2f) {
                    _result = 2f;
                }

                return _result;
            }
            public float GetAssetLength() {
                return this.m_assetLength;
            }
            public float GetAssetWidth() {
                return this.m_assetWidth;
            }

            private float DefaultSpacingNonFence() {
                float _scaleFactor = 1f;
                switch (objectMode) {
                    case ObjectMode.Props: {
                        if (m_assetLength < 4f && fenceMode == false) {
                            _scaleFactor = 2.2f;
                        }
                        break;
                    }
                    case ObjectMode.Trees: {
                        if (m_assetLength > 7f) {
                            _scaleFactor = 1.1f;
                        } else {
                            _scaleFactor = 2f;
                        }
                        break;
                    }
                    default: {
                        _scaleFactor = 2f;
                        break;
                    }
                }

                float _result = Mathf.Clamp(m_assetLength * _scaleFactor, 0f, UserParameters.SPACING_MAX);
                if (_result != 0f) {
                    return _result;
                } else {
                    return 8f;
                }
            }
            private float DefaultSpacingFenceMode() {
                //original: nearest half-number
                //float _result = Mathf.Clamp(Mathf.Round(m_assetLength) + Mathf.Round(2f * (m_assetLength % 0.5f)) * 0.5f, 0f, UserParameters.SPACING_MAX);
                //new: nearest whole-number
                float _result = Mathf.Clamp(Mathf.Round(m_assetLength), 0f, UserParameters.SPACING_MAX);
                if (_result != 0f) {
                    return _result;
                } else {
                    return 8f;
                }
            }

            //================================  POSITION  ================================|================================================================
            /// <summary>
            /// Calculates all item positions for all PLT modes and all control modes.
            /// </summary>
            /// <returns></returns>
            private bool CalculateAllPositions(bool continueDrawing) {
                bool _result = false;

                //moved from CalculateAll()
                float _initialOffset = 0f;
                Vector3 _lastFenceEndPoint = Vector3.down;

                if (continueDrawing) {
                    _initialOffset = segmentState.lastFinalOffset;
                    _lastFenceEndPoint = segmentState.lastFenceEndpoint;
                } else {
                    _lastFenceEndPoint = PropLineTool.m_mainSegment.b;
                }

                switch (PropLineTool.controlMode) {
                    case ControlMode.Itemwise: {
                        _result = CalculateItemwisePosition(spacingSingle, _initialOffset, _lastFenceEndPoint);
                        break;
                    }
                    case ControlMode.Spacing: {
                        _result = CalculateAllPositionsBySpacing(spacingSingle, _initialOffset, _lastFenceEndPoint);
                        break;
                    }
                    default: {
                        _result = false;
                        break;
                    }
                }

                return _result;
            }

            //do nothing with parameters for now
            /// <summary>
            /// Calculates one item position for Itemwise ControlMode.
            /// </summary>
            /// <param name="fencePieceLength">The length of one fence piece. (akin to spacing in ControlMode.Spacing)</param>
            /// <param name="initialOffset"></param>
            /// <param name="lastFenceEndpoint"></param>
            /// <returns></returns>
            private bool CalculateItemwisePosition(float fencePieceLength, float initialOffset, Vector3 lastFenceEndpoint) {
                if (!IsIndexWithinBounds(ITEMWISE_INDEX, false)) {
                    return false;
                }

                //set item count at beginning
                m_itemCount = 1;

                if (PropLineTool.fenceMode == true) //FenceMode = ON
                {
                    switch (PropLineTool.drawMode) {
                        // ====== STRAIGHT FENCE ======
                        case DrawMode.Straight: {
                            float _lengthFull = GetCurveLength(m_mainSegment);
                            float _speed = Math.MathPLT.LinearSpeedXZ(m_mainSegment);

                            float _mouseT = PropLineTool.hoverItemwiseT;
                            float _deltaT = fencePieceLength / _speed;

                            float _itemTStart = _mouseT;
                            float _sumT = _mouseT + _deltaT;

                            //check if out of bounds
                            if (_sumT > 1f && _lengthFull >= fencePieceLength) {
                                _itemTStart += (1f - _sumT);
                            }

                            float _itemTEnd = _itemTStart + _deltaT;

                            //calculate endpoints
                            Vector3 _positionStart = Math.MathPLT.LinePosition(m_mainSegment, _itemTStart);
                            Vector3 _positionEnd = Math.MathPLT.LinePosition(m_mainSegment, _itemTEnd);
                            SetFenceEndpoint(ITEMWISE_FENCE_INDEX_START, _positionStart);
                            SetFenceEndpoint(ITEMWISE_FENCE_INDEX_END, _positionEnd);

                            //then calculate midpoints
                            Vector3 _midpoint = m_mainSegment.b;
                            _midpoint = Vector3.Lerp(_positionStart, _positionEnd, 0.50f);
                            SetPosition(ITEMWISE_INDEX, _midpoint);

                            break;
                        }
                        // ====== CURVED/FREEFORM FENCE ======
                        case DrawMode.Curved:
                        case DrawMode.Freeform: {
                            float _lengthFull = GetCurveLength(m_mainBezier);

                            //early exit
                            if (fencePieceLength > _lengthFull) {
                                m_itemCount = 0;
                                return false;
                            }

                            float _mouseT = PropLineTool.hoverItemwiseT;
                            float _itemTStart = _mouseT;
                            float _itemTEnd = 1f;

                            Vector3 _mouseCurvePosition = m_mainBezier.Position(_mouseT);
                            if (!Math.MathPLT.CircleCurveFenceIntersectXZ(m_mainBezier, _itemTStart, fencePieceLength, m_tolerance, out _itemTEnd, false)) {

                            }

                            //check if out of bounds
                            if (_itemTEnd > 1f) {
                                //out of bounds? -> attempt to snap to d-end of curve
                                //invert the curve to go "backwards"
                                _itemTEnd = 0f;
                                Bezier3 _inverseBezier = m_mainBezier.Invert();
                                if (!Math.MathPLT.CircleCurveFenceIntersectXZ(_inverseBezier, _itemTEnd, fencePieceLength, m_tolerance, out _itemTStart, false)) {
                                    //failed to snap to d-end of curve
                                    m_itemCount = 0;
                                    return false;
                                } else {
                                    _itemTStart = 1f - _itemTStart;
                                    _itemTEnd = 1f - _itemTEnd;
                                }
                            }

                            //set fence endpoints
                            Vector3 _positionStart = m_mainBezier.Position(_itemTStart);
                            Vector3 _positionEnd = m_mainBezier.Position(_itemTEnd);
                            SetFenceEndpoint(ITEMWISE_FENCE_INDEX_START, _positionStart);
                            SetFenceEndpoint(ITEMWISE_FENCE_INDEX_END, _positionEnd);
                            //then set midpoint
                            Vector3 _midpoint = _positionStart;
                            _midpoint = Vector3.Lerp(_positionStart, _positionEnd, 0.50f);
                            SetPosition(ITEMWISE_INDEX, _midpoint);

                            break;
                        }
                        // ====== CIRCLE FENCE ======
                        case DrawMode.Circle: {
                            //early exit
                            if (PropLineTool.m_mainCircle.radius == 0f) {
                                m_itemCount = 0;
                                return false;
                            }
                            if (fencePieceLength > PropLineTool.m_mainCircle.diameter) {
                                m_itemCount = 0;
                                return false;
                            }

                            float _mouseT = PropLineTool.hoverItemwiseT;
                            float _itemTStart = _mouseT;

                            float _deltaT = PropLineTool.m_mainCircle.ChordDeltaT(fencePieceLength);
                            if (_deltaT <= 0f || _deltaT >= 1f) {
                                m_itemCount = 0;
                                return false;
                            }

                            float _itemTEnd = _itemTStart + _deltaT;

                            //set fence endpoints
                            Vector3 _positionStart = m_mainCircle.Position(_itemTStart);
                            Vector3 _positionEnd = m_mainCircle.Position(_itemTEnd);
                            SetFenceEndpoint(ITEMWISE_FENCE_INDEX_START, _positionStart);
                            SetFenceEndpoint(ITEMWISE_FENCE_INDEX_END, _positionEnd);
                            //then set midpoint
                            Vector3 _midpoint = _positionStart;
                            _midpoint = Vector3.Lerp(_positionStart, _positionEnd, 0.50f);
                            SetPosition(ITEMWISE_INDEX, _midpoint);

                            break;
                        }
                        default: {
                            m_itemCount = 0;
                            return false;
                        }
                    }


                } else //Non-fence mode
                  {
                    switch (PropLineTool.drawMode) {
                        // ====== STRAIGHT ======
                        case DrawMode.Straight: {
                            //Vector3 _position = m_mainSegment.Position(PropLineTool.hoverCurveT);
                            Vector3 _position = Math.MathPLT.LinePosition(m_mainSegment, PropLineTool.hoverItemwiseT);
                            SetTAndPosition(ITEMWISE_INDEX, PropLineTool.hoverItemwiseT, _position);
                            break;
                        }
                        // ====== CURVED/FREEFORM ======
                        case DrawMode.Curved:
                        case DrawMode.Freeform: {
                            Vector3 _position = m_mainBezier.Position(PropLineTool.hoverItemwiseT);
                            SetTAndPosition(ITEMWISE_INDEX, PropLineTool.hoverItemwiseT, _position);
                            break;
                        }
                        // ====== CIRCLE ======
                        case DrawMode.Circle: {
                            Vector3 _position = m_mainCircle.Position(PropLineTool.hoverItemwiseT);
                            SetTAndPosition(ITEMWISE_INDEX, PropLineTool.hoverItemwiseT, _position);
                            break;
                        }
                        default: {
                            m_itemCount = 0;
                            return false;
                        }
                    }
                }

                return true;
            }

            /// <summary>
            /// Calculates all item positions for all PLT modes and **SINGLE SPACING ONLY.**
            /// </summary>
            /// <param name="spacing"></param>
            /// <param name="initialOffset"></param>
            /// <param name="lastFenceEndpoint"></param>
            /// <returns></returns>
            private bool CalculateAllPositionsBySpacing(float spacing, float initialOffset, Vector3 lastFenceEndpoint) {
                int _numItems = 0;
                float _initialT = 0f;
                float _finalT = 0f;
                float _deltaT = 0f;

                int _numItemsRaw = 0;

                initialOffset = Mathf.Abs(initialOffset);

                //first early exit condition
                if (spacing == 0) {
                    m_itemCount = 0;
                    return false;
                }

                if (!PropLineTool.IsCurveLengthLongEnoughXZ()) {
                    m_itemCount = 0;
                    return false;
                }

                //   more early exit conditions
                if ((drawMode == DrawMode.Curved || drawMode == DrawMode.Freeform)) {
                    if (fenceMode == true)
                    //if (m_fenceMode == true && Vector3.Distance(m_mainBezier.Position(0.50f), m_mainBezier.a) < m_spacing)
                    {
                        //check if curve is unwieldly
                        //check if curve is too tight for convergence
                        if (m_mainElbowAngle * Mathf.Rad2Deg < 5f)     //if elbow angle is less than 5 degrees
                        {
                            m_itemCount = 0;
                            return false;
                        }
                    }
                }




                //begin code after passing early exit conditions

                if (fenceMode == true)   //FenceMode = ON
                {
                    switch (drawMode) {
                        // ====== STRAIGHT FENCE ======
                        case DrawMode.Straight: {
                            float _lengthFull = GetCurveLength(m_mainSegment);
                            float _speed = Math.MathPLT.LinearSpeedXZ(m_mainSegment);

                            float _lengthAfterFirst = segmentState.isMaxFillContinue ? _lengthFull - Mathf.Abs(initialOffset) : _lengthFull;

                            float _numItemsFloat = Mathf.Abs(_lengthAfterFirst / spacing);

                            _numItemsRaw = Mathf.FloorToInt(_numItemsFloat);
                            _numItems = Mathf.Min(m_itemCount, Mathf.Clamp(_numItemsRaw, 0, MAX_ITEM_ARRAY_LENGTH));

                            //add an extra item at the end if within 75% of spacing
                            bool _extraItem = false;
                            float _remainder = _lengthAfterFirst % spacing;
                            float _remFraction = _remainder / spacing;
                            if (_remFraction >= 0.75f && _numItems < MAX_ITEM_ARRAY_LENGTH) {
                                _numItems += 1;

                                _extraItem = true;
                            }

                            //If not MaxFillContinue:
                            //In straight fence mode, no segment-linking occurs
                            //   so we don't use initialOffset here
                            //the continuous draw resets the first control point to the last fence endpoint

                            //distance = speed * t
                            //t = distance / speed
                            _deltaT = spacing / _speed;

                            float _t = 0f;
                            Vector3 _position = Vector3.zero;

                            //Max Fill Continue
                            if (segmentState.isMaxFillContinue && initialOffset > 0f) {
                                _t = initialOffset / _lengthFull;
                            }

                            //calculate endpoints
                            for (int i = 0; i < _numItems + 1; i++) {
                                _position = Math.MathPLT.LinePosition(m_mainSegment, _t);
                                SetFenceEndpoint(i, _position);

                                _t += _deltaT;
                            }

                            //then calculate midpoints
                            Vector3 _midpoint = lastFenceEndpoint;
                            for (int i = 0; i < _numItems; i++) {
                                Vector3 _endp0 = GetFenceEndpoint(i);
                                Vector3 _endp1 = GetFenceEndpoint(i + 1);
                                _midpoint = Vector3.Lerp(GetFenceEndpoint(i), GetFenceEndpoint(i + 1), 0.50f);
                                SetPosition(i, _midpoint);
                            }

                            //linear fence fill
                            bool _realizedLinearFenceFill = false;
                            if (userSettingsControlPanel.linearFenceFill) {
                                //check conditions first
                                if (_numItems > 0 && _numItems < MAX_ITEM_ARRAY_LENGTH) {
                                    if (_numItems == 1) {
                                        if (_lengthFull > spacing) {
                                            _realizedLinearFenceFill = true;
                                        }
                                    } else {
                                        _realizedLinearFenceFill = true;
                                    }
                                }

                                //if conditions for linear fence fill are met
                                if (_realizedLinearFenceFill) {
                                    //account for extra item
                                    if (!_extraItem) {
                                        _numItems++;
                                    }

                                    Vector3 _p0 = m_mainSegment.a;
                                    Vector3 _p1 = m_mainSegment.b;
                                    _p0.y = 0f;
                                    _p1.y = 0f;

                                    SetFenceEndpoint(_numItems, _p1);

                                    Vector3 _localX = (_p1 - _p0).normalized;
                                    Vector3 _localZ = new Vector3(_localX.z, 0f, -1f * _localX.x);
                                    Vector3 _finalOffset = (0.00390625f * _localX) + (0.00390625f * _localZ);

                                    Vector3 _finalFenceMidpoint = _p1 + (0.5f * spacing) * ((_p0 - _p1).normalized);
                                    _finalFenceMidpoint += _finalOffset;    //correct for z-fighting
                                    SetPosition(_numItems - 1, _finalFenceMidpoint);

                                    //add _deltaT to account for subsequent subtraction of _deltaT
                                    _finalT = 1f + _deltaT;
                                }
                            }

                            _finalT = _t - _deltaT;
                            Vector3 _finalPos = MathPLT.LinePosition(m_mainSegment, _finalT);

                            //prep for MaxFillContinue
                            if (segmentState.isReadyForMaxContinue) {
                                segmentState.newFinalOffset = Vector3.Distance(m_mainSegment.a, _finalPos);
                            } else {
                                segmentState.newFinalOffset = Vector3.Distance(_finalPos, m_mainSegment.b);
                            }

                            break;
                        }
                        // ====== CURVED/FREEFORM FENCE ======
                        case DrawMode.Curved:
                        case DrawMode.Freeform: {
                            float _lengthFull = GetCurveLength(m_mainBezier);
                            float _lengthAfterFirst = segmentState.isMaxFillContinue ? _lengthFull - Mathf.Abs(initialOffset) : _lengthFull;

                            _numItemsRaw = Mathf.CeilToInt(_lengthAfterFirst / spacing);
                            _numItems = Mathf.Min(m_itemCount, Mathf.Clamp(_numItemsRaw, 0, MAX_ITEM_ARRAY_LENGTH));

                            //old 161111 2246
                            //if (_lengthFull < Vector3.Distance(m_mainBezier.a, m_mainBezier.d))
                            //new 161111 2246
                            if (spacing > _lengthFull) {
                                m_itemCount = 0;
                                return false;
                            }

                            if (_numItems > MAX_ITEM_ARRAY_LENGTH) {
                                _numItems = Mathf.Clamp(_numItems, 0, MAX_ITEM_ARRAY_LENGTH);
                            }

                            float _t = 0f;
                            float _penultimateT = 0f;

                            int _forLoopStart = 0;
                            Vector3 _position = lastFenceEndpoint;

                            //max fill continue
                            if (segmentState.isMaxFillContinue && initialOffset > 0f) {
                                _forLoopStart = 0;
                                MathPLT.StepDistanceCurve(m_mainBezier, 0f, initialOffset, tolerance, out _t);

                                goto label_endpointsForLoop;
                            }
                            //link curves in continuous draw
                            else if (initialOffset > 0f && lastFenceEndpoint != Vector3.down) {
                                //first continueDrawing if (1/4)
                                if (!SetFenceEndpoint(0, lastFenceEndpoint)) {
                                    _numItems = 0; //correct
                                    goto label_endpointsFinish;
                                }
                                //second continueDrawing if (2/4)
                                if (!Math.MathPLT.LinkCircleCurveFenceIntersectXZ(m_mainBezier, lastFenceEndpoint, spacing, m_tolerance, out _t, false)) {
                                    //could not link segments, so start at t = 0 instead
                                    _forLoopStart = 0;
                                    _t = 0f;
                                    goto label_endpointsForLoop;
                                }
                                _position = m_mainBezier.Position(_t);
                                //third continueDrawing if (3/4)
                                if (!SetFenceEndpoint(1, _position)) {
                                    _numItems = 0;
                                    goto label_endpointsFinish;
                                }

                                float _tFirstFencepoint = _t;

                                //fourth continueDrawing if (4/4)
                                if (!Math.MathPLT.CircleCurveFenceIntersectXZ(m_mainBezier, _t, spacing, m_tolerance, out _t, false)) {
                                    //failed to converge
                                    _numItems = 1;
                                    goto label_endpointsFinish;
                                }

                                _forLoopStart = 2;

                            } else {
                                //nothing here...
                            }

                        label_endpointsForLoop:

                            for (int i = _forLoopStart; i < _numItems + 1; i++) {
                                //this should be the first if (1/3)
                                //this is necessary for bendy fence mode since we didn't estimate count
                                if (_t > 1f) {
                                    _numItems = i - 1;
                                    goto label_endpointsFinish;
                                }

                                //second if (2/3)
                                _position = m_mainBezier.Position(_t);
                                if (!SetFenceEndpoint(i, _position)) {
                                    _numItems = i - 1;
                                    goto label_endpointsFinish;
                                }

                                //_finalT = _t;
                                _penultimateT = _t;

                                //third if (3/3)
                                if (!Math.MathPLT.CircleCurveFenceIntersectXZ(m_mainBezier, _t, spacing, m_tolerance, out _t, false)) {
                                    //failed to converge
                                    _numItems = i - 1;
                                    goto label_endpointsFinish;
                                }
                            }

                        //outside of for loop
                        label_endpointsFinish:
                            {
                                _numItems = Mathf.Clamp(_numItems, 0, MAX_ITEM_ARRAY_LENGTH);
                            }

                            _finalT = _t;

                            //then calculate midpoints
                            Vector3 _midpoint = this.segmentState.lastFenceEndpoint;
                            for (int i = 0; i < _numItems; i++) {
                                _midpoint = Vector3.Lerp(GetFenceEndpoint(i), GetFenceEndpoint(i + 1), 0.50f);
                                if (!SetPosition(i, _midpoint)) {
                                    _numItems = i;
                                    break;
                                }
                            }

                            //prep for MaxFillContinue
                            if (segmentState.isReadyForMaxContinue) {
                                segmentState.newFinalOffset = MathPLT.CubicBezierArcLengthXZGauss12(m_mainBezier, 0f, _penultimateT);
                            } else {
                                segmentState.newFinalOffset = MathPLT.CubicBezierArcLengthXZGauss04(m_mainBezier, _finalT, 1f);
                            }

                            break;
                        }
                        // ====== CIRCLE FENCE ======
                        case DrawMode.Circle: {
                            float _chordAngle = m_mainCircle.ChordAngle(spacing);

                            //early exit
                            if (_chordAngle <= 0f) {
                                _numItems = 0;
                                break;
                            }
                            if (_chordAngle > Mathf.PI) {
                                _numItems = 0;
                                break;
                            }
                            if (m_mainCircle.radius <= 0f) {
                                _numItems = 0;
                                break;
                            }

                            float _angleFull = 2f * Mathf.PI;

                            float _initialAngle = Mathf.Abs(initialOffset) / m_mainCircle.radius;
                            float _angleAfterFirst = segmentState.isMaxFillContinue ? _angleFull - _initialAngle : _angleFull;

                            if (userSettingsControlPanel.perfectCircles) {
                                _numItemsRaw = Mathf.RoundToInt(_angleAfterFirst / _chordAngle);
                                _numItems = Mathf.Min(m_itemCount, Mathf.Clamp(_numItemsRaw, 0, MAX_ITEM_ARRAY_LENGTH));
                            } else {
                                _numItemsRaw = Mathf.FloorToInt(_angleAfterFirst / _chordAngle);
                                _numItems = Mathf.Min(m_itemCount, Mathf.Clamp(_numItemsRaw, 0, MAX_ITEM_ARRAY_LENGTH));
                            }

                            _deltaT = m_mainCircle.ChordDeltaT(spacing);

                            //If No MaxFillContinue:
                            //In circle fence mode, no segment-linking occurs
                            //   so we don't use initialOffset here

                            float _t = 0f;
                            float _penultimateT = 0f;

                            //Max Fill Continue
                            if (segmentState.isMaxFillContinue && initialOffset > 0f) {
                                _t = m_mainCircle.DeltaT(initialOffset);
                                _penultimateT = _t;
                            }

                            Vector3 _position = m_mainCircle.Position(_t);
                            Vector3 _center = m_mainCircle.center;
                            Vector3 _radiusVector = _position - _center;

                            Quaternion _rotation = Quaternion.AngleAxis(-1f * _chordAngle * Mathf.Rad2Deg, Vector3.up);

                            //calculate endpoints
                            for (int i = 0; i < _numItems + 1; i++) {
                                _penultimateT = _t;

                                SetFenceEndpoint(i, _position);
                                _radiusVector = _rotation * _radiusVector;
                                _position = _center + _radiusVector;

                                _t += _deltaT;
                            }

                            //then calculate midpoints
                            Vector3 _midpoint = _position;
                            for (int i = 0; i < _numItems; i++) {
                                Vector3 _endp0 = GetFenceEndpoint(i);
                                Vector3 _endp1 = GetFenceEndpoint(i + 1);
                                _midpoint = Vector3.Lerp(GetFenceEndpoint(i), GetFenceEndpoint(i + 1), 0.50f);
                                SetPosition(i, _midpoint);
                            }

                            _finalT = _t;

                            //prep for MaxFillContinue
                            if (segmentState.isReadyForMaxContinue) {
                                segmentState.newFinalOffset = m_mainCircle.ArclengthBetween(0f, _penultimateT);
                            } else {
                                segmentState.newFinalOffset = m_mainCircle.ArclengthBetween(_t, 1f);
                            }


                            break;
                        }
                        default: {
                            _numItems = 0;
                            break;
                        }
                    }


                    if (_numItems > 0 && _numItems <= MAX_ITEM_ARRAY_LENGTH) {
                        Vector3 _finalEndpointPos = GetFenceEndpoint(_numItems);
                        segmentState.newFenceEndpoint = _finalEndpointPos;

                        //Vector3 _finalPos = GetItemPosition(_numItems - 1);
                        //segmentState.newFinalOffset = Vector3.Distance(_finalPos, m_mainSegment.b);
                    } else {
                        m_itemCount = 0;
                        return false;
                    }

                } else   //Non-fence mode
                  {
                    switch (drawMode) {
                        // ====== STRAIGHT ======
                        case DrawMode.Straight: {
                            float _lengthFull = GetCurveLength(m_mainSegment);
                            float _lengthAfterFirst = _lengthFull - initialOffset;
                            float _speed = Math.MathPLT.LinearSpeedXZ(m_mainSegment);

                            //use ceiling for non-fence, because the point at the beginning is an extra point
                            _numItemsRaw = Mathf.CeilToInt(_lengthAfterFirst / spacing);
                            _numItems = Mathf.Min(m_itemCount, Mathf.Clamp(_numItemsRaw, 0, MAX_ITEM_ARRAY_LENGTH));

                            if (_speed == 0) {
                                return false;
                            }

                            //distance = speed * t
                            //t = distance / speed
                            _deltaT = spacing / _speed;

                            float _t = 0f;
                            if (initialOffset > 0f) {
                                //calculate initial _t
                                _initialT = initialOffset / _speed;
                                _t = _initialT;
                            }

                            Vector3 _position = Vector3.zero;

                            for (int i = 0; i < _numItems; i++) {
                                _position = Math.MathPLT.LinePosition(m_mainSegment, _t);
                                SetTAndPosition(i, _t, _position);
                                _t += _deltaT;
                            }

                            Vector3 _finalPos = Vector3.zero;
                            if (!GetItemT(_numItems - 1, out _finalT) || !GetItemPosition(_numItems - 1, out _finalPos)) {
                                ResetLastContinueParameters();
                                _finalT = 1f;
                            } else {
                                if (segmentState.isReadyForMaxContinue) {
                                    segmentState.newFinalOffset = spacing + Vector3.Distance(m_mainSegment.a, _finalPos);
                                } else {
                                    segmentState.newFinalOffset = spacing - Vector3.Distance(_finalPos, m_mainSegment.b);
                                }

                            }
                            break;
                        }
                        // ====== CURVED/FREEFORM ======
                        case DrawMode.Curved:
                        case DrawMode.Freeform: {
                            if (m_mainArm1.Length() + m_mainArm2.Length() <= 0.01f) {
                                return false;
                            }

                            float _lengthFull = GetCurveLength(m_mainBezier);
                            float _lengthAfterFirst = _lengthFull - initialOffset;

                            //use ceiling for non-fence, because the point at the beginning is an extra point
                            _numItemsRaw = Mathf.CeilToInt(_lengthAfterFirst / spacing);
                            _numItems = Mathf.Min(m_itemCount, Mathf.Clamp(_numItemsRaw, 0, MAX_ITEM_ARRAY_LENGTH));

                            float _t = 0f;
                            if (initialOffset > 0f) {
                                //calculate initial _t
                                Math.MathPLT.StepDistanceCurve(m_mainBezier, 0f, initialOffset, m_tolerance, out _t);
                            }

                            Vector3 _position = Vector3.zero;

                            for (int i = 0; i < _numItems; i++) {
                                _position = m_mainBezier.Position(_t);
                                SetTAndPosition(i, _t, _position);
                                Math.MathPLT.StepDistanceCurve(m_mainBezier, _t, spacing, m_tolerance, out _t);
                            }

                            Vector3 _finalPos = Vector3.down;
                            if (!GetItemT(_numItems - 1, out _finalT) || !GetItemPosition(_numItems - 1, out _finalPos)) {
                                ResetLastContinueParameters();
                                _finalT = 1f;
                            } else {
                                if (segmentState.isReadyForMaxContinue) {
                                    segmentState.newFinalOffset = spacing + MathPLT.CubicBezierArcLengthXZGauss12(m_mainBezier, 0f, _finalT);
                                } else {
                                    segmentState.newFinalOffset = spacing - Math.MathPLT.CubicBezierArcLengthXZGauss04(m_mainBezier, _finalT, 1f);
                                }


                            }
                            break;
                        }
                        // ====== CIRCLE ======
                        case DrawMode.Circle: {
                            _deltaT = m_mainCircle.DeltaT(spacing);

                            //early exit
                            if (_deltaT <= 0f) {
                                _numItems = 0;
                                break;
                            }
                            if (_deltaT > 1f) {
                                _numItems = 0;
                                break;
                            }
                            if (m_mainCircle.radius <= 0f) {
                                _numItems = 0;
                                break;
                            }


                            float _t = 0f;
                            float _remainingSpace = m_mainCircle.circumference;


                            //if (activeState == ActiveState.MaxFillContinue || (activeState == ActiveState.LockIdle && segmentState.isMaxFillContinue))
                            if (segmentState.isMaxFillContinue) {
                                if (m_mainCircle.circumference > 0f) {
                                    _t = initialOffset / m_mainCircle.circumference;
                                    _remainingSpace -= initialOffset;
                                } else {
                                    _numItems = 0;
                                    break;
                                }
                            }

                            _initialT = _t;



                            //use ceiling for non-fence, because the point at the beginning is an extra point
                            _numItemsRaw = Mathf.CeilToInt(_remainingSpace / spacing);
                            _numItems = Mathf.Min(m_itemCount, Mathf.Clamp(_numItemsRaw, 0, MAX_ITEM_ARRAY_LENGTH));



                            Vector3 _position = m_mainCircle.Position(_t);
                            Vector3 _center = m_mainCircle.center;
                            Vector3 _radiusVector = _position - _center;

                            float _deltaAngle = m_mainCircle.DeltaAngle(spacing);
                            Quaternion _rotation = Quaternion.AngleAxis(-1f * _deltaAngle * Mathf.Rad2Deg, Vector3.up);

                            for (int i = 0; i < _numItems; i++) {
                                SetTAndPosition(i, _t, _position);

                                _radiusVector = _rotation * _radiusVector;
                                _position = _center + _radiusVector;

                                _t += _deltaT;
                            }

                            Vector3 _finalPos = Vector3.zero;
                            if (!GetItemT(_numItems - 1, out _finalT) || !GetItemPosition(_numItems - 1, out _finalPos)) {
                                ResetLastContinueParameters();
                                _finalT = 1f;
                            } else {
                                if (segmentState.isReadyForMaxContinue) {
                                    segmentState.newFinalOffset = spacing + m_mainCircle.ArclengthBetween(0f, _finalT);
                                } else {
                                    //segmentState.newFinalOffset = m_mainCircle.radius * Mathf.PI * (1f - _t);
                                    segmentState.newFinalOffset = m_mainCircle.ArclengthBetween(_t, 1f);
                                }

                            }
                            break;
                        }
                        default: {
                            _numItems = 0;
                            break;
                        }
                    }




                    //non-fence tDiff
                    if (_deltaT == 0f) {
                        _deltaT = m_placementInfo[1].t - m_placementInfo[0].t;
                    }


                }

                //re-set item count
                m_itemCount = _numItems;

                //flag if not enough item slots
                if (Mathf.FloorToInt(_numItemsRaw) > MAX_ITEM_ARRAY_LENGTH) {
                    segmentState.maxItemCountExceeded = true;
                } else {
                    segmentState.maxItemCountExceeded = false;
                }

                return true;
            }

            public static bool IsIndexWithinBounds(int index, bool isFenceEndPoint) {
                bool result = false;
                int _adj = 0;
                if (isFenceEndPoint) {
                    _adj = 1;
                }
                if (index >= 0 && index < (MAX_ITEM_ARRAY_LENGTH + _adj)) {
                    result = true;
                }
                return result;
            }

            private Vector3 GetFenceEndpoint(int index) {
                if (IsIndexWithinBounds(index, true)) {
                    return m_fenceEndPoints[index];
                }
                return Vector3.down;
            }
            public bool GetFenceEndpoint(int index, out Vector3 fenceEndpoint) {
                if (IsIndexWithinBounds(index, true)) {
                    fenceEndpoint = m_fenceEndPoints[index];
                }
                fenceEndpoint = Vector3.down;
                return false;
            }
            public bool GetItemT(int index, out float itemT) {
                if (IsIndexWithinBounds(index, false)) {
                    itemT = m_placementInfo[index].t;
                    return true;
                }
                itemT = 0f;
                return false;
            }
            public float GetItemT(int index) {
                float _itemT = 0f;

                if (IsIndexWithinBounds(index, false)) {
                    _itemT = m_placementInfo[index].t;
                }
                return _itemT;
            }
            public static bool GetItemPosition(int index, out Vector3 itemPosition) {
                if (IsIndexWithinBounds(index, false)) {
                    itemPosition = m_placementInfo[index].position;
                    return true;
                }
                itemPosition = Vector3.zero;
                return false;
            }
            public static Vector3 GetItemPosition(int index) {
                if (IsIndexWithinBounds(index, false)) {
                    return m_placementInfo[index].position;
                }
                return Vector3.down;
            }
            /// <summary>
            /// Only use for rendering geometry and placing items!
            /// </summary>
            public static bool GetItemMeshPosition(int index, out Vector3 itemPosition) {
                if (IsIndexWithinBounds(index, false)) {
                    itemPosition = m_placementInfo[index].meshPosition;
                    return true;
                }
                itemPosition = Vector3.zero;
                return false;
            }
            /// <summary>
            /// Only use for rendering geometry and placing items!
            /// </summary>
            public static Vector3 GetMeshItemPosition(int index) {
                if (IsIndexWithinBounds(index, false)) {
                    return m_placementInfo[index].meshPosition;
                }
                return Vector3.down;
            }
            private bool SetFenceEndpoint(int index, Vector3 position) {
                if (IsIndexWithinBounds(index, true)) {
                    m_fenceEndPoints[index] = position;
                    return true;
                }
                return false;
            }
            private bool SetPosition(int index, Vector3 position) {
                if (IsIndexWithinBounds(index, false)) {
                    m_placementInfo[index].position = position;
                    m_placementInfo[index].position.y = GetDetailHeight(position);
                    return true;
                }
                return false;
            }
            private bool SetTAndPosition(int index, float t, Vector3 position) {
                if (IsIndexWithinBounds(index, false)) {
                    m_placementInfo[index].t = t;
                    m_placementInfo[index].position = position;
                    m_placementInfo[index].position.y = GetDetailHeight(position);
                    return true;
                }
                return false;
            }

            private bool IncrementPosition(int index, Vector3 increment) {
                if (IsIndexWithinBounds(index, false)) {
                    Vector3 _oldPos = m_placementInfo[index].position;
                    Vector3 _newPos = _oldPos + increment;
                    m_placementInfo[index].position = _newPos;
                    m_placementInfo[index].position.y = GetDetailHeight(_newPos);
                    return true;
                } else {
                    return false;
                }
            }
            private bool IncrementFenceEndpoint(int index, Vector3 increment) {
                if (IsIndexWithinBounds(index, true)) {
                    Vector3 _oldPos = m_fenceEndPoints[index];
                    Vector3 _newPos = _oldPos + increment;
                    m_fenceEndPoints[index] = _newPos;
                    m_fenceEndPoints[index].y = GetDetailHeight(_newPos);
                    return true;
                } else {
                    return false;
                }
            }
            private float GetDetailHeight(Vector3 position) {
                float result = 0f;
                result = Singleton<TerrainManager>.instance.SampleDetailHeight(position);
                return result;
            }

            //================================  DIRECTION  ================================|================================================================
            //MUST BE CALLED AFTER CALCULATEALLPOSITIONS()
            private bool CalculateAllDirections() {
                switch (drawMode) {
                    case DrawMode.Straight: {
                        Vector3 _itemDir = new Vector3();
                        //calculate from segment
                        _itemDir = m_mainSegment.b - m_mainSegment.a;

                        //this function takes care of the normalization for you
                        for (int i = 0; i < m_itemCount; i++) {
                            SetItemDirectionsXZ(i, _itemDir);
                        }
                        break;
                    }
                    case DrawMode.Curved:
                    case DrawMode.Freeform: {
                        if (fenceMode == true) {
                            Vector3 _itemDir = new Vector3();
                            //calculate fenceEndpoint to fenceEndpoint
                            for (int i = 0; i < m_itemCount; i++) {
                                _itemDir = m_fenceEndPoints[i + 1] - m_fenceEndPoints[i];
                                SetItemDirectionsXZ(i, _itemDir);
                            }
                        } else {
                            Vector3 _itemDir = new Vector3();
                            //calculate from curve tangent
                            for (int i = 0; i < m_itemCount; i++) {
                                _itemDir = m_mainBezier.Tangent(m_placementInfo[i].t);
                                SetItemDirectionsXZ(i, _itemDir);
                            }
                        }
                        break;
                    }
                    case DrawMode.Circle: {
                        if (fenceMode == true) {
                            Vector3 _itemDir = new Vector3();
                            //calculate fenceEndpoint to fenceEndpoint
                            for (int i = 0; i < m_itemCount; i++) {
                                _itemDir = m_fenceEndPoints[i + 1] - m_fenceEndPoints[i];
                                SetItemDirectionsXZ(i, _itemDir);
                            }
                        } else {
                            Vector3 _itemDir = new Vector3();
                            //calculate from curve tangent
                            for (int i = 0; i < m_itemCount; i++) {
                                _itemDir = m_mainCircle.Tangent(m_placementInfo[i].t);
                                SetItemDirectionsXZ(i, _itemDir);
                            }
                        }
                        break;
                    }
                    default: {
                        return false;
                    }
                }

                return true;
            }

            private bool GetItemDirection(int index, out Vector3 itemDirection) {
                if (IsIndexWithinBounds(index, false)) {
                    itemDirection = m_placementInfo[index].itemDirection;
                    if (itemDirection.magnitude < 0.9998f || itemDirection.magnitude > 1.0002f) {
                        itemDirection = Vector3.zero;
                        return false;
                    }
                    return true;
                } else {
                    itemDirection = Vector3.zero;
                    return false;
                }
            }
            private bool GetOffsetDirection(int index, out Vector3 offsetDirection) {
                if (IsIndexWithinBounds(index, false)) {
                    offsetDirection = m_placementInfo[index].offsetDirection;
                    if (offsetDirection.magnitude < 0.9998f || offsetDirection.magnitude > 1.0002f) {
                        offsetDirection = Vector3.zero;
                        return false;
                    }
                    return true;
                } else {
                    offsetDirection = Vector3.zero;
                    return false;
                }
            }
            private bool SetItemDirectionsXZ(int index, Vector3 direction) {
                if (IsIndexWithinBounds(index, false)) {
                    m_placementInfo[index].SetDirectionsXZ(direction);
                    return true;
                }
                return false;
            }


            //================================  ANGLES  ================================|================================================================
            //first calculate base angles
            //   if dynamic rotation, CalculateAnglesDynamic()
            //   if single rotation, CalculateAnglesSingle()
            //then apply transforms

            /// <summary>
            /// Base mod method to calculate all angles in Dynamic or Single angle modes.
            /// </summary>
            private void CalculateAllAnglesBase() {
                if (fenceMode == true) {
                    CalculateAnglesDynamic();

                    float _offsetAngle = 0f;

                    //_offsetAngle = modelAngleOffset + angleFlip180;
                    _offsetAngle = totalPropertyAngleOffset;

                    _offsetAngle += angleOffset;

                    OffsetAnglesUniformRadians(_offsetAngle);
                } else {
                    switch (angleMode) {
                        case AngleMode.Dynamic: {
                            CalculateAnglesDynamic();

                            float _offsetAngle = angleOffset;

                            //_offsetAngle = modelAngleOffset + angleFlip180;
                            _offsetAngle = totalPropertyAngleOffset;

                            _offsetAngle += angleOffset;

                            OffsetAnglesUniformRadians(_offsetAngle);
                            break;
                        }
                        case AngleMode.Single: {
                            float _singleAngle = angleSingle;

                            //_singleAngle = modelAngleOffset + angleFlip180;
                            _singleAngle = totalPropertyAngleOffset;

                            _singleAngle += angleSingle;

                            CalculateAnglesSingleRadians(_singleAngle);
                            break;
                        }
                        default: {
                            return;
                        }
                    }
                }
            }

            //base function to calculate angles
            private void CalculateAnglesDynamic() {
                float _angle = 0f;
                Vector3 _itemDir = Vector3.right;
                Vector3 _xAxis = Vector3.right;
                Vector3 _yAxis = Vector3.up;
                for (int i = 0; i < m_itemCount; i++) {
                    if (!GetItemDirection(i, out _itemDir)) {
                        //early exit if index is incorrect
                        //or itemDir isn't set
                        m_itemCount = i;
                        return;
                    }
                    //_angle = MathPLT.AngleSigned(_xAxis, _itemDir, _yAxis) + Mathf.PI;
                    _angle = Math.MathPLT.AngleSigned(_itemDir, _xAxis, _yAxis) + Mathf.PI;
                    if (!SetAngle(i, _angle)) {
                        m_itemCount = i;
                        return;
                    }
                }
            }
            //base function to set all angles to same value
            private void CalculateAnglesSingle(float singleAngleInDegrees /*in Degrees*/) {
                float _angle = 0f;
                _angle = singleAngleInDegrees;
                for (int i = 0; i < m_itemCount; i++) {
                    if (!SetAngleInDegrees(i, _angle)) {
                        m_itemCount = i;
                        return;
                    }
                }
            }

            private void CalculateAnglesSingleRadians(float singleAngleInRadians /*in Radians*/) {
                float _angle = 0f;
                _angle = singleAngleInRadians * Mathf.Rad2Deg;
                for (int i = 0; i < m_itemCount; i++) {
                    if (!SetAngleInDegrees(i, _angle)) {
                        m_itemCount = i;
                        return;
                    }
                }
            }

            //called after setting up base angles
            //used by ActiveState.ChangeAngle
            private void OffsetAnglesUniformRadians(float relativeAngleOffsetInRadians /*in Radians*/) {
                float _angle = 0f;
                _angle = relativeAngleOffsetInRadians * Mathf.Rad2Deg;
                for (int i = 0; i < m_itemCount; i++) {
                    if (!IncrementAngleInDegrees(i, _angle)) {
                        m_itemCount = i;
                        return;
                    }
                }
            }

            //called after setting up base angles
            //used by ActiveState.ChangeAngle
            private void OffsetAnglesUniformDegrees(float relativeAngleOffsetInDegrees /*in Degrees*/) {
                float _angle = 0f;
                _angle = relativeAngleOffsetInDegrees;
                for (int i = 0; i < m_itemCount; i++) {
                    if (!IncrementAngleInDegrees(i, _angle)) {
                        m_itemCount = i;
                        return;
                    }
                }
            }

            //individual
            /// <param name="itemAngle">Angle in radians.</param>
            public bool GetAngle(int index, out float itemAngle) {
                if (IsIndexWithinBounds(index, false)) {
                    itemAngle = m_placementInfo[index].angle;
                    return true;
                } else {
                    itemAngle = 0f;
                    return false;
                }
            }
            private bool SetAngle(int index, float angleInRadians /*absolute angle, in radians*/) {
                if (IsIndexWithinBounds(index, false)) {
                    float _angle = (angleInRadians % (2f * Mathf.PI));
                    m_placementInfo[index].angle = _angle;
                    return true;
                }
                return false;
            }
            private bool SetAngleInDegrees(int index, float angleInDegrees /*absolute angle, in degrees*/) {

                if (IsIndexWithinBounds(index, false)) {
                    float _angle = Mathf.Deg2Rad * (angleInDegrees % 360f);
                    m_placementInfo[index].angle = _angle;
                    return true;
                }
                return false;
            }
            private bool IncrementAngleInDegrees(int index, float angleIncrementInDegrees /*relative angle, in degrees*/) {
                if (IsIndexWithinBounds(index, false)) {
                    float _increment = Mathf.Deg2Rad * (angleIncrementInDegrees % 360f);
                    float _oldAngle = m_placementInfo[index].angle;
                    float _newAngle = _oldAngle + _increment;
                    m_placementInfo[index].angle = _newAngle;
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Either 0 or pi/2 [radians].
            /// </summary>
            public float modelAngleOffset {
                get {
                    return m_assetModelZ > m_assetModelX ? Mathf.PI / 2f : 0f;
                }
            }

            /// <summary>
            /// Either 0 or pi [radians].
            /// </summary>
            public float angleFlip180 {
                get {
                    return userSettingsControlPanel.angleFlip180 == true ? Mathf.PI : 0f;
                }
            }

            /// <summary>
            /// Depends on multiple things [radians].
            /// </summary>
            public float totalPropertyAngleOffset {
                get {
                    return modelAngleOffset + angleFlip180;
                }
            }

            /// <summary>
            /// Calculates the absolute angle[radians][0, 2pi) of a directionVector in the XZ plane.
            /// </summary>
            /// <param name="directionVector"></param>
            /// <returns></returns>
            public static float AngleDynamicXZ(Vector3 directionVector) {
                if (directionVector == Vector3.zero) {
                    return 0f;
                }

                float _angle = 0f;

                directionVector.y = 0f;
                directionVector.Normalize();

                Vector3 _itemDir = directionVector;
                Vector3 _xAxis = Vector3.right;
                Vector3 _yAxis = Vector3.up;

                _angle = Math.MathPLT.AngleSigned(_itemDir, _xAxis, _yAxis) + Mathf.PI;

                return _angle;
            }


            //================================  ERROR CHECKING  ================================|================================================================

            public void UpdatePlacementErrors() {
                if (userSettingsControlPanel.errorChecking == false) {
                    Vector3 _position = Vector3.zero;
                    ItemCollisionType _collisionFlags = ItemCollisionType.None;

                    for (int i = 0; i < m_itemCount; i++) {
                        SetItemCollisionFlags(i, _collisionFlags);
                        SetItemValidPlacement(i, true);
                    }
                    segmentState.allItemsValid = true;
                    return;
                }

                bool _itemsValid = true;

                switch (objectMode) {
                    case ObjectMode.Props: {
                        if (propInfo != null) {
                            Vector3 _position = Vector3.zero;
                            ItemCollisionType _collisionFlags = ItemCollisionType.None;
                            PropInfo _propInfo = propInfo;

                            for (int i = 0; i < m_itemCount; i++) {
                                if (GetItemPosition(i, out _position)) {
                                    _propInfo = m_placementInfo[i].propInfo;

                                    _collisionFlags = ErrorChecker.CheckAllCollisionsProp(_position, _propInfo);
                                    SetItemCollisionFlags(i, _collisionFlags);

                                    if (_collisionFlags == ItemCollisionType.None) {
                                        SetItemValidPlacement(i, true);
                                    } else if (userSettingsControlPanel.anarchyPLT) {
                                        SetItemValidPlacement(i, true);
                                    } else if (userSettingsControlPanel.placeBlockedItems && _collisionFlags == ItemCollisionType.Blocked) {
                                        SetItemValidPlacement(i, true);
                                    } else {
                                        SetItemValidPlacement(i, false);
                                        _itemsValid = false;
                                    }
                                }
                            }
                        }
                        break;
                    }
                    case ObjectMode.Trees: {
                        if (treeInfo != null) {
                            Vector3 _position = Vector3.zero;
                            ItemCollisionType _collisionFlags = ItemCollisionType.None;
                            TreeInfo _treeInfo = treeInfo;

                            for (int i = 0; i < m_itemCount; i++) {
                                if (GetItemPosition(i, out _position)) {
                                    _treeInfo = m_placementInfo[i].treeInfo;

                                    _collisionFlags = ErrorChecker.CheckAllCollisionsTree(_position, treeInfo);
                                    SetItemCollisionFlags(i, _collisionFlags);

                                    if (_collisionFlags == ItemCollisionType.None) {
                                        SetItemValidPlacement(i, true);
                                    } else if (userSettingsControlPanel.anarchyPLT) {
                                        SetItemValidPlacement(i, true);
                                    } else if (userSettingsControlPanel.placeBlockedItems && _collisionFlags == ItemCollisionType.Blocked) {
                                        SetItemValidPlacement(i, true);
                                    } else {
                                        SetItemValidPlacement(i, false);
                                        _itemsValid = false;
                                    }
                                }
                            }
                        }
                        break;
                    }
                    default: {
                        segmentState.allItemsValid = true;
                        break;
                    }
                }

                segmentState.allItemsValid = _itemsValid;
            }

            /// <summary>
            /// Sets an item's collision flags to its current flags AND the input flags. (through Or Equals, |=)
            /// </summary>
            private bool AddItemCollisionFlags(int index, ItemCollisionType collisionFlags) {
                if (IsIndexWithinBounds(index, false)) {
                    m_placementInfo[index].collisionFlags |= collisionFlags;
                    return true;
                }
                return false;
            }
            /// <summary>
            /// Sets an item's collision flags to ONLY the input flags.
            /// </summary>
            private bool SetItemCollisionFlags(int index, ItemCollisionType collisionFlags) {
                if (IsIndexWithinBounds(index, false)) {
                    m_placementInfo[index].collisionFlags = collisionFlags;
                    return true;
                }
                return false;
            }
            private bool SetItemValidPlacement(int index, bool isValidPlacement) {
                if (IsIndexWithinBounds(index, false)) {
                    m_placementInfo[index].isValidPlacement = isValidPlacement;
                    return true;
                }
                return false;
            }


            public static bool GetItemCollisionFlags(int index, out ItemCollisionType collisionFlags) {
                if (IsIndexWithinBounds(index, false)) {
                    collisionFlags = m_placementInfo[index].collisionFlags;
                    return true;
                }
                collisionFlags = ItemCollisionType.None;
                return false;
            }
            public static bool GetItemValidPlacement(int index, out bool isValidPlacement) {
                if (IsIndexWithinBounds(index, false)) {
                    isValidPlacement = m_placementInfo[index].isValidPlacement;
                    return true;
                }
                isValidPlacement = true;
                return false;
            }



            public static bool SetTreeID(int index, uint treeID) {
                if (IsIndexWithinBounds(index, false)) {
                    m_placementInfo[index].treeID = treeID;
                    return true;
                }
                return false;
            }
            public static bool SetPropID(int index, ushort propID) {
                if (IsIndexWithinBounds(index, false)) {
                    m_placementInfo[index].propID = propID;
                    return true;
                }
                return false;
            }


            //================================  VARIATION MANAGEMENT  ================================|================================================================
            public void SetAllItemPrefabInfos() {
                //make sure to use the same randomizer as item placement (PropLineTool.FinalizePlacement)
                Randomizer _randomizer1 = randomizerFresh;

                switch (objectMode) {
                    case ObjectMode.Props: {
                        if (propInfo != null) {
                            if (propInfo.m_variations.Length > 0) {
                                PropInfo _propInfo = propInfo;
                                Randomizer _randomizer2 = randomizerFresh;

                                for (int i = 0; i < m_itemCount; i++) {
                                    if (IsIndexWithinBounds(i, false)) {
                                        _propInfo = propInfo.GetVariation(ref _randomizer1);
                                        m_placementInfo[i].propInfo = _propInfo;

                                        //incorrect with variation props
                                        //m_placementInfo[i].scale = _propInfo.m_minScale + (float)_randomizer2.Int32(10000u) * (_propInfo.m_maxScale - _propInfo.m_minScale) * 0.0001f;
                                        //m_placementInfo[i].color = _propInfo.GetColor(ref _randomizer2);

                                        //nope
                                        //test with variation props
                                        //m_placementInfo[i].scale = _propInfo.m_minScale + (float)_randomizer1.Int32(10000u) * (_propInfo.m_maxScale - _propInfo.m_minScale) * 0.0001f;
                                        //m_placementInfo[i].color = _propInfo.GetColor(ref _randomizer1);

                                        //third times the charm
                                        _randomizer2 = RandomizerNextRandom(_randomizer1);
                                        m_placementInfo[i].scale = _propInfo.m_minScale + (float)_randomizer2.Int32(10000u) * (_propInfo.m_maxScale - _propInfo.m_minScale) * 0.0001f;
                                        m_placementInfo[i].color = _propInfo.GetColor(ref _randomizer2);
                                    }
                                }
                            } else {
                                //accurate single item
                                //see PropTool.RenderGeometry
                                if (controlMode == ControlMode.Itemwise && m_itemCount == 1) {
                                    m_placementInfo[ITEMWISE_INDEX].propInfo = propInfo;

                                    ushort _seed = Singleton<PropManager>.instance.m_props.NextFreeItem(ref _randomizer1);
                                    Randomizer _randomizer2 = new Randomizer((int)_seed);

                                    m_placementInfo[ITEMWISE_INDEX].scale = propInfo.m_minScale + (float)_randomizer2.Int32(10000u) * (propInfo.m_maxScale - propInfo.m_minScale) * 0.0001f;
                                    m_placementInfo[ITEMWISE_INDEX].color = propInfo.GetColor(ref _randomizer2);
                                } else {
                                    for (int i = 0; i < m_itemCount; i++) {
                                        if (IsIndexWithinBounds(i, false)) {
                                            m_placementInfo[i].propInfo = propInfo;

                                            m_placementInfo[i].scale = propInfo.m_minScale + (float)_randomizer1.Int32(10000u) * (propInfo.m_maxScale - propInfo.m_minScale) * 0.0001f;
                                            m_placementInfo[i].color = propInfo.GetColor(ref _randomizer1);
                                        }
                                    }
                                }
                            }
                        } else {
                            m_itemCount = 0;
                        }
                        break;
                    }
                    case ObjectMode.Trees: {
                        if (treeInfo != null) {
                            if (treeInfo.m_variations.Length > 0) {
                                TreeInfo _treeInfo = treeInfo;
                                Randomizer _randomizer2 = randomizerFresh;

                                for (int i = 0; i < m_itemCount; i++) {
                                    if (IsIndexWithinBounds(i, false)) {
                                        _treeInfo = treeInfo.GetVariation(ref _randomizer1);
                                        m_placementInfo[i].treeInfo = _treeInfo;

                                        //incorrect for variation with trees
                                        //m_placementInfo[i].scale = _treeInfo.m_minScale + (float)_randomizer2.Int32(10000u) * (_treeInfo.m_maxScale - _treeInfo.m_minScale) * 0.0001f;
                                        //m_placementInfo[i].brightness = _treeInfo.m_minBrightness + (float)_randomizer2.Int32(10000u) * (_treeInfo.m_maxBrightness - _treeInfo.m_minBrightness) * 0.0001f;

                                        //test for variation with trees
                                        //correct for itemwise
                                        //m_placementInfo[i].scale = _treeInfo.m_minScale + (float)_randomizer1.Int32(10000u) * (_treeInfo.m_maxScale - _treeInfo.m_minScale) * 0.0001f;
                                        //m_placementInfo[i].brightness = _treeInfo.m_minBrightness + (float)_randomizer1.Int32(10000u) * (_treeInfo.m_maxBrightness - _treeInfo.m_minBrightness) * 0.0001f;

                                        //third times the charm
                                        _randomizer2 = RandomizerNextRandom(_randomizer1);
                                        m_placementInfo[i].scale = _treeInfo.m_minScale + (float)_randomizer2.Int32(10000u) * (_treeInfo.m_maxScale - _treeInfo.m_minScale) * 0.0001f;
                                        m_placementInfo[i].brightness = _treeInfo.m_minBrightness + (float)_randomizer2.Int32(10000u) * (_treeInfo.m_maxBrightness - _treeInfo.m_minBrightness) * 0.0001f;
                                    }
                                }
                            } else {
                                //accurate single item
                                //see TreeTool.RenderGeometry
                                if (controlMode == ControlMode.Itemwise && m_itemCount == 1) {
                                    m_placementInfo[ITEMWISE_INDEX].treeInfo = treeInfo;

                                    uint _seed = Singleton<TreeManager>.instance.m_trees.NextFreeItem(ref _randomizer1);
                                    Randomizer _randomizer2 = new Randomizer(_seed);

                                    m_placementInfo[ITEMWISE_INDEX].scale = treeInfo.m_minScale + (float)_randomizer2.Int32(10000u) * (treeInfo.m_maxScale - treeInfo.m_minScale) * 0.0001f;
                                    m_placementInfo[ITEMWISE_INDEX].brightness = treeInfo.m_minBrightness + (float)_randomizer2.Int32(10000u) * (treeInfo.m_maxBrightness - treeInfo.m_minBrightness) * 0.0001f;
                                } else {
                                    for (int i = 0; i < m_itemCount; i++) {
                                        if (IsIndexWithinBounds(i, false)) {
                                            m_placementInfo[i].treeInfo = treeInfo;

                                            m_placementInfo[i].scale = treeInfo.m_minScale + (float)_randomizer1.Int32(10000u) * (treeInfo.m_maxScale - treeInfo.m_minScale) * 0.0001f;
                                            m_placementInfo[i].brightness = treeInfo.m_minBrightness + (float)_randomizer1.Int32(10000u) * (treeInfo.m_maxBrightness - treeInfo.m_minBrightness) * 0.0001f;
                                        }
                                    }
                                }
                            }
                        } else {
                            m_itemCount = 0;
                        }
                        break;
                    }
                    default: {
                        m_itemCount = 0;
                        break;
                    }
                }

            }
            protected static bool SetItemTreeInfo(int index, TreeInfo treeInfo) {
                if (IsIndexWithinBounds(index, false)) {
                    m_placementInfo[index].treeInfo = treeInfo;
                    return true;
                }
                return false;
            }
            protected static bool SetItemPropInfo(int index, PropInfo propInfo) {
                if (IsIndexWithinBounds(index, false)) {
                    m_placementInfo[index].propInfo = propInfo;
                    return true;
                }
                return false;
            }



        }

        private static PlacementCalculator m_placementCalculator = new PlacementCalculator();
        internal static PlacementCalculator placementCalculator {
            get {
                return m_placementCalculator;
            }
            set {
                m_placementCalculator = value;
            }
        }

        //Undo Manager
        private static UndoManager m_undoManager = new UndoManager();
        internal static UndoManager undoManager {
            get {
                return m_undoManager;
            }
            private set {
                m_undoManager = value;
            }
        }

        //User Settings - Control Panel
        private static UserSettingsControlPanel m_userSettingsControlPanel = new UserSettingsControlPanel();
        internal static UserSettingsControlPanel userSettingsControlPanel {
            get {
                return m_userSettingsControlPanel;
            }
            set {
                m_userSettingsControlPanel = value;
            }
        }

        //Overlay Colors!
        public Color m_PLTColor_default = new Color32(39, 130, 204, 128);
        public Color m_PLTColor_defaultSnapZones = new Color32(39, 130, 204, 255);
        public Color m_PLTColor_locked = new Color32(28, 127, 64, 128);
        public Color m_PLTColor_lockedStrong = new Color32(28, 127, 64, 192);
        public Color m_PLTColor_lockedHighlight = new Color32(228, 239, 232, 160);
        public Color m_PLTColor_copyPlace = new Color32(114, 45, 186, 128);
        public Color m_PLTColor_copyPlaceHighlight = new Color32(214, 223, 234, 160);
        public Color m_PLTColor_hoverBase = new Color32(33, 142, 129, 204);
        public Color m_PLTColor_hoverCopyPlace = new Color32(196, 198, 242, 204);
        public Color m_PLTColor_undoItemOverlay = new Color32(214, 144, 81, 204);
        public Color m_PLTColor_curveWarning = new Color32(231, 155, 24, 160);
        //new colors
        public Color m_PLTColor_ItemwiseLock = new Color32(29, 72, 168, 128);
        public Color m_PLTColor_MaxFillContinue = new Color32(211, 193, 221, 128);

        //for in-game debugging
        private bool m_debugRenderOverlayItemPoints = false;

        //Big 3 members
        //   All 3
        private Ray m_mouseRay;
        private float m_mouseRayLength;
        private bool m_mouseRayValid;

        //   NetTool members
        private int m_cachedControlPointCount;
        private PropLineTool.ControlPoint[] m_cachedControlPoints;
        private object m_cacheLock;
        private PropLineTool.ControlPoint[] m_controlPoints;
        private int m_controlPointCount;
        private bool m_lengthChanging;
        private float m_lengthTimer;

        //   Prop/TreeTool members
        private Vector3 m_cachedPosition;
        private bool m_mouseLeftDown;
        private bool m_mouseRightDown;
        private Vector3 m_mousePosition;
        private Randomizer m_randomizer;

        //   Custom members similar to the Big 3
        private bool m_keyboardCtrlDown;
        private bool m_keyboardAltDown;
        public bool keyboardAltDown {
            get {
                return m_keyboardAltDown;
            }
        }
        private bool m_keyboardShiftDown; //this gets stuck for some reason
        private bool m_positionChanging;
        private bool m_pendingPlacementUpdate;
        private bool IsPositionChanging() {
            bool result = false;
            Vector3 _oldPos = this.m_cachedPosition;
            Vector3 _newPos = this.m_mousePosition;
            if (_oldPos == null || _newPos == null) {
                result = false;
            } else if (Vector3.SqrMagnitude(_newPos - _oldPos) > 1f) {
                result = true;
            }
            return result;
        }
        private bool IsVectorPositionChanging(Vector3 oldPosition, Vector3 newPosition, float sqrMagnitudeThreshold) {
            bool result = false;
            Vector3 _oldPos = oldPosition;
            Vector3 _newPos = newPosition;
            if (_oldPos == null || _newPos == null) {
                result = false;
            } else if (Vector3.SqrMagnitude(_newPos - _oldPos) > sqrMagnitudeThreshold) {
                result = true;
            }
            return result;
        }
        private bool IsVectorXZPositionChanging(Vector3 oldPosition, Vector3 newPosition, float tolerance) {
            bool result = false;
            Vector3 _oldPos = oldPosition;
            Vector3 _newPos = newPosition;
            float _sqrMagnitudeThreshold = tolerance * tolerance;

            _oldPos.y = 0f;
            _newPos.y = 0f;
            if (Vector3.SqrMagnitude(_newPos - _oldPos) > _sqrMagnitudeThreshold) {
                result = true;
            } else {
                result = false;
            }

            return result;
        }
        private static bool IsCurveLengthLongEnoughXZ() {
            bool _result = false;

            if (fenceMode == true) {
                switch (drawMode) {
                    case DrawMode.Straight: {
                        _result = PlacementCalculator.GetCurveLength(m_mainSegment) >= 0.75f * placementCalculator.spacingSingle;
                        break;
                    }
                    case DrawMode.Curved:
                    case DrawMode.Freeform: {
                        //_result = PlacementCalculator.GetCurveLength(m_mainBezier) >= placementCalculator.spacingSingle;
                        _result = (m_mainBezier.d - m_mainBezier.a).magnitude >= placementCalculator.spacingSingle;
                        break;
                    }
                    case DrawMode.Circle: {
                        _result = m_mainCircle.diameter >= placementCalculator.spacingSingle;
                        break;
                    }
                    default: {
                        break;
                    }
                }
            } else {
                switch (drawMode) {
                    case DrawMode.Straight: {
                        _result = PlacementCalculator.GetCurveLength(m_mainSegment) >= placementCalculator.spacingSingle;
                        break;
                    }
                    case DrawMode.Curved:
                    case DrawMode.Freeform: {
                        _result = PlacementCalculator.GetCurveLength(m_mainBezier) >= placementCalculator.spacingSingle;
                        break;
                    }
                    case DrawMode.Circle: {
                        _result = m_mainCircle.circumference >= placementCalculator.spacingSingle;
                        break;
                    }
                    default: {
                        break;
                    }
                }
            }

            return _result;
        }

        //prefab info
        //see PropTool/TreeTool
        private PropInfo m_propPrefab;
        private TreeInfo m_treePrefab;
        public PropInfo propPrefab {
            get {
                return m_propPrefab;
            }
            set {
                bool _changedPrefab = m_propPrefab != value;

                m_propPrefab = value;
                placementCalculator.SetupPropPrefab(value);

                if (_changedPrefab) {
                    if (userSettingsControlPanel.autoDefaultSpacing == true && objectMode == ObjectMode.Props) {
                        placementCalculator.SetDefaultSpacing();
                    }

                    placementCalculator.UpdateItemPlacementInfo();
                    //else
                    //{
                    //    placementCalculator.UpdateItemPlacementInfo();
                    //}
                }
            }
        }
        public TreeInfo treePrefab {
            get {
                return m_treePrefab;
            }
            set {
                bool _changedPrefab = m_treePrefab != value;

                m_treePrefab = value;
                placementCalculator.SetupTreePrefab(value);

                if (_changedPrefab) {
                    if (userSettingsControlPanel.autoDefaultSpacing == true && objectMode == ObjectMode.Trees) {
                        placementCalculator.SetDefaultSpacing();
                    }

                    placementCalculator.UpdateItemPlacementInfo();
                    //else
                    //{
                    //    placementCalculator.UpdateItemPlacementInfo();
                    //}
                }
            }
        }
        private PropInfo m_propInfo;
        private TreeInfo m_treeInfo;
        private PropInfo m_wasPropPrefab;
        private TreeInfo m_wasTreePrefab;

        //used to reset active state
        private static bool m_initialized = false;
        //public static bool m_keepActiveState = false;
        public static bool m_keepActiveState = true;

        //custom method to reset active state
        //called by ToolSwitch
        public void ResetActiveState() {
            ResetPLT();
            return;
        }

        //refer to the Big 3 [NetTool, PropTool, TreeTool]
        protected override void Awake() {
            base.Awake();

            Debug.Log("[PLT]: Begin PropLineTool.Awake()");

            PropLineTool.instance = this;

            //NetTool base stuff
            this.m_controlPoints = new PropLineTool.ControlPoint[3];
            this.m_cachedControlPoints = new PropLineTool.ControlPoint[3];
            this.lockBackupControlPoints = new PropLineTool.ControlPoint[3];
            this.m_cacheLock = new object();

            //Prop/TreeTool base methods
            this.m_randomizer = new Randomizer((int)DateTime.Now.Ticks); //standard time-based randomizer

            //clear last continue parameters
            placementCalculator.ResetLastContinueParameters();

            //check main menu settings
            if (UserSettingsMainMenu.anarchyPLTOnByDefault) {
                userSettingsControlPanel.showErrorGuides = false;
                userSettingsControlPanel.placeBlockedItems = true;
                userSettingsControlPanel.anarchyPLT = true;
                userSettingsControlPanel.errorChecking = false;
            }

            //event subscriptions
            userSettingsControlPanel.eventErrorCheckingSettingChanged += delegate () {
                placementCalculator.UpdatePlacementErrors();
            };
            userSettingsControlPanel.eventParametersTabSettingChanged += delegate () {
                placementCalculator.UpdateItemPlacementInfo();

                if (userSettingsControlPanel.autoDefaultSpacing == true) {
                    placementCalculator.SetDefaultSpacing();
                }
            };
            userSettingsControlPanel.eventRenderingPositioningSettingChanged += delegate () {
                UpdateCurves();
                placementCalculator.UpdateItemPlacementInfo();
            };
            eventDrawModeChanged += delegate (PropLineTool.DrawMode mode) {
                bool _straightOrCircle = mode == DrawMode.Straight || drawMode == DrawMode.Circle;

                //fix Straight<->Circle switching in Lock mode
                if (_straightOrCircle) {
                    UpdateCurves();
                }


                if (_straightOrCircle && fenceMode == true && (activeState == ActiveState.CreatePointSecond || activeState == ActiveState.CreatePointThird)) {
                    //snap first control point to last fence endpoint
                    //when user switched from fencemode[curved/freeform -> straight] and curve is coupled to previous segment
                    Vector3 _lastFenceEndpoint = placementCalculator.GetLastFenceEndpoint();
                    Vector3 _firstControlPoint = m_controlPoints[0].m_position;

                    //Debug.Log("[PLT]: PropLineTool.Awake(): Inside 1/2 step of fence snap.");

                    if (_lastFenceEndpoint != Vector3.zero && _lastFenceEndpoint != Vector3.down && _lastFenceEndpoint != _firstControlPoint) {
                        //Debug.Log("[PLT]: PropLineTool.Awake(): Inside 2/2 step of fence snap.");

                        ModifyControlPoint(_lastFenceEndpoint, 1);
                        placementCalculator.UpdateItemPlacementInfo();
                    }
                }
            };
            //auto-set active state when controlMode is changed
            eventControlModeChanged += delegate (PropLineTool.ControlMode mode) {
                switch (mode) {
                    case ControlMode.Itemwise: {
                        if (activeState == ActiveState.LockIdle) {
                            GoToActiveState(ActiveState.ItemwiseLock);
                        }
                        break;
                    }
                    case ControlMode.Spacing: {
                        if (activeState == ActiveState.ItemwiseLock) {
                            GoToActiveState(ActiveState.LockIdle);
                        }
                        break;
                    }
                    default: {
                        //do nothing
                        break;
                    }
                }
            };
            //update perfect circle when prefab changed
            placementCalculator.eventSpacingSingleChanged += delegate (object sender, float spacing) {
                if (userSettingsControlPanel.perfectCircles) {
                    UpdateCurves();
                }
            };

            //finally
            Debug.Log("[PLT]: End PropLineTool.Awake()");
        }

        protected override void OnDestroy() {
            PropLineTool.instance = null;

            base.OnDestroy();
        }

        //called when exiting to main menu, other save, or desktop
        internal static void OnLevelUnloading() {
            m_initialized = false;

            undoManager = new UndoManager();
        }

        //custom method
        //cancels/resets all control points
        private void ResetAllControlPoints() {
            for (int i = 0; i < this.m_controlPoints.Length; i++) {
                this.m_controlPoints[i].Clear();
            }
            this.m_controlPointCount = 0;
            this.m_positionChanging = true;
        }

        //custom method
        //on-rightclick
        //cancels the latest control point and sets its stuff to zero
        private bool CancelControlPoint() {
            bool result = false;

            //custom stuff
            if ((this.m_controlPointCount > 0) && (this.m_controlPointCount <= 3)) {
                switch (m_controlPointCount) {
                    //placing first CP
                    case 0: {
                        ResetAllControlPoints();
                        ModifyControlPoint(this.m_cachedPosition, 1);
                        this.m_positionChanging = true;
                        result = true;
                        break;
                    }
                    //placing second CP
                    case 1: {
                        this.m_controlPoints[0].Clear();
                        this.m_cachedControlPoints[0].Clear();
                        this.m_cachedControlPoints[1].Clear();
                        this.m_cachedControlPoints[2].Clear();
                        this.m_controlPointCount = 0;

                        this.m_positionChanging = true;
                        result = true;
                        break;
                    }
                    //for straight
                    //in locking mode
                    //   or the instant before placement occurs
                    //for curved and freeform
                    //   placing/creating third CP
                    case 2: {
                        Vector3 _oldDir1 = this.m_controlPoints[1].m_direction;
                        this.m_cachedControlPoints[2].Clear();
                        this.m_controlPointCount = 1;
                        if ((PropLineTool.m_snapMode == SnapMode.Objects) || (PropLineTool.m_snapMode == SnapMode.ZoneLines)) {
                            //stub
                        } else {
                            this.m_controlPoints[0].m_direction = _oldDir1;
                        }

                        ModifyControlPoint(this.m_mousePosition, 2);
                        this.m_positionChanging = true;

                        result = true;
                        break;
                    }
                    //for curved and freeform
                    //in locking mode
                    //   or the instant before placement occurs
                    case 3: {
                        Vector3 _oldPos2 = this.m_controlPoints[2].m_position;
                        this.m_controlPoints[2].Clear();
                        this.m_cachedControlPoints[2].Clear();
                        this.m_controlPointCount = 2;

                        this.m_positionChanging = true;
                        ModifyControlPoint(this.m_mousePosition, 3);

                        result = true;
                        break;
                    }
                }
            } else {
                Debug.LogError("[PLT]: CancelControlPoint(): m_controlPointCount is out of bounds! (value = " + this.m_controlPointCount.ToString() + ")");
                result = false;
            }
            UpdateCachedPosition(true);
            UpdateCachedControlPoints();
            return result;
        }

        //custom method
        //finalizes control point placement to move onto the next step
        //on-click
        private bool AddControlPoint(Vector3 position) {
            bool result = false;

            //custom stuff
            if ((this.m_controlPointCount >= 0) && (this.m_controlPointCount < 3)) {
                switch (m_controlPointCount) {
                    case 0: {
                        this.m_controlPoints[0].m_position = position;
                        Vector3 _vector0 = position;
                        this.m_controlPoints[0].m_position = _vector0;
                        if ((PropLineTool.m_snapMode == SnapMode.Objects) || (PropLineTool.m_snapMode == SnapMode.ZoneLines)) {
                            //stub
                        } else {
                            this.m_controlPoints[0].m_direction = Vector3.zero;
                        }
                        this.m_controlPointCount = 1;

                        this.m_controlPoints[1].Clear();
                        this.m_controlPoints[2].Clear();

                        this.m_positionChanging = true;
                        result = true;
                        break;
                    }
                    case 1: {
                        Vector3 _vector1 = position;
                        Vector3 _p0 = this.m_controlPoints[0].m_position;
                        Vector3 _normVector1 = (_vector1 - _p0);
                        _normVector1.y = 0f;
                        _normVector1.Normalize();
                        this.m_controlPoints[1].m_position = _vector1;
                        if ((PropLineTool.m_snapMode == SnapMode.Objects) || (PropLineTool.m_snapMode == SnapMode.ZoneLines)) {
                            //stub
                        } else {
                            this.m_controlPoints[0].m_direction = _normVector1;
                        }
                        this.m_controlPoints[1].m_direction = _normVector1;
                        this.m_controlPointCount = 2;

                        this.m_positionChanging = true;
                        result = true;
                        break;
                    }
                    case 2: {
                        if (PropLineTool.drawMode == DrawMode.Freeform) {
                            this.m_controlPoints[2].m_position = position;
                            CalculateFreeformMiddlePoint(ref this.m_controlPoints);
                        } else //must be curved
                          {
                            Vector3 _vector2 = position;
                            Vector3 _p1 = this.m_controlPoints[1].m_position;
                            if ((_p1 == null) || (_p1 == Vector3.zero)) {
                                Debug.LogError("[PLT]: AddControlPoint(): Middle control point not found!");
                                result = false;
                                break;
                            }
                            Vector3 _normVector2 = (_vector2 - _p1);
                            _normVector2.y = 0f;
                            _normVector2.Normalize();
                            this.m_controlPoints[2].m_position = _vector2;
                            this.m_controlPoints[2].m_direction = _normVector2;
                        }

                        this.m_controlPointCount = 3;
                        this.m_positionChanging = true;
                        result = true;
                        break;
                    }
                    default: {
                        break;
                    }
                }
            } else {
                Debug.LogError("[PLT]: AddControlPoint(): m_controlPointCount is out of bounds!");
                result = false;
            }
            return result;
        }

        //custom method
        //updates active control point
        //   so that curve continuously changes (renders) with mouse movement
        //index does not have to match control point count (:
        //   actually pointNumber must be less than or equal to control point count
        //   used for changing (moving) control points in lock mode
        private bool ModifyControlPoint(Vector3 position, int pointNumber) {
            bool result = false;

            int _index = pointNumber - 1;

            int _pointCount = this.m_controlPointCount;
            bool flag = pointNumber <= (_pointCount + 1);
            bool flag2 = ((_pointCount >= 0) && (_pointCount <= 3));

            if (flag && flag2) {
                switch (_index) {
                    //first point
                    case 0: {
                        bool _isPlacement = (((_pointCount == 0) || (_pointCount == 1)) && (PropLineTool.activeState == PropLineTool.ActiveState.CreatePointFirst));
                        bool _isMovement = (((_pointCount == 2) || (_pointCount == 3)) && (PropLineTool.activeState == PropLineTool.ActiveState.MovePointFirst));
                        //bool _isSnapFirstControlPoint = ((_pointCount == 1) && (activeState == ActiveState.CreatePointSecond || activeState == ActiveState.CreatePointThird) && (drawMode == DrawMode.Straight));
                        bool _isSnapFirstControlPoint = ((_pointCount == 1 && activeState == ActiveState.CreatePointSecond) || (_pointCount == 2 && activeState == ActiveState.CreatePointThird)) && (drawMode == DrawMode.Straight || drawMode == DrawMode.Circle);

                        //placement
                        if (_isPlacement) {
                            Vector3 _vector0 = position;
                            this.m_controlPoints[0].m_position = _vector0;
                            if ((PropLineTool.m_snapMode == SnapMode.Objects) || (PropLineTool.m_snapMode == SnapMode.ZoneLines)) {
                                //stub
                            } else {
                                this.m_controlPoints[0].m_direction = Vector3.zero;
                            }

                        }
                        //when pointcount = 2, should be adjusting line startpoint
                        //when pointcount = 3, should be adjusting curve startpoint
                        //movement
                        else if (_isMovement) {
                            Vector3 _vector0 = position;
                            Vector3 _p1 = this.m_controlPoints[1].m_position;
                            Vector3 _normVector0 = (_p1 - _vector0);
                            _normVector0.y = 0f;
                            _normVector0.Normalize();
                            this.m_controlPoints[0].m_position = _vector0;
                            this.m_controlPoints[0].m_direction = _normVector0;
                            this.m_controlPoints[1].m_direction = _normVector0;

                            bool _isFreeform = (PropLineTool.drawMode == PropLineTool.DrawMode.Freeform);
                            if (_isFreeform) {
                                //do the freeform algorithm stuff

                                ReverseControlPoints3();
                                CalculateFreeformMiddlePoint(ref this.m_controlPoints);
                                ReverseControlPoints3();

                            } else {

                            }
                        } else if (_isSnapFirstControlPoint) {
                            Vector3 _vector0 = position;
                            Vector3 _p1 = this.m_controlPoints[1].m_position;
                            this.m_controlPoints[0].m_position = _vector0;
                            Vector3 _normVector1 = (_p1 - _vector0);
                            _normVector1.y = 0f;
                            _normVector1.Normalize();
                            this.m_controlPoints[0].m_direction = _normVector1;
                            this.m_controlPoints[1].m_direction = _normVector1;

                            UpdateCachedControlPoints();
                            UpdateCurves();

                        } else {
                            result = false;
                            break;
                        }

                        result = true;
                        break;
                    }
                    //second point (end for Straight mode)
                    case 1: {
                        bool _isPlacement = ((_pointCount == 1) && (PropLineTool.activeState == PropLineTool.ActiveState.CreatePointSecond));
                        bool _isMovement = (((_pointCount == 2) || (_pointCount == 3)) && (PropLineTool.activeState == PropLineTool.ActiveState.MovePointSecond));
                        bool _isFreeform = (PropLineTool.drawMode == PropLineTool.DrawMode.Freeform);

                        //for lines, this means either placement or movement
                        //for curves, this means either placement or movement
                        //placement
                        if (_isPlacement) {
                            Vector3 _vector1 = position;
                            Vector3 _p0 = this.m_controlPoints[0].m_position;
                            Vector3 _normVector1 = (_vector1 - _p0);
                            _normVector1.y = 0f;
                            _normVector1.Normalize();
                            this.m_controlPoints[1].m_position = _vector1;
                            if ((PropLineTool.m_snapMode == SnapMode.Objects) || (PropLineTool.m_snapMode == SnapMode.ZoneLines)) {
                                //stub
                            } else {
                                this.m_controlPoints[0].m_direction = _normVector1;
                            }
                            this.m_controlPoints[1].m_direction = _normVector1;
                        }
                        //movement
                        else if (_isMovement) {
                            Vector3 _vector1 = position;
                            Vector3 _p0 = this.m_controlPoints[0].m_position;
                            Vector3 _normVector1 = (_vector1 - _p0);
                            _normVector1.y = 0f;
                            _normVector1.Normalize();
                            this.m_controlPoints[1].m_position = _vector1;


                            Vector3 _vector2 = this.m_controlPoints[2].m_position;
                            Vector3 _p1 = position;
                            Vector3 _normVector2 = (_vector2 - _p1);
                            _normVector2.y = 0f;
                            _normVector2.Normalize();
                            this.m_controlPoints[2].m_direction = _normVector2;

                            if ((PropLineTool.m_snapMode == SnapMode.Objects) || (PropLineTool.m_snapMode == SnapMode.ZoneLines)) {
                                //stub
                            } else {
                                this.m_controlPoints[0].m_direction = _normVector1;
                            }
                            this.m_controlPoints[1].m_direction = _normVector1;

                            if (_isFreeform) {
                                CalculateFreeformMiddlePoint(ref this.m_controlPoints);
                            } else {

                            }
                        } else {
                            result = false;
                            break;
                        }

                        result = true;
                        break;
                    }
                    //third point (end for Curved and Freeform modes)
                    case 2: {

                        bool _isPlacement = ((_pointCount == 2) && (PropLineTool.activeState == PropLineTool.ActiveState.CreatePointThird));
                        bool _isMovement = (((_pointCount == 2) || (_pointCount == 3)) && (PropLineTool.activeState == PropLineTool.ActiveState.MovePointThird));
                        bool _isFreeform = (PropLineTool.drawMode == PropLineTool.DrawMode.Freeform);

                        //placement
                        if (_isPlacement) {
                            if (_isFreeform) {
                                this.m_controlPoints[2].m_position = position;
                                //normal Freeform algorithm from NetTool.SimulationStep
                                CalculateFreeformMiddlePoint(ref this.m_controlPoints);
                            } else {
                                Vector3 _vector2 = position;
                                Vector3 _p1 = this.m_controlPoints[1].m_position;
                                Vector3 _normVector2 = (_vector2 - _p1);
                                _normVector2.y = 0f;
                                _normVector2.Normalize();
                                this.m_controlPoints[2].m_position = _vector2;
                                this.m_controlPoints[2].m_direction = _normVector2;
                            }
                        }
                        //movement
                        else if (_isMovement) {
                            if (_isFreeform) {
                                this.m_controlPoints[2].m_position = position;
                                //normal Freeform algorithm from NetTool.SimulationStep
                                CalculateFreeformMiddlePoint(ref this.m_controlPoints);
                            } else {
                                Vector3 _vector2 = position;
                                Vector3 _p1 = this.m_controlPoints[1].m_position;
                                Vector3 _normVector2 = (_vector2 - _p1);
                                _normVector2.y = 0f;
                                _normVector2.Normalize();
                                this.m_controlPoints[2].m_position = _vector2;
                                this.m_controlPoints[2].m_direction = _normVector2;
                            }
                        } else {
                            result = false;
                            break;
                        }

                        result = true;
                        break;
                    }
                }
            } else {
                if (!flag2) {
                    Debug.LogError("[PLT]: ModifyControlPoint(): m_controlPointCount is out of bounds! (value = " + this.m_controlPointCount.ToString() + ")");
                }
                result = false;
            }
            if (result == true) {
                UpdateCachedControlPoints();
            }
            return result;
        }

        //continues the curve in non-lock mode
        //   by setting up the next segment (:
        //   and clearing the old segments
        //      by calling ResetAllControlPoints();
        //should only be called immediately after placing the end point
        private bool ContinueDrawing() {
            bool result = false;
            bool _snapping = ((PropLineTool.m_snapMode == SnapMode.Objects) || (PropLineTool.m_snapMode == SnapMode.ZoneLines));
            bool _fenceMode = PropLineTool.fenceMode == true;

            ControlPoint _p0 = this.m_controlPoints[0];
            ControlPoint _p1 = this.m_controlPoints[1];
            ControlPoint _p2 = this.m_controlPoints[2];

            ResetAllControlPoints();

            switch (PropLineTool.drawMode) {
                case DrawMode.Straight: {
                    if (_fenceMode) {
                        Vector3 _lastFenceEndpoint = placementCalculator.GetLastFenceEndpoint();

                        if (_lastFenceEndpoint == Vector3.down) {
                            return false;
                        }

                        _p1.m_position = _lastFenceEndpoint;
                    }

                    if (_snapping) {
                        this.m_controlPoints[0] = _p1;
                    } else {
                        this.m_controlPoints[0].m_position = _p1.m_position;
                    }

                    this.m_controlPointCount = 1;
                    result = true;
                    break;
                }
                case DrawMode.Curved: {
                    if (_snapping) {
                        this.m_controlPoints[0] = _p2;
                        //stub
                    } else {
                        this.m_controlPoints[0] = _p2;
                    }

                    this.m_controlPointCount = 1;
                    result = true;
                    break;
                }
                case DrawMode.Freeform: {
                    //as NetTool does, snapping doesn't matter for Freeform

                    //Freeform skips to CreatePointThird
                    this.m_controlPoints[0] = _p2;
                    this.m_controlPoints[1] = _p2;
                    this.m_controlPoints[1].m_position = _p2.m_position + _p2.m_direction;

                    this.m_controlPointCount = 2;
                    result = true;
                    break;
                }
                case DrawMode.Circle: {
                    this.m_controlPoints[0] = _p0;
                    this.m_controlPoints[1] = _p1;

                    this.m_controlPointCount = 1;
                    result = true;
                    break;
                }
                default: {
                    result = false;
                    break;
                }
            }
            if (result == true) {
                //set placementCalculator bool m_isContinueDrawing to true
                placementCalculator.SetContinueDrawing(true);

                //update cached control points
                UpdateCachedControlPoints();
            }
            return result;
        }

        private bool PostCheckAndContinue() {
            bool _continueCPResult = false;

            switch (drawMode) {
                case DrawMode.Straight:
                case DrawMode.Circle: {
                    if (PropLineTool.m_lockingMode == LockingMode.Off) {
                        //enter or remain in MaxFillContinue
                        if (placementCalculator.segmentState.isReadyForMaxContinue) {
                            placementCalculator.UpdateItemPlacementInfo(true, false);

                            GoToActiveState(ActiveState.MaxFillContinue);
                        }
                        //exit MaxFillContinue if needed, then ContinueDrawing
                        else {
                            _continueCPResult = ContinueDrawing();
                            if (_continueCPResult) {
                                //PropLineTool.m_activeState = PropLineTool.ActiveState.CreatePointSecond;
                                GoToActiveState(ActiveState.CreatePointSecond);

                                ModifyControlPoint(this.m_mousePosition, 2);

                                if (fenceMode == true && placementCalculator.segmentState.isContinueDrawing == true && drawMode == DrawMode.Straight) {
                                    ModifyControlPoint(placementCalculator.GetLastFenceEndpoint(), 1);

                                }

                                UpdateCachedControlPoints();
                                UpdateCurves();

                                placementCalculator.UpdateItemPlacementInfo(true, false);
                            } else {
                                this.ResetAllControlPoints();

                                GoToActiveState(ActiveState.CreatePointFirst);
                            }
                        }
                    } else if (PropLineTool.m_lockingMode == LockingMode.Lock) //Locking is enabled
                      {
                        PropLineTool.m_wasLockingMode = PropLineTool.m_lockingMode;
                        GoToActiveState(ActiveState.LockIdle);
                    }

                    return true;
                }
                case DrawMode.Curved:
                case DrawMode.Freeform: {
                    if (PropLineTool.m_lockingMode == LockingMode.Off) {
                        //enter or remain in MaxFillContinue
                        if (placementCalculator.segmentState.isReadyForMaxContinue) {
                            placementCalculator.UpdateItemPlacementInfo(true, false);

                            GoToActiveState(ActiveState.MaxFillContinue);
                        }
                        //exit MaxFillContinue if needed, then ContinueDrawing
                        else {
                            _continueCPResult = ContinueDrawing();
                            if (_continueCPResult) {
                                if (PropLineTool.drawMode == DrawMode.Curved) {
                                    GoToActiveState(ActiveState.CreatePointSecond);

                                    Vector3 _temp1 = this.m_controlPoints[0].m_position + 0.001f * this.m_controlPoints[0].m_direction;
                                    ModifyControlPoint(_temp1, 2);

                                    UpdateCachedControlPoints();
                                    UpdateCurves();

                                    placementCalculator.UpdateItemPlacementInfo(true, false);
                                } else if (PropLineTool.drawMode == DrawMode.Freeform) {
                                    GoToActiveState(ActiveState.CreatePointThird);

                                    Vector3 _temp2 = this.m_controlPoints[0].m_position + 0.01f * this.m_controlPoints[0].m_direction;
                                    ModifyControlPoint(_temp2, 3);

                                    UpdateCachedControlPoints();
                                    UpdateCurves();

                                    placementCalculator.UpdateItemPlacementInfo(true, false);
                                } else {
                                    this.ResetAllControlPoints();

                                    GoToActiveState(ActiveState.CreatePointFirst);
                                }
                            } else {
                                this.ResetAllControlPoints();

                                GoToActiveState(ActiveState.CreatePointFirst);
                            }
                        }
                    } else if (PropLineTool.m_lockingMode == LockingMode.Lock) //Locking is enabled
                      {

                        PropLineTool.m_wasLockingMode = PropLineTool.m_lockingMode;
                        GoToActiveState(ActiveState.LockIdle);
                    }

                    return true;
                }
                default: {
                    return false;
                }
            }
        }

        //from NetTool
        //only calculates the (middle point position) and (end point direction)!
        public void CalculateFreeformMiddlePoint(ref PropLineTool.ControlPoint[] array) {
            //PropLineTool.ControlPoint result = new ControlPoint();
            Vector3 _p2_p0 = array[2].m_position - array[0].m_position;
            Vector3 _dir_p1 = array[1].m_direction;
            _p2_p0.y = 0f;
            _dir_p1.y = 0f;
            float _sqrMag_2_0 = Vector3.SqrMagnitude(_p2_p0);
            _p2_p0 = Vector3.Normalize(_p2_p0);
            float _angle_0 = Mathf.Min(1.17809725f, Mathf.Acos(Vector3.Dot(_p2_p0, _dir_p1)));
            float _dist_p1_p0 = Mathf.Sqrt(0.5f * _sqrMag_2_0 / Mathf.Max(0.001f, 1f - Mathf.Cos(3.14159274f - 2f * _angle_0)));
            array[1].m_position = array[0].m_position + _dir_p1 * _dist_p1_p0;
            Vector3 _dir_p2 = array[2].m_position - array[1].m_position;
            _dir_p2.y = 0f;
            _dir_p2.Normalize();
            array[2].m_direction = _dir_p2;

            //sometimes things don't work corrently
            if (float.IsNaN(array[1].m_position.x) || float.IsNaN(array[1].m_position.y) || float.IsNaN(array[1].m_position.z) || array[1].m_position == null) {
                array[1].m_position = array[0].m_position + 0.01f * array[0].m_direction;
                array[1].m_direction = array[0].m_direction;
            }
        }

        protected override void OnEnable() {
            base.OnEnable();

            //custom precalls
            if (m_keepActiveState == false) {
                ResetPLT();
            } else if (m_initialized == false) {
                m_initialized = true;

                ResetPLT();
            } else if (m_keepActiveState == true) {
                //do nothing extra
            }

            m_keepActiveState = true;
        }

        protected override void OnDisable() {
            base.OnDisable();
            base.ToolCursor = null;

            //custom postcalls
            if (m_keepActiveState == false) {
                ResetPLT();
            } else if (m_keepActiveState == true) {

            }

            //ToolBase base method (all 3)
            this.m_mouseRayValid = false;
        }

        protected void ResetPLT() {
            //NetTool base methods
            this.m_controlPointCount = 0;
            this.m_cachedControlPointCount = 0;
            this.m_lengthChanging = false;
            this.m_positionChanging = false;
            GoToActiveState(ActiveState.CreatePointFirst);
            PropLineTool.objectMode = PropLineTool.ObjectMode.Undefined;
            //reset control points
            this.ResetAllControlPoints();

            //reset curves
            m_mainArm1 = new Segment3();
            m_mainArm2 = new Segment3();
            m_mainSegment = new Segment3();
            m_mainBezier = new Bezier3();
            m_mainCircle = new Circle3XZ();
            m_rawCircle = new Circle3XZ();

            //reset placement calculator
            placementCalculator.Reset();

            Singleton<TerrainManager>.instance.RenderZones = false;

            //update undo previews
            undoManager.CheckItemsStillExist();
        }

        private void GoToActiveState(ActiveState state) {
            ActiveState _oldState = activeState;
            activeState = state;

            if (state == _oldState) {
                return;
            }

            //takes care of any overhead that needs to be done
            //when switching states
            switch (state) {
                case ActiveState.Undefined: {
                    break;
                }
                case ActiveState.CreatePointFirst: {
                    placementCalculator.SetContinueDrawing(false);
                    placementCalculator.FinalizeForPlacement(false);

                    //PropLineToolMod.optionPanel.AllDrawModeButtonsEnabler(true);
                    break;
                }
                case ActiveState.CreatePointSecond: {
                    //PropLineToolMod.optionPanel.AllDrawModeButtonsEnabler(true);
                    break;
                }
                case ActiveState.CreatePointThird: {
                    //PropLineToolMod.optionPanel.AllDrawModeButtonsEnabler(true);
                    break;
                }
                case ActiveState.LockIdle: {
                    UpdateCachedControlPoints();
                    UpdateCurves();

                    if (state == _oldState) {
                        placementCalculator.UpdateItemPlacementInfo(true, true);
                    } else {
                        switch (_oldState) {
                            case ActiveState.CreatePointFirst:
                            case ActiveState.CreatePointSecond:
                            case ActiveState.CreatePointThird: {
                                placementCalculator.UpdateItemPlacementInfo();
                                break;
                            }
                            case ActiveState.MovePointFirst:
                            case ActiveState.MovePointSecond:
                            case ActiveState.MovePointThird:
                            case ActiveState.MoveSegment:
                            case ActiveState.ChangeAngle: {
                                placementCalculator.UpdateItemPlacementInfo();
                                break;
                            }
                            case ActiveState.ChangeSpacing: {
                                placementCalculator.UpdateItemPlacementInfo(true, false);
                                break;
                            }
                            case ActiveState.MaxFillContinue: {
                                //placementCalculator.UpdateItemPlacementInfo(true, true);

                                //Keep This One
                                placementCalculator.UpdateItemPlacementInfo(true, false);
                                break;
                            }
                            default: {
                                placementCalculator.UpdateItemPlacementInfo();
                                break;
                            }
                        }
                    }

                    int _offset = fenceMode == true ? 0 : (controlMode == ControlMode.Itemwise ? 0 : 1);
                    if (placementCalculator.GetItemCountActual() >= (1 + _offset)) {
                        //hoverAngle = m_placementInfo[1].angle - placementCalculator.modelAngleOffset + Mathf.PI;
                        hoverAngle = m_placementInfo[hoverItemAngleCenterIndex].angle - placementCalculator.totalPropertyAngleOffset + Mathf.PI;
                    }
                    break;
                }
                case ActiveState.MovePointFirst: {
                    lockBackupControlPoints[0] = m_cachedControlPoints[0];
                    break;
                }
                case ActiveState.MovePointSecond: {
                    lockBackupControlPoints[1] = m_cachedControlPoints[1];
                    break;
                }
                case ActiveState.MovePointThird: {
                    lockBackupControlPoints[2] = m_cachedControlPoints[2];
                    break;
                }
                case ActiveState.MoveSegment: {
                    for (int i = 0; i < m_controlPoints.Length; i++) {
                        lockBackupControlPoints[i].m_position = m_controlPoints[i].m_position;
                    }
                    lockBackupCachedPosition = m_cachedPosition;
                    break;
                }
                case ActiveState.ChangeSpacing: {
                    lockBackupSpacing = placementCalculator.spacingSingle;
                    break;
                }
                case ActiveState.ChangeAngle: {
                    lockBackupAngleSingle = placementCalculator.angleSingle;
                    lockBackupAngleOffset = placementCalculator.angleOffset;
                    lockBackupItemSecondAngle = m_placementInfo[hoverItemAngleCenterIndex].angle;
                    lockBackupItemDirection = m_placementInfo[hoverItemAngleCenterIndex].itemDirection;
                    break;
                }
                case ActiveState.ItemwiseLock: {
                    //nothing here...
                    break;
                }
                case ActiveState.MoveItemwiseItem: {
                    lockBackupItemwiseT = hoverItemwiseT;
                    break;
                }
                case ActiveState.MaxFillContinue: {
                    //if (_oldState == ActiveState.LockIdle)
                    //{
                    //    placementCalculator.UpdateItemPlacementInfo(true, true);
                    //}
                    //else
                    //{
                    //    placementCalculator.UpdateItemPlacementInfo();
                    //}

                    placementCalculator.UpdateItemPlacementInfo();

                    break;
                }
                default: {
                    activeState = ActiveState.Undefined;
                    break;
                }
            }

        }

        //called implicitly in OnToolGUI
        //updates cached control points just outside of the switch block
        private void ProcessKeyInputImpl(KeyPressEvent key) {
            //UI
            bool _isInsideUI = this.m_toolController.IsInsideUI;
            bool _isMouseRayValid = this.m_mouseRayValid;
            bool _isMapClick = (!_isInsideUI && _isMouseRayValid);

            //control points
            bool _addCPResult = false;
            bool _cancelCPResult = false;
            //bool _continueCPResult = false;

            //error checking
            bool _successfulRayCast = false;
            ToolBase.ToolErrors _toolErrors = new ToolBase.ToolErrors();

            if (key._mouseDown) {
                if (key._leftClick) {
                    this.m_mouseLeftDown = true;
                }
                if (key._rightClick) {
                    this.m_mouseRightDown = true;
                }
            } else if (key._mouseUp) {
                if (key._leftClickRelease) {
                    this.m_mouseLeftDown = false;
                }
                if (key._rightClickRelease) {
                    this.m_mouseRightDown = false;
                }
            }

            //update undo previews on Ctrl-key down
            if (!this.m_keyboardCtrlDown && key._ctrl && key._keyDown) {
                undoManager.CheckItemsStillExist();
            }

            this.m_keyboardAltDown = key._alt;
            this.m_keyboardCtrlDown = key._ctrl;
            //this.m_keyboardShiftDown = key._shift;
            this.m_keyboardShiftDown = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

            //test for Esc and reset if pressed
            if (key._esc) {
                ResetPLT();

                return;
            }

            //test for undo before alt-clicks
            if (!_isInsideUI && key._ctrlZ) {
                SegmentInfo _segmentInfo = SegmentInfo.defaultValue;
                bool _resultUndo = undoManager.UndoLatestEntry(out _segmentInfo);

                //push back MFC offset
                bool _undoMaxFillContinue = _segmentInfo.m_isMaxFillContinue || _segmentInfo.isReadyForMaxContinue;
                if (_resultUndo && _undoMaxFillContinue) {
                    placementCalculator.RevertLastContinueParameters(_segmentInfo.m_lastFinalOffset, _segmentInfo.m_lastFenceEndpoint);
                }

                placementCalculator.UpdateItemPlacementInfo();
            }

            if (m_keyboardAltDown) {
                isCopyPlacing = true;
            } else {
                isCopyPlacing = false;
            }

            //test for alt-clicks before clicks or ctrl-clicks
            //test for ctrl-clicks before clicks

            if (!_isInsideUI && key._altOnlyLeftClick) {
                isCopyPlacing = true;

                switch (activeState) {
                    case ActiveState.Undefined:
                    case ActiveState.CreatePointFirst: {
                        break;
                    }
                    case ActiveState.CreatePointSecond: {
                        if (drawMode == DrawMode.Straight || drawMode == DrawMode.Circle) {
                            FinalizePlacement(true, true);
                            return;
                        }
                        break;
                    }
                    case ActiveState.CreatePointThird: {
                        if (drawMode == DrawMode.Curved || drawMode == DrawMode.Freeform) {
                            FinalizePlacement(true, true);
                            return;
                        }
                        break;
                    }
                    case ActiveState.LockIdle:
                    case ActiveState.MovePointFirst:
                    case ActiveState.MovePointSecond:
                    case ActiveState.MovePointThird:
                    case ActiveState.MoveSegment:
                    case ActiveState.ChangeSpacing:
                    case ActiveState.ChangeAngle:
                    case ActiveState.ItemwiseLock:
                    case ActiveState.MoveItemwiseItem:
                    case ActiveState.MaxFillContinue: {
                        FinalizePlacement(true, true);
                        return;
                    }
                    default: {
                        break;
                    }
                }
            }
            switch (activeState) {
                //state set to undefined
                case ActiveState.Undefined: {
                    Debug.LogError("[PLT]: ProcessKeyInputImpl(): Active state is set to undefined!");
                    break;
                }

                //creating first control point
                case ActiveState.CreatePointFirst: {
                    if (!_isInsideUI && key._leftClick) {
                        placementCalculator.FinalizeForPlacement(false);

                        _addCPResult = AddControlPoint(this.m_cachedPosition);
                        if (this.m_mouseRayValid && _addCPResult) {
                            GoToActiveState(ActiveState.CreatePointSecond);
                            ModifyControlPoint(this.m_mousePosition, 2); //update second point to mouse position
                            UpdateCachedControlPoints();
                            UpdateCurves();

                            placementCalculator.UpdateItemPlacementInfo(false, false);
                        }
                    }


                    break;
                }

                //creating second control point
                case ActiveState.CreatePointSecond: {
                    if (!_isInsideUI) {
                        bool _proceedToItemwiseLock = false;
                        bool _twoPointedDrawMode = drawMode == DrawMode.Straight || drawMode == DrawMode.Circle;
                        _proceedToItemwiseLock = controlMode == ControlMode.Itemwise && key._leftClick && _twoPointedDrawMode;

                        if (key._ctrlOnlyLeftClick && _twoPointedDrawMode) {
                            //check if curve is too short
                            if (IsCurveLengthLongEnoughXZ() == false) {
                                return;
                            }

                            _addCPResult = AddControlPoint(this.m_cachedPosition);
                            if (this.m_mouseRayValid && _addCPResult) {
                                PropLineTool.m_wasLockingMode = PropLineTool.m_lockingMode;

                                GoToActiveState(ActiveState.LockIdle);
                            }
                            break;
                        } else if (_proceedToItemwiseLock) {
                            //check if curve is too short
                            if (IsCurveLengthLongEnoughXZ() == false) {
                                return;
                            }

                            _addCPResult = AddControlPoint(this.m_cachedPosition);
                            if (this.m_mouseRayValid && _addCPResult) {
                                PropLineTool.m_wasLockingMode = PropLineTool.m_lockingMode;

                                GoToActiveState(ActiveState.ItemwiseLock);
                            }
                            break;
                        } else if (key._leftClick) {
                            if (_twoPointedDrawMode) {
                                //check if curve is too short
                                if (IsCurveLengthLongEnoughXZ() == false) {
                                    return;
                                }

                                if (!IsAtLeastOneItemValidPlacement()) {
                                    return;
                                }

                                _addCPResult = AddControlPoint(this.m_cachedPosition);
                                if (this.m_mouseRayValid && _addCPResult) {
                                    //finalize placement
                                    FinalizePlacement(true, false);

                                    //post-process
                                    if (!PostCheckAndContinue()) {
                                        ResetAllControlPoints();

                                        GoToActiveState(ActiveState.CreatePointFirst);
                                    }

                                }
                            } else   //Curved drawmode
                              {
                                _addCPResult = AddControlPoint(this.m_cachedPosition);
                                if (this.m_mouseRayValid && _addCPResult) {
                                    GoToActiveState(ActiveState.CreatePointThird);
                                    ModifyControlPoint(this.m_mousePosition, 3); //update third point to mouse position
                                    UpdateCachedControlPoints();
                                    UpdateCurves();

                                    placementCalculator.UpdateItemPlacementInfo(true, false);
                                }
                            }
                        } else if (key._rightClick) {
                            _cancelCPResult = CancelControlPoint();
                            if (this.m_mouseRayValid && _cancelCPResult) {
                                GoToActiveState(ActiveState.CreatePointFirst);
                                ModifyControlPoint(this.m_mousePosition, 1); //update position of first point
                                UpdateCachedControlPoints();
                                UpdateCurves();

                                placementCalculator.UpdateItemPlacementInfo(false, false);
                            }
                        }


                    }

                    break;
                }

                //creating third control point
                case ActiveState.CreatePointThird: {
                    if (!_isInsideUI) {
                        bool _proceedToItemwiseLock = false;
                        _proceedToItemwiseLock = controlMode == ControlMode.Itemwise && key._leftClick;

                        if (key._ctrlOnlyLeftClick) {
                            //check if curve is too short
                            if (IsCurveLengthLongEnoughXZ() == false) {
                                return;
                            }

                            _addCPResult = AddControlPoint(this.m_cachedPosition);
                            if (this.m_mouseRayValid && _addCPResult) {
                                PropLineTool.m_wasLockingMode = PropLineTool.m_lockingMode;

                                GoToActiveState(ActiveState.LockIdle);
                            }
                            break;
                        } else if (_proceedToItemwiseLock) {
                            //check if curve is too short
                            if (IsCurveLengthLongEnoughXZ() == false) {
                                return;
                            }

                            _addCPResult = AddControlPoint(this.m_cachedPosition);
                            if (this.m_mouseRayValid && _addCPResult) {
                                PropLineTool.m_wasLockingMode = PropLineTool.m_lockingMode;

                                GoToActiveState(ActiveState.ItemwiseLock);
                            }
                            break;
                        } else if (key._leftClick) {
                            //check if curve is too short
                            if (IsCurveLengthLongEnoughXZ() == false) {
                                return;
                            }

                            if (!IsAtLeastOneItemValidPlacement()) {
                                return;
                            }

                            _addCPResult = AddControlPoint(this.m_cachedPosition);
                            if (this.m_mouseRayValid && _addCPResult) {
                                //finalize placement
                                FinalizePlacement(true, false);

                                //post-process
                                if (!PostCheckAndContinue()) {
                                    ResetAllControlPoints();

                                    GoToActiveState(ActiveState.CreatePointFirst);
                                }
                            }
                        } else if (key._rightClick) {
                            _cancelCPResult = CancelControlPoint();
                            if (this.m_mouseRayValid && _cancelCPResult) {
                                GoToActiveState(ActiveState.CreatePointSecond);
                                ModifyControlPoint(this.m_mousePosition, 2); //update position of second point
                                UpdateCachedControlPoints();
                                UpdateCurves();

                                placementCalculator.UpdateItemPlacementInfo(false, false);
                            }
                        }


                    }

                    break;
                }

                //in lock mode, awaiting user input
                case ActiveState.LockIdle: {
                    if (!_isInsideUI) {
                        if (controlMode == ControlMode.Itemwise && key._ctrlOnlyLeftClick) {
                            GoToActiveState(ActiveState.ItemwiseLock);
                            break;
                        }
                        if (key._ctrlEnter || key._ctrlOnlyLeftClick) {
                            ContinueDrawingFromLockMode(_isMouseRayValid, true);
                        } else if (key._rightClick) {
                            RevertDrawingFromLockMode();
                        } else if (key._leftClick && hoverState != HoverState.Unbound) {
                            switch (hoverState) {
                                case HoverState.SpacingLocus: {
                                    GoToActiveState(ActiveState.ChangeSpacing);
                                    break;
                                }
                                case HoverState.AngleLocus: {
                                    GoToActiveState(ActiveState.ChangeAngle);
                                    break;
                                }
                                case HoverState.ControlPointFirst: {
                                    GoToActiveState(ActiveState.MovePointFirst);

                                    ModifyControlPoint(this.m_mousePosition, 1); //update first point to mouse position
                                    UpdateCachedControlPoints();
                                    UpdateCurves();
                                    placementCalculator.UpdateItemPlacementInfo();

                                    break;
                                }
                                case HoverState.ControlPointSecond: {
                                    GoToActiveState(ActiveState.MovePointSecond);

                                    ModifyControlPoint(this.m_mousePosition, 2); //update second point to mouse position
                                    UpdateCachedControlPoints();
                                    UpdateCurves();
                                    placementCalculator.UpdateItemPlacementInfo();

                                    break;
                                }
                                case HoverState.ControlPointThird: {
                                    GoToActiveState(ActiveState.MovePointThird);

                                    ModifyControlPoint(this.m_mousePosition, 3); //update third point to mouse position
                                    UpdateCachedControlPoints();
                                    UpdateCurves();
                                    placementCalculator.UpdateItemPlacementInfo();

                                    break;
                                }
                                case HoverState.Curve: {
                                    GoToActiveState(ActiveState.MoveSegment);
                                    break;
                                }
                                case HoverState.ItemwiseItem: {
                                    if (controlMode == ControlMode.Itemwise) {
                                        GoToActiveState(ActiveState.MoveItemwiseItem);
                                    }
                                    break;
                                }
                                default: {

                                    break;
                                }
                            }


                        }


                    }
                    //inside UI and changing userParameters or switching prefabs
                    else if (key._leftClick) {
                        UpdatePrefabs();    //should be called after placementCalculator.GetPrefabData
                        placementCalculator.UpdateItemPlacementInfo();
                    }


                    break;
                }

                //in lock mode, moving first control point
                case ActiveState.MovePointFirst: {
                    if (!_isInsideUI) {
                        if (key._rightClick) {
                            //reset first CP to original position
                            ModifyControlPoint(lockBackupControlPoints[0].m_position, 1);
                        } else if (key._leftClick) {
                            //check if curve is now too short
                            if (IsCurveLengthLongEnoughXZ() == false) {
                                return;
                            }

                            //set first CP to new position

                            //(don't need to do anything extra)
                        }
                        if (key._leftClick || key._rightClick) {
                            GoToActiveState(ActiveState.LockIdle);
                        }
                    }
                    break;
                }

                //in lock mode, moving second control point
                case ActiveState.MovePointSecond: {
                    if (!_isInsideUI) {
                        if (key._rightClick) {
                            //reset second CP to original position
                            ModifyControlPoint(lockBackupControlPoints[1].m_position, 2);
                        } else if (key._leftClick) {
                            //check if curve is now too short
                            if (IsCurveLengthLongEnoughXZ() == false) {
                                return;
                            }

                            //set second CP to new position

                            //(don't need to do anything extra)
                        }
                        if (key._leftClick || key._rightClick) {
                            GoToActiveState(ActiveState.LockIdle);
                        }
                    }
                    break;
                }

                //in lock mode, moving third control point
                case ActiveState.MovePointThird: {
                    if (!_isInsideUI) {
                        if (key._rightClick) {
                            //reset third CP to original position
                            ModifyControlPoint(lockBackupControlPoints[2].m_position, 3);
                        } else if (key._leftClick) {
                            //check if curve is now too short
                            if (IsCurveLengthLongEnoughXZ() == false) {
                                return;
                            }

                            //set third CP to new position

                            //(don't need to do anything extra)
                        }
                        if (key._leftClick || key._rightClick) {
                            GoToActiveState(ActiveState.LockIdle);
                        }
                    }
                    break;
                }

                //in lock mode, moving full line or curve
                case ActiveState.MoveSegment: {
                    if (!_isInsideUI) {
                        if (key._rightClick) {
                            GoToActiveState(ActiveState.LockIdle);

                            for (int i = 0; i < m_controlPoints.Length; i++) {
                                m_controlPoints[i].m_position = lockBackupControlPoints[i].m_position;
                            }

                            UpdateCachedControlPoints();
                            UpdateCurves();

                            placementCalculator.UpdateItemPlacementInfo();
                        } else if (key._leftClick) {
                            //set curve to new position

                            //(don't need to do anything extra)
                        }
                        if (key._leftClick || key._rightClick) {
                            GoToActiveState(ActiveState.LockIdle);
                        }
                    }
                    break;
                }

                //in lock mode, changing item-to-item spacing along the line or curve
                case ActiveState.ChangeSpacing: {
                    if (!_isInsideUI) {
                        if (key._rightClick) {
                            //reset spacing to original value
                            placementCalculator.spacingSingle = lockBackupSpacing;
                        } else if (key._leftClick) {
                            //set spacing to new value

                            //(don't need to do anything extra)
                        }
                        if (key._leftClick || key._rightClick) {
                            GoToActiveState(ActiveState.LockIdle);
                        }
                    }
                    break;
                }

                //in lock mode, changing initial item (first item's) angle
                case ActiveState.ChangeAngle: {
                    if (!_isInsideUI) {
                        if (key._rightClick) {
                            //reset angle to original value
                            placementCalculator.angleOffset = lockBackupAngleOffset;
                            placementCalculator.angleSingle = lockBackupAngleSingle;
                        } else if (key._leftClick) {
                            //set angle to new value

                            //(don't need to do anything extra)
                        }
                        if (key._leftClick || key._rightClick) {
                            GoToActiveState(ActiveState.LockIdle);
                        }
                    }
                    break;
                }
                case ActiveState.ItemwiseLock: {
                    if (!_isInsideUI) {
                        if (key._ctrlOnlyLeftClick) {
                            GoToActiveState(ActiveState.LockIdle);
                        } else if (key._ctrlEnter) {
                            ContinueDrawingFromLockMode(_isMouseRayValid, false);
                        } else if (key._rightClick) {
                            RevertDrawingFromLockMode();
                        } else if (key._leftClick && hoverState != HoverState.Unbound) {
                            switch (hoverState) {
                                case HoverState.ItemwiseItem: {
                                    FinalizePlacement(true, true);
                                    break;
                                }
                                default: {
                                    //do nothing
                                    break;
                                }
                            }
                        }
                    }
                    //inside UI and changing userParameters or switching prefabs
                    else if (key._leftClick) {
                        UpdatePrefabs();    //should be called after placementCalculator.GetPrefabData
                        placementCalculator.UpdateItemPlacementInfo();
                    }
                    break;
                }
                case ActiveState.MoveItemwiseItem: {
                    if (!_isInsideUI) {
                        if (key._rightClick) {
                            //reset item back to original position
                            hoverItemwiseT = lockBackupItemwiseT;
                        } else if (key._leftClick) {
                            //set itemT in DiscoverHoverState

                            //(don't need to do anything extra)
                        }
                        if (key._leftClick || key._rightClick) {
                            GoToActiveState(ActiveState.LockIdle);
                        }
                    }
                    break;
                }
                case ActiveState.MaxFillContinue: {
                    if (controlMode == ControlMode.Itemwise) {
                        GoToActiveState(ActiveState.ItemwiseLock);
                    } else if (!_isInsideUI) {
                        if (key._ctrlOnlyLeftClick || key._rightClick) {
                            if (this.m_mouseRayValid) {
                                GoToActiveState(ActiveState.LockIdle);
                            }
                        } else if (key._leftClick) {
                            //check if curve is too short
                            if (IsCurveLengthLongEnoughXZ() == false) {
                                return;
                            }
                            if (!IsAtLeastOneItemValidPlacement()) {
                                return;
                            }

                            if (this.m_mouseRayValid) {
                                //finalize placement
                                FinalizePlacement(true, false);

                                //post-process
                                if (!PostCheckAndContinue()) {
                                    ResetAllControlPoints();

                                    GoToActiveState(ActiveState.CreatePointFirst);
                                }
                            }
                        } else {
                            return;
                        }
                    }


                    break;
                }
                //out of bounds
                default: {
                    Debug.LogError("[PLT]: ProcessKeyInputImpl(): Active state does not match a case!");
                    break;
                }
            }

            if (_successfulRayCast == false) {
                _toolErrors |= ToolBase.ToolErrors.RaycastFailed;
            }
        }
        private IEnumerator ProcessKeyInput(KeyPressEvent _key_) {
            return new TProcessKeyEventTc_Iterator32G { _keyPressEvent = _key_, TTf__this = this };
        }

        private void ContinueDrawingFromLockMode(bool isMouseRayValid, bool finalizePlacement) {
            //check if in fence mode and line is too short
            if (PropLineTool.fenceMode == true && placementCalculator.GetItemCountActual() <= 0) {
                return;
            }

            if (isMouseRayValid) {
                if (finalizePlacement) {
                    //finalize placement
                    if (!FinalizePlacement(true, false)) {
                        return;
                    }

                    //post-process
                    if (!PostCheckAndContinue()) {
                        ResetAllControlPoints();

                        GoToActiveState(ActiveState.CreatePointFirst);

                        return;
                    }
                }
            }

            //bool _continueCPResult = false;
            //bool _twoPointedDrawMode = drawMode == DrawMode.Straight || drawMode == DrawMode.Circle;
            //if (_twoPointedDrawMode)
            //{
            //    if (isMouseRayValid)
            //    {
            //        //if readyForMaxContinue, then go to ActiveState.MaxFillContinue and return;
            //        //   ...thereby skipping the call to ContinueDrawing();
            //        //   --> make sure to check if isMouseRayValid is true

            //        //Think about how to integrate MaxFillContinue into the PLT flow
            //        //   --> where (ActiveState) do you get sent if you right-click?
            //        //   --> Should MaxFillContinue even be an active state?

            //        _continueCPResult = ContinueDrawing();
            //    }
            //    if (_continueCPResult)
            //    {
            //        GoToActiveState(ActiveState.CreatePointSecond);

            //        ModifyControlPoint(this.m_mousePosition, 2);

            //        if (fenceMode == true)
            //        {
            //            Vector3 _snapPosition = controlMode == ControlMode.Itemwise ? m_mainSegment.b : placementCalculator.GetLastFenceEndpoint();

            //            ModifyControlPoint(_snapPosition, 1);
            //        }

            //        UpdateCachedControlPoints();
            //        UpdateCurves();

            //        placementCalculator.UpdateItemPlacementInfo(true, false);
            //    }
            //    else
            //    {
            //        this.ResetAllControlPoints();
            //        //PropLineTool.activeState = PropLineTool.ActiveState.CreatePointFirst;
            //        GoToActiveState(ActiveState.CreatePointFirst);
            //    }


            //}
            //else if ((PropLineTool.drawMode == PropLineTool.DrawMode.Curved) || (PropLineTool.drawMode == PropLineTool.DrawMode.Freeform)) //redundant (for now)
            //{
            //    if (isMouseRayValid)
            //    {
            //        _continueCPResult = ContinueDrawing();
            //    }
            //    if (_continueCPResult)
            //    {
            //        if (PropLineTool.drawMode == DrawMode.Curved)
            //        {
            //            GoToActiveState(ActiveState.CreatePointSecond);

            //            ModifyControlPoint(this.m_mousePosition, 2);
            //            UpdateCachedControlPoints();
            //            UpdateCurves();

            //            placementCalculator.UpdateItemPlacementInfo(true, false);
            //        }
            //        else if (PropLineTool.drawMode == DrawMode.Freeform)
            //        {
            //            GoToActiveState(ActiveState.CreatePointThird);

            //            ModifyControlPoint(this.m_mousePosition, 3);
            //            UpdateCachedControlPoints();
            //            UpdateCurves();

            //            placementCalculator.UpdateItemPlacementInfo(true, false);
            //        }
            //        else
            //        {
            //            this.ResetAllControlPoints();

            //            GoToActiveState(ActiveState.CreatePointFirst);
            //        }
            //    }
            //    else
            //    {
            //        this.ResetAllControlPoints();

            //        GoToActiveState(ActiveState.CreatePointFirst);
            //    }
            //}
        }

        private void RevertDrawingFromLockMode() {
            bool _cancelCPResult = CancelControlPoint();
            if (this.m_mouseRayValid && _cancelCPResult) {
                bool _twoPointedDrawMode = drawMode == DrawMode.Straight || drawMode == DrawMode.Circle;

                if (_twoPointedDrawMode) {
                    GoToActiveState(ActiveState.CreatePointSecond);
                    ModifyControlPoint(this.m_mousePosition, 2); //update position of first point
                    UpdateCachedControlPoints();
                    UpdateCurves();

                    placementCalculator.UpdateItemPlacementInfo(false, false);
                } else if (PropLineTool.drawMode == PropLineTool.DrawMode.Curved) {
                    GoToActiveState(ActiveState.CreatePointThird);
                    ModifyControlPoint(this.m_mousePosition, 3); //update position of second point
                    UpdateCachedControlPoints();
                    UpdateCurves();

                    placementCalculator.UpdateItemPlacementInfo(false, false);
                } else if (PropLineTool.drawMode == DrawMode.Freeform) {
                    GoToActiveState(ActiveState.CreatePointThird);
                    ModifyControlPoint(this.m_mousePosition, 3); //update third point to mouse position
                    UpdateCachedControlPoints();
                    UpdateCurves();

                    placementCalculator.UpdateItemPlacementInfo(false, false);
                }
            }
        }

        protected override void OnToolGUI(Event e) {

            bool _isMouseEvent = (e.isMouse);
            bool _isKeyEvent = (e.isKey);

            if (!_isMouseEvent && !_isKeyEvent) {
                return;
            }

            PropLineTool.m_keyPressEvent.Set3(e);

            Singleton<SimulationManager>.instance.AddAction(this.ProcessKeyInput(m_keyPressEvent));

        }

        protected override void OnToolLateUpdate() {
            //_DEBUG_OnToolLateUpdate.FrameStart();
            OnToolLateUpdateImpl();
            //_DEBUG_OnToolLateUpdate.FrameEnd(true);
        }

        //OnToolLateUpdate
        protected void OnToolLateUpdateImpl() {
            //All 3 stuff
            Vector3 _mousePosition = Input.mousePosition;
            this.m_mouseRay = Camera.main.ScreenPointToRay(_mousePosition);
            this.m_mouseRayLength = Camera.main.farClipPlane;
            this.m_mouseRayValid = (!this.m_toolController.IsInsideUI && Cursor.visible);

            //Prop/Tree stuff
            UpdateCachedPosition(false); //also updates m_positionChanging

            //Net stuff
            if (this.m_lengthTimer > 0f) {
                this.m_lengthTimer = Mathf.Max(0f, this.m_lengthTimer - Time.deltaTime);
            }

            //check if user switched from Curved/Freeform(in CreatePointThird) -> Straight/Circle
            if ((drawMode == DrawMode.Straight || drawMode == DrawMode.Circle) && activeState == ActiveState.CreatePointThird) {
                bool _cancelCPResult = CancelControlPoint();
                if (_cancelCPResult) {
                    GoToActiveState(ActiveState.CreatePointSecond);
                    ModifyControlPoint(this.m_mousePosition, 2); //update second point to mouse position
                    UpdateCachedControlPoints();
                    UpdateCurves();
                } else {
                    this.ResetAllControlPoints();

                    GoToActiveState(ActiveState.CreatePointFirst);
                    ModifyControlPoint(this.m_mousePosition, 1);
                    UpdateCachedControlPoints();
                    UpdateCurves();
                }
                return;
            }

            //consider moving to event subscription in Awake() or OnDrawModeChanged()
            //check if user switched from Straight/Circle(in CreatePointSecond and continueDrawing fenceMode) -> Curved/Freeform
            if ((drawMode == DrawMode.Curved || drawMode == DrawMode.Freeform) && activeState == ActiveState.CreatePointSecond && placementCalculator.segmentState.IsPositionEqualToLastFenceEndpoint(m_cachedControlPoints[0].m_position)) {
                placementCalculator.ResetLastContinueParameters();
            }
            if (activeState == ActiveState.CreatePointFirst && placementCalculator.segmentState.IsPositionEqualToLastFenceEndpoint(m_cachedControlPoints[0].m_position)) {
                placementCalculator.ResetLastContinueParameters();
            }
            //check if control point count is > 0 in CreatePointFirst
            if (activeState == ActiveState.CreatePointFirst && m_controlPointCount > 0) {
                m_controlPointCount = 0;
            }

            //SUPER IMPORTANT
            //continuously update control points to follow mouse when applicable
            if (this.m_positionChanging) {
                UpdateControlPoints();
                DiscoverHoverState(m_cachedPosition);
                UpdateMiscHoverParameters();
            }

            //Prop/Tree stuff
            UpdateCachedPosition(false);

            //New as of 170628
            CheckPendingPlacement();
            //if (m_positionChanging)
            //{
            //    placementCalculator.UpdateItemPlacementInfo();
            //}

            if (PropLineTool.m_snapMode == PropLineTool.SnapMode.ZoneLines) {
                Singleton<TerrainManager>.instance.RenderZones = true;
            } else {
                Singleton<TerrainManager>.instance.RenderZones = false;
            }
        }

        protected override void OnToolUpdate() {
            //_DEBUG_OnToolUpdate.FrameStart();
            OnToolUpdateImpl();
            //_DEBUG_OnToolUpdate.FrameEnd(true);
        }

        protected void OnToolUpdateImpl() {

            UpdateCachedControlPoints();

            Vector3 _hitPos = new Vector3();
            if (TryRaycast(out _hitPos)) {
                if (m_mousePosition != _hitPos) {
                    m_mousePosition = _hitPos;
                }
            }

            //trying more stuff to have curves perfectly follow mouse
            if (DoesPositionNeedUpdating(out _hitPos)) {
                this.m_cachedPosition = _hitPos;
                UpdateControlPoints();
            }

            //don't like how this looks afterall
            //ShowToolInfo();

        }

        public int GetConstructionCostTotal() {
            int _totalCost = 0;

            int _itemCount = placementCalculator.GetItemCountActual();

            switch (objectMode) {
                case ObjectMode.Props: {
                    for (int i = 0; i < _itemCount; i++) {
                        if (PlacementCalculator.IsIndexWithinBounds(i, false)) {
                            if (m_placementInfo[i].isValidPlacement) {
                                _totalCost += m_placementInfo[i].propInfo.GetConstructionCost();
                            }
                        }
                    }
                    break;
                }
                case ObjectMode.Trees: {
                    for (int i = 0; i < _itemCount; i++) {
                        if (PlacementCalculator.IsIndexWithinBounds(i, false)) {
                            if (m_placementInfo[i].isValidPlacement) {
                                _totalCost += m_placementInfo[i].treeInfo.GetConstructionCost();
                            }
                        }
                    }
                    break;
                }
                default: {
                    _totalCost = 0;
                    break;
                }
            }

            return _totalCost;
        }

        public bool ShowConstructionCostToolInfo() {
            if (placementCalculator.GetItemCountActual() <= 0) {
                return false;
            }

            int _constructionCost = GetConstructionCostTotal();

            if (_constructionCost <= 0) {
                return false;
            }

            bool _modeHasGameFlag = (this.m_toolController.m_mode & ItemClass.Availability.Game) != ItemClass.Availability.None;
            bool _preCheck = !this.m_toolController.IsInsideUI && Cursor.visible && _modeHasGameFlag;
            bool _showCostInfo = false;

            switch (activeState) {
                case ActiveState.CreatePointSecond: {
                    if (drawMode == DrawMode.Straight || drawMode == DrawMode.Circle) {
                        _showCostInfo = true;
                    }
                    break;
                }
                case ActiveState.CreatePointThird: {
                    if (drawMode == DrawMode.Curved || drawMode == DrawMode.Freeform) {
                        _showCostInfo = true;
                    }
                    break;
                }
                case ActiveState.LockIdle: {
                    if (hoverState == HoverState.Unbound) {
                        _showCostInfo = true;
                    }
                    break;
                }
                case ActiveState.MaxFillContinue: {
                    _showCostInfo = true;
                    break;
                }
                case ActiveState.MovePointFirst:
                case ActiveState.MovePointSecond:
                case ActiveState.MovePointThird:
                case ActiveState.ChangeAngle:
                case ActiveState.MoveSegment:
                case ActiveState.ChangeSpacing: {
                    _showCostInfo = false;
                    break;
                }
                default:
                    break;
            }

            if (_preCheck && _showCostInfo) {
                string _costText = string.Format(Locale.Get("TOOL_CONSTRUCTION_COST"), _constructionCost / 100);

                base.ShowToolInfo(true, _costText, this.m_cachedPosition);
                return true;
            }

            return false;
        }

        public bool ShowHoverToolInfo() {
            bool _modeHasGameFlag = (this.m_toolController.m_mode & ItemClass.Availability.Game) != ItemClass.Availability.None;
            bool _preCheck = !this.m_toolController.IsInsideUI && Cursor.visible && _modeHasGameFlag;
            bool _showHoverInfo = false;

            string _hoverText = "";

            Vector3 _position = this.m_cachedPosition;

            switch (activeState) {
                case ActiveState.CreatePointFirst:
                case ActiveState.CreatePointSecond:
                case ActiveState.CreatePointThird: {
                    _showHoverInfo = false;
                    break;
                }
                case ActiveState.LockIdle: {
                    if (hoverState != HoverState.Unbound) {
                        _showHoverInfo = true;

                        switch (hoverState) {
                            case HoverState.SpacingLocus: {
                                _hoverText = "Change Spacing";
                                break;
                            }
                            case HoverState.AngleLocus: {
                                _hoverText = "Change Angle";
                                break;
                            }
                            case HoverState.ControlPointFirst: {
                                _hoverText = "Move First Point";
                                break;
                            }
                            case HoverState.ControlPointSecond: {
                                _hoverText = "Move Second Point";
                                break;
                            }
                            case HoverState.ControlPointThird: {
                                _hoverText = "Move Third Point";
                                break;
                            }
                            case HoverState.Curve: {
                                _hoverText = "Translate Curve";
                                break;
                            }
                        }
                    }
                    break;
                }
                case ActiveState.MovePointFirst: {
                    //_showHoverInfo = true;
                    break;
                }
                case ActiveState.MovePointSecond: {
                    //_showHoverInfo = true;
                    break;
                }
                case ActiveState.MovePointThird: {
                    //_showHoverInfo = true;
                    break;
                }
                case ActiveState.MoveSegment: {
                    //_showHoverInfo = true;
                    //_hoverText = "Click to lock curve position";
                    break;
                }
                case ActiveState.ChangeAngle: {
                    //_showHoverInfo = true;
                    break;
                }
                case ActiveState.ChangeSpacing: {
                    //_showHoverInfo = true;
                    break;
                }

                default: {
                    _showHoverInfo = false;
                    break;
                }
            }

            if (_preCheck && _showHoverInfo) {
                base.ShowToolInfo(true, _hoverText, _position);

                return true;
            } else {
                return false;
            }
        }

        public void ShowToolInfo() {
            Vector3 _position = this.m_cachedPosition;

            if (ShowHoverToolInfo()) {
                return;
            } else if (ShowConstructionCostToolInfo()) {
                return;
            } else {
                base.ShowToolInfo(false, null, _position);
            }

        }

        //from PropTool
        public static void DispatchPlacementEffect(Vector3 position, bool isBulldozeEffect) {
            EffectInfo _effectInfo;
            if (isBulldozeEffect) {
                _effectInfo = Singleton<PropManager>.instance.m_properties.m_bulldozeEffect;
            } else {
                _effectInfo = Singleton<PropManager>.instance.m_properties.m_placementEffect;
            }
            if (_effectInfo != null) {
                InstanceID instance = default(InstanceID);
                EffectInfo.SpawnArea spawnArea = new EffectInfo.SpawnArea(position, Vector3.up, 1f);
                Singleton<EffectManager>.instance.DispatchEffect(_effectInfo, instance, spawnArea, Vector3.zero, 0f, 1f, Singleton<AudioManager>.instance.DefaultGroup);
            }
        }

        public bool IsAtLeastOneItemValidPlacement() {
            bool _atLeastOneValid = false;
            bool _itemValid = false;
            for (int i = 0; i < placementCalculator.GetItemCountActual(); i++) {
                if (!PlacementCalculator.GetItemValidPlacement(i, out _itemValid)) {
                    _atLeastOneValid = false;
                    break;
                }
                if (_itemValid) {
                    _atLeastOneValid = true;
                    break;
                }
            }
            if (!_atLeastOneValid) {
                return false;
            } else {
                return true;
            }
        }

        public bool FinalizePlacement(bool continueDrawing, bool isCopyPlacing) {

            //Vector3 _position = Vector3.zero;
            Vector3 _meshPosition = Vector3.zero;
            float _angle = 0f;

            int _itemCount = placementCalculator.GetItemCountActual();

            if (_itemCount < 1) {
                return false;
            }

            //verify at least one item is valid
            //so we don't waste spots in the undo 'stack'
            if (!IsAtLeastOneItemValidPlacement()) {
                return false;
            }

            Randomizer _randomizer = PlacementCalculator.randomizerFresh;

            //new as of 170623
            placementCalculator.UpdateItemPlacementInfo();

            switch (objectMode) {
                case ObjectMode.Props: {
                    PropInfo _propInfo = propPrefab;
                    if (_propInfo == null) {
                        return false;
                    }
                    ushort _propID;

                    for (int i = 0; i < _itemCount; i++) {
                        //if (!PlacementCalculator.GetItemPosition(i, out _position) || !placementCalculator.GetAngle(i, out _angle))
                        if (!PlacementCalculator.GetItemMeshPosition(i, out _meshPosition) || !placementCalculator.GetAngle(i, out _angle)) {
                            //return;
                            break;
                        }
                        //new as of 161102 for Prop Precision
                        if (userSettingsControlPanel.renderAndPlacePosResVanilla) {
                            _meshPosition = _meshPosition.QuantizeToGameShortGridXYZ();
                        }
                        //for correct variation of itemwise placement
                        if (controlMode == ControlMode.Itemwise && i == PlacementCalculator.ITEMWISE_INDEX) {
                            //_propInfo = m_placementInfo[PlacementCalculator.ITEMWISE_INDEX].propInfo;
                            //_propInfo.GetVariation(ref _randomizer);

                            placementCalculator.propInfo.GetVariation(ref _randomizer);
                        }
                        if (m_placementInfo[i].isValidPlacement) {
                            _propInfo = m_placementInfo[i].propInfo;
                            if (Singleton<PropManager>.instance.CreateProp(out _propID, ref _randomizer, _propInfo, _meshPosition, _angle, true)) {
                                PlacementCalculator.SetPropID(i, _propID);

                                DispatchPlacementEffect(_meshPosition, false);
                            }
                        }
                    }

                    //undoManager.AddEntry(_itemCount, m_placementInfo, ObjectMode.Props);
                    undoManager.AddEntry(_itemCount, m_placementInfo, ObjectMode.Props, fenceMode, placementCalculator.segmentState);

                    break;
                }
                case ObjectMode.Trees: {
                    TreeInfo _treeInfo = treePrefab;
                    if (_treeInfo == null) {
                        return false;
                    }
                    uint _treeID;

                    for (int i = 0; i < _itemCount; i++) {
                        //if (!PlacementCalculator.GetItemPosition(i, out _position))
                        if (!PlacementCalculator.GetItemMeshPosition(i, out _meshPosition)) {
                            //return;
                            break;
                        }
                        //for correct variation of itemwise placement
                        if (controlMode == ControlMode.Itemwise && i == PlacementCalculator.ITEMWISE_INDEX) {
                            //_treeInfo = m_placementInfo[PlacementCalculator.ITEMWISE_INDEX].treeInfo;
                            //_treeInfo.GetVariation(ref _randomizer);

                            placementCalculator.treeInfo.GetVariation(ref _randomizer);
                        }
                        if (m_placementInfo[i].isValidPlacement) {
                            _treeInfo = m_placementInfo[i].treeInfo;
                            if (Singleton<TreeManager>.instance.CreateTree(out _treeID, ref _randomizer, _treeInfo, _meshPosition, true)) {
                                PlacementCalculator.SetTreeID(i, _treeID);

                                DispatchPlacementEffect(_meshPosition, false);
                            }
                        }
                    }

                    //undoManager.AddEntry(_itemCount, m_placementInfo, ObjectMode.Trees);
                    undoManager.AddEntry(_itemCount, m_placementInfo, ObjectMode.Trees, fenceMode, placementCalculator.segmentState);

                    break;
                }
                default: {

                    break;
                }
            }

            if (isCopyPlacing == false) {
                placementCalculator.FinalizeForPlacement(continueDrawing);
            }

            return true;
        }

        public override void RenderGeometry(RenderManager.CameraInfo cameraInfo) {

            switch (activeState) {
                //state set to undefined
                case ActiveState.Undefined: {
                    //Debug.LogError("[PLT]: RenderGeometry(): Active state is set to undefined!");
                    break;
                }

                //creating first control point
                case ActiveState.CreatePointFirst: {

                    break;
                }

                //creating second control point
                case ActiveState.CreatePointSecond: {
                    if (drawMode == DrawMode.Straight || drawMode == DrawMode.Circle) {
                        RenderItems(cameraInfo);
                    }
                    break;
                }

                //creating third control point
                case ActiveState.CreatePointThird: {
                    RenderItems(cameraInfo);
                    break;
                }

                //in lock mode, awaiting user input
                case ActiveState.LockIdle: {
                    RenderItems(cameraInfo);
                    break;
                }

                //in lock mode, moving first control point
                case ActiveState.MovePointFirst: {

                    RenderItems(cameraInfo);
                    break;
                }

                //in lock mode, moving second control point
                case ActiveState.MovePointSecond: {

                    RenderItems(cameraInfo);
                    break;
                }

                //in lock mode, moving third control point
                case ActiveState.MovePointThird: {

                    RenderItems(cameraInfo);
                    break;
                }

                //in lock mode, moving full line or curve
                case ActiveState.MoveSegment: {

                    RenderItems(cameraInfo);
                    break;
                }

                //in lock mode, changing item-to-item spacing along the line or curve
                case ActiveState.ChangeSpacing: {

                    RenderItems(cameraInfo);
                    break;
                }

                //in lock mode, changing initial item (first item's) angle
                case ActiveState.ChangeAngle: {

                    RenderItems(cameraInfo);
                    break;
                }
                case ActiveState.ItemwiseLock:
                case ActiveState.MoveItemwiseItem: {
                    RenderItems(cameraInfo);
                    break;
                }
                //out of bounds
                case ActiveState.MaxFillContinue: {
                    RenderItems(cameraInfo);
                    break;
                }
                default: {
                    //Debug.LogError("[PLT]: RenderGeometry(): Active state does not match a case!");
                    break;
                }
            }




            //at the end of PropTool.RenderGeometry();
            base.RenderGeometry(cameraInfo);
        }

        private void RenderItems(RenderManager.CameraInfo cameraInfo) {
            switch (objectMode) {
                case ObjectMode.Props: {
                    RenderProps(cameraInfo);
                    break;
                }
                case ObjectMode.Trees: {
                    RenderTrees(cameraInfo);
                    break;
                }
            }
        }

        private void RenderProps(RenderManager.CameraInfo cameraInfo) {
            PropInfo _propInfo = m_propPrefab;
            //Vector3 _position = Vector3.zero;
            Vector3 _meshPosition = Vector3.zero;
            float _angle = 0f;
            float _scale = 1f;
            Color32 _color = _propInfo.m_color0;

            bool _requireHeightMap = _propInfo.m_requireHeightMap;
            Texture _heightMap;
            Vector4 _heightMapping;
            Vector4 _surfaceMapping;

            bool _itemValid = false;

            for (int i = 0; i < placementCalculator.GetItemCountActual(); i++) {
                //if (!PlacementCalculator.GetItemPosition(i, out _position) || !placementCalculator.GetAngle(i, out _angle))
                if (!PlacementCalculator.GetItemMeshPosition(i, out _meshPosition) || !placementCalculator.GetAngle(i, out _angle)) {
                    //Debug.Log("Break triggered on prop render i = " + i.ToString());

                    //return;
                    break;
                } else if (PlacementCalculator.GetItemValidPlacement(i, out _itemValid)) {
                    if (_itemValid == true) {
                        _propInfo = m_placementInfo[i].propInfo;
                        _scale = m_placementInfo[i].scale;
                        _color = m_placementInfo[i].color;

                        if (userSettingsControlPanel.renderAndPlacePosResVanilla == true) {
                            _meshPosition = _meshPosition.QuantizeToGameShortGridXYZ();
                        }

                        InstanceID _id = default(InstanceID);
                        if (_requireHeightMap) {
                            Singleton<TerrainManager>.instance.GetHeightMapping(_meshPosition, out _heightMap, out _heightMapping, out _surfaceMapping);
                            PropInstance.RenderInstance(cameraInfo, _propInfo, _id, _meshPosition, _scale, _angle, _color, RenderManager.DefaultColorLocation, true, _heightMap, _heightMapping, _surfaceMapping);
                        } else {
                            PropInstance.RenderInstance(cameraInfo, _propInfo, _id, _meshPosition, _scale, _angle, _color, RenderManager.DefaultColorLocation, true);
                        }
                    }
                }
            }

            //end of RenderProps
        }

        private void RenderTrees(RenderManager.CameraInfo cameraInfo) {
            TreeInfo _treeInfo = m_treePrefab;
            //Vector3 _position = Vector3.zero;
            Vector3 _meshPosition = Vector3.zero;
            float _scale = 1f;
            float _brightness = 1f;

            bool _itemValid = false;

            for (int i = 0; i < placementCalculator.GetItemCountActual(); i++) {
                if (!PlacementCalculator.GetItemMeshPosition(i, out _meshPosition))
                //if (!PlacementCalculator.GetItemPosition(i, out _position))
                {
                    //Debug.Log("Break triggered on tree render i = " + i.ToString());

                    //return;
                    break;
                } else if (PlacementCalculator.GetItemValidPlacement(i, out _itemValid)) {
                    if (_itemValid == true) {
                        if (userSettingsControlPanel.renderAndPlacePosResVanilla == true) {
                            //_position.QuantizeToGameShortGridXYZ();
                            _meshPosition = _meshPosition.QuantizeToGameShortGridXYZ();
                        }

                        _treeInfo = m_placementInfo[i].treeInfo;
                        _scale = m_placementInfo[i].scale;
                        _brightness = m_placementInfo[i].brightness;

                        global::TreeInstance.RenderInstance(null, _treeInfo, _meshPosition, _scale, _brightness, RenderManager.DefaultColorLocation);
                    }
                }
            }

            //end of RenderTrees
        }

        public void DiscoverHoverState(Vector3 position) {
            bool _curvedOrFreeform = drawMode == DrawMode.Curved || drawMode == DrawMode.Freeform;
            bool _straight = drawMode == DrawMode.Straight;
            bool _circle = drawMode == DrawMode.Circle;

            bool _itemwiseState = activeState == ActiveState.ItemwiseLock || activeState == ActiveState.MoveItemwiseItem;

            //check for itemwise first before classic lock mode
            if (controlMode == ControlMode.Itemwise && _itemwiseState) {
                float _hoverItemT = 1f;

                //update only hoverItemwiseT
                if (_curvedOrFreeform && MathPLT.IsCloseToCurveXZ(m_mainBezier, hoverItemwiseCurveDistanceThreshold, position, out _hoverItemT)) {
                    hoverItemwiseT = _hoverItemT;
                    hoverState = HoverState.ItemwiseItem;
                } else if (_straight && MathPLT.IsCloseToSegmentXZ(m_mainSegment, hoverItemwiseCurveDistanceThreshold, position, out _hoverItemT)) {
                    hoverItemwiseT = _hoverItemT;
                    hoverState = HoverState.ItemwiseItem;
                } else if (_circle && MathPLT.IsCloseToCircle3XZ(m_mainCircle, hoverItemwiseCurveDistanceThreshold, position, out _hoverItemT)) {
                    hoverItemwiseT = _hoverItemT;
                    hoverState = HoverState.ItemwiseItem;
                } else {
                    hoverState = HoverState.Unbound;
                }
                //return;
            }

            //check for classic lock mode
            if (activeState != ActiveState.LockIdle) {
                return;
            }

            int _offset = fenceMode == true ? 0 : 1;
            if (placementCalculator.GetItemCountActual() < (1 + _offset)) {
                if (controlMode != ControlMode.Itemwise) {
                    hoverState = HoverState.Unbound;
                    return;
                }
            }

            float _pointRadius = hoverPointDistanceThreshold;
            float _anglePointRadius = _pointRadius;
            float _angleLocusRadius = hoverAngleLocusDiameter;

            float _angleLocusDistanceThreshold = 0.40f;

            //moved to top
            //bool _curvedOrFreeform = drawMode == DrawMode.Curved || drawMode == DrawMode.Freeform;
            //bool _straight = drawMode == DrawMode.Straight;

            bool _angleObjectMode = objectMode == ObjectMode.Props;

            Vector3 _angleCenter = m_placementInfo[hoverItemAngleCenterIndex].position;
            float _angle = hoverAngle;
            Vector3 _anglePos = Circle2.Position3FromAngleXZ(_angleCenter, _angleLocusRadius, _angle);

            Vector3 _spacingPos = fenceMode == true ? m_fenceEndPoints[hoverItemPositionIndex] : m_placementInfo[hoverItemPositionIndex].position;

            if (MathPLT.IsInsideCircleXZ(_spacingPos, _pointRadius, position)) {
                switch (controlMode) {
                    case ControlMode.Itemwise: {
                        hoverState = HoverState.ItemwiseItem;
                        break;
                    }
                    default: {
                        hoverState = HoverState.SpacingLocus;
                        break;
                    }
                }
            } else if (_angleObjectMode && MathPLT.IsInsideCircleXZ(_anglePos, _anglePointRadius, position)) {
                hoverState = HoverState.AngleLocus;
            } else if (_angleObjectMode && MathPLT.IsNearCircleOutlineXZ(_angleCenter, hoverAngleLocusDiameter, position, _angleLocusDistanceThreshold)) {
                hoverState = HoverState.AngleLocus;
            } else if (MathPLT.IsInsideCircleXZ(m_cachedControlPoints[0].m_position, _pointRadius, position)) {
                hoverState = HoverState.ControlPointFirst;
            } else if (MathPLT.IsInsideCircleXZ(m_cachedControlPoints[1].m_position, _pointRadius, position)) {
                hoverState = HoverState.ControlPointSecond;
            } else if (_curvedOrFreeform && MathPLT.IsInsideCircleXZ(m_cachedControlPoints[2].m_position, _pointRadius, position)) {
                hoverState = HoverState.ControlPointThird;
            } else if (_curvedOrFreeform && MathPLT.IsCloseToCurveXZ(m_mainBezier, hoverCurveDistanceThreshold, position, out hoverCurveT)) {
                hoverState = HoverState.Curve;
            } else if (_straight && MathPLT.IsCloseToSegmentXZ(m_mainSegment, hoverCurveDistanceThreshold, position, out hoverCurveT)) {
                hoverState = HoverState.Curve;
            } else if (_circle && MathPLT.IsCloseToCircle3XZ(m_mainCircle, hoverCurveDistanceThreshold, position, out hoverCurveT)) {
                hoverState = HoverState.Curve;
            } else {
                hoverState = HoverState.Unbound;
            }
        }


        public void RenderPlacementErrorOverlays(RenderManager.CameraInfo cameraInfo) {
            bool _override = userSettingsControlPanel.anarchyPLT || (!userSettingsControlPanel.anarchyPLT && userSettingsControlPanel.placeBlockedItems);

            if (placementCalculator.segmentState.allItemsValid == true && !_override) {
                return;
            }

            if (placementCalculator.GetItemCountActual() <= 0) {
                return;
            }

            if (IsActiveStateAnItemRenderState() == false) {
                return;
            }


            Color32 _blockedColor = _override == true ? new Color32(219, 192, 82, 80) : new Color32(219, 192, 82, 200);
            Color32 _invalidPlacementColor = userSettingsControlPanel.anarchyPLT == true ? new Color32(193, 78, 72, 50) : new Color32(193, 78, 72, 200);

            Color32 _blockedColorTransparent = new Color32(_blockedColor.r, _blockedColor.g, _blockedColor.b, 0);
            Color32 _invalidPlacementColorTransparent = new Color32(_invalidPlacementColor.r, _invalidPlacementColor.g, _invalidPlacementColor.b, 0);

            float _radius = 8f;
            float _scale = 1f;

            switch (objectMode) {
                case ObjectMode.Props: {
                    _scale = Mathf.Max(propPrefab.m_maxScale, propPrefab.m_minScale);
                    _radius = Mathf.Max(propPrefab.m_generatedInfo.m_size.x, propPrefab.m_generatedInfo.m_size.z) * _scale;
                    break;
                }
                case ObjectMode.Trees: {
                    _scale = Mathf.Max(treePrefab.m_maxScale, treePrefab.m_minScale);
                    _radius = Mathf.Max(treePrefab.m_generatedInfo.m_size.x, treePrefab.m_generatedInfo.m_size.z) * _scale;
                    break;
                }
                default: {
                    return;
                }
            }

            bool _itemValid = true;
            ItemCollisionType _itemCollisionFlags = ItemCollisionType.None;
            Vector3 _itemPosition = Vector3.zero;

            for (int i = 0; i < placementCalculator.GetItemCountActual(); i++) {
                if (PlacementCalculator.GetItemValidPlacement(i, out _itemValid) && PlacementCalculator.GetItemPosition(i, out _itemPosition)) {
                    if (_itemValid == false || _override) {
                        if (PlacementCalculator.GetItemCollisionFlags(i, out _itemCollisionFlags)) {
                            if (_itemCollisionFlags == ItemCollisionType.Blocked) {
                                RenderCircle(cameraInfo, _itemPosition, 0.10f, _blockedColor, false, false);
                                RenderCircle(cameraInfo, _itemPosition, 2f, _blockedColor, false, false);
                                RenderCircle(cameraInfo, _itemPosition, _radius, _blockedColor, false, true);
                            } else if (_itemCollisionFlags != ItemCollisionType.None) {
                                RenderCircle(cameraInfo, _itemPosition, 0.10f, _invalidPlacementColor, false, false);
                                RenderCircle(cameraInfo, _itemPosition, 2f, _invalidPlacementColor, false, false);
                                RenderCircle(cameraInfo, _itemPosition, _radius, _invalidPlacementColor, false, true);
                            }
                        }
                    }
                }

            }
        }


        //called in RenderOverlay
        public void RenderHoverObjectOverlays(RenderManager.CameraInfo cameraInfo) {
            int _offset = fenceMode == true ? 0 : 1;
            if (placementCalculator.GetItemCountActual() < (1 + _offset)) {
                if (controlMode != ControlMode.Itemwise) {
                    return;
                }
            }

            switch (activeState) {
                case ActiveState.Undefined:
                case ActiveState.CreatePointFirst:
                case ActiveState.CreatePointSecond:
                case ActiveState.CreatePointThird:
                case ActiveState.MaxFillContinue: {
                    return;
                }
                default: {
                    break;
                }
            }

            //setup highlight colors
            Color32 _baseColor = m_PLTColor_hoverBase;
            Color32 _lockIdleColor = m_PLTColor_locked;
            Color32 _highlightColor = m_PLTColor_lockedHighlight;
            if (m_keyboardAltDown == true) {
                _baseColor = m_PLTColor_hoverCopyPlace;
                _highlightColor = m_PLTColor_copyPlaceHighlight;
            }

            bool _angleObjectMode = objectMode == ObjectMode.Props;

            switch (activeState) {
                case ActiveState.LockIdle: {
                    //cp0
                    Color32 _cp0Color = hoverState == HoverState.ControlPointFirst ? _highlightColor : _baseColor;
                    RenderCircle(cameraInfo, m_cachedControlPoints[0].m_position, hoverPointDiameter, _cp0Color, false, false);
                    //cp1
                    Color32 _cp1Color = hoverState == HoverState.ControlPointSecond ? _highlightColor : _baseColor;
                    RenderCircle(cameraInfo, m_cachedControlPoints[1].m_position, hoverPointDiameter, _cp1Color, false, false);
                    //cp2
                    if (drawMode == DrawMode.Curved || drawMode == DrawMode.Freeform) {
                        Color32 _cp2Color = hoverState == HoverState.ControlPointThird ? _highlightColor : _baseColor;
                        RenderCircle(cameraInfo, m_cachedControlPoints[2].m_position, hoverPointDiameter, _cp2Color, false, false);
                    }
                    //curve
                    //this is done in RenderOverlay()

                    //spacing control point
                    Color32 _item1Color = hoverState == HoverState.SpacingLocus || hoverState == HoverState.ItemwiseItem ? _highlightColor : _baseColor;
                    Vector3 _spacingPos = fenceMode == true ? m_fenceEndPoints[hoverItemPositionIndex] : m_placementInfo[hoverItemPositionIndex].position;
                    RenderCircle(cameraInfo, _spacingPos, hoverPointDiameter, _item1Color, false, false);

                    //spacing fill indicator
                    if (hoverState == HoverState.SpacingLocus) {
                        Color32 _fillColor = Color.Lerp(_highlightColor, _lockIdleColor, 0.50f);

                        RenderProgressiveSpacingFill(cameraInfo, placementCalculator.spacingSingle, 1.00f, 0.20f, _fillColor, false, true);
                    }

                    //ANGLE
                    if (_angleObjectMode) {
                        //angle indicator
                        Vector3 _angleCenter = m_placementInfo[hoverItemAngleCenterIndex].position;
                        float _angle = hoverAngle;
                        Vector3 _anglePos = Circle2.Position3FromAngleXZ(_angleCenter, hoverAngleLocusDiameter, _angle);
                        Color32 _angleColor = hoverState == HoverState.AngleLocus ? _highlightColor : _baseColor;
                        RenderCircle(cameraInfo, _anglePos, hoverPointDiameter, _angleColor, false, false);
                        //angle locus
                        Color32 _blendColor = Color.Lerp(_baseColor, _angleColor, 0.50f);
                        _blendColor.a = 88;
                        RenderCircle(cameraInfo, _angleCenter, hoverAngleLocusDiameter * 2f, _blendColor, false, true);
                        //angle indicator line
                        Segment3 _angleLine = new Segment3(_angleCenter, _anglePos);
                        RenderLine(cameraInfo, _angleLine, 0.05f, 0.50f, _blendColor, false, true);
                    }


                    break;
                }
                case ActiveState.MovePointFirst: {
                    RenderCircle(cameraInfo, m_cachedControlPoints[0].m_position, hoverPointDiameter, _highlightColor, false, false);
                    break;
                }
                case ActiveState.MovePointSecond: {
                    RenderCircle(cameraInfo, m_cachedControlPoints[1].m_position, hoverPointDiameter, _highlightColor, false, false);
                    break;
                }
                case ActiveState.MovePointThird: {
                    if (drawMode == DrawMode.Curved || drawMode == DrawMode.Freeform) {
                        RenderCircle(cameraInfo, m_cachedControlPoints[2].m_position, hoverPointDiameter, _highlightColor, false, false);
                    }
                    break;
                }
                case ActiveState.MoveSegment: {
                    //this is done in RenderOverlay()
                    break;
                }
                case ActiveState.ChangeSpacing: {
                    //item second
                    Vector3 _spacingPos = fenceMode == true ? m_fenceEndPoints[hoverItemPositionIndex] : m_placementInfo[hoverItemPositionIndex].position;
                    RenderCircle(cameraInfo, _spacingPos, hoverPointDiameter, _highlightColor, false, false);
                    if (fenceMode == true) {
                        Color32 _blendColor = Color.Lerp(_baseColor, _highlightColor, 0.50f);
                        RenderLine(cameraInfo, new Segment3(m_fenceEndPoints[0], m_fenceEndPoints[1]), 0.05f, 0.50f, _blendColor, false, true);
                    } else {
                        RenderProgressiveSpacingFill(cameraInfo, placementCalculator.spacingSingle, 1.00f, 0.20f, _highlightColor, false, true);
                    }
                    break;
                }
                case ActiveState.ChangeAngle: {
                    //ANGLE
                    Vector3 _angleCenter = m_placementInfo[hoverItemAngleCenterIndex].position;
                    float _angle = hoverAngle;
                    Vector3 _anglePos = Circle2.Position3FromAngleXZ(_angleCenter, hoverAngleLocusDiameter, _angle);
                    Color32 _angleColor = _highlightColor;
                    RenderCircle(cameraInfo, _anglePos, hoverPointDiameter, _angleColor, false, false);
                    //angle locus
                    Color32 _blendColor = Color.Lerp(_baseColor, _angleColor, 0.50f);
                    _blendColor.a = 88;
                    RenderCircle(cameraInfo, _angleCenter, hoverAngleLocusDiameter * 2f, _blendColor, false, true);
                    //angle indicator line
                    Segment3 _angleLine = new Segment3(_angleCenter, _anglePos);
                    RenderLine(cameraInfo, _angleLine, 0.05f, 0.50f, _blendColor, false, true);

                    break;
                }
                case ActiveState.MoveItemwiseItem: {
                    Vector3 _spacingPos = fenceMode == true ? m_fenceEndPoints[hoverItemPositionIndex] : m_placementInfo[hoverItemPositionIndex].position;
                    RenderCircle(cameraInfo, _spacingPos, hoverPointDiameter, _highlightColor, false, false);
                    break;
                }
                default:
                    break;
            }
        }

        public void RenderMaxFillContinueMarkers(RenderManager.CameraInfo cameraInfo) {
            if (controlMode == ControlMode.Itemwise) {
                return;
            }

            Color _maxFillContinueColor = m_PLTColor_MaxFillContinue;
            float _radius = 6f;

            //initial item
            Vector3 _initialItemPosition = placementCalculator.initialItemPosition;
            ItemPlacementInfo _initialItem = placementCalculator.initialItem;

            Segment3 _thresholdMarkerInitial = new Segment3(_initialItemPosition - (_initialItem.offsetDirection * _radius), _initialItemPosition + (_initialItem.offsetDirection * _radius));

            //RenderCircle(cameraInfo, _initialItemPosition, 0.5f, _maxFillContinueColor, false, true);
            //RenderCircle(cameraInfo, _initialItemPosition, _radius, _maxFillContinueColor, false, true);
            RenderSegment(cameraInfo, _thresholdMarkerInitial, 0.25f, 0f, _maxFillContinueColor, false, true);

            //================

            //final item
            Vector3 _finalItemPosition = placementCalculator.finalItemPosition;
            ItemPlacementInfo _finalItem = placementCalculator.finalItem;

            Segment3 _thresholdMarkerFinal = new Segment3(_finalItemPosition - (_finalItem.offsetDirection * _radius), _finalItemPosition + (_finalItem.offsetDirection * _radius));

            RenderCircle(cameraInfo, _finalItemPosition, 0.5f, _maxFillContinueColor, false, true);
            RenderCircle(cameraInfo, _finalItemPosition, _radius, _maxFillContinueColor, false, true);
            RenderSegment(cameraInfo, _thresholdMarkerFinal, 0.25f, 0f, _maxFillContinueColor, false, true);

            //================

            //mouse indicators
            Color _maxFillContinueColorLight = new Color(_maxFillContinueColor.r, _maxFillContinueColor.g, _maxFillContinueColor.b, 0.40f * _maxFillContinueColor.a);
            Segment3 _mouseToInitial = new Segment3(m_mousePosition, _initialItemPosition);
            Segment3 _mouseToFinal = new Segment3(m_mousePosition, _finalItemPosition);

            RenderSegment(cameraInfo, _mouseToInitial, 0.05f, 3.00f, _maxFillContinueColorLight, false, true);
            RenderSegment(cameraInfo, _mouseToFinal, 0.05f, 3.00f, _maxFillContinueColorLight, false, true);
        }

        //RenderOverlay (all-encompassing)
        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo) {
            base.RenderOverlay(cameraInfo);

            bool _twoPointedDrawMode = drawMode == DrawMode.Straight || drawMode == DrawMode.Circle;

            bool _renderMFC = placementCalculator.segmentState.isReadyForMaxContinue || placementCalculator.segmentState.isMaxFillContinue;

            Color _createPointColor = m_PLTColor_default;
            Color _mainCurveColor = m_PLTColor_locked;
            Color _lockIdleColor = m_PLTColor_locked;
            Color _lockIdleColorStrong = m_PLTColor_lockedStrong;
            Color _highlightColor = m_PLTColor_lockedHighlight;
            Color _curveWarningColor = m_PLTColor_curveWarning;
            Color _copyPlaceColor = m_PLTColor_copyPlace;
            Color _itemwiseLockColor = m_PLTColor_ItemwiseLock;
            Color _maxFillContinueColor = m_PLTColor_MaxFillContinue;

            _createPointColor = controlMode == ControlMode.Itemwise ? _itemwiseLockColor : _createPointColor;

            if (m_keyboardCtrlDown && userSettingsControlPanel.showUndoPreviews == true) {
                undoManager.RenderLatestEntryCircles(cameraInfo, m_PLTColor_undoItemOverlay);
            }

            if (IsActiveStateAnItemRenderState() && placementCalculator.segmentState.isReadyForMaxContinue) {
                _copyPlaceColor = _maxFillContinueColor;
            }

            switch (activeState) {
                //state set to undefined
                case ActiveState.Undefined: {
                    Debug.LogError("[PLT]: RenderOverlay(): Active state is set to undefined!");
                    break;
                }

                //creating first control point
                case ActiveState.CreatePointFirst: {
                    if (!this.m_toolController.IsInsideUI && Cursor.visible) {
                        //medium circle
                        RenderCircle(cameraInfo, this.m_cachedPosition, 1.00f, _createPointColor, false, false);
                        //small pinpoint circle
                        RenderCircle(cameraInfo, this.m_cachedPosition, 0.10f, _createPointColor, false, true);
                    }


                    break;
                }

                //creating second control point
                case ActiveState.CreatePointSecond: {
                    if (true || this.m_cachedControlPoints[1].m_direction != Vector3.zero) {
                        bool _gotoCreatePointFirst = false;

                        switch (drawMode) {
                            case DrawMode.Straight:
                            case DrawMode.Circle: {
                                if (this.m_cachedControlPoints[1].m_direction != Vector3.zero) {
                                    if (this.m_keyboardAltDown) {
                                        _createPointColor = _copyPlaceColor; ;
                                    } else if (this.m_keyboardCtrlDown) {
                                        _createPointColor = m_PLTColor_locked;
                                        //_createPointColor = controlMode == ControlMode.Itemwise ? _itemwiseLockColor : _lockIdleColor;
                                    }

                                    switch (drawMode) {
                                        case DrawMode.Straight: {
                                            if (placementCalculator.segmentState.allItemsValid == false) {
                                                RenderSegment(cameraInfo, m_mainSegment, 1.50f, 0f, _curveWarningColor, false, true);
                                            }

                                            RenderSegment(cameraInfo, m_mainSegment, 1.00f, 0f, _createPointColor, false, true);
                                            break;
                                        }
                                        case DrawMode.Circle: {
                                            if (placementCalculator.segmentState.allItemsValid == false) {
                                                RenderMainCircle(cameraInfo, m_mainCircle, 1.50f, _curveWarningColor, false, true);
                                            }

                                            RenderMainCircle(cameraInfo, m_mainCircle, 1.00f, _createPointColor, false, true);
                                            break;
                                        }
                                        default: {
                                            //do nothing
                                            break;
                                        }
                                    }

                                    //MaxFillContinue
                                    if (_renderMFC) {
                                        RenderMaxFillContinueMarkers(cameraInfo);
                                    }

                                } else {
                                    _gotoCreatePointFirst = true;
                                }
                                break;
                            }
                            case DrawMode.Curved:
                            case DrawMode.Freeform: {
                                RenderLine(cameraInfo, m_mainArm1, 1.00f, 2f, _createPointColor, false, false);
                                break;
                            }
                            default: {
                                //do nothing
                                break;
                            }
                        }
                        if (_gotoCreatePointFirst) {
                            goto case ActiveState.CreatePointFirst;
                        }
                    }

                    //small pinpoint circles
                    RenderCircle(cameraInfo, this.m_cachedControlPoints[0].m_position, 0.10f, _createPointColor, false, true);
                    RenderCircle(cameraInfo, this.m_cachedControlPoints[1].m_position, 0.10f, _createPointColor, false, true);

                    break;
                }

                //creating third control point
                case ActiveState.CreatePointThird: {
                    if (true || this.m_cachedControlPoints[2].m_direction != Vector3.zero) {
                        if ((PropLineTool.drawMode == PropLineTool.DrawMode.Curved) || (PropLineTool.drawMode == PropLineTool.DrawMode.Freeform)) //not sure if this is necessary
                        {
                            if (this.m_keyboardAltDown) {
                                _createPointColor = _copyPlaceColor;
                            } else if (this.m_keyboardCtrlDown) {
                                _createPointColor = m_PLTColor_locked;
                                //_createPointColor = controlMode == ControlMode.Itemwise ? _itemwiseLockColor : _lockIdleColor;
                            }

                            if (placementCalculator.segmentState.allItemsValid == false) {
                                RenderBezier(cameraInfo, m_mainBezier, 1.50f, _curveWarningColor, false, true);
                            }

                            //for the size for these it should be 1/4 the size for renderline
                            RenderElbow(cameraInfo, m_mainArm1, m_mainArm2, 1.00f, 2f, _createPointColor, false, true);
                            RenderBezier(cameraInfo, m_mainBezier, 1.00f, _createPointColor, false, true);

                            //MaxFillContinue
                            if (_renderMFC) {
                                RenderMaxFillContinueMarkers(cameraInfo);
                            }
                        }
                    } else {
                        goto case ActiveState.CreatePointSecond;
                    }

                    //small pinpoint circles
                    RenderCircle(cameraInfo, this.m_cachedControlPoints[0].m_position, 0.10f, _createPointColor, false, true);
                    RenderCircle(cameraInfo, this.m_cachedControlPoints[1].m_position, 0.10f, _createPointColor, false, true);
                    RenderCircle(cameraInfo, this.m_cachedControlPoints[2].m_position, 0.10f, _createPointColor, false, true);

                    break;
                }

                //in lock mode, moving first control point
                case ActiveState.MovePointFirst:
                //in lock mode, moving second control point
                case ActiveState.MovePointSecond:
                //in lock mode, moving third control point
                case ActiveState.MovePointThird:
                //in lock mode, moving full line or curve
                case ActiveState.MoveSegment:
                //in lock mode, changing item-to-item spacing along the line or curve
                case ActiveState.ChangeSpacing:
                //in lock mode, changing initial item (first item's) angle
                case ActiveState.ChangeAngle:
                //in lock mode, awaiting user input
                case ActiveState.LockIdle:
                //in itemwise lock mode, awaiting user input
                case ActiveState.ItemwiseLock:
                //in lock mode, moving position of single item
                case ActiveState.MoveItemwiseItem: {
                    bool _hoverSpacing = hoverState == HoverState.SpacingLocus ? true : false;
                    _lockIdleColor = _hoverSpacing ? _lockIdleColorStrong : _lockIdleColor;

                    if (this.m_keyboardAltDown) {
                        _mainCurveColor = _copyPlaceColor;
                    } else {
                        //MaxFillContinue
                        if ((activeState == ActiveState.LockIdle || activeState == ActiveState.MaxFillContinue) && _renderMFC) {
                            RenderMaxFillContinueMarkers(cameraInfo);
                        }

                        if (this.m_keyboardCtrlDown) {
                            //_mainCurveColor = m_PLTColor_default;
                            if (controlMode == ControlMode.Itemwise) {
                                if (activeState == ActiveState.ItemwiseLock) {
                                    _mainCurveColor = _lockIdleColor;
                                } else if (activeState == ActiveState.LockIdle) {
                                    _mainCurveColor = _itemwiseLockColor;
                                }
                            } else //not in itemwise mode
                              {
                                _mainCurveColor = _createPointColor;
                            }
                        } else {
                            if (controlMode == ControlMode.Itemwise) {
                                if (activeState == ActiveState.ItemwiseLock) {
                                    _mainCurveColor = _itemwiseLockColor;
                                } else if (activeState == ActiveState.LockIdle) {
                                    _mainCurveColor = _lockIdleColor;
                                }
                            } else //not in itemwise mode
                              {
                                _mainCurveColor = _lockIdleColor;

                                ////MaxFillContinue
                                //if (activeState == ActiveState.LockIdle && _renderMFC)
                                //{
                                //    RenderMaxFillContinueMarkers(cameraInfo);
                                //}
                            }

                            //show adjustment circles
                            RenderHoverObjectOverlays(cameraInfo);
                            if (hoverState == HoverState.Curve && activeState == ActiveState.LockIdle) {
                                Color32 _curveColor = m_keyboardAltDown == true ? m_PLTColor_copyPlaceHighlight : m_PLTColor_lockedHighlight;
                                _mainCurveColor = _curveColor;
                            }
                        }
                    }

                    switch (drawMode) {
                        case DrawMode.Straight: {
                            RenderSegment(cameraInfo, m_mainSegment, 1.00f, 0f, _mainCurveColor, false, false);

                            if (placementCalculator.segmentState.allItemsValid == false) {
                                RenderSegment(cameraInfo, m_mainSegment, 1.50f, 0f, _curveWarningColor, false, true);
                            }
                            break;
                        }
                        case DrawMode.Curved:
                        case DrawMode.Freeform: {
                            RenderElbow(cameraInfo, m_mainArm1, m_mainArm2, 1.00f, 2f, _lockIdleColor, false, true);
                            //RenderBezier(cameraInfo, m_mainBezier, 1.00f, _mainCurveColor, false, false);
                            if (_hoverSpacing) {
                                RenderBezier(cameraInfo, m_mainBezier, 1.00f, _mainCurveColor, false, true);
                            } else {
                                RenderBezier(cameraInfo, m_mainBezier, 1.00f, _mainCurveColor, false, false);
                            }


                            if (placementCalculator.segmentState.allItemsValid == false) {
                                RenderBezier(cameraInfo, m_mainBezier, 1.50f, _curveWarningColor, false, true);
                            }
                            break;
                        }
                        case DrawMode.Circle: {
                            RenderMainCircle(cameraInfo, m_mainCircle, 1.00f, _mainCurveColor, false, true);

                            if (placementCalculator.segmentState.allItemsValid == false) {
                                RenderMainCircle(cameraInfo, m_mainCircle, 1.50f, _curveWarningColor, false, true);
                            }
                            break;
                        }
                        default: {
                            //do nothing
                            break;
                        }
                    }

                    //small pinpoint circles
                    RenderCircle(cameraInfo, this.m_cachedControlPoints[0].m_position, 0.10f, _lockIdleColor, false, true);
                    RenderCircle(cameraInfo, this.m_cachedControlPoints[1].m_position, 0.10f, _lockIdleColor, false, true);
                    RenderCircle(cameraInfo, this.m_cachedControlPoints[2].m_position, 0.10f, _lockIdleColor, false, true);

                    break;
                }
                case ActiveState.MaxFillContinue: {
                    switch (drawMode) {
                        case DrawMode.Straight:
                        case DrawMode.Circle: {
                            if (this.m_cachedControlPoints[1].m_direction != Vector3.zero) {
                                //MaxFillContinue
                                if (_renderMFC) {
                                    RenderMaxFillContinueMarkers(cameraInfo);
                                }

                                if (this.m_keyboardAltDown) {
                                    _createPointColor = _copyPlaceColor; ;
                                } else if (this.m_keyboardCtrlDown) {
                                    _createPointColor = _lockIdleColor;
                                    //_createPointColor = controlMode == ControlMode.Itemwise ? _itemwiseLockColor : _lockIdleColor;
                                } else {
                                    //nah
                                    //instead we will use play/pause markers...
                                    //_createPointColor = _maxFillContinueColor;

                                    ////MaxFillContinue
                                    //if (_renderMFC)
                                    //{
                                    //    RenderMaxFillContinueMarkers(cameraInfo);
                                    //}
                                }

                                switch (drawMode) {
                                    case DrawMode.Straight: {
                                        if (placementCalculator.segmentState.allItemsValid == false) {
                                            RenderSegment(cameraInfo, m_mainSegment, 1.50f, 0f, _curveWarningColor, false, true);
                                        }

                                        RenderSegment(cameraInfo, m_mainSegment, 1.00f, 0f, _createPointColor, false, true);
                                        break;
                                    }
                                    case DrawMode.Circle: {
                                        if (placementCalculator.segmentState.allItemsValid == false) {
                                            RenderMainCircle(cameraInfo, m_mainCircle, 1.50f, _curveWarningColor, false, true);
                                        }

                                        RenderMainCircle(cameraInfo, m_mainCircle, 1.00f, _createPointColor, false, true);
                                        break;
                                    }
                                    default: {
                                        //do nothing
                                        break;
                                    }
                                }

                            }

                            //small pinpoint circles
                            RenderCircle(cameraInfo, this.m_cachedControlPoints[0].m_position, 0.10f, _createPointColor, false, true);
                            RenderCircle(cameraInfo, this.m_cachedControlPoints[1].m_position, 0.10f, _createPointColor, false, true);

                            break;
                        }
                        case DrawMode.Curved:
                        case DrawMode.Freeform: {
                            if (true || this.m_cachedControlPoints[2].m_direction != Vector3.zero) {
                                if (this.m_keyboardAltDown) {
                                    _createPointColor = _copyPlaceColor;
                                } else if (this.m_keyboardCtrlDown) {
                                    _createPointColor = _lockIdleColor;
                                    //_createPointColor = controlMode == ControlMode.Itemwise ? _itemwiseLockColor : _lockIdleColor;
                                } else {
                                    //use markers instead
                                    //_createPointColor = _maxFillContinueColor;

                                    //MaxFillContinue
                                    if (_renderMFC) {
                                        RenderMaxFillContinueMarkers(cameraInfo);
                                    }
                                }

                                if (placementCalculator.segmentState.allItemsValid == false) {
                                    RenderBezier(cameraInfo, m_mainBezier, 1.50f, _curveWarningColor, false, true);
                                }

                                //for the size for these it should be 1/4 the size for renderline
                                RenderElbow(cameraInfo, m_mainArm1, m_mainArm2, 1.00f, 2f, _createPointColor, false, true);
                                RenderBezier(cameraInfo, m_mainBezier, 1.00f, _createPointColor, false, true);
                            }

                            //small pinpoint circles
                            RenderCircle(cameraInfo, this.m_cachedControlPoints[0].m_position, 0.10f, _createPointColor, false, true);
                            RenderCircle(cameraInfo, this.m_cachedControlPoints[1].m_position, 0.10f, _createPointColor, false, true);
                            RenderCircle(cameraInfo, this.m_cachedControlPoints[2].m_position, 0.10f, _createPointColor, false, true);

                            break;
                        }
                        default: {
                            //do nothing
                            break;
                        }
                    }
                    break;
                }
                //out of bounds
                default: {
                    //Debug.LogError("[PLT]: RenderOverlay(): Active state does not match a case!");
                    break;
                }
            }

            if (userSettingsControlPanel.showErrorGuides == true) {
                RenderPlacementErrorOverlays(cameraInfo);
            }

            bool _debugRenderItemPositionPoints = false;
            if (_debugRenderItemPositionPoints || m_debugRenderOverlayItemPoints) {
                //DEBUG/TESTING ONLY
                switch (activeState) {
                    case ActiveState.Undefined:
                    case ActiveState.CreatePointFirst: {
                        break;
                    }
                    default: {
                        switch (drawMode) {
                            case DrawMode.Straight:
                            case DrawMode.Circle: {
                                if (activeState == ActiveState.CreatePointSecond || activeState == ActiveState.LockIdle) {
                                    for (int i = 0; i < placementCalculator.GetItemCountActual(); i++) {
                                        RenderCircle(cameraInfo, m_placementInfo[i].position, 1.5f, m_PLTColor_default, false, false);
                                        RenderCircle(cameraInfo, m_placementInfo[i].position, 0.1f, m_PLTColor_default, false, true);
                                    }
                                    if (fenceMode == true) {
                                        for (int i = 0; i < placementCalculator.GetItemCountActual() + 1; i++) {
                                            RenderCircle(cameraInfo, m_fenceEndPoints[i], 0.1f, m_PLTColor_copyPlaceHighlight, false, false);
                                        }
                                    }
                                }
                                break;
                            }
                            case DrawMode.Curved:
                            case DrawMode.Freeform: {
                                if (activeState == ActiveState.CreatePointThird || activeState == ActiveState.LockIdle) {
                                    for (int i = 0; i < placementCalculator.GetItemCountActual(); i++) {
                                        RenderCircle(cameraInfo, m_placementInfo[i].position, 1.5f, m_PLTColor_default, false, false);
                                        RenderCircle(cameraInfo, m_placementInfo[i].position, 0.1f, m_PLTColor_default, false, true);
                                    }
                                    if (fenceMode == true) {
                                        for (int i = 0; i < placementCalculator.GetItemCountActual() + 1; i++) {
                                            RenderCircle(cameraInfo, m_fenceEndPoints[i], 0.1f, m_PLTColor_copyPlaceHighlight, false, true);
                                        }
                                    }
                                }
                                break;
                            }
                            default: {
                                break;
                            }
                        }
                        break;
                    }
                }
            }



            //end RenderOverlay()
        }

        public static void RenderCircle(RenderManager.CameraInfo cameraInfo, Vector3 position, float size, Color color, bool renderLimits, bool alphaBlend) {
            ToolManager _instance = Singleton<ToolManager>.instance;
            _instance.m_drawCallData.m_overlayCalls += 1;
            Singleton<RenderManager>.instance.OverlayEffect.DrawCircle(cameraInfo, color, position, size, -1f, 1280f, renderLimits, alphaBlend);
        }

        public static void RenderLine(RenderManager.CameraInfo cameraInfo, Segment3 segment, float size, float dashLength, Color color, bool renderLimits, bool alphaBlend) {
            ToolManager _instance = Singleton<ToolManager>.instance;
            _instance.m_drawCallData.m_overlayCalls += 1;
            Singleton<RenderManager>.instance.OverlayEffect.DrawSegment(cameraInfo, color, segment, size, dashLength, -1f, 1280f, renderLimits, alphaBlend);
        }

        public static void RenderSegment(RenderManager.CameraInfo cameraInfo, Segment3 segment, float size, float dashLength, Color color, bool renderLimits, bool alphaBlend) {
            ToolManager _instance = Singleton<ToolManager>.instance;
            _instance.m_drawCallData.m_overlayCalls += 1;
            Singleton<RenderManager>.instance.OverlayEffect.DrawSegment(cameraInfo, color, segment, size, dashLength, -1f, 1280f, renderLimits, alphaBlend);
        }

        public static void RenderElbow(RenderManager.CameraInfo cameraInfo, Segment3 segment1, Segment3 segment2, float size, float dashLength, Color color, bool renderLimits, bool alphaBlend) {
            ToolManager _instance = Singleton<ToolManager>.instance;
            _instance.m_drawCallData.m_overlayCalls += 1;
            Singleton<RenderManager>.instance.OverlayEffect.DrawSegment(cameraInfo, color, segment1, segment2, size, dashLength, -1f, 1280f, renderLimits, alphaBlend);
        }

        public static void RenderBezier(RenderManager.CameraInfo cameraInfo, Bezier3 bezier, float size, Color color, bool renderLimits, bool alphaBlend) {
            ToolManager _instance = Singleton<ToolManager>.instance;
            _instance.m_drawCallData.m_overlayCalls += 1;
            Singleton<RenderManager>.instance.OverlayEffect.DrawBezier(cameraInfo, color, bezier, size, -100000f, 100000f, -1f, 1280f, renderLimits, alphaBlend);
        }

        public static void RenderMainCircle(RenderManager.CameraInfo cameraInfo, Circle3XZ circle, float size, Color color, bool renderLimits, bool alphaBlend) {
            ToolManager _instance = Singleton<ToolManager>.instance;
            _instance.m_drawCallData.m_overlayCalls += 2;

            //circle
            Singleton<RenderManager>.instance.OverlayEffect.DrawCircle(cameraInfo, color, circle.center, circle.diameter + size, -1f, 1280f, renderLimits, alphaBlend);
            Singleton<RenderManager>.instance.OverlayEffect.DrawCircle(cameraInfo, color, circle.center, circle.diameter - size, -1f, 1280f, renderLimits, alphaBlend);

            //orienting line
            if (circle.radius > 0f) {
                Color _lineColor = color;
                _lineColor.a *= 0.85f;
                RenderLine(cameraInfo, new Segment3(circle.center, circle.Position(0f)), 0.05f, 1.00f, color, false, true);
            }
        }

        /// <summary>
        /// Renders a fill-segment OR a series of small circles from the start of the curve up to the given fillLength.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="interval">How far apart to space the small circles.</param>
        public static void RenderProgressiveSpacingFill(RenderManager.CameraInfo cameraInfo, float fillLength, float interval, float size, Color color, bool renderLimits, bool alphaBlend) {
            if (fenceMode) {
                return;
            }

            interval = Mathf.Clamp(interval, UserParameters.SPACING_MIN, UserParameters.SPACING_MAX);

            float _deltaT = 0f;
            int _numItems = Mathf.Clamp(Mathf.CeilToInt(fillLength / interval), 0, 262144);

            switch (drawMode) {
                case DrawMode.Straight: {
                    float _speed = MathPLT.LinearSpeedXZ(m_mainSegment);
                    _deltaT = fillLength / _speed;

                    float _firstItemT = m_placementInfo[0].t;
                    float _tFill = _firstItemT + _deltaT;

                    Segment3 _fillSegment = m_mainSegment;

                    _fillSegment = _fillSegment.Cut(_firstItemT, _tFill);

                    Color _color = new Color(color.r, color.g, color.b, 0.75f * color.a);
                    RenderSegment(cameraInfo, _fillSegment, size, 0f, _color, renderLimits, alphaBlend);

                    break;
                }
                case DrawMode.Curved:
                case DrawMode.Freeform: {
                    Bezier3 _fillBezier = m_mainBezier;

                    //original bezier fill
                    float _firstItemT = m_placementInfo[0].t;
                    float _tFill = _firstItemT;
                    MathPLT.StepDistanceCurve(m_mainBezier, _firstItemT, fillLength, placementCalculator.tolerance, out _tFill);
                    _fillBezier = _fillBezier.Cut(_firstItemT, _tFill);
                    //RenderBezier(cameraInfo, _fillBezier, size, color, renderLimits, alphaBlend);
                    Color _color = new Color(color.r, color.g, color.b, 0.75f * color.a);
                    RenderBezier(cameraInfo, _fillBezier, size, _color, renderLimits, true);

                    //new circle fill
                    //float _firstItemT = m_placementInfo[0].t;
                    //Vector3 _position = m_mainBezier.a;
                    //float _t = _firstItemT;
                    //for (int i = 0; i < _numItems; i++)
                    //{
                    //    _position = m_mainBezier.Position(_t);

                    //    RenderCircle(cameraInfo, _position, size, color, renderLimits, alphaBlend);

                    //    MathPLT.StepDistanceCurve(m_mainBezier, _t, interval, placementCalculator.tolerance, out _t);
                    //}

                    break;
                }
                case DrawMode.Circle: {
                    if (m_mainCircle.radius <= 0f) {
                        return;
                    }

                    _deltaT = interval / m_mainCircle.circumference;

                    Quaternion _rotation = Quaternion.AngleAxis(_deltaT * -360f, Vector3.up);
                    Vector3 _position = m_mainCircle.Position(0f);
                    Vector3 _center = m_mainCircle.center;
                    Vector3 _radiusVector = _position - _center;

                    for (int i = 0; i < _numItems; i++) {
                        RenderCircle(cameraInfo, _position, size, color, renderLimits, alphaBlend);

                        _radiusVector = _rotation * _radiusVector;
                        _position = _center + _radiusVector;
                    }
                    break;
                }
                default: {
                    return;
                }
            }
        }

        public static void RenderFilledCircle(RenderManager.CameraInfo cameraInfo, Vector3 position, float size, Color color, float dashLength, bool renderLimits, bool alphaBlend) {
            Segment3 _segment = new Segment3 {
                a = position,
                b = position
            };

            ToolManager _instance = Singleton<ToolManager>.instance;
            _instance.m_drawCallData.m_overlayCalls += 1;
            Singleton<RenderManager>.instance.OverlayEffect.DrawSegment(cameraInfo, color, _segment, size, dashLength, -1f, 1280f, renderLimits, alphaBlend);
        }

        public override void SimulationStep() {
            this.m_treeInfo = this.treePrefab;
            this.m_propInfo = this.propPrefab;

            //Prop/Tree methods
            //test if prefabs are all null then return
            bool _flag_propPrefabNull = (this.propPrefab == null);
            bool _flag_treePrefabNull = (this.treePrefab == null);

            if (_flag_propPrefabNull) {
                this.m_wasPropPrefab = null;
                this.m_propInfo = null;
            }
            if (_flag_treePrefabNull) {
                this.m_wasTreePrefab = null;
                this.m_treeInfo = null;
            }

            if (_flag_propPrefabNull && _flag_treePrefabNull) {
                return;
            }

            if ((this.m_treeInfo == null) || (this.m_wasTreePrefab != this.treePrefab)) {
                this.m_wasTreePrefab = this.treePrefab;

                //custom
                this.m_treeInfo = this.treePrefab;

            }
            if ((this.m_propInfo == null) || (this.m_wasPropPrefab != this.propPrefab)) {
                this.m_wasPropPrefab = this.propPrefab;

                //custom
                this.m_propInfo = this.propPrefab;

            }


            ToolBase.RaycastInput input = new ToolBase.RaycastInput(this.m_mouseRay, this.m_mouseRayLength);
            try {
                ToolBase.RaycastOutput raycastOutput;
                if (this.m_mouseRayValid && ToolBase.RayCast(input, out raycastOutput)) {

                    //check here for tool mode
                    if (true) {
                        if (!raycastOutput.m_currentEditObject) {

                            this.m_mousePosition = raycastOutput.m_hitPos;
                        }
                    }

                }
            }
            finally {
                //there was an EndColliding() here, but it cause a "Space already occupied!" bug...
                //We're not gonna use it after all. It had no function in PLT.
            }

            while (!Monitor.TryEnter(this.m_cacheLock, SimulationManager.SYNCHRONIZE_TIMEOUT)) {
            }
            try {
                //these are from NetTool
                //this.m_buildErrors = toolErrors;
                //this.m_constructionCost = ((!flag6) ? 0 : num17);
                //this.m_productionRate = productionRate;
            }
            finally {
                Monitor.Exit(this.m_cacheLock);
            }

        }

        private bool TryRaycast(out Vector3 hitPosition) {
            bool result = false;

            hitPosition = Vector3.zero;

            ToolBase.RaycastInput input = new ToolBase.RaycastInput(this.m_mouseRay, this.m_mouseRayLength);
            try {
                ToolBase.RaycastOutput raycastOutput;
                if (this.m_mouseRayValid && ToolBase.RayCast(input, out raycastOutput)) {

                    if (!raycastOutput.m_currentEditObject) {
                        result = true;
                        hitPosition = raycastOutput.m_hitPos;

                    }

                }
            }
            finally {
                //there was an EndColliding() here, but it cause a "Space already occupied!" bug...
                //We're not gonna use it after all. It had no function in PLT.
            }
            return result;
        }

        private void UpdateMiscHoverParameters() {
            int _offset = fenceMode == true ? 0 : 1;
            if (placementCalculator.GetItemCountActual() < (1 + _offset)) {
                if (controlMode != ControlMode.Itemwise) {
                    return;
                }
            }

            switch (activeState) {
                case ActiveState.MoveSegment: {
                    Vector3 _translation = m_cachedPosition - lockBackupCachedPosition;

                    for (int i = 0; i < m_controlPoints.Length; i++) {
                        m_controlPoints[i].m_position = lockBackupControlPoints[i].m_position + _translation;
                    }

                    UpdateCachedControlPoints();
                    UpdateCurves();

                    placementCalculator.UpdateItemPlacementInfo();

                    break;
                }
                case ActiveState.ChangeSpacing: {
                    switch (drawMode) {
                        case DrawMode.Straight: {
                            if (MathPLT.IsCloseToSegmentXZ(m_mainSegment, hoverCurveDistanceThreshold * 8f, m_cachedPosition, out hoverCurveT)) {
                                float _curveT = Mathf.Clamp(hoverCurveT, m_placementInfo[0].t, 0.995f);
                                Vector3 _linePosition = MathPLT.LinePosition(m_mainSegment, _curveT);

                                if (fenceMode == true) {
                                    //since straight fence mode auto snaps to last fence endpoint
                                    Vector3 _lineZero = MathPLT.LinePosition(m_mainSegment, 0f);
                                    float _distance = (_linePosition - _lineZero).MagnitudeXZ();
                                    placementCalculator.spacingSingle = _distance;
                                } else //non-fence mode
                                  {
                                    Vector3 _lineZero = MathPLT.LinePosition(m_mainSegment, m_placementInfo[0].t);
                                    float _distance = (_linePosition - _lineZero).MagnitudeXZ();
                                    placementCalculator.spacingSingle = _distance;
                                }

                            }
                            break;
                        }
                        case DrawMode.Curved:
                        case DrawMode.Freeform: {
                            if (MathPLT.IsCloseToCurveXZ(m_mainBezier, hoverCurveDistanceThreshold * 8f, m_cachedPosition, out hoverCurveT)) {

                                float _curveT = Mathf.Clamp(hoverCurveT, m_placementInfo[0].t, 0.995f);

                                if (fenceMode == true) {
                                    Vector3 _curvePosition = m_mainBezier.Position(_curveT);
                                    Vector3 _fencePointZero = m_fenceEndPoints[0];
                                    float _distance = (_curvePosition - _fencePointZero).MagnitudeXZ();
                                    placementCalculator.spacingSingle = _distance;
                                } else //non-fence mode
                                  {
                                    float _firstItemT = m_placementInfo[0].t;
                                    placementCalculator.spacingSingle = MathPLT.CubicBezierArcLengthXZGauss12(m_mainBezier, _firstItemT, _curveT);
                                }

                            }
                            break;
                        }
                        case DrawMode.Circle: {
                            if (MathPLT.IsCloseToCircle3XZ(m_mainCircle, hoverCurveDistanceThreshold * 12f, m_cachedPosition, out hoverCurveT)) {

                                //Vector3 _radiusVectorHover = _circlePositionHover - m_mainCircle.center;
                                //_radiusVectorHover.y = 0f;
                                //Vector3 _radiusVectorZero = m_mainCircle.Position(0f) - m_mainCircle.center;
                                //_radiusVectorZero.y = 0f;

                                Circle3XZ _circle = (userSettingsControlPanel.perfectCircles) ? m_rawCircle : m_mainCircle;

                                if (fenceMode == true) {
                                    float _curveT = Mathf.Clamp(hoverCurveT, m_placementInfo[0].t, 0.500f);
                                    Vector3 _circlePositionHover = _circle.Position(_curveT);

                                    Vector3 _circleZero = _circle.Position(0f);
                                    float _distance = (_circlePositionHover - _circleZero).MagnitudeXZ();

                                    if (userSettingsControlPanel.perfectCircles) {
                                        _distance = Mathf.Clamp(_distance, UserParameters.SPACING_MIN, m_rawCircle.diameter);
                                    }

                                    placementCalculator.spacingSingle = _distance;
                                } else //non-fence mode
                                  {
                                    float _curveT = Mathf.Clamp(hoverCurveT, m_placementInfo[0].t, 0.995f);
                                    Vector3 _circlePositionHover = _circle.Position(_curveT);

                                    //float _deltaAlpha = MathPLT.AngleSigned(_radiusVectorHover, _radiusVectorZero, Vector3.up) + Mathf.PI;
                                    float _deltaAlpha = _circle.AngleBetween(0f, _curveT);
                                    float _distance = _circle.radius * _deltaAlpha;

                                    if (userSettingsControlPanel.perfectCircles) {
                                        _distance = Mathf.Clamp(_distance, UserParameters.SPACING_MIN, 0.50f * m_mainCircle.circumference);
                                    }

                                    placementCalculator.spacingSingle = _distance;
                                }

                            }
                            break;
                        }
                        default: {
                            break;
                        }
                    }

                    UpdateCachedControlPoints();
                    UpdateCurves();
                    placementCalculator.UpdateItemPlacementInfo(true, true);

                    break;
                }
                case ActiveState.ChangeAngle: {
                    if (placementCalculator.angleMode == PlacementCalculator.AngleMode.Dynamic) {
                        Vector3 _xAxis = Vector3.right;
                        Vector3 _yAxis = Vector3.up;

                        Vector3 _angleCenter = m_placementInfo[hoverItemAngleCenterIndex].position;
                        Vector3 _angleVector = m_cachedPosition - _angleCenter;
                        _angleVector.y = 0f;
                        _angleVector.Normalize();

                        float _angleHover = Math.MathPLT.AngleSigned(_angleVector, _xAxis, _yAxis);


                        float _itemAngle = PlacementCalculator.AngleDynamicXZ(lockBackupItemDirection);

                        float _angleOffset = _angleHover;

                        hoverAngle = _angleHover;
                        float _itemHoverAngleDifference = Math.MathPLT.AngleSigned(_angleVector, lockBackupItemDirection, _yAxis);
                        placementCalculator.angleOffset = _itemHoverAngleDifference;
                    } else if (placementCalculator.angleMode == PlacementCalculator.AngleMode.Single) {
                        Vector3 _xAxis = Vector3.right;
                        Vector3 _yAxis = Vector3.up;

                        Vector3 _angleCenter = m_placementInfo[hoverItemAngleCenterIndex].position;
                        Vector3 _angleVector = m_cachedPosition - _angleCenter;
                        _angleVector.y = 0f;
                        _angleVector.Normalize();

                        float _angle = Math.MathPLT.AngleSigned(_angleVector, _xAxis, _yAxis);

                        hoverAngle = _angle;
                        placementCalculator.angleSingle = _angle + Mathf.PI;
                    }

                    UpdateCachedControlPoints();
                    UpdateCurves();

                    placementCalculator.UpdateItemPlacementInfo();

                    break;
                }
                case ActiveState.ItemwiseLock:
                case ActiveState.MoveItemwiseItem: {
                    placementCalculator.UpdateItemPlacementInfo();
                    break;
                }
                default: {
                    return;
                }
            }
        }

        /// <summary>
        /// Reverses control points for 3-pointed curved and freeform curves.
        /// </summary>
        private void ReverseControlPoints3() {
            ControlPoint _buffer = m_controlPoints[0];
            m_controlPoints[0] = m_controlPoints[2];
            m_controlPoints[2] = _buffer;

            m_controlPoints[0].m_direction *= -1f;
            m_controlPoints[2].m_direction *= -1f;

            m_controlPoints[1].m_direction = m_controlPoints[0].m_direction;
        }

        private void UpdatePrefabs() {
            this.m_treeInfo = this.treePrefab;
            this.m_propInfo = this.propPrefab;
        }

        //continuously update control points to follow mouse
        private void UpdateControlPoints() {
            //continuously update control points to follow mouse
            switch (activeState) {
                case ActiveState.CreatePointFirst: {
                    placementCalculator.UpdateItemPlacementInfo(false, false);
                    break;
                }
                case ActiveState.CreatePointSecond: {
                    ModifyControlPoint(this.m_cachedPosition, 2);
                    //break;
                    if (drawMode == DrawMode.Straight || drawMode == DrawMode.Circle) {
                        placementCalculator.UpdateItemPlacementInfo();
                    }
                    goto Label_updateCurves;
                }
                case ActiveState.CreatePointThird: {
                    ModifyControlPoint(this.m_cachedPosition, 3);
                    //break;
                    if (drawMode == DrawMode.Curved || drawMode == DrawMode.Freeform) {
                        placementCalculator.UpdateItemPlacementInfo();
                    }
                    goto Label_updateCurves;
                }

                case ActiveState.MoveSegment:
                case ActiveState.ChangeSpacing:
                case ActiveState.ChangeAngle:
                case ActiveState.LockIdle:
                case ActiveState.MaxFillContinue: {
                    placementCalculator.UpdateItemPlacementInfo();
                    break;
                }
                case ActiveState.MovePointFirst: {
                    ModifyControlPoint(this.m_cachedPosition, 1);

                    placementCalculator.UpdateItemPlacementInfo();

                    goto Label_updateCurves;
                }
                case ActiveState.MovePointSecond: {
                    ModifyControlPoint(this.m_cachedPosition, 2);

                    placementCalculator.UpdateItemPlacementInfo();

                    goto Label_updateCurves;
                }
                case ActiveState.MovePointThird: {
                    ModifyControlPoint(this.m_cachedPosition, 3);

                    placementCalculator.UpdateItemPlacementInfo();

                    goto Label_updateCurves;
                }
            Label_updateCurves:
                {
                    UpdateCurves();

                    break;
                }
            }
        }

        private void UpdateCurves() {
            if (this.m_cachedControlPointCount == 0) {
                return;
            }
            //very bad things...
            //else if (placementCalculator.segmentState.isMaxFillContinue)
            //{
            //    return;
            //}
            else {
                m_pendingPlacementUpdate = true;

                switch (PropLineTool.drawMode) {
                    case PropLineTool.DrawMode.Straight: {
                        if (this.m_cachedControlPointCount >= 1) {
                            m_mainSegment.a = this.m_cachedControlPoints[0].m_position;
                            m_mainSegment.b = this.m_cachedControlPoints[1].m_position;
                        }
                        break;
                    }
                    case PropLineTool.DrawMode.Curved:
                    case PropLineTool.DrawMode.Freeform: {
                        if (this.m_cachedControlPointCount >= 1) {
                            m_mainArm1.a = this.m_cachedControlPoints[0].m_position;
                            m_mainArm1.b = this.m_cachedControlPoints[1].m_position;
                        }
                        if (this.m_cachedControlPointCount >= 2) {
                            if (PropLineTool.m_useCOBezierMethod == true) {
                                //uses negative of endDirection
                                m_mainBezier = Math.MathPLT.QuadraticToCubicBezierCOMethod(this.m_cachedControlPoints[0].m_position, this.m_cachedControlPoints[1].m_direction, this.m_cachedControlPoints[2].m_position, (-this.m_cachedControlPoints[2].m_direction));
                            } else {
                                m_mainBezier = Math.MathPLT.QuadraticToCubicBezier(this.m_cachedControlPoints[0].m_position, this.m_cachedControlPoints[1].m_position, this.m_cachedControlPoints[2].m_position);
                            }
                            m_mainArm2.a = this.m_cachedControlPoints[1].m_position;
                            m_mainArm2.b = this.m_cachedControlPoints[2].m_position;

                            //***SUPER-IMPORTANT (for convergence of fenceMode)***
                            Math.MathPLT.BezierXZ(ref m_mainBezier);

                            //calculate direction here in case controlPoint direction was not set correctly
                            Vector3 _dirArm1 = (m_mainArm1.b - m_mainArm1.a);
                            _dirArm1.y = 0f;
                            _dirArm1.Normalize();
                            Vector3 _dirArm2 = (m_mainArm2.b - m_mainArm2.a);
                            _dirArm2.y = 0f;
                            _dirArm2.Normalize();
                            m_mainElbowAngle = Mathf.Abs(Math.MathPLT.AngleSigned(-_dirArm1, _dirArm2, Vector3.up));
                        }
                        break;
                    }
                    case DrawMode.Circle: {
                        if (this.m_cachedControlPointCount >= 1) {
                            Vector3 _center = this.m_cachedControlPoints[0].m_position;
                            Vector3 _pointOnCircle = this.m_cachedControlPoints[1].m_position;

                            //constrain to XZ plane
                            _center.y = 0f;
                            _pointOnCircle.y = 0f;

                            Circle3XZ _mainCircle = new Circle3XZ(_center, _pointOnCircle);
                            m_rawCircle = _mainCircle;

                            //perfect circle radius-snapping
                            if (userSettingsControlPanel.perfectCircles) {
                                switch (controlMode) {
                                    case ControlMode.Itemwise:
                                    case ControlMode.Spacing: {
                                        //snap to perfect circle
                                        if (fenceMode) {
                                            _mainCircle.radius = _mainCircle.PerfectRadiusByChords(placementCalculator.spacingSingle);
                                        } else {
                                            _mainCircle.radius = _mainCircle.PerfectRadiusByArcs(placementCalculator.spacingSingle);
                                        }
                                        break;
                                    }
                                    default: {
                                        break;
                                    }
                                }
                            }

                            //finally
                            m_mainCircle = _mainCircle;

                        }
                        break;
                    }
                    default: {
                        break;
                    }
                }
            }
        }

        private void CheckPendingPlacement() {
            if (m_pendingPlacementUpdate) {
                placementCalculator.UpdateItemPlacementInfo();

                m_pendingPlacementUpdate = false;
            }
        }

        private void UpdateCachedControlPoints() {
            while (!Monitor.TryEnter(this.m_cacheLock, SimulationManager.SYNCHRONIZE_TIMEOUT)) {
            }
            try {
                for (int i = 0; i < this.m_controlPoints.Length; i++) {
                    this.m_cachedControlPoints[i] = this.m_controlPoints[i];
                }
                this.m_cachedControlPointCount = this.m_controlPointCount;
            }
            finally {
                Monitor.Exit(this.m_cacheLock);
            }
        }

        private void UpdateCachedPosition(bool ignorePosChangingCondition) {
            if (ignorePosChangingCondition || (IsVectorXZPositionChanging(this.m_cachedPosition, this.m_mousePosition, 0.001f))) {
                this.m_positionChanging = true;
            } else if (false) {

            } else {
                this.m_positionChanging = false;
            }
            this.m_cachedPosition = this.m_mousePosition; //from prop/tree tool *IMPORTANT*
        }

        private bool DoesPositionNeedUpdating(out Vector3 newPosition) {
            bool result = false;
            Vector3 _checkPoint = new Vector3();
            Vector3 _hitPos = new Vector3();

            newPosition = Vector3.zero;

            if (!TryRaycast(out _hitPos)) {
                return false;
            }

            bool _checkPosition = false;
            switch (activeState) {
                case ActiveState.CreatePointFirst: {
                    _checkPoint = m_cachedControlPoints[0].m_position;
                    _checkPosition = true;
                    break;
                }
                case ActiveState.CreatePointSecond: {
                    _checkPoint = m_cachedControlPoints[1].m_position;
                    _checkPosition = true;
                    break;
                }
                case ActiveState.CreatePointThird: {
                    _checkPoint = m_cachedControlPoints[2].m_position;
                    _checkPosition = true;
                    break;
                }
                case ActiveState.MaxFillContinue:
                case ActiveState.LockIdle: {
                    //don't think I need to test LockIdle nor MaxFillContinue
                    break;
                }
                case ActiveState.MovePointFirst: {
                    _checkPoint = m_cachedControlPoints[0].m_position;
                    _checkPosition = true;
                    break;
                }
                case ActiveState.MovePointSecond: {
                    _checkPoint = m_cachedControlPoints[1].m_position;
                    _checkPosition = true;
                    break;
                }
                case ActiveState.MovePointThird: {
                    _checkPoint = m_cachedControlPoints[2].m_position;
                    _checkPosition = true;
                    break;
                }
                case ActiveState.MoveSegment: {
                    break;
                }
                case ActiveState.ChangeSpacing: {
                    break;
                }
                case ActiveState.ChangeAngle: {
                    break;
                }
                default:
                    break;
            }
            if (_checkPosition) {
                if (_checkPoint != _hitPos) {
                    result = true;
                }
            }
            newPosition = _hitPos;
            return result;
        }

        //place IEnumerators down here

        [CompilerGenerated]
        //big Special Thanks to JapaMala's FineRoadHeights for how to do this!
        private sealed class TProcessKeyEventTc_Iterator32G : IEnumerator, IDisposable, IEnumerator<object> {
            internal object Scurrent;
            internal int SPC;
            internal PropLineTool TTf__this;
            internal KeyPressEvent _keyPressEvent;

            [DebuggerHidden]
            public void Dispose() {
                this.SPC = -1;
            }

            public bool MoveNext() {
                uint num = (uint)this.SPC;
                this.SPC = -1;
                switch (num) {
                    case 0: {
                        TTf__this.ProcessKeyInputImpl(_keyPressEvent);
                        this.Scurrent = 0;
                        return true;
                    }
                    case 1:
                    default: {
                        break;
                    }
                }
                return false;
            }

            [DebuggerHidden]
            public void Reset() {
                throw new NotSupportedException();
            }

            object IEnumerator<object>.Current {
                [DebuggerHidden]
                get {
                    return this.Scurrent;
                }
            }

            object IEnumerator.Current {
                [DebuggerHidden]
                get {
                    return this.Scurrent;
                }
            }
        }

    }
}

