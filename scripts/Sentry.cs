using Godot;
using System;

public class Sentry : Sprite
{
    Mecha mecha;

    public override void _Ready()
    {
        mecha = GetParent() as Mecha;
    }

    void _on_ExplodeTimer_timeout() {
        GetNode<AnimationPlayer>("AnimationPlayer").Play("Boom");
    }

}
