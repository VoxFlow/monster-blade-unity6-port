// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Rebuilt for Unity 6000.3.8f1 from the shipped Unity 4.0.1f2 shader (sharedassets2.assets__6, rule: unique ShaderLab name among 109 shipped Shader objects).
// ShaderLab state copied verbatim; compiled GLES programs translated 1:1 to Cg by gles2cg.py.
Shader "Color Space/YCrCbtoRGB" {
Properties {
 _YTex ("Y (RGB)", 2D) = "white" {}
 _CrTex ("Cr (RGB)", 2D) = "white" {}
 _CbTex ("Cb (RGB)", 2D) = "white" {}
}
SubShader { 
 Tags { "RenderType"="Opaque" }
 Pass {
  Tags { "RenderType"="Opaque" }
  Fog {
   Color (0,0,0,0)
  }
  ColorMask RGB
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
sampler2D _CbTex;
float4 _CbTex_ST;
sampler2D _CrTex;
sampler2D _YTex;
float4 _YTex_ST;
struct appdata_t {
	float4 texcoord : TEXCOORD0;
	float4 vertex : POSITION;
};
struct v2f {
	float4 pos : SV_POSITION;
	float2 xlv_TEXCOORD1 : TEXCOORD0;
	float2 xlv_TEXCOORD0 : TEXCOORD1;
};
v2f vert(appdata_t v)
{
	v2f o;
	UNITY_INITIALIZE_OUTPUT(v2f, o);
							float2 tmpvar_1;
							float2 tmpvar_2;
							float2 tmpvar_3;
							tmpvar_3 = ((v.texcoord.xy * _YTex_ST.xy) + _YTex_ST.zw);
							tmpvar_1 = tmpvar_3;
							float2 tmpvar_4;
							tmpvar_4 = ((v.texcoord.xy * _CbTex_ST.xy) + _CbTex_ST.zw);
							tmpvar_2 = tmpvar_4;
							o.pos = (UnityObjectToClipPos(v.vertex));
							o.xlv_TEXCOORD0 = tmpvar_1;
							o.xlv_TEXCOORD1 = tmpvar_2;
	return o;
}
float4 frag(v2f i) : SV_Target
{
	float4 _fragOut = 0;
							float4 rgbVec;
							float4 tmpvar_1;
							tmpvar_1.w = 1.0;
							tmpvar_1.x = tex2D(_YTex, i.xlv_TEXCOORD0).x;
							tmpvar_1.y = tex2D(_CrTex, i.xlv_TEXCOORD1).y;
							tmpvar_1.z = tex2D(_CbTex, i.xlv_TEXCOORD1).z;
							rgbVec.x = dot (float4(1.16438, 0.0, 1.59603, -0.870785), tmpvar_1);
							rgbVec.y = dot (float4(1.16438, -0.391762, -0.812969, 0.529594), tmpvar_1);
							rgbVec.z = dot (float4(1.16438, 2.01723, 0.0, -1.08139), tmpvar_1);
							rgbVec.w = 1.0;
							_fragOut = rgbVec;
	return _fragOut;
}
ENDCG

 }
}
}