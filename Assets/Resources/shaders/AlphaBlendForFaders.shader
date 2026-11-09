Shader "Mobile/Monster Slayer/Alpha Blend For Faders" {
Properties {
 _Color ("Main Color", Color) = (1,1,1,1)
}
SubShader { 
 Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
 Pass {
  Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
  Material {
   Ambient [_Color]
   Diffuse [_Color]
  }
  ZWrite Off
  Fog { Mode Off }
  Blend SrcAlpha OneMinusSrcAlpha
  ColorMaterial AmbientAndDiffuse
  SetTexture [_MainTex] { ConstantColor [_Color] combine previous * constant double, previous alpha * constant alpha }
 }
}
}