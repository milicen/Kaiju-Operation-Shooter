using Godot;
using System;

public class HUD : CanvasLayer
{

    [Signal] delegate void RestartScene();

    float playerMaxHp = 20f;
    float mechaMaxHp = 100f;
    Label runeLabel;
    // TextureProgress progressBar;
    TextureProgress playerHpBar;
    TextureProgress mechaHpBar;
    Sprite progressPoint;

    Tween tween;
    Panel panel;

    public override void _Ready()
    {

        // ToggleVisibility(false);
        runeLabel = GetNode<Label>("RuneCount");
        // // progressBar = GetNode<TextureProgress>("ProgressBar");
        playerHpBar = GetNode<TextureProgress>("PlayerHpBar");
        mechaHpBar = GetNode<TextureProgress>("MechaHpBar");

        playerHpBar.MaxValue = playerMaxHp;
        mechaHpBar.MaxValue = mechaMaxHp;
        playerHpBar.Value = playerMaxHp;
        mechaHpBar.Value = mechaMaxHp;

        progressPoint = GetNode<Sprite>("ProgressPoint");

        panel = GetNode<Panel>("Panel");
        panel.GetNode<Button>("Button").Disabled = true;
        tween = GetNode<Tween>("Tween");
    }

    public void ToggleVisibility(bool value) {
        if(value) {
            foreach(Node child in GetTree().GetNodesInGroup("label")) {
                var label = child as Label;
                label.Show();
            }
        }
        else {
            foreach(Node child in GetTree().GetNodesInGroup("label")) {
                var label = child as Label;
                label.Hide();
            }
        }
    }

    void _on_UpdateRune(int runeCount) {
        runeLabel.Text = "Rune : " + runeCount.ToString();
    }
    
    public void UpdatePlayerHp(float currentHp) {
        playerHpBar.Value = currentHp;
    }

    public void UpdateMechaHp(float currentHp) {
        mechaHpBar.Value = currentHp;
    }

    void _on_UpdateProgress(float progress) {
        // progressBar.Value = progress;
        var pos = new Vector2(progressPoint.Position.x + progress, progressPoint.Position.y);

        if(pos.x >= 752f) {
            pos.x = 752f;
        }

        if(pos.x <= 16f) {
            pos.x = 16f;
        }

        progressPoint.Position = pos;
    }


    public async void OpenPanel() {
        var darken = new Color(.42f, .44f, .62f, 1);
        tween.InterpolateProperty(panel, "modulate", panel.Modulate, darken, 1f);
        tween.Start();

        await ToSignal(tween, "tween_completed");

        panel.GetNode<Button>("Button").Disabled = false;
    }

    public void ClosePanel() {
        var lighten = new Color(.42f, .44f, .62f, 0);
        tween.InterpolateProperty(panel, "modulate", panel.Modulate, lighten, 1f);
        tween.Start();
    }
    void _on_Button_pressed() {
        EmitSignal("RestartScene");
    }

    public void ReloadScene() {
        ClosePanel();
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
