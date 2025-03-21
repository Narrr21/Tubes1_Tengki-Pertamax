using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Dodge : Bot
{
    bool movingForward;
    Random random = new Random();

    static void Main()
    {
        new Dodge().Start();
    }

    Dodge() : base(BotInfo.FromFile("Dodge.json")) { }

    public override void Run()
    {
        BodyColor = Color.Blue;
        TurretColor = Color.Blue;
        RadarColor = Color.Black;
        ScanColor = Color.Yellow;

        movingForward = true;
        SetTurnGunRight(Double.PositiveInfinity);

        while (IsRunning)
        {
            SetForward(40000);
            SetTurnRight(60);
            WaitFor(new TurnCompleteCondition(this));

            SetTurnLeft(90 + (random.NextDouble() * 30));
            WaitFor(new TurnCompleteCondition(this));

            SetTurnRight(60 + (random.NextDouble() * 30)); 
            WaitFor(new TurnCompleteCondition(this));
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        double dx = e.X - X;
        double dy = e.Y - Y;
        double distance = Math.Sqrt(dx * dx + dy * dy);

        double firePower = distance > 100 ? 1 : 3;
        Fire(firePower);
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
            SetBack(150);
            movingForward = false;
        }
        else
        {
            SetForward(150);
            movingForward = true;
        }
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