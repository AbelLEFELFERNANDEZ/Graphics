using System;
using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine.Rendering
{
    using Brick = ProbeBrickIndex.Brick;
    using Flags = ProbeReferenceVolume.BrickFlags;
    using PRVolume = ProbeReferenceVolume.Volume;
    using RefTrans = ProbeReferenceVolume.RefVolTransform;

    public static class ProbeVolumePositioning
    {
        internal static Vector3[] m_Axes = new Vector3[6];

        // TODO: Take refvol translation and rotation into account
        public static PRVolume CalculateBrickVolume(ref RefTrans refTrans, Brick brick)
        {
            float scaledSize = Mathf.Pow(3, brick.size);
            Vector3 scaledPos = refTrans.refSpaceToWS.MultiplyPoint(brick.position);

            PRVolume bounds;
            bounds.Corner = scaledPos;
            bounds.X = refTrans.refSpaceToWS.GetColumn(0) * scaledSize;
            bounds.Y = refTrans.refSpaceToWS.GetColumn(1) * scaledSize;
            bounds.Z = refTrans.refSpaceToWS.GetColumn(2) * scaledSize;

            return bounds;
        }

        public static bool OBBIntersect(ref RefTrans refTrans, Brick brick, ref PRVolume volume)
        {
            var transformed = CalculateBrickVolume(ref refTrans, brick);
            return OBBIntersect(ref transformed, ref volume);
        }

        public static bool OBBIntersect(ref PRVolume a, ref PRVolume b)
        {
            m_Axes[0] = a.X.normalized;
            m_Axes[1] = a.Y.normalized;
            m_Axes[2] = a.Z.normalized;
            m_Axes[3] = b.X.normalized;
            m_Axes[4] = b.Y.normalized;
            m_Axes[5] = b.Z.normalized;

            foreach (Vector3 axis in m_Axes)
            {
                Vector2 aProj = ProjectOBB(ref a, axis);
                Vector2 bProj = ProjectOBB(ref b, axis);

                if (aProj.y < bProj.x || bProj.y < aProj.x)
                {
                    return false;
                }
            }

            return true;
        }

        private static Vector2 ProjectOBB(ref PRVolume a, Vector3 axis)
        {
            float min = Vector3.Dot(axis, a.Corner);
            float max = min;

            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        Vector3 vert = a.Corner + a.X * x + a.Y * y + a.Z * z;

                        float proj = Vector3.Dot(axis, vert);

                        if (proj < min)
                        {
                            min = proj;
                        }
                        else if (proj > max)
                        {
                            max = proj;
                        }
                    }
                }
            }

            return new Vector2(min, max);
        }
    }
}
