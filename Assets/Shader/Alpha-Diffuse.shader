// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Rebuilt for Unity 6000.3.8f1 from the shipped Unity 4.0.1f2 shader (ac2f738fc091938479249bef23fba9a5__1, rule: duplicate name; the only object referenced by shipped materials (5 uses)).
// ShaderLab state copied verbatim; compiled GLES programs translated 1:1 to Cg by gles2cg.py.
// Variants with LIGHTMAP_ON, DIRLIGHTMAP_ON, VERTEXLIGHT_ON, POINT, SPOT, POINT_COOKIE, DIRECTIONAL_COOKIE not translated: shipped data: 0 lightmaps in all 23 scenes (LightmapSettings), no renderer lightmap index, only Directional lights (3 in scenes, 2 in prefabs), no code that creates or retypes lights (Tools/light_census.py, grep 2026-10-30); no Point/Spot lights and no light cookies (all shipped lights have m_Cookie null); ForwardAdd keeps DIRECTIONAL
Shader "Transparent/Diffuse" {
Properties {
 _Color ("Main Color", Color) = (1,1,1,1)
 _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}
SubShader { 
 LOD 200
 Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
 Pass {
  Name "FORWARD"
  Tags { "LIGHTMODE"="ForwardBase" "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
  ZWrite Off
  Blend SrcAlpha OneMinusSrcAlpha
  // AlphaTest Greater 0 -> applied in frag
  ColorMask RGB
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
#include "UnityLightingCommon.cginc"
#if defined(UNITY_COLORSPACE_GAMMA)
#define MB_SH_GAMMA(x) LinearToGammaSpace(x)
#else
#define MB_SH_GAMMA(x) (x)
#endif
float4 _Color;
sampler2D _MainTex;
float4 _MainTex_ST;
struct appdata_t {
	float4 texcoord : TEXCOORD0;
	float3 normal : NORMAL;
	float4 vertex : POSITION;
};
struct v2f {
	float4 pos : SV_POSITION;
	float3 xlv_TEXCOORD2 : TEXCOORD0;
	float3 xlv_TEXCOORD1 : TEXCOORD1;
	float2 xlv_TEXCOORD0 : TEXCOORD2;
};
v2f vert(appdata_t v)
{
	v2f o;
	UNITY_INITIALIZE_OUTPUT(v2f, o);
  float3 shlight_1;
  float3 tmpvar_2;
  float3 tmpvar_3;
  float3x3 tmpvar_4;
  tmpvar_4[0] = unity_ObjectToWorld[0].xyz;
  tmpvar_4[1] = unity_ObjectToWorld[1].xyz;
  tmpvar_4[2] = unity_ObjectToWorld[2].xyz;
  float3 tmpvar_5;
  tmpvar_5 = (normalize(mul(normalize(v.normal), (float3x3)unity_WorldToObject)));
  tmpvar_2 = tmpvar_5;
  float4 tmpvar_6;
  tmpvar_6.w = 1.00000;
  tmpvar_6.xyz = tmpvar_5;
  float3 tmpvar_7;
  float4 normal_8;
  normal_8 = tmpvar_6;
  float3 x3_9;
  float vC_10;
  float3 x2_11;
  float3 x1_12;
  float tmpvar_13;
  tmpvar_13 = dot (unity_SHAr, normal_8);
  x1_12.x = tmpvar_13;
  float tmpvar_14;
  tmpvar_14 = dot (unity_SHAg, normal_8);
  x1_12.y = tmpvar_14;
  float tmpvar_15;
  tmpvar_15 = dot (unity_SHAb, normal_8);
  x1_12.z = tmpvar_15;
  float4 tmpvar_16;
  tmpvar_16 = (normal_8.xyzz * normal_8.yzzx);
  float tmpvar_17;
  tmpvar_17 = dot (unity_SHBr, tmpvar_16);
  x2_11.x = tmpvar_17;
  float tmpvar_18;
  tmpvar_18 = dot (unity_SHBg, tmpvar_16);
  x2_11.y = tmpvar_18;
  float tmpvar_19;
  tmpvar_19 = dot (unity_SHBb, tmpvar_16);
  x2_11.z = tmpvar_19;
  float tmpvar_20;
  tmpvar_20 = ((normal_8.x * normal_8.x) - (normal_8.y * normal_8.y));
  vC_10 = tmpvar_20;
  float3 tmpvar_21;
  tmpvar_21 = (unity_SHC.xyz * vC_10);
  x3_9 = tmpvar_21;
  tmpvar_7 = ((x1_12 + x2_11) + x3_9);
  shlight_1 = MB_SH_GAMMA(tmpvar_7);
  tmpvar_3 = shlight_1;
  o.pos = (UnityObjectToClipPos(v.vertex));
  o.xlv_TEXCOORD0 = ((v.texcoord.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  o.xlv_TEXCOORD1 = tmpvar_2;
  o.xlv_TEXCOORD2 = tmpvar_3;
	return o;
}
float4 frag(v2f i) : SV_Target
{
	float4 _fragOut = 0;
  float4 c_1;
  float4 tmpvar_2;
  tmpvar_2 = (tex2D(_MainTex, i.xlv_TEXCOORD0) * _Color);
  float tmpvar_3;
  tmpvar_3 = tmpvar_2.w;
  float4 c_4;
  c_4.xyz = ((tmpvar_2.xyz * _LightColor0.xyz) * (max (0.00000, dot (i.xlv_TEXCOORD1, _WorldSpaceLightPos0.xyz)) * 2.00000));
  c_4.w = tmpvar_3;
  c_1.xyz = (c_4.xyz + (tmpvar_2.xyz * i.xlv_TEXCOORD2));
  c_1.w = tmpvar_3;
  _fragOut = c_1;
	// original pass state "AlphaTest Greater 0" (fixed-function alpha test, not available to programmable passes in Unity 5+)
	if (!(_fragOut.a > 0)) discard;
	return _fragOut;
}
ENDCG

 }
 Pass {
  Name "FORWARD"
  Tags { "LIGHTMODE"="ForwardAdd" "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
  ZWrite Off
  Fog {
   Color (0,0,0,0)
  }
  Blend SrcAlpha One
  // AlphaTest Greater 0 -> applied in frag
  ColorMask RGB
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
#include "UnityLightingCommon.cginc"
float4 _Color;
sampler2D _MainTex;
float4 _MainTex_ST;
struct appdata_t {
	float4 texcoord : TEXCOORD0;
	float3 normal : NORMAL;
	float4 vertex : POSITION;
};
struct v2f {
	float4 pos : SV_POSITION;
	float3 xlv_TEXCOORD2 : TEXCOORD0;
	float3 xlv_TEXCOORD1 : TEXCOORD1;
	float2 xlv_TEXCOORD0 : TEXCOORD2;
};
v2f vert(appdata_t v)
{
	v2f o;
	UNITY_INITIALIZE_OUTPUT(v2f, o);
  float3 tmpvar_1;
  float3 tmpvar_2;
  float3x3 tmpvar_3;
  tmpvar_3[0] = unity_ObjectToWorld[0].xyz;
  tmpvar_3[1] = unity_ObjectToWorld[1].xyz;
  tmpvar_3[2] = unity_ObjectToWorld[2].xyz;
  float3 tmpvar_4;
  tmpvar_4 = (normalize(mul(normalize(v.normal), (float3x3)unity_WorldToObject)));
  tmpvar_1 = tmpvar_4;
  float3 tmpvar_5;
  tmpvar_5 = _WorldSpaceLightPos0.xyz;
  tmpvar_2 = tmpvar_5;
  o.pos = (UnityObjectToClipPos(v.vertex));
  o.xlv_TEXCOORD0 = ((v.texcoord.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  o.xlv_TEXCOORD1 = tmpvar_1;
  o.xlv_TEXCOORD2 = tmpvar_2;
	return o;
}
float4 frag(v2f i) : SV_Target
{
	float4 _fragOut = 0;
  float4 c_1;
  float3 lightDir_2;
  float4 tmpvar_3;
  tmpvar_3 = (tex2D(_MainTex, i.xlv_TEXCOORD0) * _Color);
  float tmpvar_4;
  tmpvar_4 = tmpvar_3.w;
  lightDir_2 = i.xlv_TEXCOORD2;
  float4 c_5;
  c_5.xyz = ((tmpvar_3.xyz * _LightColor0.xyz) * (max (0.00000, dot (i.xlv_TEXCOORD1, lightDir_2)) * 2.00000));
  c_5.w = tmpvar_4;
  c_1.xyz = c_5.xyz;
  c_1.w = tmpvar_4;
  _fragOut = c_1;
	// original pass state "AlphaTest Greater 0" (fixed-function alpha test, not available to programmable passes in Unity 5+)
	if (!(_fragOut.a > 0)) discard;
	return _fragOut;
}
ENDCG

 }
}
Fallback "Transparent/VertexLit"
}