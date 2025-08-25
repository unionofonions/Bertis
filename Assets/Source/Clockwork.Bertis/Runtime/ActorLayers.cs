namespace Clockwork.Bertis;

public static class ActorLayers
{
    public const int Ground = 6;
    public const int Wall = 7;
    public const int Player = 9;
    public const int Enemy = 10;
    public const int Friend = 11;
    public const int Explosive = 12;
    public const int Puddle = 18;

    public const int GroundMask = 1 << Ground;
    public const int WallMask = 1 << Wall;
    public const int PlayerMask = 1 << Player;
    public const int EnemyMask = 1 << Enemy;
    public const int FriendMask = 1 << Friend;
    public const int ExplosiveMask = 1 << Explosive;
    public const int PuddleMask = 1 << Puddle;
}