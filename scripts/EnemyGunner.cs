using Godot;
using System;

public class EnemyGunner : Enemy
{
    [Signal] delegate void GunnerDie(EnemyGunner gunner);
    [Signal] delegate void EnemyShoot(Node _parent, float damage, Vector2 position, float rotation);

    [Export] PackedScene bulletScene;

    Position2D bulletPos;

    Timer shootTimer;

    public float waitTime = .3f;

    float minDistance = 260f;

    AnimationPlayer anim;
    
    public override void _Ready()
    {
        target = GetNode<Player>("/root/Main/Player").GlobalPosition;

        currentMoveSpeed = movementSpeed;

        direction = target - GlobalPosition;

        body = GetNode<CollisionShape2D>("Body");

        hpBar = GetNode<TextureProgress>("HpBar");

        currentHp = maxHp;

        hpBar.MaxValue = maxHp;
        hpBar.Value = currentHp;

        direction = target - GlobalPosition;

        bulletPos = GetNode<Position2D>("Body/Position2D");

        shootTimer = GetNode<Timer>("ShootTimer");
        // shootTimer.WaitTime = waitTime;
        shootTimer.Start();
        // Init(Vector2.Zero);

        anim = GetNode<AnimationPlayer>("Body/Charger/AnimationPlayer");

    }

    public override void Init(Vector2 position) {
        base.Init(position);

        direction = target - Position;

        // bulletPos = GetNode<Position2D>("Body/Position2D");

        // shootTimer = GetNode<Timer>("ShootTimer");
        try
        {
            if(IsInstanceValid(shootTimer)) {
                shootTimer.WaitTime = waitTime;
            }

            if(IsInstanceValid(anim)) {
                anim.Play("Idle");
            }
        }
        catch (System.Exception)
        {
            
            throw;
        }
        // shootTimer.Start();

    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if(GetNode<Player>("/root/Main/Player") == null) {
            target = Vector2.Zero;
        }
        else {
            target = GetNode<Player>("/root/Main/Player").GlobalPosition;
        }

        direction = target - GlobalPosition;

        if(direction.Length() <= minDistance) {
            inRange = true;
        }
        else {
            inRange = false;
        }

        if(currentHp <= 0) {
            EmitSignal("GunnerDie", this);
            anim.Play("Crash");
        }
    }

    void _on_ShootTimer_timeout() {
        // EmitSignal("EnemyShoot", this, initialDmg, bulletPos.GlobalPosition, body.GlobalRotation);

        var bullet = (Bullet) bulletScene.Instance();

        bullet.Init(bulletPos.GlobalPosition, body.GlobalRotation);

        bullet.SetParent(this);
        bullet.SetDamage(initialDmg);

        // bullet.Position = bulletPos.GlobalPosition;
        // bullet.Rotation = body.GlobalRotation;

        GetParent().AddChild(bullet);
    }
}
