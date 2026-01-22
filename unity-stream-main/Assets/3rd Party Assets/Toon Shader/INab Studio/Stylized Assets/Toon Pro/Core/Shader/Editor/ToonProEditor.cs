using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ToonProEditor : ShaderGUI
{
    #region Utilities

    static GUIStyle boxScopeStyle;
    public static GUIStyle BoxScopeStyle
    {
        get
        {
            if (boxScopeStyle == null)
            {
                boxScopeStyle = new GUIStyle(EditorStyles.helpBox);
                var p = boxScopeStyle.padding;
                p.right += 6;
                p.top += 1;
                p.left += 3;
            }
            return boxScopeStyle;
        }
    }

    static GUIStyle toonLabelStyle;
    public static GUIStyle ToonLabelStyle
    {
        get
        {
            if (toonLabelStyle == null)
            {
                toonLabelStyle = new GUIStyle(EditorStyles.whiteLargeLabel);
                var p = toonLabelStyle.fontStyle = FontStyle.Bold;
            }
            return toonLabelStyle;
        }
    }
    #endregion

    #region RampVariables

    private static int s_previewWidth = 64;
    private static int s_width = 128;
    private static string s_texturePath;

    private Texture2D _cachedTexture;
    private Texture2D _cachedTexturePreview;

    private static Gradient s_gradient = new Gradient
    {
        mode = GradientMode.Fixed,
        colorKeys = new GradientColorKey[] { new GradientColorKey(Color.black, 0.5f), new GradientColorKey(Color.white, 1) },
        alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) }
    };

    #endregion

    #region MaterialProperties

    MaterialProperty outlineUse = null;
    MaterialProperty outlineThicnkess = null;
    MaterialProperty outlineAdaptiveThicnkess = null;
    MaterialProperty outlineType = null;
    MaterialProperty outlineColor = null;
    MaterialProperty outlineNoiseIntesity = null;
    MaterialProperty outlineNoiseScale = null;
    MaterialProperty outlineDynamicSpeed = null;
    MaterialProperty outlineDynamicUse = null;
    MaterialProperty outlineTextureStrength = null;

    MaterialProperty albedoMap = null;
    MaterialProperty albedoColor = null;
    MaterialProperty occlusionMap = null;
    MaterialProperty occlusionStrength = null;
    MaterialProperty normalMap = null;
    MaterialProperty normalMapStrength = null;
    MaterialProperty smoothness = null;
    MaterialProperty specularMap = null;
    MaterialProperty indirectLightStrength = null;
    MaterialProperty alphaClipThreshold = null;

    MaterialProperty emissionUse = null;
    MaterialProperty emissionMap = null;
    MaterialProperty emissionColor = null;


    MaterialProperty rampMap = null;
    MaterialProperty lightRampUse = null;
    MaterialProperty stepOffset = null;
    MaterialProperty lightRampOffset = null;
    MaterialProperty rampDiffuseTextureLoaded = null;

    MaterialProperty diffusePosterizeOffset = null;
    MaterialProperty diffusePosterizePower = null;
    MaterialProperty diffusePosterizeSteps = null;
    MaterialProperty shadowColor = null;
    MaterialProperty useShadows = null;


    MaterialProperty useAdditionalLightsDiffuse = null;
    MaterialProperty additionalLightsDiffuseAmount = null;
    MaterialProperty additionalLightsIntesity = null;
    MaterialProperty additionalLightsSmoothnessMultiplier = null;
    MaterialProperty additionalLightsFaloff = null;

    MaterialProperty diffuseWarpNoise = null;
    MaterialProperty diffuseWarpUse = null;
    MaterialProperty warpStrength = null;
    MaterialProperty warpTextureScale = null;
    MaterialProperty warpScrollSpeed = null;
    MaterialProperty warpAngle = null;
    MaterialProperty warpAnimationSpeed = null;
    MaterialProperty diffuseWarpRotation = null;
    MaterialProperty diffuseWarpUseScreenUvs = null;
    MaterialProperty diffuseWarpAffectHalftone = null;
    MaterialProperty diffuseWarpUseAsOverlay = null;
    MaterialProperty diffuseWarpBrightnessOffset = null;
    MaterialProperty diffuseWarpAdaptiveUvs = null;

    MaterialProperty specularUse = null;
    MaterialProperty smoothnessMultiplier = null;
    MaterialProperty mainLightIntesity = null;
    MaterialProperty specularFaloff = null;
    MaterialProperty stepHalftoneTex = null;
    MaterialProperty halftonePatternSize = null;
    MaterialProperty specularPosterizeSteps = null;
    MaterialProperty specularColor = null;
    MaterialProperty specularMaskUse = null;
    MaterialProperty specularMaskScale = null;
    MaterialProperty specularMaskStrength = null;
    MaterialProperty specularMaskTexture = null;
    MaterialProperty specularMaskRotation = null;
    MaterialProperty specularShadowMask = null;
    MaterialProperty useScreenUvsSpecular = null;
    MaterialProperty useAdaptiveSpecular = null;
    MaterialProperty inverseMask = null;

    MaterialProperty specularScrollSpeed = null;
    MaterialProperty specularAnimationSpeed = null;
    MaterialProperty specularScrollAngle = null;


    MaterialProperty environemntReflectionUse = null;
    MaterialProperty environemntStrength = null;

    MaterialProperty rimUse = null;
    MaterialProperty rimPower = null;
    MaterialProperty rimSmoothness = null;
    MaterialProperty rimColor = null;
    MaterialProperty rimThickness = null;
    MaterialProperty rimShadowColor = null;
    MaterialProperty rimSplitColor = null;

    MaterialProperty backlightUse = null;
    MaterialProperty backlightAmount = null;
    MaterialProperty backlightHardness = null;
    MaterialProperty backlightIntensity = null;

    MaterialProperty sideShineAmount = null;
    MaterialProperty sideShineHardness = null;
    MaterialProperty sideShineIntensity = null;

    MaterialProperty overlayMode = null;
    MaterialProperty overlayUvTilling = null;
    MaterialProperty overlayRotation = null;

    MaterialProperty darkerHatchTex = null;
    MaterialProperty lighterHatchTex = null;
    MaterialProperty hatchDarken = null;
    MaterialProperty hatchLighten = null;
    MaterialProperty maxScaleDependingOnCamera = null;
    MaterialProperty hatchColor = null;
    MaterialProperty paper = null;
    MaterialProperty paperTilling = null;

    MaterialProperty halftoneThreshold = null;
    MaterialProperty halftoneEdgeOffset = null;
    MaterialProperty halftoneSmoothness = null;
    MaterialProperty halftoneShapeSmoothness = null;
    MaterialProperty haltoneTex = null;
    MaterialProperty haftoneColor = null;
    MaterialProperty usePureSketh = null;

    MaterialProperty useScreenUvs = null;
    MaterialProperty useAdaptiveUvs = null;
    MaterialProperty uvAnimationSpeed = null;
    MaterialProperty uvScrollAngle = null;
    MaterialProperty uvScrollSpeed = null;
    MaterialProperty hatchConstantScale = null;

    MaterialProperty halftoneToonAffect = null;
    MaterialProperty hatchIndirectAffect = null;


    /*MaterialProperty hairSpecularUse = null;
    MaterialProperty hairSpecularMaskUse = null;
    MaterialProperty hairSpecularMaskScale = null;
    MaterialProperty hairSpecularMaskStrength = null;
    MaterialProperty hairSpecularMaskTexture = null;
    MaterialProperty hairSpecularMaskRotation = null;
    MaterialProperty hairSpecularShadowMask = null;

    MaterialProperty hairSpecularRotation = null;
    MaterialProperty hairSpecularLocalShiftStrength = null;
    MaterialProperty hairSpecularLocalShift = null;

    MaterialProperty hairSpecularPrimaryPower = null;
    MaterialProperty hairSpecularPrimarySmoothness = null;
    MaterialProperty hairSpecularPrimaryIntesity = null;
    MaterialProperty hairSpecularPrimaryColor = null;

    MaterialProperty hairSpecularSecondaryPower = null;
    MaterialProperty hairSpecularSecondarySmoothness = null;
    MaterialProperty hairSpecularSecondaryIntesity = null;
    MaterialProperty hairSpecularSecondaryColor = null;
    MaterialProperty hairSpecularStrandTex = null;
    MaterialProperty hairSpecularLocalShiftTex = null;*/
    #endregion

    #region EditorVariables
    
    MaterialEditor m_MaterialEditor;
    Material m_Material;

    #endregion

    public void FindProperties(MaterialProperty[] props)
    {
        useScreenUvs = FindProperty("_UseScreenUvs", props);
        useAdaptiveUvs = FindProperty("_UseAdaptiveScreenUvs", props);
        hatchConstantScale = FindProperty("_UseHatchingConstantScale", props);
        usePureSketh = FindProperty("_UsePureSketch", props);

        halftoneToonAffect = FindProperty("_HalftoneToonAffect", props);
        hatchIndirectAffect = FindProperty("_IndirectLightAffectOnHatch", props);

        outlineUse = FindProperty("_UseOutline", props);
        outlineThicnkess = FindProperty("_Thicnkess", props);
        outlineAdaptiveThicnkess = FindProperty("_AdaptiveThicnkess", props);
        outlineType = FindProperty("_OutlineType", props);
        outlineColor = FindProperty("_OutlineColor", props);
        outlineNoiseScale = FindProperty("_OutlineNoiseScale", props);
        outlineNoiseIntesity = FindProperty("_OutlineNoiseIntesity", props);
        outlineDynamicSpeed = FindProperty("_OutlineDynamicSpeed", props);
        outlineDynamicUse = FindProperty("_UseDynamicOutline", props);
        outlineTextureStrength = FindProperty("_OutlineTextureStrength", props);

        albedoMap = FindProperty("_MainTex", props);
        albedoColor = FindProperty("_Color", props);
        occlusionMap = FindProperty("_OcclusionMap", props);
        occlusionStrength = FindProperty("_OcclusionStrength", props);
        normalMap = FindProperty("_BumpMap", props);
        normalMapStrength = FindProperty("_NormalMapStrength", props);
        smoothness = FindProperty("_Glossiness", props);
        specularMap = FindProperty("_SpecGlossMap", props);
        specularColor = FindProperty("_SpecColor", props);
        indirectLightStrength = FindProperty("_IndirectLightStrength", props);
        alphaClipThreshold = FindProperty("_Cutoff", props);


        emissionUse = FindProperty("_UseEmission", props);
        emissionMap = FindProperty("_EmissionMap", props);
        emissionColor = FindProperty("_EmissionColor", props);


        rampMap = FindProperty("_LightRampTexture", props);
        lightRampUse = FindProperty("_UseLightRamp", props);
        stepOffset = FindProperty("_StepOffset", props);
        lightRampOffset = FindProperty("_LightRampOffset", props);
        rampDiffuseTextureLoaded = FindProperty("_RampDiffuseTextureLoaded", props);
        diffusePosterizeOffset = FindProperty("_DiffusePosterizeOffset", props);
        diffusePosterizePower = FindProperty("_DiffusePosterizePower", props);
        diffusePosterizeSteps = FindProperty("_DiffusePosterizeSteps", props);
        useShadows = FindProperty("_UseShadows", props);
        shadowColor = FindProperty("_ShadowColor", props);

        diffuseWarpNoise = FindProperty("_DiffuseWarpNoise", props);
        warpStrength = FindProperty("_WarpStrength", props);
        warpTextureScale = FindProperty("_WarpTextureScale", props);
        warpAngle = FindProperty("_UVSrcrollAngleWarp", props);
        warpAnimationSpeed = FindProperty("_UVAnimationSpeedWarp", props);
        warpScrollSpeed = FindProperty("_UVScrollSpeedWarp", props);
        diffuseWarpUse = FindProperty("_UseDiffuseWarp", props);

        specularFaloff= FindProperty("_SpecularFaloff", props);
        stepHalftoneTex = FindProperty("_StepHalftoneTexture", props);
        halftonePatternSize = FindProperty("_HaltonePatternSize", props);
        smoothnessMultiplier = FindProperty("_SmoothnessMultiplier", props);
        mainLightIntesity = FindProperty("_MainLightIntesity", props);

        additionalLightsSmoothnessMultiplier = FindProperty("_AdditionalLightsSmoothnessMultiplier", props);
        additionalLightsIntesity = FindProperty("_AdditionalLightsIntesity", props);

        useAdditionalLightsDiffuse = FindProperty("_UseAdditionalLightsDiffuse", props);
        additionalLightsDiffuseAmount = FindProperty("_AdditionalLightsAmount", props);
        additionalLightsFaloff = FindProperty("_AdditionalLightsFaloff", props);

        specularPosterizeSteps = FindProperty("_SpecularPosterizeSteps", props);
        specularUse = FindProperty("_UseSpecular", props);
        specularMaskUse = FindProperty("_UseSpecularMask", props);
        specularMaskScale = FindProperty("_SpecularMaskScale", props);
        specularMaskStrength = FindProperty("_SpecularMaskStrength", props);
        specularMaskTexture = FindProperty("_SpecularMaskTexture", props);
        specularMaskRotation = FindProperty("_SpecularMaskRotation", props);
        specularShadowMask = FindProperty("_SpecularShadowMask", props);
        useScreenUvsSpecular = FindProperty("_UseScreenUvsSpecular", props);
        useAdaptiveSpecular = FindProperty("_UseAdaptiveUvsSpecular", props);
        inverseMask = FindProperty("_InverseMask", props);

        specularAnimationSpeed = FindProperty("_UVAnimationSpeedSpec", props);
        specularScrollSpeed = FindProperty("_UVScrollSpeedSpec", props);
        specularScrollAngle = FindProperty("_UVSrcrollAngleSpec", props);


        /*hairSpecularUse =           FindProperty("_UseHairSpecular", props);
        hairSpecularMaskUse =       FindProperty("_UseHairSpecularMask", props);
        hairSpecularMaskScale =     FindProperty("_HairSpecularMaskScale", props);
        hairSpecularMaskStrength =  FindProperty("_HairSpecularMaskStrength", props);
        hairSpecularMaskTexture =   FindProperty("_HairSpecularMaskTexture", props);
        hairSpecularMaskRotation =  FindProperty("_HairSpecularMaskRotation", props);
        hairSpecularShadowMask =    FindProperty("_HairSpecularShadowMask", props);

        hairSpecularRotation =  FindProperty("_Rotation", props);
        hairSpecularLocalShiftStrength = FindProperty("_LocalShiftStrength", props);
        hairSpecularLocalShift = FindProperty("_LocalShift", props);

        hairSpecularPrimaryPower =      FindProperty("_PrimaryPower", props);
        hairSpecularPrimarySmoothness = FindProperty("_PrimarySmoothness", props);
        hairSpecularPrimaryIntesity =   FindProperty("_PrimaryIntensity", props);
        hairSpecularPrimaryColor =      FindProperty("_PrimaryColor", props);

        hairSpecularSecondaryPower =        FindProperty("_SecondaryPower", props);
        hairSpecularSecondarySmoothness =   FindProperty("_SecondarySmoothness", props);
        hairSpecularSecondaryIntesity =     FindProperty("_SecondaryIntensity", props);
        hairSpecularSecondaryColor =        FindProperty("_SecondaryColor", props);
        hairSpecularStrandTex = FindProperty("_StrandTexture", props);
        hairSpecularLocalShiftTex = FindProperty("_LocalShiftTexture", props);*/

        environemntReflectionUse = FindProperty("_UseEnvironmentRefletion", props);
        environemntStrength = FindProperty("_Strength", props);

        rimUse = FindProperty("_UseRimLight", props);
        rimPower = FindProperty("_RimPower", props);
        rimSmoothness = FindProperty("_RimSmoothness", props);
        rimColor = FindProperty("_RimColor", props);
        rimThickness = FindProperty("_RimThickness", props);
        rimSplitColor = FindProperty("_RimSplitColor", props);
        rimShadowColor = FindProperty("_RimShadowColor", props);

        backlightAmount = FindProperty("_BacklightAmount", props);
        backlightUse = FindProperty("_UseBacklight", props);
        backlightHardness = FindProperty("_BacklightHardness", props);
        backlightIntensity = FindProperty("_BacklightIntensity", props);

        sideShineAmount = FindProperty("_SideShineAmount", props);
        sideShineHardness = FindProperty("_SideShineHardness", props);
        sideShineIntensity = FindProperty("_SideShineIntensity", props);

        overlayMode = FindProperty("_OverlayMode", props);
        overlayUvTilling = FindProperty("_OverlayUVTilling", props);
        overlayRotation = FindProperty("_OverlayRotation", props);

        darkerHatchTex = FindProperty("_Hatch1", props);
        lighterHatchTex = FindProperty("_Hatch2", props);
        hatchDarken = FindProperty("_Darken", props);
        hatchLighten = FindProperty("_Lighten", props);
        maxScaleDependingOnCamera = FindProperty("_MaxScaleDependingOnCamera", props);
        hatchColor = FindProperty("_HatchingColor", props);
        paper = FindProperty("_PaperTexture", props);
        paperTilling = FindProperty("_PaperTilling", props);


        halftoneThreshold = FindProperty("_HalftoneThreshold", props);
        halftoneEdgeOffset = FindProperty("_HalftoneEdgeOffset", props);
        halftoneSmoothness = FindProperty("_HalftoneSmoothness", props);
        halftoneShapeSmoothness = FindProperty("_ShapeSmoothness", props);
        haltoneTex = FindProperty("_HalftoneTexture", props);
        haftoneColor = FindProperty("_HalftoneColor", props);

        uvAnimationSpeed = FindProperty("_UVAnimationSpeed", props);
        uvScrollAngle = FindProperty("_UVSrcrollAngle", props);
        uvScrollSpeed = FindProperty("_UVScrollSpeed", props);

        diffuseWarpRotation = FindProperty("_WarpTextureRotation", props);
        diffuseWarpUseScreenUvs = FindProperty("_UseScreenUvsWarp", props);
        diffuseWarpAffectHalftone = FindProperty("_DiffuseWarpAffectHalftone", props);
        diffuseWarpUseAsOverlay = FindProperty("_UseDiffuseWarpAsOverlay", props);
        diffuseWarpBrightnessOffset = FindProperty("_DiffuseWarpBrightnessOffset", props);
        diffuseWarpAdaptiveUvs = FindProperty("_UseAdaptiveScreenUvsWarp", props);
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {

        FindProperties(props);
        m_MaterialEditor = materialEditor;
        m_Material = materialEditor.target as Material;
        
        ShaderPropertiesGUI(m_Material, materialEditor);

        //materialEditor.PropertiesDefaultGUI(props);
    }

    public void ShaderPropertiesGUI(Material material, MaterialEditor materialEditor)
    {
        MainEditor();
        DiffuseLightEditor();
        SpecularLightEditor();

        OutlineEditor();
        RimEditor();
        OverlayEditor();

        EditorGUILayout.BeginVertical(BoxScopeStyle);
        EditorGUILayout.Space(2);
        
        GUILayout.Label("Advanced", ToonLabelStyle);
        
        EditorGUILayout.BeginVertical(BoxScopeStyle);
        EditorGUILayout.Space(2);
        
        m_MaterialEditor.RenderQueueField();
        m_MaterialEditor.EnableInstancingField();
        m_MaterialEditor.DoubleSidedGIField();
        
        EditorGUILayout.Space(2);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(2);
        EditorGUILayout.EndVertical();

    }

    #region HelperFunctions

    private void DrawBoxSpace(string header, List<MaterialProperty> props)
    {
        EditorGUILayout.BeginVertical(BoxScopeStyle);
        EditorGUILayout.Space(2);

        GUILayout.Label(header, ToonLabelStyle);

        EditorGUILayout.BeginVertical(BoxScopeStyle);
        EditorGUILayout.Space(2);

        foreach (var prop in props)
        {
            DrawProperty(prop);
        }

        EditorGUILayout.Space(2);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(2);
        EditorGUILayout.EndVertical();
    }

    private void DrawToggleBoxScope(MaterialProperty header,List<MaterialProperty> props)
    {
        EditorGUILayout.BeginVertical(BoxScopeStyle);
        EditorGUILayout.Space(2);

        DrawToggleHeader(header);


        bool isParamPropEnabled = !Mathf.Approximately(header.floatValue, 0f);
        if(isParamPropEnabled)
        {
            EditorGUILayout.BeginVertical(BoxScopeStyle);
            EditorGUILayout.Space(2);

            foreach (var prop in props)
            {
                DrawProperty(prop);
            }

            EditorGUILayout.Space(2);
            EditorGUILayout.EndVertical();
        }


        EditorGUILayout.Space(2);
        EditorGUILayout.EndVertical();
    }

    private void DrawProperty(MaterialProperty prop)
    {
        m_MaterialEditor.ShaderProperty(prop, prop.displayName);
        //m_MaterialEditor.ShaderProperty(prop, StylizedToonStyles.Find(prop.displayName));
    }

    private void DrawToggleHeader(MaterialProperty prop, string name = null)
    {
        if(string.IsNullOrEmpty(name))
        {
            name = prop.displayName.Replace("Use", "");
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(name, ToonLabelStyle);
        m_MaterialEditor.ShaderProperty(prop, string.Empty);

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

    }

    #endregion

    #region EditorFunctions

    private void MainEditor()
    {
        EditorGUILayout.BeginVertical(BoxScopeStyle);
        EditorGUILayout.Space(2);

        GUILayout.Label("Main", ToonLabelStyle);

        EditorGUILayout.BeginVertical(BoxScopeStyle);
        EditorGUILayout.Space(2);

        m_MaterialEditor.TexturePropertySingleLine(new GUIContent("Albedo"), albedoMap, albedoColor);
        DrawProperty(alphaClipThreshold);

        DrawProperty(indirectLightStrength);

        m_MaterialEditor.TexturePropertySingleLine(new GUIContent("Normal Map"), normalMap, normalMapStrength);
        m_MaterialEditor.TexturePropertySingleLine(new GUIContent("Occlusion Map"), occlusionMap, occlusionStrength);

        bool emission = m_MaterialEditor.EmissionEnabledProperty();
        if (emission) emissionUse.floatValue = 1;
        else emissionUse.floatValue = 0;

        using (var disableScope = new EditorGUI.DisabledScope(!emission))
        {
            EditorGUILayout.BeginHorizontal();
            m_MaterialEditor.TexturePropertyWithHDRColor(new GUIContent("Emission Map"), emissionMap, emissionColor, false);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space();

        m_MaterialEditor.TextureScaleOffsetProperty(occlusionMap);


        EditorGUILayout.Space(2);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(2);
        EditorGUILayout.EndVertical();
    }

    private void RimEditor()
    {

        EditorGUILayout.BeginVertical(BoxScopeStyle);
        EditorGUILayout.Space(2);

        DrawToggleHeader(rimUse);

        bool isParamPropEnabled = !Mathf.Approximately(rimUse.floatValue, 0f);
        if (isParamPropEnabled)
        {
            EditorGUILayout.BeginVertical(BoxScopeStyle);
            EditorGUILayout.Space(2);

            
            DrawProperty(rimColor);
            DrawProperty(rimPower);
            DrawProperty(rimSmoothness);
            DrawProperty(rimThickness);

            DrawProperty(rimSplitColor);
            if(rimSplitColor.floatValue == 2)
            {
                DrawProperty(rimShadowColor);
            }


            EditorGUILayout.Space(2);
            EditorGUILayout.EndVertical();

            DrawToggleBoxScope(backlightUse,
           new List<MaterialProperty>
           {
               backlightAmount,backlightHardness,backlightIntensity,sideShineAmount,sideShineHardness,sideShineIntensity
           }
           );
        }

        EditorGUILayout.Space(2);
        EditorGUILayout.EndVertical();
    }

    private void DiffuseLightEditor()
    {
        EditorGUILayout.BeginVertical(BoxScopeStyle);
        EditorGUILayout.Space(2);

        GUILayout.Label("Toon Shading", ToonLabelStyle);

        EditorGUILayout.BeginVertical(BoxScopeStyle);
        EditorGUILayout.Space(2);

        DrawProperty(lightRampUse);

        //bool isParamPropEnabled = !Mathf.Approximately(lightRampUse.floatValue, 0f);

        if(lightRampUse.floatValue == 0)
        {
            //m_Material.DisableKeyword("_USELIGHTRAMPON");
            DrawProperty(stepOffset);
            DrawProperty(shadowColor);

        }
        else if(lightRampUse.floatValue == 1)
        {
            //m_Material.EnableKeyword("_USELIGHTRAMPON");
            DrawProperty(lightRampOffset);

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            DrawProperty(rampMap);
            if (EditorGUI.EndChangeCheck())
            {
                rampDiffuseTextureLoaded.floatValue = 1;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.GradientField(s_gradient);
            if (EditorGUI.EndChangeCheck())
            {
                _cachedTexturePreview = UpdateTex(s_previewWidth);
                rampMap.textureValue = _cachedTexturePreview;
                rampDiffuseTextureLoaded.floatValue = 0;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            using (var disableScope = new EditorGUI.DisabledScope(rampDiffuseTextureLoaded.floatValue == 1))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Set path for texture PNG"))
                {
                    SetTexturePath();
                }


                if (GUILayout.Button("Export as PNG"))
                {
                    _cachedTexture = UpdateTex(s_width);

                    ExportToPNG(_cachedTexture);

                    rampDiffuseTextureLoaded.floatValue = 0;
                    rampMap.textureValue = null;
                }

                EditorGUILayout.EndHorizontal();

                s_texturePath = GUILayout.TextField(s_texturePath);
            }


        }
        else if(lightRampUse.floatValue == 2)
        {
            DrawProperty(shadowColor);
            DrawProperty(diffusePosterizeOffset);
            DrawProperty(diffusePosterizeSteps);
            DrawProperty(diffusePosterizePower);

        }
        DrawProperty(useShadows);


        EditorGUILayout.Space(2);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        DrawToggleBoxScope(useAdditionalLightsDiffuse,
           new List<MaterialProperty>
           {
                additionalLightsDiffuseAmount,additionalLightsFaloff
           }
           );

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical(BoxScopeStyle);
        EditorGUILayout.Space(2);

        DrawToggleHeader(diffuseWarpUse);


        bool isParamPropEnabled = !Mathf.Approximately(diffuseWarpUse.floatValue, 0f);
        if (isParamPropEnabled)
        {
            EditorGUILayout.BeginVertical(BoxScopeStyle);
            EditorGUILayout.Space(2);

            DrawProperty(warpStrength);
            DrawProperty(diffuseWarpNoise);
            DrawProperty(warpTextureScale);

            DrawProperty(diffuseWarpUseScreenUvs);
            if(diffuseWarpUseScreenUvs.floatValue == 1)
            {
                DrawProperty(diffuseWarpAdaptiveUvs);
                DrawProperty(warpAnimationSpeed);
            }

            DrawProperty(diffuseWarpRotation);
            DrawProperty(warpAngle);
            DrawProperty(warpScrollSpeed);

            DrawProperty(diffuseWarpUseAsOverlay);
            if (diffuseWarpUseAsOverlay.floatValue == 1)
            {
                DrawProperty(diffuseWarpBrightnessOffset);
            }
            EditorGUILayout.Space(2);
            EditorGUILayout.EndVertical();
        }


        EditorGUILayout.Space(2);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(2);
        EditorGUILayout.EndVertical();
    }

    private void SpecularLightEditor()
    {
        EditorGUILayout.BeginVertical(BoxScopeStyle);
        EditorGUILayout.Space(2);

        GUILayout.Label("Specular Shading", ToonLabelStyle);

        EditorGUILayout.BeginVertical(BoxScopeStyle);
        EditorGUILayout.Space(2);

        m_MaterialEditor.TexturePropertySingleLine(new GUIContent("Specular Map"), specularMap, specularColor);
        DrawProperty(smoothness);


        EditorGUILayout.Space(2);
        EditorGUILayout.EndVertical();

        List<MaterialProperty> list = new List<MaterialProperty>
            {
                smoothnessMultiplier,mainLightIntesity,additionalLightsSmoothnessMultiplier,additionalLightsIntesity,specularFaloff,specularPosterizeSteps,specularShadowMask
            };

        EditorGUILayout.BeginVertical(BoxScopeStyle);
        EditorGUILayout.Space(2);


        DrawToggleHeader(specularUse);

        bool isParamPropEnabled = !Mathf.Approximately(specularUse.floatValue, 0f);
        if (isParamPropEnabled)
        {
            EditorGUILayout.BeginVertical(BoxScopeStyle);
            EditorGUILayout.Space(2);

            foreach (var prop in list)
            {
                DrawProperty(prop);
            }

            EditorGUILayout.Space(2);
            EditorGUILayout.EndVertical();

            //-----------------------------------
            EditorGUILayout.BeginVertical(BoxScopeStyle);
            EditorGUILayout.Space(2);

            DrawToggleHeader(specularMaskUse);


            bool isParamPropEnableds = !Mathf.Approximately(specularMaskUse.floatValue, 0f);
            if (isParamPropEnableds)
            {
                EditorGUILayout.BeginVertical(BoxScopeStyle);
                EditorGUILayout.Space(2);

                DrawProperty(specularMaskStrength);
                DrawProperty(specularMaskScale);
                DrawProperty(specularMaskTexture);
                DrawProperty(inverseMask);
                
                DrawProperty(stepHalftoneTex);
                if (stepHalftoneTex.floatValue == 1)
                {
                    DrawProperty(halftonePatternSize);
                }
                DrawProperty(specularMaskRotation);

                DrawProperty(specularScrollAngle);
                DrawProperty(specularScrollSpeed);

                DrawProperty(useScreenUvsSpecular);
                if(useScreenUvsSpecular.floatValue == 1)
                {
                    DrawProperty(useAdaptiveSpecular);
                    DrawProperty(specularAnimationSpeed);
                }

                EditorGUILayout.Space(2);
                EditorGUILayout.EndVertical();
            }


            EditorGUILayout.Space(2);
            EditorGUILayout.EndVertical();

        }

        EditorGUILayout.Space(2);
        EditorGUILayout.EndVertical();


        DrawToggleBoxScope(environemntReflectionUse,
           new List<MaterialProperty>
           {
               environemntStrength
           }
           );


        EditorGUILayout.Space(2);
        EditorGUILayout.EndVertical();
    }

    private void OverlayEditor()
    {
        EditorGUILayout.BeginVertical(BoxScopeStyle);
        EditorGUILayout.Space(2);

        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("Overlay", ToonLabelStyle);

        m_MaterialEditor.ShaderProperty(overlayMode, string.Empty);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        float overlayModeValue = overlayMode.floatValue;

        if(overlayModeValue == 0)
        {
            //NONE
            m_Material.EnableKeyword("_OVERLAYMODE_NONE");
            m_Material.DisableKeyword("_OVERLAYMODE_HATCHING");
            m_Material.DisableKeyword("_OVERLAYMODE_HALFTONE");
        }
        else if (overlayModeValue == 1)
        {
            //HALFTONE
            EditorGUILayout.BeginVertical(BoxScopeStyle);
            EditorGUILayout.Space(2);

            m_Material.DisableKeyword("_OVERLAYMODE_NONE");
            m_Material.DisableKeyword("_OVERLAYMODE_HATCHING");
            m_Material.EnableKeyword("_OVERLAYMODE_HALFTONE");

            DrawProperty(haftoneColor);
            DrawProperty(halftoneToonAffect);
            DrawProperty(diffuseWarpAffectHalftone);
            DrawProperty(halftoneThreshold);
            DrawProperty(halftoneEdgeOffset);
            DrawProperty(halftoneSmoothness);
            DrawProperty(halftoneShapeSmoothness);

            EditorGUILayout.Space();

            DrawProperty(haltoneTex);
            DrawProperty(overlayUvTilling);
            EditorGUILayout.Space(2);

            DrawProperty(useScreenUvs);

            if (useScreenUvs.floatValue == 1)
            {
                DrawProperty(useAdaptiveUvs);
                DrawProperty(uvAnimationSpeed);

            }


            DrawProperty(overlayRotation);
            DrawProperty(uvScrollAngle);
            DrawProperty(uvScrollSpeed);

            EditorGUILayout.Space(2);
            EditorGUILayout.EndVertical();
        }
        else if(overlayModeValue == 2)
        {
            //HATHING
            EditorGUILayout.BeginVertical(BoxScopeStyle);
            EditorGUILayout.Space(2);

            m_Material.DisableKeyword("_OVERLAYMODE_NONE");
            m_Material.EnableKeyword("_OVERLAYMODE_HATCHING");
            m_Material.DisableKeyword("_OVERLAYMODE_HALFTONE");

            DrawProperty(usePureSketh);
            if(usePureSketh.floatValue == 1)
            {
                DrawProperty(paper);
                DrawProperty(paperTilling);

            }

            DrawProperty(hatchColor);
            DrawProperty(hatchIndirectAffect);
            DrawProperty(lighterHatchTex);
            DrawProperty(darkerHatchTex);
            DrawProperty(overlayUvTilling);

            DrawProperty(overlayRotation);
            DrawProperty(uvScrollAngle);
            DrawProperty(uvScrollSpeed);
            DrawProperty(hatchConstantScale);
            DrawProperty(maxScaleDependingOnCamera);

            EditorGUILayout.Space();

            DrawProperty(useScreenUvs);
            if(useScreenUvs.floatValue == 0)
            {
            }
            else
            {
                DrawProperty(useAdaptiveUvs);

                DrawProperty(uvAnimationSpeed);

            }

            EditorGUILayout.Space();

            DrawProperty(hatchLighten);
            DrawProperty(hatchDarken);

            EditorGUILayout.Space(2);
            EditorGUILayout.EndVertical();

        }


        EditorGUILayout.Space(2);
        EditorGUILayout.EndVertical();
    }
   
    private void OutlineEditor()
    {
        List<MaterialProperty> list = new List<MaterialProperty>
            {
                outlineThicnkess,outlineAdaptiveThicnkess,outlineType,outlineColor,outlineTextureStrength
            };

        EditorGUILayout.BeginVertical(BoxScopeStyle);
        EditorGUILayout.Space(2);


        DrawToggleHeader(outlineUse);

        bool isParamPropEnabled = !Mathf.Approximately(outlineUse.floatValue, 0f);
        if (isParamPropEnabled)
        {
            EditorGUILayout.BeginVertical(BoxScopeStyle);
            EditorGUILayout.Space(2);

            foreach (var prop in list)
            {
                DrawProperty(prop);
            }

            EditorGUILayout.Space(2);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            DrawToggleBoxScope(outlineDynamicUse,
                new List<MaterialProperty>
                {
                outlineNoiseIntesity,outlineNoiseScale,outlineDynamicSpeed
                }
                );


        }

        EditorGUILayout.Space(2);
        EditorGUILayout.EndVertical();
    }

    #endregion

    /*
   private void HairSpecularLightEditor()
   {
       List<MaterialProperty> list = new List<MaterialProperty>
           {
               hairSpecularShadowMask,hairSpecularRotation,hairSpecularLocalShift,hairSpecularLocalShiftStrength,hairSpecularLocalShiftTex,
               hairSpecularPrimaryColor,hairSpecularPrimaryIntesity,hairSpecularPrimaryPower,hairSpecularPrimarySmoothness,
               hairSpecularSecondaryColor,hairSpecularSecondaryIntesity,hairSpecularSecondaryPower,hairSpecularSecondarySmoothness,hairSpecularStrandTex
           };

       EditorGUILayout.BeginVertical(BoxScopeStyle);
       EditorGUILayout.Space(2);


       DrawToggleHeader(hairSpecularUse);

       bool isParamPropEnabled = !Mathf.Approximately(hairSpecularUse.floatValue, 0f);
       if (isParamPropEnabled)
       {
           EditorGUILayout.BeginVertical(BoxScopeStyle);
           EditorGUILayout.Space(2);

           foreach (var prop in list)
           {
               DrawProperty(prop);
           }

           EditorGUILayout.Space(2);
           EditorGUILayout.EndVertical();

           EditorGUILayout.Space();

           DrawToggleBoxScope(hairSpecularMaskUse,
               new List<MaterialProperty>
               {
               hairSpecularMaskStrength,hairSpecularMaskScale,hairSpecularMaskTexture,hairSpecularMaskRotation
               }
               );


       }

       EditorGUILayout.Space(2);
       EditorGUILayout.EndVertical();
   }*/

    #region RampFunctions

    private Texture2D UpdateTex(int width)
    {
        Texture2D tex = new Texture2D(width, 1, TextureFormat.RGBA32, false,false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        tex.name = "RampTexture";

        var colors = new Color[tex.width * tex.height];
        for (int x = 0; x < width; ++x)
        {
            colors[x] = s_gradient.Evaluate(1.0f * x / (width - 1));
        }
        tex.SetPixels(colors);
        tex.Apply();


        return tex;
    }

    private void ExportToPNG(Texture2D rampTex)
    {
        var savePath = s_texturePath;

        if (string.IsNullOrEmpty(savePath))
        {
            SetTexturePath();

            savePath = s_texturePath;

            var bytes = rampTex.EncodeToPNG();
            System.IO.File.WriteAllBytes(savePath, bytes);
            AssetDatabase.Refresh();
        }

        if (!string.IsNullOrEmpty(savePath))
        {
            var bytes = rampTex.EncodeToPNG();
            System.IO.File.WriteAllBytes(savePath, bytes);
            AssetDatabase.Refresh();
        }

        //rampMap.textureValue = (Texture)AssetDatabase.LoadMainAssetAtPath(savePath);

    }

    private void SetTexturePath()
    {
        var currentRampObjPath = AssetDatabase.GetAssetPath(m_MaterialEditor.serializedObject.targetObject);
        var defaultDirectory = System.IO.Path.GetDirectoryName(currentRampObjPath);
        s_texturePath = EditorUtility.SaveFilePanelInProject("Export as PNG file", "LightRamp", "png", "Set file path for the PNG file", defaultDirectory);
    }
    #endregion

}
