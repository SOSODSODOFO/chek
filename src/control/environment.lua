local EnvironmentController = {}
EnvironmentController.__index = EnvironmentController

function EnvironmentController.new(config, event_bus, logger)
    return setmetatable({
        config = config,
        event_bus = event_bus,
        logger = logger,
        collision_settings = {
            enabled = true,
            penetration_depth = 0.1,
            restitution = 0.3
        },
        object_speed_modifiers = {},
        auto_physics = true
    }, EnvironmentController)
end

function EnvironmentController:update_collision_settings(settings)
    for key, value in pairs(settings) do
        self.collision_settings[key] = value
    end
    self.logger:debug("Collision settings updated", self.collision_settings)
end

function EnvironmentController:set_object_speed_modifier(tag, value)
    self.object_speed_modifiers[tag] = value
end

function EnvironmentController:apply_physics(object)
    if not self.auto_physics then
        return object
    end

    local modifier = self.object_speed_modifiers[object.tag] or 1
    object.velocity.x = object.velocity.x * modifier
    object.velocity.y = object.velocity.y * modifier
    object.velocity.z = object.velocity.z * modifier

    if self.collision_settings.enabled and object.collision then
        object.collision.restitution = self.collision_settings.restitution
        object.collision.penetration_depth = self.collision_settings.penetration_depth
    end

    return object
end

function EnvironmentController:update(dt)
    self.event_bus:emit("environment_updated", {
        collision_settings = self.collision_settings,
        modifiers = self.object_speed_modifiers,
        auto_physics = self.auto_physics,
        dt = dt
    })
end

return EnvironmentController
