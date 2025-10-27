#ifndef TREE_BILLBOARD_COMMON_URP
#define TREE_BILLBOARD_COMMON_URP

float4 _ImageTexcoords[256];
int _ImageCount;

void GetImageTexcoord_float(float2 UV0, float3 LocalLookDirection, out float2 BillboardUV)
{
	float dotX = dot(LocalLookDirection, float3(1, 0, 0));
	float dotZ = dot(LocalLookDirection, float3(0, 0, 1));
	float rad = atan2(dotZ, dotX);
	rad = (rad + TWO_PI) % TWO_PI;

	float f = rad / TWO_PI;
	float imageIndexF = f * _ImageCount + 0.5;
	int imageIndex = imageIndexF >= _ImageCount ? 0 : int(imageIndexF);

	float4 rect = _ImageTexcoords[imageIndex];
	float2 min = rect.xy;
	float2 max = rect.xy + rect.zw;

	float2 result = float2(
		lerp(min.x, max.x, UV0.x),
		lerp(min.y, max.y, UV0.y));
	BillboardUV = result;
}

//Make the billboard mesh faces toward the camera
//The billboard should only rotate on Y axis
void BillboardVertex_float(float3 InLocalPosition, float2 InUV0, out float3 LocalPosition, out float3 LocalNormal, out float3 LocalTangent, out float2 ImageTexcoord)
{
	//Construct a rotation matrix in object space to rotate vertices facing the camera look direction.
	float3 cameraUpVectorOS = float3(0, 1, 0); //Assuming that trees always pointing upward. This won't work with trees that snapped to world colliders.
	float4x4 worldToObject = UNITY_MATRIX_I_M; //Use this macro instead of unity_WorldToObject. There are something with instancing.
	float3 cameraPositionOS = mul(worldToObject, float4(_WorldSpaceCameraPos,1)).xyz;
	cameraPositionOS.y = 0;
	float3 directionToCameraOS = normalize(cameraPositionOS);
	float3 cameraRightVectorOS = cross(cameraUpVectorOS, directionToCameraOS);

	LocalNormal = directionToCameraOS;
	LocalTangent = -cameraRightVectorOS; //negate here, unless you want it lit up in the opposite direction

	float4x4 faceTowardCameraMatrix = float4x4
		(
			float4(-cameraRightVectorOS, 0), //why negate? because it works!
			float4(cameraUpVectorOS, 0),
			float4(directionToCameraOS, 0),
			float4(0, 0, 0, 0)
			);

	LocalPosition = mul(faceTowardCameraMatrix, float4(InLocalPosition, 0)).xyz;
	GetImageTexcoord_float(InUV0, -LocalNormal, ImageTexcoord);
}

#endif