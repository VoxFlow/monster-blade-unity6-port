// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Rebuilt for Unity 6000.3.8f1 from the shipped Unity 4.0.1f2 shader (f192d0f091d484e8cbcf9f392d289aab__1, rule: unique ShaderLab name among 109 shipped Shader objects).
// ShaderLab state copied verbatim; compiled GLES programs translated 1:1 to Cg by gles2cg.py.
Shader "Unlit/Transparent Colored (SoftClip)" {
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
float2 _ClipSharpness;
sampler2D _MainTex;
float4 _MainTex_ST;
struct appdata_t {
	float4 texcoord : TEXCOORD0;
	float4 color : COLOR;
	float4 vertex : POSITION;
};
struct v2f {
	float4 pos : SV_POSITION;
	float2 xlv_TEXCOORD1 : TEXCOORD0;
	float2 xlv_TEXCOORD0 : TEXCOORD1;
	float4 xlv_COLOR : TEXCOORD2;
};
v2f vert(appdata_t v)
{
	v2f o;
	UNITY_INITIALIZE_OUTPUT(v2f, o);
  o.pos = (UnityObjectToClipPos(v.vertex));
  o.xlv_COLOR = v.color;
  o.xlv_TEXCOORD0 = v.texcoord.xy;
  o.xlv_TEXCOORD1 = ((v.vertex.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
	return o;
}
float4 frag(v2f i) : SV_Target
{
	float4 _fragOut = 0;
  float4 col_1;
  float2 tmpvar_2;
  tmpvar_2 = ((float2(1.00000, 1.00000) - abs(i.xlv_TEXCOORD1)) * _ClipSharpness);
  float4 tmpvar_3;
  tmpvar_3 = (tex2D(_MainTex, i.xlv_TEXCOORD0) * i.xlv_COLOR);
  col_1.xyz = tmpvar_3.xyz;
  float tmpvar_4;
  tmpvar_4 = (tmpvar_3.w * clamp (min (tmpvar_2.x, tmpvar_2.y), 0.00000, 1.00000));
  col_1.w = tmpvar_4;
  _fragOut = col_1;
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
  Blend SrcAlpha OneMinusSrcAlpha
  AlphaTest Greater 0.01
  ColorMask RGB
  ColorMaterial AmbientAndDiffuse
  SetTexture [_MainTex] { combine texture * primary }
 }
}
}