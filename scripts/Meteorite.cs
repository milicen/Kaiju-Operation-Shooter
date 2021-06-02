using Godot;
using System;

public class Meteorite : KinematicBody2D
{

    [Signal] delegate void MeteoriteDie(Meteorite meteorite, bool hasRune);
    [Signal] delegate void MeteoriteOut(Meteorite meteorite);
    
    [Export] float minScale = .5f;
    [Export] float maxScale = 1.5f;

    CollisionShape2D body;

    Position2D centerPoint;

    Vector2 direction;

    [Export] float minSpeed = 50f;
    [Export] float maxSpeed = 100f;
    float speed;

    bool hasRune;

    TextureProgress hpBar;
    float currentHp;


    public override void _Ready()
    {
        // scaling object
        body = GetNode<CollisionShape2D>("Body");

        var randScale = (float) GD.RandRange(minScale, maxScale);

        body.Scale = new Vector2(randScale, randScale);

        // rotating object
        centerPoint = GetNode<Position2D>("/root/Main/CenterPoint");

        body.LookAt(centerPoint.GlobalPosition);

        // // set move direction and speed
        var angle = Mathf.Deg2Rad(body.RotationDegrees + (float) GD.RandRange(-30f, 30f));

        direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        speed = (float) GD.RandRange(minSpeed, maxSpeed);

        // initialize hp
        hpBar = GetNode<TextureProgress>("HpBar");

        // Init(Vector2.Zero);

        currentHp = body.Scale.x * 10f;

        hpBar.MaxValue = currentHp;
        hpBar.Value = currentHp;

        // set hasRune
        var rand = (float) GD.RandRange(0, 100f);

        if(rand <= 40f) {
            hasRune = false;
        }
        else {
            hasRune = true;
        }

        GD.Print(hasRune);
    }

    public void Init(Vector2 position) {

        Position = position;

        // set scale
        var randScale = (float) GD.RandRange(minScale, maxScale);

        try
        {
            if(IsInstanceValid(body)) {
                body.Scale = new Vector2(randScale, randScale);
                body.LookAt(centerPoint.GlobalPosition);
                
                // set direction and speed
                var angle = Mathf.Deg2Rad(body.RotationDegrees + (float) GD.RandRange(-30f, 30f));

                direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                
                // set hp
                currentHp = body.Scale.x * 10f;
            }
        }
        catch (System.Exception)
        {
            
            throw;
        }



        speed = (float) GD.RandRange(minSpeed, maxSpeed);


        try
        {
            if(IsInstanceValid(hpBar)) {
                hpBar.MaxValue = currentHp;
                hpBar.Value = currentHp;
            }
        }
        catch (System.Exception)
        {
            
            throw;
        }

        // set rune
        var rand = (float) GD.RandRange(0, 100f);

        if(rand <= 51f) {
            hasRune = false;
        }
        else {
            hasRune = true;
        }
    }

    public override void _Process(float delta) {
        Position += direction.Normalized() * speed * delta;

        if(currentHp <= 0) {
            // if has rune, emit signal to main
            EmitSignal("MeteoriteDie", this, hasRune);
            // QueueFree();
            // main add rune point to player
        }
    }

    void _on_VisibilityNotifier2D_viewport_exited(Viewport vp) {
        // QueueFree();
        EmitSignal("MeteoriteOut", this);
    }

    public void TakeDamage(float damage) {
        currentHp -= damage;
        hpBar.Value = currentHp;
    }

}
