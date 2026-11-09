// Monster Blade 1.3.3 shader restored from the shipped data (KB topic 20), MANUAL 1:1 translation.
// Source: sharedassets6.assets path id 39 (Docs/Original_Shaders/data/objects/sharedassets6.assets__39.shader.txt), Unity 4.0.1f2.
// The original is hand-written GLSL (GLSLPROGRAM style, not Unity-compiled output), so Tools/gles2cg.py does not parse it;
// ShaderLab text kept verbatim, only the GLSL program replaced. Original program:
//   vertex:   gl_Position = MVP * vertex; uv = texcoord0.xy;
//   fragment: c = mix(tex(_MainTex, uv), tex(_Texture2, uv), _Blend);
//             gl_FragColor = vec4(vec3(dot(c.rgb, vec3(.222, .707, .071)) * _Color.r), 1);
Shader "Mobile/Monster Slayer/Greyscale Texture" {
Properties {
 _Color ("Main Color", Color) = (1,1,1,1)
 _FlashColor ("Flash Color", Color) = (0,0,0,1)
 _AmbientColor ("Ambient Color", Color) = (0.2,0.2,0.2,1)
 _MainTex ("Base (RGB)", 2D) = "white" {}
 _Texture2 ("Blend Texture", 2D) = "white" {}
 _Blend ("Blend between base and blend texture", Range(0,1)) = 0.5
}
SubShader {
 Pass {
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
sampler2D _MainTex;
sampler2D _Texture2;
float4 _Color;
float _Blend;
struct appdata { float4 vertex : POSITION; float4 texcoord : TEXCOORD0; };
struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
v2f vert(appdata v)
{
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
	o.uv = v.texcoord.xy;
	return o;
}
float4 frag(v2f i) : SV_Target
{
	float4 t1 = tex2D(_MainTex, i.uv);
	float4 t2 = tex2D(_Texture2, i.uv);
	float4 c = lerp(t1, t2, _Blend);
	return float4((dot(c.rgb, float3(0.222, 0.707, 0.071)) * _Color.r).xxx, 1);
}
ENDCG
 }
}
SubShader {
 Pass {
  Color (0.389,0.1465,0.4645,0.5)
  SetTexture [_MainTex] { combine one-texture * primary alpha }
  SetTexture [_] { combine previous dot3 primary }
 }
}
}
