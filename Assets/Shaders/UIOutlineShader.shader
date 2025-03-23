Shader "Custom/UIOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Float) = 2.0
        _MainTex ("Base (RGB)", 2D) = "white" { }
    }

    SubShader
    {
        Tags { "RenderType"="Overlay" }

        Pass {
            Name "OUTLINE"
            Tags { "LightMode" = "Always" }

            ZWrite On
            ZTest LEqual
            Cull Front

            // Apply the base texture first
            SetTexture[_MainTex] {
                combine primary
            }

            // Apply outline color
            ColorMask RGB
            Color (_OutlineColor)

            // Apply offset for the outline effect
            Offset 10, 10
        }

        Pass {
            Name "OUTLINE"
            Tags { "LightMode" = "Always" }

            ZWrite Off
            ZTest LEqual
            Cull Front

            // Apply the base texture for second pass
            SetTexture[_MainTex] {
                combine primary
            }
        }
    }

    Fallback "UI/Default"
}
