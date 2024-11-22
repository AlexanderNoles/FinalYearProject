void MainLight_float(out float3 Color)
{
#ifdef SHADERGRAPH_PREVIEW
    Color = 1.0f;
#else
	Light mainLight = GetMainLight();
	Color = mainLight.color;
#endif
}

void MainLight_half(out half3 Color)
{
#ifdef SHADERGRAPH_PREVIEW
    Color = 1.0f;
#else
	Light mainLight = GetMainLight();
	Color = mainLight.color;
#endif
}