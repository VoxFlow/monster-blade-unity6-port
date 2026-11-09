// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Rebuilt for Unity 6000.3.8f1 from the shipped Unity 4.0.1f2 shader (07c0db656e2a885449e0dcfb976c0bba__1, rule: unique ShaderLab name among 109 shipped Shader objects).
// ShaderLab state copied verbatim; compiled GLES programs translated 1:1 to Cg by gles2cg.py.
// Variants with LIGHTMAP_ON, DIRLIGHTMAP_ON, VERTEXLIGHT_ON, POINT, SPOT, POINT_COOKIE, DIRECTIONAL_COOKIE not translated: shipped data: 0 lightmaps in all 23 scenes (LightmapSettings), no renderer lightmap index, only Directional lights (3 in scenes, 2 in prefabs), no code that creates or retypes lights (Tools/light_census.py, grep 2026-10-30); no Point/Spot lights and no light cookies (all shipped lights have m_Cookie null); ForwardAdd keeps DIRECTIONAL
Shader "Mobile/Monster Slayer/Greyscale Texture With Saturation" {
Properties {
 _Sat ("Saturation", Range(0.3,3)) = 1
 _Degree ("GrayDegree", Range(0,1)) = 0
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
float _Degree;
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
  float3 tmpvar_2;
  float tmpvar_3;
  float4 c_4;
  float4 tmpvar_5;
  tmpvar_5 = tex2D(_MainTex, i.xlv_TEXCOORD0);
  c_4 = tmpvar_5;
  float3 tmpvar_6;
  tmpvar_6.x = (c_4.x + (_Degree * (0.333000 - c_4.x)));
  tmpvar_6.y = (c_4.y + (_Degree * (0.333000 - c_4.y)));
  tmpvar_6.z = (c_4.z + (_Degree * (0.333000 - c_4.z)));
  tmpvar_2 = tmpvar_6;
  float tmpvar_7;
  tmpvar_7 = c_4.w;
  tmpvar_3 = tmpvar_7;
  float4 c_8;
  c_8.xyz = ((tmpvar_2 * _LightColor0.xyz) * (max (0.00000, dot (i.xlv_TEXCOORD1, _WorldSpaceLightPos0.xyz)) * 2.00000));
  c_8.w = tmpvar_3;
  c_1.xyz = (c_8.xyz + (tmpvar_2 * i.xlv_TEXCOORD2));
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
float _Degree;
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
  float3 tmpvar_3;
  float tmpvar_4;
  float4 c_5;
  float4 tmpvar_6;
  tmpvar_6 = tex2D(_MainTex, i.xlv_TEXCOORD0);
  c_5 = tmpvar_6;
  float3 tmpvar_7;
  tmpvar_7.x = (c_5.x + (_Degree * (0.333000 - c_5.x)));
  tmpvar_7.y = (c_5.y + (_Degree * (0.333000 - c_5.y)));
  tmpvar_7.z = (c_5.z + (_Degree * (0.333000 - c_5.z)));
  tmpvar_3 = tmpvar_7;
  float tmpvar_8;
  tmpvar_8 = c_5.w;
  tmpvar_4 = tmpvar_8;
  lightDir_2 = i.xlv_TEXCOORD2;
  float4 c_9;
  c_9.xyz = ((tmpvar_3 * _LightColor0.xyz) * (max (0.00000, dot (i.xlv_TEXCOORD1, lightDir_2)) * 2.00000));
  c_9.w = tmpvar_4;
  c_1.xyz = c_9.xyz;
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