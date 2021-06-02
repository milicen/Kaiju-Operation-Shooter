using Godot;
using System;

public class Bullet : Area2D
{

    [Signal] delegate void BulletOut(Node actor);

    public enum Parent {
        Player,
        Enemy
    }

    Parent parent;

    [Export] public float speed;
    Vector2 direction;

    float damage;

    public void Init(Vector2 position, float rotation) {
        Position = position;
        Rotation = rotation;
        var angle = Rotation;
        direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

    }

    public override void _Process(float delta) {
        Position += direction * speed * delta;
    }

    void _on_Bullet_body_entered(Node body) {
        if(body.IsInGroup("enemy") && parent == Parent.Player) {
            var enemy = body as Enemy;

            // enemy get hit
            enemy.TakeDamage(damage);
            // send back to pool when die
        }
        else if (body.IsInGroup("player") && parent == Parent.Enemy) {
            var player = body as Player;

            player.TakeDamage(damage);
            // player get hit
        }
        else if(body.IsInGroup("meteorite") && parent == Parent.Player) {
            var meteorite = body as Meteorite;

            meteorite.TakeDamage(damage);
        }
        // particles.Emitting = true;

        // EmitSignal("BulletOut", this);
        QueueFree();
    }

    void _on_VisibilityNotifier2D_viewport_exited(Viewport vp) {
        // EmitSignal("BulletOut", this);
        QueueFree();
    }

    public void SetParent(Node _parent) {
        if(_parent.GetType() == typeof(Player)) {
            parent = Parent.Player;
        }
        else if(_parent.GetType() == typeof(EnemyGunner)) {
            parent = Parent.Enemy;
        }
    }

    public void SetDamage(float _damage) {
        damage = _damage;
    }

}
