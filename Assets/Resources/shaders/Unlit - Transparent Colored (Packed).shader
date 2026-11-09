// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Rebuilt for Unity 6000.3.8f1 from the shipped Unity 4.0.1f2 shader (a1b57c4963af54514b178d2eb08a3a97__1, rule: unique ShaderLab name among 109 shipped Shader objects).
// ShaderLab state copied verbatim; compiled GLES programs translated 1:1 to Cg by gles2cg.py.
Shader "Unlit/Transparent Colored (Packed)" {
Properties {
 _MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
}
SubShader { 
 LOD 200
 Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
 Pass {
  Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
  ZWrite Off
  Cull Off
  Fog { Mode Off }
  Blend SrcAlpha OneMinusSrcAlpha
  ColorMask RGB
  Offset -1, -1
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
sampler2D _MainTex;
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
  o.xlv_TEXCOORD0 = v.texcoord.xy;
	return o;
}
float4 frag(v2f i) : SV_Target
{
	float4 _fragOut = 0;
  float4 col_1;
  float4 mask_2;
  float4 tmpvar_3;
  tmpvar_3 = tex2D(_MainTex, i.xlv_TEXCOORD0);
  mask_2 = tmpvar_3;
  float4 tmpvar_4;
  tmpvar_4 = clamp (ceil((i.xlv_COLOR - 0.500000)), 0.00000, 1.00000);
  float4 tmpvar_5;
  tmpvar_5 = clamp ((((tmpvar_4 * 0.510000) - i.xlv_COLOR) / -0.490000), 0.00000, 1.00000);
  col_1.xyz = tmpvar_5.xyz;
  float4 tmpvar_6;
  tmpvar_6 = (mask_2 * tmpvar_4);
  mask_2 = tmpvar_6;
  col_1.w = (tmpvar_5.w * (((tmpvar_6.x + tmpvar_6.y) + tmpvar_6.z) + tmpvar_6.w));
  _fragOut = col_1;
	return _fragOut;
}
ENDCG

 }
}
Fallback Off
}