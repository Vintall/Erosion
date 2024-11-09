using System;
using Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace MonoBehavior
{
    public class TestInterfaceView : MonoBehaviour
    {
        public Action OnGeneratePress;
        [SerializeField] private HydraulicErosionIterationVo hydraulicErosionIterationVo;

        public HydraulicErosionIterationVo HydraulicErosionIterationVo => hydraulicErosionIterationVo;
        
        public void Generate() => OnGeneratePress?.Invoke();
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
            
            if(GUILayout.Button("Generate"))
                _target.Generate();
        }
    }
}
