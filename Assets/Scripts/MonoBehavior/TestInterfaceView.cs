using System;
using Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace MonoBehavior
{
    public class TestInterfaceView : MonoBehaviour
    {
        public Action OnSimulateButtonPress;
        public Action OnResetButtonPress;
        [SerializeField] private HydraulicErosionIterationVo hydraulicErosionIterationVo;
        [SerializeField] private int iterations;
        
        public HydraulicErosionIterationVo HydraulicErosionIterationVo => hydraulicErosionIterationVo;
        public int Iteration => iterations;
    }

    [CustomEditor(typeof(TestInterfaceView))]
    public class TestInterfaceViewEditor : Editor
    {
        private TestInterfaceView _target;
        
        
        private void OnEnable()
        {
            _target = target as TestInterfaceView;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            if(GUILayout.Button($"Reset terrain chunk"))
                _target.OnResetButtonPress?.Invoke();
            
            if(GUILayout.Button($"Simulate {_target.Iteration} iterations"))
                _target.OnSimulateButtonPress?.Invoke();
        }
    }
}
