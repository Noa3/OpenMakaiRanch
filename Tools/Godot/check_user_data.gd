extends SceneTree

# No smoke-test flag is passed to this separate preflight process.
func _initialize() -> void:
	var expected := OS.get_environment("OMR_EXPECTED_USER_ROOT").replace("\\", "/").trim_suffix("/") + "/"
	var actual := OS.get_user_data_dir().replace("\\", "/")
	print("USER_DATA_PATH=" + actual)
	if expected == "/" or not actual.to_lower().begins_with(expected.to_lower()):
		push_error("Test profile isolation failed; refusing to run tests.")
		quit(2)
		return
	print("USER_DATA_ISOLATION_PASS")
	quit(0)
