local MovementController = {}
MovementController.__index = MovementController

local NAVIGATION_MODES = {
    DIRECT = "direct",
    PATHFIND = "pathfind",
    ORBIT = "orbit"
}

function MovementController.new(config, event_bus, logger)
    return setmetatable({
        config = config,
        event_bus = event_bus,
        logger = logger,
        navigation_mode = NAVIGATION_MODES.DIRECT,
        position = { x = 0, y = 0, z = 0 },
        velocity = { x = 0, y = 0, z = 0 },
        target = { x = 0, y = 0, z = 0 },
        height_target = 0,
        smoothing = 0.2,
        acceleration = 20,
        speed_limit = 10,
        current_path = {},
        path_index = 1
    }, MovementController)
end

function MovementController:set_navigation_mode(mode)
    if NAVIGATION_MODES[mode:upper()] then
        self.navigation_mode = NAVIGATION_MODES[mode:upper()]
        self.logger:info("Navigation mode switched", { mode = mode })
    else
        error("Unknown navigation mode: " .. tostring(mode))
    end
end

function MovementController:set_height_target(height)
    self.height_target = height
end

function MovementController:set_speed_limit(limit)
    self.speed_limit = limit
end

function MovementController:tune_parameters(params)
    for key, value in pairs(params) do
        if self[key] ~= nil then
            self[key] = value
        end
    end
end

function MovementController:set_target(position)
    self.target = { x = position.x, y = position.y, z = position.z }
    self.event_bus:emit("movement_target_changed", self.target)
end

function MovementController:schedule_path(path)
    self.current_path = path
    self.path_index = 1
end

local function clamp(value, min, max)
    if value < min then
        return min
    end
    if value > max then
        return max
    end
    return value
end

local function approach(current, target, rate, dt)
    return current + (target - current) * clamp(rate * dt, 0, 1)
end

function MovementController:update(dt)
    if self.navigation_mode == NAVIGATION_MODES.PATHFIND and self.current_path[self.path_index] then
        self.target = self.current_path[self.path_index]
        if math.abs(self.position.x - self.target.x) < 0.05 and
            math.abs(self.position.y - self.target.y) < 0.05 and
            math.abs(self.position.z - self.target.z) < 0.05 then
            self.path_index = self.path_index + 1
        end
    elseif self.navigation_mode == NAVIGATION_MODES.ORBIT then
        local radius = self.config:get("orbit_radius") or 5
        local speed = self.config:get("orbit_speed") or 1
        local angle = (self.config:get("time") or 0) * speed
        self.target.x = radius * math.cos(angle)
        self.target.z = radius * math.sin(angle)
        self.target.y = self.height_target
    end

    self.position.x = approach(self.position.x, self.target.x, self.smoothing, dt)
    self.position.y = approach(self.position.y, self.height_target, self.smoothing, dt)
    self.position.z = approach(self.position.z, self.target.z, self.smoothing, dt)

    self.velocity.x = clamp((self.target.x - self.position.x) / math.max(dt, 0.0001), -self.speed_limit, self.speed_limit)
    self.velocity.y = clamp((self.height_target - self.position.y) / math.max(dt, 0.0001), -self.speed_limit, self.speed_limit)
    self.velocity.z = clamp((self.target.z - self.position.z) / math.max(dt, 0.0001), -self.speed_limit, self.speed_limit)

    self.event_bus:emit("movement_updated", {
        position = self.position,
        velocity = self.velocity,
        mode = self.navigation_mode
    })
end

return MovementController
