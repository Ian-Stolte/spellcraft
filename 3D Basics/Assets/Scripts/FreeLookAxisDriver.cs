using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineFreeLook)), DisallowMultipleComponent]
public class FreeLookAxisDriver : MonoBehaviour
{
    public CinemachineInputAxisDriver xAxis;
    public CinemachineInputAxisDriver yAxis;

    private CinemachineFreeLook freeLook;

    private void Awake()
    {
        freeLook = GetComponent<CinemachineFreeLook>();
        freeLook.m_XAxis.m_MaxSpeed = freeLook.m_XAxis.m_AccelTime = freeLook.m_XAxis.m_DecelTime = 0;
        freeLook.m_XAxis.m_InputAxisName = string.Empty;
        freeLook.m_YAxis.m_MaxSpeed = freeLook.m_YAxis.m_AccelTime = freeLook.m_YAxis.m_DecelTime = 0;
        freeLook.m_YAxis.m_InputAxisName = string.Empty;
    }

    private void OnValidate()
    {
        xAxis.Validate();
        yAxis.Validate();
    }

    private void Reset()
    {
        xAxis = new CinemachineInputAxisDriver
        {
            multiplier = -10f,
            accelTime = 0.1f,
            decelTime = 0.1f,
            name = "Mouse X",
        };
        yAxis = new CinemachineInputAxisDriver
        {
            multiplier = 0.1f,
            accelTime = 0.1f,
            decelTime = 0.1f,
            name = "Mouse Y",
        };
    }

    private void Update()
    {
        bool changed = yAxis.Update(Time.deltaTime, ref freeLook.m_YAxis);
        changed |= xAxis.Update(Time.deltaTime, ref freeLook.m_XAxis);
        if (changed)
        {
            freeLook.m_RecenterToTargetHeading.CancelRecentering();
            freeLook.m_YAxisRecentering.CancelRecentering();
        }
    }
}