Shader "Overdraw" {
Properties {
 _MainTex ("Base", 2D) = "white" {}
 _Color ("Main Color", Color) = (0.15,0,0,0)
}
SubShader { 
 Pass {
  ZTest False
  ZWrite Off
  Fog { Mode Off }
  Blend One One
  SetTexture [_MainTex] { ConstantColor [_Color] combine constant, texture alpha }
 }
}
}