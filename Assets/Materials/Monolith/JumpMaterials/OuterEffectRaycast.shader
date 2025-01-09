Shader"Unlit/OuterEffectRaycast"
{
    Properties
    {
		_FractalOffset ("Fractal Offset", Vector) = (0, 0, 0)
		_Speed ("Displacement Speed", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
		Cull Front
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            //Define our constants
            #define MAX_STEPS 50
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
	
				float3 rayOrigin : TEXCOORD0;
				float3 hitPos : TEXCOORD1;
            };

			float3 _FractalOffset;
			float _Speed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
	
	            //Get camera position and convert it to obejct space
                //This means the raymarched object will match the gameobject's position
                //Need to convert camera space to 4d coordinates or matrix multiplication will not apply correctly!
				o.rayOrigin = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0f));
				o.hitPos = v.vertex;
	
                return o;
            }

			float2 Rotate(float2 p, float a)
	{
	return float2(cos(a) * p.x + sin(a) * p.y, -sin(a) * p.x + cos(a) * p.y);
}

			float DistanceFractal(float3 p)
			{
	p += _FractalOffset;
	p.z += _Time * _Speed;
				float scale = 1.0;
	
	float4 orb = 1000.0;
	
	for (int i = 0; i < 4; i++)
	{
		//p.xy = Rotate(p.xy, _Time);
		p = -1.0 + 2.0 * frac(0.5 * p + 0.5);
		
		float r2 = dot(p, p);
		
		orb = min(orb, float4(abs(p), r2));
		
		float k = 1.5 / r2;
		
		p *= k;
		scale *= k;
	}
	
	return 0.25 * abs(p.y) / scale;
}

float3 GetNormal(float3 p)
{
	float2 e = float2(1e-2, 0);

	float3 normal = DistanceFractal(p) - float3(
                    DistanceFractal(p - e.xyy),
                    DistanceFractal(p - e.yxy),
                    DistanceFractal(p - e.yyx)
                    );

	return normalize(normal);
}

            //Calculate depth along viewing ray
			float SurfaceRaymarch(float3 rayOrigin, float3 rayDirection, float maxDistance)
			{
				float distanceFromOrigin = 0;
				float distanceFromSurface = 0;
			
			    //Calculate distance from surface at each point
				for (int i = 0; i < MAX_STEPS; i++)
				{
					float3 basePoint = rayOrigin + (distanceFromOrigin * rayDirection);
					float3 currentRayPoint = basePoint;
					currentRayPoint.xy = Rotate(currentRayPoint.xy, _Time);
					float3 secondaryRayPoint = basePoint;
					secondaryRayPoint.xy = Rotate(secondaryRayPoint.xy, -_Time);
					distanceFromSurface = max(DistanceFractal(secondaryRayPoint), DistanceFractal(currentRayPoint)) + (0.1 * max(1 - i, 0));
			
					distanceFromOrigin += distanceFromSurface;
			
			        //if we have hit a surface or we have gone beyond draw distance
					if (distanceFromSurface <= SURF_DIST || distanceFromOrigin > maxDistance)
					{
						break;
					}
				}
			
				return distanceFromOrigin;
			}

            fixed4 frag (v2f i) : SV_Target
            {
				//Ray origin
				float3 rayOrigin = i.rayOrigin;
	
				//Ray direction
                //Direction is just normalized displacement from point we "hit" the object
				float3 fullRay = i.hitPos - rayOrigin;
				float3 rayDirection = normalize(fullRay);
	
				float maxDistance = 100;
	
				float distanceToScene = SurfaceRaymarch(rayOrigin, rayDirection, maxDistance);
	
				float outputDist = (0.75 - (distanceToScene / maxDistance));
	
				float3 p = rayOrigin + (rayDirection * distanceToScene);
	
				//float3 normal = GetNormal(p);
				//float lighting = dot(normal, normalize(float3(0.56, 0.2, -1))) *;
	
				float lighting = pow(outputDist, 15);
	
				return float4(lighting, lighting, lighting, 1);
			}
            ENDCG
        }
    }
}
