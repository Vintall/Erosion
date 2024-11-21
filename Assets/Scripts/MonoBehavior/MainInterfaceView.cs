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
        [SerializeField] private GaussianBlurVo gaussianBlurVo;
        [SerializeField] private EHydraulicErosionType hydraulicErosionType;
        [SerializeField] private bool applyBlurAutomaticly;
        [SerializeField] private int terrainResolution;
        [SerializeField] private float terrainSize;
        [SerializeField, Range(50, 500)] private int previewTextureSize;
        [SerializeField] private float terrainOffset;
        
        public HydraulicErosionIterationVo HydraulicErosionIterationVo => hydraulicErosionIterationVo;
        public GaussianBlurVo GaussianBlurVo => gaussianBlurVo;
        public EHydraulicErosionType HydraulicErosionType => hydraulicErosionType;
        public bool ApplyBlurAutomaticly => applyBlurAutomaticly;
        public int TerrainResolution => terrainResolution;
        public float TerrainSize => terrainSize;
        public float TerrainOffset => terrainOffset;

        public int PreviewTextureSize => previewTextureSize;

        public void SetEditorInstance(MainInterfaceViewEditor editorInstance)
        {
            _editorInstance = editorInstance;
        }

        public void SetupProgressBar(
            bool isEnabled, 
            int iteration, 
            int iterationsCount)
        {
        }

        public void UpdatePreviewTexture(Texture2D newTexture)
        {
            if(_editorInstance == null)
                return;

            _editorInstance.terrainChunkPreview = new Texture2D(newTexture.width, newTexture.height);
            _editorInstance.terrainChunkPreview.SetPixels(newTexture.GetPixels());
            _editorInstance.terrainChunkPreview.Apply();
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
            terrainChunkPreview = new Texture2D(_target.PreviewTextureSize, _target.PreviewTextureSize);
            //terrainChunkPreview.wrapMode = TextureWrapMode.Clamp;
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
            var textureRect = new Rect(lastRect.x, lastRect.y + lastRect.height, _target.PreviewTextureSize, _target.PreviewTextureSize);
            
            GUILayout.Space(textureRect.height);
            GUI.DrawTextureWithTexCoords(textureRect, terrainChunkPreview, new Rect(0, 0, 1, 1));
            
            if(GUILayout.Button($"Sample to .png"))
                _target.OnSampleToPNGPress?.Invoke();
            
            if(GUILayout.Button($"Open PNG folder"))
                _target.OnOpenPNGFolderPress?.Invoke();
        }
    }
}
