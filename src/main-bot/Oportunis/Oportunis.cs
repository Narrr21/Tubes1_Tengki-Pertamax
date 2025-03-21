using System;
using System.Collections.Generic;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Oportunis : Bot
{
    private bool lowEnergy = false;
    private bool chasing = false;
    private bool kamikaze = false;

    Random random = new Random();

    double TargetX;
    double TargetY;

    private Dictionary<int, ScannedBotEvent> enemyStates = new Dictionary<int, ScannedBotEvent>();

    static void Main(string[] args)
    {
        new Oportunis().Start();
    }

    Oportunis() : base(BotInfo.FromFile("Oportunis.json")) { }

    public override void Run()
    {
        BodyColor = Color.Gray;

        TargetSpeed = 7;

        while (IsRunning)
        {
            TurnRadarLeft(360);
            lowEnergy = Energy < 40;
            if (lowEnergy)
            {
                kamikaze = Energy < 20 && chasing;
                Escape();
            }
            else
            {
                if (chasing)
                    Chase();
                else
                    Dodge();
            }
        }
    }

    private void Escape()
    {
        TargetSpeed = 6;
        Console.WriteLine("I am escaping");
        var (safeX, safeY) = Safe();
        Console.WriteLine($"Run to {safeX} {safeY}");
        GoTo(safeX, safeY);
    }

    private void Chase() {
        Console.WriteLine($"I am chasing {TargetX} {TargetY}");
        TurnBodyToTarget(TargetX, TargetY);
        TargetSpeed = 5;
    }

    private void Dodge() {
        Console.WriteLine("I am dodging");
        TargetSpeed = 5;
        SetTurnRight(60);
        WaitFor(new TurnCompleteCondition(this));

        SetTurnRight(90 + (random.NextDouble() * 30));
        WaitFor(new TurnCompleteCondition(this));

        SetTurnRight(60 + (random.NextDouble() * 30)); 
        WaitFor(new TurnCompleteCondition(this));
    }

    public (double, double) Safe()
    {
        double safeX = X, safeY = Y;
        
        if (enemyStates.Count == 0)
            return (safeX, safeY);

        double avgX = 0, avgY = 0;
        foreach (var enemy in enemyStates.Values)
        {
            avgX += enemy.X;
            avgY += enemy.Y;
        }
        avgX /= enemyStates.Count;
        avgY /= enemyStates.Count;

        double angle = Math.Atan2(Y - avgY, X - avgX);
        double distance = 200;
        
        safeX = X + (Math.Cos(angle) * distance);
        safeY = Y + (Math.Sin(angle) * distance);

        safeX = Math.Max(50, Math.Min(ArenaWidth - 50, safeX));
        safeY = Math.Max(50, Math.Min(ArenaHeight - 50, safeY));

        return (safeX, safeY);
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        Console.WriteLine($"Scanned bot {e.ScannedBotId} in {e.X} {e.Y}");
        enemyStates[e.ScannedBotId] = e;
        if (lowEnergy) {
            kamikaze = e.Energy < 20;
            banzai();
        } else {
            chasing = e.Energy < 20;
            if (chasing) {
                TargetX = e.X;
                TargetY = e.Y;
            }
            TurnGunToTarget(e.X, e.Y);
            if (e.Speed < 3) {
                firePower(distance(e.X, e.Y));
            } else {
                Fire(0.1);
            }
        }
    }

    public void firePower(double dist)
    {
        double power = Math.Max(1, Math.Min(3, 500 / dist));
        Fire(power);
    }

    public void banzai() {

    }

    public double distance(double toX, double toY) {
        double dx = toX - X;
        double dy = toY - Y;
        double distance = Math.Sqrt(dx * dx + dy * dy);
        return distance;
    }

    public override void OnHitBot(HitBotEvent e)
    {
        Console.WriteLine($"Ouch! I hit a bot at {e.X}, {e.Y}");
    }

    public override void OnHitWall(HitWallEvent e)
    {
        Console.WriteLine("Ouch! I hit a wall, must turn back!");
    }

    private void GoTo(double x, double y)
    {
        double dx = x - X;
        double dy = y - Y;
        double angle = Math.Atan2(dy, dx) * (180.0 / Math.PI);
        SetTurnLeft(NormalizeBearing(angle - Direction));
        SetForward(100);
    }

    private double NormalizeBearing(double angle)
    {
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }

    public void TurnGunToTarget(double x, double y) {
        var bearing = NormalizeRelativeAngle(DirectionTo(x, y) - GunDirection);
        TurnGunLeft(bearing);
        Console.WriteLine($"My Gun to {GunDirection} My Radar to {RadarDirection}");
        AlignRadar();
        Console.WriteLine($"My Gun to {GunDirection} My Radar to {RadarDirection}");
    }

    public void AlignRadar()
    {
        double radarTurn = GunDirection - RadarDirection;
        while (radarTurn > 180) radarTurn -= 360;
        while (radarTurn < -180) radarTurn += 360;
        TurnRadarLeft(radarTurn);
        Console.WriteLine($"Align Radar to {RadarDirection}");
    }

    public void TurnBodyToTarget(double x, double y) {
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