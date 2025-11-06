local Automation = {}
Automation.__index = Automation

function Automation.new(event_bus, logger)
    return setmetatable({
        event_bus = event_bus,
        logger = logger,
        routines = {},
        active = {}
    }, Automation)
end

function Automation:register(name, routine)
    self.routines[name] = routine
end

function Automation:start(name, context)
    local routine = self.routines[name]
    if not routine then
        return nil, "unknown routine"
    end
    self.active[name] = {
        coroutine = coroutine.create(routine),
        context = context or {},
        initialized = false
    }
    self.logger:info("Automation started", { routine = name })
end

function Automation:stop(name)
    self.active[name] = nil
    self.logger:info("Automation stopped", { routine = name })
end

function Automation:update(dt)
    for name, state in pairs(self.active) do
        if coroutine.status(state.coroutine) ~= "dead" then
            local ok, err
            if not state.initialized then
                ok, err = coroutine.resume(state.coroutine, state.context)
                state.initialized = true
            else
                ok, err = coroutine.resume(state.coroutine, dt)
            end
            if not ok then
                self.logger:error("Automation failure", { routine = name, error = err })
                self.active[name] = nil
            end
        end
    end
end

return Automation
