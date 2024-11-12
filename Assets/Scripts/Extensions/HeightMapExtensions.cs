using System;
using UnityEngine;

namespace Extensions
{
    public static class HeightMapExtensions
    {
        public static float[] ConvertToConsecutiveArray(this float[,] target)
        {
            var axisLength = (int)Math.Sqrt(target.Length);
            var result = new float[target.Length];

            for (var i = 0; i < axisLength; ++i)
            {
                var axisFill = i * axisLength;
                
                for (var j = 0; j < axisLength; ++j)
                    result[axisFill + j] = target[i, j];
            }

            return result;
        }
        
        public static float[] ConvertToConsecutiveArray(this float[][] target)
        {
            var axisLength = (int)Math.Sqrt(target.Length);
            var result = new float[target.Length];

            for (var i = 0; i < axisLength; ++i)
            {
                var axisFill = i * axisLength;
                
                for (var j = 0; j < axisLength; ++j)
                    result[axisFill + j] = target[i][j];
            }

            return result;
        }
        
        public static Vector3[] ConvertToConsecutiveArray(this Vector3[,] target, int resolution)
        {
            var result = new Vector3[resolution * resolution];
            
            Buffer.BlockCopy(target, 0, result, 0, sizeof(float) * 3 * resolution * resolution);
            
            return result;
        }
        
        public static Vector3[] ConvertToConsecutiveArray(this Vector3[][] target, int resolution)
        {
            var result = new Vector3[resolution * resolution];
            
            Buffer.BlockCopy(target, 0, result, 0, sizeof(float) * 3 * resolution * resolution);
            
            return result;
        }
        
        public static float[,] ConvertTo2DArray(this float[] target, int resolution)
        {
            var sideResolution = (int)Math.Sqrt(resolution);
            var result = new float[sideResolution, sideResolution];
            
            Buffer.BlockCopy(target, 0, result, 0, sizeof(float) * resolution);
            
            return result;
        }
        
        public static Vector3[,] ConvertTo2DArray(this Vector3[] target, int resolution)
        {
            var sideResolution = (int)Math.Sqrt(resolution);
            var result = new Vector3[sideResolution, sideResolution];
            
            Buffer.BlockCopy(target, 0, result, 0, sizeof(float) * 3 * resolution);
            
            return result;
        }
        
        public static float[][] ConvertToMatrixArray(this float[] target, int resolution)
        {
            var sideResolution = (int)Math.Sqrt(resolution);
            var result = new float[sideResolution][];
            
            for (var i = 0; i < sideResolution; ++i)
                result[i] = new float[sideResolution];
            
            Buffer.BlockCopy(target, 0, result, 0, sizeof(float) * resolution);
            
            return result;
        }
        
        public static Vector3[][] ConvertToMatrixArray(this Vector3[] target, int resolution)
        {
            var sideResolution = (int)Math.Sqrt(resolution);
            var result = new Vector3[sideResolution][];

            for (var i = 0; i < sideResolution; ++i)
                result[i] = new Vector3[sideResolution];
            
            Buffer.BlockCopy(target, 0, result, 0, sizeof(float) * 3 * resolution);
            
            return result;
        }
    }
}