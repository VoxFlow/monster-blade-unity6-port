using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FakeSoftParticleCamera : MonoBehaviour
{
	public float m_groundHeight;

	public float m_globalSoftParticleFactor = 1f;

	private void OnPreRender()
	{
		// PORT (CD-033)
		Camera cam = base.GetComponent<Camera>();
		Matrix4x4 gpuProjection = GL.GetGPUProjectionMatrix(cam.projectionMatrix, cam.targetTexture != null);
		Vector4 vec = (gpuProjection * cam.worldToCameraMatrix).transpose.inverse * new Vector4(0f, m_globalSoftParticleFactor, 0f, (0f - m_globalSoftParticleFactor) * m_groundHeight);
		Shader.SetGlobalVector("_GroundPlane", vec);
	}
}
