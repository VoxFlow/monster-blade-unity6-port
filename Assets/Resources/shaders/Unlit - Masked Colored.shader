// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Rebuilt for Unity 6000.3.8f1 from the shipped Unity 4.0.1f2 shader (3d45664f9b1374344b44c99d23248161__1, rule: unique ShaderLab name among 109 shipped Shader objects).
// ShaderLab state copied verbatim; compiled GLES programs translated 1:1 to Cg by gles2cg.py.
Shader "Unlit/Masked Colored" {
Properties {
 _MainTex ("Base (RGB) Mask (A)", 2D) = "white" {}
 _Color ("Tint Color", Color) = (1,1,1,1)
}
SubShader { 
 LOD 200
 Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
 Pass {
  Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
  ZWrite Off
  Cull Off
  Fog { Mode Off }
  ColorMask RGB
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
float4 _Color;
sampler2D _MainTex;
float4 _MainTex_ST;
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
  float4 tmpvar_1;
  float4 col_2;
  float4 tmpvar_3;
  tmpvar_3 = (tex2D(_MainTex, i.xlv_TEXCOORD0) * i.xlv_COLOR);
  col_2 = tmpvar_3;
  float4 tmpvar_4;
  tmpvar_4.xyz = lerp(col_2.xyz, (col_2.xyz * _Color.xyz), col_2.www);
  tmpvar_4.w = col_2.w;
  tmpvar_1 = tmpvar_4;
  _fragOut = tmpvar_1;
	return _fragOut;
}
ENDCG

 }
}
SubShader { 
 LOD 100
 Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
 Pass {
  Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
  ZWrite Off
  Cull Off
  Fog { Mode Off }
  AlphaTest Greater 0.01
  ColorMask RGB
  ColorMaterial AmbientAndDiffuse
  SetTexture [_MainTex] { combine texture * primary }
  SetTexture [_MainTex] { ConstantColor [_Color] combine previous * constant }
 }
}
}