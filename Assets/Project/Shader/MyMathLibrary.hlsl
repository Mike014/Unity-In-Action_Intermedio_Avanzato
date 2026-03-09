// Include Guards — evitano doppia inclusione se
// questo file viene referenziato da più shader.
// Pattern standard in C/C++/HLSL.
#ifndef MY_MATH_LIBRARY_INCLUDED
#define MY_MATH_LIBRARY_INCLUDED

// half4 = vettore di 4 valori a 16bit (r,g,b,a)
// Più efficiente di float4 su GPU mobile.
// float = 32bit, half = 16bit — usa half per colori,
// float per posizioni dove la precisione è critica.
half4 ComputeHitFlash(half4 baseColor, float hitFactor)
{
    // lerp(a, b, t) = a + (b - a) * t
    // t=0 → restituisce a (baseColor)
    // t=1 → restituisce b (bianco puro)
    // t=0.5 → mix esatto al 50%
    return lerp(baseColor, half4(1.0, 1.0, 1.0, 1.0), hitFactor);
}

#endif