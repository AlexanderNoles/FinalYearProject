#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN

void CalculateIsShadow_float(float3 Position, out float3 ShadowIntensity)
{
    float4 shadowCoord;
    ShadowIntensity = 1;
    
    //We need to calculate the shadow coord
    //If we are looking at a shadergraph preview there is no shadows so the coord is set to 0
    //Else we do two different things depending on whether cascades are enabled or not
#ifdef SHADERGRAPH_PREVIEW
    shadowCoord = 0;
#else
    #if SHADOWS_SCREEN
        float4 positionCS = TransformWorldToHClip(Position);
        shadowCoord = ComputeScreenPos(positionCS);
    #else
        shadowCoord = TransformWorldToShadowCoord(Position);
    #endif
#endif
    
#ifdef UNIVERSAL_LIGHTING_INCLUDED
    
        ShadowIntensity = MainLightRealtimeShadow(shadowCoord);
#endif
}
#endif