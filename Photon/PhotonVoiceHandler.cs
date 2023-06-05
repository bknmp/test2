using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using System.Diagnostics;
using UnityEngine;

[DisallowMultipleComponent]
public class PhotonVoiceHandler : MonoBehaviour
{
	private static bool sendThreadShouldRun;

	private static Stopwatch timerToStopConnectionInBackground;

	private static void StartFallbackSendAckThread()
	{
		if (!sendThreadShouldRun)
		{
			sendThreadShouldRun = true;
			SupportClass.StartBackgroundCalls(FallbackSendAckThread);
		}
	}

	private static void StopFallbackSendAckThread()
	{
		sendThreadShouldRun = false;
	}

	private static bool FallbackSendAckThread()
	{
		if (sendThreadShouldRun && PhotonVoiceNetwork.Client != null && PhotonVoiceNetwork.Client.loadBalancingPeer != null)
		{
			ExitGames.Client.Photon.LoadBalancing.LoadBalancingPeer loadBalancingPeer = PhotonVoiceNetwork.Client.loadBalancingPeer;
			ExitGames.Client.Photon.LoadBalancing.ClientState state = PhotonVoiceNetwork.Client.State;
			if (timerToStopConnectionInBackground != null && PhotonVoiceNetwork.BackgroundTimeout > 0.1f && (float)timerToStopConnectionInBackground.ElapsedMilliseconds > PhotonVoiceNetwork.BackgroundTimeout * 1000f)
			{
				bool flag = true;
				if (state == ExitGames.Client.Photon.LoadBalancing.ClientState.PeerCreated || (uint)(state - 12) <= 1u || state == ExitGames.Client.Photon.LoadBalancing.ClientState.ConnectedToNameServer)
				{
					flag = false;
				}
				if (flag)
				{
					PhotonVoiceNetwork.Disconnect();
				}
				timerToStopConnectionInBackground.Stop();
				timerToStopConnectionInBackground.Reset();
				return sendThreadShouldRun;
			}
			if (loadBalancingPeer.ConnectionTime - loadBalancingPeer.LastSendOutgoingTime > 200)
			{
				loadBalancingPeer.SendAcksOnly();
			}
		}
		return sendThreadShouldRun;
	}

	private void Start()
	{
		if (null != PhotonVoiceNetwork.instance)
		{
			StartFallbackSendAckThread();
		}
		else
		{
			UnityEngine.Debug.LogError("[PUNVoice]: \"FallbackSendAckThread\" not started because PhotonVoiceNetwork instance not ready yet.");
		}
	}

	protected void Update()
	{
		if (!PhotonVoiceNetwork.instance.enabled)
		{
			return;
		}
		ExitGames.Client.Photon.LoadBalancing.LoadBalancingPeer loadBalancingPeer = PhotonVoiceNetwork.Client.loadBalancingPeer;
		if (loadBalancingPeer == null)
		{
			UnityEngine.Debug.LogError("[PUNVoice]: LoadBalancingPeer broke!");
			return;
		}
		ExitGames.Client.Photon.LoadBalancing.ClientState state = PhotonVoiceNetwork.Client.State;
		bool flag = true;
		if (state == ExitGames.Client.Photon.LoadBalancing.ClientState.PeerCreated || (uint)(state - 12) <= 1u || state == ExitGames.Client.Photon.LoadBalancing.ClientState.ConnectedToNameServer)
		{
			flag = false;
		}
		if (flag)
		{
			if (PhotonNetwork.isMessageQueueRunning)
			{
				loadBalancingPeer.DispatchIncomingCommands();
			}
			if (PhotonNetwork.isMessageQueueRunning)
			{
				loadBalancingPeer.SendOutgoingCommands();
			}
		}
	}

	protected void OnApplicationPause(bool pause)
	{
		if (PhotonVoiceNetwork.BackgroundTimeout > 0.1f)
		{
			if (timerToStopConnectionInBackground == null)
			{
				timerToStopConnectionInBackground = new Stopwatch();
			}
			timerToStopConnectionInBackground.Reset();
			if (pause)
			{
				timerToStopConnectionInBackground.Start();
			}
			else
			{
				timerToStopConnectionInBackground.Stop();
			}
		}
	}

	protected void OnDestroy()
	{
		StopFallbackSendAckThread();
	}

	protected void OnApplicationQuit()
	{
		StopFallbackSendAckThread();
		PhotonVoiceNetwork.Disconnect();
	}
}
