--!strict

local RunService = game:GetService("RunService")

export type PerformanceMetrics = {
FrameRate: number,
FrameTime: number,
MemoryUsage: number,
}

local PerformanceAnalyzer = {}
PerformanceAnalyzer.__index = PerformanceAnalyzer

function PerformanceAnalyzer.new()
local self = setmetatable({}, PerformanceAnalyzer)
self._metrics = {
FrameRate = 60,
FrameTime = 1 / 60,
MemoryUsage = 0,
}
self._samples = {}
self._signal = Instance.new("BindableEvent")
self._connection = nil
return self
end

function PerformanceAnalyzer:Init()
self._connection = RunService.Heartbeat:Connect(function(dt)
self:_sample(dt)
end)
end

function PerformanceAnalyzer:_sample(dt: number)
local fps = 1 / math.max(dt, 1e-4)
local success, memory = pcall(function()
return collectgarbage("count")
end)
self._metrics.FrameRate = fps
self._metrics.FrameTime = dt
self._metrics.MemoryUsage = success and memory or self._metrics.MemoryUsage
self._signal:Fire(table.clone(self._metrics))
end

function PerformanceAnalyzer:GetMetrics(): PerformanceMetrics
return table.clone(self._metrics)
end

function PerformanceAnalyzer:OnUpdated()
return self._signal.Event
end

function PerformanceAnalyzer:Destroy()
if self._connection then
self._connection:Disconnect()
end
self._signal:Destroy()
end

return PerformanceAnalyzer
