void OtaviogoodNoise_float(float3 Position, float Scale, float3 Offset, float Iterations, out float3 SpaceColour)
{
	float nudge = 0.9;
	float normalizer = 1.0 / sqrt(1.0 + (nudge * nudge));
	
	float n = 1;
	float iter = 2.0;
	
	Position *= Scale;
	Position += Offset;
	
	for (int i = 0; i < Iterations; i++)
	{
		// add sin and cos scaled inverse with the frequency
		n += -abs(sin(Position.y * iter) + cos(Position.x * iter)) / iter; // abs for a ridged look
        // rotate by adding perpendicular and scaling down
		Position.xy += float2(Position.y, -Position.x) * nudge;
		Position.xy *= normalizer;
        // rotate on other axis
		Position.xz += float2(Position.z, -Position.x) * nudge;
		Position.xz *= normalizer;
        // increase the frequency
		iter *= 1.733733;
	}
	
	SpaceColour = n;
}

void OtaviogoodNoise_half(float3 Position, float Scale, float3 Offset, float Iterations, out float3 SpaceColour)
{
	float nudge = 0.9;
	float normalizer = 1.0 / sqrt(1.0 + (nudge * nudge));
	
	float n = 1;
	float iter = 2.0;
	
	Position *= Scale;
	Position += Offset;
	
	for (int i = 0; i < Iterations; i++)
	{
		// add sin and cos scaled inverse with the frequency
		n += -abs(sin(Position.y * iter) + cos(Position.x * iter)) / iter; // abs for a ridged look
        // rotate by adding perpendicular and scaling down
		Position.xy += float2(Position.y, -Position.x) * nudge;
		Position.xy *= normalizer;
        // rotate on other axis
		Position.xz += float2(Position.z, -Position.x) * nudge;
		Position.xz *= normalizer;
        // increase the frequency
		iter *= 1.733733;
	}
	
	SpaceColour = n;
}