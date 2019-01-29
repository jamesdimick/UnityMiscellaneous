using System.Reflection;
using UnityEngine;
using UnityEditor.ShaderGraph;

namespace Jick
{
	[Title("Jick","Fade Over Distance")]
	public class FadeOverDistanceNode : CodeFunctionNode, IMayRequirePosition
	{
		public FadeOverDistanceNode()
		{
			name = "Fade Over Distance";
		}

		protected override MethodInfo GetFunctionToConvert()
		{
			return GetType().GetMethod( "FadeOverDistance", BindingFlags.Static | BindingFlags.NonPublic );
		}

		/// <summary>
		/// Fade over distance.
		/// </summary>
		/// <param name="Position">The position to measure distance from (usually set this to the camera position).</param>
		/// <param name="Distances">The distance values to fade at (X:The near fade start distance, Y:The near fade end distance, Z:The far fade start distance, W:The far fade end distance).</param>
		/// <param name="Alpha">Whether to return an alpha value (for alpha fading) or a dithered value (0 or 1).</param>
		/// <param name="WorldSpacePosition">The position of the current pixel in world space (usually don't need to set this).</param>
		/// <param name="ScreenSpacePosition">The position of the current pixel in screen space (usually don't need to set this).</param>
		/// <param name="Out">A floating-point value between 0 and 1 (usually plug this into the Alpha input on the master node).</param>
		/// <returns></returns>
		private static string FadeOverDistance( [Slot(0,Binding.None)] Vector3 Position,
		                                        [Slot(1,Binding.None,0f,0.15f,45f,50f)] Vector4 Distances,
		                                        [Slot(2,Binding.None,0f,0f,0f,0f)] Boolean Alpha,
		                                        [Slot(3,Binding.WorldSpacePosition,true)] Vector3 WorldSpacePosition,
		                                        [Slot(4,Binding.ScreenPosition,true)] Vector4 ScreenSpacePosition,
		                                        [Slot(5,Binding.None)] out Vector1 Out )
		{
			return @"
{
	half dist = distance( Position, WorldSpacePosition );
	half a = saturate( ( dist - Distances.x ) / ( Distances.y - Distances.x ) ) * saturate( 1 - ( ( dist - Distances.z ) / ( Distances.w - Distances.z ) ) );
	Out = Alpha ? a : jick_dither( a, ScreenSpacePosition );
}
";
		}

		public override void GenerateNodeFunction( FunctionRegistry registry, GraphContext graphContext, GenerationMode generationMode )
		{
			registry.ProvideFunction( "jick_dither", s => s.Append( @"
half jick_dither( half a, half2 screenPos )
{
    half2 uv = screenPos.xy * _ScreenParams.xy;
    half DITHER_THRESHOLDS[16] = {1.0/17.0,9.0/17.0,3.0/17.0,11.0/17.0,13.0/17.0,5.0/17.0,15.0/17.0,7.0/17.0,4.0/17.0,12.0/17.0,2.0/17.0,10.0/17.0,16.0/17.0,8.0/17.0,14.0/17.0,6.0/17.0};
    return a - DITHER_THRESHOLDS[ ( ( uint( uv.x ) % 4 ) * 4 + uint( uv.y ) % 4 ) ];
}
" ) );

			base.GenerateNodeFunction( registry, graphContext, generationMode );
		}

		public NeededCoordinateSpace RequiresPosition()
		{
			return NeededCoordinateSpace.World;
		}
	}
}
