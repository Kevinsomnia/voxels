Shader "FX/Voxel Block Visualization" {
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR] _Color ("Color", Color) = (0.5, 0.5, 0.5, 1)
        _IntersectionAmount ("Intersection Amount", Range(0.0, 1.0)) = 0.1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            fixed _GridScale;

            // Intersection variables
            sampler2D _CameraDepthTexture;
            fixed _IntersectionAmount;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 vertex : POSITION;
                float3 worldPos : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                half fresnel : TEXCOORD2;
                float3 triplanarWeights : TEXCOORD3;
            };
            
            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                COMPUTE_EYEDEPTH(o.screenPos.z);

                half3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
                o.fresnel = (1.0 - abs(dot(v.normal, viewDir)));
                // Don't rotate the cube and this will work.
                o.triplanarWeights = abs(v.normal);

                return o;
            }

            fixed4 frag(v2f i) : COLOR
            {
                fixed4 finalCol = _Color;
                finalCol.rgb *= 0.5 + (i.fresnel * 0.5);

                fixed xCol = tex2D(_MainTex, (i.worldPos.zy / _GridScale) - float2(0.5, 0.5)).r * i.triplanarWeights.x;
                fixed yCol = tex2D(_MainTex, (i.worldPos.xz / _GridScale) - float2(0.5, 0.5)).r * i.triplanarWeights.y;
                fixed zCol = tex2D(_MainTex, (i.worldPos.xy / _GridScale) - float2(0.5, 0.5)).r * i.triplanarWeights.z;

                finalCol.a *= xCol + yCol + zCol;

                fixed intersectFactor = 1.0 - saturate((LinearEyeDepth(tex2Dproj(_CameraDepthTexture, i.screenPos).r) - i.screenPos.z) / _IntersectionAmount);
                intersectFactor += 1.0;
                finalCol.a *= intersectFactor;

                return finalCol;
            }
            ENDCG
        }
    }

    Fallback Off
}
