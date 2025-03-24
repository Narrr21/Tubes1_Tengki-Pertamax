/*
 * PersonalBot adalah Bot yang menggunakan strategi fokus pada menyerang. PersonalBot akan menyimpan ID Bot yang terdeteksi pertama kali.
 * Bot tersebut menjadi target dan akan diincar terus sampai bot tersebut hancur. 
*/



using System;
using System.Drawing;
using Microsoft.VisualBasic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class PersonalBot : Bot
{   
    int targetBotId = -1;
    bool movingForward;
    long lastScannedTick = 0; //Tick digunakan untuk menghitungg waktu terakhir bot terdeteksi
    long currentTick = 0; //Tick digunakan untuk menghitung waktu sekarang

    static void Main(string[] args)
    {
        new PersonalBot().Start();
    }

    PersonalBot() : base(BotInfo.FromFile("PersonalBot.json")) { }

    public override void Run()
    {
        BodyColor = Color.Red;
        movingForward = true;

        // Console.WriteLine("Targetting bot with id: " + targetBotId);

        while (IsRunning)
        {
            currentTick++;
            Console.WriteLine("Current tick: " + currentTick + "\n");
            SetTurnGunRight(double.PositiveInfinity);
            SetForward(40000);
            SetTurnRight(60); 
            WaitFor(new TurnCompleteCondition(this));

            SetTurnLeft(60);
            WaitFor(new TurnCompleteCondition(this));

            if(targetBotId != -1 && currentTick - lastScannedTick > 3) { //Jika bot tidak terdeteksi dalam 3 tick, maka bot akan mencari target baru
                Console.WriteLine("Lost target: " + targetBotId);
                targetBotId = -1;
            }
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {   

        if (targetBotId == -1) {
            targetBotId = e.ScannedBotId;
            Console.WriteLine("Locked on target: " + targetBotId); //Jika belum ada target, maka bot akan menyimpan ID bot yang terdeteksi pertama kali
        }

        if (e.ScannedBotId == targetBotId) {
            lastScannedTick = currentTick;
            Console.WriteLine("Scanned Tick: " + lastScannedTick + "\n"); //Menyimpan waktu terakhir bot terdeteksi
            TurnToFaceTarget(e.X, e.Y);
        
            var distance = DistanceTo(e.X, e.Y);

            if (distance > 100) { //Menjaga jarak dengan bot target
            SetForward(50);
            } else {
            SetBack(30); 
            }

        var bearingFromGun = GunBearingTo(e.X, e.Y);
        TurnGunLeft(bearingFromGun);

        if (Math.Abs(bearingFromGun) <= 2) Fire(2);
        if (bearingFromGun == 0) Rescan();
    }

    
    }

    // public override void OnBulletHit(BulletHitBotEvent e)
    // {
    //     int hitId = e.VictimId;
    //     if (targetBotId == -1){
    //         targetBotId = hitId;
    //         Console.WriteLine("Targetting Bot with id: " + targetBotId + "\n");
    //     }
    //     Console.WriteLine("I hit bot with id: " + hitId + "\n");
    // }

    // public override void OnBotDeath(BotDeathEvent e)
    // {
    //     if (e.VictimId == targetBotId){
    //         Console.WriteLine("Lost target: " + targetBotId);
    //         targetBotId = -1;
    //         SetForward(100);  
    //     }
    // }

    public override void OnHitWall(HitWallEvent e)
    {
        ReverseDirection();
        SetTurnLeft(45);
    }

    public override void OnHitBot(HitBotEvent e)
    {
        if (e.IsRammed || !e.IsRammed)
        {
            ReverseDirection();
            SetTurnLeft(45);
        }
    }

    private void ReverseDirection()
    {
        if (movingForward)
        {
            SetBack(100);
            movingForward = false;
        }
        else
        {
            SetForward(100);
            movingForward = true;
        }
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