using System;
using UnityEngine;
using PropLineTool.Utility;

namespace PropLineTool {
    public struct SegmentInfo {
        //used in non-fence mode
        public float m_lastFinalOffset;
        public float m_newFinalOffset;
        //used in fence mode
        public Vector3 m_lastFenceEndpoint;
        public Vector3 m_newFenceEndpoint;
        //used in both
        public bool m_isContinueDrawing;
        public bool m_keepLastOffsets;
        public bool m_maxItemCountExceeded;
        public bool m_isMaxFillContinue;
        //error checking
        public bool m_allItemsValid;

        /// <summary>
        /// Returns true if the maximum number of item slots for this segment are in use and the curve is ready to continue filling the same curve on the next segment.
        /// </summary>
        public bool isReadyForMaxContinue {
            get {
                return m_maxItemCountExceeded /*&& isContinueDrawing*/;
            }
        }

        public static SegmentInfo defaultValue {
            get {
                return new SegmentInfo(0f, 0f, Vector3.down, Vector3.down, false, false, false, false, true);
            }
        }

        public SegmentInfo(float lastFinalOffset, float newFinalOffset, Vector3 lastFenceEndpoint, Vector3 newFenceEndpoint, bool isContinueDrawing, bool keepLastOffsets, bool maxItemCountExceeded, bool isMaxFillContinue, bool allItemsValid) {
            this.m_lastFinalOffset = lastFinalOffset;
            this.m_newFinalOffset = newFinalOffset;
            this.m_lastFenceEndpoint = lastFenceEndpoint;
            this.m_newFenceEndpoint = newFenceEndpoint;
            this.m_isContinueDrawing = isContinueDrawing;
            this.m_keepLastOffsets = keepLastOffsets;
            this.m_maxItemCountExceeded = maxItemCountExceeded;
            this.m_isMaxFillContinue = isMaxFillContinue;
            this.m_allItemsValid = allItemsValid;
        }
    }


    //170527: Leave PlacementCalculator.m_itemCount out for now.
    public class SegmentState {
        //segmentInfo
        private SegmentInfo m_segmentInfo = SegmentInfo.defaultValue;

        // ======== Events ======== 
        //Can't get this event to work... nothing will subscribe to it...
        //adding the = delegate(); removes the need for a null check
        //   source: https://stackoverflow.com/questions/289002/how-to-raise-custom-event-from-a-static-class
        //...
        public event VoidEventHandler eventLastContinueParameterChanged = delegate { };
        private void OnLastContinueParameterChanged() {
            //Debug.Log("[PLTDEBUG]: OnLastContinueParameterChanged...");

            //TODO: not firing event for some reason
            //Debug.Log("[PLTDEBUG]: Firing event...");

            eventLastContinueParameterChanged?.Invoke();
        }

        // ======== Properties ======== 
        // === segmentInfo ===
        public SegmentInfo segmentInfo {
            get {
                return m_segmentInfo;
            }
        }
        // === non-fence === 
        public float lastFinalOffset {
            get {
                return m_segmentInfo.m_lastFinalOffset;
            }
            set {
                float _oldValue = m_segmentInfo.m_lastFinalOffset;
                m_segmentInfo.m_lastFinalOffset = value;
                if (value != _oldValue) {
                    OnLastContinueParameterChanged();
                }
            }
        }
        public float newFinalOffset {
            get {
                return m_segmentInfo.m_newFinalOffset;
            }
            set {
                m_segmentInfo.m_newFinalOffset = value;
            }
        }
        // === fence === 
        public Vector3 lastFenceEndpoint {
            get {
                return m_segmentInfo.m_lastFenceEndpoint;
            }
            set {
                Vector3 _oldValue = m_segmentInfo.m_lastFenceEndpoint;
                m_segmentInfo.m_lastFenceEndpoint = value;
                if (_oldValue != value) {
                    OnLastContinueParameterChanged();
                }
            }
        }
        public Vector3 newFenceEndpoint {
            get {
                return m_segmentInfo.m_newFenceEndpoint;
            }
            set {
                m_segmentInfo.m_newFenceEndpoint = value;
            }
        }
        // === both ===
        public bool isContinueDrawing {
            get {
                return m_segmentInfo.m_isContinueDrawing;
            }
            set {
                m_segmentInfo.m_isContinueDrawing = value;
            }
        }
        public bool keepLastOffsets {
            get {
                return m_segmentInfo.m_keepLastOffsets;
            }
            set {
                m_segmentInfo.m_keepLastOffsets = value;
            }
        }
        public bool maxItemCountExceeded {
            get {
                return m_segmentInfo.m_maxItemCountExceeded;
            }
            set {
                m_segmentInfo.m_maxItemCountExceeded = value;
            }
        }
        /// <summary>
        /// Returns true if the maximum number of item slots for this segment are in use and the curve is ready to continue filling the same curve on the next segment.
        /// </summary>
        public bool isReadyForMaxContinue {
            get {
                //return maxItemCountExceeded /*&& isContinueDrawing*/;

                return segmentInfo.isReadyForMaxContinue;
            }
        }
        /// <summary>
        /// Returns true if the curve is currently continue-drawing to fill the same curve because max item count threshold was exceeded.
        /// </summary>
        public bool isMaxFillContinue {
            get {
                return m_segmentInfo.m_isMaxFillContinue;
            }
            set {
                bool _oldValue = m_segmentInfo.m_isMaxFillContinue;
                m_segmentInfo.m_isMaxFillContinue = value;
                if (_oldValue != value) {
                    OnLastContinueParameterChanged();
                }
            }
        }
        // === error checking ===
        /// <summary>
        /// Whether or not all items in the segment have no collision errors. Is not necessarily true if anarchyPLT is true.
        /// </summary>
        public bool allItemsValid {
            get {
                return m_segmentInfo.m_allItemsValid;
            }
            set {
                m_segmentInfo.m_allItemsValid = value;
            }
        }

        // ======== Methods ======== 
        public bool FinalizeForPlacement(bool continueDrawing) {
            if (continueDrawing) {
                if (keepLastOffsets == false) {
                    lastFenceEndpoint = newFenceEndpoint;
                    lastFinalOffset = newFinalOffset;
                }

                if (isReadyForMaxContinue) {
                    isMaxFillContinue = true;
                } else {
                    isMaxFillContinue = false;
                }
            } else {
                lastFenceEndpoint = Vector3.down;
                lastFinalOffset = 0f;

                keepLastOffsets = false;

                isMaxFillContinue = false;
            }
            newFenceEndpoint = Vector3.down;
            newFinalOffset = 0f;
            return true;
        }

        public void ResetLastContinueParameters() {
            lastFenceEndpoint = Vector3.down;
            lastFinalOffset = 0f;
        }
        /// <summary>
        /// Used for Undo max-fill-continue.
        /// </summary>
        /// <param name="lastFinalOffsetValue"></param>
        /// <param name="lastFenceEndpointVector"></param>
        /// <returns></returns>
        internal void RevertLastContinueParameters(float lastFinalOffsetValue, Vector3 lastFenceEndpointVector) {
            keepLastOffsets = false;

            //newFinalOffset = lastFinalOffsetValue;
            //newFenceEndpoint = lastFenceEndpointVector;
            lastFinalOffset = lastFinalOffsetValue;
            lastFenceEndpoint = lastFenceEndpointVector;
        }

        public bool AreLastContinueParametersZero() {
            if ((lastFenceEndpoint == Vector3.down || lastFenceEndpoint == Vector3.zero) && lastFinalOffset == 0f) {
                return true;
            } else {
                return false;
            }
        }
        public bool AreNewContinueParametersEmpty() {
            if ((m_segmentInfo.m_newFenceEndpoint == Vector3.down || m_segmentInfo.m_newFenceEndpoint == Vector3.zero) && m_segmentInfo.m_newFinalOffset == 0f) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Checks whether an input *position* is close to the last fence endpoint
        /// </summary>
        /// <param name="position"></param>
        /// <returns>true if the input position is within 2mm of the last fence endpoint</returns>
        public bool IsPositionEqualToLastFenceEndpoint(Vector3 position) {
            float _distance = Vector3.Distance(position, lastFenceEndpoint);
            if (_distance <= 0.002f) {
                return true;
            } else {
                return false;
            }
        }
    }


}