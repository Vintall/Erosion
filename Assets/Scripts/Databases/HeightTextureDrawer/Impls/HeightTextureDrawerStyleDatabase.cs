using System;
using UnityEngine;

namespace Databases.HeightTextureDrawer.Impls
{
    [CreateAssetMenu(menuName = "Databases/HeightTextureDrawerStyleDatabase", fileName = "HeightTextureDrawerStyleDatabase")]
    public class HeightTextureDrawerStyleDatabase : ScriptableObject, IHeightTextureDrawerStyleDatabase
    {
        [SerializeField] private Color lowerColor;
        [SerializeField] private Color higherColor;

        public Color LowerColor => lowerColor;
        public Color HigherColor => higherColor;
    }
}