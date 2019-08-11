Shader "Custom/Rubik"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_Factors ("Blinn Factors", Vector) = (0.8, 0.7, 0.1, 1)

		[Toggle]_RubikFocus("Rubik Focus", Range(0, 1)) = 0
		_RubikOrder ("Rubik Order", Range(1, 10)) = 3
		_RubikPos ("Rubik Position", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _MainTex_ST;
			fixed4 _Color;
			half4 _Factors;
			half _RubikOrder;
			half3 _RubikPos;
			fixed _RubikFocus;
			fixed _RubikFocusExpand;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float3 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;

				// Rubiks
				half sideWidth = (_RubikOrder - 1) / 2.0;
				fixed3 faceColor = fixed3(0.3, 0.3, 0.3);

				if (v.normal.x == 1)//right red
				{
					if (_RubikPos.x == sideWidth)
						faceColor = fixed3(1, 0, 0);
				}
				else if (v.normal.x == -1)//left orange
				{
					if (_RubikPos.x == -sideWidth)
						faceColor = fixed3(1, 0.5, 0);
				}
				else if (v.normal.y == 1)//up yellow
				{
					if (_RubikPos.y == sideWidth)
						faceColor = fixed3(1, 1, 0);
				}
				else if (v.normal.y == -1)//down white
				{
					if (_RubikPos.y == -sideWidth)
						faceColor = fixed3(1, 1, 1);
				}
				else if (v.normal.z == 1)//back green
				{
					if (_RubikPos.z == sideWidth)
						faceColor = fixed3(0, 1, 0);
				}
				else if (v.normal.z == -1)//front blue
				{
					if (_RubikPos.z == -sideWidth)
						faceColor = fixed3(0, 0, 1);
				}

				if (_RubikFocus == 1)
				{
					// highlight
					faceColor += fixed3(0.1, 0.1, 0.1);//highlight

					// scale
					_RubikFocusExpand = 0.02;
					v.vertex.x += v.vertex.x > 0 ? _RubikFocusExpand : -_RubikFocusExpand;
					v.vertex.y += v.vertex.y > 0 ? _RubikFocusExpand : -_RubikFocusExpand;
					v.vertex.z += v.vertex.z > 0 ? _RubikFocusExpand : -_RubikFocusExpand;
				}

				// Basic
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				// Clamp
				_Factors.x = max(_Factors.x, 0);
				_Factors.y = max(_Factors.y, 0);
				_Factors.z = max(_Factors.z, 0);
				_Factors.w = max(_Factors.w, 0);
				
				// Ambient
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * _Factors.x;
				// Diffuse = I * dot(L, N)
				fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
				fixed3 worldLightDir = normalize(WorldSpaceLightDir(v.vertex));
				fixed3 diffuse = _Color * saturate(dot(worldNormal, worldLightDir)) * _Factors.y;
				// Specular = I * dot(H, N)^n
				fixed3 worldViewDir = normalize(WorldSpaceViewDir(v.vertex));
				fixed3 halfReflect = saturate(normalize(worldViewDir + worldLightDir));
				fixed3 specular = _Color * pow(saturate(dot(worldNormal, halfReflect)), _Factors.w) * _Factors.z;
				// Blinn-Phong Light Model
				fixed3 blinnColor = ambient + diffuse + specular;

				// Mix Color
				o.color = blinnColor * faceColor;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return fixed4(i.color, 1);
            }
            ENDCG
        }

		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
