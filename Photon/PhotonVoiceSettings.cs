using ExitGames.Client.Photon.Voice;
using POpusCodec.Enums;
using UnityEngine;

[DisallowMultipleComponent]
public class PhotonVoiceSettings : MonoBehaviour
{
	public enum MicAudioSourceType
	{
		Unity,
		Photon
	}

	public bool AutoConnect = true;

	public bool AutoDisconnect;

	public bool AutoTransmit = true;

	public SamplingRate SamplingRate = SamplingRate.Sampling24000;

	public OpusCodec.FrameDuration FrameDuration = OpusCodec.FrameDuration.Frame20ms;

	public int Bitrate = 30000;

	public bool VoiceDetection;

	public float VoiceDetectionThreshold = 0.01f;

	public int PlayDelayMs = 200;

	public MicAudioSourceType MicrophoneType;

	public int DebugLostPercent;

	public bool DebugInfo;

	private static PhotonVoiceSettings instance;

	public static PhotonVoiceSettings Instance => instance;

	private void Awake()
	{
		instance = this;
	}
}
