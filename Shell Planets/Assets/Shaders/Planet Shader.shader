Shader "_MyShaders/Planet Shader"
{
    SubShader
    {
        Tags 
        { 
            "LightMode" = "ForwardBase"
        }

        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityPBSLighting.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

                float3 normal: NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;

                float3 normal: TEXCOORD1;
            };

            float4 _Color;

            int _ShellIndex;
            int _GroundShellCount;
            int _SkyShellCount;
            float _Ground_Sky_Delta, _SkyCutoff;
            float _GroundFlow, _SkyFlow;

            float _GroundShellLength;
            float _GroundDensity;
            float _GroundNoiseMin, _GroundNoiseMax;

            float _SkyShellLength;
            float _SkyDensity;

            float2 GetGradient(float2 intPos, float t) 
            {
                float rand = frac(sin(dot(intPos, float2(12.9898, 78.233))) * 43758.5453);
    
                float angle = 6.283185 * rand + 4.0 * t * rand;
                return float2(cos(angle), sin(angle));
            }

            float Pseudo3dNoise(float3 pos) 
            {
            float2 i = floor(pos.xy);
            float2 f = pos.xy - i;
            float2 blend = f * f * (3.0 - 2.0 * f);
            float noiseVal = 
                lerp(
                    lerp(
                        dot(GetGradient(i + float2(0, 0), pos.z), f - float2(0, 0)),
                        dot(GetGradient(i + float2(1, 0), pos.z), f - float2(1, 0)),
                        blend.x),
                    lerp(
                        dot(GetGradient(i + float2(0, 1), pos.z), f - float2(0, 1)),
                        dot(GetGradient(i + float2(1, 1), pos.z), f - float2(1, 1)),
                        blend.x), blend.y);
                return noiseVal / 0.7; // normalize to about [-1..1]
            }

            v2f vert (appdata v)
            {
                v2f o;

                float shellHeight = 0;

                if(_ShellIndex < _GroundShellCount)
                {
                    shellHeight = (float)_ShellIndex / (float)_GroundShellCount;
                    v.vertex.xyz += v.normal.xyz * (_GroundShellLength * shellHeight);
                }
                else
                {
                    shellHeight = (float)(_ShellIndex - _GroundShellCount) / (float)_SkyShellCount;
                    v.vertex.xyz += v.normal.xyz * (_Ground_Sky_Delta + _GroundShellLength + (_SkyShellLength * shellHeight));
                }

                o.uv = v.uv;
                o.normal = normalize(UnityObjectToWorldNormal(v.normal));
                o.vertex = UnityObjectToClipPos(v.vertex);

                return o;
            }

            float3 DoGround(v2f i)
            {
                float3 albedo = _Color;

                float shellIndex = _ShellIndex;
                float shellCount = _GroundShellCount;
                float h = shellIndex / shellCount;

                float2 newUV = i.uv * _GroundDensity;
                float rand = 0.5 + 0.5 * Pseudo3dNoise(float3(newUV, _Time.y /_GroundFlow));
                rand = lerp(_GroundNoiseMin, _GroundNoiseMax, rand);
                if(rand < h)
                {
                    discard;
                }

                return albedo;
            }
            float3 DoSky(v2f i)
            {
                float3 albedo = _Color;

                float shellIndex = (float)(_ShellIndex - _GroundShellCount);
                float shellCount = _SkyShellCount;
                float h = shellIndex / shellCount;

                float2 newUV = i.uv * _SkyDensity;
                float rand = 0.5 + 0.5 * Pseudo3dNoise(float3(newUV, _Time.y / _SkyFlow));
                if(rand - h < 1 - _SkyCutoff)
                {
                    discard;
                }

                return albedo;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 albedo = 0;

                if(_ShellIndex < _GroundShellCount)
                {
                    albedo = DoGround(i);
                }
                else
                {
                    albedo = DoSky(i);
                }
                
                return float4(albedo, 1);
            }
            ENDCG
        }
    }
}
