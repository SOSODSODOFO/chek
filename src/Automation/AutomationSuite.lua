--!strict

local RunService = game:GetService("RunService")

export type Config = {
scheduledTasks: { [string]: number }?,
}

local AutomationSuite = {}
AutomationSuite.__index = AutomationSuite

function AutomationSuite.new(config: Config?)
local self = setmetatable({}, AutomationSuite)
self._config = config or {}
self._timers = {}
self._movement = nil
self._environment = nil
return self
end

function AutomationSuite:Init(deps)
self._movement = deps.movement
self._environment = deps.environment
for name, interval in (self._config.scheduledTasks or {}) do
self:_createTimer(name, interval)
end
end

function AutomationSuite:_createTimer(name: string, interval: number)
local accumulator = 0
self._timers[name] = RunService.Heartbeat:Connect(function(dt)
accumulator += dt
if accumulator >= interval then
accumulator = 0
self:_executeTask(name)
end
end)
end

function AutomationSuite:_executeTask(name: string)
if name == "RecalibrateMovement" and self._movement then
self._movement:SetSpeed((self._movement:GetState().Speed + 5) % 120)
elseif name == "RefreshEnvironment" and self._environment then
self._environment:ApplyProfile({})
end
end

function AutomationSuite:Start()
-- timers already active after Init
end

function AutomationSuite:Update(dt: number)
-- reserved for automation behaviors that need dt
end

function AutomationSuite:Destroy()
for _, connection in self._timers do
connection:Disconnect()
end
end

return AutomationSuite
