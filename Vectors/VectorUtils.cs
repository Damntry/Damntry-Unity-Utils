using Damntry.Utils.Logging;
using UnityEngine;

namespace Damntry.UtilsUnity.Vectors {

    public enum SpreadAxis {
        Horizontal,
        Vertical,
        Both
    }

    public static class VectorUtils {

        public static Vector3 GetRandomSpreadOffset(SpreadAxis spreadAxis, 
                Vector3 direction, Vector3 initialVelocity, float spread) {

            Quaternion lookRotation = Quaternion.LookRotation(direction);

            return GetRandomXY_Offset(spreadAxis, lookRotation, initialVelocity, spread);
        }

        public static Vector3 GetRandomXY_Offset(SpreadAxis spreadAxis,
                Quaternion direction, Vector3 initialVelocity, float spread) {

            // Apply randomness to velocity's X and Y
            Vector2 rand = Random.insideUnitCircle * spread;
            Vector3 randomizedVelocity = new (
                spreadAxis == SpreadAxis.Horizontal || spreadAxis == SpreadAxis.Both ?
                    initialVelocity.x + rand.x :
                    initialVelocity.x,
                spreadAxis == SpreadAxis.Vertical || spreadAxis == SpreadAxis.Both ?
                    initialVelocity.y + rand.y :
                    initialVelocity.y,
                initialVelocity.z
            );

            // Transform to world space based on the direction's orientation
            return direction * randomizedVelocity;
        }


        public static Vector3 GetRandomSpreadDirection(this Camera camera, 
                SpreadAxis spreadAxis, Vector3 direction, float spreadAngle) {

            if (!camera) {
                TimeLogger.Logger.LogError($"The camera object is null (Spread angle {spreadAngle}).", LogCategories.Other);
                return Vector3.zero;
            }

            Transform cameraTransform = camera.transform;

            if (!cameraTransform) {
                TimeLogger.Logger.LogError($"The camera transform is null (Spread angle {spreadAngle}).", LogCategories.Other);
                return Vector3.zero;
            }

            Quaternion spread = GetRandomSpreadDirection(spreadAxis,
                cameraTransform.up, cameraTransform.right, spreadAngle);

            return spread * direction;
        }

        public static Quaternion GetRandomSpreadDirection(SpreadAxis spreadAxis,
                Vector3 relativeUp, Vector3 relativeRight, float spreadAngle) {

            Vector2 rand = Random.insideUnitCircle * spreadAngle;

            Quaternion relativeSpread = Quaternion.identity;
            if (spreadAxis == SpreadAxis.Horizontal || spreadAxis == SpreadAxis.Both) {
                relativeSpread *= Quaternion.AngleAxis(rand.x, relativeRight);
            }
            if (spreadAxis == SpreadAxis.Vertical || spreadAxis == SpreadAxis.Both) {
                relativeSpread *= Quaternion.AngleAxis(rand.y, relativeUp);
            }

            return relativeSpread;
        }

        /// <summary>
        /// Offsets the camera position in world space by a random spread factor.
        /// </summary>
        public static Vector3 GetRandomWorldPosOffset(this Camera camera, 
                SpreadAxis spreadAxis, Vector3 position, float spread) {

            if (!camera) {
                TimeLogger.Logger.LogError($"The camera object is null (Spread {spread}).", LogCategories.Other);
                return Vector3.zero;
            }
            if (!camera.transform) {
                TimeLogger.Logger.LogError($"The transform object is null.", LogCategories.Other);
                return Vector3.zero;
            }
            if (spread == 0f) {
                return position;
            }

            Vector2 randOffset = Random.insideUnitCircle * spread;

            if (spreadAxis == SpreadAxis.Horizontal) {
                randOffset.y = 0f;
            }
            if (spreadAxis == SpreadAxis.Vertical) {
                randOffset.x = 0f;
            }

            return position + OffsetTransformPos(camera.transform, (Vector3)randOffset);
        }


        public static Vector3 OffsetTransformPos(Transform transform, Vector3 offset) {
            if (!transform) {
                TimeLogger.Logger.LogError($"The transform object is null.", LogCategories.Other);
                return Vector3.zero;
            }

            return (transform.right * offset.x) + (transform.up * offset.y) + (transform.forward * offset.z);
        }

    }
}
