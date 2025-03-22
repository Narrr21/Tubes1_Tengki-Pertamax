using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class MoveTrack : Bot
{
    bool movingForward;

    int turnDirection = 1;
    Random random = new Random();

    static void Main()
    {
        new MoveTrack().Start();
    }

    MoveTrack() : base(BotInfo.FromFile("MoveTrack.json")) { }

    public override void Run()
    {
        BodyColor = Color.FromArgb(0xDD, 0xDD, 0xDD);   // Light gray
        TurretColor = Color.FromArgb(0xFF, 0xFF, 0xFF); // Pure white turret
        RadarColor = Color.FromArgb(0xFF, 0xFF, 0xFF);  // Pure white radar
        BulletColor = Color.FromArgb(0x33, 0x33, 0x33); // Almost black bullets
        ScanColor = Color.FromArgb(0x00, 0x00, 0x00);   // Pure black scan

        movingForward = true;

        while (IsRunning)
        {
            SetTurnGunRight(Double.PositiveInfinity);
            SetForward(40000);
            SetTurnRight(60); 
            WaitFor(new TurnCompleteCondition(this));

            SetTurnLeft(60);
            WaitFor(new TurnCompleteCondition(this));
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {

        TurnToFaceTarget(e.X, e.Y);

        // Calculate direction of the scanned bot and bearing to it for the gun
        var bearingFromGun = GunBearingTo(e.X, e.Y);

        // Turn the gun toward the scanned bot
        TurnGunLeft(bearingFromGun);

        // If it is close enough, fire!
        if (Math.Abs(bearingFromGun) <= 3)
            Fire(2);
        if (bearingFromGun == 0)
            Rescan();
    }

    public override void OnHitWall(HitWallEvent e)
    {
        ReverseDirection();
    }

    public override void OnHitBot(HitBotEvent e)
    {
        if (e.IsRammed)
        {
            ReverseDirection();
        }
    }

    public void ReverseDirection()
    {
        if (movingForward)
        {
            SetBack(200);
            movingForward = false;
        }
        else
        {
            SetForward(200);
            movingForward = true;
        }
    }
    
    private void TurnToFaceTarget(double x, double y)
    {
        var bearing = BearingTo(x, y);
        if (bearing >= 0)
            turnDirection = 1;
        else
            turnDirection = -1;

        TurnLeft(bearing);
    }
}

public class TurnCompleteCondition : Condition
{
    private readonly Bot bot;

    public TurnCompleteCondition(Bot bot)
    {
        this.bot = bot;
    }

    public override bool Test()
    {
        return bot.TurnRemaining == 0;
    }
}