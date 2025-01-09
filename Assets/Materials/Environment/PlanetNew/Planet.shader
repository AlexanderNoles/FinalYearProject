Shader "Unlit/Planet"
{
    Properties
    {
        _LandMaskModifier ("Land Mask Modifier", float) = 0
        _PlanetRelativeRadius ("Relative Radius", float) = .5
        _AtmosphereRadius ("Atmosphere Radius", float) = .4
        _LandFloor("Land Floor", float) = 0
        _LandDisplacement ("Land Displacement", float) = 2
        _LandColor ("Land Colour", Color) = (0, 1, 0)
        _OceanColor ("Ocean Colour", Color) = (0, 1, 0)
        _RealSpacePosition ("RPS", Vector) = (0, 0, 0, 0)
        _SunLightSmoothness ("Sun Light Smoothness", float) = 1.0
        _SunLightShift ("Sun Light Shift", float) = 0
        _OceanSmoothness("Ocean Smoothness", float) = 0
        _SpecularIntensity("Ocean Specular Intensity", float) = 0
        _OceanFlat("Ocean Flat", float) = 1
    }
    SubShader
    {
        Tags {"RenderType"="Opaque"}
        Cull Off
        ZWrite On
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            //Define our constants
            #define MAX_STEPS 100
            #define SURF_DIST 1e-3
            //


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;

                float3 rayOrigin : TEXCOORD1;
                float3 hitPos : TEXCOORD2;
                float4 screenSpace : TEXCOORD3;
            };

            sampler2D _CameraDepthTexture;
            uniform float _InMap; 
            float _LandMaskModifier;
            float _OceanSmoothness;
            float _SpecularIntensity;
            float _PlanetRelativeRadius;
            float _AtmosphereRadius;
            float _SunLightSmoothness;
            float _SunLightShift;
            float _LandDisplacement;
            float _LandFloor;
            float _OceanFlat;
            float3 _LandColor;
            float3 _OceanColor;
            float3 _RealSpacePosition;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenSpace = ComputeScreenPos(o.vertex);
                //Get camera position and convert it to obejct space
                //This means the raymarched object will match the gameobject's position
                //Need to convert camera space to 4d coordinates or matrix multiplication will not apply correctly!
                o.rayOrigin = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0f));
                o.hitPos = v.vertex;
                return o;
            }
            
            float OtaviogoodNoise(float3 position, float iterations, float scale)
            {
                float nudge = 0.9;
	            float normalizer = 1.0 / sqrt(1.0 + (nudge * nudge));
	
	            float n = 1;
	            float iter = 2.0;

                position *= scale;
                position += _RealSpacePosition;
	
	            for (int i = 0; i < iterations; i++)
	            {
		            // add sin and cos scaled inverse with the frequency
		            n += -abs(sin(position.y * iter) + cos(position.x * iter)) / iter; // abs for a ridged look
                    // rotate by adding perpendicular and scaling down
		            position.xy += float2(position.y, -position.x) * nudge;
		            position.xy *= normalizer;
                    // rotate on other axis
		            position.xz += float2(position.z, -position.x) * nudge;
		            position.xz *= normalizer;
                    // increase the frequency
		            iter *= 1.733733;
	            }
	
	            return n;
            }

            float SurfaceNoise(float3 pos)
            {
                int iterationCount = 1;
                float n = 0;

                for (int i = 0; i < iterationCount; i++)
                {
                    int iPlusOne = i+1;
                    n += OtaviogoodNoise(pos, 8, 0.25 + (iPlusOne * 2)) * ((iterationCount - i) / iterationCount);
                }

                return n;
            }

            float SurfaceNoiseApplied(float3 p)
            {
                float stackedNoise = (SurfaceNoise(p * (0.5 / _PlanetRelativeRadius)) * 0.01) - (_LandFloor * 0.01);

                if (stackedNoise < 0.0)
                {
                    return stackedNoise * _LandDisplacement;
                }

                return stackedNoise;
            }

            float GetDistance(float3 p, float radius)
            {
                float distance = length(p) - radius;

                return distance;
            }

            float GetDistance(float3 p)
            {
                //Distance from a sphere at the origin of radius
                float distance = length(p) - _PlanetRelativeRadius;

                return distance;
            }

            //How far away are we from the planet's surface?
            float SurfaceDistance(float3 p) 
            {
                return GetDistance(p) + SurfaceNoiseApplied(p);
            }

            //Calculate depth along viewing ray
            float SurfaceRaymarch(float3 rayOrigin, float3 rayDirection, float maxDistance) 
            {
                float distanceFromOrigin = 0;
                float distanceFromSurface = 0;

                //Calculate distance from surface at each point
                for (int i = 0; i < MAX_STEPS; i++)
                {
                    float3 currentRayPoint = rayOrigin + (distanceFromOrigin * rayDirection);
                    distanceFromSurface = SurfaceDistance(currentRayPoint);

                    distanceFromOrigin += distanceFromSurface;

                    //if we have hit a surface or we have gone beyond draw distance
                    if (distanceFromSurface <= SURF_DIST || distanceFromOrigin > maxDistance)
                    {
                        break;
                    }
                }

                return distanceFromOrigin;
            }

            float FlatRaymarch(float3 rayOrigin, float3 rayDirection, float maxDistance, float radius)
            {
                float distanceFromOrigin = 0;
                float distanceFromSurface = 0;

                //Calculate distance from surface at each point
                for (int i = 0; i < MAX_STEPS; i++)
                {
                    float3 currentRayPoint = rayOrigin + (distanceFromOrigin * rayDirection);
                    distanceFromSurface = GetDistance(currentRayPoint, radius);

                    distanceFromOrigin += distanceFromSurface;

                    //if we have hit a surface or we have gone beyond draw distance
                    if (distanceFromSurface <= SURF_DIST || distanceFromOrigin > maxDistance)
                    {
                        break;
                    }
                }

                return distanceFromOrigin;
            }

            float3 GetNormal(float3 p) 
            {
                //epsilon - just a very small number
                float2 e = float2(1e-2, 0);

                float3 normal = SurfaceDistance(p) - float3(
                    SurfaceDistance(p-e.xyy),
                    SurfaceDistance(p-e.yxy),
                    SurfaceDistance(p-e.yyx)
                    );

                return normalize(normal);
            }

            float3 CalculateLightDirection()
            {
                return _InMap * _RealSpacePosition;
            }

            float SunLightIntensity()
            {
                return (3.0 / length(CalculateLightDirection()));
            }

            ////// MAIN COLOUR FUNCTION //////
            float3 CalculateBasicLighting(float3 normal)
            {
                float3 sunDir = CalculateLightDirection(); 

                float3 basicLighting = saturate((dot(normal, normalize(sunDir)) / _SunLightSmoothness) - (_SunLightSmoothness - _SunLightShift));
                basicLighting = clamp(basicLighting * SunLightIntensity(), 0.01, 100);

                return basicLighting;
            }

            float CalculateSpecularHighlights(float3 viewDir, float3 normal, float power)
            {
                float3 lightDir = CalculateLightDirection();

                return pow(clamp(dot(-normalize(lightDir), reflect(viewDir, normal)), 0, 1), power) * (_SpecularIntensity * SunLightIntensity());
            }

            float3 CalculatePlanetColour(float3 pos, float3 normal, float3 viewDir)
            {
                float terrainHeight = SurfaceNoiseApplied(pos) - (_LandMaskModifier / 100);
                float landMask = terrainHeight < 0;

                float usefulTerrainHeight = 70 * abs(terrainHeight);
                usefulTerrainHeight = saturate(pow(usefulTerrainHeight, 0.4));

                float3 landColour = _LandColor;
                landColour *= clamp(usefulTerrainHeight, 0.2, 1);

                float3 oceanColour = _OceanColor;
                float3 deepOceanColour = (oceanColour * 0.5);

                const float shoreLimit = 0.4;

                float3 shoreline = lerp(landColour, deepOceanColour, abs(usefulTerrainHeight) * (1 / shoreLimit));

                oceanColour = lerp(shoreline, deepOceanColour, saturate((usefulTerrainHeight > shoreLimit) * 1000));

                float3 oceanNormal = lerp(normal, normalize(pos), _OceanFlat);

                float3 finalColour = lerp(
                    oceanColour * CalculateBasicLighting(oceanNormal) + CalculateSpecularHighlights(viewDir, oceanNormal, _OceanSmoothness), 
                    landColour * CalculateBasicLighting(normal), 
                    landMask);

                return finalColour;
            }

            ///////////////////////////////////

            fixed4 frag (v2f i, out float depth : SV_Depth) : SV_Target
            {
                float4 cameraParams = _ProjectionParams;

                //Ray origin
                float3 rayOrigin = i.rayOrigin;

                //Ray direction
                //Direction is just normalized displacement from point we "hit" the object
                float3 fullRay = i.hitPos - rayOrigin;
                float3 rayDirection = normalize(fullRay);
                
                float maxDistance = 1000;

                //Raymarch baby!
                float distanceToScene = SurfaceRaymarch(rayOrigin, rayDirection, maxDistance);

                //Calculated object space point of final ray pos
                float3 p = rayOrigin + rayDirection * distanceToScene;
                float4 worldSpacePos = mul(unity_ObjectToWorld, float4(p, 1));
                float4 clipPos = UnityWorldToClipPos(worldSpacePos);
                //Get the depth of this position for filtering
                float pointDepth = clipPos.z / clipPos.w;

                //Set output depth
                depth = pointDepth;

                //Get the current depth
                // float2 screenSpaceUV = i.screenSpace.xy / i.screenSpace.w;
                // float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenSpaceUV);

                fixed4 col = 0;
                if (distanceToScene < maxDistance)
                {
                    float3 calculatedColor = float3(1, 0, 0);

                    //Normal
                    float3 normal = GetNormal(p);

                    calculatedColor = CalculatePlanetColour(p, normal, -rayDirection);

                    //Apply colour
                    col = float4(calculatedColor, 1);
                }
                else
                {
                    discard;
                }

                return col;
            }
            ENDCG
        }
    }
}
