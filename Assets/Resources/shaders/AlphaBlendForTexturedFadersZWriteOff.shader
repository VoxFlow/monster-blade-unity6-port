Shader "Mobile/Monster Slayer/Alpha Blend For Textured Faders Disable Z Write" {
Properties {
 _Color ("Main Color", Color) = (1,1,1,1)
 _AmbientColor ("Ambient Color", Color) = (0.2,0.2,0.2,1)
 _MainTex ("Base (RGB) Trans (A)", 2D) = "grey" {}
}
SubShader { 
 Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
 Pass {
  Tags { "LIGHTMODE"="Always" "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
  ZWrite Off
  Fog { Mode Off }
  Blend SrcAlpha OneMinusSrcAlpha
  SetTexture [_MainTex] { ConstantColor [_Color] combine texture * constant, texture alpha * constant alpha }
  SetTexture [_MainTex] { ConstantColor [_AmbientColor] combine previous * constant quad }
 }
}
}