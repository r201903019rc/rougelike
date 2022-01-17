Shader "Custom/Doter_Shader_CutOut"{
	Properties{
	_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
	_Cutoff("Alpha cutoff", Range(0,2)) = 0.5
	}
		SubShader{
			Tags {"Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout"}
			LOD 100

			Lighting Off

			Pass {
				CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag
					#pragma target 2.0
					#pragma multi_compile_fog

					#include "UnityCG.cginc"

					struct appdata_t {
						float4 vertex : POSITION;
						float2 texcoord : TEXCOORD0;
						UNITY_VERTEX_INPUT_INSTANCE_ID
					};

					struct v2f {
						float4 vertex : SV_POSITION;
						float2 texcoord : TEXCOORD0;
						UNITY_VERTEX_OUTPUT_STEREO
					};

					sampler2D _MainTex;
					float4 _MainTex_ST;
					fixed _Cutoff;

					v2f vert(appdata_t v)
					{
						v2f o;
						UNITY_SETUP_INSTANCE_ID(v);
						UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
						o.vertex = UnityObjectToClipPos(v.vertex);
						o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
						return o;
					}

					fixed4 frag(v2f i) : SV_Target
					{
						fixed4 col = tex2D(_MainTex, i.texcoord);
						clip(col.a - _Cutoff);
						return col;
					}
				ENDCG
			}
	}

}
	/*
	Properties{
	 [NoScaleOffset] _MainTex("Texture", 2D) = "white" {}
	 _NomalColor("NomalColor",Color) = (0.0, 0.0, 0.0)
		 _Cutoff("Alpha cutoff", Range(0,1)) = 0.5

	 _GradationNum("Gradation",Range(0,1)) = 0.4
	 _GradationColor("GradationColor",Range(0,1)) = 0.4

	 _LineThick("LineThick",RANGE(0, 5)) = 2
	_LineColor("LineColor",Color) = (0.0, 0.0, 0.0)

	[NoScaleOffset]_MatCap("Mat Cap", 2D) = "black" {}
	_CapPower("CapPower",RANGE(0,1)) = 0.2
		_BlendPower("BlendPower",Range(0,1)) = 0.2
	}

		SubShader{
			Tags {"Queue" = "AlphaTest" "RenderType" = "TransparentCutout"}
			LOD 100
		//オブジェクトの内側をダミー色で塗りつぶすパス
		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata {
				half4 vertex : POSITION;
			};

			struct v2f {
				half4 vertex : SV_POSITION;
			};

			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target{
				return fixed4(1.0, 0.0, 1.0, 0);
			}
			ENDCG
		}
		GrabPass {}
				//アウトラインを引いてポスタライズ描画
				Pass{
					CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag

					#include "UnityCG.cginc"
					#include "kansuu.cginc"
					#define EPSILON 0.001
					#define DUMMY_COLOR fixed3(1.0, 0.0, 1.0)
					#define SAMPLE_NUM 6
					#define SAMPLE_INV 0.16666666
					#define PI2 6.2831852

					struct appdata {
						half4 vertex : POSITION;
						float3 normal : NORMAL;
						half4 grabPos : TEXCOORD1;
						float3 lightDir : TEXCOORD0;
						half2 uv    : TEXCOORD2;
					};

					struct v2f {
						fixed4 shadecolor : TEXCOORD4;
						half4 pos : SV_POSITION;
						float3 normal : NORMAL;
						half4 grabPos : TEXCOORD1;
						float3 lightDir : TEXCOORD0;
						half2 uv    : TEXCOORD2;
						float2 uvtex: TEXCOORD3;
					};

					sampler2D _MainTex;
					float4 _MainTex_ST;
					sampler2D _GrabTexture;
					float4 _GrabTexture_ST;
					sampler2D _MatCap;
					float _Cutoff;

					fixed4  _LineColor;
					fixed4 _NomalColor;
					float _CapPower;
					float _CapPow;

					float4 _GrabTexture_TexelSize;

					float _GradationColor;
					float 	_BlendPower;

					int _PixelSize;

					v2f vert(appdata v) {
						v2f o;
						o.pos = UnityObjectToClipPos(v.vertex);
						o.normal = UnityObjectToWorldNormal(v.normal);
						o.grabPos = ComputeGrabScreenPos(o.pos);
						//光の方向
						o.lightDir = normalize(ObjSpaceLightDir(v.vertex));
						// カメラ座標系の法線
						float3 normal = UnityObjectToWorldNormal(v.normal);
						normal = mul((float3x3)UNITY_MATRIX_V, normal);
						// 法線のxyを0～1に変換する
						o.uv = normal.xy * 0.5 + 0.5;
						//テクスチャに合わせた法線
						o.uvtex = TRANSFORM_TEX(v.uv, _MainTex);
						//陰色を求める
						float l = dot(normalize(o.lightDir), v.normal) * 0.5 + 0.5;
						o.shadecolor = lerp(_NomalColor * _GradationColor, _NomalColor, l * l);
						return o;
					}

					fixed4 frag(v2f i) : SV_Target{
						
						//ハイライトの強さが_CapPower以下であれば、減色した陰とテクスチャを、以上ならハイライトを描画
						float a = 0.5;
						fixed4 col = ((rgb2hsv(tex2D(_MatCap, i.uv)).z < _CapPower) ?
							posterize(i.shadecolor * _BlendPower + tex2D(_MainTex, i.uvtex) * (1 - _BlendPower))
							: posterize(tex2D(_MatCap, i.uv) * _BlendPower + tex2D(_MainTex, i.uvtex) * (1 - _BlendPower))
							);
						//エッジを描画
						col = lerp(col, _LineColor, edge_col(i.grabPos, _GrabTexture));
						
						//fixed4 col = tex2D(_MatCap, i.uv);
						clip(col.a - _Cutoff);

						return col;
					}
					ENDCG
				}
	}
}
*/