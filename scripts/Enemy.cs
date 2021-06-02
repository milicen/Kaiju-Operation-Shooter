using Godot;
using System;

public class Enemy : KinematicBody2D
{

    [Signal] delegate void CrawlerDie(Enemy crawler);

    public Vector2 velocity;
    [Export] public float movementSpeed = 10f;
    public float currentMoveSpeed;
    public Vector2 direction;
    public Vector2 target;

    public CollisionShape2D body;

    public TextureProgress hpBar;
    public float maxHp = 18;
    public float currentHp = 0;

    public bool inRange = false;


    public Timer leechTimer;
    public float leechWaitTime = 1.5f;

    Mecha mecha;

    [Export] public float initialDmg = 2;

    AnimationPlayer anim;

    public override void _Ready()
    {
        target = GetNode<Mecha>("/root/Main/Mecha").GlobalPosition;

        currentMoveSpeed = movementSpeed;

        direction = target - GlobalPosition;

        body = GetNode<CollisionShape2D>("Body");

        hpBar = GetNode<TextureProgress>("HpBar");

        currentHp = maxHp;

        hpBar.MaxValue = maxHp;
        hpBar.Value = currentHp;

        leechTimer = GetNode<Timer>("LeechTimer");
        leechTimer.WaitTime = leechWaitTime;

        anim = GetNode<AnimationPlayer>("Body/Drainer/AnimationPlayer");
    }

    public virtual void Init(Vector2 position) {
        Position = position;

        currentMoveSpeed = movementSpeed;

        currentHp = maxHp;

        
        try
        {
            if(IsInstanceValid(hpBar)) {
                hpBar.MaxValue = maxHp;
                hpBar.Value = currentHp;
            }

            if(IsInstanceValid(leechTimer)) {
                leechTimer.WaitTime = leechWaitTime;
            }

            if(IsInstanceValid(anim)) {
                anim.Play("Idle");
            }
        }
        catch (System.Exception error)
        {
            GD.Print(error.Message);            
            throw;
        }


    }

    public override void _Process(float delta) {
        body.LookAt(target);
    }

    public override void _PhysicsProcess(float delta)
    {
        if(!inRange) {
            MoveHandler(delta);
        } else {
            velocity = Vector2.Zero;
        }

        if(currentHp <= 0) {
            EmitSignal("CrawlerDie", this);
        }
    }

    void MoveHandler(float delta) {
        velocity = direction * currentMoveSpeed;

        velocity = MoveAndSlide(velocity * delta);
    }

    public void StopMoving() {
        currentMoveSpeed = 0;
    }

    public void TakeDamage(float damage) {
        currentHp -= damage;
        hpBar.Value = currentHp;

        GD.Print(currentHp);
    }

    public void StartLeeching(Mecha _mecha) {
        mecha = _mecha;
        // start leech timer
        leechTimer.Start();
        anim.Play("Drain");
        // at timeout mecha.takedamage
    }

    void _on_LeechTimer_timeout() {
        mecha.TakeDamage(initialDmg);
    }
    
}
