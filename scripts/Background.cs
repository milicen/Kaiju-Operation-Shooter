using Godot;
using System;

public class Background : TextureRect
{
    Main main;

    [Export] Texture ocean;
    [Export] Texture abyss;

    [Export] float speed = 10f;

    Tween tween;

    public override void _Ready()
    {
        main = GetParent() as Main;
        main.Connect("ChangeBackground", this, "_on_ChangeBackground");

        Texture = ocean;

        tween = GetNode<Tween>("Tween");
    }

    public override void _Process(float delta)
    {
        RectPosition += Vector2.Down * speed * delta;

        if(RectPosition.y >= 600f) {
            RectPosition = new Vector2(0, -798f);
        }
    }

    async void _on_ChangeBackground() {

        var darken = new Color(0, 0, 0, 1);
        tween.InterpolateProperty(this, "modulate", Modulate, darken, 1f);
        tween.Start();

        await ToSignal(tween, "tween_completed");

        GD.Print("tween completed");

        if(Texture == ocean) {
            Texture = abyss;
        }
        else {
            Texture = ocean;
        }

        var lighten = new Color(1, 1, 1, 1);
        tween.InterpolateProperty(this, "modulate", Modulate, lighten, 1f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
        tween.Start();

        await ToSignal(tween, "tween_completed");

        GetTree().CallGroup("spawner", "SetEscortingState", false);


    }


}
