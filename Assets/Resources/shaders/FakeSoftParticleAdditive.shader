// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Rebuilt for Unity 6000.3.8f1 from the shipped Unity 4.0.1f2 shader (86c77012d18134eeba37eb289e364778__1, rule: unique ShaderLab name among 109 shipped Shader objects).
// ShaderLab state copied verbatim; compiled GLES programs translated 1:1 to Cg by gles2cg.py.
Shader "Custom/Particle/FakeSoftParticle (Additive)" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "white" {}
 _InvFade ("Soft Particles Factor", Float) = 4
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
  ColorMask RGB
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile SOFTPARTICLES_OFF SOFTPARTICLES_ON
#include "UnityCG.cginc"
#if defined(SOFTPARTICLES_OFF)
float4 _GroundPlane;
float _InvFade;
sampler2D _MainTex;
float4 _MainTex_ST;
struct appdata_t {
	float4 texcoord : TEXCOORD0;
	float4 color : COLOR;
	float4 vertex : POSITION;
};
struct v2f {
	float4 pos : SV_POSITION;
	float xlv_TEXCOORD1 : TEXCOORD0;
	float2 xlv_TEXCOORD0 : TEXCOORD1;
	float4 xlv_COLOR : TEXCOORD2;
};
v2f vert(appdata_t v)
{
	v2f o;
	UNITY_INITIALIZE_OUTPUT(v2f, o);
  float tmpvar_1;
  float4 tmpvar_2;
  tmpvar_2 = (UnityObjectToClipPos(v.vertex));
  float tmpvar_3;
  tmpvar_3 = (_InvFade * dot (_GroundPlane, tmpvar_2));
  tmpvar_1 = tmpvar_3;
  o.pos = tmpvar_2;
  o.xlv_COLOR = v.color;
  o.xlv_TEXCOORD0 = ((v.texcoord.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  o.xlv_TEXCOORD1 = tmpvar_1;
	return o;
}
float4 frag(v2f i) : SV_Target
{
	float4 _fragOut = 0;
  float4 col_1;
  float4 tmpvar_2;
  tmpvar_2 = (i.xlv_COLOR * tex2D(_MainTex, i.xlv_TEXCOORD0));
  col_1.xyz = tmpvar_2.xyz;
  col_1.w = (tmpvar_2.w * clamp (i.xlv_TEXCOORD1, 0.00000, 1.00000));
  _fragOut = col_1;
	return _fragOut;
}
#elif defined(SOFTPARTICLES_ON)
float4 _GroundPlane;
float _InvFade;
sampler2D _MainTex;
float4 _MainTex_ST;
struct appdata_t {
	float4 texcoord : TEXCOORD0;
	float4 color : COLOR;
	float4 vertex : POSITION;
};
struct v2f {
	float4 pos : SV_POSITION;
	float xlv_TEXCOORD1 : TEXCOORD0;
	float2 xlv_TEXCOORD0 : TEXCOORD1;
	float4 xlv_COLOR : TEXCOORD2;
};
v2f vert(appdata_t v)
{
	v2f o;
	UNITY_INITIALIZE_OUTPUT(v2f, o);
  float tmpvar_1;
  float4 tmpvar_2;
  tmpvar_2 = (UnityObjectToClipPos(v.vertex));
  float tmpvar_3;
  tmpvar_3 = (_InvFade * dot (_GroundPlane, tmpvar_2));
  tmpvar_1 = tmpvar_3;
  o.pos = tmpvar_2;
  o.xlv_COLOR = v.color;
  o.xlv_TEXCOORD0 = ((v.texcoord.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  o.xlv_TEXCOORD1 = tmpvar_1;
	return o;
}
float4 frag(v2f i) : SV_Target
{
	float4 _fragOut = 0;
  float4 col_1;
  float4 tmpvar_2;
  tmpvar_2 = (i.xlv_COLOR * tex2D(_MainTex, i.xlv_TEXCOORD0));
  col_1.xyz = tmpvar_2.xyz;
  col_1.w = (tmpvar_2.w * clamp (i.xlv_TEXCOORD1, 0.00000, 1.00000));
  _fragOut = col_1;
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
  ColorMask RGB
  SetTexture [_MainTex] { combine texture * primary }
 }
}
}