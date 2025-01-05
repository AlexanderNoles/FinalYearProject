Shader "Unlit/Atmosphere"
{
    Properties
    {
        _PlanetRelativeRadius ("Surface Radius", float) = .5
        _AtmospheretRelativeRadius ("Atmosphere Radius", float) = .5
        _RealSpacePosition ("RPS", Vector) = (0, 0, 0, 0)
        _DensityFalloff ("Density Falloff", float) = 1
        _ScatteringCoefficents ("Scattering Coefficents", Vector) = (0.1066, 0.3244416, 0.6830136, 0)
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off 
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            
            //Define our constants
            #define MAX_STEPS 100
            #define SURF_DIST 1e-3
            #define SCATTERING_POINTS 10
            #define OPTICAL_DEPTH_POINTS 5
            #define MAX_DISTANCE 1000
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
            };
            
            uniform float _InMap; 
            float _PlanetRelativeRadius;
            float _AtmospheretRelativeRadius;
            float _DensityFalloff;
            float3 _RealSpacePosition;
            float3 _ScatteringCoefficents;

            float3 CalculateLightDirection()
            {
                return _InMap * _RealSpacePosition;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //Get camera position and convert it to obejct space
                //This means the raymarched object will match the gameobject's position
                //Need to convery camera space to 4d coordinates or matrix multiplication will not apply correctly!
                o.rayOrigin = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0f));
                o.hitPos = v.vertex;

                return o;
            }

            // float GetDistance(float3 p, float radius)
            // {
            //     float distance = length(p) - radius;

            //     return distance;
            // }

            // float2 FlatRaymarch(float3 rayOrigin, float3 rayDirection, float maxDistance, float radius)
            // {
            //     float distanceFromOrigin = 0;
            //     float distanceFromSurface = 0;
            //     float distanceThroughSphere = 0;
            //     float3 currentRayPoint = 0;

            //     //Calculate distance from surface at each point
            //     for (int i = 0; i < MAX_STEPS; i++)
            //     {
            //         currentRayPoint = rayOrigin + (distanceFromOrigin * rayDirection);
            //         distanceFromSurface = GetDistance(currentRayPoint, radius);

            //         distanceFromOrigin += distanceFromSurface;

            //         //if we have hit a surface or we have gone beyond draw distance
            //         if (distanceFromSurface <= SURF_DIST || distanceFromOrigin > maxDistance)
            //         {
            //             //The displacement of the current (final) ray point from the center can be used to calculate how far
            //             //through the sphere this ray would travel
            //             //This displacement acts as the hypotenuse to a triangle, 
            //             //the direction of the adjacent side is equal to the view direction
    
            //             break;
            //         }
            //     }

            //     return (distanceFromOrigin, distanceThroughSphere);
            // }

            //Finds both the distance a ray travels to a sphere and the distance a ray travels through it
            //If no intersection is found the returns maxDistance, 0
            float2 SLRaySphere(float3 sphereCentre, float radius, float3 rayOrigin, float3 rayDirection, float maxDistance)
            {
                float3 offset = rayOrigin - sphereCentre;
                float b = 2 * dot(offset, rayDirection);
                float c = dot(offset, offset) - radius * radius;
                float d = b * b - 4 * c; //Quadratic formula back again :(

                if (d > 0)
                {
                    float s = sqrt(d);
                    float dstToSphereNear = max(0, (-b - s) / 2.0);
                    float dstToSphereFar = (-b + s) / 2.0;

                    if (dstToSphereFar >= 0)
                    {
                        return float2(dstToSphereNear, dstToSphereFar - dstToSphereNear);
                    }
                }

                return float2(maxDistance, 0);
            }

            float DensityAtPoint(float3 p)
            {
                float heightAboveSurface = length(p) - _PlanetRelativeRadius;
                float heigh01 = heightAboveSurface / (_AtmospheretRelativeRadius - _PlanetRelativeRadius);

                //Force density to be zero at atmosphere limit
                float localDensity = exp(-heigh01 * _DensityFalloff) * (1 - heigh01);

                return localDensity;
            }

            float OpticalDepth(float3 rayOrigin, float3 rayDirection, float rayLength)
            {
                float3 currentSamplePoint = rayOrigin;
                float stepSize = rayLength / (OPTICAL_DEPTH_POINTS - 1);
                float opticalDepth = 0;

                for (int i = 0; i < OPTICAL_DEPTH_POINTS; i++)
                {
                    float localDensity = DensityAtPoint(currentSamplePoint);
                    opticalDepth += localDensity * stepSize;
                    currentSamplePoint += rayDirection * stepSize;
                }

                return opticalDepth;
            }

            float3 CalculateColour(float3 rayOrigin, float3 rayDir, float distance)
            {
                float3 inScatterPoint = rayOrigin;
                float stepSize = distance / (SCATTERING_POINTS - 1);
                float3 sunDirection = -normalize(CalculateLightDirection());
                float3 inScatteredLight = 0;

                for (int i = 0; i < SCATTERING_POINTS; i++)
                {
                    float2 sunRayLength = SLRaySphere(0, _AtmospheretRelativeRadius, inScatterPoint, sunDirection, MAX_DISTANCE);
                    float sunRayOpitcalDepth = OpticalDepth(inScatterPoint, sunDirection, sunRayLength.y);
                    float viewRayOpticalDepth = OpticalDepth(inScatterPoint, -rayDir, stepSize * i);
                    float3 transmittance = exp(-(sunRayOpitcalDepth + viewRayOpticalDepth) * _ScatteringCoefficents);
                    float localDensity = DensityAtPoint(inScatterPoint);

                    inScatteredLight += localDensity * transmittance * _ScatteringCoefficents * stepSize;
                    inScatterPoint += rayDir * stepSize;
                }

                return inScatteredLight;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 fullRay = i.hitPos - i.rayOrigin;
                float3 rayDirection = normalize(fullRay);

                float2 atmosphereHitInfo = SLRaySphere(0, _AtmospheretRelativeRadius, i.rayOrigin, rayDirection, MAX_DISTANCE);

                //Just assume distance to surface is consistent across  the planet
                //Planet does not contribute to depth buffer currently, even if it did this is approximation is still fine
                float2 surfaceHitInfo = SLRaySphere(0, _PlanetRelativeRadius, i.rayOrigin, rayDirection, MAX_DISTANCE);
                
                float distanceToAtmosphere = atmosphereHitInfo.x;
                float distanceThroughAtmosphere = min(atmosphereHitInfo.y, surfaceHitInfo.x - distanceToAtmosphere);

                if (distanceThroughAtmosphere > 0)
                {
                    float3 pointInAtmosphere = i.rayOrigin + rayDirection * distanceToAtmosphere;
                    float3 atmosphereColour = CalculateColour(pointInAtmosphere, rayDirection, distanceThroughAtmosphere);
                    return float4(atmosphereColour, length(atmosphereColour));
                }

                return 0;
            }
            ENDCG
        }
    }
}
