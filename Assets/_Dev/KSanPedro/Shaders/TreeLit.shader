shader "ZinklofDev/TreeLit" 
{
    Properties
    {
        _Color("Color", Color) = (.8, .8, .8, 1)
    }
	SubShader 
    {
	    Tags {"RenderPipeline" = "UniversalPipeline"}
		
	    pass 
        {
		    HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float4 _Color;

            StructuredBuffer<float4> position_buffer;

		    struct attributes
            {
                float3 normal : NORMAL;
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct varyings
            {
                float4 vertex : SV_POSITION;
                float3 diffuse : TEXCOORD2;
                float3 color : TEXCOORD3;
            };

            varyings vert(attributes v, const uint instance_id : SV_InstanceID)
            {
                float4 pos = position_buffer[instance_id];

                varyings o;
                o.vertex = TransformObjectToHClip(v.positionOS.xyz) + pos;
                o.diffuse = saturate(dot(v.normal, _MainLightPosition.xyz));
                o.color = _Color;
                
                return o;
            }

            half4 frag(const varyings i) : SV_Target
            {
                return half4(i.color, 1);;
            }

		    ENDHLSL
		}
	}
}