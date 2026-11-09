Shader "iPhone/EnvMap" {
Properties {
 _Reflect ("Reflective Color", Color) = (0.5,0.5,0.5,1)
 _MainTex ("MainMap", 2D) = "" {}
 _EnvMap ("EnvMap", 2D) = "" { TexGen SphereMap }
}
SubShader { 
 Pass {
  Color [_Reflect]
  SetTexture [_EnvMap] { combine primary * texture }
  SetTexture [_MainTex] { combine previous + texture }
 }
}
}