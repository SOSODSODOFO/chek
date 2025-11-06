local DataTracker = {}
DataTracker.__index = DataTracker

function DataTracker.new(event_bus, logger)
    local tracker = setmetatable({
        logger = logger,
        event_bus = event_bus,
        participants = {},
        smoothing_factor = 0.5,
        priorities = {},
        smoothed_metrics = {}
    }, DataTracker)

    tracker:event_listeners()

    return tracker
end

function DataTracker:event_listeners()
    self.event_bus:on("participant_update", function(data)
        self.participants[data.id] = data
        self:recalculate_priorities()
    end)
end

function DataTracker:set_smoothing_factor(value)
    self.smoothing_factor = value
end

local function smooth(current, target, factor)
    if not current then
        return target
    end
    return current + (target - current) * factor
end

function DataTracker:recalculate_priorities()
    local metrics = {}
    self.priorities = {}
    for id, participant in pairs(self.participants) do
        local importance = (participant.threat or 0) * 1.5 + (participant.health or 0) * -0.5
        metrics[id] = importance
        table.insert(self.priorities, id)
    end

    table.sort(self.priorities, function(a, b)
        return metrics[a] > metrics[b]
    end)
end

function DataTracker:update(dt)
    for id, participant in pairs(self.participants) do
        local current = self.smoothed_metrics[id] or {}
        self.smoothed_metrics[id] = {
            health = smooth(current.health, participant.health or 0, self.smoothing_factor * dt),
            stamina = smooth(current.stamina, participant.stamina or 0, self.smoothing_factor * dt),
            shield = smooth(current.shield, participant.shield or 0, self.smoothing_factor * dt)
        }
    end

    self.event_bus:emit("data_tracking_updated", {
        participants = self.participants,
        priorities = self.priorities,
        smoothed_metrics = self.smoothed_metrics
    })
end

return DataTracker
