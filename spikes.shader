// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Spikes" {
    Properties
    {
        _NoiseTex("Noise", 2D) = "white" {}
        _AudioVolume("Volume", float) = 0
        _AudioPitch("Pitch", float) = 0

        _LightPoint("Light Point Position", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _NoiseTex;
            float _AudioVolume;
            float _AudioPitch;
            
            float4 _LightPoint;
            
            void vert(
                in float4 vertexPos : POSITION,
                in float4 normal : NORMAL,
                in float2 uv : TEXCOORD0,

                out float4 pos : SV_POSITION,
                out float3 worldNormal : TEXCOORD1,
                out float3 worldPosition : TEXCOORD2,
                out float4 col : TEXCOORD0)
            {
                // Calculate the position of the pixel on our
                // Perlin noise.
                float4 pixel_location = float4(
                    vertexPos.x + (_Time[1] / 5),
                    vertexPos.y + (_Time[1] / 5),
                    0,
                    0);

                // Get the r pixel value of the perlin noise to 
                // use as a random number and multiply it by
                // the audio volume to make the number larger
                // or smaller depending on volume
                float random_number = tex2Dlod(_NoiseTex, pixel_location).r * _AudioVolume;

                // Calculate the new position by adding the offet
                // of our current position to our current position.
                // Render this new position to the camera and 
                // get that new position
                pos = UnityObjectToClipPos(vertexPos  + (random_number * vertexPos));

                // Convert the normal to a worldspace normal
                // So that orientation dof the object does not matter
                worldNormal = UnityObjectToWorldNormal(normal);

                // Get the position of the vertex in the world space
                worldPosition = mul(unity_ObjectToWorld, vertexPos);

                // Color
                float blue = 0.0 + _AudioPitch;
                float red = 1.0 - _AudioPitch;

                col = float4(red, 0, blue, 1);

                return;
            }

            float4 frag(
                in float4 pos : SV_POSITION,
                in float3 worldNormal : TEXCOORD1,
                in float3 worldPosition : TEXCOORD2,
                in float4 col : TEXCOORD0) : COLOR
            {
                // Get the difference in location of the light point 
                // and the pixel. We use xyz to get the x, y, and z 
                // to turn the light point into a 3d vector
                fixed3 lightDifference = worldPosition - _LightPoint.xyz;
                
                // Get the direction of the light by normalizing it
                fixed3 lightDirection = normalize(lightDifference);

                // When we apply the dot product on two normalized vectors,
                // we get a value between -1 and 1. When the two vectors are
                // pointing towards each other, the dot will be -1. If they 
                // point in the same direction, the dot product will return -1.
                // And if they are perpendicular, the dot product returns 0.

                // In other words: the dot product of two normalized vectors 
                // will be equal to the cosine of the two vectors.

                // When a light shines directly onto out pixel, it means that
                // the normal vectors point towards each other. This will result
                // in a dot product of -1. However, because we are going to 
                // multiply our color by this intensity, we need it to be as
                // bright as can be in this situation, so we flip the value.
                // This way, when the light shines upon the object, the
                // color will not be removed.
                fixed intensity = -1 * dot(lightDirection, worldNormal);
                
                // Finally we multiply our color with the intensity,
                // with a 1 for the alpha. And we have our final color.
                col = fixed4(intensity, intensity, intensity, 1) * col;
                return col;
            }

            ENDCG
        }
    }
}