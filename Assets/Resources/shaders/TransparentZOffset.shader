// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Rebuilt for Unity 6000.3.8f1 from the shipped Unity 4.0.1f2 shader (sharedassets8.assets__50, rule: unique ShaderLab name among 109 shipped Shader objects).
// ShaderLab state copied verbatim; compiled GLES programs translated 1:1 to Cg by gles2cg.py.
Shader "Mobile/Monster Slayer/Transparent Z Offset" {
Properties {
 _MainTex ("Main Texture", 2D) = "white" {}
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
  ZTest Less
  ZWrite Off
  Fog {
   Color (0,0,0,0)
  }
  Blend SrcAlpha OneMinusSrcAlpha
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
sampler2D _MainTex;
float4 _MainTex_ST;
struct appdata_t {
	float4 texcoord : TEXCOORD0;
	float3 normal : NORMAL;
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
  float4 tmpvar_1;
  float4 tmpvar_2;
  tmpvar_2 = (UnityObjectToClipPos(v.vertex));
  tmpvar_1.xyw = tmpvar_2.xyw;
  float4 tmpvar_3;
  tmpvar_3.w = 0.00000;
  tmpvar_3.xyz = normalize(v.normal);
#if defined(UNITY_REVERSED_Z)
  tmpvar_1.z = (tmpvar_2.z + 0.5 * (((mul(UNITY_MATRIX_IT_MV, tmpvar_3)).z * 0.0500000))); // gles2cg v1.4: GL clip-z delta mapped to reversed Z
#else
  tmpvar_1.z = (tmpvar_2.z - ((mul(UNITY_MATRIX_IT_MV, tmpvar_3)).z * 0.0500000));
#endif
  o.pos = tmpvar_1;
  o.xlv_TEXCOORD0 = ((v.texcoord.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
	return o;
}
float4 frag(v2f i) : SV_Target
{
	float4 _fragOut = 0;
  _fragOut = tex2D(_MainTex, i.xlv_TEXCOORD0);
	return _fragOut;
}
ENDCG

 }
}
}