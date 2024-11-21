using System;
using Databases.CommonShadersDatabase;
using Databases.HeightTextureDrawer;
using UnityEngine;
using UnityEngine.Windows;

namespace Services.HeightTextureDrawer.Impls
{
    public class HeightTextureDrawer : IHeightTextureDrawer
    {
        private readonly IHeightTextureDrawerStyleDatabase _heightTextureDrawerStyleDatabase;
        private string _generationPath = "C:\\Users\\Vintall\\Desktop\\Maps\\";
        
        public HeightTextureDrawer(
            IHeightTextureDrawerStyleDatabase heightTextureDrawerStyleDatabase,
            ICommonShadersDatabase commonShadersDatabase)
        {
            _heightTextureDrawerStyleDatabase = heightTextureDrawerStyleDatabase;
        }

        public void SetPath(string path)
        {
            _generationPath = path;
        }
        
        public void GenerateTexture(Vector3[][] vertices, int resolution)
        {
            var heightMap = new float[resolution][];

            for (var z = 0; z < resolution; ++z)
            {
                heightMap[z] = new float[resolution];
                
                for (var x = 0; x < resolution; ++x)
                    heightMap[z][x] = vertices[z][x].y;
            }

            GenerateTexture(heightMap, resolution);
        }
        
        public void GenerateTexture(float[][] heightMap, int resolution)
        {
            var texture2D = new Texture2D(resolution, resolution);

            NormalizeHeightMap(ref heightMap, resolution);
            
            for (var z = 0; z < resolution; ++z)
            for (var x = 0; x < resolution; ++x)
            {
                var finalColor = Color.Lerp(_heightTextureDrawerStyleDatabase.LowerColor, _heightTextureDrawerStyleDatabase.HigherColor, heightMap[z][x]);
                texture2D.SetPixel(x, z, finalColor);
            }
            
            Directory.CreateDirectory(_generationPath);
            
            var path = $"{_generationPath}{DateTime.Now.Year}_{DateTime.Now.Month}_{DateTime.Now.Day}_{DateTime.Now.Hour}_{DateTime.Now.Minute}_{DateTime.Now.Second}.png";
            File.WriteAllBytes(path, texture2D.EncodeToPNG());
        }

        public Texture2D GetTexture(float[][] heightMap, int resolution)
        {
            var texture2D = new Texture2D(resolution, resolution);

            NormalizeHeightMap(ref heightMap, resolution);
            
            for (var z = 0; z < resolution; ++z)
            for (var x = 0; x < resolution; ++x)
            {
                var finalColor = Color.Lerp(_heightTextureDrawerStyleDatabase.LowerColor, _heightTextureDrawerStyleDatabase.HigherColor, heightMap[z][x]);
                texture2D.SetPixel(x, z, finalColor);
            }

            return texture2D;
        }

        public Texture2D GetTexture(Vector3[][] vertices, int resolution)
        {
            var heightMap = new float[resolution][];

            for (var z = 0; z < resolution; ++z)
            {
                heightMap[z] = new float[resolution];
                
                for (var x = 0; x < resolution; ++x)
                    heightMap[z][x] = vertices[z][x].y;
            }

            return GetTexture(heightMap, resolution);
        }

        private void NormalizeHeightMap(ref float[][] heightMap, int resolution)
        {
            var minHeight = heightMap[0][0];
            var maxHeight = heightMap[0][0];
            
            for (var z = 0; z < resolution; ++z)
            for (var x = 1; x < resolution; ++x)
            {
                if (heightMap[z][x] < minHeight)
                    minHeight = heightMap[z][x];

                if (heightMap[z][x] > maxHeight)
                    maxHeight = heightMap[z][x];
            }
            
            var difference = maxHeight - minHeight;
            
            for (var z = 0; z < resolution; ++z)
            for (var x = 0; x < resolution; ++x)
                heightMap[z][x] = (heightMap[z][x] - minHeight) / difference;
        }
    }
}