using Godot;
using System;

public class Mecha : Area2D
{
    [Signal] delegate void UpdateMechaHp(float currentHp);
    [Signal] delegate void Victory();

    Player player;

    // TextureProgress hpBar;
    float maxHp = 100;
    float currentHp;
    float healValue = 5f;

    Timer attackTimer;
    float waitTime = 3f;

    float damage = 2;

    Godot.Collections.Array<Enemy> enemies = new Godot.Collections.Array<Enemy>();

    Line2D beam;
    Position2D beamPoint;
    Position2D globalBeamPoint;

    AnimationPlayer anim;

    bool shootBeam = false;

    AudioStreamPlayer beamSfx;

    Tween tween;

    public override void _Ready()
    {
        player = GetParent().GetNode<Player>("Player");

        // hpBar = GetNode<TextureProgress>("HpBar");

        currentHp = maxHp;

        // hpBar.MaxValue = maxHp;
        // hpBar.Value = currentHp;

        attackTimer = GetNode<Timer>("AttackTimer");
        attackTimer.WaitTime = waitTime;

        beam = GetNode<Line2D>("Beam");
        beamPoint = GetNode<Position2D>("BeamPoint");

        beamSfx = GetNode<AudioStreamPlayer>("BeamSFX");

        globalBeamPoint = GetNode<Position2D>("/root/Main/GlobalBeamPoint");

        tween = GetNode<Tween>("Tween");

        anim = GetNode<AnimationPlayer>("Body/Mech/AnimationPlayer");

        anim.Play("Walk");
        
    }

    public override void _Process(float delta)
    {
        if(currentHp <= 0) {
            // go to abyss scene
            GD.Print("go to abyss");
            player.SetHp(0);
            RestoreHp();
        }

        if(shootBeam) {
            beamPoint.GlobalPosition = globalBeamPoint.GlobalPosition;
            // var length = (beamPoint.Position - Position).Length();

            // beam.Scale = new Vector2(beam.Scale.x, length);
            var direction = beamPoint.Position - beam.GetPointPosition(1);

            if(direction.Length() > 2f) {
                var pos = beam.GetPointPosition(1) + direction.Normalized() * 80f * delta;
                beam.SetPointPosition(1, pos);
            }

            anim.Play("Beaming");
        }
    }

    void _on_Mecha_body_entered(Node body) {
        if(body.IsInGroup("enemy-crawler")) {
            var enemyCrawler = body as Enemy;
            enemyCrawler.StopMoving();

            enemyCrawler.StartLeeching(this);

            enemies.Add(enemyCrawler);

            attackTimer.Start();
        }
    }

    public void TakeDamage(float damage) {
        currentHp -= damage;
        // hpBar.Value = currentHp;

        EmitSignal("UpdateMechaHp", currentHp);
    }

    public void RestoreHp() {
        currentHp = maxHp;
        // hpBar.Value = currentHp;

        EmitSignal("UpdateMechaHp", currentHp);

    }

    // enemycrawler rot mecha
    // when enemycrawler hit mecha, mecha take enemy damage per 1 or 2 second
    // 

    void _on_AttackTimer_timeout() {
        foreach(Enemy enemy in enemies) {
            if(!IsInstanceValid(enemy)) {
                // enemies.Remove(enemies.IndexOf(enemy));
                enemies.Remove(enemy);
            }
            else {
                enemy.TakeDamage(damage);
            }
        }

        if(enemies.Count <= 0) {
            attackTimer.Stop();
        }
    }

    public Vector2 GetBeamPoint() {
        return beamPoint.GlobalPosition;
    }

    async void _on_BeamFight(Vector2 globalBeampoint) {
        var pos = new Vector2(GlobalPosition.x, 360);
        tween.InterpolateProperty(this, "global_position", GlobalPosition, pos, 1f);
        tween.Start();

        await ToSignal(tween, "tween_completed");

        anim.Play("Lasercharge");

        await ToSignal(anim, "animation_finished");

        shootBeam = true;

        beamSfx.Play();
    }

    void _on_MechaResetPosition() {
        var pos = new Vector2(GlobalPosition.x, 300);
        tween.InterpolateProperty(this, "global_position", GlobalPosition, pos, 1f);
        tween.Start();
    }

    void _on_Area2D_area_entered(Area2D body) {
        if(body.IsInGroup("boss")) {
            EmitSignal("Victory");
        }
    }

    void _on_HpRestoreTimer_timeout() {
        currentHp += healValue;
        if(currentHp >= maxHp) {
            currentHp = maxHp;
        }
        // hpBar.Value = currentHp;

        EmitSignal("UpdateMechaHp", currentHp);

    }

    public void ResetBeam() {
        beam.SetPointPosition(1, Vector2.Zero);
        beam.Scale = new Vector2(beam.Scale.x, 0);
        shootBeam = false;
        anim.Play("Walk");
    }

    public float GetMaxHp() {
        return maxHp;
    }
}
