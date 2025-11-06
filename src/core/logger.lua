local Logger = {}
Logger.__index = Logger

local LEVELS = {
    TRACE = 1,
    DEBUG = 2,
    INFO = 3,
    WARN = 4,
    ERROR = 5
}

local LABELS = {
    [LEVELS.TRACE] = "TRACE",
    [LEVELS.DEBUG] = "DEBUG",
    [LEVELS.INFO] = "INFO",
    [LEVELS.WARN] = "WARN",
    [LEVELS.ERROR] = "ERROR"
}

function Logger.new(min_level, output)
    return setmetatable({
        min_level = min_level or LEVELS.INFO,
        output = output or io.stdout
    }, Logger)
end

function Logger:set_level(level)
    self.min_level = level
end

function Logger:log(level, message, context)
    if level < self.min_level then
        return
    end

    local label = LABELS[level] or "INFO"
    local time = os.date("%H:%M:%S")
    local ctx_str = ""

    if context then
        local serialized = {}
        for key, value in pairs(context) do
            table.insert(serialized, string.format("%s=%s", key, tostring(value)))
        end
        ctx_str = " {" .. table.concat(serialized, ", ") .. "}"
    end

    self.output:write(string.format("[%s] %s %s%s\n", time, label, message, ctx_str))
end

for name, level in pairs(LEVELS) do
    Logger[name:lower()] = function(self, message, context)
        self:log(level, message, context)
    end
end

Logger.levels = LEVELS

return Logger
