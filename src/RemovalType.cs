namespace BotTerminator.Models
{
	public enum RemovalType
	{
		None   = 0,
		Remove = 1 << 0,
		Spam   = 1 << 1,
	}
}
