using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using ExitGames.Client.Photon.Voice;
using System;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class PhotonVoiceNetwork : MonoBehaviour
{
	private static PhotonVoiceNetwork _instance;

	private static GameObject _singleton;

	private static bool destroyed = false;

	public static float BackgroundTimeout = 60f;

	internal UnityVoiceFrontend client;

	private string unityMicrophoneDevice;

	private int photonMicrophoneDeviceID = -1;

	internal static PhotonVoiceNetwork instance => _instance;

	public static Func<PhotonVoiceRecorder, IAudioSource> AudioSourceFactory
	{
		get;
		set;
	}

	public static UnityVoiceFrontend Client => instance.client;

	public static VoiceClient VoiceClient => instance.client.VoiceClient;

	public static ExitGames.Client.Photon.LoadBalancing.ClientState ClientState => instance.client.State;

	public static string CurrentRoomName
	{
		get
		{
			if (instance.client.CurrentRoom != null)
			{
				return instance.client.CurrentRoom.Name;
			}
			return "";
		}
	}

	public static string MicrophoneDevice
	{
		get
		{
			return instance.unityMicrophoneDevice;
		}
		set
		{
			if (value != null && !Microphone.devices.Contains(value))
			{
				UnityEngine.Debug.LogError("PUNVoice: " + value + " is not a valid microphone device");
				return;
			}
			instance.unityMicrophoneDevice = value;
			if (PhotonVoiceSettings.Instance.DebugInfo)
			{
				UnityEngine.Debug.LogFormat("PUNVoice: Setting global Unity microphone device to {0}", instance.unityMicrophoneDevice);
			}
			PhotonVoiceRecorder[] array = UnityEngine.Object.FindObjectsOfType<PhotonVoiceRecorder>();
			foreach (PhotonVoiceRecorder photonVoiceRecorder in array)
			{
				if (photonVoiceRecorder.photonView.isMine && photonVoiceRecorder.MicrophoneDevice == null)
				{
					photonVoiceRecorder.MicrophoneDevice = null;
				}
			}
		}
	}

	public static int PhotonMicrophoneDeviceID
	{
		get
		{
			return instance.photonMicrophoneDeviceID;
		}
		set
		{
			instance.photonMicrophoneDeviceID = value;
			if (PhotonVoiceSettings.Instance.DebugInfo)
			{
				UnityEngine.Debug.LogFormat("PUNVoice: Setting global Photon microphone device to {0}", instance.photonMicrophoneDeviceID);
			}
			PhotonVoiceRecorder[] array = UnityEngine.Object.FindObjectsOfType<PhotonVoiceRecorder>();
			foreach (PhotonVoiceRecorder photonVoiceRecorder in array)
			{
				if (photonVoiceRecorder.photonView.isMine && photonVoiceRecorder.PhotonMicrophoneDeviceID == -1)
				{
					photonVoiceRecorder.PhotonMicrophoneDeviceID = -1;
				}
			}
		}
	}

	private void OnDestroy()
	{
		if (!(this != _instance))
		{
			destroyed = true;
			client.Dispose();
		}
	}

	private PhotonVoiceNetwork()
	{
		client = new UnityVoiceFrontend(ConnectionProtocol.Udp);
	}

	public void Awake()
	{
		_instance = this;
		if (Microphone.devices.Length < 1)
		{
			UnityEngine.Debug.LogWarning("PUNVoice: No microphone device found");
		}
	}

	public static bool Connect()
	{
		instance.client.AppId = PhotonNetwork.PhotonServerSettings.VoiceAppID;
		instance.client.AppVersion = PhotonNetwork.gameVersion;
		if (PhotonNetwork.PhotonServerSettings.HostType == ServerSettings.HostingOption.SelfHosted)
		{
			string masterServerAddress = PhotonNetwork.networkingPeer.MasterServerAddress;
			UnityEngine.Debug.LogFormat("PUNVoice: connecting to master {0}", masterServerAddress);
			return instance.client.Connect(masterServerAddress, null, null, null, null);
		}
		UnityEngine.Debug.LogFormat("PUNVoice: connecting to region {0}", PhotonNetwork.networkingPeer.CloudRegion.ToString());
		return instance.client.ConnectToRegionMaster(PhotonNetwork.networkingPeer.CloudRegion.ToString());
	}

	public static void Disconnect()
	{
		if (instance != null && instance.client != null && ClientState != ExitGames.Client.Photon.LoadBalancing.ClientState.Disconnected && ClientState != ExitGames.Client.Photon.LoadBalancing.ClientState.Disconnecting)
		{
			UnityEngine.Debug.Log("PhotonVoiceNetwork Disconnect");
			instance.client.Disconnect();
		}
	}

	protected void OnEnable()
	{
		bool flag = this != _instance;
	}

	protected void OnApplicationQuit()
	{
		if (!(this != _instance))
		{
			client.Disconnect();
			client.Dispose();
		}
	}

	protected void Update()
	{
		if (!(this != _instance))
		{
			client.Service();
		}
	}

	private void OnJoinedRoom()
	{
		if (this != _instance)
		{
			return;
		}
		ExitGames.Client.Photon.LoadBalancing.ClientState state = client.State;
		if (state == ExitGames.Client.Photon.LoadBalancing.ClientState.Joined)
		{
			if (PhotonVoiceSettings.Instance.AutoConnect)
			{
				client.OpLeaveRoom();
			}
		}
		else if (PhotonVoiceSettings.Instance.AutoConnect)
		{
			client.Reconnect();
		}
	}

	private void OnLeftRoom()
	{
		if (!(this != _instance) && PhotonVoiceSettings.Instance.AutoDisconnect)
		{
			client.Disconnect();
		}
	}

	private void OnDisconnectedFromPhoton()
	{
		if (!(this != _instance) && PhotonVoiceSettings.Instance.AutoDisconnect)
		{
			client.Disconnect();
		}
	}

	internal static void LinkSpeakerToRemoteVoice(PhotonVoiceSpeaker speaker)
	{
		instance.client.LinkSpeakerToRemoteVoice(speaker);
	}

	internal static void UnlinkSpeakerFromRemoteVoice(PhotonVoiceSpeaker speaker)
	{
		if (!destroyed)
		{
			instance.client.UnlinkSpeakerFromRemoteVoice(speaker);
		}
	}
}
