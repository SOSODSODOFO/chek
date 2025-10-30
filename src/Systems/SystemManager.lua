--!strict

local RunService = game:GetService("RunService")
local Workspace = game:GetService("Workspace")

export type CameraConfig = {
mode: "Orbital" | "Follow" | "Free"?,
offset: Vector3?,
lerpAlpha: number?,
}

export type LoggingConfig = {
enabled: boolean?,
historySize: number?,
}

local SystemManager = {}
SystemManager.__index = SystemManager

function SystemManager.new(config: {
camera: CameraConfig?,
logging: LoggingConfig?,
movement: any?,
environment: any?,
analytics: any?,
visualization: any?,
logger: any?,
performanceAnalyzer: any?,
objectManager: any?,
environmentModifiers: any?,
})
local self = setmetatable({}, SystemManager)
self._cameraConfig = config.camera or {}
self._loggingConfig = config.logging or { enabled = true, historySize = 64 }
self._movement = config.movement
self._environment = config.environment
self._analytics = config.analytics
self._visualization = config.visualization
self._logger = config.logger
self._performanceAnalyzer = config.performanceAnalyzer
self._objectManager = config.objectManager
self._environmentModifiers = config.environmentModifiers
self._logBuffer = {}
self._cameraConnection = nil
self._performanceConnection = nil
return self
end

function SystemManager:Init()
if self._loggingConfig.enabled then
self:_log("SystemManager initialized")
end
if self._logger then
self._logger:Info("Logger connected to SystemManager")
end
if self._performanceAnalyzer then
self._performanceConnection = self._performanceAnalyzer:OnUpdated():Connect(function(metrics)
self:_log(string.format("Performance FPS %.2f", metrics.FrameRate))
end)
end
end

function SystemManager:_log(message: string)
if not self._loggingConfig.enabled then
return
end
table.insert(self._logBuffer, { time = os.time(), message = message })
local max = self._loggingConfig.historySize or 64
while #self._logBuffer > max do
table.remove(self._logBuffer, 1)
end
if self._logger then
self._logger:Info(message)
end
end

function SystemManager:_updateCamera(dt: number)
local camera = Workspace.CurrentCamera
if not camera then
return
end
local target = self._movement and self._movement:GetState()
if not target then
return
end
local offset = self._cameraConfig.offset or Vector3.new(0, 20, -60)
local lerpAlpha = self._cameraConfig.lerpAlpha or 0.15
local desired = CFrame.new(Vector3.new(0, target.Altitude, 0)) * CFrame.new(offset)
camera.CFrame = camera.CFrame:Lerp(desired, lerpAlpha)
end

function SystemManager:Start()
self._cameraConnection = RunService.Heartbeat:Connect(function(dt)
self:_updateCamera(dt)
end)
end

function SystemManager:Update(dt: number)
-- Potential place for monitoring logic
if self._objectManager then
-- Keeping the reference alive ensures descriptors remain applied
end
end

function SystemManager:GetLogs()
return table.clone(self._logBuffer)
end

function SystemManager:Destroy()
if self._cameraConnection then
self._cameraConnection:Disconnect()
end
if self._performanceConnection then
self._performanceConnection:Disconnect()
end
end

return SystemManager
