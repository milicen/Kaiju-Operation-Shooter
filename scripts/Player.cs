using Godot;
using System;

public class Player : KinematicBody2D
{    
    [Signal] delegate void UpdatePlayerHp(float currentHp);

    [Signal] delegate void PlayerShoot(Node _parent, float damage, Vector2 position, float rotation);
    [Signal] delegate void BumpMeteorite();

    [Signal] delegate void PlayerAbyss();
    [Signal] delegate void PlayerEscort();

    public Vector2 velocity;
    [Export] public float movementSpeed = 300f;
    [Export] PackedScene bulletScene;

    CollisionShape2D body;

    Position2D bulletPos;

    Timer shootTimer;
    public float waitTime = .3f;

    // TextureProgress hpBar;
    float maxHp = 20;
    float currentHp;
    float healValue = 2;

    AnimationPlayer anim;

    [Export] float initialDmg = 2;

    bool isEscorting = true;

    Godot.Collections.Array<AudioStreamPlayer> shootSFXs = new Godot.Collections.Array<AudioStreamPlayer>();
    Godot.Collections.Array<AudioStreamPlayer> bumpSFXs = new Godot.Collections.Array<AudioStreamPlayer>();

    public override void _Ready()
    {
        body = GetNode<CollisionShape2D>("Body");

        bulletPos = GetNode<Position2D>("Body/Position2D");
        
        shootTimer = GetNode<Timer>("ShootTimer");
        shootTimer.WaitTime = waitTime;
        // shootTimer.Start();

        // hpBar = GetNode<TextureProgress>("HpBar");

        currentHp = maxHp;

        // hpBar.MaxValue = maxHp;
        // hpBar.Value = currentHp;

        anim = GetNode<AnimationPlayer>("Body/Plane/AnimationPlayer");

        foreach(Node node in GetTree().GetNodesInGroup("shoot-sfx")) {
            var audio = node as AudioStreamPlayer;
            shootSFXs.Add(audio);
        }

        foreach(Node node in GetTree().GetNodesInGroup("bump-sfx")) {
            var audio = node as AudioStreamPlayer;
            bumpSFXs.Add(audio);
        }
    }

    public override void _Process(float delta) {
        body.LookAt(GetGlobalMousePosition());

        if(currentHp <= 0 && isEscorting) {
            GoToAbyss();  
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        MoveInputHandler(delta);

        if(Input.IsActionJustPressed("ui_left")) {
            anim.Play("BankLeft");
        } 
        else if(Input.IsActionJustReleased("ui_left")) {
            anim.PlayBackwards("BankLeft");
        }
        else if(Input.IsActionJustPressed("ui_right")) {
            anim.Play("BankRight");
        }
        else if(Input.IsActionJustReleased("ui_right")) {
            anim.PlayBackwards("BankRight");
        }
        else {
            anim.Play("Idle");
        }
    }

    void MoveInputHandler(float delta) {
        velocity = new Vector2(
                Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left"), 
                Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up")
            ) * movementSpeed;

        velocity = MoveAndSlide(velocity * delta);
    }

    public void StartShootTimer() {
        shootTimer.Start();
    }

    public void StopShootTimer() {
        shootTimer.Stop();
    }

    void _on_ShootTimer_timeout() {
        // EmitSignal("PlayerShoot", this, initialDmg, bulletPos.GlobalPosition, body.GlobalRotation);

        var bullet = (Bullet) bulletScene.Instance();
        bullet.Init(bulletPos.GlobalPosition, body.GlobalRotation);
        bullet.SetDamage(initialDmg);
        bullet.SetParent(this);

        GetParent().AddChild(bullet);

        for(int i = 0; i < shootSFXs.Count; i++) {
            if(!shootSFXs[i].Playing) {
                shootSFXs[i].Play();
                break;     
            }
        }
    }

    public void TakeDamage(float damage) {
        currentHp -= damage;
        // hpBar.Value = currentHp;
        EmitSignal("UpdatePlayerHp", currentHp);
    }

    public void GoToAbyss() {
        isEscorting = false;
        EmitSignal("PlayerAbyss");
    }

    public void GoEscort() {
        isEscorting = true;
        EmitSignal("PlayerEscort");

        currentHp = maxHp;
        // hpBar.Value = currentHp;
        EmitSignal("UpdatePlayerHp", currentHp);

    }

    public bool GetEscortState() {
        return isEscorting;
    }

    public void SetEscortState(bool value) {
        isEscorting = value;
    }

    public void SetHp(float value) {
        currentHp = value;
        // hpBar.Value = currentHp;
        EmitSignal("UpdatePlayerHp", currentHp);
    }

    void _on_HpRestoreTimer_timeout() {
        if(isEscorting) {
            currentHp += healValue;
        }

        if(currentHp >= maxHp) {
            currentHp = maxHp;
        }

        // hpBar.Value = currentHp;

        EmitSignal("UpdatePlayerHp", currentHp);
    }

    void _on_Area2D_area_entered(Area2D area) {

    }

    void _on_Area2D_body_entered(Node body) {
        if(body.IsInGroup("enemy")) {
            TakeDamage(3f);
        }

        if(body.IsInGroup("meteorite")) {
            EmitSignal("BumpMeteorite");
        }

        if(!body.IsInGroup("player")) {
            for(int i = 0; i < bumpSFXs.Count; i++) {
                if(!bumpSFXs[i].Playing) {
                    bumpSFXs[i].Play();
                    break;     
                }
            }

        }
    }

    public float GetMaxHp() {
        return maxHp;
    }

}
