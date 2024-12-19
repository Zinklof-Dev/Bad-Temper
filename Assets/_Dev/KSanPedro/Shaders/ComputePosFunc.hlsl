
StructuredBuffer<float4> PositionBuffer;
StructuredBuffer<float4> QuaternionBuffer;

void GetPos_float(const uint InstanceID : SV_InstanceID, out float3 Out)
{
    float4 buffer = PositionBuffer[InstanceID];
    
    Out = buffer.xyz;
}

void GetQuat_float(float instanceID, out float4 Out)
{
    Out = QuaternionBuffer[instanceID];
}