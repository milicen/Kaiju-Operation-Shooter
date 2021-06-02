using Godot;
using System;

public class Main : Node
{
    [Signal] delegate void ChangeBackground();
    [Signal] delegate void ChangeSpawner(bool isEscorting);
    [Signal] delegate void UpdateRune(int runeCount);
    [Signal] delegate void UpdateProgress(int progress);

    [Signal] delegate void BeamFight(Vector2 globalBeamPoint);

    [Signal] delegate void MechaResetPosition();

    [Signal] delegate void ChangeMusic(string state);

    [Signal] delegate void BossDead();

    // [Signal] delegate void RestartScene();

    Pool pool;
    BGM bgm;

    Player player;
    Mecha mecha;
    Spawner spawner;
    HUD hud;
    Timer voidTimer;
    Timer progressTimer;
    Timer delayTimer;
    Boss boss;

    float progress = 0;
    float progressValue = 16f; // 2f

    bool isEscorting;

    Godot.Collections.Array<Meteorite> meteorites = new Godot.Collections.Array<Meteorite>();

    int runeCount = 0;

    Position2D globalBeamPoint;

    bool bossFight = false;

    Tween oceanTween;
    public override void _Ready()
    {
        GD.Randomize();

        pool = GetNode<Pool>("/root/Pool");

        bgm = GetNode<BGM>("/root/Bgm");
        bgm.Connect("StartMission", this, "_on_StartMission");
        Connect("ChangeMusic", bgm, "_on_ChangeMusic");

        player = GetNode<Player>("Player");
        // player.Connect("PlayerShoot", this, "_on_PlayerShoot");
        player.Connect("PlayerAbyss", this, "_on_PlayerAbyss");
        player.Connect("PlayerEscort", this, "_on_PlayerEscort");
        player.Connect("BumpMeteorite", this, "_on_BumpMeteorite");
        player.Connect("UpdatePlayerHp", this, "_on_UpdatePlayerHp");

        isEscorting = player.GetEscortState();

        mecha = GetNode<Mecha>("Mecha");
        mecha.Connect("Victory", this, "_on_Victory");
        mecha.Connect("UpdateMechaHp", this, "_on_UpdateMechaHp");
        Connect("BeamFight", mecha, "_on_BeamFight");
        Connect("MechaResetPosition", mecha, "_on_MechaResetPosition");

        spawner = GetNode<Spawner>("SpawnerPath/Spawner");
        Connect("ChangeSpawner", spawner, "_on_ChangeSpawner");

        hud = GetNode<HUD>("HUD");
        hud.Connect("RestartScene", this, "_on_RestartScene");
        Connect("UpdateRune", hud, "_on_UpdateRune");
        Connect("UpdateProgress", hud, "_on_UpdateProgress");

        voidTimer = GetNode<Timer>("VoidTimer");
        progressTimer = GetNode<Timer>("ProgressTimer");
        delayTimer = GetNode<Timer>("DelayTimer");

        boss = GetNode<Boss>("Boss");
        boss.Connect("DeadFinished", this, "_on_DeadFinished");
        Connect("BeamFight", boss, "_on_BeamFight");
        Connect("BossDead", boss, "_on_BossDead");

        globalBeamPoint = GetNode<Position2D>("GlobalBeamPoint");

        oceanTween = GetNode<Tween>("Ocean/Tween");
        
        Connect("ChangeBackground", this, "_on_ChangeBackground");
    }

    public override void _Process(float delta)
    {
        if(bossFight) {
            globalBeamPoint.Position += Vector2.Up * 2f * delta; // 2f
        }
    }

    void _on_StartMission() {
        player.StartShootTimer();
        spawner.StartSpawn();
        progressTimer.Start();
    }

    void _on_PlayerShoot(Node _parent, float damage, Vector2 position, float rotation) {
        Shoot(_parent, damage, position, rotation);
        pool.GetCount();
    }

    void _on_PlayerAbyss() {
        EmitSignal("ChangeMusic", "abyss");

        isEscorting = player.GetEscortState();
        EmitSignal("ChangeBackground");
        EmitSignal("ChangeSpawner", isEscorting);

        mecha.Hide();
        mecha.RestoreHp();
        mecha.ResetBeam();
        mecha.GlobalPosition = new Vector2(384, 300);
        boss.ResetBeam();
        bossFight = false;
        boss.GlobalPosition = new Vector2(384, -100);

        globalBeamPoint.GlobalPosition = new Vector2(384, 300);

        // hud.ToggleVisibility(true);
        voidTimer.Start();

        progressValue *= -1;
        
        foreach(Enemy enemy in GetTree().GetNodesInGroup("enemy")) {
            pool.ReturnObject(enemy);
            enemy.GetParent().CallDeferred("remove_child", enemy);
        }

        if(progressTimer.IsStopped()) {
            progressTimer.Start();
        }
    }

    void _on_PlayerEscort() {
        EmitSignal("ChangeMusic", "abyss");

        isEscorting = player.GetEscortState();
        EmitSignal("ChangeBackground");
        EmitSignal("ChangeSpawner", isEscorting);

        mecha.Show();
        // hud.ToggleVisibility(false);

        progressValue *= -1;

        foreach(Meteorite meteorite in GetTree().GetNodesInGroup("meteorite")) {
            pool.ReturnObject(meteorite);
            var parent = meteorite.GetParent();
            parent.CallDeferred("remove_child", meteorite);
        }
    }

    void _on_UpdatePlayerHp(float currentHp) {
        hud.UpdatePlayerHp(currentHp);
    }

    void _on_UpdateMechaHp(float currentHp) {
        hud.UpdateMechaHp(currentHp);
    }

    void _on_BulletOut(Bullet bullet) {
        pool.ReturnObject(bullet);
        var child = GetNode(bullet.Name);
        CallDeferred("remove_child", child);
        // bullet.GetParent().CallDeferred("remove_child", bullet);
        // GD.Print(bullet.GetParent().Name);
    }

    void _on_CrawlerDie(Enemy crawler) {
        pool.ReturnObject(crawler);
        CallDeferred("remove_child", crawler);
    }

    void _on_GunnerDie(EnemyGunner gunner) {
        pool.ReturnObject(gunner);
        CallDeferred("remove_child", gunner);
    }

    void _on_EnemyShoot(Node _parent, float damage, Vector2 position, float rotation) {
        Shoot(_parent, damage, position, rotation);
    }

    void _on_MeteoriteDie(Meteorite meteorite, bool hasRune) {
        if(hasRune) {
            runeCount++;
            EmitSignal("UpdateRune", runeCount);
        }
        pool.ReturnObject(meteorite);
        CallDeferred("remove_child", meteorite);
    }

    void _on_MeteoriteOut(Meteorite meteorite) {
        pool.ReturnObject(meteorite);
        CallDeferred("remove_child", meteorite);
    }

    void Shoot(Node _parent, float damage, Vector2 position, float rotation) {
        var bullet = pool.GetBullet();
        bullet.Init(position, rotation);
        
        bullet.SetParent(_parent);
        bullet.SetDamage(damage);

        AddChild(bullet);
    }

    void _on_VoidTimer_timeout() {
        player.GoEscort();
        isEscorting = player.GetEscortState();
    }

    void _on_ProgressTimer_timeout() {
        progress += progressValue;

        if(progress <= 0) {
            progress = 0;
        }
        else if(progress >= 752) {
            progress = 752;
        }

        if(progress >= 752) {
            BossBattle();
            bossFight = true;
        }
        else {
            bossFight = false;
        }
        EmitSignal("UpdateProgress", progressValue);
    }

    void _on_DelayTimer_timeout() {
        spawner.StartSpawn();
    }

    void _on_BumpMeteorite() {
        // progress -= progressValue;
        // EmitSignal("UpdateProgress", progress);
    }


    void BossBattle() {
        EmitSignal("ChangeMusic", "boss");

        foreach(Enemy enemy in GetTree().GetNodesInGroup("enemy")) {
            pool.ReturnObject(enemy);
            CallDeferred("remove_child", enemy);
        }

        spawner.StopSpawn();
        progressTimer.Stop();

        EmitSignal("BeamFight", globalBeamPoint.GlobalPosition);

        // start delay timer
        delayTimer.Start();
        // await timer signal
        // startspawner

        // player tween position towards mecha
        // player.addchild(mecha)
        // mecha autoshoot directed with mouse

        // boss.startTween
        // move towards position
    }

    void _on_Victory() {
        EmitSignal("ChangeMusic", "victory");
        EmitSignal("BossDead");
        // boss.Hide();
        bossFight = false;
        // boss.ResetBeam();
        mecha.ResetBeam();

        EmitSignal("MechaResetPosition");

        spawner.StopSpawn();
        player.StopShootTimer();

        foreach(Enemy enemy in GetTree().GetNodesInGroup("enemy")) {
            pool.ReturnObject(enemy);
            enemy.GetParent().CallDeferred("remove_child", enemy);
        }

    }

    void _on_DeadFinished() {
        hud.OpenPanel();
    }

    async void _on_ChangeBackground() {
        if(GetNode<Node2D>("Ocean").Visible) {
            var color = new Color(0, 0, 0, 0);
            oceanTween.InterpolateProperty(GetNode<Node2D>("Ocean"), "modulate", GetNode<Node2D>("Ocean").Modulate, color, .5f);
            oceanTween.Start();

            await ToSignal(oceanTween, "tween_completed");

            GetNode<Node2D>("Ocean").Hide();
        }
        else {
            GetNode<Node2D>("Ocean").Show();

            var color = new Color(.55f, .65f, .59f, 1);
            oceanTween.InterpolateProperty(GetNode<Node2D>("Ocean"), "modulate", GetNode<Node2D>("Ocean").Modulate, color, .5f);
            oceanTween.Start();
        }
    }

    void _on_RestartScene() {
        // EmitSignal("RestartScene");
        GetTree().ReloadCurrentScene();
        // pool.ReloadScene();
        hud.ReloadScene();
        bgm.ReloadScene();
    }
}
