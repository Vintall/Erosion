using UnityEngine;

namespace Databases.HeightTextureDrawer
{
    public interface IHeightTextureDrawerStyleDatabase
    {
        Color LowerColor { get; }
        Color HigherColor { get; }
    }
}