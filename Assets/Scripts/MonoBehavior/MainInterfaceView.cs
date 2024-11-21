using System;
using Enums;
using Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.Serialization;

namespace MonoBehavior
{
    public class MainInterfaceView : MonoBehaviour
    {
        public Action OnSimulateButtonPress;
        public Action OnResetButtonPress;
        public Action OnSampleToPNGPress;
        public Action OnOpenPNGFolderPress;
        public Action OnApplyGaussianBlurPress;
        private MainInterfaceViewEditor _editorInstance;
        [SerializeField] private HydraulicErosionIterationVo hydraulicErosionIterationVo;
        [SerializeField] private EHydraulicErosionType hydraulicErosionType;
        [SerializeField] private bool applyBlurAutomaticly;
        private bool _isProgressBarActive;
        private float _progressBarProgress;
        private string _progressBarText;
        
        public HydraulicErosionIterationVo HydraulicErosionIterationVo => hydraulicErosionIterationVo;
        public EHydraulicErosionType HydraulicErosionType => hydraulicErosionType;
        public bool ApplyBlurAutomaticly => applyBlurAutomaticly;
        public bool IsProgressBarActive => _isProgressBarActive;
        public float ProgressBarProgress => _progressBarProgress;
        public string ProgressBarText => _progressBarText;

        public void SetEditorInstance(MainInterfaceViewEditor editorInstance)
        {
            _editorInstance = editorInstance;
        }

        public void SetupProgressBar(
            bool isEnabled, 
            int iteration, 
            int iterationsCount)
        {
            _isProgressBarActive = isEnabled;
            _progressBarProgress = (float)iteration / iterationsCount;
            _progressBarText = $"Iteration {iteration}/{iterationsCount}";
            RefreshInspector();
        }

        public void RefreshInspector()
        {
            _editorInstance.RefreshInspector();
            SceneView.RepaintAll();
        }

        public void UpdatePreviewTexture(Texture2D newTexture)
        {
            if(_editorInstance == null)
                return;
            
            _editorInstance.terrainChunkPreview = newTexture;
        }
    }
    
    [CustomEditor(typeof(MainInterfaceView))]
    public class MainInterfaceViewEditor : Editor
    {
        private MainInterfaceView _target;
        public Texture2D terrainChunkPreview;
        
        private void OnEnable()
        {
            _target = target as MainInterfaceView;
            _target.SetEditorInstance(this);
            terrainChunkPreview = new Texture2D(200, 200);
            terrainChunkPreview.wrapMode = TextureWrapMode.Clamp;
        }
        
        public override bool RequiresConstantRepaint() => true;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            GUILayout.Space(10);
            GUILayout.Label("Terrain and Simulation");
            
            if(GUILayout.Button($"Reset terrain chunk"))
                _target.OnResetButtonPress?.Invoke();
            
            if(GUILayout.Button($"Simulate {_target.HydraulicErosionIterationVo.IterationsCount} iterations"))
                _target.OnSimulateButtonPress?.Invoke();

            if(GUILayout.Button($"Apply Gaussian Blur"))
                _target.OnApplyGaussianBlurPress?.Invoke();
            
            
            GUILayout.Space(10);
            GUILayout.Label("HeightTextureDrawer");
            
            var lastRect = GUILayoutUtility.GetLastRect();
            var textureRect = new Rect(lastRect.x, lastRect.y + lastRect.height, 200, 200);
            
            GUILayout.Space(textureRect.height);
            GUI.DrawTextureWithTexCoords(textureRect, terrainChunkPreview, new Rect(0, 0, textureRect.height, textureRect.height));
            
            if(GUILayout.Button($"Sample to .png"))
                _target.OnSampleToPNGPress?.Invoke();
            
            if(GUILayout.Button($"Open PNG folder"))
                _target.OnOpenPNGFolderPress?.Invoke();

            if (_target.IsProgressBarActive)
                DrawProgressBar(_target.ProgressBarProgress, _target.ProgressBarText, Color.green);
        }
        
        public void RefreshInspector()
        {
            Repaint();
        }
        
        private void DrawProgressBar(float value, string label, Color barColor)
        {
            // Label and spacing
            //GUILayout.Label(label, GUILayout.Width(70)); // Add a label (optional)
            Rect rect = GUILayoutUtility.GetRect(18, 18, GUILayout.ExpandWidth(true));

            // Draw background bar (grey)
            EditorGUI.DrawRect(rect, Color.gray);

            // Draw filled bar (dynamic color)
            Rect fillRect = new Rect(rect.x, rect.y, rect.width * value, rect.height);
            EditorGUI.DrawRect(fillRect, barColor);

            // Add percentage text on the bar
            EditorGUI.LabelField(rect, $"{label}", new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState() { textColor = Color.white }
            });
        }
    }
}
