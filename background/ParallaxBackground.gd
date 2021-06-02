extends ParallaxBackground

export var scroll : int

func _process(delta):

	scroll_offset.y += scroll
#	$ParallaxLayer2.motion_offset.y += scroll
#	$ParallaxLayer3.motion_offset.y += scroll
	
