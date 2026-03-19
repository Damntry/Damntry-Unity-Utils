using UnityEngine;

namespace Damntry.UtilsUnity.Vectors {
    public class Vector3Curves {

        private readonly AnimationCurve m_CurveX;
        private readonly AnimationCurve m_CurveY;
        private readonly AnimationCurve m_CurveZ;

        public bool HasGeneratedCurves { get; private set; }


        public AnimationCurve CurveX { get { return m_CurveX; } }

        public AnimationCurve CurveY { get { return m_CurveY; } }

        public AnimationCurve CurveZ { get { return m_CurveZ; } }


        public Vector3Curves() {
            m_CurveX = new();
            m_CurveY = new();
            m_CurveZ = new();
        }
        public Vector3Curves(Vector3 startPoint, Vector3 endPoint) {
            m_CurveX = new();
            m_CurveY = new();
            m_CurveZ = new();

            SetLinearVector3(startPoint, endPoint);
        }


        public void SetValue(Vector3 value) {
            m_CurveX.SetValue(value.x);
            m_CurveY.SetValue(value.y);
            m_CurveZ.SetValue(value.z);
        }

        public void SetLinearVector3(Vector3 startPoint, Vector3 endPoint) {
            m_CurveX.AddKey(0f, startPoint.x);
            m_CurveX.AddKey(1f, endPoint.x);
            m_CurveX.SetLinear();

            m_CurveY.AddKey(0f, startPoint.y);
            m_CurveY.AddKey(1f, endPoint.y);
            m_CurveY.SetLinear();

            m_CurveZ.AddKey(0f, startPoint.z);
            m_CurveZ.AddKey(1f, endPoint.z);
            m_CurveZ.SetLinear();

            HasGeneratedCurves = true;
        }

        public Vector3 GetValue(float time) {
            return new Vector3(m_CurveX.Evaluate(time), m_CurveY.Evaluate(time), m_CurveZ.Evaluate(time));
        }

        /// <summary>
        /// 	Sets the wrap mode.
        /// </summary>
        /// <param name="inMode">In mode.</param>
        /// <param name="outMode">Out mode.</param>
        public void SetWrapMode(WrapMode inMode, WrapMode outMode) {
            m_CurveX.SetWrapMode(inMode, outMode);
            m_CurveY.SetWrapMode(inMode, outMode);
            m_CurveZ.SetWrapMode(inMode, outMode);
        }

    }

    public class Vector3CurvesDistanceWidth : Vector3Curves {

        public AnimationCurve DistanceWidth { get; set; }


        public Vector3CurvesDistanceWidth(Vector3 startPoint, Vector3 endPoint, float startWidth, 
                float endWidth, float totalProjectileDistance, float maxTracerWidthDistance) :
                    base (startPoint, endPoint) {
            //Reduce max tracer width if the projectile distance is less than the max
            float progressCap = Mathf.Clamp01(totalProjectileDistance / maxTracerWidthDistance);

            DistanceWidth = new();
            DistanceWidth.AddKey(0f, startWidth);
            DistanceWidth.AddKey(1f, endWidth * progressCap);
            DistanceWidth.SetLinear();
        }

        public Vector3 GetValue(float time, out float tracerWidth) {
            tracerWidth = DistanceWidth.Evaluate(time);
            return GetValue(time);
        }

    }

    /// <summary>
	/// 	AnimationCurveExtensions provides extension methods for working with AnimationCurves.
	/// </summary>
	public static class AnimationCurveExtensions {

        /// <summary>
        /// 	Clears the keys from the AnimationCurve.
        /// </summary>
        /// <param name="extends">Extends.</param>
        public static void Clear(this AnimationCurve extends) {
            for (int index = 0; index < extends.length; index++)
                extends.RemoveKey(index);
        }


        /// <summary>
        /// 	Sets all of the keys on the AnimationCurve to the given value.
        /// 	If there are no keys, we add one.
        /// </summary>
        /// <param name="extends">Extends.</param>
        /// <param name="value">Value.</param>
        public static void SetValue(this AnimationCurve extends, float value) {
            extends.Clear();

            extends.AddKey(0.0f, value);
            extends.AddKey(1.0f, value);
        }


        /// <summary>
        /// 	Sets the wrap mode.
        /// </summary>
        /// <param name="extends">Extends.</param>
        /// <param name="inMode">In mode.</param>
        /// <param name="outMode">Out mode.</param>
        public static void SetWrapMode(this AnimationCurve extends, WrapMode inMode, WrapMode outMode) {
            extends.preWrapMode = inMode;
            extends.postWrapMode = outMode;
        }

        /// <summary>
        /// 	Makes the animation curve linear.
        /// </summary>
        /// <param name="extends">Extends.</param>
        public static void SetLinear(this AnimationCurve extends) {
            for (int index = 0; index < extends.length; index++) {
                Vector2 point1;
                Vector2 point2;
                Vector2 deltapoint;

                Keyframe key = extends[index];

                if (index != 0) {
                    point1.x = extends[index - 1].time;
                    point1.y = extends[index - 1].value;
                    point2.x = extends[index].time;
                    point2.y = extends[index].value;

                    deltapoint = point2 - point1;

                    key.inTangent = deltapoint.y / deltapoint.x;
                }

                if (index != extends.length - 1) {
                    point1.x = extends[index].time;
                    point1.y = extends[index].value;
                    point2.x = extends[index + 1].time;
                    point2.y = extends[index + 1].value;

                    deltapoint = point2 - point1;

                    key.outTangent = deltapoint.y / deltapoint.x;
                }

                extends.MoveKey(index, key);
            }
        }

    }
}
