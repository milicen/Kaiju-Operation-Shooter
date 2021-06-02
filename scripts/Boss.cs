using Godot;
using System;

public class Boss : Area2D
{
    [Signal] delegate void DeadFinished();
    Main main;
    Position2D beamPoint;
    Position2D globalBeamPoint;

    Line2D beam;
    Tween tween;

    AnimationPlayer anim;

    bool shootBeam = false;

    AudioStreamPlayer beamSfx;
    AudioStreamPlayer roarSfx;
    AudioStreamPlayer dieSfx;

    public override void _Ready()
    {
        // main = GetParent() as Main;
        beamPoint = GetNode<Position2D>("BeamPoint");

        beam = GetNode<Line2D>("Beam");

        globalBeamPoint = GetNode<Position2D>("/root/Main/GlobalBeamPoint");

        tween = GetNode<Tween>("Tween");

        anim = GetNode<AnimationPlayer>("Body/Boss/AnimationPlayer");

        beamSfx = GetNode<AudioStreamPlayer>("BeamSFX");
        roarSfx = GetNode<AudioStreamPlayer>("RoarSFX");
        dieSfx = GetNode<AudioStreamPlayer>("DeadSFX");
    }

    public override void _Process(float delta)
    {
        if(shootBeam) {
            beamPoint.GlobalPosition = globalBeamPoint.GlobalPosition;
            var direction = beamPoint.Position - beam.GetPointPosition(1);

            if(direction.Length() > 2f) {
                var pos = beam.GetPointPosition(1) + direction.Normalized() * 250f * delta;
                beam.SetPointPosition(1, pos);
            } 


        }
    }

    async void _on_BeamFight(Vector2 globalBeamPoint) {
        var pos = new Vector2(GlobalPosition.x, 100);
        tween.InterpolateProperty(this, "global_position", GlobalPosition, pos, 1f);
        tween.Start();

        anim.Play("Walk");

        await ToSignal(tween, "tween_completed");

        anim.Play("Roar");
        roarSfx.Play();

        await ToSignal(anim, "animation_finished");

        anim.Play("Reset");

        beamPoint.GlobalPosition = globalBeamPoint;
        
        shootBeam = true;

        beamSfx.Play();
    }

    public void ResetBeam() {
        beam.SetPointPosition(1, Vector2.Zero);
        beam.Scale = new Vector2(beam.Scale.x, 0);
        shootBeam = false;
    }

    async void _on_BossDead() {
        ResetBeam();

        anim.Play("Roar");
        dieSfx.Play();

        await ToSignal(anim, "animation_finished");

        EmitSignal("DeadFinished");

        Hide();

        
    }
}
