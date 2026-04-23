namespace HermesProxy.World.Objects.Version.V3_4_3_54261;

public struct CreateObjectBits
{
	public bool NoBirthAnim;

	public bool EnablePortals;

	public bool PlayHoverAnim;

	public bool MovementUpdate;

	public bool MovementTransport;

	public bool Stationary;

	public bool CombatVictim;

	public bool ServerTime;

	public bool Vehicle;

	public bool AnimKit;

	public bool Rotation;

	public bool AreaTrigger;

	public bool GameObject;

	public bool SmoothPhasing;

	public bool ThisIsYou;

	public bool SceneObject;

	public bool ActivePlayer;

	public bool Conversation;

	public void Clear()
	{
		this.NoBirthAnim = false;
		this.EnablePortals = false;
		this.PlayHoverAnim = false;
		this.MovementUpdate = false;
		this.MovementTransport = false;
		this.Stationary = false;
		this.CombatVictim = false;
		this.ServerTime = false;
		this.Vehicle = false;
		this.AnimKit = false;
		this.Rotation = false;
		this.AreaTrigger = false;
		this.GameObject = false;
		this.SmoothPhasing = false;
		this.ThisIsYou = false;
		this.SceneObject = false;
		this.ActivePlayer = false;
		this.Conversation = false;
	}
}
