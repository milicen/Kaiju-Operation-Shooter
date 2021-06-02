using Godot;
using System;
using System.Collections.Generic;

public class Pool : Node
{

    [Export] PackedScene  enemyCrawlerScene;
    [Export] PackedScene  enemyGunnerScene;
    [Export] PackedScene  bulletScene;
    [Export] PackedScene meteoriteScene;


    [Export] int maxEnemy = 100;
    [Export] int maxBullet = 200;
    [Export] int maxMeteorite = 100;


    Queue<Enemy> enemyCrawlerPool = new Queue<Enemy>();

    Queue<EnemyGunner> enemyGunnerPool = new Queue<EnemyGunner>();

    Queue<Bullet> bulletPool = new Queue<Bullet>();
    Queue<Meteorite> meteoritePool = new Queue<Meteorite>();

    Main main;
    
    public override void _Ready()
    {
        main = GetNode<Main>("/root/Main");


        for(int i = 0; i < maxEnemy; i++) {
            var enemyCrawler = (Enemy) enemyCrawlerScene.Instance();
            enemyCrawler.Connect("CrawlerDie", main, "_on_CrawlerDie");
            enemyCrawlerPool.Enqueue(enemyCrawler);

            var enemyGunner = (EnemyGunner) enemyGunnerScene.Instance();
            enemyGunner.Connect("EnemyShoot", main, "_on_EnemyShoot");
            enemyGunner.Connect("GunnerDie", main, "_on_GunnerDie");
            enemyGunnerPool.Enqueue(enemyGunner);
        }

        // for(int i = 0; i < maxBullet; i++) {
        //     var bullet = (Bullet) bulletScene.Instance();
        //     bullet.Connect("BulletOut", main, "_on_BulletOut");
        //     bulletPool.Enqueue(bullet);
        // }

        for(int i = 0; i < maxMeteorite; i++) {
            var meteorite = (Meteorite) meteoriteScene.Instance();
            meteorite.Connect("MeteoriteDie", main, "_on_MeteoriteDie");
            meteorite.Connect("MeteoriteOut", main, "_on_MeteoriteOut");
            meteoritePool.Enqueue(meteorite);
        }

        GD.Print("crawler : " + enemyCrawlerPool.Count);
        GD.Print("gunner : " + enemyGunnerPool.Count);
        GD.Print("bullet : " + bulletPool.Count);
        GD.Print("meteorite : " + meteoritePool.Count);
    }

    public Enemy GetCrawlerEnemy() {
        return enemyCrawlerPool.Dequeue();
    }

    public EnemyGunner GetGunnerEnemy() {
        return enemyGunnerPool.Dequeue();

    }

    public Bullet GetBullet() {
        return bulletPool.Dequeue();
    }

    public Meteorite GetMeteorite() {
        return meteoritePool.Dequeue();
    }

    public void ReturnObject(Node actor) {
        if(actor.IsInGroup("enemy-crawler")) {
            var crawler = actor as Enemy;
            // crawler.GetParent().RemoveChild(crawler);
            enemyCrawlerPool.Enqueue(crawler);
            
        }
        else if(actor.IsInGroup("enemy-gunner")) {
            var gunner = actor as EnemyGunner;
            // gunner.GetParent().RemoveChild(gunner);
            enemyGunnerPool.Enqueue(gunner);
        }
        // else if(actor.IsInGroup("bullet")) {
        //     var bullet = actor as Bullet;
        //     // bullet.GetParent().RemoveChild(bullet);
        //     bulletPool.Enqueue(bullet);
        // }
        else if(actor.IsInGroup("meteorite")) {
            var meteorite = actor as Meteorite;
            // meteorite.GetParent().RemoveChild(meteorite);
            meteoritePool.Enqueue(meteorite);
        }
    }

    public void GetCount() {
        GD.Print("crawler : " + enemyCrawlerPool.Count);
        GD.Print("gunner : " + enemyGunnerPool.Count);
        GD.Print("bullet : " + bulletPool.Count);
        GD.Print("meteorite : " + meteoritePool.Count);
    }

    // public void ReloadScene() {
    //     GetTree().ReloadCurrentScene();
    // }
}
