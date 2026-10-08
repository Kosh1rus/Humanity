using Robust.Shared.Player;
namespace Content.Server.GameTicking;
public sealed partial class ServerGameTicker
{
    public void ReturnToLobby(ICommonSession session) => PlayerJoinLobby(session);
}
