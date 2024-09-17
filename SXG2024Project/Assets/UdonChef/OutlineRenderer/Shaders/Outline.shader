Shader "SXG/Outline"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth("Outline Width", Range(0, 10)) = 1
        _Cutoff("Cutoff Level", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags 
        {
            "RenderType" = "Overlay"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZTest Always
        
        Pass
        {
            Name "Outline"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment frag

            TEXTURE2D_X(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            CBUFFER_START(UnityPerMaterial)
            half2 _CameraOpaqueTexture_TexelSize;
            half4 _OutlineColor;
            half _OutlineWidth;
            half _Cutoff;            
            CBUFFER_END

            half4 frag(Varyings input) : SV_Target
            {
                half2 uv = input.texcoord;
                half2 destUV = _CameraOpaqueTexture_TexelSize * _OutlineWidth;
                half4 col = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv);

                half sum = 0;

                [unroll(3)]
                for (int i = -1; i <= 1; i++)
                {
                    [unroll(3)]
                    for (int j = -1; j <= 1; j++)
                    {
                        sum += SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + half2(destUV.x * i, destUV.y * j)).a;
                    }
                }

                sum = saturate(sum); // Clamp01
                clip(_Cutoff - col.a); // Alpha‚ª_CutoffˆÈ‰º‚È‚ç•`‰æ‚µ‚È‚¢

                half4 outline = sum * _OutlineColor;

                return outline;
            }
            ENDHLSL
        }
    }
}