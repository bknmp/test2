using ExitGames.Client.Photon.Voice;
using Photon;
using System;
using System.Linq;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(PhotonVoiceSpeaker))]
[DisallowMultipleComponent]
[AddComponentMenu("Photon Voice/Photon Voice Recorder")]
public class PhotonVoiceRecorder : Photon.MonoBehaviour
{
	public enum AudioSource
	{
		Microphone,
		AudioClip,
		Factory
	}

	public enum MicAudioSourceType
	{
		Settings,
		Unity,
		Photon
	}

	public enum SampleTypeConv
	{
		None,
		Short,
		ShortAuto
	}

	private LocalVoice voice = LocalVoiceAudio.Dummy;

	private string microphoneDevice;

	private int photonMicrophoneDeviceID = -1;

	private IAudioSource audioSource;

	public AudioSource Source;

	public MicAudioSourceType MicrophoneType;

	public SampleTypeConv TypeConvert;

	private bool forceShort;

	public AudioClip AudioClip;

	public bool LoopAudioClip = true;

	protected ILocalVoiceAudio voiceAudio => (ILocalVoiceAudio)voice;

	public AudioUtil.IVoiceDetector VoiceDetector
	{
		get
		{
			if (!base.photonView.isMine)
			{
				return null;
			}
			return voiceAudio.VoiceDetector;
		}
	}

	public string MicrophoneDevice
	{
		get
		{
			return microphoneDevice;
		}
		set
		{
			if (value != null && !Microphone.devices.Contains(value))
			{
				UnityEngine.Debug.LogError("PUNVoice: " + value + " is not a valid microphone device");
				return;
			}
			microphoneDevice = value;
			updateAudioSource();
		}
	}

	public int PhotonMicrophoneDeviceID
	{
		get
		{
			return photonMicrophoneDeviceID;
		}
		set
		{
			photonMicrophoneDeviceID = value;
			updateAudioSource();
		}
	}

	public byte AudioGroup
	{
		get
		{
			return voice.Group;
		}
		set
		{
			voice.Group = value;
		}
	}

	public bool IsTransmitting => voice.IsTransmitting;

	public AudioUtil.ILevelMeter LevelMeter => voiceAudio.LevelMeter;

	public bool Transmit
	{
		get
		{
			return voice.Transmit;
		}
		set
		{
			voice.Transmit = value;
		}
	}

	public bool Detect
	{
		get
		{
			return voiceAudio.VoiceDetector.On;
		}
		set
		{
			voiceAudio.VoiceDetector.On = value;
		}
	}

	public bool DebugEchoMode
	{
		get
		{
			return voice.DebugEchoMode;
		}
		set
		{
			voice.DebugEchoMode = value;
		}
	}

	public bool VoiceDetectorCalibrating => voiceAudio.VoiceDetectorCalibrating;

	private void updateAudioSource()
	{
		if (voice != LocalVoiceAudio.Dummy && Source == AudioSource.Microphone)
		{
			audioSource.Dispose();
			voice.RemoveSelf();
			base.gameObject.SendMessage("PhotonVoiceRemoved", SendMessageOptions.DontRequireReceiver);
			bool debugEchoMode = DebugEchoMode;
			DebugEchoMode = false;
			LocalVoice localVoice = voice;
			voice = createLocalVoiceAudioAndSource();
			voice.Group = localVoice.Group;
			voice.Transmit = localVoice.Transmit;
			voiceAudio.VoiceDetector.On = voiceAudio.VoiceDetector.On;
			voiceAudio.VoiceDetector.Threshold = voiceAudio.VoiceDetector.Threshold;
			sendPhotonVoiceCreatedMessage();
			DebugEchoMode = debugEchoMode;
		}
	}

	private void Awake()
	{
	}

	private void Start()
	{
		if (!base.photonView.isMine)
		{
			return;
		}
		switch (TypeConvert)
		{
		case SampleTypeConv.Short:
			forceShort = true;
			UnityEngine.Debug.LogFormat("PUNVoice: Type Convertion set to Short. Audio samples will be converted if source samples type differs.");
			break;
		case SampleTypeConv.ShortAuto:
		{
			SpeexDSP component = base.gameObject.GetComponent<SpeexDSP>();
			if (component != null && component.Active)
			{
				if (PhotonVoiceSettings.Instance.DebugInfo)
				{
					UnityEngine.Debug.LogFormat("PUNVoice: Type Convertion set to ShortAuto. SpeexDSP found. Audio samples will be converted if source samples type differs.");
				}
				forceShort = true;
			}
			break;
		}
		}
		voice = createLocalVoiceAudioAndSource();
		if (PhotonVoiceSettings.Instance != null)
		{
			VoiceDetector.On = PhotonVoiceSettings.Instance.VoiceDetection;
			VoiceDetector.Threshold = PhotonVoiceSettings.Instance.VoiceDetectionThreshold;
			if (voice != LocalVoiceAudio.Dummy)
			{
				voice.Transmit = PhotonVoiceSettings.Instance.AutoTransmit;
			}
			else if (PhotonVoiceSettings.Instance.AutoTransmit)
			{
				UnityEngine.Debug.LogWarning("PUNVoice: Cannot Transmit.");
			}
		}
		else
		{
			VoiceDetector.On = false;
			VoiceDetector.Threshold = 0.01f;
			voice.Transmit = false;
		}
		sendPhotonVoiceCreatedMessage();
	}

	private LocalVoice createLocalVoiceAudioAndSource()
	{
		PhotonVoiceSettings instance = PhotonVoiceSettings.Instance;
		if (instance == null)
		{
			return LocalVoiceAudio.Dummy;
		}
		switch (Source)
		{
		case AudioSource.Microphone:
		{
			if ((MicrophoneType == MicAudioSourceType.Settings && instance.MicrophoneType == PhotonVoiceSettings.MicAudioSourceType.Photon) || MicrophoneType == MicAudioSourceType.Photon)
			{
				if (PhotonMicrophoneDeviceID == -1)
				{
					int photonMicrophoneDeviceID2 = PhotonVoiceNetwork.PhotonMicrophoneDeviceID;
				}
				else
				{
					int photonMicrophoneDeviceID3 = PhotonMicrophoneDeviceID;
				}
				audioSource = new AndroidAudioInAEC();
				if (PhotonVoiceSettings.Instance.DebugInfo)
				{
					UnityEngine.Debug.LogFormat("PUNVoice: Setting recorder's source to AndroidAudioInAEC");
				}
				break;
			}
			if (Microphone.devices.Length < 1)
			{
				return LocalVoiceAudio.Dummy;
			}
			string text = microphoneDevice = ((MicrophoneDevice != null) ? MicrophoneDevice : PhotonVoiceNetwork.MicrophoneDevice);
			if (PhotonVoiceSettings.Instance.DebugInfo)
			{
				UnityEngine.Debug.LogFormat("PUNVoice: Setting recorder's source to microphone device {0}", text);
			}
			MicWrapper micWrapper = (MicWrapper)(audioSource = new MicWrapper(text, (int)instance.SamplingRate));
			break;
		}
		case AudioSource.AudioClip:
			if (AudioClip == null)
			{
				UnityEngine.Debug.LogErrorFormat("PUNVoice: AudioClip property must be set for AudioClip audio source");
				return LocalVoiceAudio.Dummy;
			}
			audioSource = new AudioClipWrapper(AudioClip);
			if (LoopAudioClip)
			{
				((AudioClipWrapper)audioSource).Loop = true;
			}
			break;
		case AudioSource.Factory:
			if (PhotonVoiceNetwork.AudioSourceFactory == null)
			{
				UnityEngine.Debug.LogErrorFormat("PUNVoice: PhotonVoiceNetwork.AudioSourceFactory must be specified if PhotonVoiceRecorder.Source set to Factory");
				return LocalVoiceAudio.Dummy;
			}
			audioSource = PhotonVoiceNetwork.AudioSourceFactory(this);
			break;
		default:
			UnityEngine.Debug.LogErrorFormat("PUNVoice: unknown Source value {0}", Source);
			return LocalVoiceAudio.Dummy;
		}
		VoiceInfo voiceInfo = VoiceInfo.CreateAudioOpus(instance.SamplingRate, audioSource.SamplingRate, audioSource.Channels, instance.FrameDuration, instance.Bitrate, base.photonView.viewID);
		return createLocalVoiceAudio(voiceInfo, audioSource);
	}

	protected virtual LocalVoice createLocalVoiceAudio(VoiceInfo voiceInfo, IAudioSource source)
	{
		if (source is IAudioPusher<float>)
		{
			if (forceShort)
			{
				throw new NotImplementedException("Voice.IAudioPusher<float> at 'short' voice is not supported currently");
			}
			LocalVoiceAudio<float> localVoice = PhotonVoiceNetwork.VoiceClient.CreateLocalVoiceAudio<float>(voiceInfo);
			((IAudioPusher<float>)source).SetCallback(delegate(float[] buf)
			{
				localVoice.PushDataAsync(buf);
			}, localVoice);
			return localVoice;
		}
		if (source is IAudioPusher<short>)
		{
			LocalVoiceAudio<short> localVoice2 = PhotonVoiceNetwork.VoiceClient.CreateLocalVoiceAudio<short>(voiceInfo);
			((IAudioPusher<short>)source).SetCallback(delegate(short[] buf)
			{
				localVoice2.PushDataAsync(buf);
			}, localVoice2);
			return localVoice2;
		}
		if (source is IAudioReader<float>)
		{
			if (forceShort)
			{
				if (PhotonVoiceSettings.Instance.DebugInfo)
				{
					UnityEngine.Debug.LogFormat("PUNVoice: Creating local voice with source samples type conversion from float to short.");
				}
				LocalVoiceAudio<short> localVoiceAudio = PhotonVoiceNetwork.VoiceClient.CreateLocalVoiceAudio<short>(voiceInfo);
				localVoiceAudio.LocalUserServiceable = new BufferReaderPushAdapterAsyncPoolFloatToShort(localVoiceAudio, source as IAudioReader<float>);
				return localVoiceAudio;
			}
			LocalVoiceAudio<float> localVoiceAudio2 = PhotonVoiceNetwork.VoiceClient.CreateLocalVoiceAudio<float>(voiceInfo);
			localVoiceAudio2.LocalUserServiceable = new BufferReaderPushAdapterAsyncPool<float>(localVoiceAudio2, source as IAudioReader<float>);
			return localVoiceAudio2;
		}
		if (source is IAudioReader<short>)
		{
			LocalVoiceAudio<short> localVoiceAudio3 = PhotonVoiceNetwork.VoiceClient.CreateLocalVoiceAudio<short>(voiceInfo);
			localVoiceAudio3.LocalUserServiceable = new BufferReaderPushAdapterAsyncPool<short>(localVoiceAudio3, source as IAudioReader<short>);
			return localVoiceAudio3;
		}
		UnityEngine.Debug.LogErrorFormat("PUNVoice: PhotonVoiceRecorder createLocalVoiceAudio does not support Voice.IAudioReader of type {0}", source.GetType());
		return LocalVoiceAudio.Dummy;
	}

	protected virtual void sendPhotonVoiceCreatedMessage()
	{
		base.gameObject.SendMessage("PhotonVoiceCreated", voice, SendMessageOptions.DontRequireReceiver);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (voice != LocalVoiceAudio.Dummy)
		{
			voice.RemoveSelf();
			if (audioSource != null)
			{
				audioSource.Dispose();
				audioSource = null;
			}
		}
	}

	public void VoiceDetectorCalibrate(int durationMs)
	{
		if (base.photonView.isMine)
		{
			voiceAudio.VoiceDetectorCalibrate(durationMs);
		}
	}

	private string tostr<T>(T[] x, int lim = 10)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < ((x.Length < lim) ? x.Length : lim); i++)
		{
			stringBuilder.Append("-");
			stringBuilder.Append(x[i]);
		}
		return stringBuilder.ToString();
	}

	public string ToStringFull()
	{
		int minFreq = 0;
		int maxFreq = 0;
		Microphone.GetDeviceCaps(MicrophoneDevice, out minFreq, out maxFreq);
		return $"Mic '{MicrophoneDevice}': {minFreq}..{maxFreq} Hz";
	}
}
