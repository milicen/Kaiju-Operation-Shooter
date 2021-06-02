using Godot;
using System;

public class Spawner : PathFollow2D
{

    Pool pool;

    // [Signal] delegate void SpawnCrawler(Vector2 position);
    // [Signal] delegate void SpawnGunner(Vector2 position);
    // [Signal] delegate void SpawnMeteorite(Vector2 position);

    [Export] PackedScene crawlerScene;
    [Export] PackedScene gunnerScene;
    [Export] PackedScene meteoriteScene;

    Timer spawnTimer;
    public float enemyWaitTime = 3f;
    public float meteoriteWaitTime = 1f;

    Enemy enemyToSpawn;
    Meteorite meteorToSpawn;

    float speed = 300f;

    bool isEscorting = true;

    public override void _Ready()
    {
        pool = GetNode<Pool>("/root/Pool");


        GD.Randomize();
        var randOffset = (float) GD.RandRange(0, 3000);
        Offset = randOffset;

        spawnTimer = GetNode<Timer>("SpawnTimer");
        spawnTimer.WaitTime = enemyWaitTime;
    }

    public override void _Process(float delta) {
        Offset += speed * delta;

        if(isEscorting) {
            spawnTimer.WaitTime = enemyWaitTime;
        }
        else {
            spawnTimer.WaitTime = meteoriteWaitTime;
        }
    }

    void _on_SpawnTimer_timeout() {
        if(isEscorting) {
            SpawnEnemy();
        }
        else {
            SpawnMeteor();
        }
    }

    void SpawnEnemy() {
        // get random number
        var randNum = (float) GD.RandRange(0, 100f);
        // if number is between 0-60, instance crawler
        if(randNum <= 60) {
            enemyToSpawn = pool.GetCrawlerEnemy();
        }
        // else, instance gunner
        else {
            enemyToSpawn = pool.GetGunnerEnemy();
        }

        enemyToSpawn.Init(GlobalPosition);
        GetNode<Main>("/root/Main").AddChild(enemyToSpawn);
    }

    void SpawnMeteor() {
        meteorToSpawn = pool.GetMeteorite();
        meteorToSpawn.Init(GlobalPosition);
        GetNode<Main>("/root/Main").AddChild(meteorToSpawn);
        GD.Print(meteorToSpawn.GetParent().Name);
    }

    void _on_ChangeSpawner(bool _isEscorting) {
        isEscorting = _isEscorting;
    }

    public void StopSpawn(){
        spawnTimer.Stop();
    }

    public void StartSpawn() {
        spawnTimer.Start();
    }
}
