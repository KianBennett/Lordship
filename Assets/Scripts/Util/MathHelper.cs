using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MathHelper {

    // Returns Vector3 where each component is random value between min and max
    public static Vector3 RandomVector3(float min, float max) {
        return new Vector3(Random.Range(min, max), Random.Range(min, max), Random.Range(min, max));
    }

    // Get average point of array of vectors
    public static Vector3 AverageVector3(params Vector3[] vectors) {
        Vector3 result = Vector3.zero;

        foreach(Vector3 v in vectors) {
            result += v;
        }
        result *= 1f / vectors.Length;
        return result;
    }

    // Get cumulative length of array of vectors
    public static float TotalVectorLengths(params Vector3[] vectors) {
        float length = 0;
        for(int i = 0; i < vectors.Length; i++) {
            if (i == 0) continue;
            length += Vector3.Distance(vectors[i], vectors[i - 1]);
        }
        return length;
    }
}