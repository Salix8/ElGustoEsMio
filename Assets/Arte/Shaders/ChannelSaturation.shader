Shader "Custom/ChannelSaturation" {
    Properties{
        _MainTex("Base", 2D) = "white" {}
        _rSat("Red Saturation", Range(0, 1)) = 1
        _gSat("Green Saturation", Range(0, 1)) = 1
        _bSat("Blue Saturation", Range(0, 1)) = 1
    }
        SubShader{
            Pass {
                CGPROGRAM
                #pragma vertex vert_img
                #pragma fragment frag
                #include "UnityCG.cginc"

                uniform sampler2D _MainTex;
                float _rSat;
                float _gSat;
                float _bSat;

                /*
                  source: modified version of https://www.shadertoy.com/view/MsKGRW
                  written @ https://gist.github.com/hiroakioishi/
                            c4eda57c29ae7b2912c4809087d5ffd0
                */
                float3 rgb2hsl(float3 c) {
                    float epsilon = 0.00000001;
                    float cmin = min( c.r, min( c.g, c.b ) );
                    float cmax = max( c.r, max( c.g, c.b ) );
                    float cd   = cmax - cmin;
                    float3 hsl = float3(0.0, 0.0, 0.0);
                    hsl.z = (cmax + cmin) / 2.0;
                    hsl.y = lerp(cd / (cmax + cmin + epsilon), 
                            cd / (epsilon + 2.0 - (cmax + cmin)), 
                            step(0.5, hsl.z));

                    float3 a = float3(1.0 - step(epsilon, abs(cmax - c)));
                    a = lerp(float3(a.x, 0.0, a.z), a, step(0.5, 2.0 - a.x - a.y));
                    a = lerp(float3(a.x, a.y, 0.0), a, step(0.5, 2.0 - a.x - a.z));
                    a = lerp(float3(a.x, a.y, 0.0), a, step(0.5, 2.0 - a.y - a.z));
    
                    hsl.x = dot( float3(0.0, 2.0, 4.0) + ((c.gbr - c.brg) 
                            / (epsilon + cd)), a );
                    hsl.x = (hsl.x + (1.0 - step(0.0, hsl.x) ) * 6.0 ) / 6.0;
                    return hsl;
                }

                /*
                  source: modified version of
                          https://stackoverflow.com/a/42261473/1092820
                */
                float3 hsl2rgb(float3 c) {
                    float3 rgb = clamp(abs(fmod(c.x * 6.0 + float3(0.0, 4.0, 2.0),
                            6.0) - 3.0) - 1.0, 0.0, 1.0);
                    return c.z + c.y * (rgb - 0.5) * (1.0 - abs(2.0 * c.z - 1.0));
                }

                float4 frag(v2f_img i) : COLOR {
                    float3 sat = float3(_rSat, _gSat, _bSat);

                    float4 c = tex2D(_MainTex, i.uv);
                    float3 hslOrig = rgb2hsl(c.rgb);

                    float3 rgbFullSat = hsl2rgb(float3(hslOrig.x, 1, .5));
                    float3 diminishedrgb = rgbFullSat * sat;

                    float diminishedHue = rgb2hsl(diminishedrgb).x;

                    float diminishedSat = hslOrig.y * length(diminishedrgb);
                    float3 mix = float3(diminishedHue, diminishedSat, hslOrig.z);
                    float3 newc = hsl2rgb(mix);

                    float4 result = c;
                    result.rgb = newc;

                    return result;
                }
                ENDCG
            }
        }
}