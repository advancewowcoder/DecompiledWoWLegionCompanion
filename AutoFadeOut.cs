using System;
using UnityEngine;

public class AutoFadeOut : MonoBehaviour
{
	public CanvasGroup m_canvasGroupToFadeOut;

	public float m_fadeOutTime;

	private bool m_enableFadeOut;

	private float m_elapsedFadeTime;

	public void EnableFadeOut()
	{
		this.m_enableFadeOut = true;
	}

	public void Reset()
	{
		this.m_enableFadeOut = false;
		this.m_canvasGroupToFadeOut.set_alpha(1f);
		this.m_elapsedFadeTime = 0f;
	}

	private void Update()
	{
		if (this.m_enableFadeOut)
		{
			this.m_elapsedFadeTime += Time.get_deltaTime();
			this.m_canvasGroupToFadeOut.set_alpha(1f - Mathf.Clamp01(this.m_elapsedFadeTime / this.m_fadeOutTime));
		}
	}
}
