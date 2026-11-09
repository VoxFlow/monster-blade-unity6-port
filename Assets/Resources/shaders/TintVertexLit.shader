// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Rebuilt for Unity 6000.3.8f1 from the shipped Unity 4.0.1f2 shader (c1bf946d9ae7f5b4aaa8511365419924__1, rule: unique ShaderLab name among 109 shipped Shader objects).
// ShaderLab state copied verbatim; compiled GLES programs translated 1:1 to Cg by gles2cg.py.
Shader "Mobile/Monster Slayer/Tint Vertex Lit" {
Properties {
 _Color ("Main Color", Color) = (1,1,1,1)
 _TintColor ("Tint Color", Color) = (1,1,1,1)
 _Brightness ("Brightness", Range(0.01,2)) = 1
 _MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader { 
 LOD 80
 Tags { "RenderType"="Opaque" }
 Pass {
  Tags { "RenderType"="Opaque" }
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
float _Brightness;
float4 _Color;
sampler2D _MainTex;
float4 _TintColor;
struct appdata_t {
	float4 texcoord : TEXCOORD0;
	float4 vertex : POSITION;
};
struct v2f {
	float4 pos : SV_POSITION;
	float2 xlv_TEXCOORD0 : TEXCOORD0;
};
v2f vert(appdata_t v)
{
	v2f o;
	UNITY_INITIALIZE_OUTPUT(v2f, o);
  o.pos = (UnityObjectToClipPos(v.vertex));
  o.xlv_TEXCOORD0 = v.texcoord.xy;
	return o;
}
float4 frag(v2f i) : SV_Target
{
	float4 _fragOut = 0;
  float4 c_1;
  float4 tmpvar_2;
  tmpvar_2 = tex2D(_MainTex, i.xlv_TEXCOORD0);
  float4 tmpvar_3;
  tmpvar_3 = (((tmpvar_2 * _Color) * _TintColor) * _Brightness);
  c_1 = tmpvar_3;
  _fragOut = c_1;
	return _fragOut;
}
ENDCG

 }
}
}