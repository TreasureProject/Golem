// Made with Amplify Shader Editor v1.9.5.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Stylized Toon"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[Toggle(_USERIMLIGHT_ON)] _UseRimLight("UseRim Light", Float) = 0
		[Toggle(_USEOUTLINE_ON)] _UseOutline("UseOutline", Float) = 0
		[Toggle(_USESPECULAR_ON)] _UseSpecular("UseSpecular Highlights", Float) = 1
		_SpecColor("Specular Value", Color) = (1,1,1,0)
		_Thicnkess("Thickness", Range( 0 , 0.1)) = 0
		_AdaptiveThicnkess("Adaptive Thickness", Range( 0 , 1)) = 0
		[KeywordEnum(Normal,Position,UVBaked)] _OutlineType("Outline Type", Float) = 0
		[HDR]_OutlineColor("Outline Color", Color) = (0,0,0,0)
		_Color("Color", Color) = (0.6792453,0.6792453,0.6792453,1)
		_SpecularFaloff("Specular Falloff", Range( 0 , 1)) = 0
		_LightRampOffset("Light Ramp Offset", Range( -1 , 1)) = 0
		_MainTex("Albedo Texture", 2D) = "white" {}
		[NoScaleOffset][SingleLineTexture]_LightRampTexture("Light Ramp Texture", 2D) = "white" {}
		_SideShineHardness("Side Shine Hardness", Range( 0 , 0.5)) = 0
		_BacklightHardness("Backlight Hardness", Range( 0 , 0.5)) = 0
		_StepOffset("Step Offset", Range( -0.5 , 0.5)) = 0
		[KeywordEnum(Step,DiffuseRamp,Posterize)] _UseLightRamp("Shading Mode", Float) = 0
		[HideInInspector]_RampDiffuseTextureLoaded("RampDiffuseTextureLoaded", Float) = 1
		[NoScaleOffset][SingleLineTexture]_DiffuseWarpNoise("Diffuse Warp Noise", 2D) = "white" {}
		[NoScaleOffset][SingleLineTexture]_SpecularMaskTexture("Specular Mask Texture", 2D) = "white" {}
		_SpecularMaskStrength("Specular Mask Strength", Range( 0 , 1)) = 0.1856417
		_WarpStrength("Warp Strength", Range( -1 , 1)) = 0
		_SpecularMaskScale("Specular Mask Tiling", Float) = 1
		_WarpTextureScale("UV Tiling", Float) = 1
		[Toggle]_UseDiffuseWarp("UseDiffuse Warp", Float) = 0
		[Toggle]_UseSpecularMask("UseSpecular Mask", Float) = 0
		[HDR]_RimColor("Rim Color", Color) = (1,1,1,0)
		_RimThickness("Rim Thickness", Range( 0 , 3)) = 1
		_RimPower("Rim Power", Range( 1 , 12)) = 2
		_RimSmoothness("Rim Smoothness", Range( 0 , 0.5)) = 0
		[Normal]_BumpMap("Normal Map", 2D) = "bump" {}
		_NormalMapStrength("Normal Map Strength", Float) = 1
		_SpecularPosterizeSteps("Specular Posterize Steps", Range( 0 , 15)) = 0
		_OutlineNoiseScale("Outline Noise Scale", Float) = 10
		_OutlineNoiseIntesity("Outline Noise Intensity", Range( 0 , 2)) = 1
		_OutlineDynamicSpeed("Outline Dynamic Speed", Range( 0 , 0.05)) = 0
		[Toggle(_USEDYNAMICOUTLINE_ON)] _UseDynamicOutline("Use Sketch Outline", Float) = 0
		[Toggle(_USEENVIRONMENTREFLETION_ON)] _UseEnvironmentRefletion("UseEnvironment Reflections", Float) = 0
		_Strength("Strength", Range( 0 , 1)) = 0
		[NoScaleOffset][SingleLineTexture]_SpecGlossMap("Specular Map", 2D) = "white" {}
		_Glossiness("Smoothness", Range( 0 , 1)) = 0.5
		_Cutoff("Alpha Clip Threshold", Range( 0 , 1)) = 0
		[HDR]_EmissionColor("Emission Color", Color) = (0,0,0,0)
		[HDR][NoScaleOffset][SingleLineTexture]_EmissionMap("Emission Map", 2D) = "white" {}
		_UseEmission("UseEmission", Float) = 0
		_IndirectLightStrength("Indirect Light Strength", Range( 0 , 1)) = 1
		_SpecularShadowMask("Specular Shadow Mask", Range( 0 , 1)) = 0
		_WarpTextureRotation("UV Rotation", Range( 0 , 180)) = 0
		_SpecularMaskRotation("Specular Mask Rotation", Range( 0 , 180)) = 0
		_BacklightAmount("Backlight Amount", Range( 0 , 1)) = 0.5
		[Toggle]_UseShadows("Use Shadows", Float) = 1
		_BacklightIntensity("Backlight Intensity", Range( 0 , 4)) = 1
		[Toggle(_USEBACKLIGHT_ON)] _UseBacklight("Rim As Backlight & Side Shine", Float) = 0
		_SideShineIntensity("Side Shine Intensity", Range( 0 , 4)) = 1
		_SideShineAmount("Side Shine Amount", Range( 0 , 0.7)) = 0.2717647
		[NoScaleOffset][SingleLineTexture]_HalftoneTexture("Halftone Texture", 2D) = "white" {}
		_HalftoneSmoothness("Halftone Smoothness", Range( 0 , 2)) = 0.3
		_HalftoneEdgeOffset("Halftone Edge Offset", Range( -1 , 1)) = 0.1
		_HalftoneThreshold("Halftone Threshold", Range( 0 , 0.15)) = 0.1
		_HalftoneColor("Halftone Color", Color) = (0,0,0,1)
		[NoScaleOffset]_Hatch2("Hatch Texture Brighter", 2D) = "white" {}
		[NoScaleOffset]_Hatch1("Hatch Texture Darker", 2D) = "white" {}
		_MaxScaleDependingOnCamera("Max Scale Depends On Camera", Range( 1 , 10)) = 1
		[Toggle(_USEHATCHINGCONSTANTSCALE_ON)] _UseHatchingConstantScale("Hatching Constant Scale", Float) = 1
		[KeywordEnum(None,Haftone,Hatching)] _OverlayMode1("Overlay Mode", Float) = 0
		_ShapeSmoothness("Transition Smoothness", Range( 0 , 1)) = 0.2
		_OverlayRotation("UV Rotation", Range( 0 , 180)) = 0
		_OverlayUVTilling("UV Tiling", Float) = 0
		[Toggle(_USESCREENUVS_ON)] _UseScreenUvs("Screen Uvs", Float) = 0
		[Toggle(_USESCREENUVSSPECULAR_ON)] _UseScreenUvsSpecular("Screen Uvs", Float) = 0
		_Darken("Darken", Range( -1 , 1)) = 0
		_Lighten("Lighten", Range( -1 , 1)) = 0
		_AdditionalLightsSmoothnessMultiplier("Additional Lights Specular Size", Range( 0 , 2)) = 1
		_SmoothnessMultiplier("Main Specular Size", Range( 0 , 2)) = 1
		_AdditionalLightsIntesity("Additional Lights Intensity", Range( 0 , 6)) = 1
		[Toggle(_USEADDITIONALLIGHTSDIFFUSE_ON)] _UseAdditionalLightsDiffuse("UseAdditional Lights", Float) = 1
		_AdditionalLightsAmount("Additional Lights Size", Range( 0 , 1)) = 1
		_AdditionalLightsFaloff("Additional Lights Falloff", Range( 0 , 1)) = 1
		_HatchingColor("Hatching Color", Color) = (0,0,0,1)
		[KeywordEnum(None,Haftone,Hatching)] _OverlayMode("Overlay Mode", Float) = 0
		[Toggle(_USEPURESKETCH_ON)] _UsePureSketch("Pure Sketch", Float) = 1
		_UVAnimationSpeedWarp("UV Animation Speed", Range( 0 , 5)) = 0
		_UVAnimationSpeedSpec("UV Animation Speed", Range( 0 , 5)) = 0
		_UVAnimationSpeed("UV Animation Speed", Range( 0 , 5)) = 0
		_UVSrcrollAngleSpec("UV Scroll Angle", Range( 0 , 360)) = 0
		_UVSrcrollAngle("UV Scroll Angle", Range( 0 , 360)) = 0
		_UVSrcrollAngleWarp("UV Scroll Angle", Range( 0 , 360)) = 0
		_UVScrollSpeedWarp("UV Scroll Speed", Range( 0 , 0.1)) = 0
		_UVScrollSpeed("UV Scroll Speed", Range( 0 , 0.1)) = 0
		_UVScrollSpeedSpec("UV Scroll Speed", Range( 0 , 0.1)) = 0
		_DiffusePosterizeSteps("Posterize Steps", Range( 1 , 10)) = 3
		_DiffusePosterizePower("Posterize Power", Range( 0.5 , 3)) = 1
		_DiffusePosterizeOffset("Posterize Offset", Range( -0.5 , 0.5)) = 0
		_MainLightIntesity("Main Light Intensity", Range( 0 , 6)) = 1
		_OutlineTextureStrength("Texture Strength ", Range( 0 , 1)) = 0
		_ShadowColor("Shadow Color", Color) = (0,0,0,0)
		_HalftoneToonAffect("Toon Affect", Range( 0 , 1)) = 0
		_IndirectLightAffectOnHatch("Indirect Light Affect On Hatch", Range( 0 , 1)) = 0
		[Toggle(_USEDIFFUSEWARPASOVERLAY_ON)] _UseDiffuseWarpAsOverlay("Impact Shadows", Float) = 0
		[Toggle(_DIFFUSEWARPAFFECTHALFTONE_ON)] _DiffuseWarpAffectHalftone("Diffuse Warp Affect Halftone", Float) = 0
		_DiffuseWarpBrightnessOffset("Brightness Offset", Float) = 1.12
		[Toggle(_USESCREENUVSWARP_ON)] _UseScreenUvsWarp("Screen Uvs", Float) = 0
		[Toggle]_StepHalftoneTexture("Step Halftone Texture", Float) = 0
		_HaltonePatternSize("Halftone Pattern Size", Range( 0 , 1)) = 0
		_RimShadowColor("Rim Shadow Color", Color) = (0,0.05551431,0.9622642,0)
		[KeywordEnum(NoSplit,MultiplyWithDiffuse,UseSecondColor)] _RimSplitColor("Rim Split Color", Float) = 0
		_OcclusionMap("Occlusion Map", 2D) = "white" {}
		_OcclusionStrength("Occlusion Strength ", Range( 0 , 1)) = 1
		[Toggle(_USEADAPTIVESCREENUVS_ON)] _UseAdaptiveScreenUvs("Adaptive Screen Uvs", Float) = 0
		[Toggle(_USEADAPTIVEUVSSPECULAR_ON)] _UseAdaptiveUvsSpecular("Adaptive Screen Uvs", Float) = 0
		[Toggle(_USEADAPTIVESCREENUVSWARP_ON)] _UseAdaptiveScreenUvsWarp("Adaptive Screen Uvs ", Float) = 0
		[Toggle(_INVERSEMASK_ON)] _InverseMask("Inverse Mask", Float) = 0
		_PaperTilling("Paper Tiling", Float) = 1
		[NoScaleOffset]_PaperTexture("Paper Texture", 2D) = "white" {}


		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25

		[HideInInspector] _QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector] _QueueControl("_QueueControl", Float) = -1

        [HideInInspector][NoScaleOffset] unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}

		[HideInInspector][ToggleOff] _ReceiveShadows("Receive Shadows", Float) = 1.0
	}

	SubShader
	{
		LOD 0

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" "UniversalMaterialType"="Unlit" }

		Cull Back
		AlphaToMask Off

		

		HLSLINCLUDE
		#pragma target 3.5
		#pragma prefer_hlslcc gles
		// ensure rendering platforms toggle list is visible

		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}

		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS
		ENDHLSL

		
		Pass
		{
			Name "ExtraPrePass"
			

			Blend One Zero
			Cull Front
			ZWrite On
			ZTest LEqual
			Offset 0,0
			ColorMask RGBA

			

			HLSLPROGRAM

			

			#define _ALPHATEST_ON 1
			#define ASE_SRP_VERSION 140011


			

			#pragma vertex vert
			#pragma fragment frag

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_VERT_POSITION
			#pragma shader_feature_local _USEOUTLINE_ON
			#pragma shader_feature_local _USEDYNAMICOUTLINE_ON
			#pragma shader_feature_local _OUTLINETYPE_NORMAL _OUTLINETYPE_POSITION _OUTLINETYPE_UVBAKED


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 positionWS : TEXCOORD1;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord : TEXCOORD2;
				#endif
				#ifdef ASE_FOG
					float fogFactor : TEXCOORD3;
				#endif
				float4 ase_texcoord4 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _ShadowColor;
			float4 _RimShadowColor;
			float4 _HalftoneColor;
			float4 _RimColor;
			float4 _Color;
			float4 _EmissionColor;
			float4 _OcclusionMap_ST;
			float4 _SpecColor;
			float4 _HatchingColor;
			float4 _OutlineColor;
			float _SmoothnessMultiplier;
			float _UseSpecularMask;
			float _MainLightIntesity;
			float _StepHalftoneTexture;
			float _SpecularMaskScale;
			float _UVAnimationSpeedSpec;
			float _AdditionalLightsIntesity;
			float _UVScrollSpeedSpec;
			float _UVSrcrollAngleSpec;
			float _SpecularMaskRotation;
			float _HaltonePatternSize;
			float _SpecularMaskStrength;
			float _SpecularFaloff;
			float _RampDiffuseTextureLoaded;
			float _RimSmoothness;
			float _Strength;
			float _MaxScaleDependingOnCamera;
			float _Lighten;
			float _Darken;
			float _IndirectLightAffectOnHatch;
			float _UseEmission;
			float _SideShineAmount;
			float _SpecularShadowMask;
			float _SideShineHardness;
			float _BacklightAmount;
			float _BacklightHardness;
			float _BacklightIntensity;
			float _RimPower;
			float _RimThickness;
			float _SpecularPosterizeSteps;
			float _SideShineIntensity;
			float _AdditionalLightsSmoothnessMultiplier;
			float _UVAnimationSpeed;
			float _OverlayRotation;
			float _WarpStrength;
			float _WarpTextureRotation;
			float _UVAnimationSpeedWarp;
			float _UVSrcrollAngleWarp;
			float _UVScrollSpeedWarp;
			float _WarpTextureScale;
			float _UseDiffuseWarp;
			float _StepOffset;
			float _OcclusionStrength;
			float _OutlineTextureStrength;
			float _Thicnkess;
			float _OutlineNoiseIntesity;
			float _OutlineNoiseScale;
			float _OutlineDynamicSpeed;
			float _AdaptiveThicnkess;
			float _NormalMapStrength;
			float _Glossiness;
			float _UseShadows;
			float _LightRampOffset;
			float _PaperTilling;
			float _UVSrcrollAngle;
			float _UVScrollSpeed;
			float _OverlayUVTilling;
			float _HalftoneSmoothness;
			float _HalftoneEdgeOffset;
			float _HalftoneThreshold;
			float _ShapeSmoothness;
			float _HalftoneToonAffect;
			float _IndirectLightStrength;
			float _AdditionalLightsFaloff;
			float _AdditionalLightsAmount;
			float _DiffusePosterizeSteps;
			float _DiffusePosterizePower;
			float _DiffusePosterizeOffset;
			float _DiffuseWarpBrightnessOffset;
			float _Cutoff;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			sampler2D _MainTex;
			sampler2D _OcclusionMap;


			float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }
			float snoise( float2 v )
			{
				const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
				float2 i = floor( v + dot( v, C.yy ) );
				float2 x0 = v - i + dot( i, C.xx );
				float2 i1;
				i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
				float4 x12 = x0.xyxy + C.xxzz;
				x12.xy -= i1;
				i = mod2D289( i );
				float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
				float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
				m = m * m;
				m = m * m;
				float3 x = 2.0 * frac( p * C.www ) - 1.0;
				float3 h = abs( x ) - 0.5;
				float3 ox = floor( x + 0.5 );
				float3 a0 = x - ox;
				m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
				float3 g;
				g.x = a0.x * x0.x + h.x * x0.y;
				g.yz = a0.yz * x12.xz + h.yz * x12.yw;
				return 130.0 * dot( m, g );
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 ase_worldPos = TransformObjectToWorld( (v.positionOS).xyz );
				float lerpResult59 = lerp( 1.0 , distance( _WorldSpaceCameraPos , ase_worldPos ) , _AdaptiveThicnkess);
				#if defined( _OUTLINETYPE_NORMAL )
				float3 staticSwitch57 = v.normalOS;
				#elif defined( _OUTLINETYPE_POSITION )
				float3 staticSwitch57 = v.positionOS.xyz;
				#elif defined( _OUTLINETYPE_UVBAKED )
				float3 staticSwitch57 = float3( v.ase_texcoord3.xy ,  0.0 );
				#else
				float3 staticSwitch57 = v.normalOS;
				#endif
				float2 temp_cast_1 = (( ( _TimeParameters.x ) * _OutlineDynamicSpeed )).xx;
				float2 texCoord591 = v.ase_texcoord.xy * float2( 1,1 ) + temp_cast_1;
				float simplePerlin2D590 = snoise( texCoord591*_OutlineNoiseScale );
				simplePerlin2D590 = simplePerlin2D590*0.5 + 0.5;
				#ifdef _USEDYNAMICOUTLINE_ON
				float3 staticSwitch606 = ( ( simplePerlin2D590 * _OutlineNoiseIntesity ) * staticSwitch57 );
				#else
				float3 staticSwitch606 = staticSwitch57;
				#endif
				#ifdef _USEOUTLINE_ON
				float3 staticSwitch365 = ( lerpResult59 * ( staticSwitch606 * _Thicnkess ) );
				#else
				float3 staticSwitch365 = float3( 0,0,0 );
				#endif
				
				o.ase_texcoord4.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = staticSwitch365;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( v.positionOS.xyz );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.positionWS = vertexInput.positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				#ifdef ASE_FOG
					o.fogFactor = ComputeFogFactor( vertexInput.positionCS.z );
				#endif

				o.positionCS = vertexInput.positionCS;
				o.clipPosV = vertexInput.positionCS;
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				o.ase_texcoord3 = v.ase_texcoord3;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.ase_texcoord3 = patch[0].ase_texcoord3 * bary.x + patch[1].ase_texcoord3 * bary.y + patch[2].ase_texcoord3 * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 WorldPosition = IN.positionWS;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				float4 ClipPos = IN.clipPosV;
				float4 ScreenPos = ComputeScreenPos( IN.clipPosV );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float2 uv_OcclusionMap = IN.ase_texcoord4.xy * _OcclusionMap_ST.xy + _OcclusionMap_ST.zw;
				float4 tex2DNode362 = tex2D( _MainTex, uv_OcclusionMap );
				float4 lerpResult1448 = lerp( float4( 1,1,1,0 ) , tex2DNode362 , _OutlineTextureStrength);
				
				float lerpResult1655 = lerp( 1.0 , tex2D( _OcclusionMap, uv_OcclusionMap ).r , _OcclusionStrength);
				float4 appendResult1656 = (float4(lerpResult1655 , lerpResult1655 , lerpResult1655 , 1.0));
				float4 MainTexture364 = ( _Color * tex2DNode362 * appendResult1656 );
				float temp_output_673_0 = ( MainTexture364.a * 1.0 );
				#ifdef _USEOUTLINE_ON
				float staticSwitch1349 = temp_output_673_0;
				#else
				float staticSwitch1349 = -2.0;
				#endif
				

				float3 Color = ( _OutlineColor * lerpResult1448 ).rgb;
				float Alpha = staticSwitch1349;
				float AlphaClipThreshold = 0.5;

				#ifdef _ALPHATEST_ON
					clip( Alpha - AlphaClipThreshold );
				#endif

				#ifdef ASE_FOG
					Color = MixFog( Color, IN.fogFactor );
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.positionCS );
				#endif

				return half4( Color, Alpha );
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForwardOnly" }

			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			

			HLSLPROGRAM

			

			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			#pragma multi_compile_instancing
			#pragma instancing_options renderinglayer
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define _ALPHATEST_ON 1
			#define ASE_SRP_VERSION 140011


			

			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3

			

			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
			#pragma multi_compile_fragment _ DEBUG_DISPLAY

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_UNLIT

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			
			#if ASE_SRP_VERSION >=140010
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#endif
		

			

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging3D.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#define ASE_NEEDS_FRAG_SCREEN_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_VERT_TEXTURE_COORDINATES1
			#define ASE_NEEDS_FRAG_SHADOWCOORDS
			#define ASE_NEEDS_FRAG_POSITION
			#pragma shader_feature_local _OVERLAYMODE_NONE _OVERLAYMODE_HAFTONE _OVERLAYMODE_HATCHING
			#pragma shader_feature_local _USELIGHTRAMP_STEP _USELIGHTRAMP_DIFFUSERAMP _USELIGHTRAMP_POSTERIZE
			#pragma shader_feature_local _USESCREENUVSWARP_ON
			#pragma shader_feature_local _USEADAPTIVESCREENUVSWARP_ON
			#pragma shader_feature_local _USEDIFFUSEWARPASOVERLAY_ON
			#pragma shader_feature_local _USEADDITIONALLIGHTSDIFFUSE_ON
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma shader_feature_local _DIFFUSEWARPAFFECTHALFTONE_ON
			#pragma shader_feature_local _USESCREENUVS_ON
			#pragma shader_feature_local _USEADAPTIVESCREENUVS_ON
			#pragma shader_feature_local _USESPECULAR_ON
			#pragma shader_feature_local _INVERSEMASK_ON
			#pragma shader_feature_local _USESCREENUVSSPECULAR_ON
			#pragma shader_feature_local _USEADAPTIVEUVSSPECULAR_ON
			#pragma shader_feature_local _OVERLAYMODE1_NONE _OVERLAYMODE1_HAFTONE _OVERLAYMODE1_HATCHING
			#pragma shader_feature_local _USEENVIRONMENTREFLETION_ON
			#pragma shader_feature_local _RIMSPLITCOLOR_NOSPLIT _RIMSPLITCOLOR_MULTIPLYWITHDIFFUSE _RIMSPLITCOLOR_USESECONDCOLOR
			#pragma shader_feature_local _USERIMLIGHT_ON
			#pragma shader_feature_local _USEBACKLIGHT_ON
			#pragma shader_feature_local _USEPURESKETCH_ON
			#pragma shader_feature_local _USEHATCHINGCONSTANTSCALE_ON
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _FORWARD_PLUS


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 positionWS : TEXCOORD1;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord : TEXCOORD2;
				#endif
				#ifdef ASE_FOG
					float fogFactor : TEXCOORD3;
				#endif
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				float4 ase_texcoord7 : TEXCOORD7;
				float4 ase_texcoord8 : TEXCOORD8;
				float4 lightmapUVOrVertexSH : TEXCOORD9;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _ShadowColor;
			float4 _RimShadowColor;
			float4 _HalftoneColor;
			float4 _RimColor;
			float4 _Color;
			float4 _EmissionColor;
			float4 _OcclusionMap_ST;
			float4 _SpecColor;
			float4 _HatchingColor;
			float4 _OutlineColor;
			float _SmoothnessMultiplier;
			float _UseSpecularMask;
			float _MainLightIntesity;
			float _StepHalftoneTexture;
			float _SpecularMaskScale;
			float _UVAnimationSpeedSpec;
			float _AdditionalLightsIntesity;
			float _UVScrollSpeedSpec;
			float _UVSrcrollAngleSpec;
			float _SpecularMaskRotation;
			float _HaltonePatternSize;
			float _SpecularMaskStrength;
			float _SpecularFaloff;
			float _RampDiffuseTextureLoaded;
			float _RimSmoothness;
			float _Strength;
			float _MaxScaleDependingOnCamera;
			float _Lighten;
			float _Darken;
			float _IndirectLightAffectOnHatch;
			float _UseEmission;
			float _SideShineAmount;
			float _SpecularShadowMask;
			float _SideShineHardness;
			float _BacklightAmount;
			float _BacklightHardness;
			float _BacklightIntensity;
			float _RimPower;
			float _RimThickness;
			float _SpecularPosterizeSteps;
			float _SideShineIntensity;
			float _AdditionalLightsSmoothnessMultiplier;
			float _UVAnimationSpeed;
			float _OverlayRotation;
			float _WarpStrength;
			float _WarpTextureRotation;
			float _UVAnimationSpeedWarp;
			float _UVSrcrollAngleWarp;
			float _UVScrollSpeedWarp;
			float _WarpTextureScale;
			float _UseDiffuseWarp;
			float _StepOffset;
			float _OcclusionStrength;
			float _OutlineTextureStrength;
			float _Thicnkess;
			float _OutlineNoiseIntesity;
			float _OutlineNoiseScale;
			float _OutlineDynamicSpeed;
			float _AdaptiveThicnkess;
			float _NormalMapStrength;
			float _Glossiness;
			float _UseShadows;
			float _LightRampOffset;
			float _PaperTilling;
			float _UVSrcrollAngle;
			float _UVScrollSpeed;
			float _OverlayUVTilling;
			float _HalftoneSmoothness;
			float _HalftoneEdgeOffset;
			float _HalftoneThreshold;
			float _ShapeSmoothness;
			float _HalftoneToonAffect;
			float _IndirectLightStrength;
			float _AdditionalLightsFaloff;
			float _AdditionalLightsAmount;
			float _DiffusePosterizeSteps;
			float _DiffusePosterizePower;
			float _DiffusePosterizeOffset;
			float _DiffuseWarpBrightnessOffset;
			float _Cutoff;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			sampler2D _DiffuseWarpNoise;
			sampler2D _BumpMap;
			sampler2D _OcclusionMap;
			sampler2D _LightRampTexture;
			sampler2D _MainTex;
			sampler2D _HalftoneTexture;
			sampler2D _SpecGlossMap;
			sampler2D _SpecularMaskTexture;
			sampler2D _EmissionMap;
			sampler2D _Hatch1;
			sampler2D _Hatch2;
			sampler2D _PaperTexture;


			inline float4 ASE_ComputeGrabScreenPos( float4 pos )
			{
				#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
				#else
				float scale = 1.0;
				#endif
				float4 o = pos;
				o.y = pos.w * 0.5f;
				o.y = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
				return o;
			}
			
			float Posterize1331( float In, float Steps )
			{
				return  floor(In / (1 / Steps)) * (1 / Steps);
			}
			
			half4 CalculateShadowMask1_g213( half2 LightmapUV )
			{
				#if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
				return SAMPLE_SHADOWMASK( LightmapUV.xy );
				#elif !defined (LIGHTMAP_ON)
				return unity_ProbesOcclusion;
				#else
				return half4( 1, 1, 1, 1 );
				#endif
			}
			
			float3 AdditionalLight( float3 WorldPosition, float3 WorldNormal, float3 LightWrapVector, float SMin, float SMax, float Faloff, float4 shadowmask, float2 ScreenUV )
			{
				float3 Color = 0;
					
					
				#define SUM_LIGHT(Light)\
						float3 DotVector = dot(light.direction,WorldNormal);	\
					half3 AttLightColor = (light.shadowAttenuation * light.distanceAttenuation);\
					 float3 colout = max(float3(0.f,0.f,0.f),LightWrapVector + (1-LightWrapVector) * DotVector )*AttLightColor*light.color; \
					float maxColor = max(colout.r,max(colout.g,colout.b));\
					float3 outColor = smoothstep(SMin,SMax,maxColor)*light.color;\
					 Color += outColor;
					InputData inputData = (InputData)0;
					inputData.normalizedScreenSpaceUV = ScreenUV;
					inputData.positionWS = WorldPosition;
					uint meshRenderingLayers = GetMeshRenderingLayer();
					uint pixelLightCount = GetAdditionalLightsCount();
					#if USE_FORWARD_PLUS
					for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
					{
						FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
						Light light = GetAdditionalLight(lightIndex, WorldPosition,shadowmask);
						#ifdef _LIGHT_LAYERS
						if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
						#endif
						{
							SUM_LIGHT( light );
						}
					}
					#endif
					LIGHT_LOOP_BEGIN( pixelLightCount )
						Light light = GetAdditionalLight(lightIndex, WorldPosition,shadowmask);
						#ifdef _LIGHT_LAYERS
						if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
						#endif
						{
							SUM_LIGHT( light );
						}
					LIGHT_LOOP_END
				return Color;
			}
			
			float3 ASEIndirectDiffuse( float2 uvStaticLightmap, float3 normalWS )
			{
			#ifdef LIGHTMAP_ON
				return SampleLightmap( uvStaticLightmap, normalWS );
			#else
				return SampleSH(normalWS);
			#endif
			}
			
			float3 AdditionalLightsSpecularMy( float3 WorldPosition, float3 WorldNormal, float3 WorldView, float3 SpecColor, float Smoothness, float Steps, float SpecFaloff )
			{
				float3 Color = 0;
				Smoothness = exp2(10 * Smoothness + 1);
				int numLights = GetAdditionalLightsCount();
				for(int i = 0; i<numLights;i++)
				{
					
							#if VERSION_GREATER_EQUAL(10, 1)
							Light light = GetAdditionalLight(i, WorldPosition, half4(1,1,1,1));
							// see AdditionalLights_float for explanation of this
						#else
							Light light = GetAdditionalLight(i, WorldPosition);
						#endif
					
					half3 AttLightColor = light.color *(light.distanceAttenuation * light.shadowAttenuation);
					Color += LightingSpecular(AttLightColor, light.direction, WorldNormal, WorldView, half4(SpecColor, 0), Smoothness);	
				}
				float IN = max(Color.b,max(Color.r,Color.g));
				float minOut = 0.5 * SpecFaloff - 0.005;
				float faloff = lerp(IN, smoothstep(minOut, 0.5, IN), SpecFaloff);
				if(Steps < 1)
				{
				    return Color *faloff;
				}
				else
				{
				    return  Color *floor(faloff / (1 / Steps)) * (1 / Steps);
				}
			}
			
			float FaloffPosterize( float IN, float SpecFaloff, float Steps )
			{
				float minOut = 0.5 * SpecFaloff - 0.005;
				float faloff = lerp(IN, smoothstep(minOut, 0.5, IN), SpecFaloff);
				if(Steps < 1)
				{
				    return faloff;
				}
				else
				{
				    return  floor(faloff / (1 / Steps)) * (1 / Steps);
				}
			}
			
			float3 Hatching476( float2 _uv, float color, sampler2D _Hatch0, sampler2D _Hatch1 )
			{
				float intensity = color;
				    float3 hatch0 = tex2D(_Hatch0, _uv).rgb;
				    float3 hatch1 = tex2D(_Hatch1, _uv).rgb;
				    float3 overbright = max(0, intensity - 1.0);
				    float3 weightsA = saturate((intensity * 6.0) + float3(-0, -1, -2));
				    float3 weightsB = saturate((intensity * 6.0) + float3(-3, -4, -5));
				    weightsA.xy -= weightsA.yz;
				    weightsA.z -= weightsB.x;
				    weightsB.xy -= weightsB.yz;
				    hatch0 = hatch0 * weightsA;
				    hatch1 = hatch1 * weightsB;
				    float3 hatching = overbright + hatch0.r +
				        hatch0.g + hatch0.b +
				        hatch1.r + hatch1.g +
				        hatch1.b;
				    return hatching;
				    return hatching;
			}
			
			float3 HatchingConstantScale491( float2 _uv, float _intensity, sampler2D _Hatch0, sampler2D _Hatch1, float _dist, float _MaxScaleDependingOnCamera )
			{
					float log2_dist = log2(_dist)-0.2;
					
					float2 floored_log_dist = floor( (log2_dist + float2(0.0, 1.0) ) * 0.5) *2.0 - float2(0.0, 1.0);				
					float2 uv_scale = min(_MaxScaleDependingOnCamera, pow(2.0, floored_log_dist));
					
					float uv_blend = abs(frac(log2_dist * 0.5) * 2.0 - 1.0);
					
					float2 scaledUVA = _uv / uv_scale.x; // 16
					float2 scaledUVB = _uv / uv_scale.y; // 8 
					float3 hatch0A = tex2D(_Hatch0, scaledUVA).rgb;
					float3 hatch1A = tex2D(_Hatch1, scaledUVA).rgb;
					float3 hatch0B = tex2D(_Hatch0, scaledUVB).rgb;
					float3 hatch1B = tex2D(_Hatch1, scaledUVB).rgb;
					float3 hatch0 = lerp(hatch0A, hatch0B, uv_blend);
					float3 hatch1 = lerp(hatch1A, hatch1B, uv_blend);
					float3 overbright = max(0, _intensity - 1.0);
					float3 weightsA = saturate((_intensity * 6.0) + float3(-0, -1, -2));
					float3 weightsB = saturate((_intensity * 6.0) + float3(-3, -4, -5));
					weightsA.xy -= weightsA.yz;
					weightsA.z -= weightsB.x;
					weightsB.xy -= weightsB.yz;
					hatch0 = hatch0 * weightsA;
					hatch1 = hatch1 * weightsB;
					float3 hatching = overbright + hatch0.r +
						hatch0.g + hatch0.b +
						hatch1.r + hatch1.g +
						hatch1.b;
					return hatching;
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 ase_worldTangent = TransformObjectToWorldDir(v.ase_tangent.xyz);
				o.ase_texcoord6.xyz = ase_worldTangent;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.normalOS);
				o.ase_texcoord7.xyz = ase_worldNormal;
				float ase_vertexTangentSign = v.ase_tangent.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 ase_worldBitangent = cross( ase_worldNormal, ase_worldTangent ) * ase_vertexTangentSign;
				o.ase_texcoord8.xyz = ase_worldBitangent;
				OUTPUT_LIGHTMAP_UV( v.ase_texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy );
				OUTPUT_SH( ase_worldNormal, o.lightmapUVOrVertexSH.xyz );
				
				o.ase_texcoord4.xy = v.ase_texcoord.xy;
				o.ase_texcoord5 = v.positionOS;
				o.ase_texcoord4.zw = v.ase_texcoord1.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord6.w = 0;
				o.ase_texcoord7.w = 0;
				o.ase_texcoord8.w = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( v.positionOS.xyz );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.positionWS = vertexInput.positionWS;
				#endif

				#ifdef ASE_FOG
					o.fogFactor = ComputeFogFactor( vertexInput.positionCS.z );
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.positionCS = vertexInput.positionCS;
				o.clipPosV = vertexInput.positionCS;
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_tangent : TANGENT;
				float4 ase_texcoord1 : TEXCOORD1;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				o.ase_texcoord = v.ase_texcoord;
				o.ase_tangent = v.ase_tangent;
				o.ase_texcoord1 = v.ase_texcoord1;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.ase_texcoord1 = patch[0].ase_texcoord1 * bary.x + patch[1].ase_texcoord1 * bary.y + patch[2].ase_texcoord1 * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag ( VertexOutput IN
				#ifdef _WRITE_RENDERING_LAYERS
				, out float4 outRenderingLayers : SV_Target1
				#endif
				 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 WorldPosition = IN.positionWS;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				float4 ClipPos = IN.clipPosV;
				float4 ScreenPos = ComputeScreenPos( IN.clipPosV );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float ase_lightIntensity = max( max( _MainLightColor.r, _MainLightColor.g ), _MainLightColor.b );
				float4 ase_lightColor = float4( _MainLightColor.rgb / ase_lightIntensity, ase_lightIntensity );
				float4 temp_output_1885_0 = max( ( ase_lightColor * ase_lightColor.a ) , float4( 0,0,0,0 ) );
				float temp_output_371_0 = ( _StepOffset + 0.5 );
				float2 temp_cast_0 = (_WarpTextureScale).xx;
				float mulTime1523 = _TimeParameters.x * _UVScrollSpeedWarp;
				float temp_output_1518_0 = radians( _UVSrcrollAngleWarp );
				float2 appendResult1520 = (float2(cos( temp_output_1518_0 ) , sin( temp_output_1518_0 )));
				float2 temp_output_1522_0 = ( mulTime1523 * appendResult1520 );
				float2 texCoord1495 = IN.ase_texcoord4.xy * temp_cast_0 + temp_output_1522_0;
				float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ScreenPos );
				float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
				float2 appendResult1847 = (float2(( _WarpTextureScale * ( _ScreenParams.x / _ScreenParams.y ) ) , _WarpTextureScale));
				float4 unityObjectToClipPos2_g211 = TransformWorldToHClip(TransformObjectToWorld(IN.ase_texcoord5.xyz));
				float4 computeScreenPos3_g211 = ComputeScreenPos( unityObjectToClipPos2_g211 );
				float temp_output_29_0_g211 = ( _WarpTextureScale / 4.0 );
				float4 unityObjectToClipPos13_g211 = TransformWorldToHClip(TransformObjectToWorld(float3(0,0,0)));
				float4 computeScreenPos17_g211 = ComputeScreenPos( unityObjectToClipPos13_g211 );
				float4 transform20_g211 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float4 break22_g211 = ( ( ( ( computeScreenPos3_g211 / (computeScreenPos3_g211).w ) * temp_output_29_0_g211 ) - ( temp_output_29_0_g211 * ( computeScreenPos17_g211 / (computeScreenPos17_g211).w ) ) ) * distance( ( float4( _WorldSpaceCameraPos , 0.0 ) - transform20_g211 ) , float4( 0,0,0,0 ) ) );
				float4 break26_g211 = _ScreenParams;
				float4 appendResult24_g211 = (float4(( break22_g211.x * ( break26_g211.x / break26_g211.y ) ) , break22_g211.y , break22_g211.z , break22_g211.w));
				#ifdef _USEADAPTIVESCREENUVSWARP_ON
				float4 staticSwitch1664 = appendResult24_g211;
				#else
				float4 staticSwitch1664 = ( ase_grabScreenPosNorm * float4( appendResult1847, 0.0 , 0.0 ) );
				#endif
				float mulTime1525 = _TimeParameters.x * _UVAnimationSpeedWarp;
				float temp_output_1527_0 = ( floor( ( mulTime1525 % 2.0 ) ) * 0.5 );
				float2 appendResult1531 = (float2(temp_output_1527_0 , temp_output_1527_0));
				#ifdef _USESCREENUVSWARP_ON
				float4 staticSwitch1514 = ( staticSwitch1664 + float4( appendResult1531, 0.0 , 0.0 ) + float4( temp_output_1522_0, 0.0 , 0.0 ) );
				#else
				float4 staticSwitch1514 = float4( texCoord1495, 0.0 , 0.0 );
				#endif
				float cos1489 = cos( radians( _WarpTextureRotation ) );
				float sin1489 = sin( radians( _WarpTextureRotation ) );
				float2 rotator1489 = mul( staticSwitch1514.xy - float2( 0.5,0.5 ) , float2x2( cos1489 , -sin1489 , sin1489 , cos1489 )) + float2( 0.5,0.5 );
				float BNLightWarpVector250 = ( _UseDiffuseWarp == 1.0 ? ( tex2D( _DiffuseWarpNoise, rotator1489 ).r * _WarpStrength ) : 0.0 );
				float2 uv_OcclusionMap = IN.ase_texcoord4.xy * _OcclusionMap_ST.xy + _OcclusionMap_ST.zw;
				float3 lerpResult1536 = lerp( float3(0,0,1) , UnpackNormalScale( tex2D( _BumpMap, uv_OcclusionMap ), 1.0f ) , _NormalMapStrength);
				float3 ase_worldTangent = IN.ase_texcoord6.xyz;
				float3 ase_worldNormal = IN.ase_texcoord7.xyz;
				float3 ase_worldBitangent = IN.ase_texcoord8.xyz;
				float3 tanToWorld0 = float3( ase_worldTangent.x, ase_worldBitangent.x, ase_worldNormal.x );
				float3 tanToWorld1 = float3( ase_worldTangent.y, ase_worldBitangent.y, ase_worldNormal.y );
				float3 tanToWorld2 = float3( ase_worldTangent.z, ase_worldBitangent.z, ase_worldNormal.z );
				float3 tanNormal1537 = lerpResult1536;
				float3 worldNormal1537 = normalize( float3(dot(tanToWorld0,tanNormal1537), dot(tanToWorld1,tanNormal1537), dot(tanToWorld2,tanNormal1537)) );
				float3 BNCurrentNormal1538 = worldNormal1537;
				float dotResult234 = dot( BNCurrentNormal1538 , _MainLightPosition.xyz );
				float BNNDotL233 = dotResult234;
				float3 temp_cast_7 = (( BNLightWarpVector250 + ( ( 1.0 - BNLightWarpVector250 ) * BNNDotL233 ) )).xxx;
				float localLightAttenuation1412 = ( 0.0 );
				float3 WorldPos1412 = WorldPosition;
				float DistanceAtten1412 = 0;
				float ShadowAtten1412 = 0;
				{
				    #if SHADOWS_SCREEN
				        half4 clipPos = TransformWorldToHClip(WorldPos1412);
				        half4 shadowCoord = ComputeScreenPos(clipPos);
				    #else
				        half4 shadowCoord = TransformWorldToShadowCoord(WorldPos1412);
				    #endif
				    Light mainLight = GetMainLight(shadowCoord);
				    DistanceAtten1412 = mainLight.distanceAttenuation;
				    ShadowAtten1412 = mainLight.shadowAttenuation;
				}
				float3 BNAttenuationColor244 = ( ase_lightColor.rgb * DistanceAtten1412 );
				float3 temp_output_260_0 = ( max( temp_cast_7 , float3(0,0,0) ) * BNAttenuationColor244 );
				float3 break3_g205 = temp_output_260_0;
				float temp_output_1188_0 = max( max( break3_g205.x , break3_g205.y ) , break3_g205.z );
				float smoothstepResult444 = smoothstep( ( temp_output_371_0 - 0.009 ) , temp_output_371_0 , temp_output_1188_0);
				float4 lerpResult1619 = lerp( _ShadowColor , temp_output_1885_0 , saturate( smoothstepResult444 ));
				float4 temp_cast_8 = (ShadowAtten1412).xxxx;
				float4 ShadowAtten1415 = ( 1.0 == _UseShadows ? temp_cast_8 : float4(1,1,1,1) );
				#ifdef _USEDIFFUSEWARPASOVERLAY_ON
				float4 staticSwitch1470 = saturate( ( ShadowAtten1415 + BNLightWarpVector250 + ( _DiffuseWarpBrightnessOffset * float4( -1,0,0,0 ) * ( 1.0 - ShadowAtten1415 ) ) ) );
				#else
				float4 staticSwitch1470 = ShadowAtten1415;
				#endif
				float4 lerpResult1626 = lerp( _ShadowColor , lerpResult1619 , staticSwitch1470);
				float2 appendResult356 = (float2(( _LightRampOffset + temp_output_1188_0 ) , 0.0));
				float2 temp_cast_10 = (0.01).xx;
				float2 temp_cast_11 = (0.98).xx;
				float2 clampResult358 = clamp( appendResult356 , temp_cast_10 , temp_cast_11 );
				float4 lerpResult1617 = lerp( tex2D( _LightRampTexture, float2( 0.02,0 ) ) , ( tex2D( _LightRampTexture, clampResult358 ) * temp_output_1885_0 ) , staticSwitch1470);
				float In1331 = pow( saturate( ( temp_output_1188_0 + ( _DiffusePosterizeOffset * -1.0 ) ) ) , _DiffusePosterizePower );
				float Steps1331 = round( _DiffusePosterizeSteps );
				float localPosterize1331 = Posterize1331( In1331 , Steps1331 );
				float4 lerpResult1629 = lerp( _ShadowColor , temp_output_1885_0 , localPosterize1331);
				float4 lerpResult1628 = lerp( _ShadowColor , lerpResult1629 , staticSwitch1470);
				#if defined( _USELIGHTRAMP_STEP )
				float4 staticSwitch372 = lerpResult1626;
				#elif defined( _USELIGHTRAMP_DIFFUSERAMP )
				float4 staticSwitch372 = lerpResult1617;
				#elif defined( _USELIGHTRAMP_POSTERIZE )
				float4 staticSwitch372 = lerpResult1628;
				#else
				float4 staticSwitch372 = lerpResult1626;
				#endif
				float3 WorldPosition1181 = WorldPosition;
				float3 WorldNormal1181 = BNCurrentNormal1538;
				float3 temp_cast_14 = (BNLightWarpVector250).xxx;
				float3 LightWrapVector1181 = temp_cast_14;
				float temp_output_1203_0 = ( 1.0 - ( (2.0 + (_AdditionalLightsAmount - 0.0) * (2.9 - 2.0) / (1.0 - 0.0)) + -2.0 ) );
				float temp_output_1241_0 = ( ( temp_output_1203_0 * _AdditionalLightsFaloff ) - 0.005 );
				float SMin1181 = temp_output_1241_0;
				float SMax1181 = temp_output_1203_0;
				float Faloff1181 = 0.0;
				half2 LightmapUV1_g213 = (IN.ase_texcoord4.zw*(unity_LightmapST).xy + (unity_LightmapST).zw);
				half4 localCalculateShadowMask1_g213 = CalculateShadowMask1_g213( LightmapUV1_g213 );
				float4 Shadowmask1839 = localCalculateShadowMask1_g213;
				float4 shadowmask1181 = Shadowmask1839;
				float4 ase_screenPosNorm = ScreenPos / ScreenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float2 ScreenUV1181 = (ase_screenPosNorm).xy;
				float3 localAdditionalLight1181 = AdditionalLight( WorldPosition1181 , WorldNormal1181 , LightWrapVector1181 , SMin1181 , SMax1181 , Faloff1181 , shadowmask1181 , ScreenUV1181 );
				#ifdef _USEADDITIONALLIGHTSDIFFUSE_ON
				float3 staticSwitch1143 = localAdditionalLight1181;
				#else
				float3 staticSwitch1143 = float3( 0,0,0 );
				#endif
				float3 AdditionalLightsDiffuse1144 = staticSwitch1143;
				float4 BNDiffuse391 = ( staticSwitch372 + float4( AdditionalLightsDiffuse1144 , 0.0 ) );
				#if defined( _OVERLAYMODE_NONE )
				float4 staticSwitch1266 = BNDiffuse391;
				#elif defined( _OVERLAYMODE_HAFTONE )
				float4 staticSwitch1266 = BNDiffuse391;
				#elif defined( _OVERLAYMODE_HATCHING )
				float4 staticSwitch1266 = BNDiffuse391;
				#else
				float4 staticSwitch1266 = BNDiffuse391;
				#endif
				float4 tex2DNode362 = tex2D( _MainTex, uv_OcclusionMap );
				float lerpResult1655 = lerp( 1.0 , tex2D( _OcclusionMap, uv_OcclusionMap ).r , _OcclusionStrength);
				float4 appendResult1656 = (float4(lerpResult1655 , lerpResult1655 , lerpResult1655 , 1.0));
				float4 MainTexture364 = ( _Color * tex2DNode362 * appendResult1656 );
				float3 bakedGI276 = ASEIndirectDiffuse( IN.lightmapUVOrVertexSH.xy, BNCurrentNormal1538);
				Light ase_mainLight = GetMainLight( ShadowCoords );
				MixRealtimeAndBakedGI(ase_mainLight, BNCurrentNormal1538, bakedGI276, half4(0,0,0,0));
				float IndirectLightStrength1221 = _IndirectLightStrength;
				float3 lerpResult692 = lerp( float3( 0,0,0 ) , bakedGI276 , IndirectLightStrength1221);
				float4 IndirectDiffuseLight1269 = ( MainTexture364 * float4( lerpResult692 , 0.0 ) );
				float4 BNFinalDiffuse239 = ( ( staticSwitch1266 * MainTexture364 ) + IndirectDiffuseLight1269 );
				float4 BNDiffuseNoAdditionalLights1554 = staticSwitch372;
				float4 lerpResult1462 = lerp( ase_lightColor , BNDiffuseNoAdditionalLights1554 , _HalftoneToonAffect);
				float4 temp_output_1225_0 = ( ( lerpResult1462 + float4( AdditionalLightsDiffuse1144 , 0.0 ) ) * MainTexture364 );
				float4 lerpResult1784 = lerp( temp_output_1225_0 , _HalftoneColor , _HalftoneColor.a);
				float3 break3_g210 = AdditionalLightsDiffuse1144;
				float3 temp_cast_20 = (BNNDotL233).xxx;
				float3 temp_cast_23 = (( BNLightWarpVector250 + ( ( 1.0 - BNLightWarpVector250 ) * BNNDotL233 ) )).xxx;
				#ifdef _DIFFUSEWARPAFFECTHALFTONE_ON
				float4 staticSwitch1498 = ( float4( temp_output_260_0 , 0.0 ) * ShadowAtten1415 );
				#else
				float4 staticSwitch1498 = ( float4( max( temp_cast_20 , float3(0,0,0) ) , 0.0 ) * float4( BNAttenuationColor244 , 0.0 ) * ShadowAtten1415 );
				#endif
				float4 CompleteDiffuseLight965 = ( max( max( break3_g210.x , break3_g210.y ) , break3_g210.z ) + staticSwitch1498 );
				float3 break3_g209 = CompleteDiffuseLight965.xyz;
				float smoothstepResult525 = smoothstep( _HalftoneEdgeOffset , ( _HalftoneEdgeOffset + _HalftoneSmoothness ) , max( max( break3_g209.x , break3_g209.y ) , break3_g209.z ));
				float2 temp_cast_26 = (_OverlayUVTilling).xx;
				float mulTime1287 = _TimeParameters.x * _UVScrollSpeed;
				float temp_output_1293_0 = radians( _UVSrcrollAngle );
				float2 appendResult1294 = (float2(cos( temp_output_1293_0 ) , sin( temp_output_1293_0 )));
				float2 temp_output_1288_0 = ( mulTime1287 * appendResult1294 );
				float2 texCoord478 = IN.ase_texcoord4.xy * temp_cast_26 + temp_output_1288_0;
				float2 appendResult1849 = (float2(( _OverlayUVTilling * ( _ScreenParams.x / _ScreenParams.y ) ) , _OverlayUVTilling));
				float4 unityObjectToClipPos2_g203 = TransformWorldToHClip(TransformObjectToWorld(IN.ase_texcoord5.xyz));
				float4 computeScreenPos3_g203 = ComputeScreenPos( unityObjectToClipPos2_g203 );
				float temp_output_29_0_g203 = ( _OverlayUVTilling / 4.0 );
				float4 unityObjectToClipPos13_g203 = TransformWorldToHClip(TransformObjectToWorld(float3(0,0,0)));
				float4 computeScreenPos17_g203 = ComputeScreenPos( unityObjectToClipPos13_g203 );
				float4 transform20_g203 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float4 break22_g203 = ( ( ( ( computeScreenPos3_g203 / (computeScreenPos3_g203).w ) * temp_output_29_0_g203 ) - ( temp_output_29_0_g203 * ( computeScreenPos17_g203 / (computeScreenPos17_g203).w ) ) ) * distance( ( float4( _WorldSpaceCameraPos , 0.0 ) - transform20_g203 ) , float4( 0,0,0,0 ) ) );
				float4 break26_g203 = _ScreenParams;
				float4 appendResult24_g203 = (float4(( break22_g203.x * ( break26_g203.x / break26_g203.y ) ) , break22_g203.y , break22_g203.z , break22_g203.w));
				#ifdef _USEADAPTIVESCREENUVS_ON
				float4 staticSwitch1661 = appendResult24_g203;
				#else
				float4 staticSwitch1661 = ( float4( appendResult1849, 0.0 , 0.0 ) * ase_grabScreenPosNorm );
				#endif
				float mulTime1278 = _TimeParameters.x * _UVAnimationSpeed;
				float temp_output_1281_0 = ( floor( ( mulTime1278 % 2.0 ) ) * 0.5 );
				#if defined( _OVERLAYMODE_NONE )
				float staticSwitch1277 = 0.0;
				#elif defined( _OVERLAYMODE_HAFTONE )
				float staticSwitch1277 = temp_output_1281_0;
				#elif defined( _OVERLAYMODE_HATCHING )
				float staticSwitch1277 = temp_output_1281_0;
				#else
				float staticSwitch1277 = 0.0;
				#endif
				float2 appendResult1803 = (float2(staticSwitch1277 , staticSwitch1277));
				#ifdef _USESCREENUVS_ON
				float4 staticSwitch488 = ( staticSwitch1661 + float4( appendResult1803, 0.0 , 0.0 ) + float4( temp_output_1288_0, 0.0 , 0.0 ) );
				#else
				float4 staticSwitch488 = float4( texCoord478, 0.0 , 0.0 );
				#endif
				float cos1047 = cos( radians( _OverlayRotation ) );
				float sin1047 = sin( radians( _OverlayRotation ) );
				float2 rotator1047 = mul( staticSwitch488.xy - float2( 0.5,0.5 ) , float2x2( cos1047 , -sin1047 , sin1047 , cos1047 )) + float2( 0.5,0.5 );
				float2 OverlayUVs1051 = rotator1047;
				float smoothstepResult1045 = smoothstep( ( ( _ShapeSmoothness * -0.5 ) + 0.5 ) , ( ( _ShapeSmoothness * 0.5 ) + 0.5 ) , ( 0.1 / ( ( _HalftoneThreshold / smoothstepResult525 ) * (0.0 + (tex2D( _HalftoneTexture, OverlayUVs1051 ).r - 0.0) * (1.0 - 0.0) / (1.0 - 0.0)) ) ));
				float4 lerpResult1774 = lerp( lerpResult1784 , temp_output_1225_0 , smoothstepResult1045);
				float4 Halftone1022 = ( lerpResult1774 + IndirectDiffuseLight1269 );
				#if defined( _OVERLAYMODE_NONE )
				float4 staticSwitch1231 = BNFinalDiffuse239;
				#elif defined( _OVERLAYMODE_HAFTONE )
				float4 staticSwitch1231 = Halftone1022;
				#elif defined( _OVERLAYMODE_HATCHING )
				float4 staticSwitch1231 = BNFinalDiffuse239;
				#else
				float4 staticSwitch1231 = BNFinalDiffuse239;
				#endif
				float3 WorldPosition1573 = WorldPosition;
				float3 WorldNormal1573 = BNCurrentNormal1538;
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - WorldPosition );
				ase_worldViewDir = normalize(ase_worldViewDir);
				float3 WorldView1573 = ase_worldViewDir;
				float3 SpecColor1573 = float3(1,1,1);
				float Smoothness638 = ( tex2D( _SpecGlossMap, uv_OcclusionMap ).r * _Glossiness );
				float Smoothness1573 = ( Smoothness638 * ( 2.0 - _AdditionalLightsSmoothnessMultiplier ) );
				float temp_output_588_0 = round( _SpecularPosterizeSteps );
				float Steps1573 = temp_output_588_0;
				float SpecFaloff1573 = _SpecularFaloff;
				float3 localAdditionalLightsSpecularMy1573 = AdditionalLightsSpecularMy( WorldPosition1573 , WorldNormal1573 , WorldView1573 , SpecColor1573 , Smoothness1573 , Steps1573 , SpecFaloff1573 );
				float3 normalizeResult222 = normalize( _MainLightPosition.xyz );
				float3 normalizeResult238 = normalize( ( normalizeResult222 + ase_worldViewDir ) );
				float3 BNHalfDirection265 = normalizeResult238;
				float dotResult252 = dot( BNHalfDirection265 , BNCurrentNormal1538 );
				float IN1578 = ( pow( max( dotResult252 , 0.0 ) , ( exp2( ( ( Smoothness638 * 10.0 * ( 2.0 - _SmoothnessMultiplier ) ) + -2.0 ) ) * 2.0 ) ) * ( _SmoothnessMultiplier == 0.0 ? 0.0 : 1.0 ) );
				float SpecFaloff1578 = _SpecularFaloff;
				float Steps1578 = temp_output_588_0;
				float localFaloffPosterize1578 = FaloffPosterize( IN1578 , SpecFaloff1578 , Steps1578 );
				float2 temp_cast_34 = (_SpecularMaskScale).xx;
				float2 texCoord1123 = IN.ase_texcoord4.xy * temp_cast_34 + float2( 0,0 );
				float2 appendResult1842 = (float2(( _SpecularMaskScale * ( _ScreenParams.x / _ScreenParams.y ) ) , _SpecularMaskScale));
				float4 unityObjectToClipPos2_g202 = TransformWorldToHClip(TransformObjectToWorld(IN.ase_texcoord5.xyz));
				float4 computeScreenPos3_g202 = ComputeScreenPos( unityObjectToClipPos2_g202 );
				float temp_output_29_0_g202 = ( _SpecularMaskScale / 4.0 );
				float4 unityObjectToClipPos13_g202 = TransformWorldToHClip(TransformObjectToWorld(float3(0,0,0)));
				float4 computeScreenPos17_g202 = ComputeScreenPos( unityObjectToClipPos13_g202 );
				float4 transform20_g202 = mul(GetObjectToWorldMatrix(),float4( 0,0,0,1 ));
				float4 break22_g202 = ( ( ( ( computeScreenPos3_g202 / (computeScreenPos3_g202).w ) * temp_output_29_0_g202 ) - ( temp_output_29_0_g202 * ( computeScreenPos17_g202 / (computeScreenPos17_g202).w ) ) ) * distance( ( float4( _WorldSpaceCameraPos , 0.0 ) - transform20_g202 ) , float4( 0,0,0,0 ) ) );
				float4 break26_g202 = _ScreenParams;
				float4 appendResult24_g202 = (float4(( break22_g202.x * ( break26_g202.x / break26_g202.y ) ) , break22_g202.y , break22_g202.z , break22_g202.w));
				#ifdef _USEADAPTIVEUVSSPECULAR_ON
				float4 staticSwitch1665 = appendResult24_g202;
				#else
				float4 staticSwitch1665 = ( float4( appendResult1842, 0.0 , 0.0 ) * ase_grabScreenPosNorm );
				#endif
				float mulTime1814 = _TimeParameters.x * _UVAnimationSpeedSpec;
				float temp_output_1817_0 = ( floor( ( mulTime1814 % 2.0 ) ) * 0.5 );
				#if defined( _OVERLAYMODE1_NONE )
				float staticSwitch1812 = temp_output_1817_0;
				#elif defined( _OVERLAYMODE1_HAFTONE )
				float staticSwitch1812 = temp_output_1817_0;
				#elif defined( _OVERLAYMODE1_HATCHING )
				float staticSwitch1812 = temp_output_1817_0;
				#else
				float staticSwitch1812 = temp_output_1817_0;
				#endif
				float2 appendResult1818 = (float2(staticSwitch1812 , staticSwitch1812));
				#ifdef _USESCREENUVSSPECULAR_ON
				float4 staticSwitch1132 = ( staticSwitch1665 + float4( appendResult1818, 0.0 , 0.0 ) );
				#else
				float4 staticSwitch1132 = float4( texCoord1123, 0.0 , 0.0 );
				#endif
				float mulTime1811 = _TimeParameters.x * _UVScrollSpeedSpec;
				float temp_output_1807_0 = radians( _UVSrcrollAngleSpec );
				float2 appendResult1808 = (float2(cos( temp_output_1807_0 ) , sin( temp_output_1807_0 )));
				float cos1124 = cos( radians( _SpecularMaskRotation ) );
				float sin1124 = sin( radians( _SpecularMaskRotation ) );
				float2 rotator1124 = mul( ( staticSwitch1132 + float4( ( mulTime1811 * appendResult1808 ), 0.0 , 0.0 ) ).xy - float2( 0.5,0.5 ) , float2x2( cos1124 , -sin1124 , sin1124 , cos1124 )) + float2( 0.5,0.5 );
				float2 SpecularUVs1125 = rotator1124;
				float4 tex2DNode441 = tex2D( _SpecularMaskTexture, SpecularUVs1125 );
				float temp_output_1634_0 = ( 1.0 - _HaltonePatternSize );
				float smoothstepResult1557 = smoothstep( ( temp_output_1634_0 - 0.3 ) , temp_output_1634_0 , tex2DNode441.r);
				float lerpResult446 = lerp( 1.0 , (( _StepHalftoneTexture )?( smoothstepResult1557 ):( tex2DNode441.r )) , _SpecularMaskStrength);
				#ifdef _INVERSEMASK_ON
				float staticSwitch1681 = ( 1.0 - lerpResult446 );
				#else
				float staticSwitch1681 = lerpResult446;
				#endif
				float SpecularMask902 = ( _UseSpecularMask == 1.0 ? staticSwitch1681 : 1.0 );
				float4 SpecularColor1388 = _SpecColor;
				#ifdef _USESPECULAR_ON
				float4 staticSwitch627 = max( ( float4( ( ( localAdditionalLightsSpecularMy1573 * _AdditionalLightsIntesity * ( _AdditionalLightsSmoothnessMultiplier == 0.0 ? 0.0 : 1.0 ) ) + ( ase_lightColor.rgb * _MainLightIntesity * localFaloffPosterize1578 ) ) , 0.0 ) * SpecularMask902 * Smoothness638 * SpecularColor1388 ) , float4( 0,0,0,0 ) );
				#else
				float4 staticSwitch627 = float4( 0,0,0,0 );
				#endif
				float4 BNspecularFinalColor243 = staticSwitch627;
				float4 lerpResult1216 = lerp( _HalftoneColor , lerpResult1462 , smoothstepResult1045);
				float4 HalftoneDiffuseShadowMask1236 = lerpResult1216;
				#if defined( _OVERLAYMODE_NONE )
				float4 staticSwitch1237 = BNDiffuse391;
				#elif defined( _OVERLAYMODE_HAFTONE )
				float4 staticSwitch1237 = HalftoneDiffuseShadowMask1236;
				#elif defined( _OVERLAYMODE_HATCHING )
				float4 staticSwitch1237 = BNDiffuse391;
				#else
				float4 staticSwitch1237 = BNDiffuse391;
				#endif
				float grayscale1797 = dot(staticSwitch1237.rgb, float3(0.299,0.587,0.114));
				float lerpResult695 = lerp( 1.0 , grayscale1797 , _SpecularShadowMask);
				half3 reflectVector618 = reflect( -ase_worldViewDir, BNCurrentNormal1538 );
				float3 indirectSpecular618 = GlossyEnvironmentReflection( reflectVector618, 1.0 - Smoothness638, 0.75 );
				#ifdef _USEENVIRONMENTREFLETION_ON
				float3 staticSwitch621 = ( indirectSpecular618 * _Strength * Smoothness638 );
				#else
				float3 staticSwitch621 = float3( 0,0,0 );
				#endif
				float3 IndirectSpecular1364 = staticSwitch621;
				float4 BNBlinnPhongLightning274 = ( staticSwitch1231 + ( BNspecularFinalColor243 * lerpResult695 ) + float4( IndirectSpecular1364 , 0.0 ) );
				float grayscale1648 = Luminance(BNDiffuseNoAdditionalLights1554.rgb);
				float4 lerpResult1650 = lerp( _RimShadowColor , _RimColor , grayscale1648);
				#if defined( _RIMSPLITCOLOR_NOSPLIT )
				float4 staticSwitch1646 = _RimColor;
				#elif defined( _RIMSPLITCOLOR_MULTIPLYWITHDIFFUSE )
				float4 staticSwitch1646 = ( _RimColor * BNDiffuseNoAdditionalLights1554 );
				#elif defined( _RIMSPLITCOLOR_USESECONDCOLOR )
				float4 staticSwitch1646 = lerpResult1650;
				#else
				float4 staticSwitch1646 = _RimColor;
				#endif
				float4 RimColor1642 = staticSwitch1646;
				float fresnelNdotV454 = dot( normalize( BNCurrentNormal1538 ), ase_worldViewDir );
				float fresnelNode454 = ( 0.0 + _RimThickness * pow( max( 1.0 - fresnelNdotV454 , 0.0001 ), _RimPower ) );
				float smoothstepResult462 = smoothstep( ( ( 1.0 - _RimSmoothness ) - 0.5 ) , 0.5 , fresnelNode454);
				float FresnelValue738 = smoothstepResult462;
				float dotResult704 = dot( ase_worldViewDir , _MainLightPosition.xyz );
				float smoothstepResult726 = smoothstep( _BacklightHardness , 0.5 , saturate( ( 1.0 - ( dotResult704 - ( ( _BacklightAmount * 2.0 ) + -2.0 ) ) ) ));
				float dotResult749 = dot( BNCurrentNormal1538 , _MainLightPosition.xyz );
				float temp_output_766_0 = ( 1.0 - ( _SideShineAmount - -0.3 ) );
				float dotResult745 = dot( ( ase_worldViewDir * -1.0 ) , _MainLightPosition.xyz );
				float clampResult753 = clamp( ( ( ( dotResult749 - temp_output_766_0 ) * 4.0 ) + ( dotResult745 - temp_output_766_0 ) ) , 0.0 , 1.1 );
				float smoothstepResult759 = smoothstep( _SideShineHardness , 0.5 , ( clampResult753 * FresnelValue738 ));
				#ifdef _USEBACKLIGHT_ON
				float staticSwitch732 = ( ( _BacklightIntensity * ( FresnelValue738 * smoothstepResult726 ) ) + ( _SideShineIntensity * smoothstepResult759 ) );
				#else
				float staticSwitch732 = FresnelValue738;
				#endif
				#ifdef _USERIMLIGHT_ON
				float staticSwitch464 = staticSwitch732;
				#else
				float staticSwitch464 = 0.0;
				#endif
				float RimLight460 = staticSwitch464;
				float4 lerpResult1635 = lerp( BNBlinnPhongLightning274 , RimColor1642 , RimLight460);
				float4 Emission680 = ( _UseEmission == 1.0 ? ( tex2D( _EmissionMap, uv_OcclusionMap ) * _EmissionColor ) : float4( 0,0,0,0 ) );
				float4 temp_output_282_0 = ( lerpResult1635 + Emission680 );
				float3 IndirectHatching1466 = lerpResult692;
				float3 lerpResult1468 = lerp( float3( 0,0,0 ) , IndirectHatching1466 , _IndirectLightAffectOnHatch);
				float4 temp_output_1270_0 = ( _HatchingColor + float4( lerpResult1468 , 0.0 ) );
				float4 lerpResult1264 = lerp( BNBlinnPhongLightning274 , temp_output_1270_0 , _HatchingColor.a);
				float2 _uv476 = OverlayUVs1051;
				float3 break3_g204 = ( RimLight460 + BNDiffuse391 + Emission680 + BNspecularFinalColor243 ).rgb;
				float temp_output_1064_0 = (( _Darken * -2.0 ) + (max( max( break3_g204.x , break3_g204.y ) , break3_g204.z ) - 0.0) * (( ( _Lighten * 2.0 ) + 1.0 ) - ( _Darken * -2.0 )) / (1.0 - 0.0));
				float color476 = temp_output_1064_0;
				sampler2D _Hatch0476 = _Hatch1;
				sampler2D _Hatch1476 = _Hatch2;
				float3 localHatching476 = Hatching476( _uv476 , color476 , _Hatch0476 , _Hatch1476 );
				float2 _uv491 = OverlayUVs1051;
				float _intensity491 = temp_output_1064_0;
				sampler2D _Hatch0491 = _Hatch1;
				sampler2D _Hatch1491 = _Hatch2;
				float _dist491 = distance( _WorldSpaceCameraPos , WorldPosition );
				float _MaxScaleDependingOnCamera491 = _MaxScaleDependingOnCamera;
				float3 localHatchingConstantScale491 = HatchingConstantScale491( _uv491 , _intensity491 , _Hatch0491 , _Hatch1491 , _dist491 , _MaxScaleDependingOnCamera491 );
				#ifdef _USEHATCHINGCONSTANTSCALE_ON
				float3 staticSwitch490 = localHatchingConstantScale491;
				#else
				float3 staticSwitch490 = localHatching476;
				#endif
				float3 Hatching1025 = saturate( staticSwitch490 );
				float4 lerpResult1260 = lerp( lerpResult1264 , BNBlinnPhongLightning274 , Hatching1025.x);
				float2 appendResult1857 = (float2(( _PaperTilling * ( _ScreenParams.x / _ScreenParams.y ) ) , _PaperTilling));
				float4 tex2DNode1795 = tex2D( _PaperTexture, ( ase_grabScreenPosNorm * float4( appendResult1857, 0.0 , 0.0 ) ).xy );
				float4 lerpResult1254 = lerp( temp_output_1270_0 , tex2DNode1795 , Hatching1025.x);
				float4 lerpResult1257 = lerp( lerpResult1254 , tex2DNode1795 , ( 1.0 - _HatchingColor.a ));
				#ifdef _USEPURESKETCH_ON
				float4 staticSwitch1258 = lerpResult1257;
				#else
				float4 staticSwitch1258 = lerpResult1260;
				#endif
				float4 lerpResult1637 = lerp( staticSwitch1258 , RimColor1642 , RimLight460);
				#if defined( _OVERLAYMODE_NONE )
				float4 staticSwitch1024 = temp_output_282_0;
				#elif defined( _OVERLAYMODE_HAFTONE )
				float4 staticSwitch1024 = temp_output_282_0;
				#elif defined( _OVERLAYMODE_HATCHING )
				float4 staticSwitch1024 = ( lerpResult1637 + Emission680 );
				#else
				float4 staticSwitch1024 = temp_output_282_0;
				#endif
				
				float temp_output_673_0 = ( MainTexture364.a * 1.0 );
				
				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float3 Color = staticSwitch1024.rgb;
				float Alpha = temp_output_673_0;
				float AlphaClipThreshold = _Cutoff;
				float AlphaClipThresholdShadow = 0.5;

				#ifdef _ALPHATEST_ON
					clip( Alpha - AlphaClipThreshold );
				#endif

				#if defined(_DBUFFER)
					ApplyDecalToBaseColor(IN.positionCS, Color);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.positionCS );
				#endif

				#ifdef ASE_FOG
					Color = MixFog( Color, IN.fogFactor );
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4( EncodeMeshRenderingLayer( renderingLayers ), 0, 0, 0 );
				#endif

				return half4( Color, Alpha );
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual
			AlphaToMask Off
			ColorMask 0

			HLSLPROGRAM

			

			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define _ALPHATEST_ON 1
			#define ASE_SRP_VERSION 140011


			

			#pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_SHADOWCASTER

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			

			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 positionWS : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _ShadowColor;
			float4 _RimShadowColor;
			float4 _HalftoneColor;
			float4 _RimColor;
			float4 _Color;
			float4 _EmissionColor;
			float4 _OcclusionMap_ST;
			float4 _SpecColor;
			float4 _HatchingColor;
			float4 _OutlineColor;
			float _SmoothnessMultiplier;
			float _UseSpecularMask;
			float _MainLightIntesity;
			float _StepHalftoneTexture;
			float _SpecularMaskScale;
			float _UVAnimationSpeedSpec;
			float _AdditionalLightsIntesity;
			float _UVScrollSpeedSpec;
			float _UVSrcrollAngleSpec;
			float _SpecularMaskRotation;
			float _HaltonePatternSize;
			float _SpecularMaskStrength;
			float _SpecularFaloff;
			float _RampDiffuseTextureLoaded;
			float _RimSmoothness;
			float _Strength;
			float _MaxScaleDependingOnCamera;
			float _Lighten;
			float _Darken;
			float _IndirectLightAffectOnHatch;
			float _UseEmission;
			float _SideShineAmount;
			float _SpecularShadowMask;
			float _SideShineHardness;
			float _BacklightAmount;
			float _BacklightHardness;
			float _BacklightIntensity;
			float _RimPower;
			float _RimThickness;
			float _SpecularPosterizeSteps;
			float _SideShineIntensity;
			float _AdditionalLightsSmoothnessMultiplier;
			float _UVAnimationSpeed;
			float _OverlayRotation;
			float _WarpStrength;
			float _WarpTextureRotation;
			float _UVAnimationSpeedWarp;
			float _UVSrcrollAngleWarp;
			float _UVScrollSpeedWarp;
			float _WarpTextureScale;
			float _UseDiffuseWarp;
			float _StepOffset;
			float _OcclusionStrength;
			float _OutlineTextureStrength;
			float _Thicnkess;
			float _OutlineNoiseIntesity;
			float _OutlineNoiseScale;
			float _OutlineDynamicSpeed;
			float _AdaptiveThicnkess;
			float _NormalMapStrength;
			float _Glossiness;
			float _UseShadows;
			float _LightRampOffset;
			float _PaperTilling;
			float _UVSrcrollAngle;
			float _UVScrollSpeed;
			float _OverlayUVTilling;
			float _HalftoneSmoothness;
			float _HalftoneEdgeOffset;
			float _HalftoneThreshold;
			float _ShapeSmoothness;
			float _HalftoneToonAffect;
			float _IndirectLightStrength;
			float _AdditionalLightsFaloff;
			float _AdditionalLightsAmount;
			float _DiffusePosterizeSteps;
			float _DiffusePosterizePower;
			float _DiffusePosterizeOffset;
			float _DiffuseWarpBrightnessOffset;
			float _Cutoff;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			sampler2D _MainTex;
			sampler2D _OcclusionMap;


			
			float3 _LightDirection;
			float3 _LightPosition;

			VertexOutput VertexFunction( VertexInput v )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				float3 positionWS = TransformObjectToWorld( v.positionOS.xyz );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.positionWS = positionWS;
				#endif

				float3 normalWS = TransformObjectToWorldDir( v.normalOS );

				#if _CASTING_PUNCTUAL_LIGHT_SHADOW
					float3 lightDirectionWS = normalize(_LightPosition - positionWS);
				#else
					float3 lightDirectionWS = _LightDirection;
				#endif

				float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

				#if UNITY_REVERSED_Z
					positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
				#else
					positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.positionCS = positionCS;

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 WorldPosition = IN.positionWS;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float2 uv_OcclusionMap = IN.ase_texcoord2.xy * _OcclusionMap_ST.xy + _OcclusionMap_ST.zw;
				float4 tex2DNode362 = tex2D( _MainTex, uv_OcclusionMap );
				float lerpResult1655 = lerp( 1.0 , tex2D( _OcclusionMap, uv_OcclusionMap ).r , _OcclusionStrength);
				float4 appendResult1656 = (float4(lerpResult1655 , lerpResult1655 , lerpResult1655 , 1.0));
				float4 MainTexture364 = ( _Color * tex2DNode362 * appendResult1656 );
				float temp_output_673_0 = ( MainTexture364.a * 1.0 );
				

				float Alpha = temp_output_673_0;
				float AlphaClipThreshold = _Cutoff;
				float AlphaClipThresholdShadow = 0.5;

				#ifdef _ALPHATEST_ON
					#ifdef _ALPHATEST_SHADOW_ON
						clip(Alpha - AlphaClipThresholdShadow);
					#else
						clip(Alpha - AlphaClipThreshold);
					#endif
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.positionCS );
				#endif

				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask 0
			AlphaToMask Off

			HLSLPROGRAM

			

			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#define _ALPHATEST_ON 1
			#define ASE_SRP_VERSION 140011


			

			#pragma vertex vert
			#pragma fragment frag

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			

			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 positionWS : TEXCOORD1;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD2;
				#endif
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _ShadowColor;
			float4 _RimShadowColor;
			float4 _HalftoneColor;
			float4 _RimColor;
			float4 _Color;
			float4 _EmissionColor;
			float4 _OcclusionMap_ST;
			float4 _SpecColor;
			float4 _HatchingColor;
			float4 _OutlineColor;
			float _SmoothnessMultiplier;
			float _UseSpecularMask;
			float _MainLightIntesity;
			float _StepHalftoneTexture;
			float _SpecularMaskScale;
			float _UVAnimationSpeedSpec;
			float _AdditionalLightsIntesity;
			float _UVScrollSpeedSpec;
			float _UVSrcrollAngleSpec;
			float _SpecularMaskRotation;
			float _HaltonePatternSize;
			float _SpecularMaskStrength;
			float _SpecularFaloff;
			float _RampDiffuseTextureLoaded;
			float _RimSmoothness;
			float _Strength;
			float _MaxScaleDependingOnCamera;
			float _Lighten;
			float _Darken;
			float _IndirectLightAffectOnHatch;
			float _UseEmission;
			float _SideShineAmount;
			float _SpecularShadowMask;
			float _SideShineHardness;
			float _BacklightAmount;
			float _BacklightHardness;
			float _BacklightIntensity;
			float _RimPower;
			float _RimThickness;
			float _SpecularPosterizeSteps;
			float _SideShineIntensity;
			float _AdditionalLightsSmoothnessMultiplier;
			float _UVAnimationSpeed;
			float _OverlayRotation;
			float _WarpStrength;
			float _WarpTextureRotation;
			float _UVAnimationSpeedWarp;
			float _UVSrcrollAngleWarp;
			float _UVScrollSpeedWarp;
			float _WarpTextureScale;
			float _UseDiffuseWarp;
			float _StepOffset;
			float _OcclusionStrength;
			float _OutlineTextureStrength;
			float _Thicnkess;
			float _OutlineNoiseIntesity;
			float _OutlineNoiseScale;
			float _OutlineDynamicSpeed;
			float _AdaptiveThicnkess;
			float _NormalMapStrength;
			float _Glossiness;
			float _UseShadows;
			float _LightRampOffset;
			float _PaperTilling;
			float _UVSrcrollAngle;
			float _UVScrollSpeed;
			float _OverlayUVTilling;
			float _HalftoneSmoothness;
			float _HalftoneEdgeOffset;
			float _HalftoneThreshold;
			float _ShapeSmoothness;
			float _HalftoneToonAffect;
			float _IndirectLightStrength;
			float _AdditionalLightsFaloff;
			float _AdditionalLightsAmount;
			float _DiffusePosterizeSteps;
			float _DiffusePosterizePower;
			float _DiffusePosterizeOffset;
			float _DiffuseWarpBrightnessOffset;
			float _Cutoff;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			sampler2D _MainTex;
			sampler2D _OcclusionMap;


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord3.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( v.positionOS.xyz );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.positionWS = vertexInput.positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.positionCS = vertexInput.positionCS;
				o.clipPosV = vertexInput.positionCS;
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.positionWS;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				float4 ClipPos = IN.clipPosV;
				float4 ScreenPos = ComputeScreenPos( IN.clipPosV );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float2 uv_OcclusionMap = IN.ase_texcoord3.xy * _OcclusionMap_ST.xy + _OcclusionMap_ST.zw;
				float4 tex2DNode362 = tex2D( _MainTex, uv_OcclusionMap );
				float lerpResult1655 = lerp( 1.0 , tex2D( _OcclusionMap, uv_OcclusionMap ).r , _OcclusionStrength);
				float4 appendResult1656 = (float4(lerpResult1655 , lerpResult1655 , lerpResult1655 , 1.0));
				float4 MainTexture364 = ( _Color * tex2DNode362 * appendResult1656 );
				float temp_output_673_0 = ( MainTexture364.a * 1.0 );
				

				float Alpha = temp_output_673_0;
				float AlphaClipThreshold = _Cutoff;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.positionCS );
				#endif
				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "SceneSelectionPass"
			Tags { "LightMode"="SceneSelectionPass" }

			Cull Off
			AlphaToMask Off

			HLSLPROGRAM

			

			#define _ALPHATEST_ON 1
			#define ASE_SRP_VERSION 140011


			

			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			
			#if ASE_SRP_VERSION >=140010
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#endif
		

			

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			

			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _ShadowColor;
			float4 _RimShadowColor;
			float4 _HalftoneColor;
			float4 _RimColor;
			float4 _Color;
			float4 _EmissionColor;
			float4 _OcclusionMap_ST;
			float4 _SpecColor;
			float4 _HatchingColor;
			float4 _OutlineColor;
			float _SmoothnessMultiplier;
			float _UseSpecularMask;
			float _MainLightIntesity;
			float _StepHalftoneTexture;
			float _SpecularMaskScale;
			float _UVAnimationSpeedSpec;
			float _AdditionalLightsIntesity;
			float _UVScrollSpeedSpec;
			float _UVSrcrollAngleSpec;
			float _SpecularMaskRotation;
			float _HaltonePatternSize;
			float _SpecularMaskStrength;
			float _SpecularFaloff;
			float _RampDiffuseTextureLoaded;
			float _RimSmoothness;
			float _Strength;
			float _MaxScaleDependingOnCamera;
			float _Lighten;
			float _Darken;
			float _IndirectLightAffectOnHatch;
			float _UseEmission;
			float _SideShineAmount;
			float _SpecularShadowMask;
			float _SideShineHardness;
			float _BacklightAmount;
			float _BacklightHardness;
			float _BacklightIntensity;
			float _RimPower;
			float _RimThickness;
			float _SpecularPosterizeSteps;
			float _SideShineIntensity;
			float _AdditionalLightsSmoothnessMultiplier;
			float _UVAnimationSpeed;
			float _OverlayRotation;
			float _WarpStrength;
			float _WarpTextureRotation;
			float _UVAnimationSpeedWarp;
			float _UVSrcrollAngleWarp;
			float _UVScrollSpeedWarp;
			float _WarpTextureScale;
			float _UseDiffuseWarp;
			float _StepOffset;
			float _OcclusionStrength;
			float _OutlineTextureStrength;
			float _Thicnkess;
			float _OutlineNoiseIntesity;
			float _OutlineNoiseScale;
			float _OutlineDynamicSpeed;
			float _AdaptiveThicnkess;
			float _NormalMapStrength;
			float _Glossiness;
			float _UseShadows;
			float _LightRampOffset;
			float _PaperTilling;
			float _UVSrcrollAngle;
			float _UVScrollSpeed;
			float _OverlayUVTilling;
			float _HalftoneSmoothness;
			float _HalftoneEdgeOffset;
			float _HalftoneThreshold;
			float _ShapeSmoothness;
			float _HalftoneToonAffect;
			float _IndirectLightStrength;
			float _AdditionalLightsFaloff;
			float _AdditionalLightsAmount;
			float _DiffusePosterizeSteps;
			float _DiffusePosterizePower;
			float _DiffusePosterizeOffset;
			float _DiffuseWarpBrightnessOffset;
			float _Cutoff;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			sampler2D _MainTex;
			sampler2D _OcclusionMap;


			
			int _ObjectId;
			int _PassValue;

			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				float3 positionWS = TransformObjectToWorld( v.positionOS.xyz );

				o.positionCS = TransformWorldToHClip(positionWS);

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				float2 uv_OcclusionMap = IN.ase_texcoord.xy * _OcclusionMap_ST.xy + _OcclusionMap_ST.zw;
				float4 tex2DNode362 = tex2D( _MainTex, uv_OcclusionMap );
				float lerpResult1655 = lerp( 1.0 , tex2D( _OcclusionMap, uv_OcclusionMap ).r , _OcclusionStrength);
				float4 appendResult1656 = (float4(lerpResult1655 , lerpResult1655 , lerpResult1655 , 1.0));
				float4 MainTexture364 = ( _Color * tex2DNode362 * appendResult1656 );
				float temp_output_673_0 = ( MainTexture364.a * 1.0 );
				

				surfaceDescription.Alpha = temp_output_673_0;
				surfaceDescription.AlphaClipThreshold = _Cutoff;

				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = surfaceDescription.AlphaClipThreshold;
					#endif
					clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = half4(_ObjectId, _PassValue, 1.0, 1.0);
				return outColor;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "ScenePickingPass"
			Tags { "LightMode"="Picking" }

			AlphaToMask Off

			HLSLPROGRAM

			

			#define _ALPHATEST_ON 1
			#define ASE_SRP_VERSION 140011


			

			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT

			#define SHADERPASS SHADERPASS_DEPTHONLY

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			
			#if ASE_SRP_VERSION >=140010
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#endif
		

			

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			

			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _ShadowColor;
			float4 _RimShadowColor;
			float4 _HalftoneColor;
			float4 _RimColor;
			float4 _Color;
			float4 _EmissionColor;
			float4 _OcclusionMap_ST;
			float4 _SpecColor;
			float4 _HatchingColor;
			float4 _OutlineColor;
			float _SmoothnessMultiplier;
			float _UseSpecularMask;
			float _MainLightIntesity;
			float _StepHalftoneTexture;
			float _SpecularMaskScale;
			float _UVAnimationSpeedSpec;
			float _AdditionalLightsIntesity;
			float _UVScrollSpeedSpec;
			float _UVSrcrollAngleSpec;
			float _SpecularMaskRotation;
			float _HaltonePatternSize;
			float _SpecularMaskStrength;
			float _SpecularFaloff;
			float _RampDiffuseTextureLoaded;
			float _RimSmoothness;
			float _Strength;
			float _MaxScaleDependingOnCamera;
			float _Lighten;
			float _Darken;
			float _IndirectLightAffectOnHatch;
			float _UseEmission;
			float _SideShineAmount;
			float _SpecularShadowMask;
			float _SideShineHardness;
			float _BacklightAmount;
			float _BacklightHardness;
			float _BacklightIntensity;
			float _RimPower;
			float _RimThickness;
			float _SpecularPosterizeSteps;
			float _SideShineIntensity;
			float _AdditionalLightsSmoothnessMultiplier;
			float _UVAnimationSpeed;
			float _OverlayRotation;
			float _WarpStrength;
			float _WarpTextureRotation;
			float _UVAnimationSpeedWarp;
			float _UVSrcrollAngleWarp;
			float _UVScrollSpeedWarp;
			float _WarpTextureScale;
			float _UseDiffuseWarp;
			float _StepOffset;
			float _OcclusionStrength;
			float _OutlineTextureStrength;
			float _Thicnkess;
			float _OutlineNoiseIntesity;
			float _OutlineNoiseScale;
			float _OutlineDynamicSpeed;
			float _AdaptiveThicnkess;
			float _NormalMapStrength;
			float _Glossiness;
			float _UseShadows;
			float _LightRampOffset;
			float _PaperTilling;
			float _UVSrcrollAngle;
			float _UVScrollSpeed;
			float _OverlayUVTilling;
			float _HalftoneSmoothness;
			float _HalftoneEdgeOffset;
			float _HalftoneThreshold;
			float _ShapeSmoothness;
			float _HalftoneToonAffect;
			float _IndirectLightStrength;
			float _AdditionalLightsFaloff;
			float _AdditionalLightsAmount;
			float _DiffusePosterizeSteps;
			float _DiffusePosterizePower;
			float _DiffusePosterizeOffset;
			float _DiffuseWarpBrightnessOffset;
			float _Cutoff;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			sampler2D _MainTex;
			sampler2D _OcclusionMap;


			
			float4 _SelectionID;

			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				float3 positionWS = TransformObjectToWorld( v.positionOS.xyz );
				o.positionCS = TransformWorldToHClip(positionWS);
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				float2 uv_OcclusionMap = IN.ase_texcoord.xy * _OcclusionMap_ST.xy + _OcclusionMap_ST.zw;
				float4 tex2DNode362 = tex2D( _MainTex, uv_OcclusionMap );
				float lerpResult1655 = lerp( 1.0 , tex2D( _OcclusionMap, uv_OcclusionMap ).r , _OcclusionStrength);
				float4 appendResult1656 = (float4(lerpResult1655 , lerpResult1655 , lerpResult1655 , 1.0));
				float4 MainTexture364 = ( _Color * tex2DNode362 * appendResult1656 );
				float temp_output_673_0 = ( MainTexture364.a * 1.0 );
				

				surfaceDescription.Alpha = temp_output_673_0;
				surfaceDescription.AlphaClipThreshold = _Cutoff;

				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = surfaceDescription.AlphaClipThreshold;
					#endif
					clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = 0;
				outColor = _SelectionID;

				return outColor;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthNormals"
			Tags { "LightMode"="DepthNormalsOnly" }

			ZTest LEqual
			ZWrite On

			HLSLPROGRAM

			

        	#pragma multi_compile_instancing
        	#pragma multi_compile _ LOD_FADE_CROSSFADE
        	#define _ALPHATEST_ON 1
        	#define ASE_SRP_VERSION 140011


			

        	#pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

			

			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define VARYINGS_NEED_NORMAL_WS

			#define SHADERPASS SHADERPASS_DEPTHNORMALSONLY

			
            #if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#endif
		

			
			#if ASE_SRP_VERSION >=140007
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#endif
		

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			
			#if ASE_SRP_VERSION >=140010
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
			#endif
		

			

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

            #if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			

			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				float3 normalWS : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _ShadowColor;
			float4 _RimShadowColor;
			float4 _HalftoneColor;
			float4 _RimColor;
			float4 _Color;
			float4 _EmissionColor;
			float4 _OcclusionMap_ST;
			float4 _SpecColor;
			float4 _HatchingColor;
			float4 _OutlineColor;
			float _SmoothnessMultiplier;
			float _UseSpecularMask;
			float _MainLightIntesity;
			float _StepHalftoneTexture;
			float _SpecularMaskScale;
			float _UVAnimationSpeedSpec;
			float _AdditionalLightsIntesity;
			float _UVScrollSpeedSpec;
			float _UVSrcrollAngleSpec;
			float _SpecularMaskRotation;
			float _HaltonePatternSize;
			float _SpecularMaskStrength;
			float _SpecularFaloff;
			float _RampDiffuseTextureLoaded;
			float _RimSmoothness;
			float _Strength;
			float _MaxScaleDependingOnCamera;
			float _Lighten;
			float _Darken;
			float _IndirectLightAffectOnHatch;
			float _UseEmission;
			float _SideShineAmount;
			float _SpecularShadowMask;
			float _SideShineHardness;
			float _BacklightAmount;
			float _BacklightHardness;
			float _BacklightIntensity;
			float _RimPower;
			float _RimThickness;
			float _SpecularPosterizeSteps;
			float _SideShineIntensity;
			float _AdditionalLightsSmoothnessMultiplier;
			float _UVAnimationSpeed;
			float _OverlayRotation;
			float _WarpStrength;
			float _WarpTextureRotation;
			float _UVAnimationSpeedWarp;
			float _UVSrcrollAngleWarp;
			float _UVScrollSpeedWarp;
			float _WarpTextureScale;
			float _UseDiffuseWarp;
			float _StepOffset;
			float _OcclusionStrength;
			float _OutlineTextureStrength;
			float _Thicnkess;
			float _OutlineNoiseIntesity;
			float _OutlineNoiseScale;
			float _OutlineDynamicSpeed;
			float _AdaptiveThicnkess;
			float _NormalMapStrength;
			float _Glossiness;
			float _UseShadows;
			float _LightRampOffset;
			float _PaperTilling;
			float _UVSrcrollAngle;
			float _UVScrollSpeed;
			float _OverlayUVTilling;
			float _HalftoneSmoothness;
			float _HalftoneEdgeOffset;
			float _HalftoneThreshold;
			float _ShapeSmoothness;
			float _HalftoneToonAffect;
			float _IndirectLightStrength;
			float _AdditionalLightsFaloff;
			float _AdditionalLightsAmount;
			float _DiffusePosterizeSteps;
			float _DiffusePosterizePower;
			float _DiffusePosterizeOffset;
			float _DiffuseWarpBrightnessOffset;
			float _Cutoff;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			sampler2D _MainTex;
			sampler2D _OcclusionMap;


			
			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( v.positionOS.xyz );

				o.positionCS = vertexInput.positionCS;
				o.clipPosV = vertexInput.positionCS;
				o.normalWS = TransformObjectToWorldNormal( v.normalOS );
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			void frag( VertexOutput IN
				, out half4 outNormalWS : SV_Target0
			#ifdef _WRITE_RENDERING_LAYERS
				, out float4 outRenderingLayers : SV_Target1
			#endif
				 )
			{
				float4 ClipPos = IN.clipPosV;
				float4 ScreenPos = ComputeScreenPos( IN.clipPosV );

				float2 uv_OcclusionMap = IN.ase_texcoord2.xy * _OcclusionMap_ST.xy + _OcclusionMap_ST.zw;
				float4 tex2DNode362 = tex2D( _MainTex, uv_OcclusionMap );
				float lerpResult1655 = lerp( 1.0 , tex2D( _OcclusionMap, uv_OcclusionMap ).r , _OcclusionStrength);
				float4 appendResult1656 = (float4(lerpResult1655 , lerpResult1655 , lerpResult1655 , 1.0));
				float4 MainTexture364 = ( _Color * tex2DNode362 * appendResult1656 );
				float temp_output_673_0 = ( MainTexture364.a * 1.0 );
				

				float Alpha = temp_output_673_0;
				float AlphaClipThreshold = _Cutoff;

				#if _ALPHATEST_ON
					clip( Alpha - AlphaClipThreshold );
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.positionCS );
				#endif

				#if defined(_GBUFFER_NORMALS_OCT)
					float3 normalWS = normalize(IN.normalWS);
					float2 octNormalWS = PackNormalOctQuadEncode(normalWS);           // values between [-1, +1], must use fp32 on some platforms
					float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);   // values between [ 0,  1]
					half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);      // values between [ 0,  1]
					outNormalWS = half4(packedNormalWS, 0.0);
				#else
					float3 normalWS = IN.normalWS;
					outNormalWS = half4(NormalizeNormalPerPixel(normalWS), 0.0);
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
				#endif
			}

			ENDHLSL
		}

	
	}
	
	CustomEditor "ToonProEditor"
	FallBack "Hidden/Shader Graph/FallbackError"
	
	Fallback Off
}
/*ASEBEGIN
Version=19501
Node;AmplifyShaderEditor.CommentaryNode;678;1058.121,1503.775;Inherit;False;1482.025;1686.722;Comment;27;636;659;686;679;685;658;680;638;682;677;364;204;362;1388;1376;1536;1537;1538;1539;1540;1541;1651;1652;1653;1655;1656;1669;Inputs;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1669;1124.039,1766.173;Inherit;False;0;1651;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;1652;1503.492,2306.291;Inherit;False;Property;_OcclusionStrength;Occlusion Strength ;108;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1651;1432.063,2063.587;Inherit;True;Property;_OcclusionMap;Occlusion Map;107;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.LerpOp;1655;1780.563,2131.611;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;1656;1932.025,2227.229;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;362;1404.155,1877.507;Inherit;True;Property;_MainTex;Albedo Texture;11;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ColorNode;204;1971.248,1808.104;Inherit;False;Property;_Color;Color;8;0;Create;True;0;0;0;False;0;False;0.6792453,0.6792453,0.6792453,1;0,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1653;2040.811,2106.22;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;364;2184.66,1995.724;Inherit;False;MainTexture;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;277;-8712.34,-525.5221;Inherit;False;6331.035;6906.721;;14;217;1122;386;901;390;213;385;215;1208;216;219;1829;1840;1845;BlinnPhong;0.2631274,0.6002151,0.6886792,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;670;46.10808,1250.098;Inherit;False;364;MainTexture;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;504;-2238.51,2542.837;Inherit;False;2567.819;1176.527;;24;1053;1069;1111;490;1066;482;503;476;491;1025;275;498;1121;1070;1120;1119;499;492;1067;1064;481;1187;1265;1460;Hatching;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;1829;-7940.005,2850.515;Inherit;False;285;161;do not delete;1;1828;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;465;423.3723,-1099.152;Inherit;False;3204.799;1529.321;;58;760;766;740;764;741;749;762;753;745;763;742;767;748;737;759;743;744;751;747;765;1644;1647;456;1645;1646;1642;739;732;731;726;712;715;457;537;463;718;704;454;467;738;708;702;464;462;719;460;716;755;721;757;758;705;458;756;466;727;1648;1650;Rim;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;213;-4675.104,-420.4454;Inherit;False;1762.018;982.026;;14;1231;274;696;694;270;695;259;693;271;1232;1237;1238;1366;1797;Diffuse + Specular;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;386;-8655.438,1520.64;Inherit;False;4310.062;1258.77;;78;1451;372;361;1360;1331;1416;1157;1504;391;1554;267;1276;370;1355;1087;1188;1418;248;262;1356;1502;264;965;260;1501;240;251;356;1505;223;1417;358;1329;1508;1211;360;371;1362;359;1359;389;1500;1274;249;444;1357;1358;1330;445;1498;1599;1600;1604;1605;1509;1611;1485;1470;1483;1511;1486;1510;1617;1618;1619;1622;1623;1625;1626;1628;1629;1680;1683;1684;1880;1881;1885;1888;Main Light Diffuse Mode;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;216;-6310.873,-429.0269;Inherit;False;698.8475;398.5884;;4;1542;269;233;234;NDotL;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;64;-1880.329,-546.7816;Inherit;False;1478.969;931.5795;;24;593;596;50;592;57;52;54;53;55;51;56;590;60;62;58;591;59;598;600;599;601;602;604;606;Outline;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;901;-4308.089,1916.059;Inherit;False;1917.12;636.1124;;14;1681;440;1561;1631;1213;1632;1634;902;1557;446;437;441;438;1682;Main Specular Mask;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;1461;-2166.924,1205.188;Inherit;False;1696.135;1222.97;;24;1790;1556;1249;1637;1555;1270;1469;1264;1026;1260;1468;1467;1256;1253;1254;1258;1257;1789;1791;1795;1856;1857;1858;1855;Hatching Implementation;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;385;-8602.182,211.8337;Inherit;False;2738.806;952.1386;Comment;34;1531;1529;1528;1527;1526;1525;1517;1521;1520;1519;1518;1524;1523;1522;381;1495;1530;1514;1488;1496;1492;1489;378;375;379;382;384;250;1664;1668;1794;1770;1846;1847;Diffuse Warp;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;387;1643.036,755.7413;Inherit;False;338;166;;1;373;Editor Properties;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;390;-4258.111,704.8892;Inherit;False;1678.375;818.0153;;14;229;263;1269;692;642;392;1221;691;276;581;239;1266;1363;1466;Final Diffuse;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;1021;-2244.832,3912.274;Inherit;False;4020.123;1130.029;;39;1022;1216;1226;1225;1222;1220;1217;1042;1186;1045;508;1055;531;1041;1040;1046;1043;529;528;525;527;966;526;516;517;1234;1235;1236;1351;1354;1462;1463;1465;1552;1774;1782;1783;1784;1785;Halftone;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;219;-8680.826,-457.1807;Inherit;False;1797.078;446.0921;;9;1415;1412;244;1414;227;247;1852;1854;1853;Light Attenuation;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;217;-5811.318,98.81934;Inherit;False;977.2441;332.3028;;6;265;238;237;236;226;222;Half Dir;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;215;-8596.564,4262.269;Inherit;False;3808.721;1802.809;;50;1577;805;1574;1576;1573;627;220;1397;903;1403;618;256;619;1093;651;1098;1094;1103;1102;623;1404;621;660;207;243;1099;650;252;1400;1097;1100;254;653;652;406;401;1096;583;649;644;588;661;1364;246;665;804;1104;1578;1579;1886;Main Specular ;1,1,1,1;0;0
Node;AmplifyShaderEditor.BreakToComponentsNode;671;280.2685,1150.97;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.CommentaryNode;1208;-8610.043,3076.147;Inherit;False;1877.47;918.8329;;18;1181;1202;1144;1203;1198;1154;1182;1143;1241;1243;1245;1247;1562;1839;1876;1877;1878;1887;Additional Lights Diffuse;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;1122;-4231.386,2746.804;Inherit;False;1770.555;1368.74;;32;1819;1818;1817;1816;1815;1814;1813;1812;1811;1810;1809;1808;1807;1806;1805;1804;1127;699;1126;1125;1124;1129;1769;1132;1123;439;1793;1665;1841;1842;1843;1859;Specular UVs;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;1054;-2110.905,5222.568;Inherit;False;2863.306;1438.541;;31;1294;1282;486;1296;1293;1047;1051;1295;1292;1050;1049;488;1287;1279;1277;1288;1278;1285;1280;1661;1297;1281;1056;1048;478;1792;1803;1848;1849;1850;1851;Overlay UVs;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;604;-1024.31,-59.87411;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;591;-1659.262,-259.775;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TimeNode;598;-1837.066,-507.4237;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;599;-1846.061,-365.7374;Inherit;False;Property;_OutlineDynamicSpeed;Outline Dynamic Speed;35;0;Create;False;0;0;0;False;0;False;0;0;0;0.05;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1096;-8391.886,5930.841;Inherit;False;Property;_SmoothnessMultiplier;Main Specular Size;73;0;Create;False;0;0;0;True;0;False;1;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;1807;-3842.239,3724.59;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;1555;-1762.134,1299.336;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DotProductOpNode;704;855.1718,-548.1996;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1241;-7770.771,3504.217;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.005;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1400;-6736.806,5184.546;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalVertexDataNode;52;-1850.069,191.2531;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;602;-1381.9,-345.7536;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1637;-878.1372,1225.523;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DistanceOpNode;55;-929.3464,-380.4816;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;1604;-4908.307,2566.073;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;606;-870.0676,46.70271;Inherit;False;Property;_UseDynamicOutline;Use Sketch Outline;36;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;503;-739.7486,3528.276;Inherit;False;Property;_MaxScaleDependingOnCamera;Max Scale Depends On Camera;62;0;Create;False;0;0;0;False;0;False;1;10;1;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;56;-1013.993,-179.219;Inherit;False;Property;_AdaptiveThicnkess;Adaptive Thickness;5;0;Create;False;0;0;0;False;0;False;0;0.32;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1087;-4574.137,1910.086;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;1064;-1271.18,3066.587;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;62;-552.3168,-51.57129;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;1098;-7062.433,4646.951;Inherit;False;Property;_AdditionalLightsIntesity;Additional Lights Intensity;74;0;Create;False;0;0;0;True;0;False;1;0;0;6;0;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;1561;-3368.183,2096.343;Inherit;False;Property;_StepHalftoneTexture;Step Halftone Texture;103;0;Create;False;0;0;0;False;0;False;0;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;600;-1471.603,-471.4403;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;1785;1204.024,4122.485;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;54;-1444.329,249.3617;Inherit;False;3;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;592;-1183.824,-103.8956;Inherit;False;2;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;651;-8329.613,5768.437;Float;False;Constant;_Float0;Float 0;0;0;Create;True;0;0;0;False;0;False;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;51;-1108.915,-350.8623;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;1502;-8367.388,2314.624;Inherit;False;233;BNNDotL;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;1682;-2942.101,2277.643;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;1576;-8069.019,4436.257;Inherit;False;Constant;_Vector6;Vector 6;125;0;Create;True;0;0;0;False;0;False;1,1,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;619;-6552.265,5461.582;Inherit;False;1538;BNCurrentNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;1264;-1482.546,1247.387;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;1,1,1,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1276;-7033.281,1816.061;Inherit;False;Constant;_Float4;Float 4;101;0;Create;True;0;0;0;False;0;False;0.98;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;1531;-7726.941,861.5594;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;673;420.2685,1256.97;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1356;-6496.462,2134.391;Inherit;False;Property;_DiffusePosterizePower;Posterize Power;92;0;Create;False;0;0;0;True;0;False;1;0.5;0.5;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;1132;-3596.041,2808.021;Inherit;False;Property;_UseScreenUvsSpecular;Screen Uvs;69;0;Create;False;0;0;0;True;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1121;-1894.876,3059.795;Inherit;False;4;4;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RadiansOpNode;1293;-1771.214,5650.848;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;601;-1733.868,-277.1592;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1269;-2978.034,1065.561;Inherit;False;IndirectDiffuseLight;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;1462;70.23511,4215.303;Inherit;False;3;0;COLOR;1,1,1,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector3Node;264;-8019.342,1914.061;Float;False;Constant;_Vector3;Vector 3;0;0;Create;True;0;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ClampOpNode;358;-6847.237,1691.888;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.02,0.02;False;2;FLOAT2;0.98,0.98;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;696;-4517.71,204.9207;Inherit;False;Property;_SpecularShadowMask;Specular Shadow Mask;46;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;742;1133.649,92.01123;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;694;-3814.204,-108.875;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1102;-8105.622,5870.156;Inherit;False;2;0;FLOAT;2;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;747;1842.102,-59.75346;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceCameraPos;50;-1180.349,-497.7589;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ClampOpNode;753;2041.405,-41.32713;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;537;510.0921,-1058.331;Inherit;False;1538;BNCurrentNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;1469;-2124.462,1991.161;Inherit;False;Property;_IndirectLightAffectOnHatch;Indirect Light Affect On Hatch;98;0;Create;False;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;456;2734.758,-872.3867;Inherit;False;Property;_RimColor;Rim Color;26;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;593;-1634.274,-144.02;Inherit;False;Property;_OutlineNoiseScale;Outline Noise Scale;33;0;Create;False;0;0;0;False;0;False;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;1486;-5319.875,2450.541;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;1266;-3901.295,784.0469;Inherit;False;Property;_OverlayMode;Overlay Mode;79;0;Create;False;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;3;None;Haftone;Hatching;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleRemainderNode;1279;-1248.82,6510.738;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;486;-908.7979,5575.512;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1647;2982.141,-773.8771;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleTimeNode;1287;-1821.28,5527.19;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;680;2194.921,2612.172;Inherit;False;Emission;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;391;-4532.718,1697.535;Inherit;False;BNDiffuse;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;57;-1367.279,63.58631;Inherit;False;Property;_OutlineType;Outline Type;6;0;Create;True;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;3;Normal;Position;UVBaked;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;1366;-3538.575,-113.5741;Inherit;False;1364;IndirectSpecular;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;691;-3773.227,1400.115;Inherit;False;Property;_IndirectLightStrength;Indirect Light Strength;45;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;732;2221.038,-1035.235;Inherit;False;Property;_UseBacklight;Rim As Backlight & Side Shine;52;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightPos;226;-5790.688,169.8815;Inherit;False;0;3;FLOAT4;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;-593.3286,72.36163;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;590;-1410.021,-165.775;Inherit;False;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;15.43;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1388;2304.728,1627.696;Inherit;False;SpecularColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1500;-7797.34,2328.851;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;379;-6571.373,718.5477;Inherit;False;Property;_WarpStrength;Warp Strength;21;0;Create;True;0;0;0;False;0;False;0;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;766;1554.617,11.79982;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;1769;-4021.579,3262.117;Inherit;False;StaticScreenSpaceUV;-1;;202;1e9a29825d5b8df43882e5bd6744aaf5;0;1;7;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;1416;-6012.252,2377.936;Inherit;False;1415;ShadowAtten;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;1397;-5930.385,5184.457;Inherit;False;638;Smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;583;-7704.521,5317.834;Inherit;False;Property;_SpecularPosterizeSteps;Specular Posterize Steps;32;0;Create;True;0;0;0;True;0;False;0;0;0;15;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1221;-3451.219,1360.915;Inherit;False;IndirectLightStrength;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;406;-6895.12,4967.884;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;1119;-2110.699,3001.061;Inherit;False;460;RimLight;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1644;2679.005,-656.2682;Inherit;False;1554;BNDiffuseNoAdditionalLights;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;763;1590.333,-108.2025;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FloorOpNode;1280;-1006.4,6511.142;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1460;-2123.598,3276.599;Inherit;False;243;BNspecularFinalColor;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;756;1672.788,-980.4533;Inherit;False;Property;_BacklightIntensity;Backlight Intensity;51;0;Create;True;0;0;0;False;0;False;1;0;0;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;58;-841.3283,219.3617;Inherit;False;Property;_Thicnkess;Thickness;4;0;Create;False;0;0;0;False;0;False;0;0;0;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;764;1678.333,108.7975;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;1504;-8336.877,2427.897;Float;False;Constant;_Vector2;Vector 2;0;0;Create;True;0;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1225;1121.372,4201.733;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1097;-6732.377,4612.236;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;53;-1660.237,206.434;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;1554;-4617.162,1576.96;Inherit;False;BNDiffuseNoAdditionalLights;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;1231;-4198.647,-370.1738;Inherit;False;Property;_OverlayMode;Overlay Mode;77;0;Create;False;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;3;None;Haftone;Hatching;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;1470;-5471.167,2275.45;Inherit;False;Property;_UseDiffuseWarpAsOverlay;Impact Shadows;99;0;Create;False;0;0;0;True;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Vector2Node;1126;-3225.028,2813.923;Inherit;False;Constant;_Vector5;Vector 5;87;0;Create;True;0;0;0;False;0;False;0.5,0.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;1539;1768.883,3052.132;Inherit;False;Property;_NormalMapStrength;Normal Map Strength;31;0;Create;False;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;1047;378.3767,5290.488;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;1292;-2055.213,5697.848;Inherit;False;Property;_UVSrcrollAngle;UV Scroll Angle;85;0;Create;False;0;0;0;False;0;False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;249;-8572.477,2017.969;Inherit;False;250;BNLightWarpVector;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;1351;-1334.522,4431.36;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1350;268.6716,661.877;Inherit;False;Constant;_Float6;Float 6;111;0;Create;True;0;0;0;False;0;False;-2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1483;-5756.334,2482.501;Inherit;False;250;BNLightWarpVector;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1622;-5804.497,1548.024;Inherit;True;Property;_LightRampTexture;Light Ramp Texture;12;2;[NoScaleOffset];[SingleLineTexture];Fetch;True;0;0;0;False;1;;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;686;1827.266,2502.482;Inherit;False;Property;_UseEmission;UseEmission;44;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1774;1302.066,4170.942;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;1,1,1,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1538;2114.056,2805.07;Float;False;BNCurrentNormal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1680;-5853.912,1735.709;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1297;-2082.536,5472.369;Inherit;False;Property;_UVScrollSpeed;UV Scroll Speed;88;0;Create;False;0;0;0;False;0;False;0;0;0;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1626;-5192.144,1747.387;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;227;-7399.636,-366.5314;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;692;-3272.457,1181.337;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;765;1128.333,-43.20255;Inherit;False;Property;_SideShineAmount;Side Shine Amount;54;0;Create;True;0;0;0;False;0;False;0.2717647;0;0;0.7;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;457;458.4457,-880.3952;Inherit;False;Property;_RimPower;Rim Power;28;0;Create;True;0;0;0;False;0;False;2;1;1;12;0;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;1489;-6901.433,291.9619;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;220;-5679.071,5035.036;Inherit;False;4;4;0;FLOAT3;1,1,1;False;1;FLOAT;0;False;2;FLOAT;0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1813;-4148.26,3972.164;Inherit;False;Property;_UVAnimationSpeedSpec;UV Animation Speed;82;0;Create;False;0;0;0;False;0;False;0;0;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1234;945.2336,4200.764;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StickyNoteNode;1830;-212.1996,1143.355;Inherit;False;233;107;Passes note;;1,1,1,1;Outline goes to pass 0 and main color - pass1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1051;559.888,5278.561;Inherit;False;OverlayUVs;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CustomExpressionNode;1331;-6098.767,2092.703;Inherit;False;return  floor(In / (1 / Steps)) * (1 / Steps)@;1;Create;2;True;In;FLOAT;0;In;;Inherit;False;True;Steps;FLOAT;0;In;;Inherit;False;Posterize;True;False;0;;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1094;-8015.695,4808.17;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1040;-870.4089,4520.315;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1790;-1930.129,2107.977;Inherit;False;Property;_PaperTilling;Paper Tiling;113;0;Create;False;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;715;1165.498,-681.5406;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1841;-4014.146,2838.305;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCGrayscale;1648;2883.622,-509.4114;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1129;-3887.68,3034.723;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;1294;-1497.214,5649.848;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1817;-3228.32,3954.951;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;236;-5736.223,268.1741;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.FloorOpNode;1816;-3360.699,3957.349;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleRemainderNode;1815;-3603.119,3956.944;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1496;-7041.899,699.3228;Inherit;False;Property;_WarpTextureRotation;UV Rotation;47;0;Create;False;0;0;0;True;0;False;0;83.96;0;180;0;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;1488;-6856.261,517.7318;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1417;-7754.294,2041.835;Inherit;False;1415;ShadowAtten;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;1542;-6257.038,-382.9371;Inherit;False;1538;BNCurrentNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleTimeNode;1814;-3864.283,3942.325;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;44;-272.0464,388.2114;Inherit;False;Property;_OutlineColor;Outline Color;7;1;[HDR];Create;True;0;0;0;True;0;False;0,0,0,0;0,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1810;-3659.305,3546.053;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;1,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;1358;-6851.262,2240.591;Inherit;False;Property;_DiffusePosterizeOffset;Posterize Offset;93;0;Create;False;0;0;0;True;0;False;0;0;-0.5;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;677;1771.378,2591.815;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;1157;-4645.257,2091.291;Inherit;False;1144;AdditionalLightsDiffuse;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SinOpNode;1804;-3712.239,3794.59;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1026;-1958.094,1282.58;Inherit;False;1025;Hatching;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode;481;-2169.376,2599.59;Inherit;True;Property;_Hatch1;Hatch Texture Darker;61;1;[NoScaleOffset];Create;False;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.GetLocalVarNode;644;-6819.08,5576.433;Inherit;False;638;Smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;726;1589.999,-580.5162;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;1818;-3103.358,3729.26;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;1463;-254.7649,4329.303;Inherit;False;Property;_HalftoneToonAffect;Toon Affect;97;0;Create;False;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;233;-5864.712,-350.3873;Float;False;BNNDotL;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;966;-2080.122,4293.563;Inherit;False;965;CompleteDiffuseLight;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WireNode;1556;-1463.134,1409.336;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1288;-1588.28,5472.312;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;1,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;248;-8406.031,1899.421;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1579;-6527.679,4680.167;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;1222;882.5724,4445.615;Inherit;False;364;MainTexture;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.CosOpNode;1806;-3699.239,3696.59;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;805;-8281.293,4298.183;Inherit;False;1538;BNCurrentNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;267;-8110.707,1770.583;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GrabScreenPosition;1792;-992.3535,5843.21;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Compare;685;2006.266,2608.482;Inherit;False;0;4;0;FLOAT;0;False;1;FLOAT;1;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;240;-7812.167,1908.289;Inherit;False;244;BNAttenuationColor;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1123;-3839.785,2759.804;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;1238;-4659.746,-28.08583;Inherit;False;1236;HalftoneDiffuseShadowMask;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RadiansOpNode;1127;-3087.906,2959.125;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1154;-8169.359,3130.21;Inherit;False;1538;BNCurrentNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1125;-2793.566,2839.703;Inherit;False;SpecularUVs;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;1198;-8171.903,3228.192;Inherit;False;250;BNLightWarpVector;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;378;-6332.124,557.7954;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;359;-7462.312,1586.428;Inherit;False;Property;_LightRampOffset;Light Ramp Offset;10;0;Create;True;0;0;0;False;0;False;0;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;1681;-2897.608,2118.314;Inherit;False;Property;_InverseMask;Inverse Mask;112;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;463;474.3687,-786.8387;Inherit;False;Property;_RimSmoothness;Rim Smoothness;29;0;Create;True;0;0;0;False;0;False;0;0;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;275;-2118.361,3093.842;Inherit;False;391;BNDiffuse;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;623;-6459.219,5675.479;Inherit;False;Property;_Strength;Strength;38;0;Create;False;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1466;-3047.77,1230.248;Inherit;False;IndirectHatching;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleTimeNode;1811;-3892.305,3600.931;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;749;1336.177,-244.8872;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;1278;-1509.984,6496.118;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;250;-6141.967,419.4187;Float;False;BNLightWarpVector;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;1182;-8395.315,3160.129;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;1650;3146.622,-579.4114;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;751;1721.406,-172.3272;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1805;-4126.238,3771.59;Inherit;False;Property;_UVSrcrollAngleSpec;UV Scroll Angle;84;0;Create;False;0;0;0;False;0;False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1216;329.6536,4330.798;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;1,1,1,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;59;-713.4068,-214.2483;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;1573;-7367.304,4528.916;Inherit;False;float3 Color = 0@$$Smoothness = exp2(10 * Smoothness + 1)@$int numLights = GetAdditionalLightsCount()@$for(int i = 0@ i<numLights@i++)${$	$			#if VERSION_GREATER_EQUAL(10, 1)$			Light light = GetAdditionalLight(i, WorldPosition, half4(1,1,1,1))@$			// see AdditionalLights_float for explanation of this$		#else$			Light light = GetAdditionalLight(i, WorldPosition)@$		#endif$	$	half3 AttLightColor = light.color *(light.distanceAttenuation * light.shadowAttenuation)@$	Color += LightingSpecular(AttLightColor, light.direction, WorldNormal, WorldView, half4(SpecColor, 0), Smoothness)@	$}$$float IN = max(Color.b,max(Color.r,Color.g))@$$float minOut = 0.5 * SpecFaloff - 0.005@$float faloff = lerp(IN, smoothstep(minOut, 0.5, IN), SpecFaloff)@$if(Steps < 1)${$    return Color *faloff@$}$else${$    return  Color *floor(faloff / (1 / Steps)) * (1 / Steps)@$}$;3;Create;7;True;WorldPosition;FLOAT3;0,0,0;In;;Float;False;True;WorldNormal;FLOAT3;0,0,0;In;;Float;False;True;WorldView;FLOAT3;0,0,0;In;;Float;False;True;SpecColor;FLOAT3;0,0,0;In;;Float;False;True;Smoothness;FLOAT;0.5;In;;Float;False;True;Steps;FLOAT;0;In;;Inherit;False;True;SpecFaloff;FLOAT;0;In;;Inherit;False;AdditionalLightsSpecularMy;False;False;0;;False;7;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0.5;False;5;FLOAT;0;False;6;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;1277;-736.0189,6461.448;Inherit;False;Property;_OverlayMode;Overlay Mode;63;0;Create;False;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;3;None;Haftone;Hatching;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;739;1998.822,-1050.442;Inherit;False;738;FresnelValue;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;621;-5829.478,5520.319;Inherit;False;Property;_UseEnvironmentRefletion;UseEnvironment Reflections;37;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;1235;682.1721,4361.406;Inherit;False;1144;AdditionalLightsDiffuse;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;1808;-3568.239,3723.59;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RoundOpNode;588;-7369.591,5277.984;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;526;-2025.089,3992.125;Inherit;False;Property;_HalftoneEdgeOffset;Halftone Edge Offset;57;0;Create;True;0;0;0;False;0;False;0.1;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;382;-6540.259,272.6273;Inherit;False;Property;_UseDiffuseWarp;UseDiffuse Warp;24;1;[Toggle];Create;True;0;0;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;1537;1916.055,2801.07;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;260;-7709.142,1771.562;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;373;1693.036,805.7413;Inherit;False;Property;_RampDiffuseTextureLoaded;RampDiffuseTextureLoaded;17;1;[HideInInspector];Create;True;0;0;0;True;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;263;-3059.157,798.1735;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;1851;-1628.057,6022.402;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1022;1575.399,4670.469;Inherit;False;Halftone;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;1355;-6330.462,1931.391;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1270;-1801.1,1520.373;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;745;1328.984,137.2247;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;740;2165.052,202.1731;Inherit;False;738;FresnelValue;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;274;-3140.385,-357.7659;Inherit;False;BNBlinnPhongLightning;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1642;3442.544,-824.1395;Inherit;False;RimColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;1465;1276.373,4608.949;Inherit;False;1269;IndirectDiffuseLight;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1363;-3200.625,1032.195;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;239;-2802.317,769.5178;Float;True;BNFinalDiffuse;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RoundOpNode;1330;-6322.955,2222.411;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;262;-7896.342,1750.062;Inherit;False;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;375;-6687.352,435.7679;Inherit;True;Property;_DiffuseWarpNoise;Diffuse Warp Noise;18;2;[NoScaleOffset];[SingleLineTexture];Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;762;2476.47,-39.55762;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1505;-8113.792,2445.615;Inherit;False;244;BNAttenuationColor;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;1617;-5316.11,1619.201;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;804;-8427.473,4775.182;Inherit;False;638;Smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;438;-2944.745,1963.174;Inherit;False;Property;_UseSpecularMask;UseSpecular Mask;25;1;[Toggle];Create;True;0;0;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;1803;-509.5449,6366.892;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;1055;-2013.269,4645.898;Inherit;False;1051;OverlayUVs;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;653;-8092.612,5714.436;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;441;-3840.463,2031.383;Inherit;True;Property;_SpecularMaskTexture;Specular Mask Texture;19;2;[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;True;0;False;-1;None;0c021cea0cf2417459ce7fa844118e17;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;721;783.0558,-265.3714;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;1843;-4143.065,2872.429;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;1665;-3711.186,3132.833;Inherit;False;Property;_UseAdaptiveUvsSpecular;Adaptive Screen Uvs;110;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SmoothstepOpNode;1045;-648.0934,4367.319;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;1625;-5706.054,2072.304;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;903;-5932.224,5081.812;Inherit;False;902;SpecularMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;1557;-3571.099,2214.36;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0.6;False;2;FLOAT;0.97;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;760;2266.413,297.66;Inherit;False;Property;_SideShineHardness;Side Shine Hardness;13;0;Create;False;0;0;0;False;0;False;0;0;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;718;1902.301,-880.9074;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;460;2924.15,-1018.172;Inherit;False;RimLight;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1809;-4153.561,3546.111;Inherit;False;Property;_UVScrollSpeedSpec;UV Scroll Speed;89;0;Create;False;0;0;0;False;0;False;0;0;0;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;1296;-1641.214,5720.848;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;1501;-8159.562,2330.285;Inherit;False;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ScreenParams;1855;-1917.826,2214.864;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CosOpNode;1295;-1628.214,5622.848;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;1265;-315.796,2787.393;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;902;-2568.28,2123.094;Inherit;False;SpecularMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1541;1421.466,2768.47;Inherit;True;Property;_BumpMap;Normal Map;30;1;[Normal];Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.LerpOp;1628;-5238.33,1951.119;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;649;-7916.612,5847.436;Float;False;Constant;_Float1;Float 1;0;0;Create;True;0;0;0;False;0;False;-2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1067;-1745.472,3174.743;Inherit;False;Property;_Darken;Darken;70;0;Create;True;0;0;0;False;0;False;0;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;965;-7175.173,2060.798;Inherit;False;CompleteDiffuseLight;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;516;-1063.541,4249.853;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;1771;-968.6199,5403.044;Inherit;False;StaticScreenSpaceUV;-1;;203;1e9a29825d5b8df43882e5bd6744aaf5;0;1;7;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SaturateNode;1611;-4620.763,2489.308;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1249;-688.2852,1269.027;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;1599;-5111.077,2598.644;Inherit;False;1415;ShadowAtten;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;372;-4920.076,1746.524;Inherit;False;Property;_UseLightRamp;Shading Mode;16;0;Create;False;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;3;Step;DiffuseRamp;Posterize;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;237;-5358.689,185.8815;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LightColorNode;1552;-250.2906,4059.738;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;712;604.3212,-477.6704;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LightColorNode;247;-7663.741,-417.6765;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.DistanceOpNode;498;-774.9536,3369.153;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1066;-1840.345,3338.422;Inherit;False;Property;_Lighten;Lighten;71;0;Create;True;0;0;0;False;0;False;0;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1643;-1137.734,830.2725;Inherit;False;1642;RimColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;1646;3250.362,-717.5472;Inherit;False;Property;_RimSplitColor;Rim Split Color;106;0;Create;False;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;3;NoSplit;MultiplyWithDiffuse;UseSecondColor;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleTimeNode;1523;-8069.095,676.4829;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1362;-6558.23,2224.656;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;748;1057.807,-244.0028;Inherit;False;1538;BNCurrentNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;1187;-1529.354,2768.867;Inherit;False;MaxFromVector3;-1;;204;92f2539b674dd3042b132cfbdf18809e;0;1;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1281;-874.0208,6508.744;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;699;-3293.921,3090.699;Inherit;False;Property;_SpecularMaskRotation;Specular Mask Rotation;48;0;Create;True;0;0;0;True;0;False;0;83.96;0;180;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1631;-4203.31,2248.858;Inherit;False;Property;_HaltonePatternSize;Halftone Pattern Size;104;0;Create;False;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1530;-7535.959,485.052;Inherit;False;3;3;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;1498;-7478.483,2388.331;Inherit;False;Property;_DiffuseWarpAffectHalftone;Diffuse Warp Affect Halftone;100;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;755;1768.169,-723.2394;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1683;-7252.914,2182.701;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;464;2662.978,-1035.445;Inherit;False;Property;_UseRimLight;UseRim Light;0;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;271;-4628.169,-376.4447;Inherit;False;239;BNFinalDiffuse;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;488;-111.0751,5272.073;Inherit;False;Property;_UseScreenUvs;Screen Uvs;68;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;705;1044.782,-545.7766;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;-0.6;False;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;1049;388.5324,5535.734;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1236;591.3068,4597.426;Inherit;False;HalftoneDiffuseShadowMask;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;1349;424.3337,676.1953;Inherit;False;Property;_UseOutline;UseOutline;1;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;259;-3353.517,-351.2742;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.Compare;1099;-7659.398,5872.401;Inherit;False;0;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1517;-8589.885,771.4034;Inherit;False;Property;_UVSrcrollAngleWarp;UV Scroll Angle;86;0;Create;False;0;0;0;False;0;False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;462;1232.969,-1000.956;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.CosOpNode;1519;-8162.881,696.4033;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.IndirectSpecularLight;618;-6310.735,5485.579;Inherit;False;World;3;0;FLOAT3;0,0,1;False;1;FLOAT;1;False;2;FLOAT;0.75;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;1359;-6473.462,1940.491;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;365;341.8629,510.9032;Inherit;False;Property;_UseOutline;UseOutline;1;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;265;-5070.689,185.8815;Float;False;BNHalfDirection;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;256;-8279.175,5533.507;Inherit;False;1538;BNCurrentNormal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;1360;-6065.672,1969.796;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;1104;-8050.104,5007.949;Inherit;False;0;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceCameraPos;492;-1058.644,3358.441;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;356;-7020.02,1622.714;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;1619;-5453.472,1878.97;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CustomExpressionNode;491;-985.7573,2994.764;Inherit;False;	float log2_dist = log2(_dist)-0.2@$	$	float2 floored_log_dist = floor( (log2_dist + float2(0.0, 1.0) ) * 0.5) *2.0 - float2(0.0, 1.0)@				$	float2 uv_scale = min(_MaxScaleDependingOnCamera, pow(2.0, floored_log_dist))@$	$	float uv_blend = abs(frac(log2_dist * 0.5) * 2.0 - 1.0)@$	$	float2 scaledUVA = _uv / uv_scale.x@ // 16$	float2 scaledUVB = _uv / uv_scale.y@ // 8 $$	float3 hatch0A = tex2D(_Hatch0, scaledUVA).rgb@$	float3 hatch1A = tex2D(_Hatch1, scaledUVA).rgb@$$	float3 hatch0B = tex2D(_Hatch0, scaledUVB).rgb@$	float3 hatch1B = tex2D(_Hatch1, scaledUVB).rgb@$$	float3 hatch0 = lerp(hatch0A, hatch0B, uv_blend)@$	float3 hatch1 = lerp(hatch1A, hatch1B, uv_blend)@$$	float3 overbright = max(0, _intensity - 1.0)@$$	float3 weightsA = saturate((_intensity * 6.0) + float3(-0, -1, -2))@$	float3 weightsB = saturate((_intensity * 6.0) + float3(-3, -4, -5))@$$	weightsA.xy -= weightsA.yz@$	weightsA.z -= weightsB.x@$	weightsB.xy -= weightsB.yz@$$	hatch0 = hatch0 * weightsA@$	hatch1 = hatch1 * weightsB@$$	float3 hatching = overbright + hatch0.r +$		hatch0.g + hatch0.b +$		hatch1.r + hatch1.g +$		hatch1.b@$$	return hatching@;3;Create;6;False;_uv;FLOAT2;0,0;In;;Inherit;False;False;_intensity;FLOAT;0;In;;Inherit;False;False;_Hatch0;SAMPLER2D;;In;;Inherit;False;False;_Hatch1;SAMPLER2D;;In;;Inherit;False;False;_dist;FLOAT;0;In;;Inherit;False;True;_MaxScaleDependingOnCamera;FLOAT;0;In;;Inherit;False;HatchingConstantScale;True;False;0;;False;6;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;SAMPLER2D;;False;3;SAMPLER2D;;False;4;FLOAT;0;False;5;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;727;1247.561,-468.5357;Inherit;False;Property;_BacklightHardness;Backlight Hardness;14;0;Create;False;0;0;0;False;0;False;0;0;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;716;1409.204,-599.4019;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;1237;-4398.406,-124.581;Inherit;False;Property;_OverlayMode;Overlay Mode;79;0;Create;False;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;3;None;Haftone;Hatching;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;1629;-5486.158,2078.652;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector2Node;1623;-6054.545,1579.347;Inherit;False;Constant;_Vector7;Vector 7;127;0;Create;True;0;0;0;False;0;False;0.02,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;642;-4138.578,1020.884;Inherit;False;364;MainTexture;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;1636;-1215.137,709.5232;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;650;-7782.619,5736.436;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Exp2OpNode;652;-7633.619,5741.436;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;708;472.0822,-281.9579;Inherit;False;Property;_BacklightAmount;Backlight Amount;49;0;Create;True;0;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;252;-8017.589,5469.842;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1111;-1538.438,3340.503;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;527;-2016.089,4125.125;Inherit;False;Property;_HalftoneSmoothness;Halftone Smoothness;56;0;Create;True;0;0;0;False;0;False;0.3;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;370;-7185.927,2449.221;Inherit;False;Property;_StepOffset;Step Offset;15;0;Create;False;0;0;0;True;0;False;0;0;-0.5;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1485;-5487.313,2442.394;Inherit;False;3;3;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Vector2Node;1048;151.0441,5347.93;Inherit;False;Constant;_Vector0;Vector 0;87;0;Create;True;0;0;0;False;0;False;0.5,0.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.OneMinusNode;466;820.7245,-839.5659;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;737;2469.855,113.0997;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;384;-6293.789,317.1498;Inherit;False;0;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;244;-7179.426,-399.5931;Float;False;BNAttenuationColor;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;1232;-4575.157,-275.6162;Inherit;False;1022;Halftone;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1510;-5609.679,2638.59;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT4;-1,0,0,0;False;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;528;-1696.994,4115.875;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;636;1382.471,1568.447;Inherit;True;Property;_SpecGlossMap;Specular Map;39;2;[NoScaleOffset];[SingleLineTexture];Create;False;0;0;0;True;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1103;-8195.563,4873.534;Inherit;False;2;0;FLOAT;2;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1376;2060.616,1574.841;Inherit;False;Property;_SpecColor;Specular Value;3;0;Create;False;0;0;0;True;0;False;1,1,1,0;1,1,1,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.WorldPosInputsNode;1414;-8611.005,-378.3377;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleSubtractOpNode;467;1004.604,-900.3163;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1632;-3725.131,2329.295;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.3;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1257;-1332.921,1677.669;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;1,1,1,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;581;-4000.692,1309.903;Inherit;False;1538;BNCurrentNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;392;-4132.12,745.6323;Inherit;False;391;BNDiffuse;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;1514;-7422.728,256.8805;Inherit;False;Property;_UseScreenUvsWarp;Screen Uvs;102;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.NormalizeNode;222;-5534.689,146.8815;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1364;-5461.899,5581.855;Inherit;False;IndirectSpecular;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;529;-1604.635,3962.274;Inherit;False;Property;_HalftoneThreshold;Halftone Threshold;58;0;Create;True;0;0;0;False;0;False;0.1;0;0;0.15;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;1511;-5778.932,2570.127;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WorldSpaceLightPos;741;1043.601,246.2542;Inherit;False;0;3;FLOAT4;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.NormalizeNode;238;-5230.689,185.8815;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;1274;-7037.281,1738.061;Inherit;False;Constant;_Float3;Float 3;101;0;Create;True;0;0;0;False;0;False;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1645;2840.679,-393.1641;Inherit;False;Property;_RimShadowColor;Rim Shadow Color;105;0;Create;False;0;0;0;False;0;False;0,0.05551431,0.9622642,0;0,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1041;-1020.704,4572.356;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;738;1419.672,-986.2852;Inherit;False;FresnelValue;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;360;-7155.594,1636.903;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;731;2120.477,-821.2676;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;1520;-8031.88,723.4033;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WorldSpaceLightPos;702;548.6017,-596.4689;Inherit;False;0;3;FLOAT4;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.LerpOp;695;-3958.377,7.337563;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1403;-5952.205,5308.702;Inherit;False;1388;SpecularColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;1188;-7375.836,1841.717;Inherit;False;MaxFromVector3;-1;;205;92f2539b674dd3042b132cfbdf18809e;0;1;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;207;-7656.765,5108.074;Inherit;False;Property;_SpecularFaloff;Specular Falloff;9;0;Create;False;0;0;0;True;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;282;-438.3608,945.2823;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1418;-7513.909,1994.875;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;1467;-2134.365,1863.206;Inherit;False;1466;IndirectHatching;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;757;2122.584,-634.8906;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1529;-8596.961,1031.286;Inherit;False;Property;_UVAnimationSpeedWarp;UV Animation Speed;81;0;Create;False;0;0;0;False;0;False;0;0;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;1574;-8246.844,4372.275;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;254;-8264.929,5444.507;Inherit;False;265;BNHalfDirection;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;638;1855.223,1676.036;Inherit;False;Smoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1536;1761.389,2825.948;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;1661;-616.9718,5410.435;Inherit;False;Property;_UseAdaptiveScreenUvs;Adaptive Screen Uvs;109;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SinOpNode;1521;-8175.882,794.4034;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1217;75.86629,3963.918;Inherit;False;Property;_HalftoneColor;Halftone Color;59;0;Create;False;0;0;0;False;0;False;0,0,0,1;0,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.Compare;437;-2681.937,2015.409;Inherit;False;0;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;758;1817.283,-552.0821;Inherit;False;Property;_SideShineIntensity;Side Shine Intensity;53;0;Create;False;0;0;0;False;0;False;1;0;0;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;444;-6758.538,2083.357;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0.495;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1046;-1312.338,4731.281;Inherit;False;Property;_ShapeSmoothness;Transition Smoothness;65;0;Create;False;0;0;0;False;0;False;0.2;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;1540;1479.323,2996.637;Inherit;False;Constant;_Vector0;Vector 0;44;0;Create;True;0;0;0;False;0;False;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;1260;-1209.246,1245.788;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;1,1,1,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;458;465.8676,-974.0062;Inherit;False;Property;_RimThickness;Rim Thickness;27;0;Create;True;0;0;0;False;0;False;1;1;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1069;-1437.051,3185.468;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-2;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;234;-6008.713,-366.3873;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;1354;-1429.461,4606.334;Inherit;False;Property;_Inverse;Inverse;91;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1668;-8004.736,343.5169;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;440;-3290.488,2295.681;Inherit;False;Property;_SpecularMaskStrength;Specular Mask Strength;20;0;Create;True;0;0;0;True;0;False;0.1856417;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.IndirectDiffuseLighting;276;-3764.335,1268.758;Inherit;False;World;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;517;-1231.262,4129.005;Inherit;False;2;0;FLOAT;0.1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1522;-7825.463,655.2419;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;1,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;1600;-4935.629,2686.68;Inherit;False;250;BNLightWarpVector;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;1186;-1771.505,4300.659;Inherit;False;MaxFromVector3;-1;;209;92f2539b674dd3042b132cfbdf18809e;0;1;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1050;105.2931,5619.522;Inherit;False;Property;_OverlayRotation;UV Rotation;66;0;Create;False;0;0;0;False;0;False;0;0;0;180;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;530;-1778.019,976.3893;Inherit;False;274;BNBlinnPhongLightning;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;371;-7003.382,2291.36;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;1492;-7149.467,337.9672;Inherit;False;Constant;_Vector1;Vector 1;87;0;Create;True;0;0;0;False;0;False;0.5,0.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.CustomExpressionNode;476;-1077.271,2633.342;Inherit;False;float intensity = color@$$    float3 hatch0 = tex2D(_Hatch0, _uv).rgb@$    float3 hatch1 = tex2D(_Hatch1, _uv).rgb@$$    float3 overbright = max(0, intensity - 1.0)@$$    float3 weightsA = saturate((intensity * 6.0) + float3(-0, -1, -2))@$    float3 weightsB = saturate((intensity * 6.0) + float3(-3, -4, -5))@$$    weightsA.xy -= weightsA.yz@$    weightsA.z -= weightsB.x@$    weightsB.xy -= weightsB.yz@$$    hatch0 = hatch0 * weightsA@$    hatch1 = hatch1 * weightsB@$$    float3 hatching = overbright + hatch0.r +$        hatch0.g + hatch0.b +$        hatch1.r + hatch1.g +$        hatch1.b@$$    return hatching@$    return hatching@;3;Create;4;False;_uv;FLOAT2;0,0;In;;Inherit;False;True;color;FLOAT;0;In;;Inherit;False;False;_Hatch0;SAMPLER2D;;In;;Inherit;False;False;_Hatch1;SAMPLER2D;;In;;Inherit;False;Hatching;True;False;0;;False;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;SAMPLER2D;;False;3;SAMPLER2D;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;446;-3118.994,2126.112;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;1518;-8305.884,724.4033;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FloorOpNode;1528;-8007.676,981.0222;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;223;-8221.254,2079.614;Inherit;False;233;BNNDotL;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;1634;-3893.671,2248.036;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;665;-7740.41,5595.694;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1527;-7881.676,894.6254;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1329;-6810.17,2362.941;Inherit;False;Property;_DiffusePosterizeSteps;Posterize Steps;90;0;Create;False;0;0;0;True;0;False;3;1;1;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1043;-805.9669,4710.431;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;1578;-6986.635,5267.024;Inherit;False;float minOut = 0.5 * SpecFaloff - 0.005@$float faloff = lerp(IN, smoothstep(minOut, 0.5, IN), SpecFaloff)@$if(Steps < 1)${$    return faloff@$}$else${$    return  floor(faloff / (1 / Steps)) * (1 / Steps)@$};1;Create;3;True;IN;FLOAT;0;In;;Inherit;False;True;SpecFaloff;FLOAT;0;In;;Inherit;False;True;Steps;FLOAT;0;In;;Inherit;False;FaloffPosterize;False;False;0;;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;482;-2170.969,2780.633;Inherit;True;Property;_Hatch2;Hatch Texture Brighter;60;1;[NoScaleOffset];Create;False;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;674;441.585,1038.581;Inherit;False;Property;_Cutoff;Alpha Clip Threshold;41;0;Create;False;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;454;757.5737,-1049.152;Inherit;False;Standard;WorldNormal;ViewDir;True;True;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1605;-4745.212,2537.469;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;269;-6073.979,-207.2886;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;743;939.8915,137.4635;Inherit;False;Constant;_Float2;Float 2;62;0;Create;True;0;0;0;False;0;False;-1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1243;-8323.181,3876.271;Inherit;False;Property;_AdditionalLightsFaloff;Additional Lights Falloff;77;0;Create;False;0;0;0;True;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleRemainderNode;1526;-8160.476,912.6194;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;445;-6943.111,2173.795;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.009;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;693;-4625.304,-155.1281;Inherit;False;391;BNDiffuse;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;759;2630.551,-58.32035;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;1782;537.9628,4803.693;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;525;-1451.088,4118.125;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;461;-1063.669,1038.62;Inherit;False;460;RimLight;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1093;-8523.727,4959.168;Inherit;False;Property;_AdditionalLightsSmoothnessMultiplier;Additional Lights Specular Size;72;0;Create;False;0;0;0;True;0;False;1;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1357;-6642.462,1926.19;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;246;-7868.181,5506.507;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;1024;-145.8171,935.0305;Inherit;False;Property;_OverlayMode;Overlay Mode;78;0;Create;True;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;3;None;Haftone;Hatching;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1285;-1793.961,6525.957;Inherit;False;Property;_UVAnimationSpeed;UV Animation Speed;83;0;Create;False;0;0;0;False;0;False;0;0;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.GrabScreenPosition;1793;-4190.236,3048.502;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;251;-8237.035,1902.421;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1211;-7791.012,2207.441;Inherit;False;1144;AdditionalLightsDiffuse;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;744;882.2424,-36.06917;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;1819;-3087.579,3257.327;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1070;-1403.74,3305.438;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1226;1529.925,4309.969;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1404;-7107.193,5108.159;Inherit;False;Property;_MainLightIntesity;Main Light Intensity;94;0;Create;False;0;0;0;False;0;False;1;0;0;6;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;661;-5980.348,5521.406;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;1784;591.0415,4015.491;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;679;1461,2580.218;Inherit;False;Property;_EmissionColor;Emission Color;42;1;[HDR];Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleAddOpNode;719;921.3605,-285.2785;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-2;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1451;-5641.219,1756.577;Inherit;False;Property;_ShadowColor;Shadow Color;96;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleTimeNode;1525;-8343.076,953.0225;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1524;-8392.847,605.2203;Inherit;False;Property;_UVScrollSpeedWarp;UV Scroll Speed;87;0;Create;False;0;0;0;False;0;False;0;0;0;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1253;-2070.124,1585.938;Inherit;False;Property;_HatchingColor;Hatching Color;78;0;Create;False;0;0;0;False;0;False;0,0,0,1;0,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1100;-7367.509,5554.123;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;229;-3574.793,815.2498;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;490;-719.8448,2737.505;Inherit;False;Property;_UseHatchingConstantScale;Hatching Constant Scale;63;0;Create;False;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;1254;-1553.623,1562.387;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;1,1,1,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;1664;-7871.438,432.9202;Inherit;False;Property;_UseAdaptiveScreenUvsWarp;Adaptive Screen Uvs ;111;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1495;-7784.187,255.1986;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;1448;-252.7794,586.307;Inherit;False;3;0;COLOR;1,1,1,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;659;1476.81,1783.215;Inherit;False;Property;_Glossiness;Smoothness;40;0;Create;False;0;0;0;True;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1509;-6016.182,2617.044;Inherit;False;Property;_DiffuseWarpBrightnessOffset;Brightness Offset;101;0;Create;False;0;0;0;True;0;False;1.12;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;681;-773.323,1053.889;Inherit;False;680;Emission;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1856;-1534.746,2176.987;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1415;-7721.192,-234.9471;Inherit;False;ShadowAtten;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.PowerNode;401;-7587.071,5545.759;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1421;-45.53701,757.0069;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldPosInputsNode;1577;-8039.773,4297.185;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;682;1351.523,2385.05;Inherit;True;Property;_EmissionMap;Emission Map;43;3;[HDR];[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.FunctionNode;1684;-7515.681,2175.847;Inherit;False;MaxFromVector3;-1;;210;92f2539b674dd3042b132cfbdf18809e;0;1;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;1783;1170.956,4503.854;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;767;1429.617,-17.20018;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;-0.3;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1508;-7986.188,2534.964;Inherit;False;1415;ShadowAtten;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WorldPosInputsNode;499;-979.4488,3531.412;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TextureCoordinatesNode;478;-761.747,5249.871;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;1056;-1707.346,5261.636;Inherit;False;Property;_OverlayUVTilling;UV Tiling;67;0;Create;False;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1635;-822.1129,860.0081;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1850;-1499.537,5960.707;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenParams;1840;-4310.225,2869.182;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;270;-4165.508,-150.3121;Inherit;False;243;BNspecularFinalColor;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;1213;-4175.321,2011.506;Inherit;False;1125;SpecularUVs;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1025;-267.1698,2626.978;Inherit;False;Hatching;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1042;-977.7531,4746.639;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;531;-811.8291,4229.577;Inherit;False;2;0;FLOAT;0.1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1449;-597.9544,675.8444;Inherit;False;Property;_OutlineTextureStrength;Texture Strength ;95;0;Create;False;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCGrayscale;1797;-4153.83,-58.39511;Inherit;False;1;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1247;-8230.354,3671.345;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;508;-1782.492,4485.812;Inherit;True;Property;_HalftoneTexture;Halftone Texture;55;2;[NoScaleOffset];[SingleLineTexture];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RotatorNode;1124;-3052.025,2760.692;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;1203;-8128.235,3545.015;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1468;-1880.071,1840.766;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;1220;-286.4756,4226.19;Inherit;False;1554;BNDiffuseNoAdditionalLights;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;660;-8374.304,5637.111;Inherit;False;638;Smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1053;-1393.442,2619.551;Inherit;False;1051;OverlayUVs;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;1842;-4019.146,2951.305;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;439;-4276.767,2774.767;Inherit;False;Property;_SpecularMaskScale;Specular Mask Tiling;22;0;Create;False;0;0;0;True;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;381;-8645.469,209.3164;Inherit;False;Property;_WarpTextureScale;UV Tiling;23;0;Create;False;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;1770;-8123.687,494.8918;Inherit;False;StaticScreenSpaceUV;-1;;211;1e9a29825d5b8df43882e5bd6744aaf5;0;1;7;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ScreenParams;1848;-1863.346,6005.213;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;1858;-1669.666,2175.111;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;1256;-1518.766,1779.329;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1245;-7929.325,3746.032;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;1854;-7931.402,-260.2408;Inherit;False;0;4;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;1849;-1349.361,5918.092;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;1258;-1192.877,1376.174;Inherit;False;Property;_UsePureSketch;Pure Sketch;80;0;Create;False;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1282;-191.5992,5439.158;Inherit;False;3;3;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;1812;-3075.319,3907.655;Inherit;False;Property;_OverlayMode1;Overlay Mode;64;0;Create;False;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;3;None;Haftone;Hatching;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;1847;-8324.31,349.5988;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector4Node;1853;-8278.682,-217.9064;Inherit;False;Constant;_Vector4;Vector 4;51;0;Create;True;0;0;0;False;0;False;1,1,1,1;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;1859;-3406.039,3288.76;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;1795;-974.3113,1902.741;Inherit;True;Property;_PaperTexture;Paper Texture;114;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;1852;-8042.545,-415.1778;Inherit;False;Property;_UseShadows;Use Shadows;50;1;[Toggle];Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;1562;-8454,3674.191;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;2;False;4;FLOAT;2.9;False;1;FLOAT;0
Node;AmplifyShaderEditor.GrabScreenPosition;1794;-8257.064,105.5836;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;658;1702.515,1656.614;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;596;-1602.23,-57.26891;Inherit;False;Property;_OutlineNoiseIntesity;Outline Noise Intensity;34;0;Create;False;0;0;0;False;0;False;1;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenParams;1844;-8730.684,484.0242;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;1845;-8606.523,362.2712;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1120;-2101.053,3178.53;Inherit;False;680;Emission;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1789;-1220.61,2037.258;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GrabScreenPosition;1791;-1492.132,1926.313;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1846;-8471.604,364.1472;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;1857;-1387.452,2162.439;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TemplateMultiPassSwitchNode;42;117.9438,783.0602;Inherit;False;0;0;10;10;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;9;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1202;-8488.096,3544.696;Inherit;False;Property;_AdditionalLightsAmount;Additional Lights Size;76;0;Create;False;0;0;0;True;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;1412;-8372.836,-363.0377;Inherit;False;$    #if SHADOWS_SCREEN$        half4 clipPos = TransformWorldToHClip(WorldPos)@$        half4 shadowCoord = ComputeScreenPos(clipPos)@$    #else$        half4 shadowCoord = TransformWorldToShadowCoord(WorldPos)@$    #endif$$    Light mainLight = GetMainLight(shadowCoord)@$    DistanceAtten = mainLight.distanceAttenuation@$    ShadowAtten = mainLight.shadowAttenuation@$;7;Create;3;True;WorldPos;FLOAT3;0,0,0;In;;Inherit;False;True;DistanceAtten;FLOAT;0;Out;;Inherit;False;True;ShadowAtten;FLOAT;0;Out;;Inherit;False;Light Attenuation;True;False;0;;False;4;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScreenPosInputsNode;1876;-7566.274,3749.709;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SwizzleNode;1877;-7365.036,3749.838;Inherit;False;FLOAT2;0;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;1143;-7292.4,3144.843;Inherit;False;Property;_UseAdditionalLightsDiffuse;UseAdditional Lights;75;0;Create;False;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1144;-6977.063,3146.878;Inherit;False;AdditionalLightsDiffuse;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;1828;-7903.005,2911.515;Inherit;False;SRP Additional Light;-1;;212;6c86746ad131a0a408ca599df5f40861;8,212,0,6,1,9,1,23,1,24,1,142,1,168,1,154,1;6;2;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;15;FLOAT3;0,0,0;False;14;FLOAT3;1,1,1;False;18;FLOAT;0.5;False;32;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CustomExpressionNode;1878;-7526.157,3392.087;Inherit;False;float3 Color = 0@$$int numLights = GetAdditionalLightsCount()@$for(int i = 0@ i<numLights@i++)${$	$			#if VERSION_GREATER_EQUAL(10, 1)$			Light light = GetAdditionalLight(i, WorldPosition, shadowmask)@$			// see AdditionalLights_float for explanation of this$		#else$			Light light = GetAdditionalLight(i, WorldPosition)@$		#endif$$	$	float3 DotVector = dot(light.direction,WorldNormal)@$	$$	half3 AttLightColor = (light.shadowAttenuation * light.distanceAttenuation)@$	 float3 colout = max(float3(0.f,0.f,0.f),LightWrapVector + (1-LightWrapVector) * DotVector )*AttLightColor*light.color@ $	float maxColor = max(colout.r,max(colout.g,colout.b))@$	float3 outColor = smoothstep(SMin,SMax,maxColor)*light.color@$	 Color += outColor@$	//Color += smoothstep(float3(Faloff,Faloff,Faloff),float3(0.5f,0.5f,0.5f),colout)@$}$$return Color@;3;Create;7;True;WorldPosition;FLOAT3;0,0,0;In;;Float;False;True;WorldNormal;FLOAT3;0,0,0;In;;Float;False;True;LightWrapVector;FLOAT3;0,0,0;In;;Inherit;False;True;SMin;FLOAT;0;In;;Inherit;False;True;SMax;FLOAT;0;In;;Inherit;False;True;Faloff;FLOAT;0;In;;Inherit;False;True;shadowmask;FLOAT4;1,1,1,1;In;;Inherit;False;AdditionalLight;False;False;0;;False;7;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT4;1,1,1,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;389;-8423.692,1747.842;Inherit;False;250;BNLightWarpVector;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;1881;-5914.11,1864.365;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;1882;-5914.11,1923.365;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;1880;-5984,1824;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;1885;-6112,1808;Inherit;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;243;-5386.463,4436.045;Float;False;BNspecularFinalColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;1886;-5504,5008;Inherit;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;627;-5408,4672;Inherit;False;Property;_UseSpecular;UseSpecular Highlights;2;0;Create;False;0;0;0;True;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CustomExpressionNode;1181;-7560.631,3127.085;Inherit;False;float3 Color = 0@$	$	$$#define SUM_LIGHT(Light)\$		float3 DotVector = dot(light.direction,WorldNormal)@	\$	half3 AttLightColor = (light.shadowAttenuation * light.distanceAttenuation)@\$	 float3 colout = max(float3(0.f,0.f,0.f),LightWrapVector + (1-LightWrapVector) * DotVector )*AttLightColor*light.color@ \$	float maxColor = max(colout.r,max(colout.g,colout.b))@\$	float3 outColor = smoothstep(SMin,SMax,maxColor)*light.color@\$	 Color += outColor@$$	InputData inputData = (InputData)0@$	inputData.normalizedScreenSpaceUV = ScreenUV@$	inputData.positionWS = WorldPosition@$$	uint meshRenderingLayers = GetMeshRenderingLayer()@$	uint pixelLightCount = GetAdditionalLightsCount()@$$	#if USE_FORWARD_PLUS$	for (uint lightIndex = 0@ lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS)@ lightIndex++)$	{$		FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK$$		Light light = GetAdditionalLight(lightIndex, WorldPosition,shadowmask)@$		#ifdef _LIGHT_LAYERS$		if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))$		#endif$		{$			SUM_LIGHT( light )@$		}$	}$	#endif$$	LIGHT_LOOP_BEGIN( pixelLightCount )$		Light light = GetAdditionalLight(lightIndex, WorldPosition,shadowmask)@$		#ifdef _LIGHT_LAYERS$		if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))$		#endif$		{$			SUM_LIGHT( light )@$		}$	LIGHT_LOOP_END$$$return Color@;3;Create;8;False;WorldPosition;FLOAT3;0,0,0;In;;Float;False;False;WorldNormal;FLOAT3;0,0,0;In;;Float;False;False;LightWrapVector;FLOAT3;0,0,0;In;;Inherit;False;False;SMin;FLOAT;0;In;;Inherit;False;False;SMax;FLOAT;0;In;;Inherit;False;False;Faloff;FLOAT;0;In;;Inherit;False;False;shadowmask;FLOAT4;1,1,1,1;In;;Inherit;False;True;ScreenUV;FLOAT2;0,0;In;;Inherit;False;AdditionalLight;False;False;0;;False;8;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT4;1,1,1,1;False;7;FLOAT2;0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1839;-8112,3376;Inherit;False;Shadowmask;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;1887;-8512,3408;Inherit;False;Shadow Mask;-1;;213;b50f5becdd6b8504a861ba5b9b861159;0;1;3;FLOAT2;0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;361;-6688,1584;Inherit;True;Property;_LightRampTexture;Light Ramp Texture;12;2;[NoScaleOffset];[SingleLineTexture];Create;False;0;0;0;True;1;;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.LightColorNode;1618;-6464,1776;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1888;-6288,1824;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1868;810.7261,969.3201;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=ShadowCaster;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1869;810.7261,969.3201;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;False;False;True;1;LightMode=DepthOnly;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1870;810.7261,969.3201;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1871;810.7261,969.3201;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;1;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=Universal2D;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1872;810.7261,969.3201;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;SceneSelectionPass;0;6;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1873;810.7261,969.3201;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ScenePickingPass;0;7;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1874;810.7261,969.3201;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthNormals;0;8;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormalsOnly;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1875;810.7261,969.3201;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthNormalsOnly;0;9;DepthNormalsOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormalsOnly;False;True;9;d3d11;metal;vulkan;xboxone;xboxseries;playstation;ps4;ps5;switch;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1866;794.142,646.0062;Float;False;False;-1;2;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;True;True;1;False;;True;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;True;True;1;False;;True;3;False;;True;False;0;False;;0;False;;True;0;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1867;1072,1008;Float;False;True;-1;2;ToonProEditor;0;13;Stylized Toon;2992e84f91cbeb14eab234972e07ea9d;True;Forward;0;1;Forward;8;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;3;True;12;all;0;False;True;1;1;False;;0;False;;1;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=UniversalForwardOnly;False;False;0;;0;0;Standard;22;Surface;0;0;  Blend;0;0;Two Sided;1;0;Forward Only;0;0;Cast Shadows;1;0;  Use Shadow Threshold;0;0;Receive Shadows;1;0;GPU Instancing;1;0;LOD CrossFade;1;638251859426275030;Built-in Fog;0;0;Meta Pass;0;638251859496447722;Extra Pre Pass;1;638251859501580475;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,;0;  Type;0;0;  Tess;16,False,;0;  Min;10,False,;0;  Max;25,False,;0;  Edge Length;16,False,;0;  Max Displacement;25,False,;0;Vertex Position,InvertActionOnDeselection;1;0;0;10;True;True;True;True;False;False;True;True;True;False;False;;False;0
WireConnection;1651;1;1669;0
WireConnection;1655;1;1651;1
WireConnection;1655;2;1652;0
WireConnection;1656;0;1655;0
WireConnection;1656;1;1655;0
WireConnection;1656;2;1655;0
WireConnection;362;1;1669;0
WireConnection;1653;0;204;0
WireConnection;1653;1;362;0
WireConnection;1653;2;1656;0
WireConnection;364;0;1653;0
WireConnection;671;0;670;0
WireConnection;604;0;592;0
WireConnection;604;1;57;0
WireConnection;591;1;601;0
WireConnection;1807;0;1805;0
WireConnection;1555;0;1026;0
WireConnection;704;0;712;0
WireConnection;704;1;702;1
WireConnection;1241;0;1245;0
WireConnection;1400;0;406;1
WireConnection;1400;1;1404;0
WireConnection;1400;2;1578;0
WireConnection;602;0;600;0
WireConnection;1637;0;1258;0
WireConnection;1637;1;1643;0
WireConnection;1637;2;461;0
WireConnection;55;0;50;0
WireConnection;55;1;51;0
WireConnection;1604;0;1599;0
WireConnection;606;1;57;0
WireConnection;606;0;604;0
WireConnection;1087;0;372;0
WireConnection;1087;1;1157;0
WireConnection;1064;0;1187;0
WireConnection;1064;3;1069;0
WireConnection;1064;4;1070;0
WireConnection;62;0;59;0
WireConnection;62;1;60;0
WireConnection;1561;0;441;1
WireConnection;1561;1;1557;0
WireConnection;600;0;598;2
WireConnection;600;1;599;0
WireConnection;1785;0;1784;0
WireConnection;592;0;590;0
WireConnection;592;1;596;0
WireConnection;1682;0;446;0
WireConnection;1264;0;530;0
WireConnection;1264;1;1270;0
WireConnection;1264;2;1253;4
WireConnection;1531;0;1527;0
WireConnection;1531;1;1527;0
WireConnection;673;0;671;3
WireConnection;1132;1;1123;0
WireConnection;1132;0;1859;0
WireConnection;1121;0;1119;0
WireConnection;1121;1;275;0
WireConnection;1121;2;1120;0
WireConnection;1121;3;1460;0
WireConnection;1293;0;1292;0
WireConnection;601;0;602;0
WireConnection;1269;0;1363;0
WireConnection;1462;0;1552;0
WireConnection;1462;1;1220;0
WireConnection;1462;2;1463;0
WireConnection;358;0;356;0
WireConnection;358;1;1274;0
WireConnection;358;2;1276;0
WireConnection;742;0;744;0
WireConnection;742;1;743;0
WireConnection;694;0;270;0
WireConnection;694;1;695;0
WireConnection;1102;1;1096;0
WireConnection;747;0;751;0
WireConnection;747;1;764;0
WireConnection;753;0;747;0
WireConnection;1486;0;1485;0
WireConnection;1266;1;392;0
WireConnection;1266;0;392;0
WireConnection;1266;2;392;0
WireConnection;1279;0;1278;0
WireConnection;486;0;1849;0
WireConnection;486;1;1792;0
WireConnection;1647;0;456;0
WireConnection;1647;1;1644;0
WireConnection;1287;0;1297;0
WireConnection;680;0;685;0
WireConnection;391;0;1087;0
WireConnection;57;1;52;0
WireConnection;57;0;53;0
WireConnection;57;2;54;0
WireConnection;732;1;739;0
WireConnection;732;0;731;0
WireConnection;60;0;606;0
WireConnection;60;1;58;0
WireConnection;590;0;591;0
WireConnection;590;1;593;0
WireConnection;1388;0;1376;0
WireConnection;1500;0;1501;0
WireConnection;1500;1;1505;0
WireConnection;1500;2;1508;0
WireConnection;766;0;767;0
WireConnection;1769;7;439;0
WireConnection;1221;0;691;0
WireConnection;763;0;749;0
WireConnection;763;1;766;0
WireConnection;1280;0;1279;0
WireConnection;764;0;745;0
WireConnection;764;1;766;0
WireConnection;1225;0;1234;0
WireConnection;1225;1;1222;0
WireConnection;1097;0;1573;0
WireConnection;1097;1;1098;0
WireConnection;1097;2;1104;0
WireConnection;1554;0;372;0
WireConnection;1231;1;271;0
WireConnection;1231;0;1232;0
WireConnection;1231;2;271;0
WireConnection;1470;1;1416;0
WireConnection;1470;0;1486;0
WireConnection;1047;0;488;0
WireConnection;1047;1;1048;0
WireConnection;1047;2;1049;0
WireConnection;1351;0;508;1
WireConnection;1622;1;1623;0
WireConnection;1774;0;1785;0
WireConnection;1774;1;1225;0
WireConnection;1774;2;1783;0
WireConnection;1538;0;1537;0
WireConnection;1680;0;361;0
WireConnection;1680;1;1880;0
WireConnection;1626;0;1451;0
WireConnection;1626;1;1619;0
WireConnection;1626;2;1470;0
WireConnection;227;0;247;1
WireConnection;227;1;1412;3
WireConnection;692;1;276;0
WireConnection;692;2;1221;0
WireConnection;1489;0;1514;0
WireConnection;1489;1;1492;0
WireConnection;1489;2;1488;0
WireConnection;220;0;1579;0
WireConnection;220;1;903;0
WireConnection;220;2;1397;0
WireConnection;220;3;1403;0
WireConnection;1234;0;1462;0
WireConnection;1234;1;1235;0
WireConnection;1051;0;1047;0
WireConnection;1331;0;1355;0
WireConnection;1331;1;1330;0
WireConnection;1094;0;804;0
WireConnection;1094;1;1103;0
WireConnection;1040;0;1041;0
WireConnection;715;0;705;0
WireConnection;1841;0;439;0
WireConnection;1841;1;1843;0
WireConnection;1648;0;1644;0
WireConnection;1129;0;1842;0
WireConnection;1129;1;1793;0
WireConnection;1294;0;1295;0
WireConnection;1294;1;1296;0
WireConnection;1817;0;1816;0
WireConnection;1816;0;1815;0
WireConnection;1815;0;1814;0
WireConnection;1488;0;1496;0
WireConnection;1814;0;1813;0
WireConnection;1810;0;1811;0
WireConnection;1810;1;1808;0
WireConnection;677;0;682;0
WireConnection;677;1;679;0
WireConnection;1804;0;1807;0
WireConnection;726;0;716;0
WireConnection;726;1;727;0
WireConnection;1818;0;1812;0
WireConnection;1818;1;1812;0
WireConnection;233;0;234;0
WireConnection;1556;0;1555;0
WireConnection;1288;0;1287;0
WireConnection;1288;1;1294;0
WireConnection;248;0;249;0
WireConnection;1579;0;1097;0
WireConnection;1579;1;1400;0
WireConnection;1806;0;1807;0
WireConnection;267;0;389;0
WireConnection;267;1;251;0
WireConnection;685;0;686;0
WireConnection;685;2;677;0
WireConnection;1123;0;439;0
WireConnection;1127;0;699;0
WireConnection;1125;0;1124;0
WireConnection;378;0;375;1
WireConnection;378;1;379;0
WireConnection;1681;1;446;0
WireConnection;1681;0;1682;0
WireConnection;1466;0;692;0
WireConnection;1811;0;1809;0
WireConnection;749;0;748;0
WireConnection;749;1;741;1
WireConnection;1278;0;1285;0
WireConnection;250;0;384;0
WireConnection;1650;0;1645;0
WireConnection;1650;1;456;0
WireConnection;1650;2;1648;0
WireConnection;751;0;763;0
WireConnection;1216;0;1217;0
WireConnection;1216;1;1462;0
WireConnection;1216;2;1045;0
WireConnection;59;1;55;0
WireConnection;59;2;56;0
WireConnection;1573;0;1577;0
WireConnection;1573;1;805;0
WireConnection;1573;2;1574;0
WireConnection;1573;3;1576;0
WireConnection;1573;4;1094;0
WireConnection;1573;5;588;0
WireConnection;1573;6;207;0
WireConnection;1277;0;1281;0
WireConnection;1277;2;1281;0
WireConnection;621;0;661;0
WireConnection;1808;0;1806;0
WireConnection;1808;1;1804;0
WireConnection;588;0;583;0
WireConnection;1537;0;1536;0
WireConnection;260;0;262;0
WireConnection;260;1;240;0
WireConnection;263;0;229;0
WireConnection;263;1;1269;0
WireConnection;1851;0;1848;1
WireConnection;1851;1;1848;2
WireConnection;1022;0;1226;0
WireConnection;1355;0;1359;0
WireConnection;1355;1;1356;0
WireConnection;1270;0;1253;0
WireConnection;1270;1;1468;0
WireConnection;745;0;742;0
WireConnection;745;1;741;1
WireConnection;274;0;259;0
WireConnection;1642;0;1646;0
WireConnection;1363;0;642;0
WireConnection;1363;1;692;0
WireConnection;239;0;263;0
WireConnection;1330;0;1329;0
WireConnection;262;0;267;0
WireConnection;262;1;264;0
WireConnection;375;1;1489;0
WireConnection;762;0;753;0
WireConnection;762;1;740;0
WireConnection;1617;0;1622;0
WireConnection;1617;1;1680;0
WireConnection;1617;2;1470;0
WireConnection;1803;0;1277;0
WireConnection;1803;1;1277;0
WireConnection;653;0;660;0
WireConnection;653;1;651;0
WireConnection;653;2;1102;0
WireConnection;441;1;1213;0
WireConnection;721;0;708;0
WireConnection;1843;0;1840;1
WireConnection;1843;1;1840;2
WireConnection;1665;1;1129;0
WireConnection;1665;0;1769;0
WireConnection;1045;0;531;0
WireConnection;1045;1;1040;0
WireConnection;1045;2;1043;0
WireConnection;1625;0;1360;0
WireConnection;1557;0;441;1
WireConnection;1557;1;1632;0
WireConnection;1557;2;1634;0
WireConnection;718;0;756;0
WireConnection;718;1;755;0
WireConnection;460;0;464;0
WireConnection;1296;0;1293;0
WireConnection;1501;0;1502;0
WireConnection;1501;1;1504;0
WireConnection;1295;0;1293;0
WireConnection;1265;0;490;0
WireConnection;902;0;437;0
WireConnection;1541;1;1669;0
WireConnection;1628;0;1451;0
WireConnection;1628;1;1629;0
WireConnection;1628;2;1470;0
WireConnection;965;0;1683;0
WireConnection;516;0;517;0
WireConnection;516;1;1351;0
WireConnection;1771;7;1056;0
WireConnection;1611;0;1605;0
WireConnection;1249;0;1637;0
WireConnection;1249;1;681;0
WireConnection;372;1;1626;0
WireConnection;372;0;1617;0
WireConnection;372;2;1628;0
WireConnection;237;0;222;0
WireConnection;237;1;236;0
WireConnection;498;0;492;0
WireConnection;498;1;499;0
WireConnection;1646;1;456;0
WireConnection;1646;0;1647;0
WireConnection;1646;2;1650;0
WireConnection;1523;0;1524;0
WireConnection;1362;0;1358;0
WireConnection;1187;1;1121;0
WireConnection;1281;0;1280;0
WireConnection;1530;0;1664;0
WireConnection;1530;1;1531;0
WireConnection;1530;2;1522;0
WireConnection;1498;1;1500;0
WireConnection;1498;0;1418;0
WireConnection;755;0;738;0
WireConnection;755;1;726;0
WireConnection;1683;0;1684;0
WireConnection;1683;1;1498;0
WireConnection;464;0;732;0
WireConnection;488;1;478;0
WireConnection;488;0;1282;0
WireConnection;705;0;704;0
WireConnection;705;1;719;0
WireConnection;1049;0;1050;0
WireConnection;1236;0;1216;0
WireConnection;1349;1;1350;0
WireConnection;1349;0;673;0
WireConnection;259;0;1231;0
WireConnection;259;1;694;0
WireConnection;259;2;1366;0
WireConnection;1099;0;1096;0
WireConnection;462;0;454;0
WireConnection;462;1;467;0
WireConnection;1519;0;1518;0
WireConnection;618;0;619;0
WireConnection;618;1;644;0
WireConnection;1359;0;1357;0
WireConnection;365;0;62;0
WireConnection;265;0;238;0
WireConnection;1360;0;444;0
WireConnection;1104;0;1093;0
WireConnection;356;0;360;0
WireConnection;1619;0;1451;0
WireConnection;1619;1;1881;0
WireConnection;1619;2;1625;0
WireConnection;491;0;1053;0
WireConnection;491;1;1064;0
WireConnection;491;2;481;0
WireConnection;491;3;482;0
WireConnection;491;4;498;0
WireConnection;491;5;503;0
WireConnection;716;0;715;0
WireConnection;1237;1;693;0
WireConnection;1237;0;1238;0
WireConnection;1237;2;693;0
WireConnection;1629;0;1451;0
WireConnection;1629;1;1882;0
WireConnection;1629;2;1331;0
WireConnection;1636;0;530;0
WireConnection;650;0;653;0
WireConnection;650;1;649;0
WireConnection;652;0;650;0
WireConnection;252;0;254;0
WireConnection;252;1;256;0
WireConnection;1111;0;1066;0
WireConnection;1485;0;1416;0
WireConnection;1485;1;1483;0
WireConnection;1485;2;1510;0
WireConnection;466;0;463;0
WireConnection;737;0;753;0
WireConnection;737;1;740;0
WireConnection;384;0;382;0
WireConnection;384;2;378;0
WireConnection;244;0;227;0
WireConnection;1510;0;1509;0
WireConnection;1510;2;1511;0
WireConnection;528;0;526;0
WireConnection;528;1;527;0
WireConnection;636;1;1669;0
WireConnection;1103;1;1093;0
WireConnection;467;0;466;0
WireConnection;1632;0;1634;0
WireConnection;1257;0;1254;0
WireConnection;1257;1;1795;0
WireConnection;1257;2;1256;0
WireConnection;1514;1;1495;0
WireConnection;1514;0;1530;0
WireConnection;222;0;226;1
WireConnection;1364;0;621;0
WireConnection;1511;0;1416;0
WireConnection;238;0;237;0
WireConnection;1041;0;1046;0
WireConnection;738;0;462;0
WireConnection;360;0;359;0
WireConnection;360;1;1188;0
WireConnection;731;0;718;0
WireConnection;731;1;757;0
WireConnection;1520;0;1519;0
WireConnection;1520;1;1521;0
WireConnection;695;1;1797;0
WireConnection;695;2;696;0
WireConnection;1188;1;260;0
WireConnection;282;0;1635;0
WireConnection;282;1;681;0
WireConnection;1418;0;260;0
WireConnection;1418;1;1417;0
WireConnection;757;0;758;0
WireConnection;757;1;759;0
WireConnection;638;0;658;0
WireConnection;1536;0;1540;0
WireConnection;1536;1;1541;0
WireConnection;1536;2;1539;0
WireConnection;1661;1;486;0
WireConnection;1661;0;1771;0
WireConnection;1521;0;1518;0
WireConnection;437;0;438;0
WireConnection;437;2;1681;0
WireConnection;444;0;1188;0
WireConnection;444;1;445;0
WireConnection;444;2;371;0
WireConnection;1260;0;1264;0
WireConnection;1260;1;530;0
WireConnection;1260;2;1556;0
WireConnection;1069;0;1067;0
WireConnection;234;0;1542;0
WireConnection;234;1;269;0
WireConnection;1668;0;1794;0
WireConnection;1668;1;1847;0
WireConnection;276;0;581;0
WireConnection;517;0;529;0
WireConnection;517;1;525;0
WireConnection;1522;0;1523;0
WireConnection;1522;1;1520;0
WireConnection;1186;1;966;0
WireConnection;371;0;370;0
WireConnection;476;0;1053;0
WireConnection;476;1;1064;0
WireConnection;476;2;481;0
WireConnection;476;3;482;0
WireConnection;446;1;1561;0
WireConnection;446;2;440;0
WireConnection;1518;0;1517;0
WireConnection;1528;0;1526;0
WireConnection;1634;0;1631;0
WireConnection;665;0;652;0
WireConnection;1527;0;1528;0
WireConnection;1043;0;1042;0
WireConnection;1578;0;1100;0
WireConnection;1578;1;207;0
WireConnection;1578;2;588;0
WireConnection;454;0;537;0
WireConnection;454;2;458;0
WireConnection;454;3;457;0
WireConnection;1605;0;1604;0
WireConnection;1605;1;1600;0
WireConnection;1526;0;1525;0
WireConnection;445;0;371;0
WireConnection;759;0;762;0
WireConnection;759;1;760;0
WireConnection;1782;0;1045;0
WireConnection;525;0;1186;0
WireConnection;525;1;526;0
WireConnection;525;2;528;0
WireConnection;1357;0;1188;0
WireConnection;1357;1;1362;0
WireConnection;246;0;252;0
WireConnection;1024;1;282;0
WireConnection;1024;0;282;0
WireConnection;1024;2;1249;0
WireConnection;251;0;248;0
WireConnection;251;1;223;0
WireConnection;1819;0;1132;0
WireConnection;1819;1;1810;0
WireConnection;1070;0;1111;0
WireConnection;1226;0;1774;0
WireConnection;1226;1;1465;0
WireConnection;661;0;618;0
WireConnection;661;1;623;0
WireConnection;661;2;644;0
WireConnection;1784;0;1225;0
WireConnection;1784;1;1217;0
WireConnection;1784;2;1217;4
WireConnection;719;0;721;0
WireConnection;1525;0;1529;0
WireConnection;1100;0;401;0
WireConnection;1100;1;1099;0
WireConnection;229;0;1266;0
WireConnection;229;1;642;0
WireConnection;490;1;476;0
WireConnection;490;0;491;0
WireConnection;1254;0;1270;0
WireConnection;1254;1;1795;0
WireConnection;1254;2;1555;0
WireConnection;1664;1;1668;0
WireConnection;1664;0;1770;0
WireConnection;1495;0;381;0
WireConnection;1495;1;1522;0
WireConnection;1448;1;362;0
WireConnection;1448;2;1449;0
WireConnection;1856;0;1790;0
WireConnection;1856;1;1858;0
WireConnection;1415;0;1854;0
WireConnection;401;0;246;0
WireConnection;401;1;665;0
WireConnection;1421;0;44;0
WireConnection;1421;1;1448;0
WireConnection;682;1;1669;0
WireConnection;1684;1;1211;0
WireConnection;1783;0;1782;0
WireConnection;767;0;765;0
WireConnection;478;0;1056;0
WireConnection;478;1;1288;0
WireConnection;1635;0;1636;0
WireConnection;1635;1;1643;0
WireConnection;1635;2;461;0
WireConnection;1850;0;1056;0
WireConnection;1850;1;1851;0
WireConnection;1025;0;1265;0
WireConnection;1042;0;1046;0
WireConnection;531;1;516;0
WireConnection;1797;0;1237;0
WireConnection;1247;0;1562;0
WireConnection;508;1;1055;0
WireConnection;1124;0;1819;0
WireConnection;1124;1;1126;0
WireConnection;1124;2;1127;0
WireConnection;1203;0;1247;0
WireConnection;1468;1;1467;0
WireConnection;1468;2;1469;0
WireConnection;1842;0;1841;0
WireConnection;1842;1;439;0
WireConnection;1770;7;381;0
WireConnection;1858;0;1855;1
WireConnection;1858;1;1855;2
WireConnection;1256;0;1253;4
WireConnection;1245;0;1203;0
WireConnection;1245;1;1243;0
WireConnection;1854;1;1852;0
WireConnection;1854;2;1412;4
WireConnection;1854;3;1853;0
WireConnection;1849;0;1850;0
WireConnection;1849;1;1056;0
WireConnection;1258;1;1260;0
WireConnection;1258;0;1257;0
WireConnection;1282;0;1661;0
WireConnection;1282;1;1803;0
WireConnection;1282;2;1288;0
WireConnection;1812;1;1817;0
WireConnection;1812;0;1817;0
WireConnection;1812;2;1817;0
WireConnection;1847;0;1846;0
WireConnection;1847;1;381;0
WireConnection;1859;0;1665;0
WireConnection;1859;1;1818;0
WireConnection;1795;1;1789;0
WireConnection;1562;0;1202;0
WireConnection;658;0;636;1
WireConnection;658;1;659;0
WireConnection;1845;0;1844;1
WireConnection;1845;1;1844;2
WireConnection;1789;0;1791;0
WireConnection;1789;1;1857;0
WireConnection;1846;0;381;0
WireConnection;1846;1;1845;0
WireConnection;1857;0;1856;0
WireConnection;1857;1;1790;0
WireConnection;42;0;1421;0
WireConnection;42;1;1024;0
WireConnection;1412;1;1414;0
WireConnection;1877;0;1876;0
WireConnection;1143;0;1181;0
WireConnection;1144;0;1143;0
WireConnection;1828;32;1839;0
WireConnection;1878;0;1182;0
WireConnection;1878;1;1154;0
WireConnection;1878;2;1198;0
WireConnection;1878;3;1241;0
WireConnection;1878;4;1203;0
WireConnection;1878;6;1839;0
WireConnection;1881;0;1885;0
WireConnection;1882;0;1885;0
WireConnection;1880;0;1885;0
WireConnection;1885;0;1888;0
WireConnection;243;0;627;0
WireConnection;1886;0;220;0
WireConnection;627;0;1886;0
WireConnection;1181;0;1182;0
WireConnection;1181;1;1154;0
WireConnection;1181;2;1198;0
WireConnection;1181;3;1241;0
WireConnection;1181;4;1203;0
WireConnection;1181;6;1839;0
WireConnection;1181;7;1877;0
WireConnection;1839;0;1887;0
WireConnection;361;1;358;0
WireConnection;1888;0;1618;0
WireConnection;1888;1;1618;2
WireConnection;1866;0;42;0
WireConnection;1866;1;1349;0
WireConnection;1866;3;365;0
WireConnection;1867;2;42;0
WireConnection;1867;3;673;0
WireConnection;1867;4;674;0
ASEEND*/
//CHKSM=A6F9C11CB1C2A880FAA7CB0BF17F01298994BF47