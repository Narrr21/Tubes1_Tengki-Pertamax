/* 
*    Bot ini menggunakan strategi greedy sederhana yang mengutamakan survivability. Bot akan pergi ke tengah arena ketika ronde dimulai
*    dan melakukan perputaran. Bot akan menembak langsung ketika ada bot yang terdeteksi.
*/


using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Claude : Bot
{
    private Random random = new Random();

    private bool lowEnergy = false;

    static void Main(string[] args)
    {
        new Claude().Start();
    }

    Claude() : base(BotInfo.FromFile("Claude.json")) { }

    public override void Run()
    {
        BodyColor = Color.LightBlue;
        TurretColor = Color.Blue;
        RadarColor = Color.DarkBlue;
        ScanColor = Color.Aqua;
        BulletColor = Color.DeepSkyBlue;

        random = new Random();

        GoToCenter();
        while (IsRunning)
        {
            TargetSpeed = 3;
            TurnRate = 20;
            TurnGunLeft(double.PositiveInfinity);
            WaitFor(new TurnCompleteCondition(this));
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        Fire(1);
    }

    private void GoToCenter()  
    {
        Console.WriteLine("Going to center");
        double centerX = ArenaWidth / 2;
        double centerY = ArenaHeight / 2;

        TurnToFaceTarget(centerX, centerY);
        Forward(DistanceTo(centerX, centerY));
        WaitFor(new TurnCompleteCondition(this));
    }

    private void TurnToFaceTarget(double x, double y)
    {
        var bearing = BearingTo(x, y);
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