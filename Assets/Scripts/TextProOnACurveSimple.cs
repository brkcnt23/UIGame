using UnityEngine;
using TMPro;

namespace ntw.CurvedTextMeshPro
{
    [ExecuteInEditMode]
    public class TextProOnACurveSimple : TextProOnACurve
    {
        [SerializeField]
        [Tooltip("The strength of the curve along the Z-axis")]
        private float curveStrength = 10.0f;  // Adjust this to control the curvature strength

        [SerializeField]
        [Tooltip("The height of the curve")]
        private float curveHeight = 5.0f;

        /// <summary>
        /// Check if the parameters have changed (for performance optimization)
        /// </summary>
        /// <returns></returns>
        protected override bool ParametersHaveChanged()
        {
            return true;
        }

        /// <summary>
        /// Computes the transformation matrix to bend the text along a curve around the Z-axis.
        /// </summary>
        /// <param name="charMidBaselinePos">Position of the central point of the character</param>
        /// <param name="zeroToOnePos">Horizontal position of the character relative to the bounds of the box, in a range [0, 1]</param>
        /// <param name="textInfo">Information on the text that we are showing</param>
        /// <param name="charIdx">Index of the character we have to compute the transformation for</param>
        /// <returns>Transformation matrix to be applied to all vertices of the text</returns>
        protected override Matrix4x4 ComputeTransformationMatrix(Vector3 charMidBaselinePos, float zeroToOnePos, TMP_TextInfo textInfo, int charIdx)
        {
            // Compute the angle for each character based on the position
            float angle = (zeroToOnePos - 0.5f) * curveStrength;  // Center the curve and apply strength

            // Calculate the new position along X and Y, keeping the Z-axis as the center of curvature
            float newX = charMidBaselinePos.x;  // Keep X position fixed (for horizontal alignment)
            float newY = curveHeight * Mathf.Sin(angle);  // Apply curve along Y
            float newZ = curveHeight * Mathf.Cos(angle);  // Apply curve along Z (this gives depth)

            // Create a transformation matrix that rotates the character around the Z-axis and applies the curved translation
            Matrix4x4 curveMatrix = Matrix4x4.TRS(new Vector3(newX, newY, newZ), Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg), Vector3.one);

            return curveMatrix;
        }
    }
}
