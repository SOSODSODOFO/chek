local Preview = {}
Preview.__index = Preview

function Preview.new(event_bus)
    return setmetatable({
        event_bus = event_bus,
        model_state = {
            pose = "idle",
            animations = {},
            transparency = 1
        },
        active_demo = nil
    }, Preview)
end

function Preview:set_model_pose(pose)
    self.model_state.pose = pose
    self.event_bus:emit("preview_pose_changed", { pose = pose })
end

function Preview:set_transparency(value)
    self.model_state.transparency = value
    self.event_bus:emit("preview_transparency_changed", { value = value })
end

function Preview:register_demo(name, callback)
    self.model_state.animations[name] = callback
end

function Preview:play_demo(name)
    local demo = self.model_state.animations[name]
    if demo then
        self.active_demo = name
        local feedback = demo()
        self.event_bus:emit("preview_demo_played", { name = name, feedback = feedback })
        return feedback
    end
end

return Preview
