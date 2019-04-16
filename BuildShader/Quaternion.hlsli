///////////////////////////////////////////
// クォータニオン関連

#define QUATERNION_IDENTITY float4(0, 0, 0, 1)

// 絶対値がこれよりも小さい数はゼロとみなされる。
#define ZeroTolerance 1e-6f

// Quaternion の球形補間
float4 quaternion_slerp(float4 start, float4 end, float amount)
{
	float4 result = QUATERNION_IDENTITY;

	float opposite;
	float inverse;
	float dt = dot(start, end);

	if (abs(dt) > 1.0f - ZeroTolerance)
	{
		inverse = 1.0f - amount;
		opposite = amount * sign(dt);
	}
	else
	{
		float acs = acos(abs(dt));
		float invSin = 1.0f / sin(acs);

		inverse = sin((1.0f - amount) * acs) * invSin;
		opposite = sin(amount * acs) * invSin * sign(dt);
	}

	result.x = (inverse * start.x) + (opposite * end.x);
	result.y = (inverse * start.y) + (opposite * end.y);
	result.z = (inverse * start.z) + (opposite * end.z);
	result.w = (inverse * start.w) + (opposite * end.w);

	return result;
}

// Quaternion (float4) から Matrix (float4x4) への変換
float4x4 quaternion_to_matrix(float4 quat)
{
    float4x4 m = float4x4(float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0));

	float xx = quat.x * quat.x;
	float yy = quat.y * quat.y;
	float zz = quat.z * quat.z;
	float xy = quat.x * quat.y;
	float zw = quat.z * quat.w;
	float zx = quat.z * quat.x;
	float yw = quat.y * quat.w;
	float yz = quat.y * quat.z;
	float xw = quat.x * quat.w;

	m[0][0] = 1.0f - (2.0f * (yy + zz));
	m[0][1] = 2.0f * (xy + zw);
	m[0][2] = 2.0f * (zx - yw);
	m[0][3] = 0.0f;
	m[1][0] = 2.0f * (xy - zw);
	m[1][1] = 1.0f - (2.0f * (zz + xx));
	m[1][2] = 2.0f * (yz + xw);
	m[1][3] = 0.0f;
	m[2][0] = 2.0f * (zx + yw);
	m[2][1] = 2.0f * (yz - xw);
	m[2][2] = 1.0f - (2.0f * (yy + xx));
	m[2][3] = 0.0f;
	m[3][0] = 0.0f;
	m[3][1] = 0.0f;
	m[3][2] = 0.0f;
	m[3][3] = 1.0f;

    return m;
}
