--!strict

export type LoggerConfig = {
enabled: boolean?,
historySize: number?,
}

local Logger = {}
Logger.__index = Logger

function Logger.new(config: LoggerConfig?)
local self = setmetatable({}, Logger)
self._enabled = config and config.enabled ~= nil and config.enabled or true
self._historySize = config and config.historySize or 128
self._entries = {}
return self
end

function Logger:_push(level: string, message: string)
if not self._enabled then
return
end
table.insert(self._entries, {
timestamp = os.clock(),
level = level,
message = message,
})
while #self._entries > self._historySize do
table.remove(self._entries, 1)
end
end

function Logger:Info(message: string)
self:_push("INFO", message)
end

function Logger:Warn(message: string)
self:_push("WARN", message)
end

function Logger:Error(message: string)
self:_push("ERROR", message)
end

function Logger:GetEntries()
return table.clone(self._entries)
end

return Logger
