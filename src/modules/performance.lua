local PerformanceAnalyzer = {}
PerformanceAnalyzer.__index = PerformanceAnalyzer

function PerformanceAnalyzer.new(event_bus, logger)
    return setmetatable({
        event_bus = event_bus,
        logger = logger,
        samples = {},
        window = 60,
        last_report = 0
    }, PerformanceAnalyzer)
end

function PerformanceAnalyzer:record(metric, value)
    if not self.samples[metric] then
        self.samples[metric] = {}
    end
    local bucket = self.samples[metric]
    table.insert(bucket, value)
    if #bucket > self.window then
        table.remove(bucket, 1)
    end
end

local function average(values)
    local sum = 0
    for _, value in ipairs(values) do
        sum = sum + value
    end
    return sum / math.max(#values, 1)
end

function PerformanceAnalyzer:update(dt)
    self.last_report = self.last_report + dt
    if self.last_report >= 1 then
        self.last_report = 0
        local report = {}
        for metric, values in pairs(self.samples) do
            report[metric] = average(values)
        end
        self.logger:debug("Performance report", report)
        self.event_bus:emit("performance_report", report)
    end
end

return PerformanceAnalyzer
