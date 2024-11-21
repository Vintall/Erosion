using System;
using UnityEngine;

namespace Models
{
    [Serializable]
    public class GaussianBlurVo
    {
        [SerializeField] private float centerModifier;
        [SerializeField] private float adjacentModifier;
        [SerializeField] private float diagonalModifier;

        public float CenterModifier => centerModifier;
        public float AdjacentModifier => adjacentModifier;
        public float DiagonalModifier => diagonalModifier;

        private float AccumulativeModifier => centerModifier + 4 * (adjacentModifier + diagonalModifier);
    }
}