// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Rebuilt for Unity 6000.3.8f1 from the shipped Unity 4.0.1f2 shader (sharedassets6.assets__40, rule: unique ShaderLab name among 109 shipped Shader objects).
// ShaderLab state copied verbatim; compiled GLES programs translated 1:1 to Cg by gles2cg.py.
Shader "Mobile/Monster Slayer/Tint Monster Vertex Lit" {
Properties {
 _Color ("Main Color", Color) = (1,1,1,1)
 _TintColor ("Tint Color", Color) = (1,1,1,1)
 _FlashColor ("Flash Color", Color) = (0,0,0,1)
 _AmbientColor ("Ambient Color", Color) = (0.2,0.2,0.2,1)
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
float4 _AmbientColor;
float4 _Color;
float4 _FlashColor;
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
  _fragOut = (((((tex2D(_MainTex, i.xlv_TEXCOORD0) * _Color) * _TintColor) * _AmbientColor) * 4.00000) + _FlashColor);
	return _fragOut;
}
ENDCG

 }
}
}