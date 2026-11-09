// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Rebuilt for Unity 6000.3.8f1 from the shipped Unity 4.0.1f2 shader (unity default resources__200, rule: shipped unity default resources, Shader pathID 200, 4 materials).
// ShaderLab state copied verbatim; compiled GLES programs translated 1:1 to Cg by gles2cg.py.
Shader "Particles/Additive" {
Properties {
 _TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
 _MainTex ("Particle Texture", 2D) = "white" {}
 _InvFade ("Soft Particles Factor", Range(0.01,3)) = 1
}
SubShader { 
 Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
 Pass {
  Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
  BindChannels {
   Bind "vertex", Vertex
   Bind "color", Color
   Bind "texcoord", TexCoord
  }
  ZWrite Off
  Cull Off
  Fog {
   Color (0,0,0,0)
  }
  Blend SrcAlpha One
  // AlphaTest Greater 0.01 -> applied in frag
  ColorMask RGB
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile SOFTPARTICLES_OFF SOFTPARTICLES_ON
#include "UnityCG.cginc"
sampler2D_float _CameraDepthTexture;
#if defined(SOFTPARTICLES_OFF)
sampler2D _MainTex;
float4 _MainTex_ST;
float4 _TintColor;
struct appdata_t {
	float4 texcoord : TEXCOORD0;
	float4 color : COLOR;
	float4 vertex : POSITION;
};
struct v2f {
	float4 pos : SV_POSITION;
	float2 xlv_TEXCOORD0 : TEXCOORD0;
	float4 xlv_COLOR : TEXCOORD1;
};
v2f vert(appdata_t v)
{
	v2f o;
	UNITY_INITIALIZE_OUTPUT(v2f, o);
  o.pos = (UnityObjectToClipPos(v.vertex));
  o.xlv_COLOR = v.color;
  o.xlv_TEXCOORD0 = ((v.texcoord.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
	return o;
}
float4 frag(v2f i) : SV_Target
{
	float4 _fragOut = 0;
  _fragOut = (((2.00000 * i.xlv_COLOR) * _TintColor) * tex2D(_MainTex, i.xlv_TEXCOORD0));
	// original pass state "AlphaTest Greater 0.01" (fixed-function alpha test, not available to programmable passes in Unity 5+)
	if (!(_fragOut.a > 0.01)) discard;
	return _fragOut;
}
#elif defined(SOFTPARTICLES_ON)
float _InvFade;
sampler2D _MainTex;
float4 _MainTex_ST;
float4 _TintColor;
struct appdata_t {
	float4 texcoord : TEXCOORD0;
	float4 color : COLOR;
	float4 vertex : POSITION;
};
struct v2f {
	float4 pos : SV_POSITION;
	float4 xlv_TEXCOORD1 : TEXCOORD0;
	float2 xlv_TEXCOORD0 : TEXCOORD1;
	float4 xlv_COLOR : TEXCOORD2;
};
v2f vert(appdata_t v)
{
	v2f o;
	UNITY_INITIALIZE_OUTPUT(v2f, o);
  float4 tmpvar_1;
  float4 tmpvar_2;
  tmpvar_2 = (UnityObjectToClipPos(v.vertex));
  float4 o_3;
  float4 tmpvar_4;
  tmpvar_4 = (tmpvar_2 * 0.500000);
  float2 tmpvar_5;
  tmpvar_5.x = tmpvar_4.x;
  tmpvar_5.y = (tmpvar_4.y * _ProjectionParams.x);
  o_3.xy = (tmpvar_5 + tmpvar_4.w);
  o_3.zw = tmpvar_2.zw;
  tmpvar_1.xyw = o_3.xyw;
  tmpvar_1.z = -((mul(UNITY_MATRIX_MV, v.vertex)).z);
  o.pos = tmpvar_2;
  o.xlv_COLOR = v.color;
  o.xlv_TEXCOORD0 = ((v.texcoord.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  o.xlv_TEXCOORD1 = tmpvar_1;
	return o;
}
float4 frag(v2f i) : SV_Target
{
	float4 _fragOut = 0;
  float4 tmpvar_1;
  tmpvar_1.xyz = i.xlv_COLOR.xyz;
  float4 tmpvar_2;
  tmpvar_2 = tex2Dproj(_CameraDepthTexture, i.xlv_TEXCOORD1);
  float z_3;
  z_3 = tmpvar_2.x;
  float tmpvar_4;
  tmpvar_4 = (i.xlv_COLOR.w * clamp ((_InvFade * ((1.0/(((_ZBufferParams.z * z_3) + _ZBufferParams.w))) - i.xlv_TEXCOORD1.z)), 0.000000, 1.00000));
  tmpvar_1.w = tmpvar_4;
  _fragOut = (((2.00000 * tmpvar_1) * _TintColor) * tex2D(_MainTex, i.xlv_TEXCOORD0));
	// original pass state "AlphaTest Greater 0.01" (fixed-function alpha test, not available to programmable passes in Unity 5+)
	if (!(_fragOut.a > 0.01)) discard;
	return _fragOut;
}
#endif
ENDCG

 }
}
SubShader { 
 Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
 Pass {
  Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
  BindChannels {
   Bind "vertex", Vertex
   Bind "color", Color
   Bind "texcoord", TexCoord
  }
  ZWrite Off
  Cull Off
  Fog {
   Color (0,0,0,0)
  }
  Blend SrcAlpha One
  AlphaTest Greater 0.01
  ColorMask RGB
  SetTexture [_MainTex] { ConstantColor [_TintColor] combine constant * primary }
  SetTexture [_MainTex] { combine texture * previous double }
 }
}
SubShader { 
 Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
 Pass {
  Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
  BindChannels {
   Bind "vertex", Vertex
   Bind "color", Color
   Bind "texcoord", TexCoord
  }
  ZWrite Off
  Cull Off
  Fog {
   Color (0,0,0,0)
  }
  Blend SrcAlpha One
  AlphaTest Greater 0.01
  ColorMask RGB
  SetTexture [_MainTex] { combine texture * primary }
 }
}
}