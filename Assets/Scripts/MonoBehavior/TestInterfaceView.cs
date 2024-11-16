using System;
using Enums;
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
        public Action OnSampleToPNGPress;
        [SerializeField] private HydraulicErosionIterationVo hydraulicErosionIterationVo;
        [SerializeField] private EHydraulicErosionType hydraulicErosionType;
        
        public HydraulicErosionIterationVo HydraulicErosionIterationVo => hydraulicErosionIterationVo;
        public EHydraulicErosionType HydraulicErosionType => hydraulicErosionType;
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
            
            if(GUILayout.Button($"Simulate {_target.HydraulicErosionIterationVo.IterationsCount} iterations"))
                _target.OnSimulateButtonPress?.Invoke();
            
            if(GUILayout.Button($"Sample to .png"))
                _target.OnSampleToPNGPress?.Invoke();
        }
    }
}
