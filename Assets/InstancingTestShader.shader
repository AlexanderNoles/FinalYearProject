Shader "Unlit/InstancingTestShader"
{
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			#include "UnityCG.cginc"
			#define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
			#include "UnityIndirect.cginc"
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 color : COLOR0;
			};

float hash11a(float p)
{
	p = frac(p * .1031);
	p *= p + 33.33;
	p *= p + p;
	return (frac(p) - 0.5) * 2;
}

float hash11b(float p)
{
	p = frac(p * .4034);
	p *= p + 33.33;
	p *= p + p;
	return (frac(p) - 0.5) * 2;
}

			float3 hash31(float p)
			{
				float3 p3 = frac(float3(p, p, p) * float3(.1031, .1030, .0973));
				p3 += dot(p3, p3.yzx + 33.33);
				return frac((p3.xxy + p3.yzz) * p3.zyx);
			}
			
			uniform float4x4 _ObjectToWorld;
			
			v2f vert(appdata_base v, uint svInstanceID : SV_InstanceID)
			{
				InitIndirectDrawArgs(0);
				v2f o;
				uint cmdID = GetCommandID(0);
				uint instanceID = GetIndirectInstanceID(svInstanceID);
	
				float percentage = (instanceID / float(GetIndirectInstanceCount()));
				float3 offset = float3(hash11a(percentage), 0, hash11b(percentage)) * 2000;
	
				float3 localPos = v.vertex + offset;
				float4 wpos = mul(_ObjectToWorld, float4(localPos, 1.0));
				o.pos = mul(UNITY_MATRIX_VP, wpos);
				o.color = 1;
				return o;
			}
			
			float4 frag(v2f i) : SV_Target
			{
				return i.color;
			}
            ENDCG
        }
    }
}
