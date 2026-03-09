Shader "Custom/HitFlash"
{
    Properties
    {
        // _MainTex → texture principale, visibile nell'Inspector del Material
        // "white" → valore default se non assegnata
        _MainTex ("Texture", 2D) = "white" {}
        
        // _HitFactor → controllato dallo script C# a runtime
        // Range(0,1) → slider nell'Inspector
        // 0 = colore normale, 1 = bianco pieno
        _HitFactor ("Hit Factor", Range(0.0, 1.0)) = 0.0
    }

    SubShader
    {
        // Tags: dice a Unity quando e come renderizzare questo oggetto.
        // RenderType Opaque = oggetto solido, nessuna trasparenza.
        Tags { "RenderType"="Opaque" }

        Pass
        {
            HLSLPROGRAM

            // Dichiara quali funzioni sono vertex e fragment shader.
            #pragma vertex vert
            #pragma fragment frag

            // Include le utility base di Unity (trasformazioni, spazio)
            #include "UnityCG.cginc"

            // Include la nostra libreria custom
            #include "MyMathLibrary.hlsl"

            // Dichiarazione delle variabili che corrispondono alle Properties.
            // DEVONO avere lo stesso nome esatto.
            sampler2D _MainTex;
            float4 _MainTex_ST; // ST = Scale/Tiling — Unity lo genera automaticamente
            float _HitFactor;

            // Struttura dati in INPUT al vertex shader.
            // POSITION e TEXCOORD0 sono "semantics" HLSL —
            // dicono alla GPU quale dato della mesh leggere.
            struct appdata
            {
                float4 vertex : POSITION;  // posizione del vertice in local space
                float2 uv     : TEXCOORD0; // coordinate UV della texture
            };

            // Struttura dati in OUTPUT dal vertex shader
            // e in INPUT al fragment shader.
            struct v2f
            {
                float4 pos : SV_POSITION; // posizione in clip space (obbligatorio)
                float2 uv  : TEXCOORD0;   // UV passate al fragment shader
            };

            // VERTEX SHADER
            // Trasforma ogni vertice da local space a clip space.
            // Gira una volta per ogni vertice della mesh.
            v2f vert(appdata v)
            {
                v2f o;

                // UnityObjectToClipPos: moltiplica per MVP matrix
                // (Model * View * Projection) — trasformazione standard.
                o.pos = UnityObjectToClipPos(v.vertex);

                // TRANSFORM_TEX applica Tiling e Offset del Material alle UV.
                // Senza questo, il Tiling impostato nel Material viene ignorato.
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            // FRAGMENT SHADER
            // Calcola il colore finale di ogni pixel.
            // Gira una volta per ogni pixel coperto dalla mesh.
            half4 frag(v2f i) : SV_Target
            {
                // Campiona la texture alle coordinate UV del pixel corrente.
                half4 baseColor = tex2D(_MainTex, i.uv);

                // Chiama la nostra funzione dalla libreria custom.
                // _HitFactor viene modificato dallo script C# a runtime.
                return ComputeHitFlash(baseColor, _HitFactor);
            }

            ENDHLSL
        }
    }
}