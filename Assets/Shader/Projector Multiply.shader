// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'

// Monster Blade 1.3.3 shader restored from the shipped data (KB topic 20), MANUAL 1:1 translation.
// Source: sharedassets1.assets path id 28 (Docs/Original_Shaders/data/objects/sharedassets1.assets__28.shader.txt), Unity 4.0.1f2.
// The original is hand-written GLSL (GLSLPROGRAM style), so Tools/gles2cg.py does not parse it; ShaderLab text kept
// verbatim, only the GLSL program replaced. Original program:
//   vertex:   gl_Position = MVP * vertex; p = _Projector * vertex; uv = p.xy / max(p.w, 0.001);
//   fragment: a = tex(_MainTex, uv).a; gl_FragColor.rgb = _Color.rgb * (1 - a) + a;  (alpha not written)
Shader "Projector/Multiply (alpha cookie)" {
Properties {
 _Color ("Color", Color) = (0,0,0,1)
 _MainTex ("Cookie (Alpha)", 2D) = "" { TexGen ObjectLinear }
}
SubShader {
 Tags { "QUEUE"="Transparent-1" }
 Pass {
  Tags { "QUEUE"="Transparent-1" }
  ZWrite Off
  Fog {
   Color (1,1,1,1)
  }
  Blend Zero SrcColor
  Offset -1, -1
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
sampler2D _MainTex;
float4 _Color;
float4x4 unity_Projector;
struct appdata { float4 vertex : POSITION; };
struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
v2f vert(appdata v)
{
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
	float4 p = mul(unity_Projector, v.vertex);
	o.uv = p.xy / max(p.w, 0.001);
	return o;
}
float4 frag(v2f i) : SV_Target
{
	float a = tex2D(_MainTex, i.uv).a;
	// gl_FragColor.a was never written in the original (undefined); with Blend Zero SrcColor it does not reach the result
	return float4(_Color.rgb * (1.0 - a) + a, 1.0);
}
ENDCG
 }
}
SubShader {
 Tags { "QUEUE"="Transparent-1" }
 Pass {
  Tags { "QUEUE"="Transparent-1" }
  ZWrite Off
  Fog {
   Color (1,1,1,1)
  }
  Blend Zero SrcColor
  Offset -1, -1
  SetTexture [_MainTex] { Matrix [_Projector] ConstantColor [_Color] combine constant alpha lerp(texture) constant }
 }
}
}
