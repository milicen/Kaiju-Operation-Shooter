extends Node2D


func _ready():
	var spritenum = int(rand_range(0,3))
	var rotate = rand_range(-20,20)
	rotation_degrees = spritenum * rotate 
	match spritenum:
		1:
			$S1.visible = true
			$S2.visible = false
			$S3.visible = false
		2:
			$S1.visible = false
			$S2.visible = true
			$S3.visible = false
		3:
			$S1.visible = false
			$S2.visible = false
			$S3.visible = true
