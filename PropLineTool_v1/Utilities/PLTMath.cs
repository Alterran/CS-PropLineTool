using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace PropLineTool.Math {
    public static class MathPLT {
        //Special Thanks to Tinus on the UnityForums for this!
        /// <summary>
        /// Determines the signed angle (-pi to pi) radians between two vectors.
        /// </summary>
        /// <param name="v1">first vector</param>
        /// <param name="v2">second vector</param>
        /// <param name="n">rotation axis (usually plane normal of v1, v2)</param>
        /// <returns>signed angle (in Radians) between v1 and v2</returns>
        public static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n) {
            float result = 0f;
            result = Mathf.Atan2(
                Vector3.Dot(n, Vector3.Cross(v1, v2)),
                Vector3.Dot(v1, v2)
                );
            return result;
        }

        /// <summary>
        /// Normalizes an angle (in degrees) between 0 and 360 degrees.
        /// </summary>
        /// <param name="inputAngle">angle in Degrees</param>
        /// <returns></returns>
        public static float NormalizeAngle360(float inputAngle) {
            float result = 0f;
            float _angle = Mathf.Abs(inputAngle) % 360f;
            if (inputAngle < 0f) {
                result = -1f * _angle;
            } else {
                result = _angle;
            }
            return result;
        }

        /// <summary>
        /// Constrains Bezier to XZ plane.
        /// </summary>
        /// <param name="bezier"></param>
        /// <returns>A bezier curve with y-components set to zero</returns>
        public static Bezier3 BezierXZ(Bezier3 bezier) {
            Bezier3 result = new Bezier3();

            result = bezier;
            result.a.y = 0f;
            result.b.y = 0f;
            result.c.y = 0f;
            result.d.y = 0f;

            return result;
        }

        /// <summary>
        /// Constrains input Bezier to XZ plane.
        /// </summary>
        /// <param name="bezier"></param>
        public static void BezierXZ(ref Bezier3 bezier) {
            bezier.a.y = 0f;
            bezier.b.y = 0f;
            bezier.c.y = 0f;
            bezier.d.y = 0f;
        }

        /// <summary>
        /// Constrains Segment to XZ plane.
        /// </summary>
        /// <param name="bezier"></param>
        /// <returns>A bezier curve with y-components set to zero</returns>
        public static Segment3 SegmentXZ(Segment3 lineSegment) {
            Segment3 result = new Segment3();

            result = lineSegment;
            result.a.y = 0f;
            result.b.y = 0f;

            return result;
        }

        /// <summary>
        /// Constrains input Segment to XZ plane.
        /// </summary>
        /// <param name="bezier"></param>
        /// <returns>A bezier curve with y-components set to zero</returns>
        public static void SegmentXZ(ref Segment3 lineSegment) {
            lineSegment.a.y = 0f;
            lineSegment.b.y = 0f;
        }

        //standard conversion
        public static Bezier3 QuadraticToCubicBezier(Vector3 startPoint, Vector3 middlePoint, Vector3 endPoint) {
            Bezier3 bezier = new Bezier3 {
                a = startPoint,
                b = startPoint + (2.0f / 3.0f) * (middlePoint - startPoint),
                c = endPoint + (2.0f / 3.0f) * (middlePoint - endPoint),
                d = endPoint
            };
            return bezier;
        }

        //CO's in-house method
        //uses negative of endDirection
        //rounds out tight re-curves (or tight curves)
        public static Bezier3 QuadraticToCubicBezierCOMethod(Vector3 startPoint, Vector3 startDirection, Vector3 endPoint, Vector3 endDirection /*switch this sign when using!*/) {
            Bezier3 bezier = new Bezier3 {
                a = startPoint,
                d = endPoint
            };
            NetSegment.CalculateMiddlePoints(startPoint, startDirection, endPoint, endDirection, false, false, out bezier.b, out bezier.c);
            return bezier;
        }

        /// <summary>
        /// Interpolates and Extrapolates the position along a parametric line defined by two points.
        /// </summary>
        /// <param name="segment">Line segment from p0 to p1</param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3 LinePosition(Segment3 segment, float t) {
            float num = 1 - t;
            Vector3 _p0 = segment.a;
            Vector3 _p1 = segment.b;
            Vector3 result = new Vector3(_p1.x + num * (_p0.x - _p1.x), _p1.y + num * (_p0.y - _p1.y), _p1.z + num * (_p0.z - _p1.z));
            return result;
        }

        //used to calculate t in non-fence Curved and Freeform modes
        //for each individual item
        /// <summary>
        /// Solves for t-value which would be a length of *distance* along the curve from original point at t = *tStart*.
        /// </summary>
        /// <param name="bezier"></param>
        /// <param name="tStart"></param>
        /// <param name="distance"></param>
        /// <param name="tolerance"></param>
        /// <param name="tEnd"></param>
        public static void StepDistanceCurve(Bezier3 bezier, float tStart, float distance, float tolerance, out float tEnd) {
            float _tCurrent = 0f;
            _tCurrent = bezier.Travel(tStart, distance);
            float _tStepInitial = _tCurrent - tStart;

            Vector3 _posCurrent = bezier.Position(_tCurrent);

            float _distCurrent = CubicBezierArcLengthXZGauss04(bezier, tStart, _tCurrent);
            float _toleranceSqr = tolerance * tolerance;
            if (Mathf.Pow(distance - _distCurrent, 2f) >= _toleranceSqr) {
                float _localSpeed = CubicSpeedXZ(bezier, _tCurrent);
                float _distDifference = distance - _distCurrent;

                int _counter = 0;
                while (_counter < 12 && (Mathf.Pow(_distDifference, 2f) > _toleranceSqr)) {
                    _distCurrent = CubicBezierArcLengthXZGauss04(bezier, tStart, _tCurrent);
                    _distDifference = distance - _distCurrent;
                    _localSpeed = CubicSpeedXZ(bezier, _tCurrent);

                    _tCurrent += _distDifference / _localSpeed;
                    _counter++;
                }
            }

            tEnd = _tCurrent;
        }


        //used to calculate t in Fence-Mode-ON Curved and Freeform modes
        //   In Placement Calculator:
        //   use this t to set the fence endpoints (:
        //   then calculate fence midpoints/placement points from the endpoints
        /// <summary>
        /// Solves for t-value which would yield a further point on the curve that is a _straight_ length of *lengthOfSegment* from the original point at t = *tStart*.
        /// </summary>
        /// <param name="bezier"></param>
        /// <param name="tStart"></param>
        /// <param name="lengthOfSegment"></param>
        /// <param name="tolerance"></param>
        /// <param name="tEnd"></param>
        /// <param name="allowBackwards">Set to false to only step forward along the curve in the direction t=0 -> t=1.</param>
        public static bool CircleCurveFenceIntersectXZ(Bezier3 bezier, float tStart, float lengthOfSegment, float tolerance, out float tEnd, bool allowBackwards) {
            tEnd = tStart;
            Bezier3 _bezier = BezierXZ(bezier);

            float _toleranceSqr = tolerance * tolerance;

            //haven't tested this to see if it will really go backwards
            //original as of 161111 2221
            //lengthOfSegment = Mathf.Abs(lengthOfSegment);
            //new as of 161111 2221
            if (!allowBackwards) {
                lengthOfSegment = Mathf.Abs(lengthOfSegment);
            }

            if (lengthOfSegment == 0f) {
                //already called in the beginning
                tEnd = tStart;
                return false;
            }

            //initial guess
            float _t0 = 0.5f;
            //initial guess setup
            StepDistanceCurve(_bezier, tStart, lengthOfSegment, tolerance, out _t0);

            //current guess t_n "t sub n"
            float _t = _t0;

            //Newton's Method
            float _errorFunc = PLTErrorFunctionXZ(_bezier, _t0, tStart, lengthOfSegment);
            float _errorPrime = PLTErrorFunctionPrimeXZ(_bezier, _t0, tStart);
            float _adjustmentScalar = 1.0f;  //if using multiplicity, _adjustmentScalar = 2

            //while loop fix
            float _iteratedDistance = Vector3.Distance(_bezier.Position(_t0), _bezier.Position(tStart));

            int _counter = 0;
            while (_counter < 25 && Mathf.Pow(_iteratedDistance - lengthOfSegment, 2f) > _toleranceSqr) {
                _t -= _adjustmentScalar * (_errorFunc / _errorPrime);

                if (!allowBackwards && _t < tStart) {
                    _t = 1f;
                }

                _errorFunc = PLTErrorFunctionXZ(_bezier, _t, tStart, lengthOfSegment);
                _errorPrime = PLTErrorFunctionPrimeXZ(_bezier, _t, tStart);

                _iteratedDistance = Vector3.Distance(_bezier.Position(_t), _bezier.Position(tStart));

                _counter++;
            }

            //finish
            tEnd = _t;


            if (Mathf.Pow(_iteratedDistance - lengthOfSegment, 2f) > _toleranceSqr) {
                //failed to converge
                return false;
            } else {
                //success in convergence!
                return true;
            }
        }

        //Specialty function
        //used to calculate t in Fence-Mode-ON Curved and Freeform modes
        //   In Placement Calculator:
        //   use this t to set the fence endpoints (:
        //   then calculate fence midpoints/placement points from the endpoints
        /// <summary>
        /// Specialty version of CircleCurveFenceIntersectXZ used to link curves. Solves for t-value which would yield a point on the curve that is a _straight_ length of *lengthOfSegment* from the original point usually off the curve at *startPos*.
        /// </summary>
        /// <param name="_bezier"></param>
        /// <param name="startPos"></param>
        /// <param name="lengthOfSegment"></param>
        /// <param name="tolerance"></param>
        /// <param name="tEnd"></param>
        /// <param name="allowBackwards">Set to false to only step forward along the curve in the direction t=0 -> t=1.</param>
        public static bool LinkCircleCurveFenceIntersectXZ(Bezier3 bezier, Vector3 startPos, float lengthOfSegment, float tolerance, out float tEnd, bool allowBackwards) {
            tEnd = 0f;
            Bezier3 _bezier = BezierXZ(bezier);

            float _toleranceSqr = tolerance * tolerance;

            lengthOfSegment = Mathf.Abs(lengthOfSegment);

            if (lengthOfSegment == 0f) {
                tEnd = 0f;
                return false;
            }

            //initial guess
            float _t0 = 0.5f;
            //initial guess setup
            float _leftoverLength = lengthOfSegment - Vector3.Distance(startPos, _bezier.a);
            StepDistanceCurve(_bezier, 0f, _leftoverLength, tolerance, out _t0);

            //current guess t_n "t sub n"
            float _t = _t0;

            //Newton's Method
            float _errorFunc = PLTLinkErrorFunctionXZ(_bezier, _t0, startPos, lengthOfSegment);
            float _errorPrime = PLTLinkErrorFunctionPrimeXZ(_bezier, _t0, startPos);
            float _adjustmentScalar = 1.0f;  //if using multiplicity, _adjustmentScalar = 2

            //while loop fix
            float _iteratedDistance = Vector3.Distance(_bezier.Position(_t0), startPos);

            int _counter = 0;
            while (_counter < 12 && Mathf.Pow(_iteratedDistance - lengthOfSegment, 2f) > _toleranceSqr) {
                _t -= _adjustmentScalar * (_errorFunc / _errorPrime);

                if (!allowBackwards && _t < 0f) {
                    _t = 1f;
                }

                _errorFunc = PLTLinkErrorFunctionXZ(_bezier, _t, startPos, lengthOfSegment);
                _errorPrime = PLTLinkErrorFunctionPrimeXZ(_bezier, _t, startPos);

                _iteratedDistance = Vector3.Distance(_bezier.Position(_t), startPos);

                _counter++;
            }

            //finish
            tEnd = _t;

            if (Mathf.Pow(_iteratedDistance - lengthOfSegment, 2f) > _toleranceSqr) {
                //failed to converge
                return false;
            } else {
                //success in convergence!
                return true;
            }
        }

        //Uses Legendre-Gauss Quadrature with n = 12.
        /// <summary>
        /// Returns the XZ arclength of a cubic bezier curve between t1 and t2.
        /// </summary>
        /// <param name="bezier"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static float CubicBezierArcLengthXZGauss12(Bezier3 bezier, float t1, float t2) {
            float result = 0f;
            float _linearAdj = (t2 - t1) / 2f;
            float _p1 = CubicSpeedXZGaussPoint(bezier, 0.1252334085114689f, 0.2491470458134028f, t1, t2);
            float _p2 = CubicSpeedXZGaussPoint(bezier, -0.1252334085114689f, 0.2491470458134028f, t1, t2);
            float _p3 = CubicSpeedXZGaussPoint(bezier, 0.3678314989981802f, 0.2334925365383548f, t1, t2);
            float _p4 = CubicSpeedXZGaussPoint(bezier, -0.3678314989981802f, 0.2334925365383548f, t1, t2);
            float _p5 = CubicSpeedXZGaussPoint(bezier, 0.5873179542866175f, 0.2031674267230659f, t1, t2);
            float _p6 = CubicSpeedXZGaussPoint(bezier, -0.5873179542866175f, 0.2031674267230659f, t1, t2);
            float _p7 = CubicSpeedXZGaussPoint(bezier, 0.7699026741943047f, 0.1600783285433462f, t1, t2);
            float _p8 = CubicSpeedXZGaussPoint(bezier, -0.7699026741943047f, 0.1600783285433462f, t1, t2);
            float _p9 = CubicSpeedXZGaussPoint(bezier, 0.9041172563704749f, 0.1069393259953184f, t1, t2);
            float _p10 = CubicSpeedXZGaussPoint(bezier, -0.9041172563704749f, 0.1069393259953184f, t1, t2);
            float _p11 = CubicSpeedXZGaussPoint(bezier, 0.9815606342467192f, 0.0471753363865118f, t1, t2);
            float _p12 = CubicSpeedXZGaussPoint(bezier, -0.9815606342467192f, 0.0471753363865118f, t1, t2);
            result = _linearAdj * (_p1 + _p2 + _p3 + _p4 + _p5 + _p6 + _p7 + _p8 + _p9 + _p10 + _p11 + _p12);
            return result;
        }

        //Uses Legendre-Gauss Quadrature with n = 4.
        /// <summary>
        /// Returns the XZ arclength of a cubic bezier curve between t1 and t2.
        /// </summary>
        /// <param name="bezier"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static float CubicBezierArcLengthXZGauss04(Bezier3 bezier, float t1, float t2) {
            float result = 0f;
            float _linearAdj = (t2 - t1) / 2f;
            float _p1 = CubicSpeedXZGaussPoint(bezier, 0.3399810435848563f, 0.6521451548625461f, t1, t2);
            float _p2 = CubicSpeedXZGaussPoint(bezier, -0.3399810435848563f, 0.6521451548625461f, t1, t2);
            float _p3 = CubicSpeedXZGaussPoint(bezier, 0.8611363115940526f, 0.3478548451374538f, t1, t2);
            float _p4 = CubicSpeedXZGaussPoint(bezier, -0.8611363115940526f, 0.3478548451374538f, t1, t2);
            result = _linearAdj * (_p1 + _p2 + _p3 + _p4);
            return result;
        }

        //Uses Legendre-Gauss Quadrature with n = 3.
        /// <summary>
        /// Returns the XZ arclength of a cubic bezier curve between t1 and t2.
        /// </summary>
        /// <param name="bezier"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static float CubicBezierArcLengthXZGauss03(Bezier3 bezier, float t1, float t2) {
            float result = 0f;
            float _linearAdj = (t2 - t1) / 2f;
            float _p1 = CubicSpeedXZGaussPoint(bezier, 0.0f, 0.88888888f, t1, t2);
            float _p2 = CubicSpeedXZGaussPoint(bezier, 0.77459667f, 0.55555555f, t1, t2);
            float _p3 = CubicSpeedXZGaussPoint(bezier, -0.77459667f, 0.55555555f, t1, t2);
            result = _linearAdj * (_p1 + _p2 + _p3);
            return result;
        }

        //returns a single point for Gaussian Quadrature
        //of cubic bezier arc length
        private static float CubicSpeedXZGaussPoint(Bezier3 bezier, float x_i, float w_i, float a, float b) {
            float result = 0f;
            float _linearAdj = (b - a) / 2f;
            float _constantAdj = (a + b) / 2f;
            result = w_i * CubicSpeedXZ(bezier, _linearAdj * x_i + _constantAdj);
            return result;
        }

        //returns the integrand of the arc length function for a cubic bezier curve
        //constrained to the XZ-plane
        //at a specific t
        private static float CubicSpeedXZ(Bezier3 bezier, float t) {
            float result = 0f;
            Vector3 _tangent = bezier.Tangent(t);
            float _derivX = _tangent.x;
            float _derivZ = _tangent.z;
            result = Mathf.Sqrt(Mathf.Pow(_derivX, 2f) + Mathf.Pow(_derivZ, 2f));
            return result;
        }



        /// <summary>
        /// Returns the xz speed of the line in units of distance/(delta-t)
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public static float LinearSpeedXZ(Segment3 segment) {
            float result = 0f;
            Vector3 _tanVector = Vector3.zero;
            _tanVector = segment.b - segment.a;
            float _derivX = _tanVector.x;
            float _derivZ = _tanVector.z;
            result = Mathf.Sqrt(Mathf.Pow(_derivX, 2f) + Mathf.Pow(_derivZ, 2f));
            return result;
        }


        /// <summary>
        /// Returns E(t) = E(tGuess) : +/- Error [meters^2] in (straight-line distance between two points on a bezier curve) vs (radius).
        /// </summary>
        /// <param name="bezier"></param>
        /// <param name="t">Where are you guessing the intersection is?</param>
        /// <param name="tCenter">Center of the circle to intersect the curve.</param>
        /// <param name="radius">Radius of the circle to intersect the curve.</param>
        /// <returns></returns>
        private static float PLTErrorFunctionXZ(Bezier3 bezier, float t, float tCenter, float radius) {
            float result = 100f;

            if (t == tCenter) {
                return 0f;
            }

            Vector3 _center = bezier.Position(tCenter);
            float x_c = _center.x;
            float z_c = _center.z;

            Vector3 _guessPos = bezier.Position(t);
            float _x = _guessPos.x;
            float _z = _guessPos.z;

            result = Mathf.Pow(_x - x_c, 2f) + Mathf.Pow(_z - z_c, 2f) - Mathf.Pow(radius, 2f);

            return result;
        }

        /// <summary>
        /// Specialty Version of PLTErrorFunctionXZ used to link curves. Returns E(t) = E(tGuess) : +/- Error [meters^2] in (straight-line distance between startPoint and point on bezier curve) vs (radius).
        /// </summary>
        /// <param name="bezier"></param>
        /// <param name="t">Where are you guessing the intersection is?</param>
        /// <param name="centerPos">Center of the circle to intersect the curve.</param>
        /// <param name="radius">Radius of the circle to intersect the curve.</param>
        /// <returns></returns>
        private static float PLTLinkErrorFunctionXZ(Bezier3 bezier, float t, Vector3 centerPos, float radius) {
            float result = 100f;

            Vector3 _center = centerPos;
            float x_c = _center.x;
            float z_c = _center.z;

            Vector3 _guessPos = bezier.Position(t);
            float _x = _guessPos.x;
            float _z = _guessPos.z;

            if (_guessPos == centerPos) {
                return 0f;
            }

            result = Mathf.Pow(_x - x_c, 2f) + Mathf.Pow(_z - z_c, 2f) - Mathf.Pow(radius, 2f);

            return result;
        }

        /// <summary>
        /// Returns E'(t) = E'(tGuess) : Derivative of +/- Error [meters^2] in (straight-line distance between two points on a bezier curve) vs (radius).
        /// </summary>
        /// <param name="bezier"></param>
        /// <param name="t">Where are you guessing the intersection is?</param>
        /// <param name="tCenter">Center of the circle to intersect the curve.</param>
        /// <returns></returns>
        private static float PLTErrorFunctionPrimeXZ(Bezier3 bezier, float t, float tCenter) {
            float result = 0f;

            if (t == tCenter) {
                return 0f;
            }

            Vector3 _center = bezier.Position(tCenter);
            float x_c = _center.x;
            float z_c = _center.z;

            Vector3 _guessPos = bezier.Position(t);
            float _x = _guessPos.x; //x(t)
            float _z = _guessPos.z; //z(t)

            Vector3 _derivPos = bezier.Tangent(t);
            float _xPrime = _derivPos.x;    //x'(t)
            float _zPrime = _derivPos.z;    //z'(t)

            result = 2 * (_x - x_c) * _xPrime + 2 * (_z - z_c) * _zPrime;

            return result;
        }

        /// <summary>
        /// Specialty Version of PLTErrorFunctionPrimeXZ used to link curves. Returns E'(t) = E'(tGuess) : Derivative of +/- Error [meters^2] in (straight-line distance between startPoint and point on bezier curve) vs (radius).
        /// </summary>
        /// <param name="bezier"></param>
        /// <param name="t">Where are you guessing the intersection is?</param>
        /// <param name="centerPos">Center of the circle to intersect the curve.</param>
        /// <returns></returns>
        private static float PLTLinkErrorFunctionPrimeXZ(Bezier3 bezier, float t, Vector3 centerPos) {
            float result = 0f;

            Vector3 _center = centerPos;
            float x_c = _center.x;
            float z_c = _center.z;

            Vector3 _guessPos = bezier.Position(t);
            float _x = _guessPos.x; //x(t)
            float _z = _guessPos.z; //z(t)

            if (_guessPos == centerPos) {
                return 0f;
            }

            Vector3 _derivPos = bezier.Tangent(t);
            float _xPrime = _derivPos.x;    //x'(t)
            float _zPrime = _derivPos.z;    //z'(t)

            result = 2 * (_x - x_c) * _xPrime + 2 * (_z - z_c) * _zPrime;

            return result;
        }

        // =============  HOVERING STUFF  =============
        /// <summary>
        /// Checks to see whether a given point lies within a circle of given center and radius. In the XZ-plane.
        /// </summary>
        /// <param name="circleCenter"></param>
        /// <param name="radius"></param>
        /// <param name="pointOfInterest"></param>
        /// <returns></returns>
        public static bool IsInsideCircleXZ(Vector3 circleCenter, float radius, Vector3 pointOfInterest) {
            if (radius == 0f) {
                return pointOfInterest == circleCenter ? true : false;
            } else if (radius < 0f) {
                radius = Mathf.Abs(radius);
            }

            //constrain to XZ plane
            circleCenter.y = 0f;
            pointOfInterest.y = 0f;



            float _radiusSqr = radius * radius;
            float _distanceSqr = (pointOfInterest - circleCenter).sqrMagnitude;

            if (_distanceSqr <= _radiusSqr) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Checks to see whether a given point is close to a circle outline of given center and radius. In the XZ-plane.
        /// </summary>
        /// <param name="circleCenter"></param>
        /// <param name="radius"></param>
        /// <param name="pointOfInterest"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool IsNearCircleOutlineXZ(Vector3 circleCenter, float circleRadius, Vector3 pointOfInterest, float distance) {
            if (distance == 0f) {
                return pointOfInterest == circleCenter ? true : false;
            } else if (distance < 0f) {
                distance = Mathf.Abs(distance);
            }

            //constrain to XZ plane
            circleCenter.y = 0f;
            pointOfInterest.y = 0f;


            float _lesserRadiusSqr = Mathf.Pow(circleRadius - distance, 2f);
            float _greaterRadiusSqr = Mathf.Pow(circleRadius + distance, 2f);
            float _distanceSqr = (pointOfInterest - circleCenter).sqrMagnitude;

            if (_distanceSqr >= _lesserRadiusSqr && _distanceSqr <= _greaterRadiusSqr) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Checks to see whether a given point is close to a circle outline. In the XZ-plane.
        /// </summary>
        /// <param name="circleCenter"></param>
        /// <param name="radius"></param>
        /// <param name="pointOfInterest"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool IsNearCircleOutlineXZ(Circle3XZ circle, Vector3 pointOfInterest, float distance) {
            Vector3 _circleCenter = circle.center;
            float _circleRadius = circle.radius;

            if (distance == 0f) {
                return pointOfInterest == _circleCenter ? true : false;
            } else if (distance < 0f) {
                distance = Mathf.Abs(distance);
            }

            //constrain to XZ plane
            _circleCenter.y = 0f;
            pointOfInterest.y = 0f;


            float _lesserRadiusSqr = Mathf.Pow(_circleRadius - distance, 2f);
            float _greaterRadiusSqr = Mathf.Pow(_circleRadius + distance, 2f);
            float _distanceSqr = (pointOfInterest - _circleCenter).sqrMagnitude;

            if (_distanceSqr >= _lesserRadiusSqr && _distanceSqr <= _greaterRadiusSqr) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Checks to see whether a given point is within a specified distance from a bezier curve. In the XZ-plane. Outputs the closes t-value (to the given point) on the curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="distanceThreshold"></param>
        /// <param name="pointOfInterest"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsCloseToCurveXZ(Bezier3 curve, float distanceThreshold, Vector3 pointOfInterest, out float t) {
            //constrain to XZ plane
            curve = BezierXZ(curve);
            pointOfInterest.y = 0f;

            //initialize output t
            t = 0.5f;

            float _radiusSqr = distanceThreshold * distanceThreshold;
            float _distanceSqr = curve.DistanceSqr(pointOfInterest, out t);

            if (_distanceSqr <= _radiusSqr) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Checks to see whether a given point is within a specified distance from a line segment. In the XZ-plane. Outputs the closest t-value (to the given point) on the curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="distanceThreshold"></param>
        /// <param name="pointOfInterest"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsCloseToSegmentXZ(Segment3 lineSegment, float distanceThreshold, Vector3 pointOfInterest, out float t) {
            //constrain to XZ plane
            lineSegment = SegmentXZ(lineSegment);
            pointOfInterest.y = 0f;

            //initialize output t
            t = 0.5f;

            float _radiusSqr = distanceThreshold * distanceThreshold;
            float _distanceSqr = lineSegment.DistanceSqr(pointOfInterest, out t);

            if (_distanceSqr <= _radiusSqr) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Checks to see whether a given point is within a specified distance from a circle outline. In the XZ-plane. Outputs the closest t-value (to the given point) on the curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="distanceThreshold"></param>
        /// <param name="pointOfInterest"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsCloseToCircle3XZ(Circle3XZ circle, float distanceThreshold, Vector3 pointOfInterest, out float t) {
            Vector3 _circleCenter = circle.center;
            float _circleRadius = circle.radius;

            //constrain to XZ plane
            _circleCenter.y = 0f;
            pointOfInterest.y = 0f;

            //initialize output t
            t = 0.5f;

            if (distanceThreshold == 0f) {
                return pointOfInterest == _circleCenter ? true : false;
            } else if (distanceThreshold < 0f) {
                distanceThreshold = Mathf.Abs(distanceThreshold);
            }

            float _lesserRadiusSqr = Mathf.Pow(_circleRadius - distanceThreshold, 2f);
            float _greaterRadiusSqr = Mathf.Pow(_circleRadius + distanceThreshold, 2f);
            //float _distanceSqr = (pointOfInterest - _circleCenter).sqrMagnitude;
            float _distanceSqr = circle.DistanceSqr(pointOfInterest, out t);

            if (_distanceSqr >= _lesserRadiusSqr && _distanceSqr <= _greaterRadiusSqr) {
                return true;
            } else {
                return false;
            }
        }


    }


    public struct Circle2 {
        public Vector2 center;

        public float radius;

        public Circle2 unitCircle {
            get {
                return new Circle2(Vector2.zero, 1f);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t">Generally from 0 to 1. [0, 1]</param>
        /// <returns></returns>
        public Vector2 Position(float t) {
            Vector2 _result = Vector2.zero;

            _result.x = center.x + radius * Mathf.Cos(2 * Mathf.PI * t);
            _result.y = center.y + radius * Mathf.Sin(2 * Mathf.PI * t);

            return _result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="theta">Angle in radians. [0, 2pi]</param>
        /// <returns></returns>
        public Vector2 PositionFromAngle(float theta) {
            Vector2 _result = Vector2.zero;

            _result.x = center.x + radius * Mathf.Cos(theta);
            _result.y = center.y + radius * Mathf.Sin(theta);

            return _result;
        }

        /// <summary>
        /// Returns XZ position on a circle in the XZ-plane.
        /// </summary>
        /// <param name="theta">Angle in radians. [0, 2pi]</param>
        /// <returns></returns>
        public static Vector3 Position3FromAngleXZ(Vector3 center, float radius, float theta) {
            Vector3 _result = Vector3.zero;

            _result.x = center.x + radius * Mathf.Cos(theta);
            _result.z = center.z + radius * Mathf.Sin(theta);

            return _result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="theta">Angle in degrees. [0, 360]</param>
        /// <returns></returns>
        public Vector2 PositionFromAngleDegrees(float theta) {
            Vector2 _result = Vector2.zero;

            theta *= Mathf.Deg2Rad;

            _result.x = center.x + radius * Mathf.Cos(theta);
            _result.y = center.y + radius * Mathf.Sin(theta);

            return _result;
        }


        //constructor
        public Circle2(Vector2 center, float radius) {
            this.center = center;
            this.radius = radius;
        }
    }

    /// <summary>
    /// A circle that lies parallel to the XZ plane, with orientation towards start angle, and at a height of center.y.
    /// </summary>
    public struct Circle3XZ {
        public Vector3 center;

        public float radius;

        /// <summary>
        /// The starting angle of the circle, in radians. Aka where the CCW rotation starts from. Conventionally zero radians.
        /// </summary>
        public float angleStart;

        public float circumference {
            get {
                return 2f * Mathf.PI * radius;
            }
        }

        public float diameter {
            get {
                return 2f * radius;
            }
        }

        //170611
        //This is equivalent to circumference...
        //public float speedXZ
        //{
        //    get
        //    {
        //        Vector3 _tangent = Tangent(0f);
        //        _tangent.y = 0f;

        //        return _tangent.magnitude;
        //    }
        //}

        /// <summary>
        /// Returns a point on the circle outline.
        /// </summary>
        /// <param name="t">Generally from 0 to 1: [0, 1].</param>
        /// <returns></returns>
        public Vector3 Position(float t) {
            if (radius == 0f) {
                return center;
            }

            Vector3 _position = center + new Vector3(radius, 0f, 0f);

            _position.x = center.x + radius * Mathf.Cos(angleStart + 2f * Mathf.PI * t);
            _position.z = center.z + radius * Mathf.Sin(angleStart + 2f * Mathf.PI * t);

            return _position;
        }

        /// <summary>
        /// Returns a tangent point on the circle outline.
        /// </summary>
        /// <param name="t">Generally from 0 to 1: [0, 1].</param>
        /// <returns></returns>
        public Vector3 Tangent(float t) {
            if (radius == 0f) {
                return Vector3.zero;
            }

            Vector3 _tangent = Vector3.zero;

            _tangent.x = -2f * Mathf.PI * radius * Mathf.Sin(angleStart + 2f * Mathf.PI * t);
            _tangent.z = 2f * Mathf.PI * radius * Mathf.Cos(angleStart + 2f * Mathf.PI * t);

            return _tangent;
        }

        //use for non-fence mode
        public float DeltaT(float distance) {
            if (distance == 0f) {
                return 0f;
            }
            if (radius == 0f) {
                //return float.NaN;
                return 1f;
            }

            float _deltaT = 0.10f;

            _deltaT = distance / (2f * Mathf.PI * radius);

            return _deltaT;
        }

        //use for non-fence mode
        /// <summary>
        /// Returns the angle, in radians, of the angle subtended by an arc of given length.
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public float DeltaAngle(float distance) {
            float _deltaAngle = Mathf.PI;

            _deltaAngle = DeltaT(distance) * 2f * Mathf.PI;

            return _deltaAngle;
        }

        //use for fence mode
        /// <summary>
        /// Returns the angle, in radians, of the angle inscribed by a chord of given length.
        /// </summary>
        /// <param name="chordLength"></param>
        /// <returns></returns>
        public float ChordAngle(float chordLength) {
            if (chordLength == 0f) {
                return 0f;
            }
            if (radius == 0f) {
                return 2f * Mathf.PI;
            }
            if (chordLength > this.diameter) {
                return 2f * Mathf.PI;
            }

            float _alpha = 2f * Mathf.Asin(chordLength / (2f * radius));

            return _alpha;
        }

        //use for fence mode?
        public float ChordDeltaT(float chordLength) {
            if (chordLength == 0f) {
                return 0f;
            }
            if (radius == 0f) {
                return 1f;
            }
            if (chordLength > this.diameter) {
                return 1f;
            }

            //float _chordDeltaT = ChordAngle(chordLength) / this.circumference;
            float _chordDeltaT = ChordAngle(chordLength) / (2f * Mathf.PI);

            return _chordDeltaT;
        }

        //use for non-fence mode
        public float PerfectRadiusByArcs(float arcLength) {
            if (arcLength == 0f) {
                return this.radius;
            }

            float _numArcsRaw = Mathf.Abs(this.circumference / arcLength);
            float _numArcsPerfect = Mathf.Round(_numArcsRaw);
            float _circumferencePerfect = arcLength * _numArcsPerfect;

            float _radiusPerfect = _circumferencePerfect / (2f * Mathf.PI);

            if (_radiusPerfect > 0f) {
                return _radiusPerfect;
            } else {
                return this.radius;
            }
        }

        //use for fence mode
        public float PerfectRadiusByChords(float chordLength) {
            if (chordLength == 0f) {
                return this.radius;
            }

            float _chordAngleRaw = this.ChordAngle(chordLength);
            float _numChordsRaw = Mathf.Abs(2f * Mathf.PI / _chordAngleRaw);
            float _numChordsPerfect = Mathf.Round(_numChordsRaw);

            if (_numChordsPerfect <= 0f) {
                return this.radius;
            }

            float _chordAnglePerfect = 2f * Mathf.PI / _numChordsPerfect;

            float _radiusPerfect = chordLength / (2f * Mathf.Sin(_chordAnglePerfect / 2f));

            if (_radiusPerfect > 0f) {
                return _radiusPerfect;
            } else {
                return this.radius;
            }
        }

        public float DistanceSqr(Vector3 position, out float u) {
            float _distanceSqr = 0f;
            u = 0f;

            if (position == this.center) {
                u = 0f;
                return 0f;
            }

            Vector3 _pointVector = position - this.center;
            _pointVector.y = 0f;

            //float _angle = MathPLT.AngleSigned(_pointVector, Vector3.right, Vector3.up) + Mathf.PI;
            //u = (_angle - this.angleStart) / this.circumference;
            float _angle = this.AngleFromStartXZ(position);
            u = _angle / (2f * Mathf.PI);

            _distanceSqr = (_pointVector).sqrMagnitude;

            return _distanceSqr;
        }

        /// <summary>
        /// Returns the angle, in radians, between a line pointing from the center to a given point, and the line pointing from the center to the start point (t = 0f).
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public float AngleFromStartXZ(Vector3 position) {
            float _angleFromStart = 0f;

            Vector3 _pointVector = position - this.center;
            _pointVector.y = 0f;
            _pointVector.Normalize();
            Vector3 _zeroVector = this.Position(0f) - this.center;
            _zeroVector.y = 0f;
            _zeroVector.Normalize();

            _angleFromStart = MathPLT.AngleSigned(_pointVector, _zeroVector, Vector3.up);
            if (_angleFromStart < 0f) {
                _angleFromStart += 2f * Mathf.PI;
            }

            return _angleFromStart;
        }

        /// <summary>
        /// Returns the angle, in radians, between two points on the circle.
        /// </summary>
        /// <param name="t1">Generally [0, 1].</param>
        /// <param name="t2">Generally [0, 1].</param>
        /// <returns></returns>
        public float AngleBetween(float t1, float t2) {
            float _angleBetween = 0f;

            if (t2 == t1) {
                return 0f;
            }

            //Vector3 _p2 = this.Position(t2) - this.center;
            //_p2.y = 0f;
            //_p2.Normalize();
            //Vector3 _p1 = this.Position(t1) - this.center;
            //_p1.y = 0f;
            //_p1.Normalize();

            //_angleBetween = MathPLT.AngleSigned(_p2, _p1, Vector3.up);
            //if (_angleBetween < 0f)
            //{
            //    _angleBetween += 2f * Mathf.PI;
            //}

            //easier method
            _angleBetween = (t2 - t1) * 2f * Mathf.PI;

            return _angleBetween;
        }

        /// <summary>
        /// Returns the distance traveled, in units of distance, between two points on the circle.
        /// </summary>
        /// <param name="t1">Generally [0, 1].</param>
        /// <param name="t2">Generally [0, 1].</param>
        /// <returns></returns>
        public float ArclengthBetween(float t1, float t2) {
            if (t2 == t1) {
                return 0f;
            }

            return AngleBetween(t1, t2) * radius;
        }

        public Circle3XZ(Vector3 center, float radius) {
            this.center = center;
            this.radius = radius;
            this.angleStart = 0f;
        }

        /// <param name="angleStart">Angle in radians.</param>
        public Circle3XZ(Vector3 center, float radius, float angleStart) {
            this.center = center;
            this.radius = radius;
            this.angleStart = angleStart;
        }

        public Circle3XZ(Vector3 center, Vector3 pointOnCircle) {
            if (pointOnCircle == center) {
                this.center = center;
                this.radius = 0f;
                this.angleStart = 0f;

                return;
            }

            this.center = center;
            center.y = 0f;
            pointOnCircle.y = 0f;
            Vector3 _radiusVector = pointOnCircle - center;
            this.radius = _radiusVector.magnitude;
            //this.angleStart = MathPLT.AngleSigned(_radiusVector, Vector3.right, Vector3.up) + Mathf.PI;
            this.angleStart = MathPLT.AngleSigned(_radiusVector, Vector3.right, Vector3.up);
        }
    }
}