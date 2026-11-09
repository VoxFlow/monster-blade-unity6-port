Shader "Mobile/Monster Slayer/MonsterVertexLit" {
Properties {
 _Color ("Main Color", Color) = (1,1,1,1)
 _FlashColor ("Flash Color", Color) = (0,0,0,1)
 _AmbientColor ("Ambient Color", Color) = (0.2,0.2,0.2,1)
 _MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader { 
 LOD 80
 Tags { "RenderType"="Opaque" }
 Pass {
  Tags { "LIGHTMODE"="Always" "RenderType"="Opaque" }
  SetTexture [_MainTex] { ConstantColor [_Color] combine texture * constant, texture alpha * constant alpha }
  SetTexture [_MainTex] { ConstantColor [_AmbientColor] combine previous * constant quad }
  SetTexture [_MainTex] { ConstantColor [_FlashColor] combine previous + constant }
 }
}
Fallback "VertexLit"
}