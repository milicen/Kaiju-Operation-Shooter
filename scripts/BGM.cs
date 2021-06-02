using Godot;
using System;

public class BGM : Node
{

    [Signal] delegate void StartMission();

    [Export] AudioStream oceanLoop;
    [Export] AudioStream voidLoop;
    [Export] AudioStream voidEnter;
    [Export] AudioStream voidOut;
    [Export] AudioStream bossEnter;
    [Export] AudioStream bossLoop;
    [Export] AudioStream victory;

    AudioStreamPlayer musicPlayer;
    AudioStreamPlayer introPlayer;


    public override async void _Ready()
    {
        musicPlayer = GetNode<AudioStreamPlayer>("MusicPlayer");

        musicPlayer.Stream = oceanLoop;

        introPlayer = GetNode<AudioStreamPlayer>("Intro");

        introPlayer.Play();

        await ToSignal(introPlayer, "finished");

        EmitSignal("StartMission");

        musicPlayer.Play();
    }

    async void _on_ChangeMusic(string state) {
        if(state == "abyss") {
            // await ToSignal(musicPlayer, "finished");

            musicPlayer.Stream = voidEnter;
            musicPlayer.Play();

            await ToSignal(musicPlayer, "finished");
            
            musicPlayer.Stream = voidLoop;
            musicPlayer.Play();
        }
        else if(state == "escort") {
            // await ToSignal(musicPlayer, "finished");
            
            musicPlayer.Stream = voidOut;
            musicPlayer.Play();

            await ToSignal(musicPlayer, "finished");

            musicPlayer.Stream = oceanLoop;
            musicPlayer.Play();
        }
        else if(state == "victory") {
            musicPlayer.Stream = victory;
            musicPlayer.Play();

            await ToSignal(musicPlayer, "finished");

            // musicPlayer.Stream = oceanLoop;
            // musicPlayer.Play();
        }
    }

    public async void ReloadScene() {
        if(musicPlayer.Playing) {
            musicPlayer.Stop();
            musicPlayer.Stream = null;
        }
        introPlayer.Play();

        await ToSignal(introPlayer, "finished");

        EmitSignal("StartMission");

        musicPlayer.Stream = oceanLoop;
        musicPlayer.Play();
    }

}
