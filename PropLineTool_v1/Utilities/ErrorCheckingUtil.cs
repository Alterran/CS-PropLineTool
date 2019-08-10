using System;
using ColossalFramework.Math;
using UnityEngine;
using ColossalFramework;

namespace PropLineTool.Utility.ErrorChecking {
    [Flags]
    public enum ItemCollisionType {
        None = 0,
        Props = 1,
        Trees = 2,
        Blocked = 4,
        Water = 8,
        GameArea = 16
    }

    public static class ItemCollisionTypeExtensions {
        public static bool HasFlag(this ItemCollisionType value, ItemCollisionType comparison) {
            if ((value & comparison) == comparison) {
                return true;
            } else {
                return false;
            }
        }
    }

    public static class ErrorChecker {
        // ==========================================================================  CHECK ALL COLLISIONS  ==========================================================================
        public static ItemCollisionType CheckAllCollisionsProp(Vector3 worldPosition, PropInfo propInfo) {
            ItemCollisionType _result = ItemCollisionType.None;

            Vector2 _center = VectorUtils.XZ(worldPosition);
            float _radius = 0.5f;
            Quad2 _quad = default(Quad2);
            _quad.a = _center + new Vector2(-_radius, -_radius);
            _quad.b = _center + new Vector2(-_radius, _radius);
            _quad.c = _center + new Vector2(_radius, _radius);
            _quad.d = _center + new Vector2(_radius, -_radius);

            float _minY = worldPosition.y;
            float _maxY = worldPosition.y + propInfo.m_generatedInfo.m_size.y * Mathf.Max(propInfo.m_maxScale, propInfo.m_minScale);

            ItemClass.CollisionType _collisionType = ItemClass.CollisionType.Terrain;

            if (DoesPropCollideWithProps(_quad, _minY, _maxY, _collisionType, propInfo)) {
                _result |= ItemCollisionType.Props;
            }
            if (DoesPropCollideWithTrees(_quad, _minY, _maxY, _collisionType, propInfo)) {
                _result |= ItemCollisionType.Trees;
            }
            if (CheckPropBlocked(_quad, _minY, _maxY, _collisionType, propInfo)) {
                _result |= ItemCollisionType.Blocked;
            }
            if (DoesPositionHaveWater(worldPosition)) {
                _result |= ItemCollisionType.Water;
            }
            if (IsQuadOutOfGameArea(_quad)) {
                _result |= ItemCollisionType.GameArea;
            }


            return _result;
        }
        public static ItemCollisionType CheckAllCollisionsTree(Vector3 worldPosition, TreeInfo treeInfo) {
            ItemCollisionType _result = ItemCollisionType.None;

            Vector2 _center = VectorUtils.XZ(worldPosition);
            float _radius = 0.5f;
            Quad2 _quad = default(Quad2);
            _quad.a = _center + new Vector2(-_radius, -_radius);
            _quad.b = _center + new Vector2(-_radius, _radius);
            _quad.c = _center + new Vector2(_radius, _radius);
            _quad.d = _center + new Vector2(_radius, -_radius);

            float _minY = worldPosition.y;
            float _maxY = worldPosition.y + treeInfo.m_generatedInfo.m_size.y * Mathf.Max(treeInfo.m_maxScale, treeInfo.m_minScale);

            ItemClass.CollisionType _collisionType = ItemClass.CollisionType.Terrain;

            if (DoesTreeCollideWithProps(_quad, _minY, _maxY, _collisionType, treeInfo)) {
                _result |= ItemCollisionType.Props;
            }
            if (DoesTreeCollideWithTrees(_quad, _minY, _maxY, _collisionType, treeInfo)) {
                _result |= ItemCollisionType.Trees;
            }
            if (CheckTreeBlocked(_quad, _minY, _maxY, _collisionType, treeInfo)) {
                _result |= ItemCollisionType.Blocked;
            }
            if (DoesPositionHaveWater(worldPosition)) {
                _result |= ItemCollisionType.Water;
            }
            if (IsQuadOutOfGameArea(_quad)) {
                _result |= ItemCollisionType.GameArea;
            }


            return _result;
        }

        // ==========================================================================  CHECK ALL COLLISIONS LITE  ==========================================================================
        public static bool CheckValidPlacementPropLite(Vector3 worldPosition, PropInfo propInfo) {
            bool _result = true;

            Vector2 _center = VectorUtils.XZ(worldPosition);
            float _radius = 0.5f;
            Quad2 _quad = default(Quad2);
            _quad.a = _center + new Vector2(-_radius, -_radius);
            _quad.b = _center + new Vector2(-_radius, _radius);
            _quad.c = _center + new Vector2(_radius, _radius);
            _quad.d = _center + new Vector2(_radius, -_radius);

            float _minY = worldPosition.y;
            float _maxY = worldPosition.y + propInfo.m_generatedInfo.m_size.y * Mathf.Max(propInfo.m_maxScale, propInfo.m_minScale);

            ItemClass.CollisionType _collisionType = ItemClass.CollisionType.Terrain;

            if (IsQuadOutOfGameArea(_quad)) {
                _result = false;
            } else if (CheckPropBlocked(_quad, _minY, _maxY, _collisionType, propInfo)) {
                _result = false;
            } else if (DoesPositionHaveWater(worldPosition)) {
                _result = false;
            } else if (DoesPropCollideWithTrees(_quad, _minY, _maxY, _collisionType, propInfo)) {
                _result = false;
            } else if (DoesPropCollideWithProps(_quad, _minY, _maxY, _collisionType, propInfo)) {
                _result = false;
            }

            return _result;
        }
        public static bool CheckValidPlacementTreeLite(Vector3 worldPosition, TreeInfo treeInfo) {
            bool _result = true;

            Vector2 _center = VectorUtils.XZ(worldPosition);
            float _radius = 0.5f;
            Quad2 _quad = default(Quad2);
            _quad.a = _center + new Vector2(-_radius, -_radius);
            _quad.b = _center + new Vector2(-_radius, _radius);
            _quad.c = _center + new Vector2(_radius, _radius);
            _quad.d = _center + new Vector2(_radius, -_radius);

            float _minY = worldPosition.y;
            float _maxY = worldPosition.y + treeInfo.m_generatedInfo.m_size.y * Mathf.Max(treeInfo.m_maxScale, treeInfo.m_minScale);

            ItemClass.CollisionType _collisionType = ItemClass.CollisionType.Terrain;

            if (IsQuadOutOfGameArea(_quad)) {
                _result = false;
            } else if (CheckTreeBlocked(_quad, _minY, _maxY, _collisionType, treeInfo)) {
                _result = false;
            } else if (DoesPositionHaveWater(worldPosition)) {
                _result = false;
            } else if (DoesTreeCollideWithTrees(_quad, _minY, _maxY, _collisionType, treeInfo)) {
                _result = false;
            } else if (DoesTreeCollideWithProps(_quad, _minY, _maxY, _collisionType, treeInfo)) {
                _result = false;
            }

            return _result;
        }

        // ==========================================================================  PROP COLLISION  ==========================================================================
        public static bool DoesPropCollideWithProps(Quad2 quad, float minY, float maxY, ItemClass.CollisionType collisionType, PropInfo propInfo) {
            bool _result = false;

            if (Singleton<PropManager>.instance.OverlapQuad(quad, minY, maxY, collisionType, 0, 0)) {
                _result = true;
            }

            return _result;
        }
        public static bool DoesTreeCollideWithProps(Quad2 quad, float minY, float maxY, ItemClass.CollisionType collisionType, TreeInfo treeInfo) {
            bool _result = false;

            if (Singleton<PropManager>.instance.OverlapQuad(quad, minY, maxY, collisionType, 0, 0)) {
                _result = true;
            }

            return _result;
        }

        // ==========================================================================  TREE COLLISION  ==========================================================================
        public static bool DoesPropCollideWithTrees(Quad2 quad, float minY, float maxY, ItemClass.CollisionType collisionType, PropInfo propInfo) {
            bool _result = false;

            if (Singleton<TreeManager>.instance.OverlapQuad(quad, minY, maxY, collisionType, 0, 0u)) {
                _result = true;
            }

            return _result;
        }
        public static bool DoesTreeCollideWithTrees(Quad2 quad, float minY, float maxY, ItemClass.CollisionType collisionType, TreeInfo treeInfo) {
            bool _result = false;

            if (Singleton<TreeManager>.instance.OverlapQuad(quad, minY, maxY, collisionType, 0, 0u)) {
                _result = true;
            }

            return _result;
        }

        // ==========================================================================  NET/BUILDING COLLISION  ==========================================================================
        public static bool CheckPropBlocked(Quad2 quad, float minY, float maxY, ItemClass.CollisionType collisionType, PropInfo propInfo) {
            bool _result = false;

            if (Singleton<NetManager>.instance.OverlapQuad(quad, minY, maxY, collisionType, propInfo.m_class.m_layer, 0, 0, 0)) {
                _result = true;
            }
            if (Singleton<BuildingManager>.instance.OverlapQuad(quad, minY, maxY, collisionType, propInfo.m_class.m_layer, 0, 0, 0)) {
                _result = true;
            }

            return _result;
        }
        public static bool CheckTreeBlocked(Quad2 quad, float minY, float maxY, ItemClass.CollisionType collisionType, TreeInfo treeInfo) {
            bool _result = false;

            if (Singleton<NetManager>.instance.OverlapQuad(quad, minY, maxY, collisionType, treeInfo.m_class.m_layer, 0, 0, 0)) {
                _result = true;
            }
            if (Singleton<BuildingManager>.instance.OverlapQuad(quad, minY, maxY, collisionType, treeInfo.m_class.m_layer, 0, 0, 0)) {
                _result = true;
            }

            return _result;
        }

        // ==========================================================================  TERRAIN WATER COLLISION  ==========================================================================
        public static bool DoesPositionHaveWater(Vector3 worldPosition) {
            bool _result = false;

            if (Singleton<TerrainManager>.instance.HasWater(new Vector2(worldPosition.x, worldPosition.z)) == true) {
                _result = true;
            }

            return _result;
        }

        // ==========================================================================  GAME AREA COLLISION  ==========================================================================
        public static bool IsQuadOutOfGameArea(Quad2 quad) {
            bool _result = false;

            if (Singleton<GameAreaManager>.instance.QuadOutOfArea(quad)) {
                _result = true;
            }

            return _result;
        }


    }



}