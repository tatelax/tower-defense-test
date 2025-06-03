using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Helpers
{
  public static class PathSmoothingUtil
  {
    public static List<Vector3> CatmullRomSpline(Vector3[] points, int subdivisionsPerSegment = 5)
    {
      var smoothPath = new List<Vector3>();
      if (points.Length < 2)
        return points.ToList();

      for (int i = 0; i < points.Length - 1; i++)
      {
        Vector3 p0 = i == 0 ? points[i] : points[i - 1];
        Vector3 p1 = points[i];
        Vector3 p2 = points[i + 1];
        Vector3 p3 = (i + 2 < points.Length) ? points[i + 2] : points[i + 1];

        for (int j = 0; j < subdivisionsPerSegment; j++)
        {
          float t = j / (float)subdivisionsPerSegment;
          Vector3 pos = GetCatmullRomPosition(t, p0, p1, p2, p3);
          smoothPath.Add(pos);
        }
      }
      smoothPath.Add(points[^1]); // add the final point
      return smoothPath;
    }

    private static Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
      // Catmull-Rom spline (centripetal)
      return 0.5f * (
        2f * p1 +
        (p2 - p0) * t +
        (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
        (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
      );
    }
  }
}