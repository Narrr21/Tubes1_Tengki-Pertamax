/*
 * Oportunis adalah Bot yang menggunakan strategi fokus pada energi management dan adaptasi pada segala situasi.
 * Bot ini akan berusaha untuk selalu berada pada posisi aman jika energi rendah dan menyerang musuh yang memiliki energi rendah.
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Oportunis : Bot
{
    private bool lowEnergy = false; // kondisi energi rendah
    private bool chasing = false; // bot mengejar musuh

    Random random = new Random(); // random untuk dodging

    double TargetX; // posisi x musuh
    double TargetY; // posisi y musuh

    double TargetId; // id musuh

    private Dictionary<int, ScannedBotEvent> enemyStates = new Dictionary<int, ScannedBotEvent>(); // map untuk menyimpan data musuh

    static void Main(string[] args)
    {
        new Oportunis().Start();
    }

    Oportunis() : base(BotInfo.FromFile("Oportunis.json")) { }

    public override void Run()
    {
        BodyColor = Color.Gray;

        TargetSpeed = 7; // base speed

        while (IsRunning)
        {
            TurnRadarLeft(360); // scan
            lowEnergy = Energy < 40;
            if (lowEnergy)
            {
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
        TargetSpeed = 8; // lari
        Console.WriteLine("Not Today, Buddy Boy !");
        var (safeX, safeY) = Safe(); // cari posisi aman
        Console.WriteLine($"I am safe here {safeX} {safeY}");
        GoTo(safeX, safeY); // pergi ke posisi aman
        Dodge(); // lakukan dodging
    }

    private void Chase() {
        Console.WriteLine($"Thou shall die {TargetId}");
        TurnBodyToTarget(TargetX, TargetY); // arahkan badan ke musuh
        TargetSpeed = 5; // kejar musuh dengan kecepatan rendah agar lebih akurat jika musuh sering berbelok
    }

    private void Dodge() {
        Console.WriteLine("Ha!, I Raised My Dex Stat to lvl 9999");
        TargetSpeed = 8; // naikkan kecepatan agar lebih mudah menghindar
        // lakukan loop dengan random untuk menambah variasi putaran
        SetTurnRight(90 + (random.NextDouble() * 30)); // putar ke kanan
        WaitFor(new TurnCompleteCondition(this));

        SetTurnRight(90 + (random.NextDouble() * 30)); // putar ke kanan
        WaitFor(new TurnCompleteCondition(this));
    }

    public (double, double) Safe()
    {
        double safeX = X, safeY = Y; // base value posisi saat ini
        
        if (enemyStates.Count == 0) // musuh habis
            return (safeX, safeY);

        double avgX = 0, avgY = 0; // menghitung rata rata posisi semua musuh
        foreach (var enemy in enemyStates.Values)
        {
            avgX += enemy.X;
            avgY += enemy.Y;
        }
        avgX /= enemyStates.Count;
        avgY /= enemyStates.Count;

        double angle = Math.Atan2(Y - avgY, X - avgX);
        double distance = 200; // jarak aman koordinat ke rata rata posisi musuh
        
        // hitung posisi aman
        safeX = X + (Math.Cos(angle) * distance);
        safeY = Y + (Math.Sin(angle) * distance);

        // batasi posisi aman agar tidak keluar arena
        safeX = Math.Max(50, Math.Min(ArenaWidth - 50, safeX));
        safeY = Math.Max(50, Math.Min(ArenaHeight - 50, safeY));

        return (safeX, safeY);
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        Console.WriteLine($"Found Them! {e.ScannedBotId} at {e.X}, {e.Y}");

        enemyStates[e.ScannedBotId] = e; // simpan data musuh
        double BearingRadar = NormalizeRelativeAngle(DirectionTo(e.X, e.Y) - RadarDirection); // hitung sudut antara radar dan musuh
        if (lowEnergy) {
            TurnGunToTarget(e.X, e.Y); // arahkan senjata ke musuh
            Fire(1);
            if (BearingRadar < 3) { // kalau musuh dekat tembak serangan kuat dan scan lagi
                firePower(distance(e.X, e.Y));
                Rescan();
            }
        } else {
            if (chasing) {
                if (TargetId == e.ScannedBotId) {
                    chasing = true;
                } else { // jika bukan musuh yang ditarget skip.
                    return;
                }
            } else {
                chasing = e.Energy < 40;
                TargetX = e.X; // simpan posisi musuh
                TargetY = e.Y; // simpan posisi musuh
                TargetId = e.ScannedBotId; // simpan id musuh target
            }
            
            if (BearingRadar < 3) { // kalau musuh dekat scan lagi
                Rescan();
            }
            TurnGunToTarget(e.X, e.Y); // arahkan senjata ke musuh
            if (e.Speed < 3) {
                firePower(distance(e.X, e.Y)); // tembak dengan kekuatan lebih jika musuh tidak bergerak / bergerak lambat
            } else {
                Fire(1); // tembak dengan kekuatan biasa
            }
        }
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        Console.WriteLine($"Bot {e.VictimId} has been returned to the void");

        enemyStates.Remove(e.VictimId); // hapus data musuh yang mati
        if (e.VictimId == TargetId) {
            chasing = false;
            Console.WriteLine("Gotcha! gotta kill them all");
        }
    }

    public void firePower(double dist)
    {
        double power = Math.Max(1, Math.Min(3, 500 / dist)); // hitung kekuatan tembakan antara 1-3 berdasarkan jarak
        Fire(power);
    }

    public double distance(double toX, double toY) {
        double dx = toX - X;
        double dy = toY - Y;
        double distance = Math.Sqrt(dx * dx + dy * dy); // hitung jarak antara bot dan target
        return distance;
    }

    public override void OnHitBot(HitBotEvent e)
    {
        TurnGunToTarget(e.X, e.Y);
        Fire(3); // jika tertaan tembak dengan kuat
        Console.WriteLine($"Get out of my way {e.VictimId}");
    }

    public override void OnHitWall(HitWallEvent e)
    {
        TargetSpeed *= -1; // putar balik
        Console.WriteLine("Breaking wall ? are we titan now ?");
    }

    private void GoTo(double x, double y)
    {
        double dx = x - X;
        double dy = y - Y;
        double angle = Math.Atan2(dy, dx) * (180.0 / Math.PI);
        SetTurnLeft(NormalizeBearing(angle - Direction)); // arahkan bot ke posisi tujuan
        SetForward(distance(x, y)); // maju ke posisi tujuan
    }

    private double NormalizeBearing(double angle)
    {
        // jaga di range -180 hingga 180
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }

    public void TurnGunToTarget(double x, double y) {
        var bearing = NormalizeRelativeAngle(DirectionTo(x, y) - GunDirection);
        TurnGunLeft(bearing);
        // Console.WriteLine($"My Gun to {GunDirection} My Radar to {RadarDirection}");
        AlignRadar();
        // Console.WriteLine($"My Gun to {GunDirection} My Radar to {RadarDirection}");
    }

    public void AlignRadar()
    {
        double radarTurn = GunDirection - RadarDirection;
        TurnRadarLeft(NormalizeBearing(radarTurn));
        // Console.WriteLine($"Align Radar to {RadarDirection}");
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