local Monitoring = {}
Monitoring.__index = Monitoring

function Monitoring.new(event_bus, logger)
    return setmetatable({
        event_bus = event_bus,
        logger = logger,
        history = {}
    }, Monitoring)
end

function Monitoring:record(event, payload)
    table.insert(self.history, {
        timestamp = os.time(),
        event = event,
        payload = payload
    })
    self.logger:trace("Event recorded", { event = event })
end

function Monitoring:bind()
    local record = function(name)
        return function(payload)
            self:record(name, payload)
        end
    end

    self.event_bus:on("movement_updated", record("movement_updated"))
    self.event_bus:on("environment_updated", record("environment_updated"))
    self.event_bus:on("performance_report", record("performance_report"))
end

return Monitoring
