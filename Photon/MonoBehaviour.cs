using LightUtility;

namespace Photon
{
	public class MonoBehaviour : BatchUpdateBehaviour
	{
		private PhotonView pvCache;

		public PhotonView photonView
		{
			get
			{
				if (pvCache == null)
				{
					pvCache = PhotonView.Get(this);
				}
				return pvCache;
			}
		}
	}
}
