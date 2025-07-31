Shader "Custom/DynamicMaskEffect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Main Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _MaskPosition ("Mask Position", Vector) = (0.5, 0.5, 0, 0)
        _MaskSize ("Mask Size", Vector) = (0.3, 0.3, 0, 0)
        _ColorThreshold ("Color Threshold", Range(0, 1)) = 0.5
        _EdgeSoftness ("Edge Softness", Range(0, 0.5)) = 0.1
        _InvertMask ("Invert Mask", Float) = 0
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float2 maskUV   : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            sampler2D _MaskTex;
            float4 _MaskPosition;
            float4 _MaskSize;
            float _ColorThreshold;
            float _EdgeSoftness;
            float _InvertMask;
            
            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.color = IN.color;
                OUT.texcoord = IN.texcoord;
                
                // 计算遮罩UV
                float2 maskUV = (IN.texcoord - _MaskPosition.xy) * _MaskSize.zw + 0.5;
                maskUV.y = 1 - maskUV.y; // Flip Y for UI
                OUT.maskUV = maskUV;
                
                return OUT;
            }
            
            fixed4 frag(v2f IN) : SV_Target
            {
                // 采样主纹理
                fixed4 mainColor = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // 采样遮罩纹理
                fixed4 maskColor = tex2D(_MaskTex, IN.maskUV);
                
                // 计算遮罩alpha
                float maskAlpha = maskColor.a;
                
                // 反转遮罩
                if (_InvertMask > 0.5)
                    maskAlpha = 1 - maskAlpha;
                
                // 计算边缘柔化
                float edgeSoftness = clamp(_EdgeSoftness, 0.001, 0.5);
                float softFactor = smoothstep(_ColorThreshold - edgeSoftness, 
                                             _ColorThreshold + edgeSoftness, 
                                             maskAlpha);
                
                // 应用遮罩
                mainColor.a *= softFactor;
                
                return mainColor;
            }
            ENDCG
        }
    }
}